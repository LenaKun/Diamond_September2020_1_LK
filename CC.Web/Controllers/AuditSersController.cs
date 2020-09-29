using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CC.Web.Areas.Admin.Controllers
{
	[CcAuthorize(CC.Data.FixedRoles.Admin, CC.Data.FixedRoles.AuditorReadOnly)]
	public class AuditSersController : AdminControllerBase
	{
		public ViewResult Index()
		{
			ViewBag.Sers = db.AgencyGroups.OrderBy(f => f.Name).Select(f => new
				{
					Id= f.Id,
					Name = f.Name,
					Selected = f.IsAudit
				}).ToList().Select(f => new SelectListItem() { Text = f.Name, Value = f.Id.ToString(), Selected = f.Selected });
			return View();
		}

		[HttpPost]
		public ActionResult Index(IEnumerable<string> sids)
		{
			var sers = sids.Select(f => f.Parse<int>()).Where(f => f.HasValue).Select(f => f.Value);
			foreach(var sid in sers)
			{
				var ser = db.AgencyGroups.SingleOrDefault(f => f.Id == sid);
				if(ser != null)
				{
					ser.IsAudit = true;
				}
			}
			var unCheckedSers = db.AgencyGroups.Where(f => f.IsAudit && !sers.Contains(f.Id));
			foreach (var ser in unCheckedSers)
			{
				ser.IsAudit = false;
			}
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
					while(inner != null)
					{
						inner = inner.InnerException;
					}
					msg = inner.Message;
				}
				ModelState.AddModelError(string.Empty, msg);
				ViewBag.Sers = db.AgencyGroups.OrderBy(f => f.Name).Select(f => new
				{
					Id = f.Id,
					Name = f.Name,
					Selected = f.IsAudit
				}).ToList().Select(f => new SelectListItem() { Text = f.Name, Value = f.Id.ToString(), Selected = f.Selected });
				return View();
			}
			return RedirectToAction("Index");
		}
	}
}