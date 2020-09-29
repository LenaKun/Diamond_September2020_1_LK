using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CC.Data;
using System.Web.Security;
using CC.Web.Controllers.Attributes;
using CC.Web.Controllers;
using System.ComponentModel.DataAnnotations;
using CC.Web.Areas.Admin.Models;

namespace CC.Web.Areas.Admin.Controllers
{
	[PasswordExpirationCheckAttribute()]
	[CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer, FixedRoles.RegionOfficer, FixedRoles.RegionAssistant, FixedRoles.Ser, FixedRoles.SerAndReviewer)]
	public class UsersController : AdminControllerBase
	{
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(AccountController));
		private IQueryable<User> GetAllowedUsers()
		{
			return db.Users.Where(this.Permissions.UsersFilter);
		}

		//
		// GET: /Admin/Users/

		public ViewResult Index()
		{
			return View();
		}

		public JsonResult IndexData(CC.Web.Models.jQueryDataTableParamModel p)
		{
			var usersModel = new UsersModel();
            var source = usersModel.GetUsers(db, this.Permissions);

			var sSortCol_0 = Request["mDataProp_" + p.iSortCol_0];
			var bSortAsc_0 = p.sSortDir_0 == "asc";

			var filtered = source;
			if (!string.IsNullOrEmpty(p.sSearch))
			{
				filtered = filtered.Where(f => System.Data.Objects.SqlClient.SqlFunctions.StringConvert((decimal)f.Id).Trim().Equals(p.sSearch)
					|| f.Username.Contains(p.sSearch)
					|| f.Role.Contains(p.sSearch)
					|| f.RoleEnd.Contains(p.sSearch)
				);
			}

			var data = filtered.OrderByField(sSortCol_0, bSortAsc_0).Skip(p.iDisplayStart).Take(p.iDisplayLength);

			var result = new CC.Web.Models.jQueryDataTableResult()
			{
				aaData = data,
				sEcho = p.sEcho,
				iTotalRecords = source.Count(),
				iTotalDisplayRecords = filtered.Count()
			};

			return this.MyJsonResult(result);
		}

		//
		// GET: /Admin/Users/Details/5

		public ViewResult Details(int id)
		{
			User user = db.Users.Single(u => u.Id == id);
            var memUser = db.MembershipUsers.SingleOrDefault(f => f.Id == id);
            ViewBag.FailedPasswordAttemptCount = memUser != null ? memUser.FailedPasswordAttemptCount : 0;
			return View(user);
		}

		//
		// GET: /Admin/Users/Create

		[CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer, FixedRoles.RegionOfficer, FixedRoles.RegionAssistant, FixedRoles.Ser, FixedRoles.SerAndReviewer)]
		public ActionResult Create()
		{
			var model = new CC.Web.Models.RegisterModel();
			model.LoadData(this.db, this.Permissions);
			return View(model);
		}

		//
		// POST: /Admin/Users/Create

		[HttpPost]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer, FixedRoles.RegionOfficer, FixedRoles.RegionAssistant, FixedRoles.Ser, FixedRoles.SerAndReviewer)]
		public ActionResult Create(CC.Web.Models.RegisterModel model)
		{
			//revalidate model with permissions set
			model.Permissions = this.Permissions;
			ModelState.Clear();
			TryValidateModel(model);

			if (ModelState.IsValid)
			{

				//create the user
				var user = CC.Data.User.CreateUser(model.UserName, model.Password);

				//update entity
				model.ApplyValuesTo(user, db);

				//validate entity
				TryValidateModel(user);

				if (ModelState.IsValid)
				{
					//add user
					db.Users.AddObject(user);

					//and detach agencygroups
					if (user.AgencyGroups.Any())
					{
						foreach (var ser in user.AgencyGroups)
						{
							db.ObjectStateManager.GetObjectStateEntry(ser).ChangeState(EntityState.Unchanged);
						}
					}

					// Attempt to register the user
					var createStatus = MembershipCreateStatus.Success;
					try
					{
						db.SaveChanges();
                        log.Debug(string.Format("Create User Success: user's username {0}, membershipUser's username {1}", 
                            user.UserName, user.MembershipUser.LoweredUserName));
					}
					catch
					{
						createStatus = MembershipCreateStatus.ProviderError;
					}



					if (createStatus == MembershipCreateStatus.Success)
					{
						return this.RedirectToAction("Index");
					}
					else
					{
						ModelState.AddModelError("", CC.Web.Controllers.AccountController.ErrorCodeToString(createStatus));
					}
				}
			}


			model.LoadData(this.db, this.Permissions);
			return View(model);
		}

		//
		// GET: /Admin/Users/Edit/5

		[CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer, FixedRoles.RegionOfficer, FixedRoles.RegionAssistant, FixedRoles.Ser, FixedRoles.SerAndReviewer)]
		public ActionResult Edit(int id)
		{
			var user = GetAllowedUsers()
				.Select(f => new CC.Web.Models.RegEditModel
				{
					Id = f.Id,
					FirstName = f.FirstName,
					LastName = f.LastName,
					AgencyGroupId = f.AgencyGroupId,
					AgencyGroupIds = f.AgencyGroups.Select(a => a.Id),
					AgencyId = f.AgencyId,
                    RegionId = f.RegionId,
					Email = f.Email,
                    AddToBcc = f.AddToBcc,
					RoleId = f.RoleId,
					UserName = f.UserName,
					DecimalDisplayDigits = f.DecimalDisplayDigits,
                    TemporaryPassword = f.TemporaryPassword,
                    Disabled = f.Disabled
				})
				.Single(u => u.Id == id);
			user.Permissions = this.Permissions;
			user.LoadData(db, Permissions);
			return View(user);
		}

		//
		// POST: /Admin/Users/Edit/5

		[HttpPost]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer, FixedRoles.RegionOfficer, FixedRoles.RegionAssistant, FixedRoles.Ser, FixedRoles.SerAndReviewer)]
		public ActionResult Edit(CC.Web.Models.RegEditModel model)
		{
			//revalidate model with permissions set
			model.Permissions = this.Permissions;
			ModelState.Clear();
			TryValidateModel(model);

			if (ModelState.IsValid)
			{
				var user = GetAllowedUsers()
					.Include(f => f.MembershipUser)
					.Single(f => f.Id == model.Id);

                log.Debug(string.Format("Edit User - got the user: user's username {0}, membershipUser's username {1}",
                            user.UserName, user.MembershipUser.LoweredUserName));

				model.ApplyValuesTo(user, db);

				TryValidateModel(user);
				if (ModelState.IsValid)
				{

					if (!string.IsNullOrEmpty(model.Password))
					{
						user.MembershipUser.SetPassword(model.Password);
					}


					try
					{
						db.SaveChanges();
                        log.Debug(string.Format("Edit User Success: user's username {0}, membershipUser's username {1}",
                            user.UserName, user.MembershipUser.LoweredUserName));
						return RedirectToAction("Index");
					}
					catch (Exception)
					{
					}
				}
			}
			model.LoadData(db, Permissions);
			return View(model);
		}

		[HttpPost]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer, FixedRoles.RegionOfficer, FixedRoles.RegionAssistant, FixedRoles.Ser, FixedRoles.SerAndReviewer)]
		public ActionResult ResetPassword(CC.Web.Models.RegEditModel model)
		{
			if (ModelState.IsValid)
			{
				var user = GetAllowedUsers()
					.Include(f => f.MembershipUser)
					.Single(f => f.Id == model.Id);

				user.MembershipUser.SetPassword(model.Password);

				TryValidateModel(user);
				if (ModelState.IsValid)
				{
					db.SaveChanges();
					return RedirectToAction("Index");
				}
			}
			model.LoadData(db, Permissions);
			return View("Edit", model);
		}

		//
		// GET: /Admin/Users/Delete/5

		[CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer, FixedRoles.RegionOfficer, FixedRoles.RegionAssistant, FixedRoles.Ser, FixedRoles.SerAndReviewer)]
		public ActionResult Delete(int id)
		{
			User user = GetAllowedUsers().Single(u => u.Id == id);
			return View(user);
		}

		//
		// POST: /Admin/Users/Delete/5

		[HttpPost, ActionName("Delete")]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer, FixedRoles.RegionOfficer, FixedRoles.RegionAssistant, FixedRoles.Ser, FixedRoles.SerAndReviewer)]
		public ActionResult DeleteConfirmed(int id)
		{
			User user = GetAllowedUsers().Single(u => u.Id == id);
			db.Users.DeleteObject(user);
			db.SaveChanges();
			return RedirectToAction("Index");
		}

		[CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer, FixedRoles.RegionOfficer, FixedRoles.RegionAssistant, FixedRoles.Ser, FixedRoles.SerAndReviewer)]
		public ActionResult EnableDisable(int id)
		{
			User user = GetAllowedUsers().Single(u => u.Id == id);
			if (user.Disabled)
			{
				user.Disabled = false;
			}
			else
			{
				user.Disabled = true;
			}
			try
			{
				db.SaveChanges();
			}
			catch
			{

			}
			return RedirectToAction("Index");
		}
                
        public ActionResult Export()
        {
			var usersModel = new UsersModel();
			var users = ((IQueryable<UsersExportListRow>)usersModel.GetUsers(db, this.Permissions)).ToList();
            return this.Excel("Users", "Sheet1", users);
        }

		protected override void Dispose(bool disposing)
		{
			db.Dispose();
			base.Dispose(disposing);
		}        
	}
}