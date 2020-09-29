using CC.Web.Areas.Admin.Models;
using CC.Web.Controllers;
using CC.Web.Helpers;
using CC.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CC.Web.Areas.Admin.Controllers
{
	[CcAuthorize(CC.Data.FixedRoles.Admin)]
	public class LandingPageSettingsController : AdminControllerBase
    {
        public ActionResult Index(string msg, string successMsg)
        {
			if(!string.IsNullOrEmpty(msg))
			{
				ModelState.AddModelError("", msg);
			}
			if(!string.IsNullOrEmpty(successMsg))
			{
				ViewBag.SuccessMsg = successMsg;
			}
			var model = new LandingPageSettingsModel();
			model.LandingPageMessageContent = GlobalDbSettings.GetString(GlobalDbSettings.GlobalStringNames.LandingPageMessage);
            return View(model);
        }

		public JsonResult IndexData(jQueryDataTableParamModel model)
		{
			var source = db.Files.Where(f => f.IsLandingPage);

			var sSortCol_0 = Request["mDataProp_" + model.iSortCol_0];
			var bSortAsc_0 = model.sSortDir_0 == "asc";
			var sorted = source.OrderByField(sSortCol_0, bSortAsc_0).Skip(model.iDisplayStart).Take(model.iDisplayLength);

			var result = new jQueryDataTableResult
			{
				sEcho = model.sEcho,
				aaData = sorted.ToList(),
				iTotalRecords = sorted.Count(),
				iTotalDisplayRecords = sorted.Count()
			};
			return this.MyJsonResult(result);
		}

		[HttpPost]
		public ActionResult UploadNewFile(HttpPostedFileBase newFile, string Description, float Order)
		{
			if(newFile == null)
			{
				return RedirectToAction("Index", new { msg = "Please select a file" });
			}
			if(string.IsNullOrEmpty(Description))
			{
				return RedirectToAction("Index", new { msg = "Description is a required field for uploading a file" });
			}
			List<string> errors = new List<string>();
			if (FilesHelper.SaveFile(newFile, Guid.NewGuid(), Description, Order, true, Server, ref errors))
			{
				return RedirectToAction("Index", new { successMsg = "File uploaded successfully" });
			}
			else
			{
				return RedirectToAction("Index", new { msg = errors.FirstOrDefault() });
			}
		}

		[HttpPost]
		public ActionResult UpdateMessageContent(LandingPageSettingsModel model)
		{
			GlobalDbSettings.Set(GlobalDbSettings.GlobalStringNames.LandingPageMessage, model.LandingPageMessageContent);
			return RedirectToAction("Index", new { successMsg = "Message Content saved successfully" });
		}

		public ActionResult UpdateDescription(Guid Id, float order, string Description)
		{
			if(string.IsNullOrEmpty(Description))
			{
				return this.MyJsonResult(new { success = false, errors = new[] { "Description cannot be empty" } });
			}
			var file = db.Files.SingleOrDefault(f => f.Id == Id);
			if(file == null)
			{
				return this.MyJsonResult(new { success = false, errors = new[] { "File could not be found" } });
			}
			file.Description = Description;
			file.Order = order;
			try
			{
				db.SaveChanges();
				return this.MyJsonResult(new { success = true, data = file });
			}
			catch(Exception ex)
			{
				var msg = ex.Message;
				if (ex.InnerException != null)
				{
					var inner = ex.InnerException;
					while (inner.InnerException != null)
					{
						inner = inner.InnerException;
					}
					msg = inner.Message;
				}
				return this.MyJsonResult(new { success = false, errors = new[] { msg } });
			}
		}

		public ActionResult DeleteFile(Guid Id)
		{
			List<string> errors = new List<string>();
			FilesHelper.DeleteFile(Id, true, Server, ref errors);
			if(errors.Count == 0)
			{
				return this.MyJsonResult(new { success = true });
			}
			else
			{
				return this.MyJsonResult(new { success = false, errors = new[] { errors.FirstOrDefault() } });
			}
		}
    }
}
