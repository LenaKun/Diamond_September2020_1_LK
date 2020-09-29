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
    public class UserAgreementsController : AdminControllerBase
    {
        //
        // GET: /Admin/UserAgreements/

        public ViewResult Index()
        {
            return View(db.UserAgreements.ToList());
        }

        //
        // GET: /Admin/UserAgreements/Details/5

        public ViewResult Details(int id)
        {
            UserAgreement useragreement = db.UserAgreements.Single(u => u.Id == id);
            return View(useragreement);
        }

        //
        // GET: /Admin/UserAgreements/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Admin/UserAgreements/Create

        [HttpPost]
        public ActionResult Create(UserAgreement useragreement)
        {
            if (ModelState.IsValid)
            {
                db.UserAgreements.AddObject(useragreement);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View(useragreement);
        }
        
        //
        // GET: /Admin/UserAgreements/Edit/5
 
        public ActionResult Edit(int id)
        {
            UserAgreement useragreement = db.UserAgreements.Single(u => u.Id == id);
            return View(useragreement);
        }

        //
        // POST: /Admin/UserAgreements/Edit/5

        [HttpPost]
        public ActionResult Edit(UserAgreement useragreement)
        {
            if (ModelState.IsValid)
            {
                db.UserAgreements.Attach(useragreement);
                db.ObjectStateManager.ChangeObjectState(useragreement, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(useragreement);
        }

        //
        // GET: /Admin/UserAgreements/Delete/5
 
        public ActionResult Delete(int id)
        {
            UserAgreement useragreement = db.UserAgreements.Single(u => u.Id == id);
            return View(useragreement);
        }

        //
        // POST: /Admin/UserAgreements/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            UserAgreement useragreement = db.UserAgreements.Single(u => u.Id == id);
            db.UserAgreements.DeleteObject(useragreement);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}