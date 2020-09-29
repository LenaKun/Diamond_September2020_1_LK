using CC.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Data.Entity;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.Routing;

namespace CC.Web.Controllers.Attributes
{
    /// <summary>
    /// Forces password change if it's expired or the user is loging in for the first time
    /// expiration days are set in the web.config, if it's set to 0 then this feature is turned off
    /// </summary>
    public class PasswordExpirationCheckAttribute : AuthorizeAttribute
    {
        private static readonly int PasswordExpiresInDays = int.Parse(ConfigurationManager.AppSettings["PasswordExpiresInDays"]);

        public PasswordExpirationCheckAttribute()
        {
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            using (ccEntities context = new ccEntities())
            {
                var membershipUser = context.MembershipUsers
                    .Include(f => f.User)
                    .SingleOrDefault(f => f.LoweredUserName == filterContext.HttpContext.User.Identity.Name);

                if (membershipUser != null)
                {
                    if (PasswordExpiresInDays > 0 && (membershipUser.LastPasswordChangedDate == null || ((DateTime)membershipUser.LastPasswordChangedDate).AddDays(PasswordExpiresInDays) < DateTime.Now.Date) || membershipUser.User.TemporaryPassword)
                    {
                        if (!filterContext.HttpContext.Request.IsJsonRequest())
                        {
                            filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Account", action = "ChangePassword" }));
                        }
                        else
                        {
                            filterContext.Result = new jsdtr() { Data = new {
                                errorCode = "PasswordExpired",
                                message = "Password has expired"
                            }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                            filterContext.HttpContext.Response.StatusCode = 403;
                            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
                        }
                    }
                }
            }
            base.OnAuthorization(filterContext);
        }
    }
}