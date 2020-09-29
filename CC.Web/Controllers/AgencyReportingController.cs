using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CC.Data;

namespace CC.Web.Controllers
{
	[CcAuthorize(FixedRoles.Admin
		,FixedRoles.AgencyUser
		, FixedRoles.AgencyUserAndReviewer
		,FixedRoles.AuditorReadOnly
		,FixedRoles.BMF
		,FixedRoles.GlobalOfficer
		,FixedRoles.GlobalReadOnly
		,FixedRoles.RegionAssistant
		,FixedRoles.RegionOfficer
		,FixedRoles.RegionReadOnly
		,FixedRoles.Ser
		,FixedRoles.SerAndReviewer)]
	public class AgencyReportingController : CcControllerBase
	{
		//
		// GET: /AgencyReporting/

		public ActionResult Index()
		{
			return View();
		}

        public ActionResult FinancialReportRecap()
        {
            return View();
        }

        public ActionResult BudgetRecap()
        {
            return View();
        }

		public ActionResult ReportedHcClients()
		{
			return View();
		}
		
		public ActionResult HomecareDetails()
		{
			var model = new CC.Web.Models.AgencyReportingHomecareDetailsModel();
			model.ServiceTypeId = 8;
			return View(model);
		}

        [CcAuthorize(FixedRoles.Admin
        , FixedRoles.AgencyUser
		, FixedRoles.AgencyUserAndReviewer
        , FixedRoles.AuditorReadOnly
        , FixedRoles.GlobalOfficer
        , FixedRoles.GlobalReadOnly
        , FixedRoles.RegionAssistant
        , FixedRoles.RegionOfficer
        , FixedRoles.RegionReadOnly
        , FixedRoles.Ser
		, FixedRoles.SerAndReviewer)]
        public ActionResult SocializationClientsCountReport()
        {
            return View();
        }

		public ActionResult DafDateRangeReport()
		{
			return View();
		}

		public JsonResult IndexDataTable(CC.Web.Models.AgencyReportingIndexModel model)
		{

			var all = model.GetAgencyReportingData(db, Permissions);
            if(User.IsInRole("RegionReadOnly"))
            {
                model.SelectedRegionId = db.Users.Where(f => f.UserName == User.Identity.Name).Select(f => f.RegionId).SingleOrDefault();
            }
			var filtered = model.ApplyFilter(all);
            if(!string.IsNullOrEmpty(model.sSearch))
            {
                filtered = filtered.Where(f => f.AgencyGroupName.Contains(model.sSearch) || f.AgencyName.Contains(model.sSearch)
                    || f.ServiceTypeName.Contains(model.sSearch) || f.ServiceName.Contains(model.sSearch));
            }
			var sorted = model.ApplySort(filtered);

			var data = new CC.Web.Models.jQueryDataTableResult()
			{
				sEcho = model.sEcho,
				iTotalDisplayRecords = filtered.Count(),
				iTotalRecords = all.Count(),
				aaData = sorted.Skip(model.iDisplayStart).Take(model.iDisplayLength)
			};

			return MyJsonResult(data);
		}
		public ActionResult Export(CC.Web.Models.AgencyReportingIndexModel model)
		{
			var all = model.GetAgencyReportingData(db, Permissions);
            if (User.IsInRole("RegionReadOnly"))
            {
                model.SelectedRegionId = db.Users.Where(f => f.UserName == User.Identity.Name).Select(f => f.RegionId).SingleOrDefault();
            }
			var filtered = model.ApplyFilter(all);
            if (!string.IsNullOrEmpty(model.sSearch))
            {
                filtered = filtered.Where(f => f.AgencyGroupName.Contains(model.sSearch) || f.AgencyName.Contains(model.sSearch)
                    || f.ServiceTypeName.Contains(model.sSearch) || f.ServiceName.Contains(model.sSearch));
            }
			var sorted = model.ApplySort(filtered);

			return this.Excel("AgencyReportingFinancialReportRecap", "Data", sorted);

		}

        public JsonResult BudgetDataTable(CC.Web.Models.AgencyReportingBudgetModel model)
        {

            var all = model.GetAgencyReportingData(db, Permissions);
            var filtered = model.ApplyFilter(all);
            if (!string.IsNullOrEmpty(model.sSearch))
            {
                filtered = filtered.Where(f => f.AgencyGroupName.Contains(model.sSearch) || f.AgencyName.Contains(model.sSearch)
                    || f.ServiceTypeName.Contains(model.sSearch) || f.ServiceName.Contains(model.sSearch));
            }
            var sorted = model.ApplySort(filtered);

            var data = new CC.Web.Models.jQueryDataTableResult()
            {
                sEcho = model.sEcho,
                iTotalDisplayRecords = filtered.Count(),
                iTotalRecords = all.Count(),
                aaData = sorted.Skip(model.iDisplayStart).Take(model.iDisplayLength)
            };

            return MyJsonResult(data);
        }

        public ActionResult ExportBudget(CC.Web.Models.AgencyReportingBudgetModel model)
        {
            var all = model.GetAgencyReportingData(db, Permissions);
            var filtered = model.ApplyFilter(all);
            if (!string.IsNullOrEmpty(model.sSearch))
            {
                filtered = filtered.Where(f => f.AgencyGroupName.Contains(model.sSearch) || f.AgencyName.Contains(model.sSearch)
                    || f.ServiceTypeName.Contains(model.sSearch) || f.ServiceName.Contains(model.sSearch));
            }
            var sorted = model.ApplySort(filtered);

            return this.Excel("AgencyReportingBudgetRecap", "Data", sorted);

        }

		public JsonResult ReportedHcClientsDataTable(CC.Web.Models.AgencyReportingHomecareModel model)
		{

			var all = model.GetAgencyReportingData(db, Permissions);
			if (User.IsInRole("RegionReadOnly"))
			{
				model.SelectedRegionId = db.Users.Where(f => f.UserName == User.Identity.Name).Select(f => f.RegionId).SingleOrDefault();
			}

			var data = new CC.Web.Models.jQueryDataTableResult()
			{
				sEcho = model.sEcho,
				iTotalDisplayRecords = all.Count() > model.iDisplayLength ? model.iDisplayStart + model.iDisplayLength + 1 : model.iDisplayStart + 1,
				aaData = all.Take(model.iDisplayLength)
			};

			return MyJsonResult(data);
		}

		public ActionResult ExportHcClients(CC.Web.Models.AgencyReportingHomecareModel model)
		{
			model.iDisplayLength = int.MaxValue;
			var all = model.GetAgencyReportingData(db, Permissions);
			if (User.IsInRole("RegionReadOnly"))
			{
				model.SelectedRegionId = db.Users.Where(f => f.UserName == User.Identity.Name).Select(f => f.RegionId).SingleOrDefault();
			}
			return this.Excel("AgencyReportingReportedHcClients", "Data", all);

		}

		public JsonResult HomecareDetailsDataTable(CC.Web.Models.AgencyReportingHomecareDetailsModel model)
		{
			if(model.CurId == null)
			{
				var empty = new CC.Web.Models.jQueryDataTableResult()
				{
					sEcho = model.sEcho,
					iTotalDisplayRecords = 0,
					aaData = new List<CC.Web.Models.AgencyReportingHomecareDetailsRow>()
				};
				return MyJsonResult(empty);
			}
			var all = model.GetAgencyReportingData(db, Permissions);

			var data = new CC.Web.Models.jQueryDataTableResult()
			{
				sEcho = model.sEcho,
				iTotalDisplayRecords = all.Count() > model.iDisplayLength ? model.iDisplayStart + model.iDisplayLength + 1 : model.iDisplayStart + 1,
				aaData = all.Take(model.iDisplayLength)
			};

			return MyJsonResult(data);
		}

		public ActionResult ExportHomecareDetails(CC.Web.Models.AgencyReportingHomecareDetailsModel model)
		{
			model.iDisplayLength = int.MaxValue;
			var all = model.GetAgencyReportingData(db, Permissions);
			return this.Excel("AgencyReportingHomecareDetails", "Data", all);

		}

        public JsonResult SocializationClientCountDataTable(CC.Web.Models.AgencyReportingSocializationClientsCount model)
        {
            var all = model.GetAgencyReportingData(db, Permissions);
            var filtered = model.ApplyFilter(all);
            if (!string.IsNullOrEmpty(model.sSearch))
            {
                filtered = filtered.Where(f => f.AgencyGroupName.Contains(model.sSearch) || f.AgencyName.Contains(model.sSearch)
                    || f.Remarks.Contains(model.sSearch));
            }
            var sorted = model.ApplySort(filtered);
            var data = new CC.Web.Models.jQueryDataTableResult()
            {
                sEcho = model.sEcho,
                iTotalDisplayRecords = filtered.Count(),
                iTotalRecords = all.Count(),
                aaData = sorted.Skip(model.iDisplayStart).Take(model.iDisplayLength)
            };

            return MyJsonResult(data);
        }

        public ActionResult ExportSocializationClientsCount(CC.Web.Models.AgencyReportingSocializationClientsCount model)
        {
            model.iDisplayLength = int.MaxValue;
            var all = model.GetAgencyReportingData(db, Permissions);
            var filtered = model.ApplyFilter(all);
            if (!string.IsNullOrEmpty(model.sSearch))
            {
                filtered = filtered.Where(f => f.AgencyGroupName.Contains(model.sSearch) || f.AgencyName.Contains(model.sSearch)
                    || f.Remarks.Contains(model.sSearch));
            }
            var sorted = model.ApplySort(filtered);
            return this.Excel("AgencyReportingSocializationClientsCount", "Data", sorted);

        }

		public JsonResult DafDateRangeDataTable(CC.Web.Models.AgencyReportingDafDateRangeModel model)
		{
			var all = model.GetAgencyReportingData(db, Permissions);
			var filtered = model.ApplyFilter(all);
			var sorted = model.ApplySort(filtered);
			var data = new CC.Web.Models.jQueryDataTableResult()
			{
				sEcho = model.sEcho,
				iTotalDisplayRecords = filtered.Count(),
				iTotalRecords = all.Count(),
				aaData = sorted.Skip(model.iDisplayStart).Take(model.iDisplayLength)
			};

			return MyJsonResult(data);
		}

		public ActionResult ExportDafDateRange(CC.Web.Models.AgencyReportingDafDateRangeModel model)
		{
			model.iDisplayLength = int.MaxValue;
			var all = model.GetAgencyReportingData(db, Permissions);
			var filtered = model.ApplyFilter(all);
			var sorted = model.ApplySort(filtered);
			return this.Excel("AgencyReportingDafDateRangeReport", "Data", sorted);

		}
	}
}
