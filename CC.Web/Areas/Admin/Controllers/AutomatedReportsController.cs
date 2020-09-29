using CC.Web.Areas.Admin.Models;
using CC.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CC.Web.Areas.Admin.Controllers
{
	[CcAuthorize(CC.Data.FixedRoles.Admin, CC.Data.FixedRoles.GlobalOfficer)]
    public class AutomatedReportsController: AdminControllerBase
    {
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(ClientApprovalStatusController));

        public AutomatedReportsController() { }

        public ActionResult Index(string msg)
        {
            var model = new AutomatedReportsModel();

            var lastEmailSuccess = db.Globals.Where(f => f.Message == "Automated Reports Email - Success").OrderByDescending(f => f.Date).FirstOrDefault();
            var lastEmailFailure = db.Globals.Where(f => f.Message == "Automated Reports Email - Failure").OrderByDescending(f => f.Date).FirstOrDefault();

            if(lastEmailSuccess != null && lastEmailFailure != null)
            {
                if(lastEmailSuccess.Date > lastEmailFailure.Date)
                {
                    model.LastEmailDate = lastEmailSuccess.Date;
                    model.Status = "Success";
                }
                else
                {
                    model.LastEmailDate = lastEmailFailure.Date;
                    model.Status = "Failure";
                }
            }
            else if(lastEmailSuccess != null)
            {
                model.LastEmailDate = lastEmailSuccess.Date;
                model.Status = "Success";
            }
            else if(lastEmailFailure != null)
            {
                model.LastEmailDate = lastEmailFailure.Date;
                model.Status = "Failure";
            }

            if(!string.IsNullOrEmpty(msg))
            {
                ModelState.AddModelError("Success", msg);
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Update(AutomatedReportsModel model)
        {
            if(model.IsActive)
            {
                
            }

            return RedirectToAction("Index", new { msg = "Automated reports were updated" });
        }

        public ActionResult EmailAllReports()
        {
            AutomatedReportsHelper.AutoEmailAllReports(Permissions);

            return RedirectToAction("Index", new { msg = "Done emailing all reports" });
        }
    }
}