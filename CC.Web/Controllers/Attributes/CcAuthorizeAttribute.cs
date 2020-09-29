using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Entity;
using System.Linq;
using CC.Data;
using CC.Web.Security;
using CC.Data.Repositories;

namespace System.Web.Mvc
{
	/// <summary>
	/// Authorizes user request based on a list of the FixedRoles Enum
	/// </summary>
	public class CcAuthorizeAttribute : AuthorizeAttribute
	{
		log4net.ILog _log = log4net.LogManager.GetLogger(typeof(CcAuthorizeAttribute));

		public string Message { get; set; }

		public new FixedRoles Roles { get; set; }

		public CcAuthorizeAttribute()
		{

		}
		public CcAuthorizeAttribute(params FixedRoles[] roles)
			: this()
		{
			this.Roles = roles.Aggregate((FixedRoles)0, (res, cur) => res | cur);
		}

		/// <summary>
		/// Checks for user's RoleId is matched against internally stored list of allowed roles
		/// </summary>
		/// <param name="httpContext">is unused</param>
		/// <returns></returns>
		protected override bool AuthorizeCore(HttpContextBase httpContext)
		{
			bool result = false;

			AuthorizeApiRequest(httpContext);

			if (httpContext.User.Identity.IsAuthenticated)
			{
				using (var db = new ccEntities())
				{
					var user = db.MembershipUsers.SingleOrDefault(f => f.LoweredUserName == httpContext.User.Identity.Name.ToLower());
					if (user == null || user.User == null)
					{
						result = false;
					}
					else
					{
						result = this.Roles.HasFlag((FixedRoles)user.User.RoleId);
					}
				}
			}
			return result;
		}

		/// <summary>
		/// internally calls AuthorizeCore
		/// Fired after controller.onAuthorization
		/// </summary>
		/// <param name="filterContext"></param>
		public override void OnAuthorization(AuthorizationContext filterContext)
		{
			base.OnAuthorization(filterContext);
		}

		protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
		{
			if (filterContext.HttpContext.Request.IsJsonRequest())
			{
				if (filterContext.HttpContext.User.Identity.IsAuthenticated)
				{
					filterContext.Result = new HttpStatusCodeResult(403);
				}
				else
				{
					filterContext.Result = new HttpStatusCodeResult(401);
				}
			}
			else
			{
				var showViewResult = filterContext.HttpContext.User.Identity.IsAuthenticated;

				showViewResult &= !string.IsNullOrEmpty(Message);
				if (showViewResult)
				{
					var viewResult = new ViewResult
					{
						ViewName = "Unauthorized"
					};
					viewResult.ViewBag.Message = this.Message;
					filterContext.Result = viewResult;
				}
				else
				{
					base.HandleUnauthorizedRequest(filterContext);
				}
			}
		}

		private void AuthorizeApiRequest(HttpContextBase httpContext)
		{
			if (!httpContext.User.Identity.IsAuthenticated)
			{
				var token = GetApiToken(httpContext.Request);
				if (!string.IsNullOrEmpty(token))
				{
					try
					{
						var encodedTicket = System.Text.Encoding.ASCII.GetString(Convert.FromBase64String(token));
						var ticket = System.Web.Security.FormsAuthentication.Decrypt(encodedTicket);
						if (!ticket.Expired)
						{
							var genericprincipal = 
								new System.Security.Principal.GenericPrincipal(
								new System.Security.Principal.GenericIdentity(ticket.Name, "api"),
								null
								);
							var ccPrincipal = new CcPrincipal(genericprincipal);
							httpContext.User = ccPrincipal;
						}
					}
					catch (Exception ex)
					{
						_log.Error(ex);
					}
				}
			}
		}

		
		private string GetApiToken(HttpRequestBase request)
		{
			return request.Headers["X-CC-Auth-Ticket"];
		}

	}
}