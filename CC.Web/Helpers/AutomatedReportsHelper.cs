using CC.Data;
using CC.Web.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace CC.Web.Helpers
{
    public static class AutomatedReportsHelper
    {
        private static readonly string emailTo = ConfigurationManager.AppSettings["AutomatedReportsEmailTo"];
        private static readonly string emailCc = ConfigurationManager.AppSettings["AutomatedReportsEmailCc"];
        public static List<ApprovalStatusRow> ApprovalStatusReportHelper(ccEntities db, CC.Data.Services.IPermissionsBase Permissions, int? regionId, int? countryId, int? agencyGroupId, DateTime? dateFrom, DateTime? dateTo)
        {
            var q1 = from a in db.viewFinancialReportApprovalStatusReports
                     where regionId == null || a.RegionId == regionId
                     where countryId == null || a.CountryId == countryId
                     where agencyGroupId == null || a.AgencyGroupId == agencyGroupId
                     where dateFrom == null || a.StatusChangeDate >= dateFrom
                     where dateTo == null || a.StatusChangeDate <= dateTo                     
                     join mr in db.MainReports.Where(Permissions.MainReportsFilter) on a.MainReportId equals mr.Id
                     select new
                     {
                         MainReportId = a.MainReportId,
                         RegionId = a.RegionId,
                         CountryId = a.CountryId,
                         AgencyGroupId = a.AgencyGroupId,
                         AppName = a.AppName,
                         Start = a.Start,
                         End = a.End,
                         AgencyName = a.AgencyName,
                         StatusChangeDate = a.StatusChangeDate,
                         CurrentStatusId = a.NewStatusId,
                         PreviousStatusId = a.OldStatusId
                     };

            var q = q1.OrderBy(f => f.MainReportId).ThenBy(f => f.StatusChangeDate).ToList();

            List<ApprovalStatusRow> approvalStatusList = new List<ApprovalStatusRow>();

            int prevId = 0;
            DateTime prevChangeDate = new DateTime();
            for (int i = 0; i < q.Count; i++)
            {
                ApprovalStatusRow asr = new ApprovalStatusRow();
                if (prevId != q[i].MainReportId)
                {
                    asr.DateOfPreviousStatus = null;
                }
                else
                {
                    asr.DateOfPreviousStatus = prevChangeDate;
                }
                prevId = q[i].MainReportId;
                prevChangeDate = q[i].StatusChangeDate;
                asr.AppName = q[i].AppName;
                asr.MonthFrom = q[i].Start.ToString("MMM yy");
                asr.MonthTo = q[i].End.AddDays(-1).ToString("MMM yy");
                asr.AgencyName = q[i].AgencyName;
                string curApprovalStatus = "";
                switch (q[i].CurrentStatusId)
                {
                    case (int)MainReport.Statuses.Approved:
                        curApprovalStatus = MainReport.Statuses.Approved.ToString();
                        break;
                    case (int)MainReport.Statuses.AwaitingProgramAssistantApproval:
                        curApprovalStatus = MainReport.Statuses.AwaitingProgramAssistantApproval.ToString();
                        break;
                    case (int)MainReport.Statuses.AwaitingProgramOfficerApproval:
                        curApprovalStatus = MainReport.Statuses.AwaitingProgramOfficerApproval.ToString();
                        break;
                    case (int)MainReport.Statuses.Cancelled:
                        curApprovalStatus = MainReport.Statuses.Cancelled.ToString();
                        break;
                    case (int)MainReport.Statuses.New:
                        curApprovalStatus = MainReport.Statuses.New.ToString();
                        break;
                    case (int)MainReport.Statuses.Rejected:
                        curApprovalStatus = MainReport.Statuses.Rejected.ToString();
                        break;
                    case (int)MainReport.Statuses.ReturnedToAgency:
                        curApprovalStatus = MainReport.Statuses.ReturnedToAgency.ToString();
                        break;
					case (int)MainReport.Statuses.AwaitingAgencyResponse:
						curApprovalStatus = MainReport.Statuses.AwaitingAgencyResponse.ToString();
						break;
                }
                asr.CurrentApprovalStatus = curApprovalStatus;
                asr.DateOfCurrentStatus = q[i].StatusChangeDate;
                string prevApprovalStatus = "";
                switch (q[i].PreviousStatusId)
                {
                    case (int)MainReport.Statuses.Approved:
                        prevApprovalStatus = MainReport.Statuses.Approved.ToString();
                        break;
                    case (int)MainReport.Statuses.AwaitingProgramAssistantApproval:
                        prevApprovalStatus = MainReport.Statuses.AwaitingProgramAssistantApproval.ToString();
                        break;
                    case (int)MainReport.Statuses.AwaitingProgramOfficerApproval:
                        prevApprovalStatus = MainReport.Statuses.AwaitingProgramOfficerApproval.ToString();
                        break;
                    case (int)MainReport.Statuses.Cancelled:
                        prevApprovalStatus = MainReport.Statuses.Cancelled.ToString();
                        break;
                    case (int)MainReport.Statuses.New:
                        prevApprovalStatus = MainReport.Statuses.New.ToString();
                        break;
                    case (int)MainReport.Statuses.Rejected:
                        prevApprovalStatus = MainReport.Statuses.Rejected.ToString();
                        break;
                    case (int)MainReport.Statuses.ReturnedToAgency:
                        prevApprovalStatus = MainReport.Statuses.ReturnedToAgency.ToString();
                        break;
					case (int)MainReport.Statuses.AwaitingAgencyResponse:
						prevApprovalStatus = MainReport.Statuses.AwaitingAgencyResponse.ToString();
						break;
                }
                asr.PreviousApprovalStatus = prevApprovalStatus;
                asr.DaysElapsed = Math.Round((asr.DateOfCurrentStatus - (asr.DateOfPreviousStatus ?? asr.DateOfCurrentStatus)).TotalDays, 0).ToString();
                asr.HoursElapsed = Math.Round((asr.DateOfCurrentStatus - (asr.DateOfPreviousStatus ?? asr.DateOfCurrentStatus)).TotalHours, 0).ToString();
                approvalStatusList.Add(asr);
            }

            return approvalStatusList.OrderByDescending(f => f.AppName).ThenByDescending(f => f.DateOfCurrentStatus).ToList();
        }

        public static List<FunctionalityChangeRow> FunctionalityChangeReportHelper(ccEntities db, CC.Data.Services.IPermissionsBase Permissions, int? regionId, int? countryId, int? agencyGroupId)
        {
            var date30DaysAgo = DateTime.Now.AddDays(-30);
            var date180DaysAgo = DateTime.Now.AddDays(-180);

            var q = from a in db.viewFunctionalityScoreChangeReports
                    where regionId == null || a.RegionId == regionId
                    where countryId == null || a.CountryId == countryId
                    where agencyGroupId == null || a.AgencyGroupId == agencyGroupId
                    where a.UpdatedAt >= date30DaysAgo ? a.UpdatedAt >= date180DaysAgo : false
                    join c in db.Clients.Where(Permissions.ClientsFilter) on a.ClientId equals c.Id
                    select new
                    {
                        ClientId = a.ClientId,
                        RegionId = a.RegionId,
                        CountryId = a.CountryId,
                        AgencyGroupId = a.AgencyGroupId,
                        AgencyName = a.AgencyName,
                        DiagnosticScore = a.DiagnosticScore,
                        StartDate = a.StartDate
                    };

            var q1 = q.OrderBy(f => f.ClientId).ThenByDescending(f => f.StartDate).GroupBy(f => f.ClientId).ToList();

            List<FunctionalityChangeRow> functionalityChangeList = new List<FunctionalityChangeRow>();

            foreach (var item in q1)
            {
                FunctionalityChangeRow fcr = new FunctionalityChangeRow();
                for (int i = 0; i < item.Count(); i++)
                {
                    switch (i)
                    {
                        case 0: fcr.ActiveScore = item.ElementAt(i).DiagnosticScore;
                            fcr.DateOfActiveScore = item.ElementAt(i).StartDate;
                            break;
                        case 1: fcr.PriorScore = item.ElementAt(i).DiagnosticScore;
                            fcr.DateOfPriorScore = item.ElementAt(i).StartDate;
                            break;
                        case 2: fcr.PriorPriorScore = item.ElementAt(i).DiagnosticScore;
                            fcr.DateOfPriorPriorScore = item.ElementAt(i).StartDate;
                            break;
                        default:
                            break;
                    }
                }
                var temp = item.First();
                fcr.ClientId = temp.ClientId;
                fcr.AgencyName = temp.AgencyName;
                functionalityChangeList.Add(fcr);
            }

            return functionalityChangeList.OrderBy(f => f.DateOfActiveScore).ToList();
        }

        public static List<DeceasedDateEntryRow> DeceasedDateEntryReportHelper(ccEntities db, CC.Data.Services.IPermissionsBase Permissions, int? regionId, int? countryId, int? agencyGroupId)
        {
            var date30DaysAgo = DateTime.Now.AddDays(-30);

            var q = from a in db.viewDeceasedDateEntryReports
                    where a.UpdateDate >= date30DaysAgo
                    join c in db.Clients.Where(Permissions.ClientsFilter) on a.ClientId equals c.Id
                    select new
                    {
                        ClientId = a.ClientId,
                        RegionId = a.RegionId,
                        CountryId = a.CountryId,
                        AgencyGroupId = a.AgencyGroupId,
                        AgencyName = a.AgencyName,
                        DeceasedDate = a.DeceasedDate,
                        UpdateDate = a.UpdateDate
                    };

            var q1 = q.OrderBy(f => f.ClientId).ThenByDescending(f => f.UpdateDate).GroupBy(f => f.ClientId).ToList();

            List<DeceasedDateEntryRow> ddeList = new List<DeceasedDateEntryRow>();

            foreach (var item in q1)
            {
                DeceasedDateEntryRow dder = new DeceasedDateEntryRow();
                var temp = item.First();
                dder.ClientId = temp.ClientId;
                dder.AgencyName = temp.AgencyName;
                dder.DeceasedDate = temp.DeceasedDate.Value;
                dder.DeceasedDateEntered = temp.UpdateDate;
                dder.Diff = Math.Round((dder.DeceasedDateEntered - dder.DeceasedDate).TotalDays, 0).ToString();
                ddeList.Add(dder);
            }

            return ddeList.OrderBy(f => f.ClientId).ToList();
        }

        public static void AutoEmailAllReports(CC.Data.Services.IPermissionsBase Permissions)
        {
            var msg = new System.Net.Mail.MailMessage();
            msg.Body = "";
            msg.To.Add(new System.Net.Mail.MailAddress(emailTo));
            msg.CC.Add(new System.Net.Mail.MailAddress(emailCc));
            using(var db = new ccEntities())
            {
                var scheduled = db.AutomatedReports.ToList();

                foreach(var s in scheduled)
                {
                    string tempPath = "";
                    if(s.ReportName == AutomatedReportsEnum.FinancialReportApprovalStatusReport.DisplayName())
                    {
                        var result = ApprovalStatusReportHelper(db, Permissions, null, null, null, new DateTime(DateTime.Now.Year, DateTime.Now.AddMonths(-1).Month, 15), new DateTime(DateTime.Now.Year, DateTime.Now.Month, 14));
                        tempPath = Path.Combine(Path.GetTempPath(), "FinancialReportApprovalStatusReport_" + DateTime.Now.ToString("MMM") + DateTime.Now.ToString("yyyy") + ".csv");
                        using (var csvWriter = new CsvHelper.CsvWriter(new StreamWriter(tempPath)))
                        {
                            csvWriter.WriteField("App #");
                            csvWriter.WriteField("Month From");
                            csvWriter.WriteField("Month To");
                            csvWriter.WriteField("Agency Name");
                            csvWriter.WriteField("Current Approval Status");
                            csvWriter.WriteField("Date of Current Approval Status");
                            csvWriter.WriteField("Previous Approval Status");
                            csvWriter.WriteField("Date of Previous Approval Status");
                            csvWriter.WriteField("Days Elapsed (full days)");
                            csvWriter.WriteField("Hours Elapsed (full hours)");
                            csvWriter.NextRecord();
                            foreach (var item in result)
                            {
                                csvWriter.WriteField(item.AppName);
                                csvWriter.WriteField(item.MonthFrom);
                                csvWriter.WriteField(item.MonthTo);
                                csvWriter.WriteField(item.AgencyName);
                                csvWriter.WriteField(item.CurrentApprovalStatus);
                                csvWriter.WriteField(item.DateOfCurrentStatus);
                                csvWriter.WriteField(item.PreviousApprovalStatus);
                                csvWriter.WriteField(item.DateOfPreviousStatus);
                                csvWriter.WriteField(item.DaysElapsed);
                                csvWriter.WriteField(item.HoursElapsed);
                                csvWriter.NextRecord();
                            }
                        }
                                                
                        msg.Subject = string.Format("{0} {1}", AutomatedReportsEnum.FinancialReportApprovalStatusReport.DisplayName(), DateTime.Now.ToString("MMM"));
                    }
                    else if(s.ReportName == AutomatedReportsEnum.FunctionalityScoreChangeReport.DisplayName())
                    {
                        var result = FunctionalityChangeReportHelper(db, Permissions, null, null, null);
                        tempPath = Path.Combine(Path.GetTempPath(), "FunctionalityChangeReport_" + DateTime.Now.ToString("MMM") + DateTime.Now.ToString("yyyy") + ".csv");
                        using (var csvWriter = new CsvHelper.CsvWriter(new StreamWriter(tempPath)))
                        {
                            csvWriter.WriteField("CC ID");
                            csvWriter.WriteField("Agency");
                            csvWriter.WriteField("Active Score");
                            csvWriter.WriteField("Date of Active Score");
                            csvWriter.WriteField("Prior Score");
                            csvWriter.WriteField("Date of Prior Score");
                            csvWriter.WriteField("Previous Approval Status");
                            csvWriter.WriteField("Date of Prior Prior Score");
                            csvWriter.NextRecord();
                            foreach (var item in result)
                            {
                                csvWriter.WriteField(item.ClientId);
                                csvWriter.WriteField(item.AgencyName);
                                csvWriter.WriteField(item.ActiveScore);
                                csvWriter.WriteField(item.DateOfActiveScore);
                                csvWriter.WriteField(item.PriorScore);
                                csvWriter.WriteField(item.DateOfPriorScore);
                                csvWriter.WriteField(item.PriorPriorScore);
                                csvWriter.WriteField(item.DateOfPriorPriorScore);
                                csvWriter.NextRecord();
                            }
                        }

                        msg.Subject = string.Format("{0} {1}", AutomatedReportsEnum.FunctionalityScoreChangeReport.DisplayName(), DateTime.Now.ToString("MMM"));
                    }
                    else if(s.ReportName == AutomatedReportsEnum.DeceasedDateEntryReport.DisplayName())
                    {
                        var result = DeceasedDateEntryReportHelper(db, Permissions, null, null, null);
                        tempPath = Path.Combine(Path.GetTempPath(), "DeceasedDateEntryReport_" + DateTime.Now.ToString("MMM") + DateTime.Now.ToString("yyyy") + ".csv");
                        using (var csvWriter = new CsvHelper.CsvWriter(new StreamWriter(tempPath)))
                        {
                            csvWriter.WriteField("CC ID");
                            csvWriter.WriteField("Agency");
                            csvWriter.WriteField("Deceased Date");
                            csvWriter.WriteField("Deceased Date Entered");
                            csvWriter.WriteField("Days from Actual to Entry (rounded up)");
                            csvWriter.NextRecord();
                            foreach (var item in result)
                            {
                                csvWriter.WriteField(item.ClientId);
                                csvWriter.WriteField(item.AgencyName);
                                csvWriter.WriteField(item.DeceasedDate);
                                csvWriter.WriteField(item.DeceasedDateEntered);
                                csvWriter.WriteField(item.Diff);
                                csvWriter.NextRecord();
                            }
                        }

                        msg.Subject = string.Format("{0} {1}", AutomatedReportsEnum.DeceasedDateEntryReport.DisplayName(), DateTime.Now.ToString("MMM"));
                    }

                    msg.Attachments.Add(new System.Net.Mail.Attachment(tempPath, "text/csv"));

                    using (var smtpClient = new System.Net.Mail.SmtpClient())
                    {
                        try
                        {
                            smtpClient.Send(msg);
                            db.Globals.AddObject(new Global() { Date = DateTime.Now, Message = "Automated Reports Email - Success" });
                            db.SaveChanges();
                        }
                        catch
                        {
                            msg.Attachments.Clear();
                            msg.Body = "The report could not be sent, please contact Diamond administrator";
                            try
                            {
                                smtpClient.Send(msg);
                                db.Globals.AddObject(new Global() { Date = DateTime.Now, Message = "Automated Reports Email - Failure" });
                                db.SaveChanges();
                            }
                            catch
                            {

                            }
                        }
                    }
                }
            }
        }
    }
}