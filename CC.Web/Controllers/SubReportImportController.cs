using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CC.Data;
using MvcContrib;
using MvcContrib.ActionResults;
using CC.Web.Models.Import.ClientReports;

namespace CC.Web.Controllers
{
	[CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
	public class SubReportImportController : CC.Web.Controllers.PrivateCcControllerBase
	{
		private readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(SubReportImportController));
		private ccEntities _db;
		private Func<ccEntities> getDbContext = () => { var db = new ccEntities(false, false); return db; };

		#region Constructors

		public SubReportImportController()
			: base()
		{
			_db = getDbContext();
		}

		#endregion


		#region Http methods

		//
		// GET: /SubReportImport/

		public ActionResult Index()
		{
			return View();
		}

		[HttpGet]
		public ActionResult Upload(UploadModel model)
		{
			var reportingMethodId = _db.AppBudgetServices.Where(Permissions.AppBudgetServicesFilter)
				.Where(f => f.Id == model.AppBudgetServiceId)
				.Select(f => f.Service.ReportingMethodId).Single();
			
			var importHelper = CC.Web.Models.Import.ClientReports.ClientReportImportFactory.GetByReportingTypeId(reportingMethodId, this.Permissions);
			model.CsvColumns = importHelper.CsvColumnNames();

			return View(model);
		}

		/// <summary>
		/// Recieves a csv file
		/// Validates csv fromat and data types
		/// Pushes records to a temp table
		/// </summary>
		/// <param name="subReportId"></param>
		/// <returns></returns>
		[HttpPost, ActionName("Upload")]
		public ActionResult ProcessFile(CC.Web.Models.Import.ClientReports.UploadModel model)
		{
			//new import id
			var importId = Guid.NewGuid();
			var subReportId = model.SubReportId;
			var mainReportId = model.MainReportId;
			var appBudgetServiceId = model.AppBudgetServiceId;

			var subReport = GetExistingOrCreateNewSubReport(subReportId, appBudgetServiceId, mainReportId);
            

			var service = this.GetService(subReport);

			//save to temp location
			string filename = null;
			var file = model.File;
			if (file != null)
			{

				filename = FileName(importId);
				file.SaveAs(filename);
			}
			else
			{
				throw new InvalidOperationException("No file recieved");
			}



			//get the type of the data/clientreport being reported

			var reportingMethodId = service.ReportingMethodId;

			var import = new Import() { StartedAt = DateTime.Now, TargetId = subReport.Id, Id = importId, UserId=this.Permissions.User.Id };
			_db.Imports.AddObject(import);
			_db.SaveChanges();


			var importHelper = CC.Web.Models.Import.ClientReports.ClientReportImportFactory.GetByReportingTypeId(reportingMethodId, this.Permissions);
				
			System.Threading.ThreadPool.QueueUserWorkItem(f =>
			{
				try
				{
					importHelper.Upload(filename, import.Id, subReport.Id);
				}
				catch (Exception ex)
				{
					_log.Fatal(ex);
				}
				finally
				{
					if (System.IO.File.Exists(filename))
					{
						System.IO.File.Delete(filename);
					}
				}
			});

			return this.RedirectToAction(f => f.Preview(importId));

		}

		private static string FileName(Guid importId)
		{
			return System.IO.Path.Combine(System.IO.Path.GetTempPath(), importId.ToString() + ".csv");
		}

		public JsonResult StatusAmount(Guid id)
		{
			return this.Status(id);
		}

		public JsonResult StatusNoAmount(Guid id)
		{
			return this.Status(id);
		}

		public JsonResult Status(Guid id)
		{
			string filename = FileName(id);
			bool fileExists = System.IO.File.Exists(filename);

			var model = new PreviewModel()
			{
				Id = id,
				Finished = !fileExists,
			};
			if (model.Finished)
			{
				var service = getservicebyimportid(id);

				var helper = CC.Web.Models.Import.ClientReports.ClientReportImportFactory.GetByReportingTypeId(service.ReportingMethodId, this.Permissions);

				model.IsValid = helper.IsValid(id);
			}
			
			return this.MyJsonResult(model, JsonRequestBehavior.AllowGet);

		}
		



		/// <summary>
		/// Shows preview for an import
		/// Including business validation
		/// </summary>
		/// <param name="importId"></param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult Preview(Guid id)
		{
			var service = getservicebyimportid(id);

			var helper = CC.Web.Models.Import.ClientReports.ClientReportImportFactory.GetByReportingTypeId(service.ReportingMethodId, this.Permissions);

			var model = new PreviewModel()
			{
				Id = id,
				DeleteAll = false,
				Columns = helper.PreviewDataColumns(),
				IsValid = true,
				Finished = !System.IO.File.Exists(FileName(id))
			};

			if (model.Finished)
			{
				model.IsValid = helper.IsValid(id);
			}

			if (!model.IsValid)
			{
				ModelState.AddModelError(string.Empty, "One or more rows contains data that can not be imported.");
			}

			return View(model);
		}

		[HttpPost]
		public ActionResult CancelImport(Guid id)
		{
			var import =_db.Imports.SingleOrDefault(f=>f.Id==id);
			if (import == null)
			{
				//import might be already cancelled. 
				throw new Exception("Imoprt data not found. Most probably it is already cancelled.");
			}
			Service service = getservicebyimportid(id);
			var subreport = GetSubReportByImportId(id);

			_db.ExecuteStoreCommand("delete from [dbo].[ImportClientReports] where importid={0}", id);
			_db.ExecuteStoreCommand("delete from [dbo].[Imports] where id={0}", id);
			
			if (service.ReportingMethodId == (int)CC.Data.Service.ReportingMethods.SoupKitchens)
			{				
				return RedirectToAction("Calendar", "SubReports", new { Id = subreport.Id });
			}
			return RedirectToAction("Details", "SubReports", new { id = import.TargetId });
		}

		public JsonResult PreviewData(Guid id, int? subReportId, CC.Web.Models.jQueryDataTableParamModel jq)
		{

			Service service = getservicebyimportid(id);

			var reportingMethodId = service.ReportingMethodId;

			
			var importHelper = CC.Web.Models.Import.ClientReports.ClientReportImportFactory.GetByReportingTypeId(reportingMethodId, this.Permissions);

			CC.Web.Models.jQueryDataTableResult<object> result = null;

			while (result == null)
			{
				try
				{
					result = importHelper.GetPreviewData(id, jq);
				}
				catch (System.Data.SqlClient.SqlException ex)
				{
					//rerun the query on deadlock exception
					if (!(ex.Message.StartsWith("Transaction") && ex.Message.Contains("was deadlocked")))
					{
						//rethrow if it's anything else
						throw ;
					}
				}
			}

			result.sEcho = jq.sEcho;

			return this.MyJsonResult(result, JsonRequestBehavior.AllowGet);
		}



		/// <summary>
		/// Copies records from the temp table
		/// </summary>
		/// <param name="importId"></param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult Import(PreviewModel model)
		{
			Service service = getservicebyimportid(model.Id.Value);

			var subreport = GetSubReportByImportId(model.Id.Value);

			var reportingMethodId = service.ReportingMethodId;

			var importHelper = CC.Web.Models.Import.ClientReports.ClientReportImportFactory.GetByReportingTypeId(reportingMethodId, this.Permissions);

			importHelper.Import(model.Id.Value);

			if (reportingMethodId == (int)CC.Data.Service.ReportingMethods.SoupKitchens)
			{
				return RedirectToAction("Calendar", "SubReports", new { Id = subreport.Id });
			}
			return RedirectToAction("Details", "SubReports", new { Id = subreport.Id });
		}

		#endregion


		#region Private methods

		protected override void Dispose(bool disposing)
		{
			if (_db != null)
			{
				_db.Dispose();
			}
			base.Dispose(disposing);
		}

		private SubReport GetSubReportByImportId(Guid id)
		{
			var s = from item in _db.Imports

					join a in _db.SubReports.Where(Permissions.SubReportsFilter) on item.TargetId equals a.Id
					where item.Id == id
					select a;
			var result = s.SingleOrDefault();
			if (result == null) { throw new InvalidOperationException("Import or subreport not found"); }
			else
			{
				return result;
			}
		}

		private Service getservicebyimportid(Guid id)
		{
			var s = from item in _db.Imports

					join a in _db.SubReports.Where(Permissions.SubReportsFilter) on item.TargetId equals a.Id
					where item.Id == id
					select a.AppBudgetService.Service;
			var result = s.SingleOrDefault();

			if (result == null) { throw new InvalidOperationException("Import or subreport not found"); }
			else
			{
				return result;
			}
		}


		private Service GetService(SubReport subReport)
		{
			if (subReport.AppBudgetService == null || subReport.AppBudgetService.Service == null)
			{
				var q = from a in _db.AppBudgetServices
						where a.Id == subReport.AppBudgetServiceId
						select a.Service;
				return q.Single();
			}
			else
			{
				return subReport.AppBudgetService.Service;
			}
		}

       

        private SubReport GetExistingOrCreateNewSubReport(int? subReportId, int? appBudgetServiceId, int? mainReportId)
		{
			var subReport = _db.SubReports.SingleOrDefault(f => f.Id == subReportId || (f.AppBudgetServiceId == appBudgetServiceId && f.MainReportId == mainReportId));
			if (subReport == null)
			{
				var mainReport = _db.MainReports.Where(Permissions.MainReportsFilter).SingleOrDefault(f => f.Id == mainReportId);
				if (mainReport == null) { throw new Exception("Main report id " + mainReportId + " not found."); }

				var appbudgetservice = _db.AppBudgetServices.Where(Permissions.AppBudgetServicesFilter).Single(f => f.Id == appBudgetServiceId);
				if (appbudgetservice == null) { throw new Exception("Budget service id " + appBudgetServiceId + " not found."); }

				subReport = new SubReport()
				{
					AppBudgetServiceId = appBudgetServiceId.Value,
					MainReportId = mainReportId.Value
				};
				_db.SubReports.AddObject(subReport);
				_db.SaveChanges();
			}

			return subReport;
		}


		#endregion


	}


}
