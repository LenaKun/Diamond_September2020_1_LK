using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CC.Web.Controllers
{
	public class DefaultController : PrivateCcControllerBase
	{
		// GET: Default
		public ActionResult Index()
		{
			//if (User.IsInRole(CC.Data.FixedRoles.DafEvaluator) || User.IsInRole(CC.Data.FixedRoles.DafReviewer))
			//{
			//	return RedirectToAction("Index", "Daf");
			//}
			//else
			//{
				return RedirectToAction("Index", "LandingPage");
			//}
		}
	}
}