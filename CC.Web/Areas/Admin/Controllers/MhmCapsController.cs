using CC.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CC.Web.Areas.Admin.Controllers
{
	[CcAuthorize(CC.Data.FixedRoles.Admin)]
	public class MhmCapsController : AdminControllerBase
	{
		private readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(MhmCapsController));

		[HttpGet]
		public ActionResult Index()
		{
			var q = from mhmcap in db.MhmCaps
					select mhmcap;

			return View(q);
		}

		[HttpGet]
		public ActionResult Create()
		{
			var cap = new MhmCap();

			FetchRelationships(cap);

			return View("Edit", cap);
		}

		[HttpPost]
		public ActionResult Create(MhmCap cap, IEnumerable<int> cIds, IEnumerable<int> fIds)
		{



			cap.UpdatedAt = DateTime.Now;
			cap.UpdatedBy = this.Permissions.User.Id;

			if (ModelState.IsValid)
			{

				try
				{
					db.MhmCaps.AddObject(cap);
					foreach (var country in cap.Countries)
					{
						db.ObjectStateManager.ChangeObjectState(country, System.Data.EntityState.Unchanged);
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

			var cap = db.MhmCaps.Single(f => f.Id == id);

			FetchRelationships(cap);



			return View(cap);
		}

		[HttpPost]
		public ActionResult Edit(MhmCap input)
		{
			db.ContextOptions.LazyLoadingEnabled = false;
			db.ContextOptions.ProxyCreationEnabled = false;

			var cap = db.MhmCaps.Include(f => f.Countries).Include(f => f.Funds).Single(f => f.Id == input.Id);
			var entry = db.ObjectStateManager.GetObjectStateEntry(cap);

			db.ApplyCurrentValues<MhmCap>(entry.EntitySet.Name, input);

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
			var ec = db.MhmCaps.Single(f => f.Id == id);

			return View(ec);
		}

		[HttpGet]
		public ActionResult Delete(int id)
		{
			var cap = db.MhmCaps.Single(f => f.Id == id);
			foreach (var c in cap.Countries.ToList())
			{
				cap.Countries.Remove(c);
			}
			foreach (var f in cap.Funds.ToList())
			{
				cap.Funds.Remove(f);
			}
			db.MhmCaps.Attach(cap);
			db.MhmCaps.DeleteObject(cap);
			db.SaveChanges();
			return RedirectToIndex();
		}

		public ActionResult Export()
		{
			var ecaps = (from ecap in db.MhmCaps.ToList()
						 select new MhmCapsListRow
						 {
							 Name = ecap.Name,
							 CapPerPerson = ecap.CapPerPerson,
							 CurrencyId = ecap.CurrencyId,
							 CountryNames = string.Join(", ", ecap.CountryNames),
							 FundNames = string.Join(", ", ecap.FundNames),
							 StartDate = ecap.StartDate.ToString("dd MMM yyyy"),
							 EndDateDisplay = ecap.EndDateDisplay.HasValue ? ecap.EndDateDisplay.Value.ToString("dd MMM yyyy") : "N/A",
							 Active = ecap.Active ? "True" : "False"
						 });
			return this.Excel("Mhm Caps", "Sheet1", ecaps);
		}

		private class MhmCapsListRow
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
		private void NewMethod(IEnumerable<int> cIds, IEnumerable<int> fIds, MhmCap cap)
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
		private void FetchRelationships(MhmCap cap)
		{
			ViewBag.Countries = db.Countries.Select(f => new { Id = f.Id, Name = f.Name, Selected = f.MhmCaps.Any(ec => ec.Id == cap.Id) })
						 .OrderBy(f => f.Name)
						 .ToList().Select(c =>
						 new SelectListItem()
						 {
							 Value = c.Id.ToString(),
							 Text = c.Name,
							 Selected = c.Selected
						 });
			ViewBag.Funds = db.Funds.Select(f => new { Id = f.Id, Name = f.Name, Selected = f.MhmCaps.Any(ec => ec.Id == cap.Id) })
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
