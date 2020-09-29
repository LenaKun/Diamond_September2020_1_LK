using CC.Web.Areas.Admin.Models;
using System;
using System.Collections.Generic;
using System.Web.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Text.RegularExpressions;
using CC.Web.Helpers;
using System.Net.Mail;
using CC.Web.Controllers;
using Quartz;
using CC.Web.Jobs;

namespace CC.Web.Areas.Admin.Controllers
{
	[CcAuthorize(CC.Data.FixedRoles.Admin)]
	public class GeneralSettingsController : AdminControllerBase
    {
        //
        // GET: /Admin/GeneralSettings/

		public ActionResult Index(string errorMsg, string errorKey, string successMessage, string cfsExportSuccessMessage, string cfsDailyDigestSuccessMessage)
        {
			var model = new GeneralSettingsModel();
			model.NewSerOrgNotifyEmail = GlobalDbSettings.GetString(GlobalDbSettings.GlobalStringNames.NewSerOrgNotifyEmail);
			model.CfsDailyLastDate = GlobalDbSettings.GetString(GlobalDbSettings.GlobalStringNames.CfsDailyDigestLastDateTime);
			model.CfsRecordsExportDateTime = GlobalDbSettings.GetString(GlobalDbSettings.GlobalStringNames.ExportCfsClientRecordsDateTime);
            var hour = GlobalDbSettings.GetString(GlobalDbSettings.GlobalStringNames.CfsDailyDigestFireHour);
            int h;
            if(!string.IsNullOrEmpty(hour) && int.TryParse(hour, out h))
            {
                model.CfsDailyDigestFireHour = h;
            }
			if(string.IsNullOrEmpty(model.CfsDailyLastDate))
			{
				model.CfsDailyLastDate = "N/A";
			}
			if (string.IsNullOrEmpty(model.CfsRecordsExportDateTime))
			{
				model.CfsRecordsExportDateTime = "N/A";
			}
			if(!string.IsNullOrEmpty(errorMsg))
			{
				ModelState.AddModelError(errorKey ?? "", errorMsg);
			}
			if(!string.IsNullOrEmpty(successMessage))
			{
				ViewBag.SuccessMessage = successMessage;
			}
			if (!string.IsNullOrEmpty(cfsExportSuccessMessage))
			{
				ViewBag.CfsRecordsExportSuccessMessage = cfsExportSuccessMessage;
			}
            if (!string.IsNullOrEmpty(cfsDailyDigestSuccessMessage))
            {
                ViewBag.CfsDailyDigestSuccessMessage = cfsDailyDigestSuccessMessage;
            }
            return View(model);
        }

		public ActionResult UpdateNewSerOrgNotifyEmail(GeneralSettingsModel model)
		{
			if (!string.IsNullOrEmpty(model.NewSerOrgNotifyEmail))
			{
				string regexEmail = @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
				//string regexEmails = @"^([a-zA-Z0-9.!#$%&’*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+),?)+$";
				var invalidList = model.NewSerOrgNotifyEmail.Split('@');
				if (model.NewSerOrgNotifyEmail.Contains(","))
				{
					foreach (var email in model.NewSerOrgNotifyEmail.Split(','))
					{
						if (!Regex.IsMatch(email, regexEmail))
						{
							return RedirectToAction("Index", new { errorMsg = "One of the email's format is not valid", errorKey = "NewSerOrgNotifyEmail" });
						}
					}
				}
				else if (invalidList.Count() > 2)
				{
					return RedirectToAction("Index", new { errorMsg = "Please enter email addresses separated by comma", errorKey = "NewSerOrgNotifyEmail" });
				}
				else if (!Regex.IsMatch(model.NewSerOrgNotifyEmail, regexEmail))
				{
					return RedirectToAction("Index", new { errorMsg = "Email format is not valid", errorKey = "NewSerOrgNotifyEmail" });
				}
				GlobalDbSettings.Set(GlobalDbSettings.GlobalStringNames.NewSerOrgNotifyEmail, model.NewSerOrgNotifyEmail);
				return RedirectToAction("Index", new { successMessage = "Updated Successfully" });
			}
			return RedirectToAction("Index", new { errorMsg = "Please enter an email address", errorKey = "NewSerOrgNotifyEmail" });
		}

		public ActionResult InitiateCfsRecordsExport()
		{
			try
			{
				ClientsController cl = new ClientsController();
				cl.ExportCfsClientRecords(false);
				return RedirectToAction("Index", new { cfsExportSuccessMessage = "CFS records exported successfully" });
			}
			catch(Exception ex)
			{
				var msg = ex.Message;
				if(ex.InnerException != null)
				{
					var inner = ex.InnerException;
					while(inner.InnerException != null)
					{
						inner = inner.InnerException;
					}
					msg = inner.Message;
				}
				return RedirectToAction("Index", new { errorMsg = "There was ane error exporting the file: " + msg, errorKey = "CfsRecordsExportDateTime" });
			}
		}

        public ActionResult UpdateDailyDigestFireHour(GeneralSettingsModel model)
        {
            if(model.CfsDailyDigestFireHour.HasValue)
            {
                GlobalDbSettings.Set(GlobalDbSettings.GlobalStringNames.CfsDailyDigestFireHour, model.CfsDailyDigestFireHour);
                var jobKey = new JobKey("CfsDailyDigestJob");
                var job = Schedule.GetJobDetailByJobKey(jobKey);
                if(job != null)
                {
                    Schedule.DeleteJob(jobKey);
                }
                Schedule.RegisterCfsDailyDigestJob();
            }
            return RedirectToAction("Index", new { cfsDailyDigestSuccessMessage = "CFS Daily Digest Fire Hour saved successfully" });
        }
    }
}
