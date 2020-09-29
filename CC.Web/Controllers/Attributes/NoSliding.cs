using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Security;
using System.Linq;
using System.Web;

namespace System.Web.Mvc
{
	///<summary>
	/// Prevent the auth cookie from being reset for this action, allows you to
	/// have requests that do not reset the login timeout.
	/// </summary>
	public class DoNotResetAuthCookieAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			var response = filterContext.HttpContext.Response;
			//response.Cookies.Remove(FormsAuthentication.FormsCookieName);
		}
	}
}