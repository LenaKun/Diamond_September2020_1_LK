using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CC.Data;
using CC.Web.Models;
using CsvHelper;
using MvcContrib;
using MvcContrib.ActionResults;
using System.Data.SqlClient;

namespace CC.Web.Controllers
{
	public class FunctionalityScoresController : PrivateCcControllerBase
	{


		//
		// GET: /FunctionalityScores/

		public ViewResult Index(int ClientId)
		{
			var model = new GenericModel<int>(ClientId, this.CcUser);
			return View(model);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Id"></param>
		/// <returns></returns>
		public JsonResult IndexDataTable(Models.FunctionalityScoresIndexModel model)
		{


			var data = Repo.FunctionalityScores.Select
				.Include(f => f.User)
				.Include(f => f.FunctionalityLevel.RelatedFunctionalityLevel)
				.Where(f => f.ClientId == model.Id);

			var client = Repo.Clients.Select.Single(f => f.Id == model.Id);

			bool sortDesc = HttpContext.Request["sSortDir_0"] == "desc";
			switch (HttpContext.Request["iSortCol_0"])
			{
				case "0":
					if (sortDesc) { data = data.OrderByDescending(f => f.FunctionalityLevel.Name); }
					else { data = data.OrderBy(f => f.FunctionalityLevel.Name); }
					break;

				case "1":
					if (sortDesc) { data = data.OrderByDescending(f => f.StartDate); }
					else { data = data.OrderBy(f => f.StartDate); }
					break;
				case "2":
					if (sortDesc) { data = data.OrderByDescending(f => f.FunctionalityLevel.RelatedFunctionalityLevel.Name); }
					else { data = data.OrderBy(f => f.FunctionalityLevel.RelatedFunctionalityLevel.Name); }
					break;
				case "3":
					if (sortDesc) { data = data.OrderByDescending(f => f.User.UserName); }
					else { data = data.OrderBy(f => f.User.UserName); }
					break;
				case "4":
					if (sortDesc) { data = data.OrderByDescending(f => f.UpdatedAt); }
					else { data = data.OrderBy(f => f.UpdatedAt); }
					break;
				default:
					data = data.OrderByDescending(f => f.StartDate);
					break;
			}


			var skipped = data.Skip(model.iDisplayStart).Take(model.iDisplayLength);




			return this.MyJsonResult(new
			{
				sEcho = Request["sEcho"],
				iTotalRecords = data.Count(),
				iTotalDisplayRecords = data.Count(),
				aaData = skipped.AsEnumerable().Select(f =>
					new
					{
						Id = f.Id,
						Score = f.DiagnosticScore.ToString(),
						StartDate = f.StartDate.ToShortDateString(),
						LevelName = f.FunctionalityLevel.Name,
						HcHours = f.FunctionalityLevel.HcHoursLimit,
						MaxHcHours = MathEx.Max(f.FunctionalityLevel.HcHoursLimit, client.ExceptionalHours),
						DiagnosticScore = f.DiagnosticScore,
						UpdatedAt = f.UpdatedAt.ToShortDateString(),
						UpdatedBy = f.User == null ? "" : f.User.UserName,
						DafId = f.Dafs.Select(d=>(int?)d.Id).FirstOrDefault()
					})

			}, JsonRequestBehavior.AllowGet);
		}


		[CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult Create(int Id)
		{
			if (!this.Permissions.CanUpdateExistingClient) throw new InvalidOperationException();

			var model = new FunctionalityScore() { ClientId = Id };
			return View(model);
		}


		[HttpPost]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult Create(FunctionalityScore score)
		{

			if (!this.Permissions.CanUpdateExistingClient) throw new InvalidOperationException();

			score = MakeScore(score, db);

			return View(score);
		}

		public FunctionalityScore MakeScore(FunctionalityScore score, ccEntities db)
		{
			db.ContextOptions.LazyLoadingEnabled = false;
			db.ContextOptions.ProxyCreationEnabled = false;

			ViewBag.Success = false;

			ModelState.Clear();

			var functiolnalityLevel = FunctionalityLevel.GetLevelByScore(db.FunctionalityLevels, score);
			if (functiolnalityLevel != null)
			{
				score.FunctionalityLevelId = functiolnalityLevel.Id;
			}

            var client = db.Clients.SingleOrDefault(f => f.Id == score.ClientId);
            if(score.StartDate < client.JoinDate)
            {
                ModelState.AddModelError(string.Empty, "Start Date cannot be earlier than client's join date.");
            }

			var existing = db.FunctionalityScores.Any(f => f.StartDate == score.StartDate && f.ClientId == score.ClientId);
			if (existing)
			{
				ModelState.AddModelError(string.Empty,
					"Duplicate functionality scores are not allowed"
				);
			}



			var lastReport = ccEntitiesExtensions.LastSubmittedHcRepDate(score.ClientId);
			if (this.Permissions.User.RoleId != (int)FixedRoles.Admin && score.StartDate < lastReport)
			{
				ModelState.AddModelError(string.Empty, "Start date cannot be within an already submitted Financial Report period.");
			}

			TryValidateModel(score);

			if (ModelState.IsValid)
			{
				try
				{

					score.UpdatedAt = DateTime.Now;
					score.UpdatedBy = this.Permissions.User.Id;


					db.FunctionalityScores.AddObject(score);

					var rowsUpdated = db.SaveChanges();
					if (rowsUpdated > 0)
					{
						score = new FunctionalityScore();
					}
					ViewBag.Success = true;
				}
				catch (ValidationException ex)
				{
					ModelState.AddModelError(ex.GetType().ToString(), ex);
				}
				catch (UpdateException ex)
				{
					ModelState.AddModelError(ex.GetType().ToString(), ex);
					ModelState.AddModelError(string.Empty, ex.InnerException.Message);
				}
				catch (Exception ex)
				{
					ModelState.AddModelError(string.Empty, ex.Message);
				}


			}
			return score;
		}

		[HttpPost]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult Delete(int Id)
		{

			if (!this.Permissions.CanDeleteFuncScore) throw new InvalidOperationException();

			var toDelete = new FunctionalityScore() { Id = Id };
			Repo.FunctionalityScores.Attach(toDelete);
			Repo.FunctionalityScores.Remove(toDelete);
			try
			{
				Repo.SaveChanges();
			}
			catch
			{
				return Content("Delete failed");
			}

			return Content("ok");
		}

		[HttpGet]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult Upload()
		{
			var csvColumnNames = CsvHelper.CsvHelperExtenstions.ColumnHeaderNames<FunctionalityScoresCsvMap>();
			return View(csvColumnNames);
		}

		[HttpPost]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult Upload(HttpPostedFileWrapper file)
		{
			if(file == null)
			{
				ModelState.AddModelError(string.Empty, "Please select a file");
				var csvColumnNames = CsvHelper.CsvHelperExtenstions.ColumnHeaderNames<FunctionalityScoresCsvMap>();
				return View(csvColumnNames);
			}

			var ImportId = Guid.NewGuid();

			string fileName = System.IO.Path.Combine(System.IO.Path.GetTempPath(), ImportId.ToString());

			var csvConf = new CsvHelper.Configuration.CsvConfiguration()
			{
				IsStrictMode = false,
				IsCaseSensitive = false,
				SkipEmptyRecords = true
			};

			csvConf.ClassMapping<FunctionalityScoresCsvMap>();

			var updatedAt = DateTime.Now;
			var updatedBy = this.Permissions.User.Id;

			using (var db = new ccEntities())
			{
				db.Imports.AddObject(new Import()
				{
					Id = ImportId,
					StartedAt = updatedAt,
					UserId = this.Permissions.User.Id
				});
				db.SaveChanges();
			}

			int rowIndex = 1;
			using (var csvReader = new CsvHelper.CsvReader(new System.IO.StreamReader(file.InputStream), csvConf))
			{

				foreach (var csvChunk in csvReader.GetRecords<ImportFunctionalityScore>().Split(10000))
				{
					string connectionString = ConnectionStringHelper.GetProviderConnectionString();

					using (var sqlBulk = new System.Data.SqlClient.SqlBulkCopy(connectionString))
					{
						foreach (var record in csvChunk)
						{
							record.ImportId = ImportId;
							record.UpdatedAt = updatedAt;
							record.UpdatedBy = updatedBy;
							record.RowIndex = rowIndex++;
						}


						var reader = new CC.Web.Helpers.IEnumerableDataReader<ImportFunctionalityScore>(csvChunk);

						sqlBulk.DestinationTableName = "ImportFunctionalityScores";
						sqlBulk.ColumnMappings.Add("ImportId", "ImportId");
						sqlBulk.ColumnMappings.Add("RowIndex", "RowIndex");
						sqlBulk.ColumnMappings.Add("Id", "Id");
						sqlBulk.ColumnMappings.Add("ClientId", "ClientId");
						sqlBulk.ColumnMappings.Add("DiagnosticScore", "DiagnosticScore");
						sqlBulk.ColumnMappings.Add("StartDate", "StartDate");
						sqlBulk.ColumnMappings.Add("FunctionalityLevelId", "FunctionalityLevelId");
						sqlBulk.ColumnMappings.Add("UpdatedAt", "UpdatedAt");
						sqlBulk.ColumnMappings.Add("UpdatedBy", "UpdatedBy");

						sqlBulk.WriteToServer(reader);
					}
				}
			}


			return this.RedirectToAction(f => f.Preview(ImportId));
		}

		[CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult Preview(Guid id)
		{
			CheckImportIdPermissions(id);
			using (var db = new ccEntities())
			{
				var valid = PreviewDataQuery(db, id).Where(f => f.Errors != "").Count() > 0;

				if (valid)
				{
					ModelState.AddModelError(string.Empty, "One or more rows contain errors.");
				}
				return View(id);
			}
		}

		[CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public JsonResult PreviewData(Guid id, CC.Web.Models.jQueryDataTableParamModel jq)
		{
			CheckImportIdPermissions(id);
			using (var db = new ccEntities())
			{
				var q = PreviewDataQuery(db, id);
				var filtered = q;
				var sorted = filtered.OrderByDescending(f => f.Errors.Count());
				sorted = filtered.OrderByField(Request["mDataProp_" + jq.iSortCol_0], jq.sSortDir_0 == "asc");
				if (Request["iSortCol_2"] != null)
					sorted = sorted.ThenByField(Request["mDataProp_" + Request["iSortCol_2"]], Request["sSortDir_2"] == "asc");
				if (Request["iSortCol_2"] != null)
					sorted = sorted.ThenByField(Request["mDataProp_" + Request["iSortCol_2"]], Request["sSortDir_2"] == "asc");

				var result = new jQueryDataTableResult()
				{
					sEcho = jq.sEcho,
					iTotalDisplayRecords = filtered.Count(),
					iTotalRecords = q.Count(),
					aaData = sorted.Skip(jq.iDisplayStart).Take(jq.iDisplayLength).ToList()

				};
				return this.MyJsonResult(result, JsonRequestBehavior.AllowGet);
			}
		}
		private IQueryable<fsimportpreviewrow> PreviewDataQuery(ccEntities db, Guid id)
		{
			var maxDate = DateTime.Now.Date;
			return from i in db.ImportFunctionalityScores
				   where i.ImportId == id
				   join c in db.Clients.Where(this.Permissions.ClientsFilter) on i.ClientId equals c.Id into cG
				   from c in cG.DefaultIfEmpty()
				   from fl in db.FunctionalityLevels.Where(fl => fl.MinScore <= i.DiagnosticScore && i.DiagnosticScore <= fl.MaxScore).DefaultIfEmpty()
				   join dups1 in db.ImportFunctionalityScores.Where(f => f.ImportId == id) on i.ClientId equals dups1.ClientId into dupsg1
				   let lastReport = (from cr in db.ClientReports
									 where cr.ClientId == c.Id
									 join sr in db.SubReports on cr.SubReportId equals sr.Id
									 join mr in db.MainReports.Where(MainReport.Submitted) on sr.MainReportId equals mr.Id
									 where sr.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.Homecare ||
										   sr.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.HomecareWeekly
									 select mr).OrderByDescending(f => f.Start).FirstOrDefault()
				   let max = lastReport != null ? (DateTime?)lastReport.End : (DateTime?)null
				   select new fsimportpreviewrow
				   {
					   RowIndex = i.RowIndex,
					   ClientId = i.ClientId,
					   ClientName = c.FirstName + " " + c.LastName,
					   StartDate = i.StartDate,
					   DiagnosticScore = i.DiagnosticScore,
					   FunctionalityLevelName = fl.Name,
					   Errors =

								(c == null ? "Invalid CC_ID" : "") +
								(i.ClientId == null ? "CC_ID is required" : "") +
								(i.StartDate == null ? "START_DATE is required" : "") +
								(i.DiagnosticScore == null ? "DIAGNOSTIC_SCORE is required" : "") +
								(fl == null ? "Matching Functionality Level not found" : "") +
								(dupsg1.Any(f => f.Id != i.Id && f.StartDate == f.StartDate) ? "There is a duplicate record in the file " : "") +
								(i.StartDate > maxDate ? "Start date can not be in the future" : "") +
								(i.StartDate < c.JoinDate ? "Start date must be greater or equal to Join Date" : "") +
								((this.Permissions.User.RoleId != (int)FixedRoles.Admin && (i.StartDate < max)) ? "Start Date cannot be within an already submitted Financial Report period." : "")

				   };
		}

		[HttpPost]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult Import(Guid id)
		{
			CheckImportIdPermissions(id);
			using (var db = new ccEntities())
			{
				var res = db.ImportFS(id, Permissions.User.AgencyId, Permissions.User.AgencyGroupId, Permissions.User.RegionId);

				return RedirectToAction("Index", "Clients", null);
			}
		}

		private void CheckImportIdPermissions(Guid id)
		{
			using (var db = new ccEntities())
			{
				var import = db.Imports.Where(this.Permissions.ImportsFilter).Where(f => f.Id == id).SingleOrDefault();
				if (import == null)
				{
					throw new InvalidOperationException("Import data not found.");
				}
			}
		}


		private IEnumerable<List<T>> ReadChunks<T>(CsvReader reader, int chunkSize = 10000) where T : class
		{
			var list = new List<T>(chunkSize);
			T item = null;
			while (reader.Read())
			{

				try { item = reader.GetRecord<T>(); }
				catch (IndexOutOfRangeException) { item = reader.GetRecord<T>(); }
				list.Add(item);

				if (list.Count == chunkSize)
				{
					yield return list;
					list = new List<T>(chunkSize);
				}
			}
			yield return list;
		}

	}
}

namespace System
{
	public static partial class MathEx
	{
		public static decimal? Max(params decimal?[] args)
		{
			if (args == null || !args.Any(f => f.HasValue))
			{
				return null;
			}

			return args.Where(f => f.HasValue).Select(f => f.Value).Max();
		}
	}
}