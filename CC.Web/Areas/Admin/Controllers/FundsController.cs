using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CC.Data;

namespace CC.Web.Areas.Admin.Controllers
{
    [CcAuthorize(CC.Data.FixedRoles.Admin)]
	public class FundsController : AdminControllerBase
	{
		//
		// GET: /Admin/Funds/

		public ViewResult Index()
		{
			var funds = db.Funds.Include("Currency").Include("MasterFund");
			return View(funds.ToList());
		}

		//
		// GET: /Admin/Funds/Details/5

		public ViewResult Details(int id)
		{
			Fund fund = db.Funds
				.Include(f => f.FundExchangeRates)
				.Single(f => f.Id == id);
			return View(fund);
		}

		//
		// GET: /Admin/Funds/Create

		public ActionResult Create()
		{
			var model = new Fund();
			FetchViewbagItems(null);

			return View(model);
		}

		//
		// POST: /Admin/Funds/Create

		[HttpPost]
		public ActionResult Create(Fund fund)
		{
            if(fund.CurrencyCode == null)
            {
                ModelState.AddModelError("", "Please select currency");
            }
			if (ModelState.IsValid)
			{
				GetExRatesFromRequest(fund);
				db.Funds.AddObject(fund);
				db.SaveChanges();
				return RedirectToAction("Index");
			}

			FetchViewbagItems(fund);
			return View(fund);
		}

		private void FetchViewbagItems(Fund fund)
		{
			ViewBag.CurrencyCode = new SelectList(db.Currencies.OrderBy(f => f.Name), "Id", "Name", fund == null ? null : fund.CurrencyCode);
			ViewBag.MasterFundId = new SelectList(db.MasterFunds.OrderBy(f => f.Name), "Id", "Name", fund == null ? (int?)null : fund.MasterFundId);
		}

		//
		// GET: /Admin/Funds/Edit/5

		public ActionResult Edit(int id)
		{

			Fund fund = db.Funds
				.Single(f => f.Id == id);

			FetchViewbagItems(fund);
			return View(fund);
		}

		//
		// POST: /Admin/Funds/Edit/5

		[HttpPost]
		public ActionResult Edit(Fund input)
		{


			if (ModelState.IsValid)
			{
				var fund = db.Funds.Include(f => f.FundExchangeRates).SingleOrDefault(f => f.Id == input.Id);

				if (fund.MasterFundId != input.MasterFundId) { fund.MasterFundId = input.MasterFundId; }
				if (fund.Name != input.Name) { fund.Name = input.Name; }
				if (fund.StartDate != input.StartDate) { fund.StartDate = input.StartDate; }
				if (fund.EndDate != input.EndDate) { fund.EndDate = input.EndDate; }
				if (fund.Amount != input.Amount) { fund.Amount = input.Amount; }
				if (fund.CurrencyCode != input.CurrencyCode) { fund.CurrencyCode = input.CurrencyCode; }
				if (fund.OtherServicesMax != input.OtherServicesMax) { fund.OtherServicesMax = input.OtherServicesMax; }
				if (fund.HomecareMin != input.HomecareMin) { fund.HomecareMin = input.HomecareMin; }
				if (fund.AdminMax != input.AdminMax) { fund.AdminMax = input.AdminMax; }
				if (fund.AustrianEligibleOnly != input.AustrianEligibleOnly) { fund.AustrianEligibleOnly = input.AustrianEligibleOnly; }
				if (fund.RomanianEligibleOnly != input.RomanianEligibleOnly) { fund.RomanianEligibleOnly = input.RomanianEligibleOnly; }
				GetExRatesFromRequest(fund);
				db.SaveChanges();
				return RedirectToAction("Index");
			}
			FetchViewbagItems(input);
			return View(input);
		}

		private void GetExRatesFromRequest(Fund fund)
		{
			var currencies = db.Currencies.ToList();
			foreach (var currency in currencies)
			{

				decimal rate;
				if (decimal.TryParse(Request[currency.Id], out rate))
				{
					var fundExRate = fund.FundExchangeRates.SingleOrDefault(f => f.CurId == currency.Id);
					if (fundExRate == null)
					{
						fundExRate = new FundExchangeRate { FundId = fund.Id, CurId = currency.Id };
						fund.FundExchangeRates.Add(fundExRate);
					}
					fundExRate.Value = rate;
				}
			}

		}

		//
		// GET: /Admin/Funds/Delete/5

		public ActionResult Delete(int id)
		{
			Fund fund = db.Funds.Single(f => f.Id == id);
			return View(fund);
		}

		//
		// POST: /Admin/Funds/Delete/5

		[HttpPost, ActionName("Delete")]
		public ActionResult DeleteConfirmed(int id)
		{
			Fund fund = db.Funds.Single(f => f.Id == id);
			db.Funds.DeleteObject(fund);
			db.SaveChanges();
			return RedirectToAction("Index");
		}

        public ActionResult Export()
        {
            var funds = (from f in db.Funds.Include("Currency").Include("MasterFund").ToList()
                               select new FundsListRow
                               {
                                   Id = f.Id,
                                   MasterFund = f.MasterFund.Name,
                                   Name = f.Name,
                                   StartDate = f.StartDate.ToString("dd MMM yyyy"),
                                   EndDate = f.EndDate.ToString("dd MMM yyyy"),
                                   Amount = f.Amount,
                                   Currency = f.Currency.Name
                               });
            return this.Excel("Funds", "Sheet1", funds);
        }

		protected override void Dispose(bool disposing)
		{
			db.Dispose();
			base.Dispose(disposing);
		}

        private class FundsListRow
        {
            public int Id { get; set; }
            public string MasterFund { get; set; }
            public string Name { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }
            public decimal Amount { get; set; }
            public string Currency { get; set; }
        }
	}
}