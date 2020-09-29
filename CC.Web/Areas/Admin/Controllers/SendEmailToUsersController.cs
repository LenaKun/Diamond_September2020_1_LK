using CC.Web.Areas.Admin.Models;
using CC.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CC.Web.Areas.Admin.Controllers
{
	[CcAuthorize(CC.Data.FixedRoles.Admin)]
    public class SendEmailToUsersController : AdminControllerBase
    {
        //
        // GET: /Admin/SendEmailToUsers/

		public ActionResult Index(int? RegionId, int? CountryId, int? AgencyGroupId, int? AgencyId, string errorMsg, string successMsg)
        {
			var model = new UsersListModel();
			model.SelectedRegionId = RegionId;
			model.SelectedCountryId = CountryId;
			model.SelectedAgencyGroupId = AgencyGroupId;
			model.SelectedAgencyId = AgencyId;
			if(!string.IsNullOrEmpty(errorMsg))
			{
				ModelState.AddModelError(string.Empty, errorMsg);
			}
			if(!string.IsNullOrEmpty(successMsg))
			{
				ModelState.AddModelError("Success", successMsg);
			}
            return View(model);
        }

		public JsonResult IndexData(int? RegionId, int? CountryId, int? AgencyGroupId, int? AgencyId, jQueryDataTableParamModel p)
		{
			var usersModel = new UsersModel();
			var source = usersModel.GetUsers(db, this.Permissions);

			var sSortCol_0 = Request["mDataProp_" + p.iSortCol_0];
			var bSortAsc_0 = p.sSortDir_0 == "asc";

			var filtered = source;

			if(RegionId.HasValue)
			{
				filtered = filtered.Where(f => f.RegionId == RegionId);
			}

			if (CountryId.HasValue)
			{
				filtered = filtered.Where(f => f.CountryId == CountryId);
			}

			if (AgencyGroupId.HasValue)
			{
				filtered = filtered.Where(f => f.AgencyGroupId == AgencyGroupId);
			}

			if (AgencyId.HasValue)
			{
				filtered = filtered.Where(f => f.AgencyId == AgencyId);
			}

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

		[HttpPost]
		[ValidateInput(false)]
		public ActionResult SendEmail(UsersListModel model)
		{
			if(string.IsNullOrEmpty(model.EmailTo))
			{
				ModelState.AddModelError(string.Empty, "To is a required field");
				return RedirectToAction("Index", new { RegionId = model.SelectedRegionId, CountryId = model.SelectedCountryId, AgencyGroupId = model.SelectedAgencyGroupId, AgencyId = model.SelectedAgencyId, errorMsg = "To is a required field" });
			}
			using (var smtpClient = new System.Net.Mail.SmtpClient())
			{
              //  smtpClient.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                
                //smtpClient = 25;
                try
				{
					var msg = new System.Net.Mail.MailMessage();
                    smtpClient.Port = 25;
					msg.IsBodyHtml = true;
					msg.Subject = model.Subject;
					msg.Body = model.Body;
					msg.To.Add(model.EmailTo);
					if (!string.IsNullOrEmpty(model.EmailBcc))
					{
						msg.Bcc.Add(model.EmailBcc);
					}
					if (!string.IsNullOrEmpty(model.EmailCc))
					{
						msg.CC.Add(model.EmailCc);
					}
					smtpClient.Send(msg);
					return RedirectToAction("Index", new { RegionId = model.SelectedRegionId, CountryId = model.SelectedCountryId, AgencyGroupId = model.SelectedAgencyGroupId, AgencyId = model.SelectedAgencyId, successMsg = "The email sent successfully" });
				}
				catch (Exception ex)
				{
					ModelState.AddModelError(string.Empty, ex);
					return RedirectToAction("Index", new { RegionId = model.SelectedRegionId, CountryId = model.SelectedCountryId, AgencyGroupId = model.SelectedAgencyGroupId, AgencyId = model.SelectedAgencyId, errorMsg = ex.Message });
				}
			}
		}
    }
}
