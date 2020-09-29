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
	public class CommPrefsController : AdminControllerBase
    {
		//
		// GET: /Admin/Default1/

		public ViewResult Index()
		{
			return View(db.CommunicationsPreferences.Where(f => f.Id != 0).ToList());
		}

		//
		// GET: /Admin/Default1/Details/5

		public ViewResult Details(int id)
		{
			CommunicationsPreference commPref = db.CommunicationsPreferences.Single(f => f.Id == id);
			return View(commPref);
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
		public ActionResult Create(CommunicationsPreference commPref)
		{
			if (ModelState.IsValid)
			{
				db.CommunicationsPreferences.AddObject(commPref);
				try
				{
					db.SaveChanges();
					return RedirectToAction("Index");
				}
				catch(Exception ex)
				{
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
					if (msg.Contains("UNIQUE"))
					{
						msg = "There is already a Communications Preference with that name";
					}
					ModelState.AddModelError("", msg);
				}
			}

			return View(commPref);
		}

		//
		// GET: /Admin/Default1/Edit/5

		public ActionResult Edit(int id)
		{
			CommunicationsPreference commPref = db.CommunicationsPreferences.Single(f => f.Id == id);
			return View(commPref);
		}

		//
		// POST: /Admin/Default1/Edit/5

		[HttpPost]
		public ActionResult Edit(CommunicationsPreference commPref)
		{
			if (ModelState.IsValid)
			{
				db.CommunicationsPreferences.Attach(commPref);
				db.ObjectStateManager.ChangeObjectState(commPref, EntityState.Modified);
				try
				{
					db.SaveChanges();
					return RedirectToAction("Index");
				}
				catch (Exception ex)
				{
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
					if (msg.Contains("UNIQUE"))
					{
						msg = "There is already a Communications Preference with that name";
					}
					ModelState.AddModelError("", msg);
				}
			}
			return View(commPref);
		}

		//
		// GET: /Admin/Default1/Delete/5

		public ActionResult Delete(int id)
		{
			CommunicationsPreference commPref = db.CommunicationsPreferences.Single(f => f.Id == id);
			return View(commPref);
		}

		//
		// POST: /Admin/Default1/Delete/5

		[HttpPost, ActionName("Delete")]
		public ActionResult DeleteConfirmed(int id)
		{
			CommunicationsPreference commPref = db.CommunicationsPreferences.Single(f => f.Id == id);
			db.CommunicationsPreferences.DeleteObject(commPref);
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
