using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CC.Data;
using System.ComponentModel.DataAnnotations;
using CC.Web.Helpers;

namespace CC.Web.Areas.Admin.Controllers
{
    [CcAuthorize(CC.Data.FixedRoles.Admin)]
	public class AgencyGroupsController : AdminControllerBase
	{

		private void PrepareViewBag(int? CountryId, int? StateId, string Culture, string Cur)
		{
			ViewBag.CountryId = new SelectList(db.Countries, "Id", "Code", CountryId);
			ViewBag.StateId = new SelectList(db.States, "Id", "Code", StateId);
			ViewBag.Culture = new SelectList(db.Languages.OrderBy(f => f.Name), "Id", "Name", Culture);
			ViewBag.DefaultCurrency = new SelectList(db.Currencies.OrderBy(f => f.Id), "Id", "Id", Cur);
		}

		//

		// GET: /Admin/AgencyGroups/

		public ViewResult Index()
		{
			return View();
		}

		public JsonResult IndexData(CC.Web.Models.jQueryDataTableParamModel p)
		{
			var source = from ag in db.AgencyGroups.Where(this.Permissions.AgencyGroupsFilter)
						 select new
						 {
							 Id = ag.Id,
							 Name = ag.Name,
							 Addr1 = ag.Addr1,
							 Addr2 = ag.Addr2,
							 City = ag.City,
							 State = ag.State.Code,
							 Country = ag.Country.Code,
							 Region = ag.Country.Region.Name,
							 CanSubmitRevisionReports = ag.CanSubmitRevisionReports,
							 ForceIsraelID = ag.ForceIsraelID,
							 ReportingPeriodId = ag.ReportingPeriodId,
							 RequiredMatch = ag.RequiredMatch,
                             ExcludeFromReports = ag.ExcludeFromReports,
							 ag.Culture,
							 DefaultCurrency = ag.DefaultCurrency,
							 CfsDate = ag.CfsDate,
                             FluxxId = ag.FluxxId
						 };

			var sSortCol_0 = Request["mDataProp_" + p.iSortCol_0];
			var bSortAsc_0 = p.sSortDir_0 == "asc";

			var filtered = source;
			if (!string.IsNullOrEmpty(p.sSearch))
			{
				filtered = filtered.Where(f => System.Data.Objects.SqlClient.SqlFunctions.StringConvert((decimal)f.Id).Trim().Equals(p.sSearch)
					|| f.Name.Contains(p.sSearch)
					|| f.Addr1.Contains(p.sSearch)
					|| f.Addr2.Contains(p.sSearch)
					|| f.City.Contains(p.sSearch)
					|| f.Region.Contains(p.sSearch)
					|| f.Country.Equals(p.sSearch)
					|| f.State.Equals(p.sSearch)
					|| f.DefaultCurrency.Contains(p.sSearch)
				);
			}

			var data = filtered.OrderByField(sSortCol_0, bSortAsc_0).Skip(p.iDisplayStart).Take(p.iDisplayLength);

			var result = new CC.Web.Models.jQueryDataTableResult()
			{
				aaData = data,
				sEcho = p.sEcho,
				iTotalRecords = source.Count(),
				iTotalDisplayRecords = filtered.Count()
			};

			return this.MyJsonResult(result);
		}

		//
		// GET: /Admin/AgencyGroups/Details/5

		public ViewResult Details(int id)
		{
			AgencyGroup agencygroup = db.AgencyGroups.Single(a => a.Id == id);
			return View(agencygroup);
		}

		//
		// GET: /Admin/AgencyGroups/Create

		public ActionResult Create()
		{
			PrepareViewBag(null, null, null, null);
			return View();
		}

		//
		// POST: /Admin/AgencyGroups/Create

		[HttpPost]
		public ActionResult Create(AgencyGroup agencygroup)
		{
			ValidateSave(agencygroup);
			if (db.AgencyGroups.Any(f => f.Id == agencygroup.Id))
			{
				ModelState.AddModelError(string.Empty, "Org ID must be unique");
			}
			if (ModelState.IsValid)
			{
				db.AgencyGroups.AddObject(agencygroup);
				db.SaveChanges();
				if (!string.IsNullOrEmpty(GlobalDbSettings.GetString(GlobalDbSettings.GlobalStringNames.NewSerOrgNotifyEmail)))
				{
					EmailHelper.SendNewSerOrgNotification(true, agencygroup.Id, agencygroup.Name);
				}
				return RedirectToAction("Index");
			}

			PrepareViewBag(agencygroup.CountryId, agencygroup.StateId, agencygroup.Culture, agencygroup.DefaultCurrency);
			
			return View(agencygroup);
		}

		//
		// GET: /Admin/AgencyGroups/Edit/5

		public ActionResult Edit(int id)
		{
			AgencyGroup agencygroup = db.AgencyGroups.Single(a => a.Id == id);
			PrepareViewBag(agencygroup.CountryId, agencygroup.StateId, agencygroup.Culture, agencygroup.DefaultCurrency);
			return View(agencygroup);
		}

	
		//
		// POST: /Admin/AgencyGroups/Edit/5

		[HttpPost]
		public ActionResult Edit(AgencyGroup agencygroup)
		{
			ValidateSave(agencygroup);
			if (ModelState.IsValid)
			{
				db.AgencyGroups.Attach(agencygroup);
				db.ObjectStateManager.ChangeObjectState(agencygroup, EntityState.Modified);
				db.SaveChanges();
				return RedirectToAction("Index");
			}
			PrepareViewBag(agencygroup.CountryId, agencygroup.StateId, agencygroup.Culture, agencygroup.DefaultCurrency);
			return View(agencygroup);
		}

		//
		// GET: /Admin/AgencyGroups/Delete/5
		private void ValidateSave(AgencyGroup agencygroup)
		{
			if (agencygroup.CountryId != 0)
			{
				var country = db.Countries.Where(c => c.Id == agencygroup.CountryId).First();

				if (country != null)
				{
					if (country.Name == "Israel" && agencygroup.ForceIsraelID == false)
					{
						ModelState.AddModelError(string.Empty, "For Country IL field 'Israel ID is required' should be checked.");
					}
				}
			}
		}
		private void ValidateDelete(int id)
		{
           // var hasDependancies;
            //var AppsSer = db.Apps.Where(c => c.AgencyGroupId == id).DefaultIfEmpty();
           // var AgenciesSer = db.Agencies.Where(c => c.GroupId == id).DefaultIfEmpty();
            var hasDependancies = db.AgencyGroups.Where(f => f.Apps.Any() || f.Agencies.Any()).Where(f => f.Id==id).Any();
           // if(AppsSer != null || AgenciesSer != null)
           // {
               // hasDependancies == true; 
			if (hasDependancies)
			{
				ModelState.AddModelError(string.Empty, "Ser can not be deleted.");
			}
		}
		public ActionResult Delete(int id)
		{
			AgencyGroup agencygroup = db.AgencyGroups.Single(a => a.Id == id);

			ValidateDelete(id);

			return View(agencygroup);
		}

        public ActionResult SubsidyLevels()
        {
            return View();
        }

        public ActionResult SubsidyLevelsData(CC.Web.Models.jQueryDataTableParamModel p)
        {
            var source = from sc in db.ScSubsidyAmounts
                         select new
                         {
                             LevelId = sc.LevelId,
                             FullSubsidy = sc.FullSubsidy,
                             StartDate = sc.StartDate,
                             Amount = sc.Amount
                         };

            var sSortCol_0 = Request["mDataProp_" + p.iSortCol_0];
            var bSortAsc_0 = p.sSortDir_0 == "asc";

            var filtered = source;
            if (!string.IsNullOrEmpty(p.sSearch))
            {
                filtered = filtered.Where(f => System.Data.Objects.SqlClient.SqlFunctions.StringConvert((decimal)f.LevelId).Trim().Equals(p.sSearch)
                    || System.Data.Objects.SqlClient.SqlFunctions.StringConvert((decimal)f.Amount).Trim().Equals(p.sSearch)
                );
            }

            var data = filtered.OrderByField(sSortCol_0, bSortAsc_0).Skip(p.iDisplayStart).Take(p.iDisplayLength).OrderBy(f => f.LevelId).ThenBy(f => f.StartDate);

            var result = new CC.Web.Models.jQueryDataTableResult()
            {
                aaData = data,
                sEcho = p.sEcho,
                iTotalRecords = source.Count(),
                iTotalDisplayRecords = filtered.Count()
            };

            return this.MyJsonResult(result);
        }

		//
		// POST: /Admin/AgencyGroups/Delete/5

		[HttpPost, ActionName("Delete")]
		public ActionResult DeleteConfirmed(int id)
		{
			ValidateDelete(id);
			AgencyGroup agencygroup = db.AgencyGroups.Single(a => a.Id == id);
			if (ModelState.IsValid)
			{

				db.AgencyGroups.DeleteObject(agencygroup);
				try
				{
					db.SaveChanges();
				}
				catch (Exception ex)
				{
					ModelState.AddModelError(string.Empty, ex.Message);
				}
				return RedirectToAction("Index");
			}
			else
			{
				return View(agencygroup);
			}
		}

        public ActionResult Export()
        {
            var sers = (from ag in db.AgencyGroups.Where(this.Permissions.AgencyGroupsFilter)
                        select new AgencyGroupsListRow
                        {
                            Id = ag.Id,
                            Name = ag.Name,
                            Addr1 = ag.Addr1,
                            Addr2 = ag.Addr2,
                            City = ag.City,
                            State = ag.State.Code,
                            Country = ag.Country.Code,
                            Region = ag.Country.Region.Name,
                            CanSubmitRevisionReports = ag.CanSubmitRevisionReports ? "true" : "false",
                            ForceIsraelID = ag.ForceIsraelID ? "true" : "false",
                            ReportingPeriodId = ag.ReportingPeriodId,
                            RequiredMatch = ag.RequiredMatch ? "true" : "false",
                            ExcludeFromReports = ag.ExcludeFromReports ? "true" : "false",
							DefaultCurrency = ag.DefaultCurrency,
							CfsDate = ag.CfsDate,
                            FluxxId =  (ag.FluxxId ?? 0) 
                        }).ToList();
            return this.Excel("SERs", "Sheet1", sers);
        }

        private class AgencyGroupsListRow
        {
            [Display(Name = "CCID")]
            public int Id { get; set; }
            [Display(Name = "Name")]
            public string Name { get; set; }
            [Display(Name = "Addr1")]
            public string Addr1 { get; set; }
            [Display(Name = "Addr2")]
            public string Addr2 { get; set; }
            [Display(Name = "City")]
            public string City { get; set; }
            [Display(Name = "State")]
            public string State { get; set; }
            [Display(Name = "Country")]
            public string Country { get; set; }
            [Display(Name = "Region")]
            public string Region { get; set; }
            [Display(Name = "Can Submit Revision Reports")]
            public string CanSubmitRevisionReports { get; set; }
            [Display(Name = "Force Israel ID Validation")]
            public string ForceIsraelID { get; set; }
            [Display(Name = "Reporting Period (months)")]
            public int ReportingPeriodId { get; set; }
            [Display(Name = "Required Match")]
            public string RequiredMatch { get; set; }
            [Display(Name = "Exclude From Reporting/Financial Summary")]
            public string ExcludeFromReports { get; set; }
			[Display(Name = "Default Currency")]
			public string DefaultCurrency { get; set; }
			[Display(Name = "CFS Date")]
			public DateTime? CfsDate { get; set; }
            [Display(Name = "FluxxId")]
            public int FluxxId { get; set; }
        }

		protected override void Dispose(bool disposing)
		{
			db.Dispose();
			base.Dispose(disposing);
		}
	}
}