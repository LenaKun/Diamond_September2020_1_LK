using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;
using System.Linq.Expressions;
using CC.Data;

namespace CC.Web.Helpers
{
	public static class EmailHelper
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(EmailHelper));


		public static void Add(IQueryable<User> users, MailAddressCollection c)
		{
			if (users != null)
			{
				foreach (var s in users.Select(f => f.Email))
				{
					try
					{
						var m = new MailAddress(s);
						c.Add(m);
					}
					catch (Exception ex)
					{
						log.Error(ex.Message, ex);
					}
				}
			}
		}

		/// <summary>
		/// "All the emails- should be sent to:  To: all of the Ser users + Agency Users assodicated with the agency.  
		/// CC: all of the agency users assodicated with the budget/ report/ client. 
		/// BcC: All of the related RPO, RPA, GPs (agency-> ser-> region), adminMail"
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="agencyId"></param>
		public static void AddRecipientsByAgency(MailMessage msg, int agencyId)
		{
			using (var db = new ccEntities())
			{

				var agencyUsers = from u in db.Users
								  where (u.RoleId == (int)FixedRoles.AgencyUser || u.RoleId == (int)FixedRoles.AgencyUserAndReviewer) && !u.Disabled
								  where u.AgencyId == agencyId
								  select new
								  {
									  DisplayName =
									  (u.FirstName + " " +
									  u.LastName),
									  u.Email,
                                      u.AddToBcc
								  };
				foreach (var user in agencyUsers)
				{
                    if (!user.AddToBcc)
                    {
                        msg.CC.TryAdd(user.Email, user.DisplayName);
                    }
                    else
                    {
                        msg.Bcc.TryAdd(user.Email, user.DisplayName);
                    }
				}

				var serUsers = from u in db.Users
							   where (u.RoleId == (int)FixedRoles.Ser || u.RoleId == (int)FixedRoles.SerAndReviewer) && !u.Disabled
							   from a in u.AgencyGroup.Agencies
							   where a.Id == agencyId
							   select new
							   {
								   DisplayName =
								   (u.FirstName + " " +
								   u.LastName),
								   u.Email,
                                   u.AddToBcc
							   };
				foreach (var user in serUsers)
				{
                    if (!user.AddToBcc)
                    {
                        msg.To.TryAdd(user.Email, user.DisplayName);
                    }
                    else
                    {
                        msg.Bcc.TryAdd(user.Email, user.DisplayName);
                    }
				}

				var rps = from u in db.Users
                          where (u.RoleId == (int)FixedRoles.RegionOfficer || u.RoleId == (int)FixedRoles.RegionAssistant || u.RoleId == (int)FixedRoles.RegionReadOnly) && !u.Disabled
						  from ag in u.AgencyGroups
						  from a in ag.Agencies
						  where a.Id == agencyId
						  select new
						  {
							  DisplayName =
							  (u.FirstName + " " +
							  u.LastName),
							  u.Email,
                              u.AddToBcc
						  };
				foreach (var user in rps)
				{
                    if (!user.AddToBcc)
                    {
                        msg.CC.TryAdd(user.Email, user.DisplayName);
                    }
                    else
                    {
                        msg.Bcc.TryAdd(user.Email, user.DisplayName);
                    }
				}

				var gpos = from u in db.Users
                           where (u.RoleId == (int)FixedRoles.GlobalOfficer || u.RoleId == (int)FixedRoles.GlobalReadOnly || u.RoleId == (int)FixedRoles.AuditorReadOnly) && !u.Disabled
						   select new
						   {
							   DisplayName =
							   (u.FirstName + " " +
							   u.LastName),
							   u.Email,

						   };
				foreach (var user in gpos)
				{
					msg.Bcc.TryAdd(user.Email, user.DisplayName);
				}

			}
			try
			{
				msg.Bcc.Add(System.Web.Configuration.WebConfigurationManager.AppSettings["adminMail"]);
			}
			catch (Exception) { }
		}

		/// <summary>
		/// 		/// "All the emails- should be sent to:  To: all of the Ser users + Agency Users assodicated with the agency.  
		/// CC: all of the agency users assodicated with the budget/ report/ client. 
		/// BcC: All of the related RPO, RPA, GPs (agency-> ser-> region), adminMail"
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="appBudgetId"></param>
		public static void AddRecipeintsByAppBudgetId(MailMessage msg, int appBudgetId)
		{
			using (var db = new ccEntities())
			{
				var agencyUsers = from b in db.AppBudgets
								  where b.Id == appBudgetId
								  from a in b.App.AgencyGroup.Agencies
								  from u in a.Users
								  where (u.RoleId == (int)FixedRoles.AgencyUser || u.RoleId == (int)FixedRoles.AgencyUserAndReviewer) && !u.Disabled
								  select new
								{
									DisplayName =
									(u.FirstName + " " +
									u.LastName),
									u.Email,
                                    u.AddToBcc
								};


				foreach (var user in agencyUsers)
				{
                    if (!user.AddToBcc)
                    {
                        msg.CC.TryAdd(user.Email, user.DisplayName);
                    }
                    else
                    {
                        msg.Bcc.TryAdd(user.Email, user.DisplayName);
                    }
				}

				var serUsers = from b in db.AppBudgets
							   where b.Id == appBudgetId
							   from u in b.App.AgencyGroup.Users
							   where (u.RoleId == (int)FixedRoles.Ser || u.RoleId == (int)FixedRoles.SerAndReviewer) && !u.Disabled
							   select new
							   {
								   DisplayName =
								   (u.FirstName + " " +
								   u.LastName),
								   u.Email,
                                   u.AddToBcc
							   };
				foreach (var user in serUsers)
				{
                    if (!user.AddToBcc)
                    {
                        msg.To.TryAdd(user.Email, user.DisplayName);
                    }
                    else
                    {
                        msg.Bcc.TryAdd(user.Email, user.DisplayName);
                    }
				}

				var rps = from b in db.AppBudgets
						  where b.Id == appBudgetId
						  from u in b.App.AgencyGroup.PoUsers
                          where (u.RoleId == (int)FixedRoles.RegionOfficer || u.RoleId == (int)FixedRoles.RegionAssistant || u.RoleId == (int)FixedRoles.RegionReadOnly) && !u.Disabled
						  select new
						  {
							  DisplayName =
							  (u.FirstName + " " +
							  u.LastName),
							  u.Email,
                              u.AddToBcc
						  };
				foreach (var user in rps)
				{
                    if (!user.AddToBcc)
                    {
                        msg.CC.TryAdd(user.Email, user.DisplayName);
                    }
                    else
                    {
                        msg.Bcc.TryAdd(user.Email, user.DisplayName);
                    }
				}

				var gpos = from u in db.Users
                           where (u.RoleId == (int)FixedRoles.GlobalOfficer || u.RoleId == (int)FixedRoles.GlobalReadOnly || u.RoleId == (int)FixedRoles.AuditorReadOnly) && !u.Disabled
						   select new
						   {
							   DisplayName =
							   (u.FirstName + " " +
							   u.LastName),
							   u.Email,

						   };
				foreach (var user in gpos)
				{
					msg.Bcc.TryAdd(user.Email, user.DisplayName);
				}

				try
				{
					msg.Bcc.Add(System.Web.Configuration.WebConfigurationManager.AppSettings["adminMail"]);
				}
				catch (Exception) { }
			}
		}

		/// <summary>
		/// 		/// "All the emails- should be sent to:  To: all of the Ser users + Agency Users assodicated with the agency.  
		/// CC: all of the agency users assodicated with the budget/ report/ client. 
		/// BcC: All of the related RPO, RPA, GPs (agency-> ser-> region), adminMail"
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="mainReportId"></param>
		public static void AddRecipeintsByMainReportId(MailMessage msg, int mainReportId)
		{
			using (var db = new ccEntities())
			{
				var agencyUsers = from mr in db.MainReports
								  where mr.Id == mainReportId
								  from a in mr.AppBudget.App.AgencyGroup.Agencies
								  from u in a.Users
								  where (u.RoleId == (int)FixedRoles.AgencyUser || u.RoleId == (int)FixedRoles.AgencyUserAndReviewer) && !u.Disabled
								  select new
								  {
									  DisplayName =
									  (u.FirstName + " " +
									  u.LastName),
									  u.Email,
                                      u.AddToBcc
								  };


				foreach (var user in agencyUsers)
				{
                    if (!user.AddToBcc)
                    {
                        msg.CC.TryAdd(user.Email, user.DisplayName);
                    }
                    else
                    {
                        msg.Bcc.TryAdd(user.Email, user.DisplayName);
                    }
				}

				var serUsers = from mr in db.MainReports
							   where mr.Id == mainReportId
							   from u in mr.AppBudget.App.AgencyGroup.Users
							   where (u.RoleId == (int)FixedRoles.Ser || u.RoleId == (int)FixedRoles.SerAndReviewer) && !u.Disabled
							   select new
							   {
								   DisplayName =
								   (u.FirstName + " " +
								   u.LastName),
								   u.Email,
                                   u.AddToBcc
							   };
				foreach (var user in serUsers)
				{
                    if (!user.AddToBcc)
                    {
                        msg.To.TryAdd(user.Email, user.DisplayName);
                    }
                    else
                    {
                        msg.Bcc.TryAdd(user.Email, user.DisplayName);
                    }
				}

				var rps = from mr in db.MainReports
						  where mr.Id == mainReportId
						  from u in mr.AppBudget.App.AgencyGroup.PoUsers
                          where (u.RoleId == (int)FixedRoles.RegionOfficer || u.RoleId == (int)FixedRoles.RegionAssistant || u.RoleId == (int)FixedRoles.RegionReadOnly) && !u.Disabled
						  select new
						  {
							  DisplayName =
							  (u.FirstName + " " +
							  u.LastName),
							  u.Email,
                              u.AddToBcc
						  };
				foreach (var user in rps)
				{
                    if (!user.AddToBcc)
                    {
                        msg.CC.TryAdd(user.Email, user.DisplayName);
                    }
                    else
                    {
                        msg.Bcc.TryAdd(user.Email, user.DisplayName);
                    }
				}

				var gpos = from u in db.Users
                           where (u.RoleId == (int)FixedRoles.GlobalOfficer || u.RoleId == (int)FixedRoles.GlobalReadOnly || u.RoleId == (int)FixedRoles.AuditorReadOnly) && !u.Disabled
						   select new
						   {
							   DisplayName =
							   (u.FirstName + " " +
							   u.LastName),
							   u.Email,

						   };
				foreach (var user in gpos)
				{
					msg.Bcc.TryAdd(user.Email, user.DisplayName);
				}

			}
			try
			{
				msg.Bcc.Add(System.Web.Configuration.WebConfigurationManager.AppSettings["adminMail"]);
			}
			catch (Exception) { }
		}

		public static void AddRecipientsMainReportForRpaApproval(MailMessage msg, MainReport report)
		{
			using (var db = new ccEntities())
			{
				var rps = from mr in db.MainReports
						  where mr.Id == report.Id
						  from u in mr.AppBudget.App.AgencyGroup.PoUsers
                          where (u.RoleId == (int)FixedRoles.RegionOfficer || u.RoleId == (int)FixedRoles.RegionAssistant || u.RoleId == (int)FixedRoles.RegionReadOnly) && !u.Disabled
						  select new
						  {
							  DisplayName =
							  (u.FirstName + " " +
							  u.LastName),
							  u.Email,
                              u.AddToBcc,
							  u.RoleId

						  };
				foreach (var user in rps)
				{
					if (user.RoleId == (int)FixedRoles.RegionOfficer)
					{
                        if (!user.AddToBcc)
                        {
                            msg.To.TryAdd(user.Email, user.DisplayName);
                        }
                        else
                        {
                            msg.Bcc.TryAdd(user.Email, user.DisplayName);
                        }
					}
					else
					{
                        if (!user.AddToBcc)
                        {
                            msg.CC.TryAdd(user.Email, user.DisplayName);
                        }
                        else
                        {
                            msg.Bcc.TryAdd(user.Email, user.DisplayName);
                        }
					}
				}
			}
		}
		private static void TryAdd(this MailAddressCollection mailAddressCollection, string email, string displayName)
		{
			try
			{
				var address = new MailAddress(email, displayName);
				mailAddressCollection.Add(address);
			}
			catch (Exception)
			{

			}
		}

		internal static void AddRecipeintsForAppBudgetRpoApproval(MailMessage msg, AppBudget appBudget)
		{
			using (var db = new ccEntities())
			{

				var rps = from b in db.AppBudgets
						  where b.Id == appBudget.Id
						  from u in b.App.AgencyGroup.PoUsers
                          where (u.RoleId == (int)FixedRoles.RegionOfficer || u.RoleId == (int)FixedRoles.RegionReadOnly) && !u.Disabled
						  select new
						  {
							  DisplayName =
							  (u.FirstName + " " +
							  u.LastName),
							  u.Email,
                              u.AddToBcc
						  };
				foreach (var user in rps)
				{
                    if (!user.AddToBcc)
                    {
                        msg.CC.TryAdd(user.Email, user.DisplayName);
                    }
                    else
                    {
                        msg.Bcc.TryAdd(user.Email, user.DisplayName);
                    }
				}

				var gpos = from u in db.Users
                           where (u.RoleId == (int)FixedRoles.GlobalOfficer || u.RoleId == (int)FixedRoles.GlobalReadOnly || u.RoleId == (int)FixedRoles.AuditorReadOnly) && !u.Disabled
						   select new
						   {
							   DisplayName =
							   (u.FirstName + " " +
							   u.LastName),
							   u.Email,
                               u.AddToBcc
						   };
				foreach (var user in gpos)
				{
                    if (!user.AddToBcc)
                    {
                        msg.To.TryAdd(user.Email, user.DisplayName);
                    }
                    else
                    {
                        msg.Bcc.TryAdd(user.Email, user.DisplayName);
                    }
				}

			}
		}

        public static void AddRecipeintsForHcMaximumAllowed(MailMessage msg, int mainReportId)
        {
            using (var db = new ccEntities())
            {
                var rps = from mr in db.MainReports
                          where mr.Id == mainReportId
                          from u in mr.AppBudget.App.AgencyGroup.PoUsers
                          where (u.RoleId == (int)FixedRoles.RegionOfficer || u.RoleId == (int)FixedRoles.RegionAssistant || u.RoleId == (int)FixedRoles.RegionReadOnly) && !u.Disabled
                          select new
                          {
                              DisplayName =
                              (u.FirstName + " " +
                              u.LastName),
                              u.Email,
                              u.AddToBcc
                          };
                foreach (var user in rps)
                {
                    if (!user.AddToBcc)
                    {
                        msg.CC.TryAdd(user.Email, user.DisplayName);
                    }
                    else
                    {
                        msg.Bcc.TryAdd(user.Email, user.DisplayName);
                    }
                }

                var gpos = from u in db.Users
                           where (u.RoleId == (int)FixedRoles.GlobalOfficer || u.RoleId == (int)FixedRoles.GlobalReadOnly || u.RoleId == (int)FixedRoles.AuditorReadOnly) && !u.Disabled
                           select new
                           {
                               DisplayName =
                               (u.FirstName + " " +
                               u.LastName),
                               u.Email,
                               u.AddToBcc
                           };
                foreach (var user in gpos)
                {
                    if (!user.AddToBcc)
                    {
                        msg.To.TryAdd(user.Email, user.DisplayName);
                    }
                    else
                    {
                        msg.Bcc.TryAdd(user.Email, user.DisplayName);
                    }
                }
            }
        }

		public static void SendHCYTDExceedingNotification(List<CC.Data.Queries.HCWeeklyCapValidationResult> source, int mainReportId, int appId, decimal percentage)
		{
			using (var smtpClient = new System.Net.Mail.SmtpClient())
			{
                foreach (var a in source.GroupBy(f => f.AgencyId))
                {
                    try
                    {
                        var msg = new System.Net.Mail.MailMessage();
                        msg.IsBodyHtml = true;
                        msg.Subject = string.Format("Homecare Reporting Trending Above Yearly Maximum Reporting Amount - {0}", a.FirstOrDefault().AgencyName);
                        msg.Body = YTDExceedingMsgBody(source, appId, percentage);

                        AddRecipeintsForHcYTDExceeding(msg, mainReportId, a.Key);
                        smtpClient.Send(msg);
                        log.InfoFormat("sent SendHCYTDExceedingNotification email");
                    }
                    catch (Exception ex)
                    {
                        log.Error("SendHCYTDExceedingNotification error sending email: " + ex.Message);
                        throw ex;
                    }
                }
			}
		}

		public static void AddRecipeintsForHcYTDExceeding(MailMessage msg, int mainReportId, int agencyId)
		{
			using (var db = new ccEntities())
			{
				var rps = from mr in db.MainReports
						  where mr.Id == mainReportId
						  from u in mr.AppBudget.App.AgencyGroup.PoUsers
						  where (u.RoleId == (int)FixedRoles.RegionOfficer || u.RoleId == (int)FixedRoles.RegionAssistant || u.RoleId == (int)FixedRoles.RegionReadOnly) && !u.Disabled
						  select new
						  {
							  DisplayName =
							  (u.FirstName + " " +
							  u.LastName),
							  u.Email,
							  u.AddToBcc
						  };
				foreach (var user in rps)
				{
					msg.Bcc.TryAdd(user.Email, user.DisplayName);
				}

				var gpos = from u in db.Users
						   where (u.RoleId == (int)FixedRoles.GlobalOfficer || u.RoleId == (int)FixedRoles.GlobalReadOnly || u.RoleId == (int)FixedRoles.AuditorReadOnly || u.RoleId == (int)FixedRoles.Admin) && !u.Disabled
						   select new
						   {
							   DisplayName =
							   (u.FirstName + " " +
							   u.LastName),
							   u.Email,
							   u.AddToBcc
						   };
				foreach (var user in gpos)
				{
					msg.Bcc.TryAdd(user.Email, user.DisplayName);
				}

				var gs = from mr in db.MainReports
						  where mr.Id == mainReportId
						  from u in mr.AppBudget.App.AgencyGroup.Users
						 where (u.RoleId == (int)FixedRoles.Ser || u.RoleId == (int)FixedRoles.SerAndReviewer) && !u.Disabled
						  select new
						  {
							  DisplayName =
							  (u.FirstName + " " +
							  u.LastName),
							  u.Email,
							  u.AddToBcc
						  };
				foreach (var user in gs)
				{
					if (!user.AddToBcc)
					{
						msg.To.TryAdd(user.Email, user.DisplayName);
					}
					else
					{
						msg.Bcc.TryAdd(user.Email, user.DisplayName);
					}
				}

				var ags = from mr in db.MainReports
						  where mr.Id == mainReportId
						  from a in mr.AppBudget.App.AgencyGroup.Agencies
						  from u in a.Users
						  where (u.RoleId == (int)FixedRoles.AgencyUser || u.RoleId == (int)FixedRoles.AgencyUserAndReviewer) && !u.Disabled
                          where u.AgencyId == agencyId
						  select new
						  {
							  DisplayName =
							  (u.FirstName + " " +
							  u.LastName),
							  u.Email,
							  u.AddToBcc
						  };
				foreach (var user in ags)
				{
					if (!user.AddToBcc)
					{
						msg.To.TryAdd(user.Email, user.DisplayName);
					}
					else
					{
						msg.Bcc.TryAdd(user.Email, user.DisplayName);
					}
				}
			}
		}

		private static string YTDExceedingMsgBody(List<CC.Data.Queries.HCWeeklyCapValidationResult> clients, int appId, decimal percentage)
		{
			using (var db = new ccEntities())
			{
				var sb = new System.Text.StringBuilder();
                if(percentage.Format().ToString().Contains("100"))
                {
                    sb.Append(@"
				<p>The following clients have been reported ");
                    sb.Append(percentage.Format().ToString());
                    sb.Append(@"% of the allowable reporting amount as noted in the Allocation Letter. 
Please reduce Homecare provided (either Hours or Rate) in the next quarter or Homecare services may have to be discontinued before the end of the year. 
(If applicable, only the first 100 clients meeting these criteria are appearing on this notification). Please contact your Program Assistant or Program Officer if you have any questions regarding this e-mail.</p>

<p>Clients Details:</p>
<table>
    <thead>
    </thead>
    <tbody><tr><td>CC ID</td><td>First Name</td><td>Last Name</td><td>Total amount reported YTD for this fund</td><td>CUR</td></tr>");
                }
                else
                {
                    sb.Append(@"
				<p>The following clients have been reported more than ");
                    sb.Append(percentage.Format().ToString());
                    sb.Append(@"% of the allowable reporting amount as noted in the Allocation Letter. 
Please reduce Homecare provided (either Hours or Rate) in the next quarter or Homecare services may have to be discontinued before the end of the year. 
(If applicable, only the first 100 clients meeting these criteria are appearing on this notification). Please contact your Program Assistant or Program Officer if you have any questions regarding this e-mail.</p>

<p>Clients Details:</p>
<table>
    <thead>
    </thead>
    <tbody><tr><td>CC ID</td><td>First Name</td><td>Last Name</td><td>Total amount reported YTD for this fund</td><td>CUR</td></tr>");
                };

				foreach (var c in clients)
				{
					var amount = c.TotalAmount * db.viewAppExchangeRates.FirstOrDefault(f => f.AppId == appId && f.ToCur == c.Cur).Value;
					sb.Append(@"<tr><td>").Append(c.ClientId).Append(@"</td>");
					sb.Append(@"<td>").Append(c.ClientFirstName).Append(@"</td>");
					sb.Append(@"<td>").Append(c.ClientLastName).Append(@"</td>");
					sb.Append(@"<td>").Append(amount.Format()).Append(@"</td>");
					sb.Append(@"<td>").Append(c.Cur).Append(@"</td>");
				}
				sb.Append(@"</tbody></table>");
				return sb.ToString();
			}
		}

		public static void SendNewSerOrgNotification(bool Ser, int Id, string Name)
		{
			using (var smtpClient = new System.Net.Mail.SmtpClient())
			{
				try
				{
					var msg = new System.Net.Mail.MailMessage();
					msg.IsBodyHtml = true;
					string subject = "";
					var sb = new System.Text.StringBuilder();
					sb.Append(@"<p></p>");
					if (Ser)
					{
						subject = string.Format("New Ser Notification");
						sb.Append("<p>New Ser #");
					}
					else
					{
						subject = string.Format("New Org Notification");
						sb.Append("<p>New Org #");
					}
					sb.Append(Id).Append(" ").Append(Name).Append(", has been created in Diamond</p>");

					msg.Subject = subject;
					msg.Body = sb.ToString();
					IEnumerable<string> emailsList = GlobalDbSettings.GetString(GlobalDbSettings.GlobalStringNames.NewSerOrgNotifyEmail).Split(',').ToList().Select(f => f.Trim());
					string emails = string.Join(",", emailsList);
					msg.To.Add(emails);
					smtpClient.Send(msg);
					log.InfoFormat("sent SendNewSerOrgNotification email");
				}
				catch (Exception ex)
				{
					log.Error("SendNewSerOrgNotification error sending email: " + ex.Message);
				}
			}			
		}
	}
}
