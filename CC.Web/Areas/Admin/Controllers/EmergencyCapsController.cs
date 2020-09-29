using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web;
using System.Web.Mvc;
using MvcContrib.ActionResults;
using CC.Data;
using System.ComponentModel.DataAnnotations;

namespace CC.Web.Areas.Admin.Controllers
{
    [CcAuthorize(CC.Data.FixedRoles.Admin)]
	public class EmergencyCapsController: AdminControllerBase
	{

		private readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(EmergencyCapsController));


		[HttpGet]
		public ActionResult Index()
		{
			var q = from ecap in db.EmergencyCaps
					select ecap;

			return View(q);
		}

		[HttpGet]
		public ActionResult Create()
		{
			var cap = new EmergencyCap();

			FetchRelationships(cap);

			return View("Edit", cap);
		}

		[HttpPost]
		public ActionResult Create(EmergencyCap cap, IEnumerable<int> cIds, IEnumerable<int> fIds)
		{

			

			cap.UpdatedAt = DateTime.Now;
			cap.UpdatedBy = this.Permissions.User.Id;

			if (ModelState.IsValid)
			{

				try
				{
					db.EmergencyCaps.AddObject(cap);
					foreach (var country in cap.Countries)
					{
						db.ObjectStateManager.ChangeObjectState(country,System.Data.EntityState.Unchanged);
					}

					foreach (var fund in cap.Funds)
					{
						db.ObjectStateManager.ChangeObjectState(fund, System.Data.EntityState.Unchanged);
					}
					db.SaveChanges();
					return this.RedirectToAction("Index");
				}
				catch (Exception ex)
				{
					_log.Error(ex, ex);
					throw;
				}

			}
			FetchRelationships(cap);
			return View("Edit", cap);

		}

		[HttpGet]
		public ActionResult Edit(int id)
		{
			db.ContextOptions.LazyLoadingEnabled = false;
			db.ContextOptions.ProxyCreationEnabled = false;

			var cap = db.EmergencyCaps.Single(f => f.Id == id);

			FetchRelationships(cap);



			return View(cap);
		}

		[HttpPost]
		public ActionResult Edit(EmergencyCap input)
		{
			db.ContextOptions.LazyLoadingEnabled = false;
			db.ContextOptions.ProxyCreationEnabled = false;

			var cap = db.EmergencyCaps.Include(f => f.Countries).Include(f => f.Funds).Single(f => f.Id == input.Id);
			var entry = db.ObjectStateManager.GetObjectStateEntry(cap);

			db.ApplyCurrentValues<EmergencyCap>(entry.EntitySet.Name, input);

			NewMethod(input.cIds, input.fIds, cap);

			cap.UpdatedAt = DateTime.Now;
			cap.UpdatedBy = this.Permissions.User.Id;


			if (ModelState.IsValid)
			{
				try
				{
					var rowsUpdated = db.SaveChanges();
					return this.RedirectToIndex();
				}
				catch (Exception ex)
				{
					_log.Error(ex, ex);
					throw;
				}
			}
			FetchRelationships(cap);

			return View("Edit", cap);
		}

		[HttpGet]
		public ActionResult Details(int id)
		{
			var ec = db.EmergencyCaps.Single(f => f.Id == id);
			var h = db.Histories.Where(f => f.TableName == "emergencycaps" && f.ReferenceId == id);


			ViewBag.History = h;

			return View(ec);
		}

		[HttpGet]
		public ActionResult Delete(int id)
		{
			var cap = db.EmergencyCaps.Single(f => f.Id == id);
			foreach (var c in cap.Countries.ToList())
			{
				cap.Countries.Remove(c);
			}
			foreach (var f in cap.Funds.ToList())
			{
				cap.Funds.Remove(f);
			}
			db.EmergencyCaps.Attach(cap);
			db.EmergencyCaps.DeleteObject(cap);
			db.SaveChanges();
			return RedirectToIndex();
		}

        public ActionResult Export()
        {
            var ecaps = (from ecap in db.EmergencyCaps.ToList()
                        select new EmergencyCapsListRow
                        {
                            Name = ecap.Name,
                            CapPerPerson = ecap.CapPerPerson,
                            CurrencyId = ecap.CurrencyId,
                            CountryNames = string.Join(", ", ecap.CountryNames),
                            FundNames = string.Join(", ", ecap.FundNames),
                            DiscretionaryPercentage = ecap.DiscretionaryPercentage,
                            StartDate = ecap.StartDate.ToString("dd MMM yyyy"),
                            EndDateDisplay = ecap.EndDateDisplay.HasValue ? ecap.EndDateDisplay.Value.ToString("dd MMM yyyy") : "N/A",
                            Active = ecap.Active ? "True" : "False"
                        });
            return this.Excel("Emergency Caps", "Sheet1", ecaps);
        }

        private class EmergencyCapsListRow
        {
            [Display(Name = "Cap Name")]
            public string Name { get; set; }
            [Display(Name = "Cap per person")]
            public decimal CapPerPerson { get; set; }
            [Display(Name = "CUR")]
            public string CurrencyId { get; set; }
            [Display(Name = "Countries")]
            public string CountryNames { get; set; }
            [Display(Name = "Funds")]
            public string FundNames { get; set; }
            [Display(Name = "Discretionary Percentage")]
            public decimal DiscretionaryPercentage { get; set; }
            [Display(Name = "Start Date")]
            public string StartDate { get; set; }
            [Display(Name = "End Date")]
            public string EndDateDisplay { get; set; }
            [Display(Name = "Active")]
            public string Active { get; set; }
        }


		private ActionResult RedirectToIndex()
		{
			return RedirectToAction("Index");
		}
		private void NewMethod(IEnumerable<int> cIds, IEnumerable<int> fIds, EmergencyCap cap)
		{
			if (cIds == null)
			{
				cap.Countries.Clear();
			}
			else
			{
				foreach (var country in cap.Countries.ToList())
				{
					if (!cIds.Contains(country.Id))
						cap.Countries.Remove(country);
				}

				foreach (var countryId in cIds)
				{
					if (!cap.Countries.Any(f => f.Id == countryId))
					{
						var country = new Country() { Id = countryId };
						db.Countries.Attach(country);
						cap.Countries.Add(country);
					}
				}
			}

			if (fIds == null)
			{
				cap.Funds.Clear();
			}
			else
			{
				foreach (var fund in cap.Funds.ToList())
				{
					if (!fIds.Contains(fund.Id))
					{
						cap.Funds.Remove(fund);
					}
				}

				foreach (var fundId in fIds)
				{
					if (!cap.Funds.Any(f => f.Id == fundId))
					{
						var fund = new Fund() { Id = fundId };
						db.Funds.Attach(fund);
						cap.Funds.Add(fund);
					}
				}
			}
		}
		private void FetchRelationships(EmergencyCap cap)
		{
			ViewBag.Countries = db.Countries.Select(f => new { Id = f.Id, Name = f.Name, Selected = f.EmergencyCaps.Any(ec => ec.Id == cap.Id) })
						 .OrderBy(f => f.Name)
						 .ToList().Select(c =>
						 new SelectListItem()
						 {
							 Value = c.Id.ToString(),
							 Text = c.Name,
							 Selected = c.Selected
						 });
			ViewBag.Funds = db.Funds.Select(f => new { Id = f.Id, Name = f.Name, Selected = f.EmergencyCaps.Any(ec => ec.Id == cap.Id) })
				.OrderBy(f => f.Name)
				.ToList().Select(c =>
				new SelectListItem()
				{
					Value = c.Id.ToString(),
					Text = c.Name,
					Selected = c.Selected
				});
			ViewBag.CurrencyId = new SelectList(db.Currencies.Select(f => new { Id = f.Id, Name = f.Name }), "Id", "Name", cap.CurrencyId);
		}
		


	}
}
