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
    public class RegionsController : AdminControllerBase
    {
        //
        // GET: /Admin/Default1/

        public ViewResult Index()
        {
            return View(db.Regions.Where(f => f.Id != 0).ToList());
        }

        //
        // GET: /Admin/Default1/Details/5

        public ViewResult Details(int id)
        {
            Region region = db.Regions.Single(r => r.Id == id);
            return View(region);
        }

        //
        // GET: /Admin/Default1/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Admin/Default1/Create

        [HttpPost]
        public ActionResult Create(Region region)
        {
            if (ModelState.IsValid)
            {
                db.Regions.AddObject(region);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View(region);
        }
        
        //
        // GET: /Admin/Default1/Edit/5
 
        public ActionResult Edit(int id)
        {
            Region region = db.Regions.Single(r => r.Id == id);
            return View(region);
        }

        //
        // POST: /Admin/Default1/Edit/5

        [HttpPost]
        public ActionResult Edit(Region region)
        {
            if (ModelState.IsValid)
            {
                db.Regions.Attach(region);
                db.ObjectStateManager.ChangeObjectState(region, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(region);
        }

        //
        // GET: /Admin/Default1/Delete/5
 
        public ActionResult Delete(int id)
        {
            Region region = db.Regions.Single(r => r.Id == id);
            return View(region);
        }

        //
        // POST: /Admin/Default1/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            Region region = db.Regions.Single(r => r.Id == id);
            db.Regions.DeleteObject(region);
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