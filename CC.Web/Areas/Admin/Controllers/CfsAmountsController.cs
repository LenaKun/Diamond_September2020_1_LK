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
	[CcAuthorize(CC.Data.FixedRoles.Admin, CC.Data.FixedRoles.CfsAdmin)]
	public class CfsAmountsController : AdminControllerBase
    {
		private readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(EmergencyCapsController));


		[HttpGet]
		public ActionResult Index()
		{
			var q = from cfsamount in db.CfsAmounts
					select cfsamount;

			return View(q);
		}

		[CcAuthorize(CC.Data.FixedRoles.Admin)]
		[HttpGet]
		public ActionResult Create()
		{
			var cfsamount = new CfsAmount();

			FetchRelationships(cfsamount);

			return View("Edit", cfsamount);
		}

		[CcAuthorize(CC.Data.FixedRoles.Admin)]
		[HttpPost]
		public ActionResult Create(CfsAmount cfsamount, IEnumerable<int> cIds)
		{
			if (ModelState.IsValid)
			{
				try
				{
					db.CfsAmounts.AddObject(cfsamount);
                    foreach (var country in cfsamount.Countries)
                    {
                        db.ObjectStateManager.ChangeObjectState(country, System.Data.EntityState.Unchanged);
                    }
                    db.SaveChanges();
					return this.RedirectToAction("Index");
				}
				catch (Exception ex)
				{
					_log.Error(ex, ex);
					var msg = ex.Message;
					if(ex.InnerException != null)
					{
						var inner = ex.InnerException;
						while(inner.InnerException != null)
						{
							inner = inner.InnerException;
						}
						msg = inner.Message;
					}
					if (msg.Contains("Violation of UNIQUE KEY"))
					{
						msg = "There is a duplicate CFS amount for this Year, CUR and Level";
					}
					ModelState.AddModelError("", msg);
				}

			}
			FetchRelationships(cfsamount);
			return View("Edit", cfsamount);

		}

		[CcAuthorize(CC.Data.FixedRoles.Admin)]
		[HttpGet]
		public ActionResult Edit(int id)
		{
			db.ContextOptions.LazyLoadingEnabled = false;
			db.ContextOptions.ProxyCreationEnabled = false;

			var cfsamount = db.CfsAmounts.Single(f => f.Id == id);
			FetchRelationships(cfsamount);
			return View(cfsamount);
		}

		[CcAuthorize(CC.Data.FixedRoles.Admin)]
		[HttpPost]
		public ActionResult Edit(CfsAmount input)
		{
			db.ContextOptions.LazyLoadingEnabled = false;
			db.ContextOptions.ProxyCreationEnabled = false;

			var cfsamount = db.CfsAmounts.Include(f => f.Countries).Single(f => f.Id == input.Id);
			var entry = db.ObjectStateManager.GetObjectStateEntry(cfsamount);

			db.ApplyCurrentValues<CfsAmount>(entry.EntitySet.Name, input);

            NewMethod(input.cIds, cfsamount);

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
					if (msg.Contains("Violation of UNIQUE KEY"))
					{
						msg = "There is a duplicate CFS amount for this Year, CUR and Level";
					}
					ModelState.AddModelError("", msg);
				}
			}
			FetchRelationships(cfsamount);
			return View("Edit", cfsamount);
		}

		[CcAuthorize(CC.Data.FixedRoles.Admin)]
		[HttpGet]
		public ActionResult Copy(int id)
		{
			db.ContextOptions.LazyLoadingEnabled = false;
			db.ContextOptions.ProxyCreationEnabled = false;

			var cfsamount = db.CfsAmounts.Include(f => f.Countries).Single(f => f.Id == id);
			var copy = new CfsAmount();
			copy.Year = cfsamount.Year + 1;
			copy.Level = cfsamount.Level;
			copy.CurrencyId = cfsamount.CurrencyId;
			copy.Amount = cfsamount.Amount;
            foreach(var c in cfsamount.Countries)
            {
                copy.Countries.Add(c);
            }
			try
			{
				db.CfsAmounts.AddObject(copy);
				db.SaveChanges();
				return this.RedirectToAction("Index");
			}
			catch (Exception ex)
			{
				_log.Error(ex, ex);
				throw;
			}
		}

		[HttpGet]
		public ActionResult Details(int id)
		{
			var cfsamount = db.CfsAmounts.Single(f => f.Id == id);
			return View(cfsamount);
		}

		[CcAuthorize(CC.Data.FixedRoles.Admin)]
		[HttpGet]
		public ActionResult Delete(int id)
		{
			var cfsamount = db.CfsAmounts.Single(f => f.Id == id);
            foreach (var c in cfsamount.Countries.ToList())
            {
                cfsamount.Countries.Remove(c);
            }
            db.CfsAmounts.Attach(cfsamount);
			db.CfsAmounts.DeleteObject(cfsamount);
			db.SaveChanges();
			return RedirectToIndex();
		}

		public ActionResult Export()
		{
			var cfsamounts = (from cfsamount in db.CfsAmounts.ToList()
							  select new CfsAmountsListRow
							 {
								 Year = cfsamount.Year,
								 CurrencyId = cfsamount.CurrencyId,
                                 CountryNames = string.Join(", ", cfsamount.CountryNames),
								 Level = cfsamount.Level,
								 Amount = cfsamount.Amount
							 });
			return this.Excel("CFS Amounts", "Sheet1", cfsamounts);
		}

		private class CfsAmountsListRow
		{
			[Display(Name = "Year")]
			public int Year { get; set; }
			[Display(Name = "CUR")]
			public string CurrencyId { get; set; }
            [Display(Name = "Countries")]
            public string CountryNames { get; set; }
            [Display(Name = "Level")]
			public int Level { get; set; }
			[Display(Name = "Amount")]
			public decimal Amount { get; set; }
		}


		private ActionResult RedirectToIndex()
		{
			return RedirectToAction("Index");
		}

        private void NewMethod(IEnumerable<int> cIds, CfsAmount cfs)
        {
            if (cIds == null)
            {
                cfs.Countries.Clear();
            }
            else
            {
                foreach (var country in cfs.Countries.ToList())
                {
                    if (!cIds.Contains(country.Id))
                        cfs.Countries.Remove(country);
                }

                foreach (var countryId in cIds)
                {
                    if (!cfs.Countries.Any(f => f.Id == countryId))
                    {
                        var country = new Country() { Id = countryId };
                        db.Countries.Attach(country);
                        cfs.Countries.Add(country);
                    }
                }
            }
        }
        private void FetchRelationships(CfsAmount cfsamount)
		{
			var currentYear = DateTime.Today.Year;
			List<int> _years = new List<int>();
			for (int i = currentYear; i < currentYear + 5; i++ )
			{
				_years.Add(i);
			}
			cfsamount.Years = _years.Select(f => new SelectListItem() { Text = f.ToString(), Value = f.ToString() });
			List<int> _levels = new List<int>();
			for (int i = 1; i < 8; i++)
			{
				_levels.Add(i);
			}
			cfsamount.Levels = _levels.Select(f => new SelectListItem() { Text = f.ToString(), Value = f.ToString() });
			cfsamount.Currencies = CC.Data.Currency.ConvertableCurrencies.Select(f => new SelectListItem() { Text = f, Value = f });
            ViewBag.Countries = db.Countries.Select(f => new { Id = f.Id, Name = f.Name, Selected = f.CfsAmounts.Any(ca => ca.Id == cfsamount.Id) })
                         .OrderBy(f => f.Name)
                         .ToList().Select(c =>
                         new SelectListItem()
                         {
                             Value = c.Id.ToString(),
                             Text = c.Name,
                             Selected = c.Selected
                         });
        }
    }
}
