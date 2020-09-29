using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CC.Web.Models;
using CC.Data;

namespace CC.Web.Controllers
{
	[CcAuthorize(FixedRoles.Admin
		, FixedRoles.AgencyUser
		, FixedRoles.AuditorReadOnly
		, FixedRoles.BMF
		, FixedRoles.GlobalOfficer
		, FixedRoles.GlobalReadOnly
		, FixedRoles.RegionAssistant
		, FixedRoles.RegionOfficer
		, FixedRoles.RegionReadOnly
		, FixedRoles.Ser
		, FixedRoles.SerAndReviewer
		, FixedRoles.AgencyUserAndReviewer
		, FixedRoles.DafEvaluator
		, FixedRoles.DafReviewer)]
	public class FinancialSummaryController : PrivateCcControllerBase
	{
		public FinancialSummaryController()
			: base()
		{
			this.db.CommandTimeout = 300;
		}
		public ActionResult Overview(FinancialSummaryOverviewModel model)
		{
			model.Load(db, Permissions);
            if(User.IsInRole("RegionReadOnly"))
            {
                model.RegionId = db.Users.Where(f => f.UserName == User.Identity.Name).Select(f => f.RegionId).SingleOrDefault();
            }
			return View("Overview", model);
		}
		public ActionResult OverviewData(FinancialSummaryOverviewModel model)
		{
			try
			{
				var result = model.GetJqResult(db, Permissions);
				return this.MyJsonResult(result);
			}
			catch
			{
				throw;
			}
		}
		public ActionResult OverviewExport(FinancialSummaryOverviewModel model)
		{
			db.CommandTimeout = 180;
			model.iDisplayStart = 0;
			model.iDisplayLength = int.MaxValue;
			var result = model.Data(db, Permissions);
			var missingExRates = result.Where(f=>f.Amount == null).ToList();
			if (missingExRates.Any())
            {
                model.Load(db, Permissions);
                if (User.IsInRole("RegionReadOnly"))
                {
                    model.RegionId = db.Users.Where(f => f.UserName == User.Identity.Name).Select(f => f.RegionId).SingleOrDefault();
                }
				foreach (var item in missingExRates.Select(f=>f.MasterFundName).Distinct())
				{
					var msg = string.Format("No exchange rate is defined for the {0} in the Master Fund {1}", model.CurId, item);
					this.ModelState.AddModelError("", msg);
				}
                return View("Overview", model);
            }
			return this.Excel("output", "data", result.ToList());
		}
		public ActionResult OverviewPreview(FinancialSummaryOverviewModel model)
		{
			var data = model.Data(db, Permissions);
			return View(model);
		}


		public ActionResult Index(FinancialSummaryIndexModel model)
		{
			model.Load(db, Permissions);
            if (User.IsInRole("RegionReadOnly"))
            {
                model.RegionId = db.Users.Where(f => f.UserName == User.Identity.Name).Select(f => f.RegionId).SingleOrDefault();
            }
			return View("Index", model);
		}
		public ActionResult IndexData(FinancialSummaryIndexModel model)
		{
			try
			{
				var result = model.GetJqResult(db, Permissions);
				return this.MyJsonResult(result);
			}
			catch
			{
				throw;
			}
		}
		public ActionResult IndexExport(FinancialSummaryIndexModel model)
		{
			db.CommandTimeout = 180;
			model.iDisplayStart = 0;
			model.iDisplayLength = int.MaxValue;
			var result = model.Data(db, Permissions);
			var missingExRates = result.Where(f => f.Amount == null).Select(f => new { f.Cur, f.AppName }).Distinct().ToList();
			if (missingExRates.Any())
			{
				model.Load(db, Permissions);
				if (User.IsInRole("RegionReadOnly"))
				{
					model.RegionId = db.Users.Where(f => f.UserName == User.Identity.Name).Select(f => f.RegionId).SingleOrDefault();
				}
				foreach (var item in missingExRates)
				{
					var msg = string.Format("No exchange rate is defined for the {0} in the app {1}", item.Cur, item.AppName);
					this.ModelState.AddModelError("", msg);
				}
				return View("Index", model);
			}
			return this.Excel("output", "data", result.ToList());
		}
		public ActionResult IndexPreview(FinancialSummaryIndexModel model)
		{
			var data = model.Data(db, Permissions);
			return View(model);
		}

		public ActionResult Details(FinancialSummaryDetailsModel model)
		{
			model.Load(db, Permissions);
            if (User.IsInRole("RegionReadOnly"))
            {
                model.RegionId = db.Users.Where(f => f.UserName == User.Identity.Name).Select(f => f.RegionId).SingleOrDefault();
            }
			return View("Details", model);
		}
		public ActionResult DetailsData(FinancialSummaryDetailsModel model)
		{
			try
			{
				var result = model.GetJqResult(db, Permissions);
				return this.MyJsonResult(result);
			}
			catch
			{
				throw;
			}
		}
		public ActionResult DetailsExport(FinancialSummaryDetailsModel model)
		{
			db.CommandTimeout = 180;
			model.iDisplayStart = 0;
			model.iDisplayLength = int.MaxValue;
			var result = model.Data(db, Permissions);
			var missingExRates = result.Where(f => f.Amount == null).Select(f=>new {f.Cur, f.AppName}).Distinct().ToList();
            if (missingExRates.Any())
            {
                model.Load(db, Permissions);
                if (User.IsInRole("RegionReadOnly"))
                {
                    model.RegionId = db.Users.Where(f => f.UserName == User.Identity.Name).Select(f => f.RegionId).SingleOrDefault();
                }
				foreach (var item in missingExRates)
				{
					var msg = string.Format("No exchange rate is defined for the {0} in the app {1}", item.Cur, item.AppName);
					this.ModelState.AddModelError("", msg);
				}
                return View("Details", model);
            }
            return this.Excel("output", "data", result.ToList());
		}
		public ActionResult DetailsPreview(FinancialSummaryDetailsModel model)
		{
			var data = model.Data(db, Permissions);
			return View(model);
		}
	}

}
