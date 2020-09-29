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
    public class MasterFundsController : AdminControllerBase
    {
        //
        // GET: /Admin/MasterFunds/

        public ViewResult Index()
        {
            var masterfunds = db.MasterFunds.Include("Currency");
            return View(masterfunds.ToList());
        }

        //
        // GET: /Admin/MasterFunds/Details/5

        public ViewResult Details(int id)
        {
            MasterFund masterfund = db.MasterFunds.Single(m => m.Id == id);
            return View(masterfund);
        }

        //
        // GET: /Admin/MasterFunds/Create

        public ActionResult Create()
        {
            ViewBag.CurrencyCode = new SelectList(db.Currencies, "Id", "Id");
            return View();
        } 

        //
        // POST: /Admin/MasterFunds/Create

        [HttpPost]
        public ActionResult Create(MasterFund masterfund)
        {
            masterfund.CurrencyCode = masterfund.CurrencyCode.ToUpper();

            if (ModelState.IsValid)
            {
                db.MasterFunds.AddObject(masterfund);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            ViewBag.CurrencyCode = new SelectList(db.Currencies.OrderBy(f=>f.Id), "Id", "Id", masterfund.CurrencyCode);
            return View(masterfund);
        }
        
        //
        // GET: /Admin/MasterFunds/Edit/5
 
        public ActionResult Edit(int id)
        {
            MasterFund masterfund = db.MasterFunds.Single(m => m.Id == id);
            ViewBag.CurrencyCode = new SelectList(db.Currencies.OrderBy(f=>f.Id), "Id", "Id", masterfund.CurrencyCode);
            return View(masterfund);
        }

        //
        // POST: /Admin/MasterFunds/Edit/5

        [HttpPost]
        public ActionResult Edit(MasterFund masterfund)
        {
            masterfund.CurrencyCode = masterfund.CurrencyCode.ToUpper();

            if (ModelState.IsValid)
            {
                db.MasterFunds.Attach(masterfund);
                db.ObjectStateManager.ChangeObjectState(masterfund, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CurrencyCode = new SelectList(db.Currencies.OrderBy(f=>f.Id), "Id", "Id", masterfund.CurrencyCode);
            return View(masterfund);
        }

        //
        // GET: /Admin/MasterFunds/Delete/5
 
        public ActionResult Delete(int id)
        {
            MasterFund masterfund = db.MasterFunds.Single(m => m.Id == id);
            return View(masterfund);
        }

        //
        // POST: /Admin/MasterFunds/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            MasterFund masterfund = db.MasterFunds.Single(m => m.Id == id);
            db.MasterFunds.DeleteObject(masterfund);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Export()
        {
            var masterFunds = (from mf in db.MasterFunds.Include("Currency").ToList()
                               select new MasterFundsListRow 
                               {
                                    Name = mf.Name,
                                    StartDate = mf.StartDate.ToString("dd MMM yyyy"),
                                    EndDate = mf.EndDate.ToString("dd MMM yyyy"),
                                    Amount = mf.Amount,
                                    Currency = mf.Currency.Name
                               });
            return this.Excel("Master Funds", "Sheet1", masterFunds);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        private class MasterFundsListRow
        {
            public string Name { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }
            public decimal Amount { get; set; }
            public string Currency { get; set; }
        }
    }
}