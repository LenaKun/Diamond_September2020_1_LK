using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CC.Data;
using CC.Web.Models;
using CC.Web.Areas.Admin.Models;
using Quartz;
using System.ComponentModel.DataAnnotations;
using CC.Web.Helpers;

namespace CC.Web.Jobs
{
	public class CfsDailyDigestJob : LoggingJob
	{
		public CfsDailyDigestJob()
		{
			this.log = log4net.LogManager.GetLogger(typeof(StatusChangeNotificationsJob));
		}

		protected override void ExecuteInternal(IJobExecutionContext contex)
		{
			SendCfsDailyNotification();
		}

		public void SendCfsDailyNotification()
		{
			using(var db = new ccEntities())
			{
                GlobalDbSettings.Set(GlobalDbSettings.GlobalStringNames.CfsDailyDigestLastDateTime, DateTime.Now.ToString("M/d/yyyy h:mm:ss tt"));
                var today = DateTime.Today.AddDays(-1);
				var qsd = (from cfs in db.CfsRows
						   where cfs.StartDate != null && cfs.UpdatedAt >= today
						   join h in db.Histories on cfs.ClientId equals h.ReferenceId
						   where h.TableName == "Clients" && h.FieldName == "CFS_StartDate" && h.UpdateDate >= today
                           select new
						   {
							   ClientId = cfs.ClientId,
							   FirstName = cfs.Client.FirstName,
							   LastName = cfs.Client.LastName,
							   StartDateStr = h.NewValue,
							   StartDate = cfs.StartDate,
							   AgencyGroupId = cfs.Client.Agency.GroupId,
							   AgencyId = cfs.Client.AgencyId,
                               AgencyName = cfs.Client.Agency.Name
						   }).ToList();

				var startDates = from c in qsd
								 where c.StartDate.Value.ToString("M/d/yyyy h:mm:ss tt") == c.StartDateStr
								 select new CfsDailyRow
								 {
									 ClientId = c.ClientId,
									 FirstName = c.FirstName,
									 LastName = c.LastName,
									 StartDate = c.StartDate,
									 AgencyGroupId = c.AgencyGroupId,
									 AgencyId = c.AgencyId,
                                     AgencyName = c.AgencyName
								 };

				var qed = (from cfs in db.CfsRows
						 where cfs.EndDate != null && cfs.UpdatedAt >= today
						join h in db.Histories on cfs.ClientId equals h.ReferenceId
						where h.TableName == "Clients" && h.FieldName == "CFS_EndDate" && h.UpdateDate >= today
						select new
						{
							ClientId = cfs.ClientId,
							FirstName = cfs.Client.FirstName,
							LastName = cfs.Client.LastName,
							EndDateStr = h.NewValue,
							EndDate = cfs.EndDate,
							Reason = cfs.CfsEndDateReason.Name,
							AgencyGroupId = cfs.Client.Agency.GroupId,
							AgencyId = cfs.Client.AgencyId,
                            AgencyName  = cfs.Client.Agency.Name
						}).ToList();

				var endDates = from c in qed
							   where c.EndDate.Value.ToString("M/d/yyyy h:mm:ss tt") == c.EndDateStr
							   select new CfsDailyRow
							   {
								   ClientId = c.ClientId,
								   FirstName = c.FirstName,
								   LastName = c.LastName,
								   EndDate = c.EndDate,
								   EndDateReason = c.Reason,
								   AgencyGroupId = c.AgencyGroupId,
								   AgencyId = c.AgencyId,
                                   AgencyName = c.AgencyName
							   };

				var allDates = startDates.Union(endDates);

				var result = SendDatesChangeNotifications(allDates.ToList());
			}
		}

		private static string msgbody(List<CfsDailyRow> model)
		{
			var sb = new System.Text.StringBuilder();
			sb.Append(@"<p>Your agency will receive this daily report on any business day when a client of your agency has a start date or end date that is newly entered, updated, or triggered by end of eligibility (such as a deceased date entered into Diamond).</p>

<p>PART 1: The following clients have a START DATE for CFS. As of a client’s start date, you will be unable to report on them for homecare services.</p>
<table>
    <thead>
        <tr>
            <th>CC ID</th>
            <th>First Name</th>
            <th>Last Name</th>
            <th>Start Date</th>
            <th>Agency Name</th>
        </tr>
    </thead>");
			foreach (var c in model.Where(f => f.StartDate.HasValue))
			{
				sb.Append(@"<tbody>
            <tr>
                <td>").Append(c.ClientId).Append(@"</td>
                <td>").Append(c.FirstName).Append(@"</td>
				<td>").Append(c.LastName).Append(@"</td>
				<td>").Append(c.StartDate.Value.ToString("dd MMM yyyy")).Append(@"</td>
				<td>").Append(c.AgencyName).Append(@"</td>

            </tr>
        </tbody>");
			}
			sb.Append(@"</table>

<p>PART 2: The following clients have an END DATE for CFS.</p>
<table>
    <thead>
        <tr>
            <th>CC ID</th>
            <th>First Name</th>
            <th>Last Name</th>
            <th>End Date</th>
			<th>Reason</th>
            <th>Agency Name</th>
        </tr>

    </thead>");
			foreach (var c in model.Where(f => f.EndDate.HasValue))
			{
				sb.Append(@"<tbody>
            <tr>
                <td>").Append(c.ClientId).Append(@"</td>
                <td>").Append(c.FirstName).Append(@"</td>
				<td>").Append(c.LastName).Append(@"</td>
				<td>").Append(c.EndDate.Value.ToString("dd MMM yyyy")).Append(@"</td>
				<td>").Append(c.EndDateReason).Append(@"</td>
                <td>").Append(c.AgencyName).Append(@"</td>
            </tr>
        </tbody>");
			}
			sb.Append(@"</table>");
			return sb.ToString();
		}

		public Dictionary<int, int> SendDatesChangeNotifications(List<CfsDailyRow> clients)
		{
			var result = new Dictionary<int, int>();
			using (var db = new ccEntities())
			{
				var gpos = db.Users.Where(f => f.RoleId == (int)FixedRoles.GlobalOfficer);
				var groups = from c in clients
							 group c by c.AgencyId into cg
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

					foreach (var user in serUsers.Where(f => f.AgencyGroupId == g.FirstOrDefault().AgencyGroupId))
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

					foreach (var user in agencyUsers.Where(f => f.AgencyId == g.Key))
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

					foreach (var user in poUsers.Where(f => f.AgencyGroupId == g.FirstOrDefault().AgencyGroupId))
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
					msg.Subject = string.Format("CFS Daily Digest");
					msg.Body = msgbody(groupClients);

					try
					{
						using (var smtpClient = new System.Net.Mail.SmtpClient())
						{
							smtpClient.Send(msg);
							result.Add(g.Key, g.Count());
							log.InfoFormat("{0} ({1} clients), to: {2}, cc: {3}, bcc: {4}", msg.Subject, g.Count(), msg.To, msg.CC, msg.Bcc);
						}
					}
					catch(Exception ex)
					{
						log.Info(ex.Message, ex);
					}

				}
				return result;
			}
		}

		public class CfsDailyRow
		{
			public int ClientId { get; set; }
			public string FirstName { get; set; }
			public string LastName { get; set; }
			public DateTime? StartDate { get; set; }
			public DateTime? EndDate { get; set; }
			public string EndDateReason { get; set; }
			public int AgencyGroupId { get; set; }
			public int AgencyId { get; set; }
            public string AgencyName { get; set; }
        }
	}
}