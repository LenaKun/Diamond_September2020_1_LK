using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CC.Data;
using System.Diagnostics;


namespace CC.Web.Areas.Admin.Controllers
{

   
[CcAuthorize(CC.Data.FixedRoles.Admin)]
    public class ServicesController : AdminControllerBase
    {

       public static  List<string> Fluxx_Admin_List = (System.Configuration.ConfigurationManager.AppSettings["Fluxx_Admin_List"] ?? "")
            .Split(',').Select(h => h.ToLower().Trim()).ToList();

            
    public ViewResult Index()
        {
            var services = db.Services.Include("ServiceType");
            return View(services.ToList());
        }

        //
        // GET: /Admin/Services/Details/5

        public ViewResult Details(int id)
        {
            Service service = db.Services.Single(s => s.Id == id);
            ViewBag.IsFluxxAdmin = Fluxx_Admin_List.Any(l => l == CcUser.UserName);
            return View(service);
        }

        //
        // GET: /Admin/Services/Create

        public ActionResult Create()
        {
            ViewBag.TypeId = new SelectList(db.ServiceTypes, "Id", "Name");
            ViewBag.ReportingMethodId = EnumExtensions.ToSelectList<Service.ReportingMethods>();
            ViewBag.IsFluxxAdmin = Fluxx_Admin_List.Any(l => l == CcUser.UserName);
            return View();
        }

        //
        // POST: /Admin/Services/Create

        [HttpPost]
        public ActionResult Create(Service service)
        {
           
            if (ModelState.IsValid)
            {
                if (service.DefaultConstraint.MinExpPercentage == null && service.DefaultConstraint.MaxExpPercentage == null)
                {
                    service.DefaultConstraint = null;
                }

				if(!service.ExceptionalHomeCareHours && service.CoPGovHoursValidation)
				{
					service.CoPGovHoursValidation = false;
				}
                

                db.Services.AddObject(service);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.TypeId = new SelectList(db.ServiceTypes, "Id", "Name", service.TypeId);
            ViewBag.ReportingMethodId = EnumExtensions.ToSelectList<Service.ReportingMethods>(service.ReportingMethodId);
            ViewBag.IsFluxxAdmin = Fluxx_Admin_List.Any(l => l == CcUser.UserName); 

            return View(service);
        }

        
        public ActionResult Edit(int id)
        {
            Service service = db.Services.Single(s => s.Id == id);
            service.DefaultConstraint = db.ServiceConstraints.Where(f => f.ServiceId == id && f.FundId == null).SingleOrDefault();

            ViewBag.TypeId = new SelectList(db.ServiceTypes, "Id", "Name", service.TypeId);
            ViewBag.IsFluxxAdmin = Fluxx_Admin_List.Any(l => l == CcUser.UserName);
            ViewBag.ReportingMethodId = EnumExtensions.ToSelectList<Service.ReportingMethods>(service.ReportingMethodId);
            return View(service);
        }

      
        [HttpPost]
        public ActionResult Edit(Service input)
        {
            Service service = db.Services.Single(s => s.Id == input.Id);
            service.DefaultConstraint = db.ServiceConstraints.Where(f => f.ServiceId == input.Id && f.FundId == null).SingleOrDefault();
            var IsFluxxAdmin = Fluxx_Admin_List.Any(l => l == CcUser.UserName);

            if (ModelState.IsValid)
            {
                service.Name = input.Name;

                if (IsFluxxAdmin)
                { 
                   service.FluxxFieldName = input.FluxxFieldName;
                }

                service.ReportingMethodId = input.ReportingMethodId;
                service.TypeId = input.TypeId;
                service.EnforceTypeConstraints = input.EnforceTypeConstraints;
                service.SingleClientPerYearAgency = input.SingleClientPerYearAgency;
                service.ServiceLevel = input.ServiceLevel;
                service.ExceptionalHomeCareHours = input.ExceptionalHomeCareHours;
				if(!input.ExceptionalHomeCareHours && input.CoPGovHoursValidation)
				{
					service.CoPGovHoursValidation = false;
				}
				else
				{
					service.CoPGovHoursValidation = input.CoPGovHoursValidation;
				}
				service.Active = input.Active;
                service.Personnel = input.Personnel;

                if (input.DefaultConstraint.MinExpPercentage == null && input.DefaultConstraint.MaxExpPercentage == null)
                {
                    if (service.DefaultConstraint != null)
                    {
                        db.ServiceConstraints.DeleteObject(service.DefaultConstraint);
                        input.DefaultConstraint = null;
                    }
                }
                else
                {
                    if (service.DefaultConstraint == null)
                    {
                        service.DefaultConstraint = new ServiceConstraint
                        {
                            FundId = null,
                            ServiceId = service.Id,
                            ServiceTypeId = null
                        };
                        db.ServiceConstraints.AddObject(service.DefaultConstraint);
                    }
                    service.DefaultConstraint.MinExpPercentage = input.DefaultConstraint.MinExpPercentage;
                    service.DefaultConstraint.MaxExpPercentage = input.DefaultConstraint.MaxExpPercentage;
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.TypeId = new SelectList(db.ServiceTypes, "Id", "Name", service.TypeId);
            ViewBag.IsFluxxAdmin = IsFluxxAdmin;
            ViewBag.ReportingMethodId = EnumExtensions.ToSelectList<Service.ReportingMethods>(service.ReportingMethodId);
            return View(service);
        }

        [HttpPost]
        public ActionResult AllowToAllApps(int id)
        {
            var q = db.Apps.Where(f => !f.Services.Any(s => s.Id == id));
            var service = new Service { Id = id };
            db.Services.Attach(service);
            foreach (var item in q)
            {
                item.Services.Add(service);
            }
            db.SaveChanges();
            return RedirectToAction("Details", new { id = id });
        }


        //
        // GET: /Admin/Services/Delete/5

        public ActionResult Delete(int id)
        {
            Service service = db.Services.Single(s => s.Id == id);
            return View(service);
        }

        //
        // POST: /Admin/Services/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {

            Service service = db.Services.Single(s => s.Id == id);

            db.Services.DeleteObject(service);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Export()
        {
            var services = (from s in db.Services.Include("ServiceType").ToList()
                            select new ServicesListRow
                            {
                                Name = s.Name,
                                ReportingMethod = s.ReportingMethodEnum.ToString(),
                                ServiceType = s.ServiceType.Name
                            });
            return this.Excel("Services", "Sheet1", services);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        private class ServicesListRow
        {
            public string Name { get; set; }
            public string ReportingMethod { get; set; }
            public string ServiceType { get; set; }
        }
    }
}