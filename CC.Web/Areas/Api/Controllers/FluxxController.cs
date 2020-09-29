using System;
using System.Collections.Generic;
using MvcContrib;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using CC.Web.Models;
using MvcContrib.ActionResults;
using CC.Data;
using CC.Web.Attributes;
using CC.Web.Controllers.Attributes;
using System.Configuration;
using CC.Web.Controllers;

namespace CC.Web.Areas.Api.Controllers
{

    public class FluxxController : CC.Web.Controllers.CcControllerBase
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(AccountController));
        
        public ActionResult CreateInPDFAndSaveReport(int id)
        {
            CC.Data.MembershipUser membershipUser = null;
            if (ModelState.IsValid)
            {
                LogOnModel model = new LogOnModel() { UserName = "admin", Password = "dennis2001IZ=" };

                var context = this.db;
                //get membership user with the same username
                membershipUser = context.MembershipUsers
                    .Include(f => f.User)
                    .SingleOrDefault(f => f.LoweredUserName == model.UserName);

                //validate password if the user exists
                if (membershipUser == null)
                {
                    var u = context.Users.SingleOrDefault(f => f.UserName == model.UserName);
                    if (u != null)
                    {
                        var mu = context.MembershipUsers.SingleOrDefault(f => f.Id == u.Id);
                        log.Debug(string.Format("LogOn failed: membershipUser is null, user not null. The user's username is {0}, the username of membershipUser with same id is {1}",
                            u.UserName, mu.LoweredUserName));
                    }
                    else
                    {
                        log.Debug(string.Format("LogOn failed: user wasn't found. The entered username is {0}", model.UserName));
                    }
                    ModelState.AddModelError("", "The user name or password provided is incorrect.");
                }
                else if (membershipUser != null && membershipUser.ValidatePassword(model.Password))
                {
                    if (membershipUser.User.Disabled)
                    {
                        ModelState.AddModelError("", "This ID has been disabled due to inactivity.  If you wish to have this ID re-enabled, please contact your local administrator or your Claims Conference Program Assistant.");
                    }
                    else if (membershipUser.ExpirationDate > DateTime.Now)
                    {
                        ModelState.AddModelError("", "Account is expired");
                    }
                    else
                    {
                        //set authentication cookie
                        var user = membershipUser.User;

                        membershipUser.FailedPasswordAttemptCount = 0;
                        membershipUser.LastLoginDate = DateTime.Now;
                        context.SaveChanges();

                    }
                    if (!( FixedRoles.Admin ).HasFlag((FixedRoles)membershipUser.User.RoleId))
                   // if (false)
                    {
                        ModelState.AddModelError(string.Empty, "Only Fluxx User are allowed to logon.");
                    }
                }
                else
                {

                    //if user inserted incorrect password less then 5 times and he is not disabled
                    if (membershipUser.FailedPasswordAttemptCount < 4 && !membershipUser.User.Disabled)
                    {
                        membershipUser.FailedPasswordAttemptCount++;
                        ModelState.AddModelError("", "The user name or password provided is incorrect.");
                    }
                    //user inserted incorrect password on his 5th time, or he is already disabled
                    else
                    {
                        membershipUser.User.Disabled = true;
                        ModelState.AddModelError("", "You have exceeded the number of failed login attempts allowed and your account has been locked. Please contact your Program Assistant or Program Officer for assistance.");
                    }
                    context.SaveChanges();
                }
            }

            if (ModelState.IsValid && membershipUser != null)
            {

                var username = membershipUser.User.UserName;
                var saved_doc_name = "Error";
                try
                {
                    var oneWeek = 10080;//one week
                    var ticket = new FormsAuthenticationTicket(membershipUser.User.UserName, true, oneWeek);
                    //_user = ((CC.Web.Security.CcPrincipal)System.Web.HttpContext.Current.User).CcUser);
                    System.Web.HttpContext.Current.SetSessionStateBehavior(System.Web.SessionState.SessionStateBehavior.Required);
                    var oo1 = System.Web.HttpContext.Current;
                    var oo = System.Web.HttpContext.Current.User;
                    var gg = (CC.Web.Security.CcPrincipal)oo;
                    var ggg = gg.CcUser;
                    saved_doc_name = (new MainReportsController() { CcUser = membershipUser.User }).CreateAndSaveMainReportDocument(id);
                }
                catch (Exception ex)
                {
                    return this.MyJsonResult(new
                    {
                        errors = ex.InnerException.Message
                    }, 400);
                }
                return this.MyJsonResult(new
                {
                    username = membershipUser.User.UserName,

                    roleId = membershipUser.User.RoleId,
                    report = saved_doc_name ?? ""
                });
            }
            else
            {


                var data = ModelState.Where(f => f.Value.Errors.Any())
                    .SelectMany(f => f.Value.Errors)
                    .Select(f => f.ErrorMessage)
                    .ToList();
                return this.MyJsonResult(new
                {
                    errors = data
                }, 400);
            }
        }

    }
}

