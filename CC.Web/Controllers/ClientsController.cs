using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using CC.Data;
using CC.Web.Models;
using MvcContrib;
using System.IO;
using System.Web.Hosting;
using CC.Data.Services;
using System.Configuration;
using System.Net;
using System.Text;
using CC.Web.Controllers.Attributes;
using System.Text.RegularExpressions;
using System.Globalization;
using System.ComponentModel.DataAnnotations;
using CC.Web.Helpers;

namespace CC.Web.Controllers
{
	[PasswordExpirationCheckAttribute()]
	public class ClientsController : PrivateCcControllerBase
	{

		#region Privates
		private readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(ClientsController));
		private ccEntities _db = new ccEntities();
		#endregion

		#region Constructor
		public ClientsController()
			: base()
		{
			_log.Debug("constructing");

			_log.Debug("constructed");
		}
		#endregion

		#region Index

		//
		// GET: /Clients/
		[AcceptVerbs(HttpVerbs.Get)]
		public ViewResult Index()
		{
			HttpContext.Server.ScriptTimeout = 1;
			var model = new ClientsListModel(Repo);
			ViewBag.Permissions = this.Permissions;

			model.Permissions = this.Permissions;
			int regionId = -1;
			if (User.IsInRole("RegionReadOnly"))
			{
				using (var db = new ccEntities())
				{
					regionId = db.Regions.Where(this.Permissions.RegionsFilter).OrderBy(f => f.Name).ToDictionary(f => f.Id, f => f.Name).Select(f => f.Key).SingleOrDefault();
				}
			}
			ViewBag.RegionId = regionId;
			var filter = model.Filter;
			return View(model);
		}

		[HttpPost]
		public ActionResult Index(ClientsListModel model)
		{

			var updateModel = model.UpdateModel;
			var updateCount = 0;

			if (updateModel.NewApprovalStatusId == null)
			{
				ModelState.AddModelError("ApprovalStatus", "ApprovalStatus is required");
			}
			if (updateModel.SelectedClientIds == null)
			{
				ModelState.AddModelError("ApprovalStatus", "selection is empty");
			}
			if (!CC.Data.Services.PermissionsFactory.GetPermissionsFor(this.CcUser).CanChangeApprovalStatus)
			{
				ModelState.AddModelError("User", "You are not allowed to change the JNV Status");
			}

			if (ModelState.IsValid)
			{
				//the magic zero!
				var clients = updateModel.SelectedClientIds.Select(ClientId => new Client() { Id = ClientId, ApprovalStatusId = 0 }).ToList();

				clients.ForEach(f => Repo.Clients.Attach(f));

				clients.ForEach(client => client.ApprovalStatusId = updateModel.NewApprovalStatusId.Value);

				try
				{
					updateCount = Repo.SaveChanges();
				}
				catch
				{
					ModelState.AddModelError("", "Updating JNV Status failed");
				}
			}


			model = new ClientsListModel();

			ViewBag.CcUser = this.CcUser;

			if (Request.IsAjaxRequest())
			{
				if (ModelState.IsValid)
				{
					return Content(string.Format("{0} records updated.", updateCount));
				}
				else
				{
					return Content(ModelState.ValidationErrorMessages().First());
				}
			}
			else
			{
				return View(model);
			}
		}



		public ActionResult IndexDataTables(ClientsListDataTableModel param)
		{
			//iSortCol_0:4
			//sSortDir_0:asc

			if (User.IsInRole("BMF"))
			{
				param.GGReportedOnly = true;
			}

			var data = ClientsListDataTableModel.GetClientsList(this.Permissions, this.db, param);

			bool sortDesc = HttpContext.Request["sSortDir_0"] == "desc";
			int? sortColIndex = HttpContext.Request["iSortCol_0"].Parse<int>();


			#region Sort

			switch (sortColIndex)
			{
				case 1:
					if (sortDesc) { data = data.OrderByDescending(f => f.LastName); }
					else { data = data.OrderBy(f => f.LastName); }
					break;
				case 2:
					if (sortDesc) { data = data.OrderByDescending(f => f.FirstName); }
					else { data = data.OrderBy(f => f.FirstName); }
					break;
				case 9:
					if (sortDesc) { data = data.OrderByDescending(f => f.AgencyGroupName); }
					else { data = data.OrderBy(f => f.AgencyGroupName); }
					break;
				case 10:
					if (sortDesc) { data = data.OrderByDescending(f => f.AgencyName); }
					else { data = data.OrderBy(f => f.AgencyName); }
					break;
				case 3:
					if (sortDesc) { data = data.OrderByDescending(f => f.Id); }
					else { data = data.OrderBy(f => f.Id); }
					break;
				case 5:
					if (sortDesc) { data = data.OrderByDescending(f => f.NationalId); }
					else { data = data.OrderBy(f => f.NationalId); }
					break;
				case 11:
					if (sortDesc) { data = data.OrderByDescending(f => f.JoinDate); }
					else { data = data.OrderBy(f => f.JoinDate); }
					break;
				case 6:
					if (sortDesc) { data = data.OrderByDescending(f => f.Address); }
					else { data = data.OrderBy(f => f.Address); }
					break;
				case 7:
					if (sortDesc) { data = data.OrderByDescending(f => f.City); }
					else { data = data.OrderBy(f => f.City); }
					break;
				case 8:
					if (sortDesc) { data = data.OrderByDescending(f => f.BirthCountry); }
					else { data = data.OrderBy(f => f.BirthCountry); }
					break;
				case 4:
					if (sortDesc) { data = data.OrderByDescending(f => f.BirthDate); }
					else { data = data.OrderBy(f => f.BirthDate); }
					break;
				case 12:
					if (sortDesc) { data = data.OrderByDescending(f => f.FunctionalityLevelName); }
					else { data = data.OrderBy(f => f.FunctionalityLevelName); }
					break;
				case 13:
					if (sortDesc) { data = data.OrderByDescending(f => f.ApprovalStatusName); }
					else { data = data.OrderBy(f => f.ApprovalStatusName); }
					break;
				case 14:
					if (sortDesc) { data = data.OrderByDescending(f => f.AllowedHcHours); }
					else { data = data.OrderBy(f => f.AllowedHcHours); }
					break;
				case 15:
					if (sortDesc) { data = data.OrderByDescending(f => f.GfHours); }
					else { data = data.OrderBy(f => f.GfHours); }
					break;
				case 16:
					if (sortDesc) { data = data.OrderByDescending(f => f.GGReportedOnly); }
					else { data = data.OrderBy(f => f.GGReportedOnly); }
					break;
				default:
					data = data.OrderBy(f => f.LastName).ThenBy(f => f.FirstName);
					break;
			}
			#endregion

			#region filter

			#endregion

			param.iTotalRecords = db.Clients.Where(this.Permissions.ClientsFilter).Count();
			param.iTotalDisplayRecords = data.Count();
			param.aaData = data.Skip(param.iDisplayStart).Take(param.iDisplayLength).AsEnumerable();
			return this.MyJsonResult(param, JsonRequestBehavior.AllowGet);
		}

		public ActionResult Export(ClientsListDataTableModel param)
		{
			switch ((ClientExportList)Enum.Parse(typeof(ClientExportList), param.ExportOption))
			{
				case ClientExportList.Clients:
					return ExportClients(param);
				case ClientExportList.Eligibility:
					return EligibilityPeriodsExport(param);
				case ClientExportList.Functionality:
					return FunctionalityScoresExport(param);
				case ClientExportList.ApprovalStatusChanges:
					return ApprovalStatusChangesExport(param);
				case ClientExportList.BEG:
					return BegExport(param);
				case ClientExportList.Duplicates:
					return DuplicateExport(param);
				case ClientExportList.UnmetNeedsOther:
					return UnmetNeedsOtherExport(param);
				case ClientExportList.GovHcHours:
					return GovHcHoursExport(param);
				case ClientExportList.LeaveEntries:
					return LeaveEntriesExport(param);
				case ClientExportList.HAS:
					return HASExport(param);
				default:
					return ExportClients(param);
			}

		}
		public ActionResult ExportClients(ClientsListDataTableModel param)
		{
			db.CommandTimeout = 120;
			var userRole = (FixedRoles)this.CcUser.RoleId;
			var model = ClientsListDataTableModel.GetExportData(db, Permissions, param);
			switch (userRole)
			{
				case FixedRoles.Admin:
				case FixedRoles.AgencyUser:
				case FixedRoles.AgencyUserAndReviewer:
				case FixedRoles.GlobalOfficer:
				case FixedRoles.GlobalReadOnly:
				case FixedRoles.AuditorReadOnly:
				case FixedRoles.RegionAssistant:
				case FixedRoles.RegionOfficer:
				case FixedRoles.RegionReadOnly:
				case FixedRoles.Ser:
				case FixedRoles.SerAndReviewer:
					return this.Excel("output", "Sheet1", model);
				case FixedRoles.BMF:
				default:
					return this.Excel("output", "Sheet1", model.Cast<ClientsExportModelBMF>());
			}
		}
		public ActionResult BegExport(ClientsListDataTableModel param)
		{
			db.CommandTimeout = 120;
			var model = ClientsListDataTableModel.GetBegExportData(db, Permissions, param);
			return this.Excel("output", "Sheet1", model);
		}
		public ActionResult DuplicateExport(ClientsListDataTableModel param)
		{
			db.CommandTimeout = 120;
			var tso = new System.Transactions.TransactionOptions()
			{
				IsolationLevel = System.Transactions.IsolationLevel.Snapshot
			};
			using (var ts = new System.Transactions.TransactionScope(System.Transactions.TransactionScopeOption.Suppress, tso))
			{
				var model = ClientsListDataTableModel.GetDuplicateExportData(db, Permissions, param);
				return this.Excel("output", "Sheet1", model);
			}
		}
		public ActionResult EligibilityPeriodsExport(ClientsListDataTableModel param)
		{
			db.CommandTimeout = 120;
			var model = ClientsListDataTableModel.GetEligibilityPeriodsExportData(db, Permissions, param);
			return this.Excel("output", "Sheet1", model);
		}
		public ActionResult FunctionalityScoresExport(ClientsListDataTableModel param)
		{
			db.CommandTimeout = 120;
			var model = ClientsListDataTableModel.GetFunctionalityScoresExportData(db, Permissions, param);
			return this.Excel("output", "Sheet1", model);
		}
		public ActionResult ApprovalStatusChangesExport(ClientsListDataTableModel param)
		{
			db.CommandTimeout = 120;
			var model = ClientsListDataTableModel.GetApprovalStatusChangesExportData(db, Permissions, param);
			return this.Excel("output", "Sheet1", model);
		}
		public ActionResult UnmetNeedsOtherExport(ClientsListDataTableModel param)
		{
			db.CommandTimeout = 120;
			var model = ClientsListDataTableModel.GetUnmetNeedsOtherExportData(db, Permissions, param);
			return this.Excel("output", "Sheet1", model);
		}
		public ActionResult GovHcHoursExport(ClientsListDataTableModel param)
		{
			db.CommandTimeout = 120;
			var model = ClientsListDataTableModel.GetGovHcHoursExportData(db, Permissions, param);
			return this.Excel("output", "Sheet1", model);
		}
		public ActionResult LeaveEntriesExport(ClientsListDataTableModel param)
		{
			db.CommandTimeout = 120;
			var model = ClientsListDataTableModel.LeaveEntriesExportData(db, Permissions, param);
			return this.Excel("output", "Sheet1", model);
		}
		public ActionResult HASExport(ClientsListDataTableModel param)
		{
			db.CommandTimeout = 120;
			var userRole = (FixedRoles)this.CcUser.RoleId;
			var model = ClientsListDataTableModel.HASExportData(db, Permissions, param);
			if (Permissions.CanSeeProgramField)
			{
				return this.Excel("output", "Sheet1", model);
			}
			else
			{
				return this.Excel("output", "Sheet1", model.Cast<HASExportModelNoProgramField>());
			}
		}
		public ActionResult UpdateData(EditableDataTableUpdateModel param)
		{
			ModelState.Clear();

			var permissions = CC.Data.Services.PermissionsFactory.GetPermissionsFor(this.CcUser);
			var client = Repo.Clients.Select.Single(f => f.Id == param.id);

			if (!permissions.CanChangeApprovalStatus)
			{
				ModelState.AddModelError(param.columnName, "You are not allowed to update this field");
			}

			if (ModelState.IsValid)
			{
				try
				{
					client.SetProp(param.columnName, param.value);
				}
				catch (Exception)
				{
					ModelState.AddModelError(param.columnName, "Failed to set " + param.columnName + "to " + param.value.ToString());
				}
			}
			if (ModelState.IsValid)
			{
				try
				{
					Repo.SaveChanges();
				}
				catch
				{
					ModelState.AddModelError(param.columnName, "Update failed.");
				}
			}
			if (ModelState.IsValid)
			{
				return Content("Update succeeded.");
			}
			else
			{
				return Content("Update failed.");
			}
		}

		#endregion

		#region GovHocHours
		public ActionResult GovHcHours(NewGovHcEntryModel model)
		{
			ViewBag.Permissions = this.Permissions;
			return View(model);
		}
		public JsonResult GovHcHoursData(ClientGovHcHoursTabDataModel model)
		{

			return this.MyJsonResult(model.GetResult(db, this.Permissions));

		}
		[CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult AddNewGovHcHoursEntry(NewGovHcEntryModel model)
		{
			var client = db.Clients.SingleOrDefault(f => f.Id == model.ClientId);
			if (model.GovHcStartDate < client.JoinDate)
			{
				ModelState.AddModelError(string.Empty, "Start Date cannot be earlier than client's join date.");
			}

			if (this.Permissions.User.RoleId != (int)FixedRoles.Admin)
			{
				var lastReport = getLastReport(model.ClientId);

				if (lastReport != null && model.GovHcStartDate < lastReport)
				{
					ModelState.AddModelError(string.Empty, "Start Date cannot be within an already submitted Financial Report period.");
				}
			}

			if (ModelState.IsValid)
			{

				try
				{
					model.Insert(db, this.Permissions);
					return this.MyJsonResult(null);
				}
				catch (Exception ex)
				{
					return this.MyJsonResult(new
					{
						errors = new string[] { ex.Message }
					});
				}

			}
			else
			{
				return this.MyJsonResult(new
				{
					errors = ModelState.SelectMany(f => f.Value.Errors).Select(f => f.ErrorMessage)
				});
			}
		}
		[CcAuthorize(FixedRoles.Admin)]
		public JsonResult DeleteGovHcHoursEntry(int clientId, string startDate)
		{
			var date = DateTime.Parse(startDate).Date;
			var entry = db.GovHcHours.SingleOrDefault(f => f.ClientId == clientId && f.StartDate == date);
			if (entry != null)
			{
				db.GovHcHours.DeleteObject(entry);
				try
				{
					db.SaveChanges();
					return this.MyJsonResult(true);
				}
				catch (Exception ex)
				{
					_log.Error(ex.Message, ex);
					return this.MyJsonResult(false);
				}

			}
			return this.MyJsonResult(false);
		}

		[CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult GovHcHoursUpload()
		{
			var model = new GovHcHoursUploadModel();
			return View(model);
		}

		[HttpPost]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult GovHcHoursUpload(GovHcHoursUploadModel model)
		{
			if (model.file == null)
			{
				ModelState.AddModelError(string.Empty, "Please select a file");
			}
			if (ModelState.IsValid)
			{
				try
				{
					model.Import();
					return this.RedirectToAction(f => f.GovHcHoursPreview(model.id));
				}
				catch (Exception ex)
				{
					ModelState.AddModelError(string.Empty, ex.Message);
				}
			}

			return View(model);
		}

		[CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult GovHcHoursPreview(Guid id)
		{
			//push to db,
			//redir to preview
			var model = new GovHcHoursImportPreviewModel { Id = id };
			model.Permissions = this.Permissions;
			model.db = db;
			TryValidateModel(model);
			return View(model);
		}
		[CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public JsonResult GovHcHoursPreviewData(GovHcHoursImportPreviewDataModel model)
		{
			model.Permissions = this.Permissions;
			var result = model.GetData();
			return this.MyJsonResult(result);
		}

		[HttpPost]
		[ActionName("GovHcHoursImportRollback")]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult GovHcHoursImportRollback(Guid id)
		{
			//push to db,
			try
			{
				db.ImportGovHcHoursCancelProc(id);
			}
			catch (Exception ex)
			{
				ModelState.AddModelError(string.Empty, ex.Message);
				return this.GovHcHoursPreview(id);
			}
			//redir to preview

			return RedirectToAction("Index", "Clients");
		}

		[HttpPost]
		[ActionName("GovHcHoursPreview")]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult ImportGovHcHours(Guid id)
		{
			//push to db,
			try
			{
				db.ImportGovHcHoursProc(id, Permissions.User.Id);
			}
			catch (Exception ex)
			{
				ModelState.AddModelError(string.Empty, ex.Message);
				return this.GovHcHoursPreview(id);
			}
			//redir to preview

			return RedirectToAction("Index", "Clients");
		}
		#endregion

		#region GFHours
		public ActionResult GFHours(NewGFHoursEntryModel model)
		{
			Dictionary<int, string> types = new Dictionary<int, string>();
			types.Add(0, "Grandfathered");
			types.Add(1, "Exceptional");
            types.Add(2, "BMF Approved");

            model.GFHoursTypes = new SelectList(types, "Key", "Value", 0);
			return View(model);
		}
		public JsonResult GFHoursData(ClientGFHoursTabDataModel model)
		{

			return this.MyJsonResult(model.GetResult(db, this.Permissions));

		}
		[CcAuthorize(FixedRoles.Admin)]
		public ActionResult AddNewGFHoursEntry(NewGFHoursEntryModel model)
		{
			var client = db.Clients.SingleOrDefault(f => f.Id == model.ClientId);
			if (model.GFStartDate < client.JoinDate)
			{
				ModelState.AddModelError(string.Empty, "Start Date cannot be earlier than client's join date.");
			}

			if (ModelState.IsValid)
			{

				try
				{
					model.Insert(db, this.Permissions);
					return this.MyJsonResult(null);
				}
				catch (Exception ex)
				{
					return this.MyJsonResult(new
					{
						errors = new string[] { ex.Message }
					});
				}

			}
			else
			{
				return this.MyJsonResult(new
				{
					errors = ModelState.SelectMany(f => f.Value.Errors).Select(f => f.ErrorMessage)
				});
			}
		}
		[CcAuthorize(FixedRoles.Admin)]
		public ActionResult UpdateGFHoursEntry(NewGFHoursEntryModel input)
		{
			var gfHours = db.GrandfatherHours.SingleOrDefault(f => f.ClientId == input.ClientId && f.StartDate == input.GFStartDate);
			if (gfHours == null)
			{
				var coderesult = new HttpStatusCodeResult(404, "GF Hours entry could not be found.");
				return coderesult;
			}
			var dbUser = db.Users.SingleOrDefault(f => f.UserName == User.Identity.Name);
			gfHours.Value = input.Value;
			gfHours.Type = input.Type;
			gfHours.UpdatedAt = DateTime.Now;
			if (dbUser != null)
			{
				gfHours.UpdatedBy = dbUser.Id;
			}
			try
			{
				db.SaveChanges();
			}
			catch (Exception ex)
			{
				string error = ex.Message;
				if (ex.InnerException != null)
				{
					var inner = ex.InnerException;
					while (inner.InnerException != null)
					{
						inner = inner.InnerException;
					}
					if (inner.Message.Contains("CK_GFHours_Value"))
					{
						error = "Grandfather Hours value must be between 0 and 168.";
					}
					else
					{
						error = inner.Message;
					}
				}
				var coderesult = new HttpStatusCodeResult(500, error);
				return coderesult;
			}
			return new EmptyResult();

		}
		[CcAuthorize(FixedRoles.Admin)]
		public JsonResult DeleteGFHoursEntry(int clientId, string startDate)
		{
			var date = DateTime.Parse(startDate).Date;
			var entry = db.GrandfatherHours.SingleOrDefault(f => f.ClientId == clientId && f.StartDate == date);
			if (entry != null)
			{
				db.GrandfatherHours.DeleteObject(entry);
				try
				{
					db.SaveChanges();
					return this.MyJsonResult(true);
				}
				catch (Exception ex)
				{
					_log.Error(ex.Message, ex);
					return this.MyJsonResult(false);
				}

			}
			return this.MyJsonResult(false);
		}
		#endregion



		#region UnmetNeeds

		public ActionResult UnmetNeeds(NewUnmetNeedsEntryModel model)
		{

			return View(model);
		}
		public JsonResult UnmetNeedsData(ClientUnmetNeedsTabDataModel model)
		{

			return this.MyJsonResult(model.GetResult(db, this.Permissions));

		}
		[CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult AddNewUnmetNeedsEntry(NewUnmetNeedsEntryModel model)
		{
			var client = db.Clients.SingleOrDefault(f => f.Id == model.ClientId);
			if (model.WeeklyStartDate < client.JoinDate)
			{
				ModelState.AddModelError(string.Empty, "Start Date cannot be earlier than client's join date.");
			}

			if (this.Permissions.User.RoleId != (int)FixedRoles.Admin)
			{
				var lastReport = getLastReport(model.ClientId);

				if (lastReport != null && model.WeeklyStartDate < lastReport)
				{
					ModelState.AddModelError(string.Empty, "Start Date cannot be within an already submitted Financial Report period.");
				}
			}

			if (ModelState.IsValid)
			{

				try
				{
					model.Insert(db, this.Permissions);
					return this.MyJsonResult(null);
				}
				catch (Exception ex)
				{
					return this.MyJsonResult(new
					{
						errors = new string[] { ex.Message }
					});
				}

			}
			else
			{
				return this.MyJsonResult(new
				{
					errors = ModelState.SelectMany(f => f.Value.Errors).Select(f => f.ErrorMessage)
				});
			}
		}
		public ActionResult CheckFunctionalityForUnmetNeedsEntry(NewUnmetNeedsEntryModel model)
		{
			var funcScore = db.FunctionalityScores.Where(f => f.ClientId == model.ClientId && f.StartDate <= model.WeeklyStartDate)
				.OrderByDescending(f => f.StartDate)
				.Include(f => f.FunctionalityLevel)
				.FirstOrDefault();
			if (funcScore != null)
			{
				if (funcScore.FunctionalityLevelId == 26 && model.WeeklyHours > 10) //Medium Functionality Level
				{
					return this.MyJsonResult(new
					{
						error = "Client has medium functionality for the entered start date, are you sure?"
					});
				}
				else if (funcScore.FunctionalityLevelId == 25 && model.WeeklyHours > 4) //High Functionality Level
				{
					return this.MyJsonResult(new
					{
						error = "Client has high functionality for the entered start date, are you sure?"
					});
				}
				else
				{
					return this.MyJsonResult(null);
				}
			}
			else
			{
				return this.MyJsonResult(new
				{
					error = "Client has no functionality level for the entered start date. Please enter first functionality level before entering unmet needs for this date, Are you sure?"
				});
			}
		}
		[CcAuthorize(FixedRoles.Admin)]
		public JsonResult DeleteUnmetNeedsEntry(int clientId, string startDate)
		{
			var date = DateTime.Parse(startDate).Date;
			var entry = db.UnmetNeeds.SingleOrDefault(f => f.ClientId == clientId && f.StartDate == date);
			if (entry != null)
			{
				db.UnmetNeeds.DeleteObject(entry);
				try
				{
					db.SaveChanges();
					return this.MyJsonResult(true);
				}
				catch (Exception ex)
				{
					_log.Error(ex.Message, ex);
					return this.MyJsonResult(false);
				}

			}
			return this.MyJsonResult(false);
		}



		[CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult UnmetNeedsUpload()
		{
			var model = new UnmetNeedsUploadModel();
			return View(model);
		}

		[HttpPost]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult UnmetNeedsUpload(UnmetNeedsUploadModel model)
		{
			if (model.file == null)
			{
				ModelState.AddModelError(string.Empty, "Please select a file");
			}
			if (ModelState.IsValid)
			{
				try
				{
					model.Import();
					return this.RedirectToAction(f => f.UnmetNeedsPreview(model.id));
				}
				catch (Exception ex)
				{
					ModelState.AddModelError(string.Empty, ex.Message);
				}
			}

			return View(model);
		}

		[CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult UnmetNeedsPreview(Guid id)
		{
			//push to db,
			//redir to preview
			var model = new UnmetNeedsImportPreviewModel { Id = id };
			model.Permissions = this.Permissions;
			model.db = db;
			TryValidateModel(model);
			return View(model);
		}
		[CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public JsonResult UnmetNeedsPreviewData(UnmetNeedsImportPreviewDataModel model)
		{
			model.Permissions = this.Permissions;
			var result = model.GetData();
			return this.MyJsonResult(result);
		}

		[HttpPost]
		[ActionName("UnmetNeedsImportRollback")]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult UnmetNeedsImportRollback(Guid id)
		{
			//push to db,
			try
			{
				db.ImportUnmetNeedsCancelProc(id);
			}
			catch (Exception ex)
			{
				ModelState.AddModelError(string.Empty, ex.Message);
				return this.UnmetNeedsPreview(id);
			}
			//redir to preview

			return RedirectToAction("Index", "Clients");
		}

		[HttpPost]
		[ActionName("UnmetNeedsPreview")]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult ImportUnmetNeeds(Guid id)
		{
			//push to db,
			try
			{
				db.ImportUnmetNeedsProc(id, Permissions.User.Id);
			}
			catch (Exception ex)
			{
				ModelState.AddModelError(string.Empty, ex.Message);
				return this.UnmetNeedsPreview(id);
			}
			//redir to preview

			return RedirectToAction("Index", "Clients");
		}


		#endregion

		#region Unmet Needs Other

		public ActionResult UnmetNeedsOther(NewUnmetNeedsOtherEntryModel model)
		{
			Client client = this.GetClientById(model.ClientId);

			model.ServiceTypes = new SelectList(db.ServiceTypes.Where(f => !f.DoNotReportInUnmetNeedsOther).Select(f => new { Key = f.Id, Value = f.Name }), "Key", "Value");

			model.CUR = client.Agency != null && client.Agency.AgencyGroup != null && !string.IsNullOrEmpty(client.Agency.AgencyGroup.DefaultCurrency) ? client.Agency.AgencyGroup.DefaultCurrency : "";

			return View(model);
		}
		public JsonResult UnmetNeedsOtherData(ClientUnmetNeedsOtherTabDataModel model)
		{

			return this.MyJsonResult(model.GetResult(db, this.Permissions));

		}
		[CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult AddNewUnmetNeedsOtherEntry(NewUnmetNeedsOtherEntryModel model)
		{
			if (!model.ServiceTypeId.HasValue)
			{
				ModelState.AddModelError(string.Empty, "Service Type is required field.");
			}
			var client = db.Clients.SingleOrDefault(f => f.Id == model.ClientId);
			if (db.UnmetNeedsOthers.Any(f => f.ClientId == model.ClientId && f.ServiceTypeId == model.ServiceTypeId))
			{
				ModelState.AddModelError(string.Empty, "The same service type cannot be used more than once per client.");
			}

			if (ModelState.IsValid)
			{

				try
				{
					model.Insert(db, this.Permissions);
					return this.MyJsonResult(null);
				}
				catch (Exception ex)
				{
					return this.MyJsonResult(new
					{
						errors = new string[] { ex.Message }
					});
				}

			}
			else
			{
				return this.MyJsonResult(new
				{
					errors = ModelState.SelectMany(f => f.Value.Errors).Select(f => f.ErrorMessage)
				});
			}
		}
		[CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult SaveUnmetNeedsOther(NewUnmetNeedsOtherEntryModel model)
		{
			if (model.Amount < 0)
			{
				return this.MyJsonResult(new { success = false, errors = new[] { "Invalid Amount value. must be >= 0" } });
			}

			var unmetNeedsOther = db.UnmetNeedsOthers
					.Include(f => f.ServiceType)
					.SingleOrDefault(f => f.ClientId == model.ClientId && f.ServiceTypeId == model.ServiceTypeId);
			if (unmetNeedsOther == null)
			{
				return this.MyJsonResult(new { success = false, errors = new[] { "Entry not found" } });
			}

			try
			{
				unmetNeedsOther.Amount = model.Amount;
				db.SaveChanges();

				return this.MyJsonResult(new { success = true });
			}
			catch (Exception ex)
			{
				_log.Debug(ex.Message, ex);
				return this.MyJsonResult(new { success = false, errors = new[] { ex.Message } });
			}
		}

		[CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult UnmetNeedsOtherUpload(bool newRows)
		{
			var model = new UnmetNeedsOtherUploadModel();
			model.NewRows = newRows;
			return View(model);
		}

		[HttpPost]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult UnmetNeedsOtherUpload(UnmetNeedsOtherUploadModel model)
		{
			if (model.file == null)
			{
				ModelState.AddModelError(string.Empty, "Please select a file");
			}
			if (ModelState.IsValid)
			{
				try
				{
					model.Import();
					return this.RedirectToAction(f => f.UnmetNeedsOtherPreview(model.id, model.NewRows));
				}
				catch (Exception ex)
				{
					ModelState.AddModelError(string.Empty, ex.Message);

				}
			}

			return View(model);
		}

		[CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult UnmetNeedsOtherPreview(Guid id, bool newRows)
		{
			//push to db,
			//redir to preview
			var model = new UnmetNeedsOtherImportPreviewModel { Id = id, NewRows = newRows };
			model.Permissions = this.Permissions;
			model.db = db;
			TryValidateModel(model);
			return View(model);
		}
		[CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public JsonResult UnmetNeedsOtherPreviewData(UnmetNeedsOtherImportPreviewDataModel model, bool newRows)
		{
			model.Permissions = this.Permissions;
			var result = model.GetData(newRows);
			return this.MyJsonResult(result);
		}

		[HttpPost]
		[ActionName("UnmetNeedsOtherImportRollback")]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult UnmetNeedsOtherImportRollback(Guid id, bool newRows)
		{
			//push to db,
			try
			{
				db.ImportUnmetNeedsOtherCancelProc(id);
			}
			catch (Exception ex)
			{
				ModelState.AddModelError(string.Empty, ex.Message);
				return this.UnmetNeedsOtherPreview(id, newRows);
			}
			//redir to preview

			return RedirectToAction("Index", "Clients");
		}

		[HttpPost]
		[ActionName("UnmetNeedsOtherPreview")]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult ImportUnmetNeedsOther(Guid id, bool newRows)
		{
			//push to db,
			try
			{
				if (!newRows)
				{
					db.ImportUnmetNeedsOtherProc(id, Permissions.User.Id);
				}
				else
				{
					db.ImportUnmetNeedsOtherNewProc(id, Permissions.User.Id);
				}
			}
			catch (Exception ex)
			{
				ModelState.AddModelError(string.Empty, ex.Message);
				return this.UnmetNeedsOtherPreview(id, newRows);
			}
			//redir to preview

			return RedirectToAction("Index", "Clients");
		}
		#endregion

		#region History DataTables

		public JsonResult HistoryDataTables(HistoryDataTablesModel param)
		{
			int clientId = 0;
			if (int.TryParse(Request["ClientId"], out clientId))
			{

				var model = new ClientsListModel();

				var client = GetClientById(clientId);
				if (client == null)
				{
					throw new HttpException(404, "client not found/not allowed");
				}

				var data = from f in Repo.Histories.Select
						   where f.TableName == "Clients" || f.TableName == "GrandfatherHours"
						   where f.ReferenceId == param.ClientId
						   select f;
				if (!this.Permissions.CanSeeProgramField)
				{
					data = data.Where(f => f.FieldName != "FundStatusId");
				}

				data = SortHistoryData(data);


				//filter
				var name = param.FieldName == null ? null : param.FieldName.Trim();
				if (name == "") name = null;


				Func<History, bool> where = f => f.UpdateDate >= param.fromDate.Value;

				var filtered = data;
				if (param.fromDate.HasValue)
					filtered = filtered.Where(f => f.UpdateDate >= param.fromDate.Value);
				if (param.toDate.HasValue)
					filtered = filtered.Where(f => f.UpdateDate <= param.toDate.Value);
				if (!string.IsNullOrWhiteSpace(name))
					filtered = filtered.Where(f => f.FieldName.ToLower() == name);


				//skip




				//return
				return this.MyJsonResult(new
				{
					sEcho = param.sEcho,
					iTotalRecords = data.Count(),
					iTotalDisplayRecords = filtered.Count(),
					aaData = filtered
						.Skip(param.iDisplayStart)
						.Take(param.iDisplayLength)
						.ToList().Select(f =>
							new List<string>(){
                            f.UpdateDate.ToString("dd-MMM-yyyy HH:mm"),
                            f.FieldName,
                            f.OldValue,
                            f.NewValue,
                            f.User==null?"":f.User.UserName
                        })

				}, JsonRequestBehavior.AllowGet);
			}
			return new JsonResult();
		}

		private IQueryable<History> SortHistoryData(IQueryable<History> data)
		{
			bool sortDesc = HttpContext.Request["sSortDir_0"] == "desc";
			switch (HttpContext.Request["iSortCol_0"])
			{
				case "0":
					if (sortDesc) { data = data.OrderByDescending(f => f.UpdateDate); }
					else { data = data.OrderBy(f => f.UpdateDate); }
					break;
				case "1":
					if (sortDesc) { data = data.OrderByDescending(f => f.FieldName); }
					else { data = data.OrderBy(f => f.FieldName); }
					break;
				case "2":
					if (sortDesc) { data = data.OrderByDescending(f => f.OldValue); }
					else { data = data.OrderBy(f => f.OldValue); }
					break;
				case "3":
					if (sortDesc) { data = data.OrderByDescending(f => f.NewValue); }
					else { data = data.OrderBy(f => f.NewValue); }
					break;
				case "4":
					if (sortDesc) { data = data.OrderByDescending(f => f.User.UserName); }
					else { data = data.OrderBy(f => f.User.UserName); }
					break;
				default:
					data = data.OrderByDescending(f => f.UpdateDate);
					break;
			}
			return data;
		}


		#endregion

		#region Details

		//GET: /Private/Clients/Details/5
		public ViewResult Details(int? id, bool? newClient)
		{
			if (id == null) return Index();

			if (newClient == true)
				ViewBag.Message = "Client Added Successfully, Please click Edit to further add/ update details";

			Client client = this.GetClientById(id.Value);
			if (client == null) return View("NotFound");

			if (client.CountryId == null && client.Agency != null)
			{
				client.CountryId = client.Agency.AgencyGroup.CountryId;
			}
			if (client != null && client.Agency != null && client.Agency.AgencyGroup != null)
			{
				SetScDc(client.Agency.AgencyGroup);
			}

			if (User.IsInRole("BMF") && (client.GGReportedCount == null || client.GGReportedCount == 0))
			{
				throw new Exception("You are not allowed to view this page");
			}

			ViewBag.Permissions = CC.Data.Services.PermissionsFactory.GetPermissionsFor(this.CcUser);

			AgencyClientPickerModel clientPickerModel = new AgencyClientPickerModel() { AgencyGroupId = client.Agency.AgencyGroup.Id, AgencyId = client.AgencyId, ClientId = client.Id };

			if (client.IsIncomeVerificationRequired() && !client.IncomeCriteriaComplied)
			{
				ModelState.AddModelError("IncomeCriteriaComplied", "Income Criteria Compliance has to be verified and checked, otherwise this client will not be allowed to be reported for any services.");
			}
			bool isCfsActive = false;
			if (client.Agency != null && client.Agency.AgencyGroup != null)
			{
				isCfsActive = client.Agency.AgencyGroup.CfsDate.HasValue;
			}
			ViewBag.IsCfsActive = isCfsActive;
			ViewBag.CanAddEligibility = Client.CanAddEligibility(client);
			return View(client);
		}

		private void SetScDc(AgencyGroup ser)
		{
			ViewBag.isDccSubside = ser.DayCenter;
			ViewBag.isSC = ser.SupportiveCommunities;
		}



		#endregion

		#region Create
		//
		// GET: /Private/Clients/Create

		public ActionResult Create()
		{
			//todo: move to authorization class
			if (!CC.Data.Services.PermissionsFactory.GetPermissionsFor(this.CcUser).CanCreateNewClient)
			{
				return RedirectToAction("Index");
			}

			ClientCreateModel model = new ClientCreateModel();
			PopulateOptions(model);
			model.UserRegionId = db.Users.Where(f => f.UserName == this.Permissions.User.UserName).Select(f => f.Agency != null ? f.Agency.AgencyGroup.Country.RegionId : f.AgencyGroup != null ? f.AgencyGroup.Country.RegionId : 0).SingleOrDefault();
			if (model.UserRegionId == 2)
			{
				model.Countries = db.Countries.Where(f => f.Id == 344).AsEnumerable().Select(f => new SelectListItem() { Text = f.Name, Value = f.Id.ToString() }).ToList();
				model.NationalIdTypes = db.NationalIdTypes.Where(f => f.Id == 1).AsEnumerable().Select(f => new SelectListItem() { Text = f.Name, Value = f.Id.ToString() }).ToList();
			}
			return View(model);
		}

		private void PopulateOptions(ClientCreateModel model)
		{
			ViewBag.FundStatuses = Repo.FundStatuses.Select.ToList();
			ViewBag.FunctionalityLevels = Repo.FunctionalityLevels.Select.ToList().Select(f => new SelectListItem() { Text = f.Name, Value = f.Id.ToString() });
			ViewBag.Countries = Repo.Countries.Select.OrderBy(f => f.Name);

			var countries = Repo.Countries.Select.OrderBy(f => f.Name).AsEnumerable().Select(f => new SelectListItem() { Text = f.Name, Value = f.Id.ToString() }).ToList();
			countries.Insert(0, new SelectListItem() { Text = "", Value = "" });
			model.Countries = countries;

			if (model.Data == null)
			{
				model.States =
				new List<SelectListItem>();
			}
			else
			{
				var q = (from c in _db.Countries
						 where c.Id == model.Data.CountryId
						 from s in c.States
						 select new
						 {
							 Id = s.Id,
							 Name = s.Name
						 }).OrderBy(f => f.Name);

				model.States = new SelectList(q, "Id", "Name", model.Data.StateId);
			}

			var agencies = Repo.Agencies.Select.OrderBy(f => f.Name).AsEnumerable().Select(f => new SelectListItem() { Text = f.Name, Value = f.Id.ToString() }).ToList();
			agencies.Insert(0, new SelectListItem() { Text = "", Value = "" });
			model.Agencies = agencies;

			var approvalStatuses = Repo.ApprovalStatuses.Select.OrderBy(f => f.Name).AsEnumerable().Select(f => new SelectListItem() { Text = f.Name, Value = f.Id.ToString() }).ToList();
			approvalStatuses.Insert(0, new SelectListItem() { Text = "", Value = "" });
			model.ApprovalStatuses = approvalStatuses;

			var idtypes = Repo.NationalIdTypes.Select.OrderBy(f => f.Name).AsEnumerable().Select(f => new SelectListItem() { Text = f.Name, Value = f.Id.ToString() }).ToList();
			idtypes.Insert(0, new SelectListItem() { Text = "", Value = "" });
			model.NationalIdTypes = idtypes;

			var lreasons = Repo.LeaveReasons.Select.AsEnumerable().OrderBy(f => f.Name).AsEnumerable().Select(f => new SelectListItem() { Text = f.Name, Value = f.Id.ToString() }).ToList();
			string notEligible = ((int)LeaveReasonEnum.NotEligible).ToString();
			if (!User.IsInRole(FixedRoles.Admin) && lreasons.Any(f => f.Value == notEligible))
			{
				lreasons.Remove(lreasons.SingleOrDefault(f => f.Value == notEligible));
			}
			lreasons.Insert(0, new SelectListItem() { Text = "", Value = "" });
			model.LeaveReasons = lreasons;

			var commPrefs = Repo.CommunicationsPreferences.Select.AsEnumerable().OrderBy(f => f.Name).AsEnumerable().Select(f => new SelectListItem() { Text = f.Name, Value = f.Id.ToString() }).ToList();
			commPrefs.Insert(0, new SelectListItem() { Text = "", Value = "" });
			model.CommunicationsPreferences = commPrefs;

			var dccs = Repo.DccSubsides.Select.AsEnumerable().OrderBy(f => f.Name).AsEnumerable().Select(f => new SelectListItem() { Text = f.Name, Value = f.Id.ToString() }).ToList();
			dccs.Insert(0, new SelectListItem() { Text = "", Value = "" });
			model.DccSubsides = dccs;

			var careOptions = Repo.CareReceivingOptions.Select.AsEnumerable().OrderBy(f => f.Id).AsEnumerable().Select(f => new SelectListItem() { Text = f.Name, Value = f.Id.ToString() }).ToList();
			careOptions.Insert(0, new SelectListItem() { Text = "", Value = "" });
			model.CareReceivingOptions = careOptions;

		}

		//
		// POST: /Private/Clients/Create
		[HttpPost]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult Create(ClientCreateModel model)
		{
			if (!this.Permissions.CanCreateNewClient)
			{
				return RedirectToAction("Index");
			}

			if (model.Data.Gender == null)
			{
				ModelState.AddModelError("", "The Gender field is required");
				ViewBag.NoGender = true;
			}

			if (model.Data.LeaveDate.HasValue && !model.Data.LeaveReasonId.HasValue)
			{
				ModelState.AddModelError(string.Empty, "Leave Reason is required if Leave Date is specified");
			}
			else if (model.Data.LeaveReasonId.HasValue && !model.Data.LeaveDate.HasValue)
			{
				ModelState.AddModelError(string.Empty, "Leave Date ie required if Leave Reason is specified");
			}
			if (model.Data.LeaveReasonId.HasValue && model.Data.LeaveReasonId == (int)LeaveReasonEnum.NotEligible && this.CcUser.RoleId == (int)FixedRoles.Admin)
			{
				model.Data.AdministrativeLeave = true;
			}
			else if (model.Data.LeaveReasonId.HasValue && model.Data.LeaveReasonId == (int)LeaveReasonEnum.NotEligible)
			{
				throw new InvalidOperationException("You are not allowed to change to Leave Reason Not Eligible.");
			}

			if ((this.CcUser.RoleId == (int)FixedRoles.Admin || this.CcUser.RoleId == (int)FixedRoles.GlobalOfficer) && model.Data.AutoLeaveOverride.HasValue && model.Data.AutoLeaveOverride < DateTime.Today)
			{
				ModelState.AddModelError(string.Empty, "Auto Leave Override Until must be not lower than today");
			}

			if (model.Data.NationalIdTypeId == 0 && !string.IsNullOrEmpty(model.Data.NationalId) && model.Data.NationalId.Length > 4)
			{
				ModelState.AddModelError(string.Empty, "Government Issued ID can be up to 4 digits long");
			}

			if (ModelState.IsValid)
			{
				model.Data.CreatedAt = DateTime.Now;
				model.Data.ApprovalStatusUpdated = DateTime.Now;
				model.Data.UpdatedById = Permissions.User.Id;
				Repo.Clients.Add(model.Data);
				Repo.SaveChanges();
				UpdateByPrivatePay(model.Data.Id, model.Data.CareReceivedId);
				return this.RedirectToAction("Details", new { id = model.Data.Id, newClient = true });
			}


			PopulateOptions(model);
			return View(model);
		}

		[HttpPost]
		[CcAuthorize(FixedRoles.Admin)]
		public JsonResult CreateEligibility(int serId)
		{
			if (!this.Permissions.CanUpdateExistingClient)
			{
				return this.MyJsonResult(new { success = false, errors = new[] { "You are not allowed to perform this action" } });
			}
			var today = DateTime.Today;
			var clients = from c in db.Clients
						  where c.Agency.GroupId == serId && c.JoinDate <= today && (c.LeaveDate == null || c.LeaveDate > today)
						  where c.ApprovalStatusId == (int)ApprovalStatusEnum.New
						  where !c.HomeCareEntitledPeriods.Any()
						  select c;
			foreach (var c in clients)
			{
				HomeCareEntitledPeriod hc = new HomeCareEntitledPeriod()
				{
					StartDate = c.JoinDate,
					ClientId = c.Id,
					UpdatedBy = this.Permissions.User.Id,
					UpdatedAt = DateTime.Now
				};
				db.HomeCareEntitledPeriods.AddObject(hc);
			}
			var rows = clients.Count();
			try
			{
				rows = db.SaveChanges();
			}
			catch (Exception ex)
			{
				var msg = ex.Message;
				if (ex.InnerException != null)
				{
					var inner = ex.InnerException;
					while (inner.InnerException != null)
					{
						inner = inner.InnerException;
					}
					msg = inner.Message;
				}
				return this.MyJsonResult(new { success = false, errors = new[] { msg } });
			}
			return this.MyJsonResult(new { success = true, numOfRows = rows });
		}

		#endregion

		#region Edit
		//
		// GET: /Private/Clients/Edit/5

		[CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.GlobalOfficer, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult Edit(int id)
		{
			if (!this.Permissions.CanUpdateExistingClient && !User.IsInRole("GlobalOfficer"))
			{
				return RedirectToAction("Index");
			}

			var client = GetClientById(id);
			if (client == null) return View("NotFound");

			var regionId = db.Users.Where(f => f.UserName == this.Permissions.User.UserName).Select(f => f.Agency != null ? f.Agency.AgencyGroup.Country.RegionId : f.AgencyGroup != null ? f.AgencyGroup.Country.RegionId : 0).SingleOrDefault();
			if (this.User.IsInRole(FixedRoles.Admin))
			{
				regionId = db.Agencies.Where(f => f.Id == client.AgencyId).Select(f => f.AgencyGroup.Country.RegionId).SingleOrDefault();
			}
			if (regionId == 2)
			{
				bool lazyLoading = Repo.GetLazyLoading();
				Repo.SetLazyLoading(false);
				client.CountryId = 344;
				client.NationalIdTypeId = 1;
				Repo.SetLazyLoading(lazyLoading);
			}

			if (client.CountryId == null && client.Agency != null)
			{
				client.CountryId = client.Agency.AgencyGroup.CountryId;
			}

			var model = new ClientEditModel();
			model.UserRegionId = regionId;

			if (client.Agency != null && client.Agency.AgencyGroup != null)
			{
				model.isDccSubside = client.Agency.AgencyGroup.DayCenter;
			}

			model.User = Repo.CurrentUser;

			model.Data = client;

			model.Principal = HttpContext.User;
			PopulateOptions(model);
			model.CcUser = this.CcUser;

			return View(model);

		}

		/// <summary>
		/// returns a client with calculated fileds (currentfunctionality, etc)
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		private Client GetClientById(int id)
		{
			var data = Repo.Clients.Select
				.Include(f => f.Agency.AgencyGroup.Country)
				.Include(f => f.FundStatus)
				.Include(f => f.FunctionalityScores.Select(d => d.FunctionalityLevel))
				.Select(f =>
					new
					{
						Client = f,
						CurrentFunctionalityLevel = f.FunctionalityScores.Where(s => s.StartDate < DateTime.Now).OrderByDescending(s => s.StartDate).FirstOrDefault().FunctionalityLevel,
						HomeCareEntitled = f.HomeCareEntitledPeriods.Any(h => h.StartDate < DateTime.Now && (h.EndDate == null || DateTime.Now < h.EndDate))
					}
				)
				.SingleOrDefault(c => c.Client.Id == id);
			if (data == null) return null;
			var client = data.Client;
			client.CurrentFunctionalityLevel = data.CurrentFunctionalityLevel;
			client.CurrentGovHcHours = db.GovHcHours.Where(f => f.ClientId == client.Id)
				.Where(f => f.StartDate <= DateTime.Now)
				.OrderByDescending(f => f.StartDate)
				.Select(f => (decimal?)f.Value).FirstOrDefault();
			client.HomeCareEntitled = data.HomeCareEntitled && client.CurrentGovHcHours.HasValue;
			client.HomeCareAllowedHours = db.HcCapsTableRaws
							.Where(cap => cap.ClientId == client.Id)
							.Where(cap => cap.StartDate <= DateTime.Now)
							.Where(cap => cap.EndDate == null || cap.EndDate > DateTime.Now)
							.OrderByDescending(cap => cap.StartDate)
							.Select(cap => cap.HcCap)
							.FirstOrDefault();
			client.HasOpenCfsRecord = db.Clients.Any(f => f.Id == client.Id && f.CfsRows.Any(c => c.EndDate == null));
			client.CurrentHomeCareApprovalStatus = (from a in db.ClientHcStatuses
													where a.ClientId == client.Id
													where a.StartDate <= DateTime.Now
													orderby a.StartDate descending
													select a.HcStatus.Name).FirstOrDefault();

            var SCLevelid = client.Agency.AgencyGroup.ScSubsidyLevelId;
            if(SCLevelid ==2)
                {
                client.SC_MonthlyCost = 15;
                }
            else
            {
                client.SC_MonthlyCost = 25;
            }

           // if (client.CountryId == 252) client.CurrentHomeCareApprovalStatus = 1;
            var allowd = Permissions.ClientsFilter.Compile();

			if (User.IsInRole("BMF"))
			{
				client.Duplicates = db.Clients.Where(f =>
					((f.MasterIdClcd) == (client.MasterIdClcd)) && f.Id != client.Id && f.GGReportedCount > 0
					)
				.Select(f => new { Id = f.Id, FirstName = f.FirstName, LastName = f.LastName, AgencyName = f.Agency.Name })
				.ToList()
				 .Select(f => new Client.DuplicateLinks()
				 {
					 Id = f.Id,
					 Name = string.Format("{0} {1} {2} (Agency: {3})", f.Id, f.FirstName, f.LastName, f.AgencyName),
					 Allowed = true
				 }
					).ToList();
			}
			else
			{
				client.Duplicates = db.Clients.Where(f =>
					((f.MasterIdClcd) == (client.MasterIdClcd)) && f.Id != client.Id
					)
				.Select(f => new { Id = f.Id, FirstName = f.FirstName, LastName = f.LastName, AgencyName = f.Agency.Name })
				.ToList()
				 .Select(f => new Client.DuplicateLinks()
					{
						Id = f.Id,
						Name = string.Format("{0} {1} {2} (Agency: {3})", f.Id, f.FirstName, f.LastName, f.AgencyName),
						Allowed = true
					}
					).ToList();
			}


			return client;
		}

		//
		// POST: /Private/Clients/Edit/5
		[HttpPost]
		[ValidateInput(false)]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.GlobalOfficer, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult Edit(ClientEditModel model)
		{
			model.CcUser = this.CcUser;
			if (!this.Permissions.CanUpdateExistingClient && !User.IsInRole("GlobalOfficer"))
			{
				return RedirectToAction("Index");
			}



			using (var db = new ccEntities(false, false))
			{
				db.ContextOptions.ProxyCreationEnabled = false;
				db.ContextOptions.LazyLoadingEnabled = false;

				var existingClient = db.Clients.Where(this.Permissions.ClientsFilter).Include(f => f.HomeCareEntitledPeriods).Include(f => f.CfsRows).SingleOrDefault(f => f.Id == model.Data.Id);
				bool existingClientHasLeaveDate = existingClient.LeaveDate.HasValue;

				if (existingClient == null)
				{
					throw new InvalidOperationException("Client not found");
				}
				else
				{
					var existingClientEntry = db.ObjectStateManager.GetObjectStateEntry(existingClient);

					if (this.CcUser.RoleId != (int)FixedRoles.Admin && model.Data.ApprovalStatusId != (int)ApprovalStatusEnum.New && model.Data.NationalIdTypeId == 1 && model.Data.NationalIdTypeId == existingClient.NationalIdTypeId && !model.Data.NationalId.Equals(existingClient.NationalId))
					{
						throw new InvalidOperationException("You are not allowed to change Government Issued ID of the client.");
					}

					if (existingClient.ApprovalStatusEnum == ApprovalStatusEnum.Approved && !Permissions.CanChangeApprovedClientName)
					{
						bool noChanges =
							((existingClient.FirstName ?? string.Empty) == (model.Data.FirstName ?? string.Empty)) &&
							((existingClient.LastName ?? string.Empty) == (model.Data.LastName ?? string.Empty));
						if (!noChanges)
						{
							ModelState.AddModelError(string.Empty, "First Name / Last Name cannot be revised for Approved clients.");
						}
					}
					if (!Permissions.CanChangeApprovalStatus && existingClient.ApprovalStatusId != model.Data.ApprovalStatusId)
					{
						throw new InvalidOperationException("You are not allowed to change JNV Status of the client.");
					}

					if (this.CcUser.RoleId != (int)FixedRoles.Admin && this.CcUser.RoleId != (int)FixedRoles.GlobalOfficer)
					{
						var lastReport = getLastMainReport(existingClient.Id);
						var firstReport = getFirstMainReport(existingClient.Id);
						//the actual leave date is extended in case of death
						var leaveDateExt = model.Data.LeaveDate;
						if (leaveDateExt.HasValue)
						{
							if (model.Data.DeceasedDate.HasValue || model.Data.LeaveReasonId == (int)LeaveReasonEnum.Deceased)
							{
								leaveDateExt = leaveDateExt.Value.AddDays(SubReport.DeceasedDaysOverhead);
							}
						}
						//join date must be less than firs submitted hc report period start date
						//validation only on join date update
						if (model.Data.JoinDate.Date != existingClient.JoinDate.Date)
						{
							if (model.Data.JoinDate > firstReport)
							{
								if (existingClient.JoinDate == null)
								{
									ModelState.AddModelError(string.Empty, "New Value of Join Date is not allowed since it is within an already submitted Financial Report period.");
								}
								else
								{
									ModelState.AddModelError(string.Empty, "Join Date update is not allowed since it is within an already submitted Financial Report period.");
								}
							}
						}
						//leave date must be greater than the last submitted hc report period end date
						//no need to chech deceased date as it must be greater than the leavedate
						//validation only on leave date update
						if ((model.Data.LeaveDate ?? default(DateTime)).Date != (existingClient.LeaveDate ?? default(DateTime)).Date)
						{
							if (leaveDateExt.HasValue)
							{
								if (leaveDateExt < lastReport)
								{
									if (existingClient.LeaveDate.HasValue)
									{
										ModelState.AddModelError(string.Empty, "Leave Date update is not allowed since it is within an already submitted Financial Report period.");
									}
									else
									{
										ModelState.AddModelError(string.Empty, "New Value of Leave Date is not allowed since it is within an already submitted Financial Report period.");
									}
								}
							}
						}
					}
					else if (model.Data.AutoLeaveOverride.HasValue && model.Data.AutoLeaveOverride < DateTime.Today)
					{
						ModelState.AddModelError(string.Empty, "Auto Leave Override Until must be not lower than today");
					}

					if (model.Data.LeaveDate.HasValue && !model.Data.LeaveReasonId.HasValue)
					{
						ModelState.AddModelError(string.Empty, "Leave Reason is required if Leave Date is specified");
					}
					else if (model.Data.LeaveReasonId.HasValue && !model.Data.LeaveDate.HasValue)
					{
						ModelState.AddModelError(string.Empty, "Leave Date ie required if Leave Reason is specified");
					}
					if (model.Data.LeaveReasonId.HasValue && model.Data.LeaveReasonId == (int)LeaveReasonEnum.NotEligible && this.CcUser.RoleId == (int)FixedRoles.Admin)
					{
						model.Data.AdministrativeLeave = true;
					}
					else if (model.Data.LeaveReasonId.HasValue && model.Data.LeaveReasonId == (int)LeaveReasonEnum.NotEligible)
					{
						throw new InvalidOperationException("You are not allowed to change to Leave Reason Not Eligible.");
					}

					if (model.Data.NationalIdTypeId == 0 && !string.IsNullOrEmpty(model.Data.NationalId) && model.Data.NationalId.Length > 4)
					{
						ModelState.AddModelError(string.Empty, "Government Issued ID can be up to 4 digits long");
					}

					var originalApprovalStatus = existingClient.ApprovalStatusEnum;
					var currentApprovalStatus = model.Data.ApprovalStatusEnum;
					var originalHcStatus = GetLastHcStatus(existingClient.Id);

					if (originalApprovalStatus != currentApprovalStatus)
						model.Data.ApprovalStatusUpdated = DateTime.Now;

					if (User.IsInRole(FixedRoles.GlobalOfficer) || User.IsInRole(FixedRoles.AgencyUser) || User.IsInRole(FixedRoles.Ser) || User.IsInRole(FixedRoles.AgencyUserAndReviewer) || User.IsInRole(FixedRoles.SerAndReviewer))
					{
						if (existingClient.CountryId != model.Data.CountryId && model.Data.CountryId != 344 && model.UserRegionId != 2)
						{
							ModelState.AddModelError("CountryId", "Country of resedence field can not be updated if not empty.");
						}
						if (existingClient.BirthCountryId != model.Data.BirthCountryId)
						{

							ModelState.AddModelError("BirthCountryId", "Birth country field can not be updated if not empty.");
						}
					}
					UpdateClient(existingClient, model.Data, Permissions);




					if (ModelState.IsValid)
					{



						var rowsUpdated = db.SaveChanges();
						var currentHcStatus = GetLastHcStatus(existingClient.Id);
						var currentHcStatusName = currentHcStatus.Value;
						var hcStatusChanged = originalHcStatus.Key != currentHcStatus.Key;

						if (hcStatusChanged && originalApprovalStatus == currentApprovalStatus)
						{
							var emailModel = new CC.Web.Areas.Admin.Models.ClientHcStatusChangeEmailModel
							{
								ClientId = existingClient.Id,
								AgencyId = existingClient.AgencyId,
								ClientName = existingClient.FirstName + " " + existingClient.LastName,
								BirthCountryName = db.BirthCountries.Where(f => f.Id == existingClient.BirthCountryId).Select(f => f.Name).FirstOrDefault(),
								CountryName = db.Countries.Where(f => f.Id == existingClient.CountryId).Select(f => f.Name).FirstOrDefault(),
								ApprovalStatusName = currentApprovalStatus.DisplayName(),
								HcStatusName = currentHcStatusName
							};
							using (var j = new CC.Web.Jobs.HcStatusChangeNotificationsJob())
							{
								j.SendNotifications(emailModel);
							}
						}
						else if (originalApprovalStatus != currentApprovalStatus)
						{
							var agency = db.Agencies.SingleOrDefault(f => f.Id == existingClient.AgencyId);
							if (agency != null)
							{
								CC.Web.Areas.Admin.Controllers.ClientApprovalStatusController.SendStatusChangeNotifications(new[] {
									new ImportClientFundStatusProc_Result {
										AgencyGroupId = agency.GroupId,
										AgencyId = agency.Id,
                                        ClientId = existingClient.Id,
										FirstName = existingClient.FirstName,
										LastName = existingClient.LastName,
										NewApprovalStatusId = (int)currentApprovalStatus,
										OldApprovalStatusId = (int)originalApprovalStatus,
										HcStatusName = currentHcStatusName
									}
								});
							}
						}
						if (originalApprovalStatus != currentApprovalStatus)
						{
							var agency = db.Agencies.SingleOrDefault(f => f.Id == existingClient.AgencyId);
							if (agency != null)
							{

								var IsraelCashSerNumber = System.Web.Configuration.WebConfigurationManager.AppSettings["IsraelCashSerNumber"].Parse<int>();
								if (agency.GroupId == IsraelCashSerNumber)
								{
									ExportCsvForChangedApprovalStatus(new[] {
                                        new ImportClientFundStatusProc_Result {
                                            ClientId = existingClient.Id,
                                            NationalId = existingClient.NationalId,
                                            NewApprovalStatusId = (int)currentApprovalStatus
                                        }
                                    });
								}
							}

						}

						if (currentApprovalStatus == ApprovalStatusEnum.NotEligible && originalApprovalStatus != ApprovalStatusEnum.NotEligible)
						{
							var q = new[] { model }.Select(f => new DisapprovedClientsEmailModel()
							{
								Agency = db.Agencies.SingleOrDefault(a => a.Id == model.Data.AgencyId),
								Clients = new[] { model }.Select(a => new DisapprovedClientsEmailModel.ClientEntry()
								{
									Date = DateTime.Now,
									Id = a.Data.Id,
									Name = a.Data.FirstName + " " + a.Data.LastName

								})

							});
							SendDisApproveClientsNotificationEmail(q);
						}

						if (rowsUpdated > 0)
						{
							UpdateByPrivatePay(existingClient.Id, existingClient.CareReceivedId);
							if (model.Data.LeaveDate.HasValue && !existingClientHasLeaveDate && existingClient.CfsRows.Any(f => f.EndDate == null))
							{
								var cfs = db.CfsRows.FirstOrDefault(f => f.ClientId == existingClient.Id && f.EndDate == null);
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
							return RedirectToAction("Details", new { id = existingClient.Id });
						}
					}
				}
				PopulateOptions(model);
				return View(model);
			}
		}
		private KeyValuePair<int?, string> GetLastHcStatus(int clientid)
		{
			var x = db.ClientHcStatuses
				.Include(f => f.HcStatus)
				.Where(f => f.ClientId == clientid)
				.OrderByDescending(f => f.StartDate)
				.Select(f => new
				{
					Id = (int?)f.HcStatus.Id,
					f.HcStatus.Name
				})
				.FirstOrDefault();
			if (x == null)
			{
				return new KeyValuePair<int?, string>();
			}
			else
			{
				return new KeyValuePair<int?, string>(x.Id, x.Name);
			}
		}

		private string NormalizeString(string s)
		{
			if (s == null) return s;
			else return s.Trim();
		}
		private void UpdateClient(Client target, Client source, CC.Data.Services.IPermissionsBase permissions)
		{
			//main details
			target.Id = source.Id;
			target.FirstName = NormalizeString(source.FirstName);
			target.MiddleName = NormalizeString(source.MiddleName);
			target.LastName = NormalizeString(source.LastName);
			target.AgencyId = source.AgencyId;
			if (ClientEditModel.IsNationalIdEditable(target, permissions.User))
			{
				target.NationalId = source.NationalId;
				target.NationalIdTypeId = source.NationalIdTypeId;
			}
			target.InternalId = NormalizeString(source.InternalId);
			if (source.ApprovalStatusId == (int)ApprovalStatusEnum.New || !User.IsInRole(FixedRoles.AgencyUser) && !User.IsInRole(FixedRoles.Ser) && !User.IsInRole(FixedRoles.AgencyUserAndReviewer) && !User.IsInRole(FixedRoles.SerAndReviewer))
			{
				target.BirthDate = source.BirthDate;
			}
			target.Address = NormalizeString(source.Address);
			target.City = NormalizeString(source.City);
			target.StateId = source.StateId;
			target.ZIP = NormalizeString(source.ZIP);
			//leave/join
			target.JoinDate = source.JoinDate;
			//only admin can change leavedate/leave reason if client is marked as administrative leave 
			if (permissions.User.RoleId == (int)FixedRoles.Admin || permissions.User.RoleId == (int)FixedRoles.GlobalOfficer)
			{
				target.AdministrativeLeave = source.AdministrativeLeave;
			}
			if (!target.AdministrativeLeave || permissions.User.RoleId == (int)FixedRoles.Admin || permissions.User.RoleId == (int)FixedRoles.GlobalOfficer)
			{
				target.LeaveDate = source.LeaveDate;
				target.LeaveReasonId = source.LeaveReasonId;
			}
			target.LeaveRemarks = NormalizeString(source.LeaveRemarks);
			target.DeceasedDate = source.DeceasedDate;

			//other
			target.NaziPersecutionDetails = NormalizeString(source.NaziPersecutionDetails);
			target.Remarks = NormalizeString(source.Remarks);
			//eligibility
			target.IncomeCriteriaComplied = source.IncomeCriteriaComplied;
			if (permissions.CanChangeApprovalStatus)
				target.ApprovalStatusId = source.ApprovalStatusId;
			target.IsCeefRecipient = source.IsCeefRecipient;
			target.CeefId = NormalizeString(source.CeefId);
			target.AddCompName = NormalizeString(source.AddCompName);
			target.AddCompId = NormalizeString(source.AddCompId);
			//functionality
			//target.GfHours = source.GfHours;
			//personal
			target.PobCity = NormalizeString(source.PobCity);
			target.BirthCountryId = source.BirthCountryId;
			target.CountryId = source.CountryId;
			target.PrevFirstName = NormalizeString(source.PrevFirstName);
			target.PrevLastName = NormalizeString(source.PrevLastName);
			target.Phone = NormalizeString(source.Phone);
			target.CompensationProgramName = NormalizeString(source.CompensationProgramName);
			target.OtherFirstName = NormalizeString(source.OtherFirstName);
			target.OtherLastName = NormalizeString(source.OtherLastName);
			target.OtherDob = source.OtherDob;
			target.OtherId = NormalizeString(source.OtherId);
			target.OtherIdTypeId = source.OtherIdTypeId;
			target.OtherAddress = NormalizeString(source.OtherAddress);
			target.PreviousAddressInIsrael = NormalizeString(source.PreviousAddressInIsrael);
			target.Gender = source.Gender;

			target.UpdatedAt = DateTime.Now;
			target.UpdatedById = permissions.User.Id;
			target.AustrianEligible = source.AustrianEligible;
			target.RomanianEligible = source.RomanianEligible;
			if (source.ApprovalStatusUpdated.HasValue)
			{
				target.ApprovalStatusUpdated = source.ApprovalStatusUpdated;
			}
			target.DCC_Client = source.DCC_Client;
			target.SC_Client = source.SC_Client;
			target.DCC_Subside = source.DCC_Subside;
			target.DCC_VisitCost = source.DCC_VisitCost;
			target.SC_MonthlyCost = source.SC_MonthlyCost;
			target.HomecareWaitlist = source.HomecareWaitlist;
            target.UnableToSign = source.UnableToSign;
            target.OtherServicesWaitlist = source.OtherServicesWaitlist;
			if (permissions.User.RoleId == (int)FixedRoles.Admin || permissions.User.RoleId == (int)FixedRoles.GlobalOfficer)
			{
				target.AutoLeaveOverride = source.AutoLeaveOverride;
			}
			if (target.MAFDate == null || User.IsInRole(FixedRoles.Admin))
			{
				target.MAFDate = source.MAFDate;
			}
			if (target.MAF105Date == null || User.IsInRole(FixedRoles.Admin))
			{
				target.MAF105Date = source.MAF105Date;
			}
            if (target.HAS2Date == null || User.IsInRole(FixedRoles.Admin))
            {
                target.HAS2Date = source.HAS2Date;
            }
            target.CommPrefsId = source.CommPrefsId;
			target.CareReceivedId = source.CareReceivedId;
		}

		private void UpdateByPrivatePay(int clientId, int? CareReceivedId)
		{
			var privatePay = db.CareReceivingOptions.SingleOrDefault(f => f.Id == 3); //Private Pay Family
			if (privatePay != null && privatePay.Id == CareReceivedId)
			{
				if (db.CfsRows.Any(f => f.ClientId == clientId && f.EndDate == null))
				{
					var cfs = db.CfsRows.OrderByDescending(f => f.StartDate).FirstOrDefault(f => f.ClientId == clientId && (f.StartDate == null || f.StartDate < DateTime.Now) && f.EndDate == null);
					if (cfs != null && !cfs.ClientResponseIsYes)
					{
						cfs.ClientResponseIsYes = true;
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
		}

		[HttpPost]
		[CcAuthorize(FixedRoles.Admin)]
		public ActionResult ButchApprovalStatusUpdate(List<int> ClientIds, int newStatusId)
		{
			if (!this.Permissions.CanChangeApprovalStatus)
			{
				return RedirectToAction("Index");
			}

			var clients = ClientIds.Select(ClientId => new Client() { Id = ClientId }).ToList();

			clients.ForEach(f => Repo.Clients.Attach(f));

			clients.ForEach(client => client.ApprovalStatusId = newStatusId);

			try
			{
				Repo.SaveChanges();

				return Content("ok");
			}
			catch (Exception)
			{
				return Content("fail");
			}
		}


		#endregion

		#region Delete
		//
		// GET: /Private/Clients/Delete/5
		[HttpGet]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUser, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult Delete(int id)
		{
			Client client = Repo.Clients.Select.SingleOrDefault(c => c.Id == id);

			ViewBag.HasDaf = db.Dafs.Any(f => f.ClientId == id);

			if (client == null)
			{
				ModelState.AddModelError(string.Empty, "Cleint not found");
			}
			else
			{
				CheckSlaves(id);
			}
			return View(client);
		}
		private void CheckSlaves(int id)
		{
			var duplicateClients = _db.Clients.Where(f => f.MasterId == id && f.Id != id).Any();
			if (duplicateClients)
			{
				ModelState.AddModelError(string.Empty, "This Client can not be deleted. There are one or more clients whose MasterId is set to this client's CC ID.");
			}
			if (_db.ClientReports.Any(f => f.ClientId == id) || _db.EmergencyReports.Any(f => f.ClientId == id) || _db.MhmReports.Any(f => f.ClientId == id) || _db.MedicalEquipmentReports.Any(f => f.ClientId == id))
			{
				ModelState.AddModelError(string.Empty, "This client appears in financial reports.");
			}
		}

		//
		// POST: /Private/Clients/Delete/5

		[HttpPost, ActionName("Delete")]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUser, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult DeleteConfirmed(int id, int deleteReasonId)
		{

			using (var db = new ccEntities())
			{
				var c = db.Clients.Where(this.Permissions.ClientsFilter).SingleOrDefault(f => f.Id == id);
				if (c == null)
				{
					ModelState.AddModelError(string.Empty, "Cleint not found");
				}
				else
				{
					var now = DateTime.Now;
					CheckSlaves(id);
					var dafs = db.Dafs.Where(f => f.ClientId == id).Select(CC.Data.Repositories.DafRepository.sdf(now, Permissions.User.Id))
								.ToList();
					ViewBag.HasDaf = dafs.Any();
					if (ModelState.IsValid)
					{
						var dc = new DeletedClient()
						{
							Address = c.Address,
							BirthDate = c.BirthDate,
							DeletedAt = DateTime.Now,
							DeletedByUserId = this.Permissions.User.Id,
							Id = c.Id,
							DeleteRasonId = deleteReasonId,
							JoinDate = c.JoinDate,
							LeaveDate = c.LeaveDate,
							Name = c.FirstName + " " + c.LastName
						};
						foreach (var item in dafs)
						{
							db.DafDeleteds.AddObject(item);
						}
						foreach (var item in c.Dafs.ToList())
						{
							c.Dafs.Remove(item);
							db.Dafs.DeleteObject(item);
						}


						db.DeletedClients.AddObject(dc);
						db.Clients.DeleteObject(c);

						var rowsUpdated = db.SaveChanges();

						return RedirectToAction("Index");
					}
				}
			}
			return View(_db.Clients.Where(this.Permissions.ClientsFilter).SingleOrDefault(f => f.Id == id));
		}

		#endregion

		#region Details/Edit

		public ActionResult HomeCareTab(int id)
		{
			var model = new ClientHomeCareTabJqModel { ClientId = id };
			if (User.IsInRole("BMF"))
			{
				model.GGOnly = true;
			}
			model.LoadData(db, Permissions);
			return View(model);
		}
		public JsonResult HomeCareTabData(ClientHomeCareTabJqModel model)
		{
			var result = model.GetResult(db, Permissions);

			return this.MyJsonResult(result);
		}
		public ActionResult EmergenciesTab(int id)
		{
			var model = new ClientEmergenciesTabModel { ClientId = id };
			if (User.IsInRole("BMF"))
			{
				model.GGOnly = true;
			}
			model.LoadData(db, Permissions);
			return View(model);
		}
		public JsonResult EmergenciesTabData(ClientEmergenciesTabModel model)
		{
			var result = model.GetResult(db, Permissions);

			return this.MyJsonResult(result);
		}
		public ActionResult OtherServicesTab(int id)
		{
			var model = new ClientOtherServicesTabModel { ClientId = id };
			if (User.IsInRole("BMF"))
			{
				model.GGOnly = true;
			}
			model.LoadData(db, Permissions);
			return View(model);
		}
		public JsonResult OtherServicesTabData(ClientOtherServicesTabModel model)
		{
			var result = model.GetResult(db, Permissions);

			return this.MyJsonResult(result);
		}

		#endregion

		#region Imports

		[CcAuthorize(FixedRoles.Admin, FixedRoles.AgencyUser, FixedRoles.Ser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult ImportHome()
		{
			return View();
		}

		#region Upload

		[CcAuthorize(FixedRoles.Admin, FixedRoles.AgencyUser, FixedRoles.Ser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult UploadNew()
		{
			var model = new NewClientsImportModel();
			return View("Upload", model);
		}

		[CcAuthorize(FixedRoles.Admin, FixedRoles.AgencyUser, FixedRoles.Ser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult UploadExisting()
		{
			var model = new ExistingClientsImportModel();
			return View("Upload", model);
		}

		[HttpPost]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.AgencyUser, FixedRoles.Ser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult UploadNew(NewClientsImportModel model)
		{
			if (model.File == null)
			{
				ModelState.AddModelError(string.Empty, "Please select a file");
				return View("Upload", model);
			}
			model.Permissions = this.Permissions;
			model.ProcessFile(true);
			return this.RedirectToAction(f => f.PreviewNew(model.Id));
		}

		[HttpPost]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.AgencyUser, FixedRoles.Ser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult UploadExisting(NewClientsImportModel model)
		{
			if (model.File == null)
			{
				ModelState.AddModelError(string.Empty, "Please select a file");
				return View("Upload", model);
			}
			model.Permissions = this.Permissions;
			model.ProcessFile(false);
			return this.RedirectToAction(f => f.PreviewExisting(model.Id));
		}

		#endregion

		#region Preview View
		[CcAuthorize(FixedRoles.Admin, FixedRoles.AgencyUser, FixedRoles.Ser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult PreviewNew(Guid id)
		{
			var model = new ImportPreviewModel
				{
					Id = id,
					ImportAction = "New"
				};

			return Preview(model);
		}

		[CcAuthorize(FixedRoles.Admin, FixedRoles.AgencyUser, FixedRoles.Ser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult PreviewExisting(Guid id)
		{
			var model = new ImportPreviewModel
			{
				Id = id,
				ImportAction = "Existing"
			};
			return Preview(model);
		}

		[CcAuthorize(FixedRoles.Admin, FixedRoles.AgencyUser, FixedRoles.Ser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		private ActionResult Preview(ImportPreviewModel model)
		{
			using (var db = new ccEntities())
			{
				CheckImportIdPermissions(model.Id, db);

				return View("Preview", model);
			}
		}
		#endregion

		#region Preview Data

		/// <summary>
		/// throws exception if current userid <> import.userid except admin
		/// </summary>
		/// <param name="id"></param>
		/// <param name="db"></param>
		private void CheckImportIdPermissions(Guid id, ccEntities db)
		{
			var import = db.Imports.Where(this.Permissions.ImportsFilter).Where(f => f.Id == id).SingleOrDefault();
			if (import == null)
			{
				throw new InvalidOperationException("Import data not found.");
			}
		}

		[CcAuthorize(FixedRoles.Admin, FixedRoles.AgencyUser, FixedRoles.Ser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public JsonResult PreviewNewData(Guid id, jQueryDataTableParamModel jq)
		{
			using (var db = new ccEntities())
			{
				CheckImportIdPermissions(id, db);
				var q = queryImportNew(id);
				var filtered = q;


				var sortColName = Request.Form["mDataProp_" + Request.Form["iSortCol_0"]];
				var sortDir = Request.Form["sSortDir_0"] == "asc";
				var sorted = filtered.OrderByField(sortColName, sortDir);

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

		[CcAuthorize(FixedRoles.Admin, FixedRoles.AgencyUser, FixedRoles.Ser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public JsonResult PreviewExistingData(Guid id, jQueryDataTableParamModel jq)
		{
			using (var db = new ccEntities())
			{
				CheckImportIdPermissions(id, db);
				var q = queryImportExisting(id);
				var filtered = q;


				var sortColName = Request.Form["mDataProp_" + Request.Form["iSortCol_0"]];
				var sortDir = Request.Form["sSortDir_0"] == "asc";
				var sorted = filtered.OrderByField(sortColName, sortDir);

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

		[HttpPost]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.AgencyUser, FixedRoles.Ser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult ImportNew(Guid id)
		{
			var q = queryImportNew(id).ToList();
			if (q.Any(f => f.Errors != string.Empty))
			{
				var model = new ImportPreviewModel
				{
					Id = id,
					ImportAction = "New"
				};
				ModelState.AddModelError("", "There are some errors in the file, please fix them first and then import the file again");
				return Preview(model);
			}
			using (var db = new ccEntities())
			{

				CheckImportIdPermissions(id, db);

				db.CommandTimeout = 300;

				var rowCount = db.ImportNewClientsProc(id, this.Permissions.User.AgencyId, this.Permissions.User.AgencyGroupId, this.Permissions.User.RegionId);

				return RedirectToAction("Index", "Clients");
			}

		}

		[HttpPost]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.AgencyUser, FixedRoles.Ser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult ImportExisting(Guid id)
		{
			var q = queryImportExisting(id).ToList();
			if (q.Any(f => f.Errors != string.Empty))
			{
				var model = new ImportPreviewModel
				{
					Id = id,
					ImportAction = "Existing"
				};
				ModelState.AddModelError("", "There are some errors in the file, please fix them first and then import the file again");
				return Preview(model);
			}
			using (var db = new ccEntities())
			{

				CheckImportIdPermissions(id, db);

				db.CommandTimeout = 300;

				var rowCount = db.ImportExistingClientsProc(id, this.Permissions.User.AgencyId, this.Permissions.User.AgencyGroupId, this.Permissions.User.RegionId, this.Permissions.User.Id);

				return RedirectToAction("Index", "Clients");
			}

		}

		#endregion

		#endregion

		#region Exports
		[CcAuthorize(FixedRoles.Admin, FixedRoles.AgencyUser, FixedRoles.Ser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult ExportIds()
		{
			var model = new ExportClientsNewClientIdsModel();
			using (var db = new ccEntities())
			{
				var ags = db.AgencyGroups.Where(this.Permissions.AgencyGroupsFilter)
							.Select(f => new { Id = f.Id, Name = f.DisplayName }).ToList();
				model.AgencyGroups = new SelectList(ags, "Id", "Name", model.AgencyGroupId);
			}
			return View(model);
		}
		[HttpPost]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.AgencyUser, FixedRoles.Ser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult ExportIds(ExportClientsNewClientIdsModel model)
		{
			var userRole = (FixedRoles)this.CcUser.RoleId;
			using (var db = new ccEntities())
			{
				var clients = db.Clients.Where(this.Permissions.ClientsFilter)
					.Where(c => c.CreatedAt > model.CreateDate && c.Agency.GroupId == model.AgencyGroupId);
				var data = acprow.GetExportData(clients, db, Permissions)
					.ToList();
				switch (userRole)
				{
					case FixedRoles.Admin:
					case FixedRoles.GlobalOfficer:
					case FixedRoles.GlobalReadOnly:
					case FixedRoles.AuditorReadOnly:
					case FixedRoles.RegionAssistant:
					case FixedRoles.RegionOfficer:
					case FixedRoles.RegionReadOnly:
					case FixedRoles.BMF:
						return this.Excel("Clients", "List", data);
					case FixedRoles.AgencyUser:
					case FixedRoles.Ser:
					case FixedRoles.AgencyUserAndReviewer:
					case FixedRoles.SerAndReviewer:
					default:
						return this.Excel("Clients", "List", data.Cast<acprowBase>());
				}
			}
		}
		#endregion

		#region IDisposable

		protected override void Dispose(bool disposing)
		{
			_db.Dispose();
			Repo.Dispose();
			base.Dispose(disposing);
		}

		#endregion

		public ActionResult SendDisapprovedClientsNotifications(DateTime? date)
		{
			DateTime fromdate = (date ?? DateTime.Now).Date;
			DateTime todate = fromdate.AddDays(1);
			var stringStatus = ((int)ApprovalStatusEnum.NotEligible).ToString();

			var qq = from h in _db.Histories
					 where h.TableName == "clients" && h.FieldName == "ApprovalStatusId" && h.NewValue == stringStatus
					 group h by h.ReferenceId into hg
					 select new { ClientId = hg.Key, Date = hg.Max(f => f.UpdateDate) };
			var q = from item in
						(
							from c in _db.Clients
							where c.ApprovalStatusId == (int)ApprovalStatusEnum.NotEligible
							join h in qq on c.Id equals h.ClientId
							where h.Date > fromdate && h.Date < todate
							select new { Id = c.Id, Date = h.Date, Agency = c.Agency, FirstName = c.FirstName, LastName = c.LastName }
							)
					group item by item.Agency into cg
					where cg.Any()
					select new DisapprovedClientsEmailModel()
						{
							Agency = cg.Key,
							Clients = cg.Select(f => new DisapprovedClientsEmailModel.ClientEntry() { Id = f.Id, Name = f.FirstName + " " + f.LastName, Date = f.Date }).OrderBy(f => f.Name)
						};

			SendDisApproveClientsNotificationEmail(q);

			return new EmptyResult();

		}

		private void SendDisApproveClientsNotificationEmail(IEnumerable<DisapprovedClientsEmailModel> q)
		{
			using (var client = new SmtpClientWrapper())
			{
				foreach (var item in q)
				{
					var email = new System.Net.Mail.MailMessage();

					CC.Web.Helpers.EmailHelper.AddRecipientsByAgency(email, item.Agency.Id);

					email.IsBodyHtml = true;
					email.Body = this.RenderRazorViewToString("~/Views/Clients/EmailTemplates/DisapprovedClients.cshtml", item);

					try
					{
						client.Send(email);
					}
					catch (SmtpException ex)
					{
						_log.Error(ex);
					}

				}
			}
		}

		private DateTime? getLastReport(int id)
		{
			return ccEntitiesExtensions.LastSubmittedHcRepDate(id);
		}

		private DateTime? getLastMainReport(int id)
		{
			return (from c in db.Clients
					where c.Id == id
					join cr in db.ViewClientReports on c.Id equals cr.ClientId
					join sr in db.SubReports on cr.SubReportId equals sr.Id
					join mr in db.MainReports.Where(MainReport.Submitted) on sr.MainReportId equals mr.Id
					select mr).Max(f => (DateTime?)f.End);
		}

		private DateTime? getFirstMainReport(int id)
		{
			return (from c in db.Clients
					where c.Id == id
					join cr in db.ViewClientReports on c.Id equals cr.ClientId
					join sr in db.SubReports on cr.SubReportId equals sr.Id
					join mr in db.MainReports.Where(MainReport.Submitted) on sr.MainReportId equals mr.Id
					select mr).Min(f => (DateTime?)f.Start);
		}

		private IQueryable<ImportNewDataRow> queryImportNew(Guid id)
		{
			var maxBirthDayString = Client.MaxEligibleBirthDate.ToShortDateString();
			var minBirthDayString = Client.MinEligibleBirthDate.ToShortDateString();
			var minJoinDayString = Client.MinEligibleJoinDate.ToShortDateString();
			var tomorrow = DateTime.Today.AddDays(1);
			var EarliestJoinDate = new DateTime(1946, 2, 9);
            var earliestMaf = new DateTime(2017, 1, 1);
			var earliestMafStr = earliestMaf.ToShortDateString();
            var earliestHAS2 = new DateTime(2019, 12, 31);
            var earliestHAS2Str = earliestHAS2.ToShortDateString();
			return from i in db.ImportClients
				   join import in db.Imports.Where(this.Permissions.ImportsFilter) on i.ImportId equals import.Id
				   where i.ImportId == id
				   join existing in db.Clients.Where(this.Permissions.ClientsFilter) on i.ClientId equals existing.Id into existingGroup
				   from existing in existingGroup.DefaultIfEmpty()
				   join agency in db.Agencies.Where(this.Permissions.AgencyFilter) on i.AgencyId equals agency.Id into agenciesGroup
				   from agency in agenciesGroup.DefaultIfEmpty()
				   join state in db.States on i.StateId equals state.Id into statesGroup
				   from state in statesGroup.DefaultIfEmpty()
				   join nationalIdType in db.NationalIdTypes on i.NationalIdTypeId equals nationalIdType.Id into nitGroup
				   from nationalIdType in nitGroup.DefaultIfEmpty()
				   join approvalStatus in db.ApprovalStatuses on i.ApprovalStatusId equals approvalStatus.Id into approvalStatusesGroup
				   from approvalStatus in approvalStatusesGroup.DefaultIfEmpty()
				   join birthCountry in db.BirthCountries on i.BirthCountryId equals birthCountry.Id into bcg
				   from birthCountry in bcg.DefaultIfEmpty()
				   join country in db.Countries on i.CountryId equals country.Id into cg
				   from country in cg.DefaultIfEmpty()
				   join ghc in db.GovHcHours on i.ClientId equals ghc.ClientId into ghcg
				   join commPref in db.CommunicationsPreferences on i.CommPrefsId equals commPref.Id into commPrefg
				   from commPref in commPrefg.DefaultIfEmpty()
				   join careReceived in db.CareReceivingOptions on i.CareReceivedId equals careReceived.Id into careReceivedg
				   from careReceived in careReceivedg.DefaultIfEmpty()
				   select new ImportNewDataRow
				   {
					   RowIndex = i.RowIndex,
					   ClientId = i.ClientId,
					   InternalId = i.InternalId,
					   AgencyId = i.AgencyId,
					   AgencyName = agency.Name,
					   FirstName = i.FirstName,
					   MiddleName = i.MiddleName,
					   LastName = i.LastName,
					   PrevFirstName = i.PrevFirstName,
					   PrevLastName = i.PrevLastName,
					   Address = i.Address,
					   StateCode = state.Code,
					   BirthDate = i.BirthDate,
					   PobCity = i.PobCity,
					   BirthCountryId = i.BirthCountryId,
					   CountryId = i.CountryId,
					   BirthCountryName = birthCountry.Name,
					   CountryName = country.Name,
					   NationaliIdTypeId = i.NationalIdTypeId,
					   NationalIdTypeName = nationalIdType.Name,
					   NationalId = i.NationalId,
					   LeaveReasonId = i.LeaveReasonId,
					   LeaveDate = i.LeaveDate,
					   DeceasedDate = i.DeceasedDate,
					   JoinDate = i.JoinDate,
					   Gender = i.Gender,
					   HomecareWaitlist = i.HomecareWaitlist ?? false,
                       UnableToSign = i.UnableToSign ?? false,
					   OtherServicesWaitlist = i.OtherServicesWaitlist ?? false,
					   CommPrefs = commPref != null ? commPref.Name : "",
					   CareReceivedVia = careReceived != null ? careReceived.Name : "",
					   MAFDate = i.MAFDate,
					   MAF105Date = i.MAF105Date,
                       HAS2Date = i.HAS2Date,
                       Errors =
						   ((i.FirstName == null || i.FirstName.Trim().Equals(string.Empty)) ? "FirstName is required.  " : "") +
						   ((i.LastName == null || i.LastName.Trim().Equals(string.Empty)) ? "LastName is required.  " : "") +
						   ((this.Permissions.User.RoleId != (int)FixedRoles.Admin && i.FirstName.Equals(i.LastName)) ? "First Name and Last Name can't be the same.  " : "") +
						   ((i.City == null || i.City.Trim().Equals(string.Empty)) ? "City is required.  " : "") +
						   ((i.Address == null || i.Address.Trim().Equals(string.Empty)) ? "Address is required.  " : "") +
						   ((i.AgencyId == null) ? "ORG_ID is required.  " : "") +
						   ((i.BirthDate == null) ? "Birth_Date is required.  " : "") +
						   ((this.Permissions.User.RoleId != (int)FixedRoles.Admin && i.BirthDate < Client.MinEligibleBirthDate) ? "Birth_Date can't be earlier than " + minBirthDayString + ".  " : "") +
						   ((i.BirthDate > Client.MaxEligibleBirthDate) ? "Birth_Date must be less or equal to " + maxBirthDayString + ".  " : "") +
						   ((i.JoinDate == null) ? "Join_Date is required.  " : "") +                        
                           ((this.Permissions.User.RoleId == (int)FixedRoles.Admin && i.JoinDate < EarliestJoinDate) ? "Join_Date can't be earlier than 09 Feb 1946.  " : "") +
                           ((i.LeaveDate < i.JoinDate) ? "Leave_date must be greateer than Join_Date, " : "") +
						   ((i.LeaveDate < i.BirthDate) ? "Leave_date must be greateer than Birth_Date, " : "") +
						   ((i.DeceasedDate < i.LeaveDate) ? "Leave_Date must be less than Deceased_Date, " : "") +
						   ((i.DeceasedDate < i.JoinDate) ? "Deceased_Date must be greater than Join_Date, " : "") +
						   ((i.DeceasedDate < i.BirthDate) ? "Deceased_Date must be greater than Birth_Date, " : "") +
						   ((i.JoinDate >= tomorrow) ? "Join_Date can't be greater then the date of entry, " : "") +						   
						   ((i.MAFDate != null && (i.MAFDate >= tomorrow || i.MAFDate < earliestMaf)) ? "MAF Date can't be future date or early than " + earliestMafStr + ", " : "") +
                           ((i.MAF105Date != null && (i.MAF105Date >= tomorrow || i.MAF105Date < earliestMaf)) ? "MAF 105+ Date can't be future date or early than " + earliestMafStr + ", " : "") +
                           ((i.HAS2Date != null && (i.HAS2Date >= tomorrow || i.HAS2Date < earliestHAS2)) ? "HAS2Date can't be future date or early than " + earliestHAS2Str + ", " : "") + 
                           ((this.Permissions.User.RoleId != (int)FixedRoles.Admin && i.JoinDate < Client.MinEligibleJoinDate) ? "Join_Date can't be earlier than " + minJoinDayString + ".  " : "") +
						   ((i.LeaveDate != null && i.LeaveReasonId == null) ? "Leave_Reason has to be specified if Leave_Date is not empty, " : "") +
						   ((i.LeaveReasonId != null && i.LeaveDate == null) ? "Leave_Date is required if Leave_Reason_ID is not empty, " : "") +
						   ((i.LeaveReasonId != null && i.LeaveReasonId == (int)LeaveReasonEnum.Deceased && (i.DeceasedDate == null || i.LeaveDate == null)) ? "Deceased_Date or Leave_Date is required if Leave_reason_id is 2 (Deceased, " : "") +
						   ((i.LeaveReasonId != null && i.LeaveReasonId == (int)LeaveReasonEnum.NotEligible && this.Permissions.User.RoleId != (int)FixedRoles.Admin) ? "You are not allowed to change Leave_Reason to Not Eligible, " : "") +
						   ((i.StateId == null && country.States.Any()) ? "State is required.  " : "") +
						   ((state != null && state.CountryId != country.Id) ? "Invalid State, " : "") +
						   ((i.NationalIdTypeId != null && (i.NationalId == null || i.NationalId == "")) ? "Government Issued ID is required when ID Type is selected, " : "") +
						   ((i.NationalIdTypeId == null && i.NationalId != null && i.NationalId != "") ? "ID Type is required when Government Issued ID is not empty, " : "") +
						   (
							   (agency.AgencyGroup.ForceIsraelID || (i.NationalIdTypeId ?? existing.NationalIdTypeId) == 1) ?
								   i.NationalId == null ? "National Id is required." :
								   i.NationalId.Length < 9 ? "National Id must be 9 digits long" :
								   ccEntitiesExtensions.tz_check(i.NationalId) != true ? "Invalid IsraelID" :
								   db.Clients.Any(f => f.AgencyId == i.AgencyId && f.NationalId == i.NationalId) || db.ImportClients.Any(f => f.RowIndex != i.RowIndex && f.NationalId == i.NationalId && f.AgencyId == i.AgencyId && f.ImportId == i.ImportId) ? "Duplicate IsraelID, " :
								   string.Empty :
							   string.Empty) +
						   ((i.NationalIdTypeId == 0 && i.NationalId.Length > 4) ? "Government Issued ID can be up to 4 digits long, " : "") +
						   ((i.NationalIdTypeId != null && nationalIdType == null) ? "Invalid GOV_ID_TYP, " : "") +

						   (agency == null ? "Invalid Agency, " : "") +
						   (state == null && i.StateId != null ? "Invalid StateID, " : "") +
						   (birthCountry == null ? "Birth Country is required, " : "") +
						   (country == null ? "Country is required, " : "") +
						   (i.Gender == null ? "Gender is required, " : "")
				   };
		}

		private IQueryable<ImportNewDataRow> queryImportExisting(Guid id)
		{
			var maxBdayStr = Client.MaxEligibleBirthDate.ToShortDateString();
			var minBirthDayString = Client.MinEligibleBirthDate.ToShortDateString();
			var now = DateTime.Now;
			var tomorrow = DateTime.Today.AddDays(1);
			var isAdmin = this.User.IsInRole(FixedRoles.Admin);
			var isGO = this.User.IsInRole(FixedRoles.GlobalOfficer);
			var EarliestJoinDate = new DateTime(1946, 2, 9);
            var earliestMaf = new DateTime(2017, 1, 1);
			var earliestMafStr = earliestMaf.ToShortDateString();
            var earliestHAS2 = new DateTime(2019, 12, 31);
            var earliestHAS2Str = earliestHAS2.ToShortDateString();
			return from i in db.ImportClients
				   join import in db.Imports.Where(this.Permissions.ImportsFilter) on i.ImportId equals import.Id
				   where i.ImportId == id
				   join existing in db.Clients.Where(this.Permissions.ClientsFilter) on i.ClientId equals existing.Id into existingGroup
				   from existing in existingGroup.DefaultIfEmpty()
				   join newAgency in db.Agencies.Where(this.Permissions.AgencyFilter) on i.AgencyId equals newAgency.Id into newAgenciesGroup
				   from newAgency in newAgenciesGroup.DefaultIfEmpty()
				   join oldAgency in db.Agencies.Where(this.Permissions.AgencyFilter) on existing.AgencyId equals oldAgency.Id into oldAgenciesGroup
				   from oldAgency in oldAgenciesGroup.DefaultIfEmpty()
				   join state in db.States on (i.StateId ?? existing.StateId) equals state.Id into statesGroup
				   from state in statesGroup.DefaultIfEmpty()
				   join nationalIdType in db.NationalIdTypes on (i.NationalIdTypeId ?? existing.NationalIdTypeId) equals nationalIdType.Id into nitGroup
				   from nationalIdType in nitGroup.DefaultIfEmpty()
				   join approvalStatus in db.ApprovalStatuses on (i.ApprovalStatusId ?? existing.ApprovalStatusId) equals approvalStatus.Id into approvalStatusesGroup
				   from approvalStatus in approvalStatusesGroup.DefaultIfEmpty()
				   let firstSubmittedRep = (from c in db.Clients
											where c.Id == i.ClientId
											join cr in db.ViewClientReports on c.Id equals cr.ClientId
											join sr in db.SubReports on cr.SubReportId equals sr.Id
											join mr in db.MainReports.Where(MainReport.Submitted) on sr.MainReportId equals mr.Id
											select mr).Min(f => (DateTime?)f.Start)
				   let lastSubmittedRep = (from c in db.Clients
										   where c.Id == i.ClientId
										   join cr in db.ViewClientReports on c.Id equals cr.ClientId
										   join sr in db.SubReports on cr.SubReportId equals sr.Id
										   join mr in db.MainReports.Where(MainReport.Submitted) on sr.MainReportId equals mr.Id
										   select mr).Max(f => (DateTime?)System.Data.Objects.EntityFunctions.AddMonths(f.Start, 1))
				   let leaveDateExt = ((i.DeceasedDate ?? existing.DeceasedDate) != null || (i.LeaveReasonId ?? existing.LeaveReasonId) == (int)LeaveReasonEnum.Deceased) ?
					   System.Data.Objects.EntityFunctions.AddDays(i.LeaveDate, SubReport.DeceasedDaysOverhead) :
					   (i.LeaveDate ?? existing.LeaveDate)
				   join bc in db.BirthCountries on (i.BirthCountryId ?? existing.BirthCountryId) equals bc.Id into bcg
				   from bc in bcg.DefaultIfEmpty()
				   join co in db.Countries on (i.CountryId ?? existing.CountryId) equals co.Id into cog
				   from co in cog.DefaultIfEmpty()
				   join commPref in db.CommunicationsPreferences on (i.CommPrefsId ?? existing.CommPrefsId) equals commPref.Id into commPrefg
				   from commPref in commPrefg.DefaultIfEmpty()
				   join careReceived in db.CareReceivingOptions on (i.CareReceivedId ?? existing.CareReceivedId) equals careReceived.Id into careReceivedg
				   from careReceived in careReceivedg.DefaultIfEmpty()
				   select new ImportNewDataRow
				   {
					   RowIndex = i.RowIndex,
					   ClientId = i.ClientId,
					   InternalId = i.InternalId,
					   AgencyId = i.AgencyId,
					   AgencyName = newAgency.Name,
					   FirstName = (i.FirstName ?? existing.FirstName),
					   MiddleName = (i.MiddleName ?? existing.MiddleName),
					   LastName = (i.LastName ?? existing.LastName),
					   PrevFirstName = (i.PrevFirstName ?? existing.PrevFirstName),
					   PrevLastName = (i.PrevLastName ?? existing.PrevLastName),
					   Address = (i.Address ?? existing.Address),
					   StateCode = state.Code,
					   BirthDate = (DateTime?)(i.BirthDate ?? existing.BirthDate),
					   PobCity = (i.PobCity ?? existing.PobCity),
					   BirthCountryName = bc.Name,
					   CountryName = co.Name,
					   NationaliIdTypeId = (i.NationalIdTypeId ?? existing.NationalIdTypeId),
					   NationalIdTypeName = nationalIdType.Name,
					   NationalId = (i.NationalId ?? existing.NationalId),
					   LeaveReasonId = (i.LeaveReasonId ?? existing.LeaveReasonId),
					   LeaveDate = (DateTime?)(i.LeaveDate ?? existing.LeaveDate),
					   DeceasedDate = (DateTime?)(i.DeceasedDate ?? existing.DeceasedDate),
					   JoinDate = (DateTime?)(i.JoinDate ?? existing.JoinDate),
					   Gender = i.Gender,
					   HomecareWaitlist = i.HomecareWaitlist ?? (bool?)existing.HomecareWaitlist,
                       UnableToSign = i.UnableToSign ?? (bool?)existing.UnableToSign,
                       OtherServicesWaitlist = i.OtherServicesWaitlist ?? (bool?)existing.OtherServicesWaitlist,
					   CommPrefs = commPref != null ? commPref.Name : "",
					   CareReceivedVia = careReceived != null ? careReceived.Name : "",
					   MAFDate = i.MAFDate ?? existing.MAFDate,
					   MAF105Date = i.MAF105Date ?? existing.MAF105Date,
                       HAS2Date = i.HAS2Date ?? existing.HAS2Date,
					   Errors =
						   ((i.ClientId == null) ? "Client ID is required. " : "") +
						   ((this.Permissions.User.RoleId != (int)FixedRoles.Admin && (i.FirstName ?? existing.FirstName).Equals(i.LastName ?? existing.LastName)) ? "First Name and Last Name can't be the same.  " : "") +
						   ((existing == null) ? "Client not found. " : "") +
						   ((newAgency.Id == null && oldAgency.Id == null) ? "Agency not found. " : "") +
						   (
						   (!this.Permissions.CanChangeApprovedClientName &&
							   existing.ApprovalStatusId == (int)ApprovalStatusEnum.Approved &&
							   ((i.FirstName ?? existing.FirstName) != existing.FirstName || (i.LastName ?? existing.LastName) != existing.LastName)
						   ) ? "First Name / Last Name cannot be revised for Approved clients" : "") +
						   ((this.Permissions.User.RoleId != (int)FixedRoles.Admin && (i.ApprovalStatusId ?? existing.ApprovalStatusId) != (int)ApprovalStatusEnum.New && i.BirthDate != null) ? "You are not allowed to change Birth_Date. " : "") +
						   ((this.Permissions.User.RoleId != (int)FixedRoles.Admin && (i.ApprovalStatusId ?? existing.ApprovalStatusId) == (int)ApprovalStatusEnum.New &&
								i.BirthDate < Client.MinEligibleBirthDate) ? "Birth_Date can't be earlier than " + minBirthDayString + ".  " : "") +
						   (((i.BirthDate ?? existing.BirthDate) > Client.MaxEligibleBirthDate) ? "Birth_Date must be less or equal to " + maxBdayStr + ". " : "") +
						   (((i.DeceasedDate ?? existing.DeceasedDate) != null && (i.LeaveDate ?? existing.LeaveDate) == null) ? "Leave Date is required if Deceased Date is specified, " : "") +
						   (((i.DeceasedDate ?? existing.DeceasedDate) != null && (i.LeaveReasonId ?? existing.LeaveReasonId) == null) ? "Leave Reason is required if Deceased Date is specified, " : "") +
						   (((i.LeaveDate ?? existing.LeaveDate) < (i.JoinDate ?? existing.JoinDate)) ? "Leave_date must be greateer than Join_Date, " : "") +
						   (((i.LeaveDate ?? existing.LeaveDate) < (i.BirthDate ?? existing.BirthDate)) ? "Leave_date must be greater than Birth_Date, " : "") +
						   (((i.DeceasedDate ?? existing.DeceasedDate) < (i.LeaveDate ?? existing.LeaveDate)) ? "Leave_Date must be less than Deceased_Date, " : "") +
						   (((i.DeceasedDate ?? existing.DeceasedDate) < (i.JoinDate ?? existing.JoinDate)) ? "Deceased_Date must be greater than Join_Date, " : "") +
						   (((i.DeceasedDate ?? existing.DeceasedDate) < (i.BirthDate ?? existing.BirthDate)) ? "Deceased_Date must be greater than Birth_Date, " : "") +
						   (((i.JoinDate ?? existing.JoinDate) >= tomorrow) ? "Join_Date can't be greater then the date of entry, " : "") +
						   (((i.JoinDate ?? existing.JoinDate) < EarliestJoinDate) ? "Join_Date can't be earlier than 09 Feb 1946, " : "") +						   
						   (((i.MAFDate ?? existing.MAFDate) != null && ((i.MAFDate ?? existing.MAFDate) >= tomorrow || (i.MAFDate ?? existing.MAFDate) < earliestMaf)) ? "MAF Date can't be future date or early than " + earliestMafStr + ", " : "") +
                           (((i.MAF105Date ?? existing.MAF105Date) != null && ((i.MAF105Date ?? existing.MAF105Date) >= tomorrow || (i.MAF105Date ?? existing.MAF105Date) < earliestMaf)) ? "MAF 105+ Date can't be future date or early than " + earliestMafStr + ", " : "") +
                           (((i.HAS2Date ?? existing.HAS2Date) != null && ((i.HAS2Date ?? existing.HAS2Date) >= tomorrow || (i.HAS2Date ?? existing.HAS2Date) < earliestHAS2)) ? "HAS2 Date can't be future date or early than " + earliestHAS2Str + ", " : "") +                     
                           ((this.Permissions.User.RoleId != (int)FixedRoles.Admin && i.JoinDate != null && existing.JoinDate != null && i.JoinDate != existing.JoinDate) ? "You are not allowed to change the Join_Date. " : "") +
						   (((i.LeaveDate ?? existing.LeaveDate) != null && (i.LeaveReasonId ?? existing.LeaveReasonId) == null) ? "Leave_Reason has to be specified if Leave_Date is not empty, " : "") +
						   ((i.LeaveDate != null && i.LeaveDate > now && !isAdmin && !isGO) ? "Leave Date can't be greater then the date of entry, " : "") +
						   ((i.DeceasedDate != null && i.DeceasedDate > now) ? "Deceased Date can't be greater then the date of entry, " : "") +
						   (((i.LeaveReasonId ?? existing.LeaveReasonId) != null && (i.LeaveDate ?? existing.LeaveDate) == null) ? "Leave_Date is required if Leave_Reason_ID is not empty, " : "") +
						   (((i.LeaveReasonId ?? existing.LeaveReasonId) == (int)LeaveReasonEnum.Deceased && (i.DeceasedDate ?? existing.DeceasedDate) == null) ? "Deceased_Date or Leave_Date is required if Leave_reason_id is 2 (Deceased, " : "") +
						   (((i.LeaveReasonId ?? existing.LeaveReasonId) != null && (i.LeaveReasonId ?? existing.LeaveReasonId) == (int)LeaveReasonEnum.NotEligible && this.Permissions.User.RoleId != (int)FixedRoles.Admin) ? "You are not allowed to change Leave_Reason to Not Eligible, " : "") +
						   (((i.NationalIdTypeId ?? existing.NationalIdTypeId) != null && nationalIdType == null) ? "Invalid GOV_ID_TYP, " : "") +
						   ((state.CountryId != null && state.CountryId != newAgency.AgencyGroup.CountryId) ? "Invalid State, " : "") +
						   ((i.BirthCountryId != null && bc == null) ? "Invalid Birth Country field value." : "") +
						   ((i.CountryId != null && co == null) ? "Invalid Country field value." : "") +
						   ((newAgency.AgencyGroup.Country.States.Any() && state.Id == null) ? "State is required, " : "") +
						   (((i.NationalIdTypeId ?? existing.NationalIdTypeId) != null && ((i.NationalId ?? existing.NationalId) == null || (i.NationalId ?? existing.NationalId) == "")) ? "Government Issued ID is required when ID Type is selected, " : "") +
						   (((i.NationalIdTypeId ?? existing.NationalIdTypeId) == null && (i.NationalId ?? existing.NationalId) != null && (i.NationalId ?? existing.NationalId) != "") ? "ID Type is required when Government Issued ID is not empty, " : "") +
						   (
							   (newAgency.AgencyGroup.ForceIsraelID || ((i.NationalIdTypeId ?? existing.NationalIdTypeId) ?? existing.NationalIdTypeId) == 1) ?
								   (i.NationalId ?? existing.NationalId) == null ? "National Id is required," : /* enforce israelid constraints: one is for israel id type */
								   (i.NationalId ?? existing.NationalId).Length < 9 ? "National Id must be 9 digits long" :
								   ccEntitiesExtensions.tz_check((i.NationalId ?? existing.NationalId)) != true ? "Invalid IsraelID," :
								   db.Clients.Any(c => c.AgencyId == (i.AgencyId ?? existing.AgencyId) && c.NationalId == (i.NationalId ?? existing.NationalId) && c.Id != (i.ClientId)) || db.ImportClients.Any(f => f.RowIndex != i.RowIndex && f.NationalId == i.NationalId && f.AgencyId == (i.AgencyId ?? existing.AgencyId)) ? "Duplicate IsraelID,"
								   : string.Empty
							   : string.Empty
						   ) +
						   (((i.NationalIdTypeId ?? existing.NationalIdTypeId) == 0 && (i.NationalId ?? existing.NationalId).Length > 4) ? "Government Issued ID can be up to 4 digits long, " : "") +
						   (state == null && (i.StateId ?? existing.StateId) != null ? "Invalid StateId, " : "") +
						   (
							   this.Permissions.User.RoleId != (int)FixedRoles.Admin ?
							   (

								   (
										i.JoinDate != null && i.JoinDate > firstSubmittedRep ?
										   "Join Date update is not allowed since it is within an already submitted Financial Report period," :
										   ""
								   ) +
								   (
									   (i.LeaveDate != null && leaveDateExt < lastSubmittedRep) ?
										   "Leave Date update is not allowed since it is within an already submitted Financial Report period," :
										   ""
								   )
							   ) : ""
						   )
				   };
		}

		private class ImportNewDataRow
		{
			public int RowIndex { get; set; }
			public int? ClientId { get; set; }
			public string InternalId { get; set; }
			public int? AgencyId { get; set; }
			public string AgencyName { get; set; }
			public string FirstName { get; set; }
			public string MiddleName { get; set; }
			public string LastName { get; set; }
			public string PrevFirstName { get; set; }
			public string PrevLastName { get; set; }
			public string Address { get; set; }
			public string StateCode { get; set; }
			public DateTime? BirthDate { get; set; }
			public string PobCity { get; set; }
			public int? BirthCountryId { get; set; }
			public int? CountryId { get; set; }
			public string BirthCountryName { get; set; }
			public string CountryName { get; set; }
			public int? NationaliIdTypeId { get; set; }
			public string NationalIdTypeName { get; set; }
			public string NationalId { get; set; }
			public int? LeaveReasonId { get; set; }
			public DateTime? LeaveDate { get; set; }
			public DateTime? DeceasedDate { get; set; }
			public DateTime? JoinDate { get; set; }
			public int? Gender { get; set; }
			public bool? HomecareWaitlist { get; set; }
            public bool? UnableToSign { get; set; }
            public bool? OtherServicesWaitlist { get; set; }
			public string CommPrefs { get; set; }
			public string CareReceivedVia { get; set; }
			public string Errors { get; set; }
			public DateTime? MAFDate { get; set; }
			public DateTime? MAF105Date { get; set; }
            public DateTime? HAS2Date { get; set; }
		}

		#region export csv for changed approval status
		public static void ExportCsvForChangedApprovalStatus(IEnumerable<ImportClientFundStatusProc_Result> clients)
		{
			var splitedDate = DateTime.Now.ToShortDateString().Replace('/', '_');
			var splitedTime = DateTime.Now.ToLongTimeString().Replace(':', '_');
			string filePath = HostingEnvironment.MapPath("~/Files-CFS/ChangedApprovalStatus/");
			filePath = filePath + splitedDate + "_" + splitedTime + "_changed.csv";
			using (var csvWriter = new CsvHelper.CsvWriter(new StreamWriter(filePath)))
			{
				csvWriter.WriteField("CC ID");
				csvWriter.WriteField("Israeli ID");
				csvWriter.WriteField("JNV Status");
				csvWriter.NextRecord();
				foreach (var c in clients)
				{
					csvWriter.WriteField(c.ClientId);
					csvWriter.WriteField(c.NationalId);
					csvWriter.WriteField(c.NewApprovalStatusId);
					csvWriter.NextRecord();
				}
			}
		}
		#endregion

		#region scan cfs files
		public void ScanAndImportCfsFiles()
		{
			using (var db = new ccEntities())
			{
				try
				{
					_log.Debug("ScanAndImportCfsFiles started");
					string importEmails = ConfigurationManager.AppSettings["ImportCfsEmail"];
					var email = new System.Net.Mail.MailMessage();
					if (!string.IsNullOrEmpty(importEmails))
					{
						var emails = importEmails.Split(',');
						foreach (var i in emails)
						{
							email.To.Add(i);
						}
					}
					else
					{
						email.To.Add("support@prog4biz.com");
					}
					string[] newFileList = Directory.GetFiles(HostingEnvironment.MapPath("~/Files-CFS/NewClients/"), "*.csv");
					foreach (var file in newFileList)
					{
						string fileName = getFileNameByPath(file);
						_log.Debug(string.Format("ScanAndImportCfsFiles new clients file: {0}", fileName));
						var model = new NewClientsImportModel();
						model.Permissions = CC.Data.Services.PermissionsFactory.GetPermissionsFor("sysadmin");
						this.Permissions = model.Permissions;
						model.File = new MyHttpPostedFile(file, fileName);

						var previewModel = new ImportPreviewModel
						{
							Id = model.Id,
							ImportAction = "New"
						};

						var splitedDate = DateTime.Now.ToShortDateString().Replace('/', '_');
						var splitedTime = DateTime.Now.ToLongTimeString().Replace(':', '_');
						try
						{
							model.ProcessFile(true);
							_log.Debug("ScanAndImportCfsFiles new clients file processed");
							CheckImportIdPermissions(previewModel.Id, db);
							var res = queryImportNew(previewModel.Id).ToList();
							if (res.Any(f => f.Errors != ""))
							{
								foreach (var e in res.Where(f => f.Errors != ""))
								{
									_log.Error(string.Format("ScanAndImportCfsFiles new clients error: {0}", e.Errors));
								}
								//file has errors, reject it
								throw new Exception("File has errors");
							}
							db.CommandTimeout = 300;
							var clients = db.ImportNewClientsProc(previewModel.Id, null, null, null);
							_log.Debug("ScanAndImportCfsFiles new clients import proc finished");
							email.Body = fileName + " was imported OK in Diamond";
							string newFilePath = file.Substring(0, file.IndexOf(fileName)) + "Export\\" + splitedDate + "_" + splitedTime + "_" + fileName;
							using (var csvWriter = new CsvHelper.CsvWriter(new StreamWriter(newFilePath)))
							{
								csvWriter.WriteField("CC ID");
								csvWriter.WriteField("Israeli ID");
								csvWriter.NextRecord();
								foreach (var c in clients)
								{
									var id = c.id != null ? c.id.ToString() : "";
									csvWriter.WriteField(id);
									csvWriter.WriteField(c.NationalId);
									csvWriter.NextRecord();
								}
							}
							_log.Debug("ScanAndImportCfsFiles new clients csvWriter finished");
						}
						catch (Exception e)
						{
							_log.Error(e);
							//File has import error, reject it                            
							string newFilePath = file.Substring(0, file.IndexOf(fileName)) + "Rejected\\" + splitedDate + "_" + splitedTime + "_" + fileName;
							model.File.SaveAs(newFilePath);
							email.Body = fileName + " was rejected by the auto daily import in Diamond";
						}
						model.File = null;
						System.IO.File.Delete(file);
						using (var client = new SmtpClientWrapper())
						{
							try
							{
								client.Send(email);
							}
							catch (SmtpException ex)
							{
								_log.Error(ex);
							}
						}
					}
					string[] updateFileList = Directory.GetFiles(HostingEnvironment.MapPath("~/Files-CFS/UpdateClients/"), "*.csv");
					foreach (var file in updateFileList)
					{
						string fileName = getFileNameByPath(file);
						_log.Debug(string.Format("ScanAndImportCfsFiles update clients file: {0}", fileName));
						var model = new ExistingClientsImportModel();
						model.Permissions = CC.Data.Services.PermissionsFactory.GetPermissionsFor("sysadmin");
						this.Permissions = model.Permissions;
						model.File = new MyHttpPostedFile(file, fileName);
						model.ProcessFile(false);
						_log.Debug("ScanAndImportCfsFiles update clients file processed");
						var previewModel = new ImportPreviewModel
						{
							Id = model.Id,
							ImportAction = "Existing"
						};

						var splitedDate = DateTime.Now.ToShortDateString().Replace('/', '_');
						var splitedTime = DateTime.Now.ToLongTimeString().Replace(':', '_');
						try
						{
							CheckImportIdPermissions(previewModel.Id, db);
							var res = queryImportExisting(previewModel.Id).ToList();
							if (res.Any(f => f.Errors != ""))
							{
								foreach (var e in res.Where(f => f.Errors != ""))
								{
									_log.Error(string.Format("ScanAndImportCfsFiles update clients error: {0}", e.Errors));
								}
								//file has errors, reject it
								throw new Exception("File has errors");
							}
							db.CommandTimeout = 300;
							var clients = db.ImportExistingClientsProc(previewModel.Id, null, null, null, model.Permissions.User.Id);
							_log.Debug("ScanAndImportCfsFiles update clients import proc finished");
							email.Body = fileName + " was imported OK in Diamond";
							string newFilePath = file.Substring(0, file.IndexOf(fileName)) + "Export\\" + splitedDate + "_" + splitedTime + "_" + fileName;
							using (var csvWriter = new CsvHelper.CsvWriter(new StreamWriter(newFilePath)))
							{
								csvWriter.WriteField("CC ID");
								csvWriter.WriteField("Israeli ID");
								csvWriter.NextRecord();
								foreach (var c in clients)
								{
									var id = c.id != null ? c.id.ToString() : "";
									csvWriter.WriteField(id);
									csvWriter.WriteField(c.NationalId);
									csvWriter.NextRecord();
								}
							}
							_log.Debug("ScanAndImportCfsFiles update clients csvWriter finished");
						}
						catch (Exception e)
						{
							_log.Error(e);
							//File has import error, reject it                            
							string newFilePath = file.Substring(0, file.IndexOf(fileName)) + "Rejected\\" + splitedDate + "_" + splitedTime + "_" + fileName;
							model.File.SaveAs(newFilePath);
							email.Body = fileName + " was rejected by the auto daily import in Diamond";
						}
						model.File = null;
						System.IO.File.Delete(file);
						using (var client = new SmtpClientWrapper())
						{
							try
							{
								client.Send(email);
							}
							catch (SmtpException ex)
							{
								_log.Error(ex);
							}
						}
					}
					db.Globals.AddObject(new Global() { Date = DateTime.Now, Message = "CFS auto import/upload finished" });
					db.SaveChanges();
				}
				catch (DirectoryNotFoundException ex)
				{
					_log.Error(ex);
				}
				catch (Exception ex)
				{
					_log.Error(ex);
				}
			}
		}

		private class MyHttpPostedFile : HttpPostedFileBase
		{
			private readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(ClientsController));
			private string filePath;
			private Stream fs;
			private string fileName;
			public MyHttpPostedFile(string _filePath, string _fileName)
			{
				filePath = _filePath;
				fileName = _fileName;
				try
				{
					fs = System.IO.File.OpenRead(filePath);
				}
				catch (Exception ex)
				{
					_log.Error(ex);
				}
			}

			public override int ContentLength
			{
				get
				{
					return (int)fs.Length;
				}
			}

			public override string ContentType
			{
				get
				{
					return "csv";
				}
			}

			public override string FileName
			{
				get
				{
					return fileName;
				}
			}

			public override Stream InputStream
			{
				get
				{
					return fs;
				}
			}

			public override void SaveAs(string filename)
			{
				using (var file = System.IO.File.Open(filename, FileMode.CreateNew))
				{
					try
					{
						fs = System.IO.File.OpenRead(filePath);
						fs.CopyTo(file);
						fs.Dispose();
					}
					catch (Exception ex)
					{
						_log.Error(ex);
					}
				}
			}
		}

		private string getFileNameByPath(string filePath)
		{
			var split = filePath.Split('\\');
			return split[split.Count() - 1];
		}
		#endregion

		#region export cfs client records
		public void ExportCfsClientRecords(bool isAutomated)
		{
			_log.Debug("ExportCfsClientRecords started");
			var today = DateTime.Today;
			var source = (from cfs in db.CfsRows
						  where cfs.StartDate != null && cfs.EndDate == null
						  let has = cfs.Client.ClientHcStatuses.Where(f => f.StartDate <= today).OrderByDescending(f => f.StartDate).FirstOrDefault()
						  let functionality = cfs.Client.FunctionalityScores.OrderByDescending(f => f.StartDate).FirstOrDefault()
						  let lastHcDate = (from cr in cfs.Client.ClientReports
											join sr in db.SubReports on cr.SubReportId equals sr.Id
											join mr in db.MainReports.Where(MainReport.Submitted) on sr.MainReportId equals mr.Id
											where sr.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.Homecare
														|| sr.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.HomecareWeekly
											from car in cr.ClientAmountReports
											select car.ReportDate)
						  select new
						  {
							  ClientId = cfs.ClientId,
							  FirstName = cfs.Client.FirstName,
							  LastName = cfs.Client.LastName,
							  AgencyGroupId = cfs.Client.Agency.GroupId,
							  AgencyGroupName = cfs.Client.Agency.AgencyGroup.Name,
							  Address = cfs.Client.Address,
							  City = cfs.Client.City,
							  Country = cfs.Client.Country.Name,
							  State = cfs.Client.Country.States.Any() ? cfs.Client.Country.States.FirstOrDefault().Name : "",
							  ZIP = cfs.Client.ZIP,
							  OtherAddress = cfs.Client.OtherAddress,
							  Phone = cfs.Client.Phone,
							  HAS = has != null ? has.HcStatus.Name : "",
							  DiagnosticScore = functionality != null ? functionality.DiagnosticScore : (decimal?)null,
							  FunctionalityLevel = functionality != null ? functionality.FunctionalityLevel.Name : "",
							  FunctionalityStartDate = functionality != null ? functionality.StartDate : (DateTime?)null,
							  Maf105 = cfs.Client.MAF105Date != null ? "Yes" : "No",
                              Has2 = cfs.Client.HAS2Date != null ? "Yes" : "No",
							  HcDate = lastHcDate.Count() > 0 ? lastHcDate.Max() : (DateTime?)null
						  }).ToList();

			var q = from c in source
					select new ExportCfsRecordRow
					{
						ClientId = c.ClientId,
						FirstName = c.FirstName,
						LastName = c.LastName,
						AgencyGroupId = c.AgencyGroupId,
						AgencyGroupName = c.AgencyGroupName,
						Address = c.Address,
						City = c.City,
						Country = c.Country,
						State = c.State,
						ZIP = c.ZIP,
						OtherAddress = c.OtherAddress,
						Phone = c.Phone,
						HAS = c.HAS,
						DiagnosticScore = c.DiagnosticScore,
						FunctionalityLevel = c.FunctionalityLevel,
						FunctionalityStartDate = c.FunctionalityStartDate.HasValue ? c.FunctionalityStartDate.Value.ToString("dd MMM yyyy") : "",
						Maf105 = c.Maf105,
                        Has2 = c.Has2,
						HcDate = c.HcDate.HasValue ? c.HcDate.Value.ToString("dd MMM yyyy") : ""
					};

			try
			{
				var csvConf = new CsvHelper.Configuration.CsvConfiguration()
				{
					IsStrictMode = false,
					IsCaseSensitive = false,
					SkipEmptyRecords = true
				};

				csvConf.ClassMapping<ExportCfsRecordCsvMap>();

				var folderName = ConfigurationManager.AppSettings["ExportCfsRecordsFolderName"];
				if (string.IsNullOrEmpty(folderName))
				{
					folderName = "CfsRecords";
				}
				var splitedDate = DateTime.Now.ToString("MMddyyyy");
				var splitedTime = DateTime.Now.ToLongTimeString().Replace(':', '_');
				string dirPath = HostingEnvironment.MapPath("~/Files-CFS/" + folderName + "/");
				if (!Directory.Exists(dirPath))
				{
					Directory.CreateDirectory(dirPath);
				}
				var filePath = dirPath + "CFS_Export_" + splitedDate + "_" + splitedTime + ".csv";
				using (var csvWriter = new CsvHelper.CsvWriter(new StreamWriter(filePath), csvConf))
				{
					csvWriter.WriteRecords(q.ToList());
				}
				if (isAutomated)
				{
					GlobalDbSettings.Set(GlobalDbSettings.GlobalStringNames.ExportCfsClientRecordsDateTime, DateTime.Now.ToString("M/d/yyyy h:mm:ss tt"));
				}
			}
			catch (Exception ex)
			{
				_log.Error(ex);
				throw ex;
			}
			_log.Debug("ExportCfsClientRecords finished");
		}

		public class ExportCfsRecordRow
		{
			[Display(Name = "CC ID")]
			public int ClientId { get; set; }
			[Display(Name = "First Name")]
			public string FirstName { get; set; }
			[Display(Name = "Last Name")]
			public string LastName { get; set; }
			[Display(Name = "SER ID")]
			public int AgencyGroupId { get; set; }
			[Display(Name = "SER Name")]
			public string AgencyGroupName { get; set; }
			[Display(Name = "Address")]
			public string Address { get; set; }
			[Display(Name = "City")]
			public string City { get; set; }
			[Display(Name = "Country of Residence")]
			public string Country { get; set; }
			[Display(Name = "State")]
			public string State { get; set; }
			[Display(Name = "ZIP")]
			public string ZIP { get; set; }
			[Display(Name = "Other Address")]
			public string OtherAddress { get; set; }
			[Display(Name = "Phone")]
			public string Phone { get; set; }
			[Display(Name = "HAS")]
			public string HAS { get; set; }
			[Display(Name = "Diagnostic score")]
			public decimal? DiagnosticScore { get; set; }
			[Display(Name = "Functionality Level")]
			public string FunctionalityLevel { get; set; }
			[Display(Name = "Functionality Start Date")]
			public string FunctionalityStartDate { get; set; }
			[Display(Name = "MAF 105+")]
			public string Maf105 { get; set; }
            [Display(Name = "HAS2")]
            public string Has2 { get; set; }
			[Display(Name = "Last Homecare provided date")]
			public string HcDate { get; set; }
		}

		internal class ExportCfsRecordCsvMap : CsvHelper.Configuration.CsvClassMap<ExportCfsRecordRow>
		{
			public ExportCfsRecordCsvMap()
			{
				Map(f => f.ClientId).Name("CC ID");
				Map(f => f.FirstName).Name("First Name");
				Map(f => f.LastName).Name("Last Name");
				Map(f => f.AgencyGroupId).Name("SER ID");
				Map(f => f.AgencyGroupName).Name("SER Name");
				Map(f => f.Address).Name("Address");
				Map(f => f.City).Name("City");
				Map(f => f.Country).Name("Country");
				Map(f => f.State).Name("State");
				Map(f => f.ZIP).Name("ZIP");
				Map(f => f.OtherAddress).Name("Other Address");
				Map(f => f.Phone).Name("Phone");
				Map(f => f.HAS).Name("HAS");
				Map(f => f.DiagnosticScore).Name("Diagnostic score");
				Map(f => f.FunctionalityLevel).Name("Functionality Level");
				Map(f => f.FunctionalityStartDate).Name("Functionality Start Date");
				Map(f => f.Maf105).Name("MAF 105+");
                Map(f => f.Has2).Name("HAS2");
				Map(f => f.HcDate).Name("Last Homecare provided date");
			}
		}
		#endregion
	}
}