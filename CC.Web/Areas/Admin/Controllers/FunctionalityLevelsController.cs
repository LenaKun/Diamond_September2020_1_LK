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
    public class FunctionalityLevelsController : AdminControllerBase
    {
        //
        // GET: /Admin/FunctionalityLevels/

        public ViewResult Index()
        {
            var functionalitylevels = db.FunctionalityLevels.Include("RelatedFunctionalityLevel");
            return View(functionalitylevels.ToList());
        }

        //
        // GET: /Admin/FunctionalityLevels/Details/5

        public ViewResult Details(int id)
        {
            FunctionalityLevel functionalitylevel = db.FunctionalityLevels.Single(f => f.Id == id);
            return View(functionalitylevel);
        }

        //
        // GET: /Admin/FunctionalityLevels/Create

        public ActionResult Create()
        {
            ViewBag.RelatedLevel = new SelectList(db.RelatedFunctionalityLevels, "Id", "Name");
            return View();
        } 

        //
        // POST: /Admin/FunctionalityLevels/Create

        [HttpPost]
        public ActionResult Create(FunctionalityLevel functionalitylevel)
        {
            if (ModelState.IsValid)
            {
                db.FunctionalityLevels.AddObject(functionalitylevel);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            ViewBag.RelatedLevel = new SelectList(db.RelatedFunctionalityLevels, "Id", "Name", functionalitylevel.RelatedLevel);
            return View(functionalitylevel);
        }
        
        //
        // GET: /Admin/FunctionalityLevels/Edit/5
 
        public ActionResult Edit(int id)
        {
            FunctionalityLevel functionalitylevel = db.FunctionalityLevels.Single(f => f.Id == id);
            ViewBag.RelatedLevel = new SelectList(db.RelatedFunctionalityLevels, "Id", "Name", functionalitylevel.RelatedLevel);
            return View(functionalitylevel);
        }

        //
        // POST: /Admin/FunctionalityLevels/Edit/5

        [HttpPost]
        public ActionResult Edit(FunctionalityLevel functionalitylevel)
        {
            if (ModelState.IsValid)
            {
                db.FunctionalityLevels.Attach(functionalitylevel);
                db.ObjectStateManager.ChangeObjectState(functionalitylevel, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.RelatedLevel = new SelectList(db.RelatedFunctionalityLevels, "Id", "Name", functionalitylevel.RelatedLevel);
            return View(functionalitylevel);
        }

        //
        // GET: /Admin/FunctionalityLevels/Delete/5
 
        public ActionResult Delete(int id)
        {
            FunctionalityLevel functionalitylevel = db.FunctionalityLevels.Single(f => f.Id == id);
            return View(functionalitylevel);
        }

        //
        // POST: /Admin/FunctionalityLevels/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            FunctionalityLevel functionalitylevel = db.FunctionalityLevels.Single(f => f.Id == id);
            db.FunctionalityLevels.DeleteObject(functionalitylevel);
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