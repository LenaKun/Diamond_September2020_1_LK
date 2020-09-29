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

namespace CC.Web.Areas.Api.Controllers
{

	public class AccountController : CC.Web.Controllers.CcControllerBase
	{
		private readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(AccountController));

		//
		// POST: /Account/LogOn
		[HttpPost]
		public ActionResult LogOn(LogOnModel model)
		{
			CC.Data.MembershipUser membershipUser = null;
			if (ModelState.IsValid)
			{
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
					if (!(FixedRoles.DafEvaluator | FixedRoles.DafReviewer | FixedRoles.AgencyUserAndReviewer | FixedRoles.SerAndReviewer).HasFlag((FixedRoles)membershipUser.User.RoleId))
					{
						ModelState.AddModelError(string.Empty, "Only DAF Evaluators and Reviewers are allowed to logon.");
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
				var oneWeek = 10080;//one week
				var ticket = new System.Web.Security.FormsAuthenticationTicket(membershipUser.User.UserName, true, oneWeek);
				
				var encryptedTicket = System.Web.Security.FormsAuthentication.Encrypt(ticket);
				return this.MyJsonResult(new
				{
					username = membershipUser.User.UserName,
					firstName = membershipUser.User.FirstName,
					lastName = membershipUser.User.LastName,
                    roleId = membershipUser.User.RoleId,
                    agency = new {
                        id = membershipUser.User.Agency.Id,
                        name = membershipUser.User.Agency.Name
                    },
					ticket = encryptedTicket
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
