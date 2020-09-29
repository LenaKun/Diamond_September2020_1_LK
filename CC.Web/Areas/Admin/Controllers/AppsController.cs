using System;
using System.Collections.Generic;
using MvcContrib;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CC.Data;
using CC.Web.Models;
using System.ComponentModel.DataAnnotations;
namespace CC.Web.Areas.Admin.Controllers
{
	[CcAuthorize(CC.Data.FixedRoles.Admin, CC.Data.FixedRoles.GlobalOfficer)]
	public class AppsController : AdminControllerBase
	{
		//
		// GET: /Admin/Apps/
		public ViewResult Index()
		{

			return View();
		}

		/// <summary>
		/// provides datatable with json data
		/// </summary>
		/// <param name="jq"></param>
		/// <returns></returns>
		public JsonResult IndexData(jQueryDataTableParamModel jq)
		{
			var q = from app in db.Apps
					select new
					{
						Id = app.Id,
						FundName = app.Fund.Name,
						AgencyGroupName = app.AgencyGroup.DisplayName,
						Name = app.Name,
						AgencyContribution = app.AgencyContribution,
						CcGrant = app.CcGrant,
						RequiredMatch = app.RequiredMatch,
						CalendaricYear = System.Data.Objects.SqlClient.SqlFunctions.DatePart("year", app.StartDate),
						app.EndOfYearValidationOnly,
						app.InterlineTransfer,
					}
			;

			var filtered = q;
			if (!string.IsNullOrWhiteSpace(jq.sSearch))
			{
				foreach (var s in jq.sSearch.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
				{
					filtered = filtered.Where(f => f.AgencyGroupName.Contains(s) || f.FundName.Contains(s) || f.Name.Contains(s));
				}
			}

			var sortFieldName = Request["mDataProp_" + Request["iSortCol_0"]];
			var sortDir = Request["sSortDir_0"] == "asc";
			var sorted = filtered.OrderByField(sortFieldName, sortDir);

			return this.MyJsonResult(new jQueryDataTableResult
			{
				aaData = sorted.Skip(jq.iDisplayStart).Take(jq.iDisplayLength),
				sEcho = jq.sEcho,
				iTotalDisplayRecords = filtered.Count(),
				iTotalRecords = q.Count()
			}, JsonRequestBehavior.AllowGet);
		}

		//
		// GET: /Admin/Apps/Details/5
		public ViewResult Details(int id)
		{
			App app = db.Apps.Single(a => a.Id == id);
			return View(app);
		}

		//
		// GET: /Admin/Apps/Create
        [CcAuthorize(CC.Data.FixedRoles.Admin)]
		public ActionResult Create()
		{
			var model = new App();
			ViewBag.FundId = new SelectList(db.Funds, "Id", "Name");
			ViewBag.AgencyGroupId = new SelectList(db.AgencyGroups
				.Where(this.Permissions.AgencyGroupsFilter)
				.Select(f => new { Id = f.Id, Name = f.DisplayName })
				.OrderBy(f => f.Name), "Id", "Name", null);
			ViewBag.CurrencyId = new SelectList(db.Currencies.OrderBy(f => f.Id), "Id", "Id", null);
			return View(model);
		}

		//
		// POST: /Admin/Apps/Create
        [CcAuthorize(CC.Data.FixedRoles.Admin)]
		[HttpPost]
		public ActionResult Create(App app)
		{
			if (ModelState.IsValid)
			{
				db.Apps.AddObject(app);
				GetExRatesFromRequest(app);
				try
				{
					db.SaveChanges();

					return RedirectToAction("Index");
				}
				catch (UpdateException ex)
				{
					ModelState.AddModelError(string.Empty, ex.InnerException.Message);
				}
			}

			ViewBag.FundId = new SelectList(db.Funds, "Id", "Name", app.FundId);
			ViewBag.AgencyGroupId = new SelectList(db.AgencyGroups, "Id", "Name", app.AgencyGroupId);
			ViewBag.CurrencyId = new SelectList(db.Currencies.OrderBy(f => f.Id), "Id", "Id", app.CurrencyId);
			return View(app);
		}

		//
		// GET: /Admin/Apps/Edit/5
        [CcAuthorize(CC.Data.FixedRoles.Admin, CC.Data.FixedRoles.GlobalOfficer)]
		public ActionResult Edit(int id)
		{
			App app = db.Apps.Single(a => a.Id == id);
			ViewBag.FundId = new SelectList(db.Funds, "Id", "Name", app.FundId);
			ViewBag.AgencyGroupId = new SelectList(db.AgencyGroups, "Id", "Name", app.AgencyGroupId);
			ViewBag.CurrencyId = new SelectList(db.Currencies.OrderBy(f => f.Id), "Id", "Id", app.CurrencyId);
			ViewBag.serviceId = new SelectList(db.Services.Where(f => !f.Apps.Any(a => a.Id == id)).OrderBy(f => f.Name), "Id", "Name");

			ViewBag.Services = db.Services.OrderBy(f => f.ServiceType.Name).ThenBy(f => f.Name).Select(s => new
								{
									Id = s.Id,
									Name = s.Name,
									TypeName = s.ServiceType.Name,
									Selected = s.Apps.Any(f => f.Id == id)
								}).ToList().Select(f => new SelectListItem() { Text = f.TypeName + " - " + f.Name, Value = f.Id.ToString(), Selected = f.Selected });
			return View(app);
		}

		//
		// POST: /Admin/Apps/Edit/5
		[CcAuthorize(CC.Data.FixedRoles.Admin, CC.Data.FixedRoles.GlobalOfficer)]
		[HttpPost]
		public ActionResult Edit(int id, App app, IEnumerable<string> sids)
		{
			var services = sids.Select(f => f.Parse<int>()).Where(f => f.HasValue).Select(f => f.Value);

			var existing = db.Apps.Include(f => f.Services)
				.Include(f => f.AppExchangeRates)
				.SingleOrDefault(f => f.Id == id);
			if (User.IsInRole(FixedRoles.Admin))
			{
				existing.CurrencyId = app.CurrencyId;
				existing.Name = app.Name;
				existing.AgencyContribution = app.AgencyContribution;
				existing.CcGrant = app.CcGrant;
				existing.RequiredMatch = app.RequiredMatch;
				existing.CalendaricYear = app.CalendaricYear;
				existing.OtherServicesMax = app.OtherServicesMax;
				existing.HomecareMin = app.HomecareMin;
				existing.AdminMax = app.AdminMax;
				existing.EndOfYearValidationOnly = app.EndOfYearValidationOnly;
				existing.InterlineTransfer = app.InterlineTransfer;
				existing.MaxAdminAmount = app.MaxAdminAmount;
				existing.MaxNonHcAmount = app.MaxNonHcAmount;
				existing.HistoricalExpenditureAmount = app.HistoricalExpenditureAmount;
				existing.AvgReimbursementCost = app.AvgReimbursementCost;
				existing.MaxHcCaseManagementPersonnel = app.MaxHcCaseManagementPersonnel;
				existing.MaxServicesPersonnelOther = app.MaxServicesPersonnelOther;
                existing.FuneralExpenses = app.FuneralExpenses;
                GetExRatesFromRequest(existing);
			}
			if (User.IsInRole(FixedRoles.Admin) || User.IsInRole(FixedRoles.GlobalOfficer))
			{

				if (services != null && services.Any())
				{
					foreach (var sid in services)
					{
						if (!existing.Services.Any(f => f.Id == sid))
						{
							var service = new Service() { Id = sid };
							db.Services.Attach(service);
							existing.Services.Add(service);
						}
					}
					foreach (var service in existing.Services.ToList())
					{
						if (!services.Contains(service.Id))
						{
							existing.Services.Remove(service);
						}
					}
				}
				else
				{
					existing.Services.Clear();
				}
			}
			ModelState.Clear();
			TryValidateModel(existing);
			if (ModelState.IsValid)
			{
				db.SaveChanges();
				return RedirectToAction("Index");
			}
			ViewBag.FundId = new SelectList(db.Funds, "Id", "Name", app.FundId);
			ViewBag.AgencyGroupId = new SelectList(db.AgencyGroups, "Id", "Name", app.AgencyGroupId);
			ViewBag.CurrencyId = new SelectList(db.Currencies.OrderBy(f => f.Id), "Id", "Id", app.CurrencyId);
			ViewBag.Services = db.Services.Select(s => new
			{
				Id = s.Id,
				Name = s.Name,
				TypeName = s.ServiceType.Name,
				Selected = s.Apps.Any(f => f.Id == app.Id)
			}).ToList().Select(f => new SelectListItem() { Text = f.TypeName + " - " + f.Name, Value = f.Id.ToString(), Selected = f.Selected });
			return View(app);

		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="serviceId"></param>
		/// <param name="appId"></param>
		/// <returns></returns>
        [CcAuthorize(CC.Data.FixedRoles.Admin)]
		[HttpPost]
		public ActionResult AddService(int serviceId, int appId)
		{
			var app = db.Apps.Single(f => f.Id == appId);
			var service = db.Services.Single(f => f.Id == serviceId);
			app.Services.Add(service);
			db.Apps.Attach(app);
			db.SaveChanges();
			return RedirectToAction("Edit", new { id = appId });

		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="App"></param>
		private void GetExRatesFromRequest(App App)
		{
			var currencies = db.Currencies.ToList();
			foreach (var currency in currencies)
			{

				decimal rate;
				var AppExRate = App.AppExchangeRates.SingleOrDefault(f => f.CurId == currency.Id);
				if (decimal.TryParse(Request[currency.Id], out rate))
				{
					
					if (AppExRate == null)
					{
						AppExRate = new AppExchangeRate { AppId = App.Id, CurId = currency.Id };
						App.AppExchangeRates.Add(AppExRate);
					}
					AppExRate.Value = rate;
				}
				else
				{
					if (AppExRate != null)
					{
						App.AppExchangeRates.Remove(AppExRate);
					}
				}
			}

		}

		//
		// GET: /Admin/Apps/Delete/5
        [CcAuthorize(CC.Data.FixedRoles.Admin)]
		public ActionResult Delete(int id)
		{
			App app = db.Apps.Where(this.Permissions.AppsFilter).Single(a => a.Id == id);
			if (db.AppBudgets.Any(f => f.AppId == id))
			{
				ModelState.AddModelError(string.Empty, "The App can not be deleted because of related Budgets.");
			}
			return View(app);
		}

		//
		// POST: /Admin/Apps/Delete/5
        [CcAuthorize(CC.Data.FixedRoles.Admin)]
		[HttpPost, ActionName("Delete")]
		public ActionResult DeleteConfirmed(int id)
		{
			App app = db.Apps.Include(f => f.Services).Single(a => a.Id == id);
			if (db.AppBudgets.Any(f => f.AppId == id))
			{
				ModelState.AddModelError(string.Empty, "The App can not be deleted because of related Budgets.");
			}
			if (ModelState.IsValid)
			{
				app.Services.Clear();
				db.Apps.DeleteObject(app);
				db.SaveChanges();
			}
			return RedirectToAction("Index");
		}

		/// <summary>
		/// Recieves uploaded csv data
		/// </summary>
		/// <param name="file"></param>
		/// <returns></returns>
        [CcAuthorize(CC.Data.FixedRoles.Admin)]
		[HttpPost, ActionName("Upload")]
		public ActionResult Upload(HttpPostedFileBase file)
		{
			var model = new CC.Web.Areas.Admin.Models.AppsImportModel();
			model.Upload(file);
			return this.RedirectToAction(f => f.Preview(model.Id));
		}

		/// <summary>
		/// Returns preivew html
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
        [CcAuthorize(CC.Data.FixedRoles.Admin)]
		public ActionResult Preview(Guid id)
		{
			var model = new CC.Web.Areas.Admin.Models.AppsImportModel(id);
			var data = model.PreviewData(db);
			ViewBag.HasErrors = data.Any(f => f.Errors != null && f.Errors != "");
			return View(model.Id);
		}

		/// <summary>
		/// returns preview data
		/// </summary>
		/// <param name="id"></param>
		/// <param name="jq"></param>
		/// <returns></returns>
        [CcAuthorize(CC.Data.FixedRoles.Admin)]
		public ActionResult PreviewData(Guid id, CC.Web.Models.jQueryDataTableParamModel jq)
		{
			var model = new CC.Web.Areas.Admin.Models.AppsImportModel(id);
			var result = model.GetPreview(this.db, jq);
			return this.MyJsonResult(result);
		}

		/// <summary>
		/// imports the temp data
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
        [CcAuthorize(CC.Data.FixedRoles.Admin)]
		[HttpPost]
		public ActionResult Import(Guid id)
		{
			var model = new CC.Web.Areas.Admin.Models.AppsImportModel();
			var cnt = db.ExecuteStoreCommand("exec [dbo].[ImportApps] @id",
				new System.Data.SqlClient.SqlParameter { ParameterName = "id", Value = id });

			var lft = db.AppsImports.Count(f => f.Id == id);
			if (lft>0)
			{
				ModelState.AddModelError(string.Empty,
					string.Format("{0} rows have been imported. {1} rows left.", cnt, lft));
			}

			return RedirectToAction("Index");
		}

		/// <summary>
		/// deletes temp data
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
        [CcAuthorize(CC.Data.FixedRoles.Admin)]
		[HttpPost]
		public ActionResult CancelImport(Guid id)
		{

			db.ExecuteStoreCommand("delete from appsImport where id=@id",
				new System.Data.SqlClient.SqlParameter { ParameterName = "id", Value = id });
			return RedirectToAction("Index");
		}
        public ActionResult Export()
        {
            var apps = (from app in db.Apps.Include("Fund").Include("AgencyGroup").ToList()
                        select new AppsListRow
                               {
                                   FundName = app.Fund.Name,
                                   AgencyGroupName = app.AgencyGroup.DisplayName,
                                   Name = app.Name,
                                   AgencyContribution = app.AgencyContribution ? "true" : "false",
                                   CcGrant = app.CcGrant,
                                   Currency = app.CurrencyId,
						           RequiredMatch = app.RequiredMatch,
						           CalendaricYear = app.StartDate.Year,
                                   EndOfYearValidationOnly = app.EndOfYearValidationOnly ? "true" : "false",
                                   InterlineTransfer = app.InterlineTransfer ? "true" : "false",
								   MaxAdminAmount = app.MaxAdminAmount,
								   MaxNonHcAmount = app.MaxNonHcAmount,
								   HistoricalExpenditureAmount = app.HistoricalExpenditureAmount
                               });
            return this.Excel("Apps", "Sheet1", apps);
        }

		protected override void Dispose(bool disposing)
		{
			db.Dispose();
			base.Dispose(disposing);
		}

        private class AppsListRow
        {
            [Display(Name = "Fund Name")]
            public string FundName { get; set; }
            [Display(Name = "Ser")]
            public string AgencyGroupName { get; set; }
            [Display(Name = "Name")]
            public string Name { get; set; }
            [Display(Name = "Agency Contribution")]
            public string AgencyContribution { get; set; }
            [Display(Name = "CcGrant")]
            public decimal CcGrant { get; set; }
            [Display(Name = "CUR")]
            public string Currency { get; set; }
            [Display(Name = "Required Match")]
            public decimal RequiredMatch { get; set; }
            [Display(Name = "Calendaric Year")]
            public int CalendaricYear { get; set; }
            [Display(Name = "Only EOY validation")]
            public string EndOfYearValidationOnly { get; set; }
            [Display(Name = "Interline Transfer")]
            public string InterlineTransfer { get; set; }

			[Display(Name="Total Admin allowed")]
			public decimal? MaxAdminAmount { get; set; }

			[Display(Name="Total all NONE Homecare services amount allowed")]
			public decimal? MaxNonHcAmount { get; set; }

			[Display(Name="Historical Expenditure Amount")]
			public decimal? HistoricalExpenditureAmount { get; set; }
		}
	}
}