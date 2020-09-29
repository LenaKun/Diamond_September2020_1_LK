using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using System.Web.Mvc;
using CC.Data;
using System.Collections.Specialized;

namespace CC.Web.Attributes
{
	/// <summary>
	/// Performs password conformation of currently logged on user before executing a controller action. Default field name = "password"
	/// </summary>
	public class ConfirmPasswordAttribute : ActionFilterAttribute, IActionFilter
	{
		private string fieldKey = null;
		private string UniqueIdFieldName = null;

		/// <summary>
		/// Default constractor
		/// </summary>
		public ConfirmPasswordAttribute()
		{
			this.fieldKey = "password";
		}

		/// <summary>
		/// Initializes a name of the field that contains the password.
		/// </summary>
		/// <param name="passFieldName">The name of the field that contains password. Default = "password"</param>
		public ConfirmPasswordAttribute(string passFieldName = "password", string uniqueIdFiledName = null)
		{
			this.UniqueIdFieldName = uniqueIdFiledName;
			this.fieldKey = passFieldName;
		}

        private string PasswordValueFromHeader(NameValueCollection headers)
        {
            var headerKey = ("x-" + this.fieldKey).ToUpper();
            //try get password from a x-password header - used by daf app - sign daf
            try
            {
                var base64password = headers[headerKey];
                if (base64password != null)
                {
                    var passbytes = Convert.FromBase64String(base64password);
                    var pass = System.Text.Encoding.UTF8.GetString(passbytes);
                    return pass;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
        private string PasswordFromForm(NameValueCollection form)
        {
            return form[this.fieldKey];
        }
        private string ExtractPasswordFromRequest(ActionExecutingContext filterContext)
        {
            if (this.fieldKey == null)
            {
                return null;
            }
            else
            {
                var request = filterContext.HttpContext.Request;
                var headerPass = PasswordValueFromHeader(request.Headers);
                if (headerPass != null)
                {
                    return headerPass;
                }
                var formPass = PasswordFromForm(request.Form);
                if (formPass != null)
                {
                    return formPass;
                }
                return null;
                
            }
        }

		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{

			Guid uniquId;
			bool isPassValid = false;
            var pass = ExtractPasswordFromRequest(filterContext);

			MembershipUser mu = null;

			if (string.IsNullOrWhiteSpace(pass))
			{
				throw new InvalidPasswordException("Password field not found");
			}
			else if (!string.IsNullOrEmpty(UniqueIdFieldName) && Guid.TryParse(filterContext.RequestContext.HttpContext.Request[UniqueIdFieldName], out uniquId))
			{
				using (var db = new ccEntities())
				{
					mu = db.MembershipUsers.SingleOrDefault(f => f.User.UniqueId == uniquId);
				}
			}
			else if (filterContext.RequestContext.HttpContext.User != null && filterContext.RequestContext.HttpContext.User.Identity.IsAuthenticated)
			{
				using (var db = new ccEntities())
				{
					var username = filterContext.RequestContext.HttpContext.User.Identity.Name;
					mu = db.MembershipUsers.Single(f => f.User.UserName == username);
				}
			}

			if (mu != null)
			{
				isPassValid = mu.ValidatePassword(pass);
			}
			else
			{
				isPassValid = false;
			}

			if (!isPassValid)
			{
				throw new InvalidPasswordException();
			}
		}
	}
	public class InvalidPasswordException : Exception
	{
		public InvalidPasswordException() : base() { }
		public InvalidPasswordException(string message) : base(message) { }
	}

	public class MessageActionAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			var modelState = ((Controller)filterContext.Controller).ModelState;
			var qs = filterContext.RequestContext.HttpContext.Request.QueryString;
			if (filterContext.Controller.ViewData["warnings"] == null) filterContext.Controller.ViewData["warnings"] = new List<string>();
			var warnings = filterContext.Controller.ViewData["warnings"] as List<string>;
			foreach (var msg in qs.ToPairs().Where(f => f.Key.Equals("msg", StringComparison.InvariantCultureIgnoreCase)).Select(f => f.Value))
			{
				warnings.Add(msg);

			}

			base.OnActionExecuting(filterContext);
		}
	}

	public class LogActionFilter : ActionFilterAttribute
	{
		private Guid id = Guid.NewGuid();
		DateTime t1 = DateTime.Now;

		public override void OnResultExecuted(ResultExecutedContext filterContext)
		{
			this.Log(filterContext, System.Reflection.MethodBase.GetCurrentMethod().Name);
		}

		private readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(LogActionFilter));
		private void Log(ControllerContext filterContext, string name)
		{
			log.DebugFormat("ExecutedResult Url:{0},\nUsername:{1},\nin:{2},\nValues:{3}",
				filterContext.RequestContext.HttpContext.Request.Url,
			filterContext.HttpContext.User.Identity.Name,
			(DateTime.Now - t1),
			string.Join(", ", filterContext.RouteData.Values.Where(f => !f.Key.Contains("password")).Select(f => f.Key + "=" + f.Value)));
			t1 = DateTime.Now;
		}
	}

	public class LogErrorAttribute : ActionFilterAttribute, IExceptionFilter
	{

		private readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(LogErrorAttribute));

		private void LogException(ExceptionContext filterContext)
		{
			var sb = new System.Text.StringBuilder();
			sb.Append("u:").Append(filterContext.HttpContext.User.Identity.Name).Append(Environment.NewLine);
			sb.Append("url:").Append(filterContext.HttpContext.Request.RawUrl).Append(Environment.NewLine);

			if (filterContext.Exception.GetType() == typeof(InvalidPasswordException))
			{
				_log.Info(sb.ToString(), filterContext.Exception);
			}
			else
			{
				_log.Fatal(sb.ToString(), filterContext.Exception);
			}

		}

		public void OnException(ExceptionContext filterContext)
		{
			LogException(filterContext);
		}

	}

}