using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CC.Data;
using System.Data.Entity;
using CC.Web.Models;
using MvcContrib;
using MvcContrib.ActionResults;
using System.Data.Objects.SqlClient;

namespace CC.Web.Controllers
{
	public class HomeCareEntitledPeriodsController : PrivateCcControllerBase
	{
		public ActionResult Index(int ClientId)
		{

			var model = new HomeCareEntitledPeriodsIndexModel()
			{
				ClientId = ClientId,
			};
			return View(model);
		}

		public JsonResult IndexDataTable(Models.jQueryDataTableParamModel model)
		{
			var data = Repo.HomeCareEntitledPeriods.Select.Include(f => f.User).Where(f => f.ClientId == model.ClientId);

			bool sortDesc = HttpContext.Request["sSortDir_0"] == "desc";

			switch (HttpContext.Request["iSortCol_0"])
			{
				case "0":
					if (sortDesc) { data = data.OrderByDescending(f => f.StartDate); }
					else { data = data.OrderBy(f => f.StartDate); }
					break;

				case "1":
					if (sortDesc) { data = data.OrderByDescending(f => f.EndDate); }
					else { data = data.OrderBy(f => f.EndDate); }
					break;
				case "2":
					if (sortDesc) { data = data.OrderByDescending(f => f.User.UserName); }
					else { data = data.OrderBy(f => f.User.UserName); }
					break;
				case "3":
					if (sortDesc) { data = data.OrderByDescending(f => f.UpdatedAt); }
					else { data = data.OrderBy(f => f.UpdatedAt); }
					break;
				default:
					data = data.OrderByDescending(f => f.StartDate);
					break;
			}


			var result = data.Skip(model.iDisplayStart).Take(model.iDisplayLength);


			return this.MyJsonResult(new
			{
				sEcho = Request["sEcho"],
				iTotalRecords = data.Count(),
				iTotalDisplayRecords = data.Count(),
				aaData = result.AsEnumerable().Select(f =>
					new
					{
						Id = f.Id,
						StartDate = f.StartDate.ToShortDateString(),
						//The end date is inclusive - substract 1 for the display value
						EndDate = f.EndDate.HasValue ? f.EndDate.Value.AddDays(-1).ToShortDateString() : "",
						UpdatedAt = f.UpdatedAt.HasValue ? f.UpdatedAt.Value.ToShortDateString() : "",
						UpdatedBy = f.User == null ? "" : f.User.UserName
					})

			}, JsonRequestBehavior.AllowGet);
		}



		//
		// GET: /HomeCareEntitledPeriods/Create

		public ActionResult Create()
		{
			return View();
		}


		//
		// POST: /HomeCareEntitledPeriods/Create

		[HttpPost]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult Create(HomeCareEntitledPeriod model)
		{
			if (!this.Permissions.CanUpdateExistingClient) throw new InvalidOperationException();

            var client = db.Clients.Include(f => f.HomeCareEntitledPeriods).SingleOrDefault(f => f.Id == model.ClientId);

            if(model.StartDate < client.JoinDate)
            {
                ModelState.AddModelError(string.Empty, "Start Date cannot be earlier than client's join date.");
            }

			if (this.Permissions.User.RoleId != (int)FixedRoles.Admin)
			{
				var lastReport = ccEntitiesExtensions.LastSubmittedHcRepDate(model.ClientId);
				if (model.StartDate < lastReport)
				{
					ModelState.AddModelError(string.Empty, "Start Date cannot be within an already submitted Financial Report period.");
				}

				if(!Client.CanAddEligibility(client) || model.StartDate < ccEntitiesExtensions.ClientFirstApprovedDate(client.Id))
				{
					ModelState.AddModelError(string.Empty, "You are not allowed to add eligibility for this client on this period.");
				}
			}

			if (ModelState.IsValid)
			{
				try
				{

					Repo.HomeCareEntitledPeriods.Add(model);

					var rowsUpdated = Repo.SaveChanges();

					ViewBag.Success = true;

					return View();

				}
				catch (Exception ex)
				{
					ModelState.AddModelError("Exception", ex.Message);
				}
			}


			return View();
		}




		//
		// GET: /HomeCareEntitledPeriods/Delete/5
		[CcAuthorize(FixedRoles.Admin)]
		public ActionResult Delete(int id)
		{
			return View();
		}

		//
		// POST: /HomeCareEntitledPeriods/Delete/5

		[HttpPost]
		[CcAuthorize(FixedRoles.Admin)]
		public ActionResult Delete(int id, FormCollection collection)
		{
			try
			{
				var model = new HomeCareEntitledPeriod() { Id = id };
				Repo.HomeCareEntitledPeriods.Attach(model);
				Repo.HomeCareEntitledPeriods.Remove(model);
				Repo.SaveChanges();

				return Content("ok");
			}
			catch
			{

				return Content("oh dear, oh dear!");
			}
		}

		[HttpPost]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult Update(Models.EditableDataTableUpdateModel input)
		{

			HomeCareEntitledPeriod model = Repo.HomeCareEntitledPeriods.Select.Single(f => f.Id == input.id);
			if (model.EndDate.HasValue)
			{
				return Content("The end date can't be changed.");
			}

			var ed = input.value.Parse<DateTime>();
			model.EndDate = ed.HasValue ? ed.Value.AddDays(1) : (DateTime?)null;

			if (this.Permissions.User.RoleId != (int)FixedRoles.Admin)
			{
				var lastReport = ccEntitiesExtensions.LastSubmittedHcRepDate(model.ClientId);
				if (model.EndDate < lastReport)
				{
					ModelState.AddModelError(string.Empty, "End Date cannot be within an already submitted Financial Report period.");
				}
			}

			TryValidateModel(model);
			if (ModelState.IsValid)
			{
				var rowsUpdated = Repo.SaveChanges();
				if (model.EndDate.HasValue)
				{
					var cfs = db.CfsRows.FirstOrDefault(f => f.ClientId == model.ClientId && f.EndDate == null);
					if (cfs != null)
					{
						var lastDay = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month));
						cfs.EndDate = lastDay;
						cfs.EndDateReasonId = db.CfsEndDateReasons.SingleOrDefault(f => f.Name == "A leave reason/leave date has been entered in the client details").Id;
						cfs.UpdatedById = this.CcUser.Id;
						try
						{
							db.SaveChanges();
						}
						catch (Exception ex)
						{

						}
					}
				}
			}

			if (ModelState.IsValid)
			{
				return Content("Success");
			}
			else
			{
				return Content(ModelState.ValidationErrorMessages().First());
			}
		}

		private void UpdateProperty(object target, string propertyName, object value)
		{
			var property = System.ComponentModel.TypeDescriptor.GetProperties(target)
				.OfType<System.ComponentModel.PropertyDescriptor>()
				.Single(f => f.Name == propertyName);
			if (value != null)
				value = System.ComponentModel.TypeDescriptor.GetConverter(property.PropertyType).ConvertFromString(value.ToString());
			property.SetValue(target, value);
		}

		#region import
		[CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult Upload()
		{
			var csvColumnNames = CsvHelper.CsvHelperExtenstions.ColumnHeaderNames<ImportHcepCsvMap>();
			return View(csvColumnNames);
		}
		[HttpPost]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult Upload(HttpPostedFileWrapper file)
		{
			if (file == null)
			{
				ModelState.AddModelError(string.Empty, "Please select a file");
				var csvColumnNames = CsvHelper.CsvHelperExtenstions.ColumnHeaderNames<ImportHcepCsvMap>();
				return View(csvColumnNames);
			}

			var ImportId = Guid.NewGuid();

			string fileName = System.IO.Path.Combine(System.IO.Path.GetTempPath(), ImportId.ToString());

			var csvConf = new CsvHelper.Configuration.CsvConfiguration()
			{
				IsStrictMode = false,
				IsCaseSensitive = false,
				SkipEmptyRecords = true,
			};

			csvConf.ClassMapping<ImportHcepCsvMap>();


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

			using (var csvReader = new CsvHelper.CsvReader(new System.IO.StreamReader(file.InputStream), csvConf))
			{
				var csvChunkSize = 10000;
				var recordIndex = 1;
				foreach (var csvChunk in csvReader.GetRecords<ImportHcep>().Split(csvChunkSize))
				{
					string connectionString = ConnectionStringHelper.GetProviderConnectionString();

					using (var sqlBulk = new System.Data.SqlClient.SqlBulkCopy(connectionString))
					{
						foreach (var record in csvChunk)
						{
							record.RowIndex = recordIndex++;
							record.ImportId = ImportId;
							record.UpdatedAt = updatedAt;
							record.UpdatedBy = updatedBy;
						}


						sqlBulk.DestinationTableName = "ImportHcep";

						sqlBulk.ColumnMappings.Add("ImportId", "ImportId");
						sqlBulk.ColumnMappings.Add("RowIndex", "RowIndex");
						sqlBulk.ColumnMappings.Add("Id", "Id");
						sqlBulk.ColumnMappings.Add("ClientId", "ClientId");
						sqlBulk.ColumnMappings.Add("StartDate", "StartDate");
						sqlBulk.ColumnMappings.Add("EndDate", "EndDate");
						sqlBulk.ColumnMappings.Add("UpdatedAt", "UpdatedAt");
						sqlBulk.ColumnMappings.Add("UpdatedBy", "UpdatedBy");

						var reader = new CC.Web.Helpers.IEnumerableDataReader<ImportHcep>(csvChunk);
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

				var HomeCareEntitledPeriods = db.HomeCareEntitledPeriods;
				var ImportHceps = db.ImportHceps;
				var isAdmin = this.Permissions.User.RoleId == (int)FixedRoles.Admin;
				var q = from i in ImportHceps
						where i.ImportId == id
						join c in db.Clients.Where(this.Permissions.ClientsFilter) on i.ClientId equals c.Id into cG
						from c in cG.DefaultIfEmpty()
						join ol in HomeCareEntitledPeriods on i.ClientId equals ol.ClientId into olg
						join sol in ImportHceps.Where(f => f.ImportId == id) on i.ClientId equals sol.ClientId into solg
						let lastReport = (from cr in db.ClientReports
										  where cr.ClientId == c.Id
										  join sr in db.SubReports on cr.SubReportId equals sr.Id
										  join mr in db.MainReports.Where(MainReport.Submitted) on sr.MainReportId equals mr.Id
										  where sr.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.Homecare ||
												sr.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.HomecareWeekly
										  select mr).OrderByDescending(f => f.Start).FirstOrDefault()
						let max = lastReport != null ? (DateTime?)lastReport.End : (DateTime?)null
						let firstApprove = (from h in db.Histories
											 where h.ReferenceId == c.Id && h.TableName == "Clients" && h.FieldName == "ApprovalStatusId"
											 where h.NewValue == "2" || h.NewValue == "2048"
											 select h).OrderBy(f => f.UpdateDate).OrderBy(f => f.UpdateDate).FirstOrDefault()
						where
									c == null ||

									(i.ClientId == null) ||
									(i.StartDate == null) ||
									(i.EndDate < i.StartDate) ||
									(olg.Any(ol => ol.Id != i.Id && (ol.EndDate == null || (i.StartDate < ol.EndDate) && (i.EndDate == null || (ol.StartDate < i.EndDate))))) ||
									(solg.Any(ol => ol.Id != i.Id && (ol.EndDate == null || (i.StartDate < ol.EndDate) && (i.EndDate == null || (ol.StartDate < i.EndDate))))) ||
									(!isAdmin && (i.StartDate < max || i.EndDate < max)) ||
									(!isAdmin && (!(c.HomeCareEntitledPeriods.Any() && (c.ApprovalStatusId == (int)ApprovalStatusEnum.Approved || c.ApprovalStatusId == (int)ApprovalStatusEnum.ResearchinProgresswithProof)
										&& firstApprove != null && i.StartDate >= firstApprove.UpdateDate)))

						select i

					;
				if (q.Count() > 0)
				{
					ModelState.AddModelError(string.Empty, "One or more rows contain errors.");
				}
			}

			return View(id);
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
		public JsonResult PreviewData(Guid id, CC.Web.Models.jQueryDataTableParamModel jq)
		{
			CheckImportIdPermissions(id);

			using (var db = new ccEntities())
			{
				var HomeCareEntitledPeriods = db.HomeCareEntitledPeriods;
				var ImportHceps = db.ImportHceps;
				var q = PreviewDataQuery(id, db, HomeCareEntitledPeriods, ImportHceps);
				var filtered = q;
				var sorted = filtered.OrderByDescending(f => f.Errors.Count());
				if (jq.iSortCol_0 != 1)
				{
					sorted = filtered.OrderByField(Request["mDataProp_" + jq.iSortCol_0], jq.sSortDir_0 == "asc");
				}
				if (Request["iSortCol_1"] != null && jq.iSortCol_0 != 1)
					sorted = sorted.ThenByField(Request["mDataProp_" + Request["iSortCol_1"]], Request["sSortDir_1"] == "asc");
				if (Request["iSortCol_2"] != null && jq.iSortCol_0 != 1)
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

		private IQueryable<ImportHcepPreviewRow> PreviewDataQuery(Guid id, ccEntities db, System.Data.Objects.IObjectSet<HomeCareEntitledPeriod> HomeCareEntitledPeriods, System.Data.Objects.IObjectSet<ImportHcep> ImportHceps)
		{
			var isAdmin = this.Permissions.User.RoleId == (int)FixedRoles.Admin;
			var q = from i in ImportHceps
					where i.ImportId == id
					join c in db.Clients.Where(this.Permissions.ClientsFilter) on i.ClientId equals c.Id into cG
					from c in cG.DefaultIfEmpty()
					join ol in HomeCareEntitledPeriods on i.ClientId equals ol.ClientId into olg
					join sol in ImportHceps.Where(f => f.ImportId == id) on i.ClientId equals sol.ClientId into solg
					let lastReport = (from cr in db.ClientReports
									  where cr.ClientId == c.Id
									  join sr in db.SubReports on cr.SubReportId equals sr.Id
									  join mr in db.MainReports.Where(MainReport.Submitted) on sr.MainReportId equals mr.Id
									  where sr.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.Homecare ||
											sr.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.HomecareWeekly
									  select mr).OrderByDescending(f => f.Start).FirstOrDefault()
					let max = lastReport != null ? (DateTime?)lastReport.End : (DateTime?)null
					let firstApprove = (from h in db.Histories
										where h.ReferenceId == c.Id && h.TableName == "Clients" && h.FieldName == "ApprovalStatusId"
										where h.NewValue == "2" || h.NewValue == "2048"
										select h).OrderBy(f => f.UpdateDate).OrderBy(f => f.UpdateDate).FirstOrDefault()
					select new ImportHcepPreviewRow
					{
						RowIndex = i.RowIndex,
						ClientId = i.ClientId,
						ClientName = c.FirstName + " " + c.LastName,
						StartDate = i.StartDate,
						EndDate = System.Data.Objects.EntityFunctions.AddDays(i.EndDate, -1),
						Errors = new List<string>
							{
								(c==null? "Invalid CC_ID":null),
								(i.ClientId==null?"CC_ID is required":null),
								(i.StartDate==null?"START_DATE is required":null),
								(i.EndDate<i.StartDate?"END_DATE must be greater than START_DATE":null),
								(olg.Any(ol => ol.Id != i.Id && (ol.EndDate == null && i.EndDate == null || ol.EndDate == null && i.EndDate > ol.StartDate || (i.StartDate < ol.EndDate) && (i.EndDate == null || (ol.StartDate < i.EndDate))))?
									"There is an overlapping or duplicate period in the system":null),
								(solg.Any(ol => ol.Id != i.Id && (ol.EndDate == null && i.EndDate == null || ol.EndDate == null && i.EndDate > ol.StartDate || (i.StartDate < ol.EndDate) && (i.EndDate == null || (ol.StartDate < i.EndDate))))?
									"There is an overlapping or duplicate period in the same file": null),
                                ((!isAdmin && (i.StartDate < max || i.EndDate < max)) ?
                                    "Start Date cannot be within an already submitted Financial Report period": null),
								(!isAdmin && (!(c.HomeCareEntitledPeriods.Any() && (c.ApprovalStatusId == (int)ApprovalStatusEnum.Approved || c.ApprovalStatusId == (int)ApprovalStatusEnum.ResearchinProgresswithProof)
										&& firstApprove != null && i.StartDate >= firstApprove.UpdateDate))
									? "You are not allowed to add eligibility for this client on this period" : null)
							}.Where(f => f != null)

					};
			return q;
		}


		[HttpPost]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult Import(Guid id)
		{
			using (var db = new ccEntities())
			{
				var res = db.ImportHcepProc(id, Permissions.User.AgencyId, Permissions.User.AgencyGroupId, Permissions.User.RegionId);

				return RedirectToAction("Index", "Clients");
			}
		}

		#endregion

	}


}
