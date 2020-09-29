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
	public class StatusChangeNotificationsJob : LoggingJob
    {

		public StatusChangeNotificationsJob()
		{
			this.log = log4net.LogManager.GetLogger(typeof(StatusChangeNotificationsJob));
		}
		protected override void ExecuteInternal(IJobExecutionContext contex)
		{
			SendNotifications(DateTime.Now.AddDays(-1), DateTime.Now);
		}

        public void SendNotifications(DateTime f, DateTime t)
		{

			using (var db = new ccEntities())
			{
				var q = from h in db.Histories
						where h.TableName == "Clients"
						where h.FieldName == "ApprovalStatusId"
						where h.UpdateDate >= f
						where h.UpdateDate <= t
						join c in db.Clients on h.ReferenceId equals c.Id
						where c.LeaveReasonId == null || c.LeaveReasonId != (int)LeaveReasonEnum.Deceased
						let hcStatusName = (from s in c.ClientHcStatuses
											where s.StartDate <= DateTime.Now.Date
											orderby s.StartDate descending
											select s.HcStatus.Name).FirstOrDefault()
						select new
						{
							ClientId = c.Id,
							AgencyId = c.AgencyId,
							AgencyName = c.Agency.Name,
							AgencyGroupId = c.Agency.GroupId,
							OldStatusId = h.OldValue,
							NewStatusId = h.NewValue,
							CurStatusId = c.ApprovalStatusId,
							c.FirstName,
							c.LastName,
							HcStatusName = hcStatusName
						};
				var a = from item in q.ToList()
						where item.CurStatusId == item.NewStatusId.Parse<int>()
						select new ImportClientFundStatusProc_Result
						{
							AgencyGroupId = item.AgencyGroupId,
							ClientId = item.ClientId,
							FirstName = item.FirstName,
							LastName = item.LastName,
							NewApprovalStatusId = item.NewStatusId.Parse<int>(),
							OldApprovalStatusId = item.OldStatusId.Parse<int>(),
							HcStatusName = item.HcStatusName
						};
				var b = a.Distinct();
				var result = SendStatusChangeNotifications(b);

			}

		}
		private static string msgbody(ClientStatusChangeEmailModel model)
		{
			var sb = new System.Text.StringBuilder();
			sb.Append(@"<p></p>

<p>This email delivers official notice of Change of Status for ").Append(model.Clients.Count()).Append(@" of your agency's clients.  </p>

<p>See list below. </p>

<p>If you have any questions or concerns, please contact your Program Assistant. </p>

<table>
    <thead>
        <tr>
            <th>CC ID</th>
            <th>Name</th>
            <th>Old JNV Approval Status</th>
            <th>New JNV Approval Status</th>
            <th>Homecare Approval Status</th>
        </tr>
    </thead>");
			foreach (var c in model.Clients)
			{
				sb.Append(@"<tbody>
            <tr>
                <td>").Append(c.ClientId).Append(@"</td>
                <td>").Append(c.FirstName).Append(" ").Append(c.LastName).Append(@"</td>
                <td>").Append(((CC.Data.ApprovalStatusEnum)c.OldApprovalStatusId).DisplayName()).Append(@"</td>
                <td>").Append(((CC.Data.ApprovalStatusEnum)c.NewApprovalStatusId).DisplayName()).Append(@"</td>
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

		public Dictionary<int, int> SendStatusChangeNotifications(IEnumerable<ImportClientFundStatusProc_Result> clients)
		{
			var result = new Dictionary<int, int>();
			using (var db = new ccEntities())
			{
				var gpos = db.Users.Where(f => f.RoleId == (int)FixedRoles.GlobalOfficer);
				var groups = from c in clients
							 group c by c.AgencyGroupId into cg
							 select cg;
				var serUsers = (from s in db.AgencyGroups
								from u in s.Users
								where u.Email != null
								where (u.RoleId == (int)FixedRoles.Ser || u.RoleId == (int)FixedRoles.SerAndReviewer) && !u.Disabled
								select new
								{
									Email = u.Email,
                                    AddToBcc = u.AddToBcc,
									DisplayName = (u.FirstName + " " + u.LastName) ?? u.UserName,
									AgencyGroupId = s.Id
								}).ToList();

                var agencyUsers = (from s in db.Agencies
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
                               where !u.Disabled
							   select new
							   {
								   Email = u.Email,
                                   AddToBcc = u.AddToBcc,
								   DisplayName = (u.FirstName + " " + u.LastName) ?? u.UserName,
								   AgencyGroupId = s.Id
							   }).ToList();
				foreach (var g in groups.OrderBy(f => f.Key))
				{
					var msg = new System.Net.Mail.MailMessage();

					foreach (var user in serUsers.Where(f => f.AgencyGroupId == g.Key))
					{
						try 
                        {
                            if (!user.AddToBcc)
                            {
                                msg.To.Add(new System.Net.Mail.MailAddress(user.Email, user.DisplayName));
                            }
                            else
                            {
                                msg.Bcc.Add(new System.Net.Mail.MailAddress(user.Email, user.DisplayName));
                            }
                        }
						catch (Exception ex) { log.Info(ex.Message, ex); }
					}

                    foreach (var user in agencyUsers.Where(f => f.AgencyGroupId == g.Key))
                    {
                        try
                        {
                            if (!user.AddToBcc)
                            {
                                msg.CC.Add(new System.Net.Mail.MailAddress(user.Email, user.DisplayName));
                            }
                            else
                            {
                                msg.Bcc.Add(new System.Net.Mail.MailAddress(user.Email, user.DisplayName));
                            }
                        }
                        catch (Exception ex) { log.Info(ex.Message, ex); }
                    }

					foreach (var user in poUsers.Where(f => f.AgencyGroupId == g.Key))
					{
                        try
                        {
                            if (!user.AddToBcc)
                            {
                                msg.CC.Add(new System.Net.Mail.MailAddress(user.Email, user.DisplayName));
                            }
                            else
                            {
                                msg.Bcc.Add(new System.Net.Mail.MailAddress(user.Email, user.DisplayName));
                            }
                        }
						catch (Exception ex) { log.Info(ex.Message, ex); }
					}

					foreach (var user in gpos)
					{
                        try
                        {
                            msg.Bcc.Add(new System.Net.Mail.MailAddress(user.Email, user.UserName));
                        }
						catch (Exception ex) { log.Info(ex.Message, ex); }
					}

					var groupClients = g.ToList();
					var agencygroup = db.AgencyGroups.SingleOrDefault(f => f.Id == g.Key);

					msg.IsBodyHtml = true;
					msg.Subject = string.Format("Notification of Client Status Change (SER {0})", agencygroup.DisplayName);
					msg.Body = msgbody(
						new ClientStatusChangeEmailModel { Clients = groupClients });

					using (var smtpClient = new System.Net.Mail.SmtpClient())
					{

						smtpClient.Send(msg);
						result.Add(g.Key, g.Count());
						log.InfoFormat("{0} ({1} clients), to: {2}, cc: {3}, bcc: {4}", msg.Subject, g.Count(), msg.To, msg.CC, msg.Bcc);
					}

				}
				return result;
			}
		}
		
		
    }
}
