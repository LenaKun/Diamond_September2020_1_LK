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
using CC.Web.Helpers;

namespace CC.Web.Controllers
{


	public class AccountController : CcControllerBase
	{
		private readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(AccountController));
        //private static readonly int PasswordExpiresInDays = int.Parse(ConfigurationManager.AppSettings["PasswordExpiresInDays"]);
        
		//
		// GET: /Account/LogOn
		[MessageAction]
		public ActionResult LogOn()
		{
			var n = (Request.Browser.IsBrowser("IE") && Request.Browser.MajorVersion < 8);
			if (n)
			{
				throw new Exception("You are using " + Request.Browser.Browser + " " + Request.Browser.Version + ". This browser may not be supported in the system. Please use IE8+, or Google Chrome.");
			}

			if (!this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains("FailedLogonAttempts"))
			{
				HttpCookie cookie = new HttpCookie("FailedLogonAttempts");
				cookie.Value = (0).ToString();
				cookie.Expires = DateTime.Now.AddMinutes(30);
				this.ControllerContext.HttpContext.Response.Cookies.Add(cookie);
			}

			HttpCookie value = this.ControllerContext.HttpContext.Request.Cookies["FailedLogonAttempts"];
			int FailedLogonAttempts;
			if (int.TryParse(value.Value, out FailedLogonAttempts))
			{
				ViewBag.FailedLogonAttempts = FailedLogonAttempts;
			}
			return View();
		}

		//
		// POST: /Account/LogOn
		[HttpPost]
		public ActionResult LogOn(LogOnModel model, string returnUrl)
		{
			string EncodedResponse = Request.Form["g-Recaptcha-Response"];
			bool IsCaptchaValid = (ReCaptchaClass.Validate(EncodedResponse) == "True" ? true : false);
			int FailedLogonAttempts = 0;
			HttpCookie cookie = this.ControllerContext.HttpContext.Request.Cookies["FailedLogonAttempts"];
			if (cookie != null)
			{
				int.TryParse(cookie.Value, out FailedLogonAttempts);
			}
			if (ModelState.IsValid && (FailedLogonAttempts < 2 || IsCaptchaValid))
			{
				using (ccEntities context = new ccEntities())
				{
					//get membership user with the same username
					var membershipUser = context.MembershipUsers
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
						FailedLogonAttempts++;
						if(cookie != null)
						{
							cookie.Value = FailedLogonAttempts.ToString();
							this.ControllerContext.HttpContext.Response.Cookies.Add(cookie);
						}
					}
					else if (membershipUser != null && membershipUser.ValidatePassword(model.Password))
					{
						//get the last agreement id
						var lastAgreement = context.UserAgreements.OrderByDescending(f => f.Date).FirstOrDefault();

						//user is required to sing the agreement if it exists an he had not already sing it
						var agreementRequired = lastAgreement != null && !membershipUser.User.UserAgreementAudits.Any(f => f.UserAgreementId == lastAgreement.Id);

						if(membershipUser.User.Disabled)
                        {
                            ModelState.AddModelError("", "This ID has been disabled due to inactivity.  If you wish to have this ID re-enabled, please contact your local administrator or your Claims Conference Program Assistant.");
                        }
                        else if (membershipUser.ExpirationDate > DateTime.Now)
						{
							return this.RedirectToAction(f => f.AccountExpired(membershipUser.User.UniqueId));
						}
						else if (agreementRequired)
						{
							return this.RedirectToAction(f => f.UserAgreement(membershipUser.User.UniqueId));
						}
						else
						{
							//set authentication cookie
							var user = membershipUser.User;

                            membershipUser.FailedPasswordAttemptCount = 0;
                            membershipUser.LastLoginDate = DateTime.Now;
                            context.SaveChanges();

							FailedLogonAttempts++;
							if (cookie != null)
							{
								cookie.Value = FailedLogonAttempts.ToString();
								cookie.Expires = DateTime.Now.AddDays(-1);
								this.ControllerContext.HttpContext.Response.Cookies.Add(cookie);
							}
                            
							FormsAuthentication.RedirectFromLoginPage(user.UserName, true);

							//redirect from login
							if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
								&& !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
							{
								return Redirect(returnUrl);
							}
							else
							{
								return Redirect("~/");
							}
						}
					}
					else
					{
                        //if user inserted incorrect password less then 5 times and he is not disabled
                        if (membershipUser.FailedPasswordAttemptCount < 4 && !membershipUser.User.Disabled)
                        {
                            membershipUser.FailedPasswordAttemptCount++;
                            ModelState.AddModelError("", "The user name or password provided is incorrect.");
							FailedLogonAttempts++;
							if (cookie != null)
							{
								cookie.Value = FailedLogonAttempts.ToString();
								this.ControllerContext.HttpContext.Response.Cookies.Add(cookie);
							}
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
			}
			else if(!IsCaptchaValid)
			{
				ModelState.AddModelError("", "Please complete captcha verification to login.");
			}
			ViewBag.FailedLogonAttempts = FailedLogonAttempts;

			// If we got this far, something failed, redisplay form
			return View(model);
		}

		[HttpGet]
		public ActionResult AccountExpired(Guid Id)
		{
			throw new NotImplementedException();
		}
		[HttpGet]
		public ActionResult ThrowException()
		{
			throw new Exception("Test exception");
		}

		[HttpGet]
		public ActionResult UserAgreement(Guid id)
		{
			using (var context = new ccEntities())
			{

				var lastAgreement = context.UserAgreements.OrderByDescending(f => f.Date).FirstOrDefault();

				if (lastAgreement == null)
				{
					throw new InvalidOperationException("Agreement not exists");
				}
				else
				{

					var user = context.Users.SingleOrDefault(f => f.UniqueId == id);
					if (user == null)
					{
						throw new InvalidOperationException("User not found");
					}
					else
					{
						var existing = context.UserAgreementAudits.SingleOrDefault(f => f.UserAgreementId == lastAgreement.Id && f.User.UniqueId == id);
						if (existing == null)
						{
							var model = new UserAgreementAudit()
							{
								User = user,
								UserAgreement = lastAgreement
							};
							return View(model);

						}
						else
						{
							//user had already cofirmed the agreement
							//inform him in any way?
							return Redirect("~/");
						}
					}

				}

			}
		}

		[HttpPost]
		[ConfirmPassword("password", "User.UniqueId")]
		public ActionResult UserAgreement(UserAgreementAudit model)
		{
			ModelState.Clear();

			if (ModelState.IsValid)
			{
				using (var db = new ccEntities())
				{
					var user = db.Users.SingleOrDefault(f => f.UniqueId == model.User.UniqueId);

					model.Date = DateTime.Now;
					model.User = user;
					model.IP = Request.ServerVariables["X_FORWARDED_FOR"] ?? Request.UserHostAddress;

					db.UserAgreementAudits.AddObject(model);
					db.SaveChanges();
					FormsAuthentication.RedirectFromLoginPage(user.UserName, false);
					return null;
				}
			}
			return View(model);
		}

		/// <summary>
		/// GET Log Off
		/// </summary>
		/// <returns></returns>
		public ActionResult LogOff()
		{
			FormsAuthentication.SignOut();

			return RedirectToAction("LogOn", "Account");
		}


		//
		// GET: /Account/ChangePassword

		public ActionResult ChangePassword()
		{
            try
            {
                var model = new ChangePasswordModel();
                model.UserName = User.Identity.Name;
                return View(model);
            }
            catch
            {
                
            }
            return RedirectToAction("LogOn", "Account");
		}

		//
		// POST: /Account/ChangePassword

		[HttpPost]
		public ActionResult ChangePassword(ChangePasswordModel model)
		{
			if (ModelState.IsValid) {

				// ChangePassword will throw an exception rather
				// than return false in certain failure scenarios.
				bool changePasswordSucceeded = false;
				try
				{
					using (var context = new ccEntities())
					{
						var membershipUser = context.MembershipUsers.Include(f => f.User).Single(f => f.LoweredUserName == model.UserName);

						if (membershipUser.ValidatePassword(model.OldPassword))
						{
                            if (model.NewPassword != model.OldPassword)
                            {
                                membershipUser.SetPassword(model.NewPassword);
                                membershipUser.LastPasswordChangedDate = DateTime.Now;
                                membershipUser.User.TemporaryPassword = false;
                                membershipUser.LastLoginDate = DateTime.Now;
                                context.SaveChanges();
                                changePasswordSucceeded = true;
                            }
						}

					}
				}
				catch (InvalidOperationException)
				{
					//logged in user not found in db
					//probably he was deleted since last logon
					changePasswordSucceeded = false;
				}

				if (changePasswordSucceeded)
				{
                    if (this.Request.IsJsonRequest())
                    {
                        return this.MyJsonResult(new
                        {
                            message = "Password was changed successfully"
                        }, 200);
                    }
                    else
                    {
                        return RedirectToAction("ChangePasswordSuccess");
                    }
				}
				ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
			}

            // If we got this far, something failed, redisplay form
            if (this.Request.IsJsonRequest())
            {
                return this.MyJsonResult(ModelState, 400);
            }
            else
            {
                return View(model);
            }
		}

		//
		// GET: /Account/ChangePasswordSuccess

		public ActionResult ChangePasswordSuccess()
		{
			return View();
		}


		public ActionResult Error()
		{
			log.Fatal("test fatal", new Exception());
			return new EmptyResult();
		}

		public ActionResult InitUsers()
		{
			var res = new List<Tuple<int, string, string, string>>();

			using (var db = new ccEntities())
			{
				foreach (var agency in db.Agencies.Where(f => !f.Users.Any()))
				{
					string pass = System.Web.Security.Membership.GeneratePassword(8, 2);
					var user = CC.Data.User.CreateUser(agency.Id + "_agencyUser", pass);
					agency.Users.Add(user);
					user.RoleId = (int)FixedRoles.AgencyUser;
					res.Add(new Tuple<int, string, string, string>(agency.Id, agency.Name, user.UserName, pass));
				}
				db.SaveChanges();
			}
			using (var db = new ccEntities())
			{
				foreach (var ser in db.AgencyGroups.Where(f => !f.Users.Any()))
				{
					string pass = System.Web.Security.Membership.GeneratePassword(8, 2);
					var user = CC.Data.User.CreateUser(ser.Id + "_serUser", pass);
					user.RoleId = (int)FixedRoles.Ser;
					ser.Users.Add(user);
					res.Add(new Tuple<int, string, string, string>(ser.Id, ser.DisplayName, user.UserName, pass));
				}
				db.SaveChanges();
			}
			using (var db = new ccEntities())
			{
				foreach (var item in db.Regions.Where(f => f.Id != 0 && !f.Users.Any()))
				{
					string pass = System.Web.Security.Membership.GeneratePassword(8, 2);
					var user = CC.Data.User.CreateUser(item.Id + "_rpo", pass);
					user.RoleId = (int)FixedRoles.RegionOfficer;
					item.Users.Add(user);
					res.Add(new Tuple<int, string, string, string>(item.Id, item.Name, user.UserName, pass));
				}
				db.SaveChanges();
			}
			var sb = new System.Text.StringBuilder();

			return this.Excel("users", "user", res);
		}

		public ActionResult SessionStatus()
		{
			var result = new SessionStatusResult
			{
				Expired = true,
				ExpiresIn = -1

			};
			if (Request.Cookies.Count > 0)
			{
				var cookie = Request.Cookies[FormsAuthentication.FormsCookieName];
				if (cookie != null)
				{
					var ticket = FormsAuthentication.Decrypt(cookie.Value);

					var expires = ticket.Expiration - DateTime.Now;

					result.Expired = ticket.Expired;
					result.ExpiresIn = expires.TotalSeconds;
				}
			}
			Response.Cookies.Clear();
			return Json((object)result, JsonRequestBehavior.AllowGet);
		}
		[DoNotResetAuthCookie]
		public ActionResult SessionTimeOut(string returnUrl)
		{
			ViewBag.ReturnUrl = returnUrl;
			if (ViewBag.ReturnUrl == null)
			{
				if (Request.UrlReferrer != null)
				{
					if (Request.UrlReferrer.Host == Request.Url.Host)
					{
						ViewBag.ReturnUrl = Request.UrlReferrer.ToString();
					}
				}
			}
			if (ViewBag.ReturnUrl == null)
			{

				ViewBag.ReturnUrl = Url.Content("~/");
			}
			return View();
		}
		#region Status Codes
		public static string ErrorCodeToString(MembershipCreateStatus createStatus)
		{
			// See http://go.microsoft.com/fwlink/?LinkID=177550 for
			// a full list of status codes.
			switch (createStatus)
			{
				case MembershipCreateStatus.DuplicateUserName:
					return "User name already exists. Please enter a different user name.";

				case MembershipCreateStatus.DuplicateEmail:
					return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

				case MembershipCreateStatus.InvalidPassword:
					return "The password provided is invalid. Please enter a valid password value.";

				case MembershipCreateStatus.InvalidEmail:
					return "The e-mail address provided is invalid. Please check the value and try again.";

				case MembershipCreateStatus.InvalidAnswer:
					return "The password retrieval answer provided is invalid. Please check the value and try again.";

				case MembershipCreateStatus.InvalidQuestion:
					return "The password retrieval question provided is invalid. Please check the value and try again.";

				case MembershipCreateStatus.InvalidUserName:
					return "The user name provided is invalid. Please check the value and try again.";

				case MembershipCreateStatus.ProviderError:
					return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

				case MembershipCreateStatus.UserRejected:
					return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

				default:
					return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
			}
		}
		#endregion

		[PasswordExpirationCheckAttribute()]
		public ViewResult Details()
		{
			var model = db.Users.SingleOrDefault(f => f.UserName == User.Identity.Name);
			return View(model);
		}

		[PasswordExpirationCheckAttribute()]
		public ViewResult Edit()
		{
			var model = db.Users.SingleOrDefault(f => f.UserName == User.Identity.Name);
			return View(model);
		}

		[PasswordExpirationCheckAttribute()]
		[HttpPost]
		public ActionResult Edit(User input)
		{
			var model = db.Users.SingleOrDefault(f => f.UserName == User.Identity.Name);
			model.Email = input.Email;
			model.FirstName = input.FirstName;
			model.LastName = input.LastName;
			model.DecimalDisplayDigits = input.DecimalDisplayDigits;
			ModelState.Clear();
			TryValidateModel(model);
			if (ModelState.IsValid)
			{

				db.SaveChanges();
				return RedirectToAction("Details");
			}
			else
			{
				return View(model);
			}
		}

	}
}
