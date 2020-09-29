using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CC.Data;
using CC.Web.Helpers;

namespace CC.Web.Areas.Admin.Controllers
{
    [CcAuthorize(CC.Data.FixedRoles.Admin)]
    public class AgenciesController : AdminControllerBase
    {
        //
        // GET: /Admin/Agencies/

        public ViewResult Index()
        {
			return View();
        }
		public JsonResult IndexData(CC.Web.Models.jQueryDataTableParamModel p)
		{
			var source = from ag in db.Agencies.Where(this.Permissions.AgencyFilter)
						 select new
						 {
							 Id = ag.Id,
							 Name = ag.Name,
							 Ser = ag.AgencyGroup.Name
						 };

			var sSortCol_0 = Request["mDataProp_" + p.iSortCol_0];
			var bSortAsc_0 = p.sSortDir_0 == "asc";

			var filtered = source;
			if (!string.IsNullOrEmpty(p.sSearch))
			{
				filtered = filtered.Where(f => System.Data.Objects.SqlClient.SqlFunctions.StringConvert((decimal)f.Id).Trim().Equals(p.sSearch)
					|| f.Name.Contains(p.sSearch)
					|| f.Ser.Contains(p.sSearch)
				);
			}

			var data = filtered.OrderByField(sSortCol_0, bSortAsc_0).Skip(p.iDisplayStart).Take(p.iDisplayLength);

			var result = new CC.Web.Models.jQueryDataTableResult()
			{
				aaData = data,
				sEcho = p.sEcho,
				iTotalRecords = source.Count(),
				iTotalDisplayRecords = filtered.Count()
			};

			return this.MyJsonResult(result);
		}


        //
        // GET: /Admin/Agencies/Details/5

        public ViewResult Details(int id)
        {
            Agency agency = db.Agencies.Single(a => a.Id == id);
            return View(agency);
        }

        //
        // GET: /Admin/Agencies/Create

        public ActionResult Create()
        {
            ViewBag.GroupId = new SelectList(db.AgencyGroups.OrderBy(f=>f.Name), "Id", "Name");
            return View();
        } 

        //
        // POST: /Admin/Agencies/Create

        [HttpPost]
        public ActionResult Create(Agency agency)
        {
            if (ModelState.IsValid)
            {
                db.Agencies.AddObject(agency);
                db.SaveChanges();
				if (!string.IsNullOrEmpty(GlobalDbSettings.GetString(GlobalDbSettings.GlobalStringNames.NewSerOrgNotifyEmail)))
				{
					EmailHelper.SendNewSerOrgNotification(false, agency.Id, agency.Name);
				}
                return RedirectToAction("Index");  
            }

			ViewBag.GroupId = new SelectList(db.AgencyGroups.OrderBy(f => f.Name), "Id", "Name", agency.GroupId);
            return View(agency);
        }
        
        //
        // GET: /Admin/Agencies/Edit/5
 
        public ActionResult Edit(int id)
        {
            Agency agency = db.Agencies.Single(a => a.Id == id);
			ViewBag.GroupId = new SelectList(db.AgencyGroups.OrderBy(f => f.Name), "Id", "Name", agency.GroupId);
            return View(agency);
        }

        //
        // POST: /Admin/Agencies/Edit/5

        [HttpPost]
        public ActionResult Edit(Agency agency)
        {
            if (ModelState.IsValid)
            {
                db.Agencies.Attach(agency);
                db.ObjectStateManager.ChangeObjectState(agency, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
			ViewBag.GroupId = new SelectList(db.AgencyGroups.OrderBy(f => f.Name), "Id", "Name", agency.GroupId);
            return View(agency);
        }

        //
        // GET: /Admin/Agencies/Delete/5
 
        public ActionResult Delete(int id)
        {
            Agency agency = db.Agencies.Single(a => a.Id == id);
            return View(agency);
        }

        //
        // POST: /Admin/Agencies/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            Agency agency = db.Agencies.Single(a => a.Id == id);
            db.Agencies.DeleteObject(agency);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Export()
        {
            var agencies = (from a in db.Agencies.Include("AgencyGroup").ToList()
                            select new AgenciesListRow
                            {
                                Id = a.Id,
                                Name = a.Name,
                                Ser = a.AgencyGroup.Name
                            }).OrderBy(f => f.Id);
            return this.Excel("Agencies", "Sheet1", agencies);
        }

        private class AgenciesListRow
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Ser { get; set; }
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}