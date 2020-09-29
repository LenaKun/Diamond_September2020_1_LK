using CC.Data;
using CC.Web.Helpers;
using Quartz;
using System;
using System.Collections.Generic;
using System.Data.Objects.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace CC.Web.Jobs
{
    public class HcMaximumAllowedCheckJob : LoggingJob
    {
		public HcMaximumAllowedCheckJob()
		{
			this.log = log4net.LogManager.GetLogger(typeof(HcMaximumAllowedCheckJob));
		}
		protected override void ExecuteInternal(IJobExecutionContext contex)
		{
            DateTime today = DateTime.Now;
            DateTime pastMonth = DateTime.Now.AddDays(-30);
            using (var db = new ccEntities())
            {
                var q = (from g in
                             (
                              from mr in db.MainReports.Where(MainReport.Submitted)
                                  .Where(f => (f.SubmittedAt >= pastMonth || f.ApprovedAt >= pastMonth) && f.C168OK == null)
                              join sr in db.SubReports on mr.Id equals sr.MainReportId
                              where sr.AppBudgetService.Service.TypeId == 8
                              join cr in db.ClientReports on sr.Id equals cr.SubReportId
                              join ar in db.ClientAmountReports on cr.Id equals ar.ClientReportId
                              join c in db.Clients on cr.ClientId equals c.Id
                              select new
                              {
                                  MainReportId = mr.Id,
                                  MasterId = c.MasterId ?? c.Id,
                                  StartDate = ar.ReportDate,
                                  Quantity = ar.Quantity
                              }
                             )
                         group g by new { g.MainReportId, g.MasterId, g.StartDate } into gg
                         select new ClientsExceeded
                               {
                                   MainReportId = gg.Key.MainReportId,
                                   ClientId = gg.Key.MasterId,
                                   ReportDate = gg.Key.StartDate,
                                   Quantity = gg.Sum(f => f.Quantity)
                               }).Distinct().OrderBy(f => f.ClientId).GroupBy(f => f.MainReportId).ToList();
                var clientsExceeded = new List<ClientsExceeded>();
                foreach (var item in q)
                {
                    var mr = db.MainReports.SingleOrDefault(f => f.Id == item.Key);
                    var result = (from m in db.MainReports.Where(f => f.Id == mr.Id)
                            select new
                            {
                                Id = m.Id,
                                Fund = m.AppBudget.App.Fund.Name,
                                App = m.AppBudget.App.Name,
                                Start = m.Start,
                                End = m.End,
                                Ser = m.AppBudget.App.AgencyGroup.Name
                            }).SingleOrDefault();
                    var mainReportDetails = new MainReportEmailDetails();
                    if(result != null)
                    {
                        mainReportDetails.Id = result.Id;
                        mainReportDetails.Fund = result.Fund;
                        mainReportDetails.App = result.App;
                        mainReportDetails.MonthFrom = result.Start.ToString("MMM yyyy");
                        mainReportDetails.MonthTo = result.End.AddDays(-1).ToString("MMM yyyy");
                        mainReportDetails.Ser = result.Ser;
                    }
                    bool _168ok = true;
                    clientsExceeded.Clear();
                    foreach (var c in item)
                    {
                        if (c.Quantity / DateTime.DaysInMonth(c.ReportDate.Year, c.ReportDate.Month) > 24)
                        {
                            clientsExceeded.Add(c);
                            _168ok = false;
                            log.Debug(string.Format("HcMaximumAllowedCheckJob: client exceeded - main report id: {0}, client id: {1}, quantity: {2}, report date: {3}",
                                mainReportDetails.Id, c.ClientId, c.Quantity, c.ReportDate.ToString("dd MMM yyyy")));
                        }
                    }                    
                    if (!_168ok)
                    {
                        using (var smtpClient = new System.Net.Mail.SmtpClient())
                        {
                            try
                            {
                                var msg = new System.Net.Mail.MailMessage();
                                msg.IsBodyHtml = true;
                                msg.Subject = string.Format("Notification of Clients Exceeded Maximum Allowed HC Hours/Week");
                                msg.Body = msgbody(clientsExceeded, mainReportDetails);
                                var newComment = new Comment()
                                {
                                    UserId = db.Users.FirstOrDefault(f => f.UserName == "sysadmin" || f.UserName == "ccdevadmin").Id,
                                    Date = DateTime.Now,
                                    Content = msg.Body
                                };

                                mr.InternalComments.Add(newComment);

                                EmailHelper.AddRecipeintsForHcMaximumAllowed(msg, item.Key);
                                smtpClient.Send(msg);
                                log.InfoFormat("sent HcMaximumAllowedCheckJob email");
                            }
                            catch (Exception ex)
                            {
                                log.Error("HcMaximumAllowedCheckJob error sending email: " + ex.Message);
                            }
                        }
                    }
                    else
                    {
                        mr.C168OK = DateTime.Now;
                    }
                }
                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    log.Error("HcMaximumAllowedCheckJob SaveChanges error: " + ex.Message, ex);
                }
            }
        }

        public class ClientsExceeded
        {
            public int ClientId { get; set; }
            public decimal Quantity { get; set; }
            public DateTime ReportDate { get; set; }
            public int MainReportId { get; set; }
        }

        public class MainReportEmailDetails
        {
            public int Id { get; set; }
            public string Fund { get; set; }
            public string App { get; set; }
            public string MonthFrom { get; set; }
            public string MonthTo { get; set; }
            public string Ser { get; set; }
        }

        private static string msgbody(List<ClientsExceeded> model, MainReportEmailDetails mr)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append(@"<p></p>

<p>The following CC IDs have been reported as receiving more than 168 hours per week on recently submitted report:</p>

<p>Report Details:</p>
<table>
    <thead>
    </thead>
    <tbody>
    <tr><td>Report Id: ").Append(mr.Id).Append(@"</td></tr>");
sb.Append(@"<tr><td>Fund: ").Append(mr.Fund).Append(@"</td></tr>");
sb.Append(@"<tr><td>App #: ").Append(mr.App).Append(@"</td></tr>");
sb.Append(@"<tr><td>Month From: ").Append(mr.MonthFrom).Append(@"</td></tr>");
sb.Append(@"<tr><td>Month To: ").Append(mr.MonthTo).Append(@"</td></tr>");
sb.Append(@"<tr><td>Ser: ").Append(mr.Ser).Append(@"</td></tr>");
sb.Append(@"</tbody></table><table>
    <thead>
        <tr>
            <th>CC ID</th>
        </tr>
    </thead><tbody>");
            foreach (var c in model)
            {
                string baseUrl = System.Web.Configuration.WebConfigurationManager.AppSettings["BaseUrl"];
                string url = string.Format("{0}MainReports/Details/{1}", baseUrl, c.MainReportId);
                sb.Append(@"<tr><td><a href='");
                sb.Append(url).Append(@"'>").Append(c.ClientId.ToString()).Append(@"</a></td></tr>");
            }
            sb.Append(@"</tbody></table>");
            return sb.ToString();
        }
    }
}