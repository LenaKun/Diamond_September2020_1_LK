using System;
using CC.Web.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CC.Data;


namespace CC.Web.Controllers
{
	[CcAuthorize(FixedRoles.Admin, FixedRoles.RegionOfficer, FixedRoles.GlobalOfficer, FixedRoles.GlobalReadOnly, FixedRoles.AuditorReadOnly)]
	public class ClientContactsController : CcControllerBase
	{
		log4net.ILog log = log4net.LogManager.GetLogger(typeof(ClientContactsController));
		//
		// GET: /ClientContacts/

		public ActionResult Index(int clientId)
		{
			CheckPermissions(clientId);
			ViewBag.CanDelete = Permissions.User.RoleId == (int)FixedRoles.Admin;
			return View(clientId);
		}

		public JsonResult IndexData(int clientId, CC.Web.Models.jQueryDataTableParamModel p)
		{
			CheckPermissions(clientId);
			var source = from ag in db.ClientContacts
						 where ag.ClientId == clientId
						 select new
						 {

							 Id = ag.Id,
							 DateOfContact = ag.ContactDate,
							 Contacted = ag.Contacted,
							 ContactedUsing = ag.ContactedUsing,
							 CcStaffContact = ag.CcStaffContact,
							 ReasonForContact = ag.ReasonForContact,
							 ResponseRecievedDate = ag.ResponseRecievedDate,
							 EntryDate = ag.EntryDate,
							 UserName = ag.User.UserName,
							 DocumentName = ag.Filename
						 };

			var sSortCol_0 = Request["mDataProp_" + p.iSortCol_0];
			var bSortAsc_0 = p.sSortDir_0 == "asc";

			var filtered = source;
			if (!string.IsNullOrEmpty(p.sSearch))
			{
				filtered = filtered.Where(f => System.Data.Objects.SqlClient.SqlFunctions.StringConvert((decimal)f.Id).Trim().Equals(p.sSearch));
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

        [CcAuthorize(FixedRoles.Admin, FixedRoles.RegionOfficer, FixedRoles.GlobalOfficer)]
		public ActionResult Create(int clientId)
		{
			CheckPermissions(clientId);
			return View(new ClientContact() { ClientId = clientId });
		}

        [CcAuthorize(FixedRoles.Admin, FixedRoles.RegionOfficer, FixedRoles.GlobalOfficer)]
		[HttpPost]
		public ActionResult Create(CC.Data.ClientContact model, HttpPostedFileWrapper file)
		{
			CheckPermissions(model.ClientId);

			if (ModelState.IsValid)
			{
				if (file != null)
				{
					model.Filename = file.FileName;
				}
				else
				{
					model.Filename = null;
				}
				model.EntryDate = DateTime.Now;
				model.UserId = this.CcUser.Id;

				db.ClientContacts.AddObject(model as CC.Data.ClientContact);

				try
				{

					db.SaveChanges();
				}
				catch (Exception ex)
				{
					log.Error(ex);
					ModelState.AddModelError(string.Empty, ex.Message);
				}
			}

			if (ModelState.IsValid)
			{
				if (file != null)
				{
					var path = Server.MapPath(model.RelativeFilePath);
					try
					{
						file.SaveAs(path);
					}
					catch (System.IO.DirectoryNotFoundException)
					{
						var directory = System.IO.Path.GetDirectoryName(path);
						System.IO.Directory.CreateDirectory(directory);
						file.SaveAs(path);
					}
					catch (Exception ex)
					{
						log.Error(ex);
						ModelState.AddModelError(string.Empty, ex.Message);
					}
				}
			}

			if (ModelState.IsValid)
			{
				return this.RedirectToAction("Details", "Clients", new { id = model.ClientId });
			}
			else
			{
				return View(model);
			}
		}

		public ActionResult Details(int id)
		{
			var model = db.ClientContacts.SingleOrDefault(f => f.Id == id);
			CheckPermissions(model.ClientId);
			if (ModelState.IsValid)
			{
				return View(model);
			}
			else
			{
				throw new InvalidOperationException("CCID not found");
			}
		}

        [CcAuthorize(FixedRoles.Admin, FixedRoles.RegionOfficer, FixedRoles.GlobalOfficer)]
		public ActionResult Edit(int id)
		{
			var model = db.ClientContacts.SingleOrDefault(f => f.Id == id);
			CheckPermissions(model.ClientId);
			if (ModelState.IsValid)
			{
				return EditView(model);
			}
			else
			{
				throw new InvalidOperationException("CCID not found");
			}
		}
        [CcAuthorize(FixedRoles.Admin, FixedRoles.RegionOfficer, FixedRoles.GlobalOfficer)]
		[HttpPost]
		public ActionResult Edit(ClientContact input, HttpPostedFileWrapper file)
		{
			var model = db.ClientContacts.SingleOrDefault(f => f.Id == input.Id);
			if (ModelState.IsValid)
			{
				if (CcUser.RoleId == (int)FixedRoles.Admin)
				{
					model.ReasonForContact = input.ReasonForContact;
					model.CcStaffContact = input.CcStaffContact;
					model.ContactDate = input.ContactDate;
					model.Contacted = input.Contacted;
					model.ContactedUsing = input.ContactedUsing;

					if (model.Filename!=null)
					{
						if (file != null || input.Filename == null)
						{
							var abs = Server.MapPath(model.RelativeFilePath);
							if (System.IO.File.Exists(abs))
							{
								System.IO.File.Delete(abs);
							}
						}
						input.Filename = null;
					}

					if (file != null)
					{
					
						model.Filename = file.FileName;
						file.SaveAs(Server.MapPath(model.RelativeFilePath));
					}

				}

				model.ResponseRecievedDate = input.ResponseRecievedDate;

				try
				{
					db.SaveChanges();
				}
				catch (Exception ex)
				{
					log.Error(ex.Message, ex);
					ModelState.AddModelError(string.Empty, ex.Message);
				}
			}

			if (ModelState.IsValid)
			{
				return this.RedirectToAction("Details", "Clients", new { id = model.ClientId });
			}
			else
			{
				return EditView(model);
			}
		}

        [CcAuthorize(FixedRoles.Admin, FixedRoles.RegionOfficer, FixedRoles.GlobalOfficer)]
		ActionResult EditView(ClientContact model)
		{
			if (CcUser.RoleId == (int)FixedRoles.Admin)
			{
				return View("EditAdmin", model);
			}
			else
			{
				return View("Edit", model);
			}
		}

		public FileResult Download(int id)
		{
			var model = db.ClientContacts.SingleOrDefault(f => f.Id == id);
			CheckPermissions(model.ClientId);
			if (model != null)
			{
				return this.File(model.RelativeFilePath, System.Net.Mime.MediaTypeNames.Application.Octet, model.Filename);
			}
			else
			{
				throw new InvalidOperationException();
			}
		}

        [CcAuthorize(FixedRoles.Admin, FixedRoles.RegionOfficer, FixedRoles.GlobalOfficer)]
		public ActionResult Delete(int id)
		{
			var model = db.ClientContacts.SingleOrDefault(f => f.Id == id);
			CheckPermissions(model.ClientId);
			return View(model);
		}

		[CcAuthorize(FixedRoles.Admin)]
		[HttpPost, ActionName("Delete")]
		public ActionResult DeleteConfirmed(int id)
		{
			var model = db.ClientContacts.SingleOrDefault(f => f.Id == id);
			CheckPermissions(model.ClientId);
			if (ModelState.IsValid)
			{
				if (model == null)
				{
					throw new InvalidOperationException();
				}
				else
				{
					try
					{
						if (model.Filename != null)
						{
							var f = Server.MapPath(model.RelativeFilePath);
							if (System.IO.File.Exists(f)) { System.IO.File.Delete(f); }
						}
						db.ClientContacts.DeleteObject(model);
						db.SaveChanges();
					}
					catch (Exception ex)
					{
						ModelState.AddModelError(string.Empty, ex.Message);
					}
					if (ModelState.IsValid)
					{
						return this.RedirectToAction("Details", "Clients", new { id = model.ClientId });
					}
					else
					{
						return View(model);
					}
				}
			}
			else
			{
				return View(model);
			}
		}

		[CcAuthorize(FixedRoles.Admin)]
		[HttpPost]
		public ActionResult DeleteFile(int id)
		{
			var model = db.ClientContacts.SingleOrDefault(f => f.Id == id);
			if (model.Filename != null)
			{

				var absolutePath = Server.MapPath(model.RelativeFilePath);
				if (System.IO.File.Exists(absolutePath))
				{
					System.IO.File.Delete(absolutePath);
				}
				model.Filename = null;
				db.SaveChanges();
			}
			return EditView(model);
		}

		private void CheckPermissions(int clientId)
		{
			var allowedClient = db.Clients.Where(Permissions.ClientsFilter).Any(f => f.Id == clientId);
			if (!allowedClient)
			{
				ModelState.AddModelError(string.Empty, "CCID not found.");
			}
		}
	}
}
