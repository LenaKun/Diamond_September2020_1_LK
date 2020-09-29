using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CC.Data;
using CC.Web.Models;
using CC.Web.Areas.Admin.Models;
using Quartz;

namespace CC.Web.Jobs
{
	public class HcStatusChangeNotificationsJob : LoggingJob, IDisposable
	{

		private ccEntities db = new ccEntities();
		public HcStatusChangeNotificationsJob()
		{
			this.log = log4net.LogManager.GetLogger(typeof(HcStatusChangeNotificationsJob));
		}
		protected override void ExecuteInternal(IJobExecutionContext contex)
		{
			throw new NotImplementedException();
		}

		internal void SendNotifications(IEnumerable<ClientsHcStatusChangeEmailModel> groupedBySer)
		{
			foreach (var item in groupedBySer)
			{
				var agencyGroup = (from a in db.AgencyGroups
								   where a.Id == item.AgencyGroupId
								   select new
								   {
									   a.Id,
									   a.DisplayName,
								   }).FirstOrDefault();
				foreach (var m in item.Clients.GroupBy(f => f.AgencyId))
				{
					SendNotifications(m, agencyGroup.Id, agencyGroup.DisplayName);
				}
			}
		}

		public void SendNotifications(ClientHcStatusChangeEmailModel model)
		{
			var agencyGroup = (from a in db.Agencies
							   where a.Id == model.AgencyId
							   select new
							   {
								   a.AgencyGroup.Id,
								   a.AgencyGroup.DisplayName,
							   }).FirstOrDefault();
			SendNotifications(new[] { model }, agencyGroup.Id, agencyGroup.DisplayName);
		}

		internal void SendNotifications(IEnumerable<ClientHcStatusChangeEmailModel> model, int agencyGroupId, string agencyGroupName)
		{
			var msg = new System.Net.Mail.MailMessage();
			msg.Body = msgbody(model);
			msg.IsBodyHtml = true;
			msg.Subject = string.Format("Notification of Client HC Status Change (SER {0})", agencyGroupName);
			AddRecipients(msg, agencyGroupId, model.FirstOrDefault().AgencyId);
			using (var smtpClient = new System.Net.Mail.SmtpClient())
			{
				smtpClient.Send(msg);
				log.InfoFormat("{0} ({1} clients), to: {2}, cc: {3}, bcc: {4}", msg.Subject, 1, msg.To, msg.CC, msg.Bcc);
			}
		}

		private static string msgbody(IEnumerable<ClientHcStatusChangeEmailModel> model)
		{
			var sb = new System.Text.StringBuilder();
			sb.Append(@"<p></p>

<p>This email delivers official notice of Change of HC Status for ").Append(model.Count()).Append(@" of your agency's clients.  </p>

<p>See list below. </p>

<p>If you have any questions or concerns, please contact your Program Assistant. </p>

<table>
    <thead>
        <tr>
            <th>CC ID</th>
            <th>Name</th>
            <th>Birth Country</th>
            <th>Country of Residence</th>
            <th>JNV Approval Status</th>
            <th>Homecare Approval Status</th>
        </tr>
    </thead>");
			foreach (var c in model)
			{
				sb.Append(@"<tbody>
            <tr>
                <td>").Append(c.ClientId).Append(@"</td>
                <td>").Append(c.ClientName).Append(@"</td>
                <td>").Append(c.BirthCountryName).Append(@"</td>
                <td>").Append(c.CountryName).Append(@"</td>
                <td>").Append(c.ApprovalStatusName).Append(@"</td>
                <td>").Append(c.HcStatusName).Append(@"</td>
            </tr>
        </tbody>");
			}
			sb.Append(@"</table>
<h3>STATUSES REQUIRING AGENCY ACTION:</h3>
 <ul>
<li>
NeverAppliedToCc: Clients did not match to individual CC compensations and clients should submit an application.  Clients joining the program in 2018 cannot be served with CC funds unless Approved in Diamond. A leave date will populate if no action is taken.
</li>
<li>
Agency Action Needed: Clients are matched to rejected or incomplete claims and there is insufficient evidence to verify Jewish Nazi victim status. Clients should  contact Survivor Services. A leave date will populate if the claim is not reopened and clients cannot receive CC funded services. See Diamond Help Screen for more information.
</li>
<li>
Not Eligible: Clients are not eligible to receive CC funded services and must cease receiving them within 3 days of this notification for existing clients (prior to 2018).
</li></ul><p>This is an automated message from Diamond, please do not reply to this email address.</p>");
			return sb.ToString();
		}
		private void TryAddAddress(string email, string displayName, System.Net.Mail.MailAddressCollection col)
		{

			try
			{
				var a = new System.Net.Mail.MailAddress(email, displayName);
				col.Add(a);

			}
			catch (Exception ex) { log.Debug(ex.Message, ex); }

		}


		public void AddRecipients(System.Net.Mail.MailMessage msg, int agencyGroupId, int agencyId)
		{
			var result = new Dictionary<int, int>();

			var gpos = (from u in db.Users
						where (u.RoleId == (int)FixedRoles.GlobalOfficer || u.RoleId == (int)FixedRoles.GlobalReadOnly || u.RoleId == (int)FixedRoles.AuditorReadOnly)
						where u.Email != null
						select new
							{
								Email = u.Email,
								AddToBcc = u.AddToBcc,
								DisplayName = (u.FirstName + " " + u.LastName) ?? u.UserName,
							}).ToList();

			var serUsers = (from s in db.AgencyGroups
							where s.Id == agencyGroupId
							from u in s.Users
							where u.Email != null
							where (u.RoleId == (int)FixedRoles.Ser || u.RoleId == (int)FixedRoles.SerAndReviewer) && !u.Disabled
							select new
							{
								Email = u.Email,
								AddToBcc = u.AddToBcc,
								DisplayName = (u.FirstName + " " + u.LastName) ?? u.UserName,
							}).ToList();

			var agencyUsers = (from s in db.Agencies
							   where s.Id == agencyId
							   from u in s.Users
							   where u.Email != null
							   where (u.RoleId == (int)FixedRoles.AgencyUser || u.RoleId == (int)FixedRoles.AgencyUserAndReviewer) && !u.Disabled
							   select new
							   {
								   Email = u.Email,
								   AddToBcc = u.AddToBcc,
								   DisplayName = (u.FirstName + " " + u.LastName) ?? u.UserName,
								   AgencyId = s.Id,
								   AgencyGroupId = s.GroupId
							   }).ToList();

			var poUsers = (from s in db.AgencyGroups
						   from u in s.PoUsers
						   where s.Id == agencyGroupId
						   where !u.Disabled
						   select new
						   {
							   Email = u.Email,
							   AddToBcc = u.AddToBcc,
							   DisplayName = (u.FirstName + " " + u.LastName) ?? u.UserName,
							   AgencyGroupId = s.Id
						   }).ToList();


			foreach (var user in serUsers)
			{
				TryAddAddress(user.Email, user.DisplayName, user.AddToBcc ? msg.Bcc : msg.To);
			}

			foreach (var user in agencyUsers)
			{
				TryAddAddress(user.Email, user.DisplayName, user.AddToBcc ? msg.Bcc : msg.To);
			}

			foreach (var user in poUsers)
			{
				TryAddAddress(user.Email, user.DisplayName, user.AddToBcc ? msg.Bcc : msg.To);
			}

			foreach (var user in gpos)
			{
				TryAddAddress(user.Email, user.DisplayName, user.AddToBcc ? msg.Bcc : msg.To);
			}
		}




		public void Dispose()
		{
			if (this.db != null)
			{
				this.db.Dispose();
			}
		}


	}
}
