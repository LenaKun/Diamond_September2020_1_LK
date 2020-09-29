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
    public class ServiceTypesController : AdminControllerBase
    {
        //
        // GET: /Admin/ServiceTypes/

        public ViewResult Index()
        {
            return View(db.ServiceTypes.ToList());
        }

        //
        // GET: /Admin/ServiceTypes/Details/5

        public ViewResult Details(int id)
        {
            ServiceType servicetype = db.ServiceTypes.Single(s => s.Id == id);
            return View(servicetype);
        }

        //
        // GET: /Admin/ServiceTypes/Edit/5
 
        public ActionResult Edit(int id)
        {
            ServiceType servicetype = db.ServiceTypes.Single(s => s.Id == id);
            return View(servicetype);
        }

        //
        // POST: /Admin/ServiceTypes/Edit/5

        [HttpPost]
        public ActionResult Edit(ServiceType input)
        {
			var servicetype = db.ServiceTypes.Single(s => s.Id == input.Id);
            if (ModelState.IsValid)
            {
				servicetype.Name = input.Name;
				if (servicetype.DefaultConstraint == null)
				{
					servicetype.DefaultConstraint = new ServiceConstraint
					{
						ServiceTypeId = servicetype.Id,
						ServiceId = null,
						FundId = null
					};
				}

				servicetype.DefaultConstraint.MinExpPercentage = input.DefaultConstraint.MinExpPercentage;
				servicetype.DefaultConstraint.MaxExpPercentage = input.DefaultConstraint.MaxExpPercentage;
				servicetype.DoNotReportInUnmetNeedsOther = input.DoNotReportInUnmetNeedsOther;
				servicetype.ServiceTypeImportId = input.ServiceTypeImportId;

				try
				{
					db.SaveChanges();
				}
				catch(Exception ex)
				{
					string msg = ex.Message;
					if(ex.InnerException != null)
					{
						var inner = ex.InnerException;
						while(inner.InnerException != null)
						{
							inner = inner.InnerException;
						}
						msg = inner.Message;
					}
					if (msg.Contains("UQ_ServiceTypes_ImportId"))
					{
						msg = "Service Type Import Id must be unique";
					}
					else if (msg.Contains("UQ_ServiceTypes"))
					{
						msg = "Name must be unique";
					}					
					ModelState.AddModelError(string.Empty, msg);
					return View(input);
				}
                return RedirectToAction("Index");
            }
            return View(servicetype);
        }

        //
        // POST: /Admin/ServiceTypes/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            ServiceType servicetype = db.ServiceTypes.Single(s => s.Id == id);
            db.ServiceTypes.DeleteObject(servicetype);
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