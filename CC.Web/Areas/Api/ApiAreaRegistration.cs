using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CC.Web.Areas.Api
{
	public class ApiAreaRegistration : System.Web.Mvc.AreaRegistration
	{
		public override string AreaName
		{
			get { return "api"; }
		}
		public override void RegisterArea(AreaRegistrationContext context)
		{
			context.MapRoute(
				"Api_default",
				"api/{controller}/{action}/{id}",
				new { controller = "Values", action = "Get", id = UrlParameter.Optional }, // Parameter defaults,
				new[] { "CC.Web.Areas.Api.Controllers" }
			);
		}
	}
}