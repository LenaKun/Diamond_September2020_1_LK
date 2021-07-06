using System;
using MvcContrib;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CC.Data;
using CC.Web.Models;
using CC.Web.Attributes;
using System.Net.Mail;
using CC.Web.Helpers;
using MvcContrib.ActionResults;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using iTextSharp.text;
using System.Globalization;
using iTextSharp.text.pdf;
using System.IO;
using System.Threading;

namespace CC.Web.Controllers
{
    [CcAuthorize(FixedRoles.Admin
        , FixedRoles.AgencyUser
        , FixedRoles.AgencyUserAndReviewer
        , FixedRoles.AuditorReadOnly
        , FixedRoles.BMF
        , FixedRoles.GlobalOfficer
        , FixedRoles.GlobalReadOnly
        , FixedRoles.RegionAssistant
        , FixedRoles.RegionOfficer
        , FixedRoles.RegionReadOnly
        , FixedRoles.Ser
        , FixedRoles.SerAndReviewer)]
    public class AppBudgetsController : PrivateCcControllerBase
    {



        private readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(ClientsController));

        static HelpersFluxx.CommandLineApp commandLineApp =
                      new HelpersFluxx.CommandLineApp("IsFluxxAPI_AppBudget", "ExeFullName", "Arguments_AppBudget");

       
        /// <summary>
        /// /Index - List of Budgets
        /// </summary>
        /// <param name="agencyGroupId"></param>
        /// <param name="statusId"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public ViewResult Index(int? regionId, int? agencyGroupId, int? statusId, int? year, bool? GGOnly, string filter)
        {
            //set default filter
            if (Request.HttpMethod.ToString() == "GET")
            {
                switch (this.CcUser.RoleId)
                {
                    case (int)FixedRoles.RegionOfficer:
                        statusId = (int)AppBudgetApprovalStatuses.AwaitingRegionalPoApproval;
                        break;

                    case (int)FixedRoles.GlobalOfficer:
                        statusId = (int)AppBudgetApprovalStatuses.AwaitingGlobalPoApproval;
                        break;
                    case (int)FixedRoles.BMF:
                        statusId = (int)AppBudgetApprovalStatuses.Approved;
                        break;
                }
            }

            //years selection
            var minStart = db.Apps.Where(this.Permissions.AppsFilter).Where(f => f.AppBudgets.Any()).Min(f => (DateTime?)f.StartDate);
            var maxStart = db.Apps.Where(this.Permissions.AppsFilter).Where(f => f.AppBudgets.Any()).Max(f => (DateTime?)f.StartDate);

            var years = new List<int?>();
            years.Add(null);
            if (minStart.HasValue && maxStart.HasValue)
            {
                foreach (var i in Enumerable.Range((minStart).Value.Year, (maxStart).Value.Year - (minStart).Value.Year + 1))
                {
                    years.Add(i);
                }
            }
            ViewBag.year = new SelectList(years, year);

            ViewBag.isBmf = User.IsInRole("BMF");
            ViewBag.ggOnly = GGOnly ?? false;

            if (User.IsInRole("RegionReadOnly"))
            {
                regionId = db.Users.Where(f => f.UserName == User.Identity.Name).Select(f => f.RegionId).SingleOrDefault();
            }

            //get data
            var appbudgets = GetFilteredBudgets(regionId, agencyGroupId, statusId, year, GGOnly == true);

            if (statusId.HasValue && (statusId.Value == (int)AppBudgetApprovalStatuses.AwaitingGlobalPoApproval || statusId.Value == (int)AppBudgetApprovalStatuses.AwaitingRegionalPoApproval))
            {
                switch (statusId.Value)
                {
                    case (int)AppBudgetApprovalStatuses.AwaitingRegionalPoApproval:
                        appbudgets = appbudgets.OrderBy(f => f.UpdatedBySerAt);
                        break;
                    case (int)AppBudgetApprovalStatuses.AwaitingGlobalPoApproval:
                        appbudgets = appbudgets.OrderBy(f => f.UpdatedByRpoAt);
                        break;
                }
            }

            //get helper data
            //var allowedRegions = db.Regions.Where(this.Permissions.RegionsFilter)
            //	.Select(f => new { Id = f.Id, Name = f.Name })
            //	.OrderBy(f => f.Name);
            //var allowedAgencyGroups = db.AgencyGroups
            //	.Where(this.Permissions.AgencyGroupsFilter)
            //	.Select(f => new { Id = f.Id, Name = f.DisplayName })
            //	.OrderBy(f => f.Name);
            var statuses = new Dictionary<int, string>();
            foreach (AppBudgetApprovalStatuses status in Enum.GetValues(typeof(AppBudgetApprovalStatuses)))
            {
                if (User.IsInRole("BMF") && status == AppBudgetApprovalStatuses.Approved)
                {
                    statuses.Add((int)status, status.ToString().SplitCapitalizedWords());
                }
                else if (!User.IsInRole("BMF"))
                {
                    statuses.Add((int)status, status.ToString().SplitCapitalizedWords());
                }
            }

            //ViewBag.regionId = new SelectList(allowedRegions, "Id", "Name", regionId);
            //ViewBag.agencyGroupId = new SelectList(allowedAgencyGroups, "Id", "Name", agencyGroupId);
            ViewBag.statusId = new SelectList(statuses, "Key", "Value", statusId);
            ViewBag.GGOnly = GGOnly;


            //return view
            return View(appbudgets.ToList());
        }

        public bool CanBeEdited(int? statusId)
        {
            if (statusId == null) return false;
            if (CcUser.RoleId != (int)FixedRoles.Ser && CcUser.RoleId != (int)FixedRoles.SerAndReviewer)
                return true;
            else
                if (statusId.HasValue && AppBudget.EditableStatusIds.Contains(statusId.Value))
                return true;
            else
                return false;


        }


        //
        // GET: /AppBudgets/Details/5
        /// <summary>
        /// Get /Details - Details of appbudget + approval/rejection flow
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Details(int id)
        {
            var model = GetDetailsModel(id);

            return View("Details", model);
        }

        public ActionResult ExportAppBudgetServices(int id)
        {

            var model = new AppBudgetDetailsModel();
            if (!User.IsInRole("BMF"))
            {
                model.AppBudgetServices = FetchServiceDetails(id);
                return this.Excel("budgets", "budgets", model.AppBudgetServices);
            }
            else
            {
                model.AppBudgetServicesForBmf = FetchServiceDetailsForBmf(id);
                return this.Excel("budgets", "budgets", model.AppBudgetServicesForBmf);
            }
        }



        [CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer, FixedRoles.RegionOfficer, FixedRoles.RegionAssistant, FixedRoles.GlobalReadOnly, FixedRoles.RegionReadOnly, FixedRoles.AuditorReadOnly)]
        public class ReportInPdf
        {
            public byte[] ReportContent;
            public AppBudgetDetailsModel AppBudgetDetailsModel;

        }

        [CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer, FixedRoles.RegionOfficer, FixedRoles.RegionAssistant, FixedRoles.GlobalReadOnly, FixedRoles.RegionReadOnly, FixedRoles.AuditorReadOnly)]
        public string CreateAndSaveAppBudgetReportDocument(int id)
        {
            string diamand_report_name = null;
            try
            {
                var content = GetPDF(id);
                diamand_report_name = GetReportName(content, true);
                HelpersFluxx.MainReportDocument.SaveBytesToFile_NetworkConnection(diamand_report_name, content.ReportContent);
            }
            catch (Exception ex)
            {
                throw new Exception((ex.InnerException != null ? ex.InnerException.Message : ex.Message) +
                                 " AppBudgetId: " + id, ex);
            }
            return diamand_report_name;
        }

        [CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer, FixedRoles.RegionOfficer, FixedRoles.RegionAssistant, FixedRoles.GlobalReadOnly, FixedRoles.RegionReadOnly, FixedRoles.AuditorReadOnly)]
        public void ExportToPdf(int id, bool ifCtera = false)
        {
            try
            {
                var content = GetPDF(id);
                var date = content.AppBudgetDetailsModel.Data.UpdatedAt?.ToString("yyyy_MM_dd") ?? "No date";
                string fileName = date + " REVB.pdf";

                if (ifCtera)
                {
                    try
                    {
                        var diamand_report_name = GetReportName(content, true);
                        HelpersFluxx.MainReportDocument.SaveBytesToFile_NetworkConnection(diamand_report_name, content.ReportContent);
                        var ctera_name = GetReportName(content, false);
                        fileName = "CTERA" + ctera_name;
                    }
                    catch (Exception ex_ctera)
                    {
                        log.Error("Update Pdf. ReportId: " + id, ex_ctera);
                    }
                }
                HttpContext context = System.Web.HttpContext.Current;
                context.Response.BinaryWrite(content.ReportContent);
                context.Response.ContentType = "application/pdf";
                context.Response.AppendHeader("Content-Disposition", "attachment; filename=" + fileName);
                context.Response.End();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException != null ? ex.InnerException.Message : ex.Message, ex);
            }
            finally
            {
                RedirectToAction("Details", new { id = id });
            }
        }



        [CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer, FixedRoles.RegionOfficer, FixedRoles.RegionAssistant, FixedRoles.GlobalReadOnly, FixedRoles.RegionReadOnly, FixedRoles.AuditorReadOnly)]
        ReportInPdf GetPDF(int id)
        {
            ReportInPdf reportInPdf = null;

            Document doc = new Document(PageSize.A4.Rotate(), 2, 2, 2, 2);
            string filePath = System.IO.Path.GetTempFileName();
            NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;
            int widthPercent = 95;

            try
            {
                PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));

                var budgetDetails = GetDetailsModel(id);

                int budgetHeaderColumns = 10;
                PdfPTable pdfTabBudgetHeader = new PdfPTable(budgetHeaderColumns);
                pdfTabBudgetHeader.HorizontalAlignment = 1;
                pdfTabBudgetHeader.SpacingBefore = 5f;
                pdfTabBudgetHeader.SpacingAfter = 5f;
                pdfTabBudgetHeader.WidthPercentage = widthPercent;
                PdfPCell headerCellBudgetDetails = new PdfPCell(new Phrase("Budget Details"));
                headerCellBudgetDetails.Colspan = budgetHeaderColumns;
                headerCellBudgetDetails.Border = 0;
                headerCellBudgetDetails.HorizontalAlignment = 1;
                pdfTabBudgetHeader.AddCell(headerCellBudgetDetails);
                pdfTabBudgetHeader.AddCell("SER");
                pdfTabBudgetHeader.AddCell("Fund");
                pdfTabBudgetHeader.AddCell("App #");
                pdfTabBudgetHeader.AddCell("App Currency");
                pdfTabBudgetHeader.AddCell("App's Period");
                pdfTabBudgetHeader.AddCell("Status");
                pdfTabBudgetHeader.AddCell("PO Remarks");
                pdfTabBudgetHeader.AddCell("Valid Until");
                pdfTabBudgetHeader.AddCell("Valid Remarks");
                pdfTabBudgetHeader.AddCell("Was a Form A submitted?");
                pdfTabBudgetHeader.AddCell(budgetDetails.Data.App.AgencyGroup.Name);
                pdfTabBudgetHeader.AddCell(budgetDetails.Data.App.Fund.Name);
                pdfTabBudgetHeader.AddCell(budgetDetails.Data.App.CurrencyId);
                pdfTabBudgetHeader.AddCell(budgetDetails.Data.App.Name);
                pdfTabBudgetHeader.AddCell(budgetDetails.Data.App.StartDate.ToString("dd MMM yyyy") + " - " + (budgetDetails.Data.App.EndDate.AddDays(-1)).ToString("dd MMM yyyy"));
                string status = budgetDetails.Data.StatusDisplay;
                if (budgetDetails.Data.StatusId == (int)AppBudgetApprovalStatuses.Approved)
                {
                    status += " on " + ((DateTime)budgetDetails.Data.UpdatedAt).ToString("dd MMM yyyy");
                }
                pdfTabBudgetHeader.AddCell(status);
                string poRemarks = budgetDetails.Data.PoRemarks;
                if (poRemarks != null)
                {
                    poRemarks = Regex.Replace(poRemarks, @"<[^>]*>", String.Empty);
                    poRemarks = Regex.Replace(poRemarks, @"&nbsp;", " ");
                }
                pdfTabBudgetHeader.AddCell(poRemarks);
                string validUntilStr = budgetDetails.Data.ValidUntill != null ? ((DateTime)budgetDetails.Data.ValidUntill).ToString("dd MMM yyyy") : "------";
                pdfTabBudgetHeader.AddCell(validUntilStr);
                string validRemarks = budgetDetails.Data.ValidRemarks;
                if (validRemarks != null)
                {
                    validRemarks = Regex.Replace(validRemarks, @"<[^>]*>", String.Empty);
                    validRemarks = Regex.Replace(validRemarks, @"&nbsp;", " ");
                }
                pdfTabBudgetHeader.AddCell(validRemarks);
                pdfTabBudgetHeader.AddCell(budgetDetails.Data.FormASubmitted.ToYesNo());

                int budgetDetailsColumns = budgetDetails.Data.Revised ? 9 : 8;
                float[] widths = new float[] { 2f, 2f, 2f, 1f, 1f, 1f, 1f, 1f };
                if (budgetDetails.Data.Revised)
                {
                    widths = new float[] { 2f, 2f, 2f, 1f, 1f, 1f, 1f, 1f, 1f };
                }
                PdfPTable pdfTabBudgetDetails = new PdfPTable(budgetDetailsColumns);
                pdfTabBudgetDetails.SetWidths(widths);
                pdfTabBudgetDetails.HorizontalAlignment = 1;
                pdfTabBudgetDetails.SpacingBefore = 5f;
                pdfTabBudgetDetails.SpacingAfter = 5f;
                pdfTabBudgetDetails.WidthPercentage = widthPercent;
                PdfPCell cellBudgetDetails = new PdfPCell(new Phrase("Details"));
                cellBudgetDetails.Colspan = budgetDetailsColumns;
                cellBudgetDetails.HorizontalAlignment = 1;
                pdfTabBudgetDetails.AddCell(cellBudgetDetails);
                pdfTabBudgetDetails.AddCell("Agency");
                pdfTabBudgetDetails.AddCell("Service Type");
                pdfTabBudgetDetails.AddCell("Service");
                pdfTabBudgetDetails.AddCell("CC Grant");
                pdfTabBudgetDetails.AddCell("Required Match");
                pdfTabBudgetDetails.AddCell("Agency Contribution");
                pdfTabBudgetDetails.AddCell("CUR");
                pdfTabBudgetDetails.AddCell("Remarks");
                if (budgetDetails.Data.Revised)
                {
                    pdfTabBudgetDetails.AddCell("Original CC Grant");
                }

                foreach (var item in budgetDetails.AppBudgetServices)
                {
                    pdfTabBudgetDetails.AddCell(item.AgencyName);
                    pdfTabBudgetDetails.AddCell(item.ServiceType);
                    pdfTabBudgetDetails.AddCell(item.ServiceName);
                    pdfTabBudgetDetails.AddCell(item.CCGrant.Format());
                    pdfTabBudgetDetails.AddCell(item.RequiredMatch.Format());
                    pdfTabBudgetDetails.AddCell(item.AgencyContribution.Format());
                    pdfTabBudgetDetails.AddCell(item.CurrencyCode);
                    string remarks = item.Remarks;
                    if (remarks != null)
                    {
                        remarks = Regex.Replace(remarks, @"<[^>]*>", String.Empty);
                        remarks = Regex.Replace(remarks, @"&nbsp;", " ");
                    }
                    pdfTabBudgetDetails.AddCell(remarks);
                    if (budgetDetails.Data.Revised)
                    {
                        var origccgrant = item.OriginalCcGrant.HasValue ? item.OriginalCcGrant.Value.Format() : ((decimal)0).Format();
                        pdfTabBudgetDetails.AddCell(origccgrant);
                    }
                    if (item.ServicePersonnel)
                    {
                        var personnelDetails = getPersonnelQuery(item.Id).ToList();
                        if (personnelDetails.Count > 0)
                        {
                            int personnelColumns = 5;
                            PdfPTable pdfTabPersonnel = new PdfPTable(personnelColumns);
                            pdfTabPersonnel.HorizontalAlignment = 1;
                            pdfTabPersonnel.SpacingBefore = 5f;
                            pdfTabPersonnel.SpacingAfter = 5f;
                            pdfTabPersonnel.WidthPercentage = widthPercent;
                            PdfPCell headerCellPersonnel = new PdfPCell(new Phrase("Personnel"));
                            headerCellPersonnel.Colspan = personnelColumns;
                            headerCellPersonnel.Border = 0;
                            headerCellPersonnel.HorizontalAlignment = 1;
                            pdfTabPersonnel.AddCell(headerCellPersonnel);
                            pdfTabPersonnel.AddCell("Position");
                            pdfTabPersonnel.AddCell("Currency");
                            pdfTabPersonnel.AddCell("CC Grant");
                            pdfTabPersonnel.AddCell("% of time spent on CC program");
                            pdfTabPersonnel.AddCell("Agency Portion of Salary");
                            foreach (var p in personnelDetails)
                            {
                                pdfTabPersonnel.AddCell(p.Position);
                                pdfTabPersonnel.AddCell(p.Cur);
                                var pCcGrant = p.Salary.HasValue ? p.Salary.Value.Format() : "0.00";
                                pdfTabPersonnel.AddCell(pCcGrant);
                                var partTimePerc = p.PartTimePercentage.HasValue ? p.PartTimePercentage.Value.Format() : "0.00";
                                pdfTabPersonnel.AddCell(partTimePerc);
                                var servicePerc = p.ServicePercentage.HasValue ? p.ServicePercentage.Value.Format() : "0.00";
                                pdfTabPersonnel.AddCell(servicePerc);
                            }
                            PdfPCell innerCell = new PdfPCell(pdfTabPersonnel);
                            innerCell.Colspan = budgetDetailsColumns;
                            pdfTabBudgetDetails.AddCell(innerCell);
                            pdfTabBudgetDetails.AddCell(cellBudgetDetails);
                            pdfTabBudgetDetails.AddCell("Agency");
                            pdfTabBudgetDetails.AddCell("Service Type");
                            pdfTabBudgetDetails.AddCell("Service");
                            pdfTabBudgetDetails.AddCell("CC Grant");
                            pdfTabBudgetDetails.AddCell("Required Match");
                            pdfTabBudgetDetails.AddCell("Agency Contribution");
                            pdfTabBudgetDetails.AddCell("CUR");
                            pdfTabBudgetDetails.AddCell("Remarks");
                            if (budgetDetails.Data.Revised)
                            {
                                pdfTabBudgetDetails.AddCell("Original CC Grant");
                            }
                        }
                    }
                }

                var totals = getAppBudgetTotalsModel(id);
                int totalsColumns = 9;
                PdfPTable pdfTabTotals = new PdfPTable(totalsColumns);
                pdfTabTotals.HorizontalAlignment = 1;
                pdfTabTotals.SpacingBefore = 5f;
                pdfTabTotals.SpacingAfter = 5f;
                pdfTabTotals.WidthPercentage = widthPercent;
                PdfPCell headerCellTotals = new PdfPCell(new Phrase("Totals"));
                headerCellTotals.Colspan = totalsColumns;
                headerCellTotals.Border = 0;
                headerCellTotals.HorizontalAlignment = 1;
                pdfTabTotals.AddCell(headerCellTotals);
                pdfTabTotals.AddCell("Total grant as Specified by Ser");
                pdfTabTotals.AddCell("Total Required Match as Specified by Ser");
                pdfTabTotals.AddCell("Total Agency Contribution as Specified by Ser");
                pdfTabTotals.AddCell("Total CC Grant");
                pdfTabTotals.AddCell("Total Required Match");
                pdfTabTotals.AddCell("Total Spent");
                pdfTabTotals.AddCell("Administrative overhead percentage");
                pdfTabTotals.AddCell("Home care percentage");
                pdfTabTotals.AddCell("Other percentage");
                pdfTabTotals.AddCell(totals.CcGrant.Format());
                pdfTabTotals.AddCell(totals.RequiredMatch.Format());
                pdfTabTotals.AddCell(totals.AgencyContribution.Format());
                pdfTabTotals.AddCell(totals.AppAmount.Format());
                pdfTabTotals.AddCell(totals.AppMatch.Format());
                var spend = totals.Spend.HasValue ? totals.Spend.Value.Format() : ((decimal)0).Format();
                pdfTabTotals.AddCell(spend);
                var adminPerc = totals.AdminPercentage.HasValue ? totals.AdminPercentage.Value.FormatPercentage() : ((decimal)0).FormatPercentage();
                pdfTabTotals.AddCell(adminPerc);
                var hcPerc = totals.HcPercentage.HasValue ? totals.HcPercentage.Value.FormatPercentage() : ((decimal)0).FormatPercentage();
                pdfTabTotals.AddCell(hcPerc);
                var otherPerc = totals.OtherPercentage.HasValue ? totals.OtherPercentage.Value.FormatPercentage() : ((decimal)0).FormatPercentage();
                pdfTabTotals.AddCell(otherPerc);

                if (totals.AgencyTotals.Count > 0)
                {
                    int agencyTotalsColumns = 4;
                    PdfPTable pdfTabAgencyTotals = new PdfPTable(agencyTotalsColumns);
                    pdfTabAgencyTotals.HorizontalAlignment = 1;
                    pdfTabAgencyTotals.SpacingBefore = 5f;
                    pdfTabAgencyTotals.SpacingAfter = 5f;
                    pdfTabAgencyTotals.WidthPercentage = widthPercent;
                    PdfPCell headerCellAgencyTotals = new PdfPCell(new Phrase("Agency Totals"));
                    headerCellAgencyTotals.Colspan = agencyTotalsColumns;
                    headerCellAgencyTotals.Border = 0;
                    headerCellAgencyTotals.HorizontalAlignment = 1;
                    pdfTabAgencyTotals.AddCell(headerCellAgencyTotals);
                    pdfTabAgencyTotals.AddCell("Agency");
                    pdfTabAgencyTotals.AddCell("CC Grant");
                    pdfTabAgencyTotals.AddCell("Required Match");
                    pdfTabAgencyTotals.AddCell("Agency Contribution");
                    foreach (var at in totals.AgencyTotals)
                    {
                        pdfTabAgencyTotals.AddCell(at.AgencyName);
                        var atCcGrant = at.CcGrant.HasValue ? at.CcGrant.Value.Format() : "0.00";
                        pdfTabAgencyTotals.AddCell(atCcGrant);
                        var atRequiredMatch = at.RequiredMatch.HasValue ? at.RequiredMatch.Value.Format() : "0.00";
                        pdfTabAgencyTotals.AddCell(atRequiredMatch);
                        var atAgencyContribution = at.AgencyContribution.HasValue ? at.AgencyContribution.Value.Format() : "0.00";
                        pdfTabAgencyTotals.AddCell(atAgencyContribution);
                    }
                    PdfPCell innerAgencyTotals = new PdfPCell(pdfTabAgencyTotals);
                    innerAgencyTotals.Padding = 0f;
                    innerAgencyTotals.Colspan = totalsColumns;
                    pdfTabTotals.AddCell(innerAgencyTotals);
                }

                PdfPCell innerTotals = new PdfPCell(pdfTabTotals);
                innerTotals.Padding = 0f;
                innerTotals.Colspan = budgetDetailsColumns;
                pdfTabBudgetDetails.AddCell(innerTotals);

                doc.Open();
                doc.Add(pdfTabBudgetHeader);
                doc.Add(pdfTabBudgetDetails);
                doc.Close();

                byte[] content = System.IO.File.ReadAllBytes(filePath);

                reportInPdf = new ReportInPdf()
                {
                    AppBudgetDetailsModel = budgetDetails,
                    ReportContent = content
                };

                return reportInPdf;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException != null ? ex.InnerException.Message : ex.Message, ex);
            }
            finally
            {
                RedirectToAction("Details", new { id = id });
            }
        }

        string GetReportName(ReportInPdf content, bool isFullName = false)
        {
            var data = content.AppBudgetDetailsModel.Data;
            var id = data.Id;
            var app_id = data.AppId;
            var agencyGroupId = data.App.AgencyGroupId;


            var date = data.UpdatedAt.Value.ToString("yyyy_MM_dd");

            var uri = System.Configuration.ConfigurationManager.AppSettings["MainReportUri"];
            if (string.IsNullOrEmpty(uri))
            {
                throw new Exception("Missing path for main reports folder \"MainReportUri\"at config");
            }

            var template = System.Configuration.ConfigurationManager.AppSettings["AppBudgetReportTemplate"];
            if (string.IsNullOrEmpty(template))
            {
                template = @"{0}\OrgGroupID_{1}\AppID_{2}\{3}_REVB.pdf";
                throw new Exception("Missing  template for main reports folder \"AppBudgetReportTemplate\" at config");
            }

            var diamand_report_name = isFullName ?
                string.Format(template, uri, agencyGroupId, app_id, date) :
                string.Format(template, "", agencyGroupId, app_id, date);

            return diamand_report_name;
        }


        
        void AddPdfToCteraAndFluxx(AppBudget appbudget)
        {
            if (appbudget.ApprovalStatus == AppBudgetApprovalStatuses.Approved)
            {
                try
                {
                    //Save pdf report at ctera
                    CreateAndSaveAppBudgetReportDocument(appbudget.Id);
                   
                    //save pdf at FLUXX
                    var is_export_to_Fluxx = appbudget.App.Fund?.MasterFund?.FluxxExport ?? false;
                    if (commandLineApp.is_fluxx_api && is_export_to_Fluxx)
                    {
                        try
                        {
                            Thread.Sleep(600);//delay 1 minute
                            Thread t = new Thread(this.RunCommandLineApp);
                            t.Start(appbudget.Id);
                        }
                        catch (Exception fluxx_error)
                        {
                            log.Error("Budget LaunchCommandLineApp:", fluxx_error);
                        }
                    }
                }
                catch (Exception ex)
                {

                    log.Error("Approve, Saving budget report in pdf at ctera:", ex);
                }
            }
        }

        //Call FluxxAPI to add pdf to fluxx
        public void RunCommandLineApp(object id_)
        {
            try
            {

                var id = (int)id_;               
                commandLineApp.Launch(id);

            }
            catch (Exception fluxx_error)
            {
                log.Error("LaunchCommandLineApp:", fluxx_error);
            }
        }

        [CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer, FixedRoles.RegionOfficer, FixedRoles.RegionAssistant, FixedRoles.GlobalReadOnly, FixedRoles.RegionReadOnly, FixedRoles.AuditorReadOnly)]
        public void ExportToPdf_OLD(int id)
        {
            Document doc = new Document(PageSize.A4.Rotate(), 2, 2, 2, 2);
            string fileName = "BudgetPearlExport.pdf";
            string filePath = System.IO.Path.GetTempFileName();
            NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;
            int widthPercent = 95;

            try
            {
                PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));

                var budgetDetails = GetDetailsModel(id);

                int budgetHeaderColumns = 10;
                PdfPTable pdfTabBudgetHeader = new PdfPTable(budgetHeaderColumns);
                pdfTabBudgetHeader.HorizontalAlignment = 1;
                pdfTabBudgetHeader.SpacingBefore = 5f;
                pdfTabBudgetHeader.SpacingAfter = 5f;
                pdfTabBudgetHeader.WidthPercentage = widthPercent;
                PdfPCell headerCellBudgetDetails = new PdfPCell(new Phrase("Budget Details"));
                headerCellBudgetDetails.Colspan = budgetHeaderColumns;
                headerCellBudgetDetails.Border = 0;
                headerCellBudgetDetails.HorizontalAlignment = 1;
                pdfTabBudgetHeader.AddCell(headerCellBudgetDetails);
                pdfTabBudgetHeader.AddCell("SER");
                pdfTabBudgetHeader.AddCell("Fund");
                pdfTabBudgetHeader.AddCell("App #");
                pdfTabBudgetHeader.AddCell("App Currency");
                pdfTabBudgetHeader.AddCell("App's Period");
                pdfTabBudgetHeader.AddCell("Status");
                pdfTabBudgetHeader.AddCell("PO Remarks");
                pdfTabBudgetHeader.AddCell("Valid Until");
                pdfTabBudgetHeader.AddCell("Valid Remarks");
                pdfTabBudgetHeader.AddCell("Was a Form A submitted?");
                pdfTabBudgetHeader.AddCell(budgetDetails.Data.App.AgencyGroup.Name);
                pdfTabBudgetHeader.AddCell(budgetDetails.Data.App.Fund.Name);
                pdfTabBudgetHeader.AddCell(budgetDetails.Data.App.CurrencyId);
                pdfTabBudgetHeader.AddCell(budgetDetails.Data.App.Name);
                pdfTabBudgetHeader.AddCell(budgetDetails.Data.App.StartDate.ToString("dd MMM yyyy") + " - " + (budgetDetails.Data.App.EndDate.AddDays(-1)).ToString("dd MMM yyyy"));
                string status = budgetDetails.Data.StatusDisplay;
                if (budgetDetails.Data.StatusId == (int)AppBudgetApprovalStatuses.Approved)
                {
                    status += " on " + ((DateTime)budgetDetails.Data.UpdatedAt).ToString("dd MMM yyyy");
                }
                pdfTabBudgetHeader.AddCell(status);
                string poRemarks = budgetDetails.Data.PoRemarks;
                if (poRemarks != null)
                {
                    poRemarks = Regex.Replace(poRemarks, @"<[^>]*>", String.Empty);
                    poRemarks = Regex.Replace(poRemarks, @"&nbsp;", " ");
                }
                pdfTabBudgetHeader.AddCell(poRemarks);
                string validUntilStr = budgetDetails.Data.ValidUntill != null ? ((DateTime)budgetDetails.Data.ValidUntill).ToString("dd MMM yyyy") : "------";
                pdfTabBudgetHeader.AddCell(validUntilStr);
                string validRemarks = budgetDetails.Data.ValidRemarks;
                if (validRemarks != null)
                {
                    validRemarks = Regex.Replace(validRemarks, @"<[^>]*>", String.Empty);
                    validRemarks = Regex.Replace(validRemarks, @"&nbsp;", " ");
                }
                pdfTabBudgetHeader.AddCell(validRemarks);
                pdfTabBudgetHeader.AddCell(budgetDetails.Data.FormASubmitted.ToYesNo());

                int budgetDetailsColumns = budgetDetails.Data.Revised ? 9 : 8;
                float[] widths = new float[] { 2f, 2f, 2f, 1f, 1f, 1f, 1f, 1f };
                if (budgetDetails.Data.Revised)
                {
                    widths = new float[] { 2f, 2f, 2f, 1f, 1f, 1f, 1f, 1f, 1f };
                }
                PdfPTable pdfTabBudgetDetails = new PdfPTable(budgetDetailsColumns);
                pdfTabBudgetDetails.SetWidths(widths);
                pdfTabBudgetDetails.HorizontalAlignment = 1;
                pdfTabBudgetDetails.SpacingBefore = 5f;
                pdfTabBudgetDetails.SpacingAfter = 5f;
                pdfTabBudgetDetails.WidthPercentage = widthPercent;
                PdfPCell cellBudgetDetails = new PdfPCell(new Phrase("Details"));
                cellBudgetDetails.Colspan = budgetDetailsColumns;
                cellBudgetDetails.HorizontalAlignment = 1;
                pdfTabBudgetDetails.AddCell(cellBudgetDetails);
                pdfTabBudgetDetails.AddCell("Agency");
                pdfTabBudgetDetails.AddCell("Service Type");
                pdfTabBudgetDetails.AddCell("Service");
                pdfTabBudgetDetails.AddCell("CC Grant");
                pdfTabBudgetDetails.AddCell("Required Match");
                pdfTabBudgetDetails.AddCell("Agency Contribution");
                pdfTabBudgetDetails.AddCell("CUR");
                pdfTabBudgetDetails.AddCell("Remarks");
                if (budgetDetails.Data.Revised)
                {
                    pdfTabBudgetDetails.AddCell("Original CC Grant");
                }

                foreach (var item in budgetDetails.AppBudgetServices)
                {
                    pdfTabBudgetDetails.AddCell(item.AgencyName);
                    pdfTabBudgetDetails.AddCell(item.ServiceType);
                    pdfTabBudgetDetails.AddCell(item.ServiceName);
                    pdfTabBudgetDetails.AddCell(item.CCGrant.Format());
                    pdfTabBudgetDetails.AddCell(item.RequiredMatch.Format());
                    pdfTabBudgetDetails.AddCell(item.AgencyContribution.Format());
                    pdfTabBudgetDetails.AddCell(item.CurrencyCode);
                    string remarks = item.Remarks;
                    if (remarks != null)
                    {
                        remarks = Regex.Replace(remarks, @"<[^>]*>", String.Empty);
                        remarks = Regex.Replace(remarks, @"&nbsp;", " ");
                    }
                    pdfTabBudgetDetails.AddCell(remarks);
                    if (budgetDetails.Data.Revised)
                    {
                        var origccgrant = item.OriginalCcGrant.HasValue ? item.OriginalCcGrant.Value.Format() : ((decimal)0).Format();
                        pdfTabBudgetDetails.AddCell(origccgrant);
                    }
                    if (item.ServicePersonnel)
                    {
                        var personnelDetails = getPersonnelQuery(item.Id).ToList();
                        if (personnelDetails.Count > 0)
                        {
                            int personnelColumns = 5;
                            PdfPTable pdfTabPersonnel = new PdfPTable(personnelColumns);
                            pdfTabPersonnel.HorizontalAlignment = 1;
                            pdfTabPersonnel.SpacingBefore = 5f;
                            pdfTabPersonnel.SpacingAfter = 5f;
                            pdfTabPersonnel.WidthPercentage = widthPercent;
                            PdfPCell headerCellPersonnel = new PdfPCell(new Phrase("Personnel"));
                            headerCellPersonnel.Colspan = personnelColumns;
                            headerCellPersonnel.Border = 0;
                            headerCellPersonnel.HorizontalAlignment = 1;
                            pdfTabPersonnel.AddCell(headerCellPersonnel);
                            pdfTabPersonnel.AddCell("Position");
                            pdfTabPersonnel.AddCell("Currency");
                            pdfTabPersonnel.AddCell("CC Grant");
                            pdfTabPersonnel.AddCell("% of time spent on CC program");
                            pdfTabPersonnel.AddCell("Agency Portion of Salary");
                            foreach (var p in personnelDetails)
                            {
                                pdfTabPersonnel.AddCell(p.Position);
                                pdfTabPersonnel.AddCell(p.Cur);
                                var pCcGrant = p.Salary.HasValue ? p.Salary.Value.Format() : "0.00";
                                pdfTabPersonnel.AddCell(pCcGrant);
                                var partTimePerc = p.PartTimePercentage.HasValue ? p.PartTimePercentage.Value.Format() : "0.00";
                                pdfTabPersonnel.AddCell(partTimePerc);
                                var servicePerc = p.ServicePercentage.HasValue ? p.ServicePercentage.Value.Format() : "0.00";
                                pdfTabPersonnel.AddCell(servicePerc);
                            }
                            PdfPCell innerCell = new PdfPCell(pdfTabPersonnel);
                            innerCell.Colspan = budgetDetailsColumns;
                            pdfTabBudgetDetails.AddCell(innerCell);
                            pdfTabBudgetDetails.AddCell(cellBudgetDetails);
                            pdfTabBudgetDetails.AddCell("Agency");
                            pdfTabBudgetDetails.AddCell("Service Type");
                            pdfTabBudgetDetails.AddCell("Service");
                            pdfTabBudgetDetails.AddCell("CC Grant");
                            pdfTabBudgetDetails.AddCell("Required Match");
                            pdfTabBudgetDetails.AddCell("Agency Contribution");
                            pdfTabBudgetDetails.AddCell("CUR");
                            pdfTabBudgetDetails.AddCell("Remarks");
                            if (budgetDetails.Data.Revised)
                            {
                                pdfTabBudgetDetails.AddCell("Original CC Grant");
                            }
                        }
                    }
                }

                var totals = getAppBudgetTotalsModel(id);
                int totalsColumns = 9;
                PdfPTable pdfTabTotals = new PdfPTable(totalsColumns);
                pdfTabTotals.HorizontalAlignment = 1;
                pdfTabTotals.SpacingBefore = 5f;
                pdfTabTotals.SpacingAfter = 5f;
                pdfTabTotals.WidthPercentage = widthPercent;
                PdfPCell headerCellTotals = new PdfPCell(new Phrase("Totals"));
                headerCellTotals.Colspan = totalsColumns;
                headerCellTotals.Border = 0;
                headerCellTotals.HorizontalAlignment = 1;
                pdfTabTotals.AddCell(headerCellTotals);
                pdfTabTotals.AddCell("Total grant as Specified by Ser");
                pdfTabTotals.AddCell("Total Required Match as Specified by Ser");
                pdfTabTotals.AddCell("Total Agency Contribution as Specified by Ser");
                pdfTabTotals.AddCell("Total CC Grant");
                pdfTabTotals.AddCell("Total Required Match");
                pdfTabTotals.AddCell("Total Spent");
                pdfTabTotals.AddCell("Administrative overhead percentage");
                pdfTabTotals.AddCell("Home care percentage");
                pdfTabTotals.AddCell("Other percentage");
                pdfTabTotals.AddCell(totals.CcGrant.Format());
                pdfTabTotals.AddCell(totals.RequiredMatch.Format());
                pdfTabTotals.AddCell(totals.AgencyContribution.Format());
                pdfTabTotals.AddCell(totals.AppAmount.Format());
                pdfTabTotals.AddCell(totals.AppMatch.Format());
                var spend = totals.Spend.HasValue ? totals.Spend.Value.Format() : ((decimal)0).Format();
                pdfTabTotals.AddCell(spend);
                var adminPerc = totals.AdminPercentage.HasValue ? totals.AdminPercentage.Value.FormatPercentage() : ((decimal)0).FormatPercentage();
                pdfTabTotals.AddCell(adminPerc);
                var hcPerc = totals.HcPercentage.HasValue ? totals.HcPercentage.Value.FormatPercentage() : ((decimal)0).FormatPercentage();
                pdfTabTotals.AddCell(hcPerc);
                var otherPerc = totals.OtherPercentage.HasValue ? totals.OtherPercentage.Value.FormatPercentage() : ((decimal)0).FormatPercentage();
                pdfTabTotals.AddCell(otherPerc);

                if (totals.AgencyTotals.Count > 0)
                {
                    int agencyTotalsColumns = 4;
                    PdfPTable pdfTabAgencyTotals = new PdfPTable(agencyTotalsColumns);
                    pdfTabAgencyTotals.HorizontalAlignment = 1;
                    pdfTabAgencyTotals.SpacingBefore = 5f;
                    pdfTabAgencyTotals.SpacingAfter = 5f;
                    pdfTabAgencyTotals.WidthPercentage = widthPercent;
                    PdfPCell headerCellAgencyTotals = new PdfPCell(new Phrase("Agency Totals"));
                    headerCellAgencyTotals.Colspan = agencyTotalsColumns;
                    headerCellAgencyTotals.Border = 0;
                    headerCellAgencyTotals.HorizontalAlignment = 1;
                    pdfTabAgencyTotals.AddCell(headerCellAgencyTotals);
                    pdfTabAgencyTotals.AddCell("Agency");
                    pdfTabAgencyTotals.AddCell("CC Grant");
                    pdfTabAgencyTotals.AddCell("Required Match");
                    pdfTabAgencyTotals.AddCell("Agency Contribution");
                    foreach (var at in totals.AgencyTotals)
                    {
                        pdfTabAgencyTotals.AddCell(at.AgencyName);
                        var atCcGrant = at.CcGrant.HasValue ? at.CcGrant.Value.Format() : "0.00";
                        pdfTabAgencyTotals.AddCell(atCcGrant);
                        var atRequiredMatch = at.RequiredMatch.HasValue ? at.RequiredMatch.Value.Format() : "0.00";
                        pdfTabAgencyTotals.AddCell(atRequiredMatch);
                        var atAgencyContribution = at.AgencyContribution.HasValue ? at.AgencyContribution.Value.Format() : "0.00";
                        pdfTabAgencyTotals.AddCell(atAgencyContribution);
                    }
                    PdfPCell innerAgencyTotals = new PdfPCell(pdfTabAgencyTotals);
                    innerAgencyTotals.Padding = 0f;
                    innerAgencyTotals.Colspan = totalsColumns;
                    pdfTabTotals.AddCell(innerAgencyTotals);
                }

                PdfPCell innerTotals = new PdfPCell(pdfTabTotals);
                innerTotals.Padding = 0f;
                innerTotals.Colspan = budgetDetailsColumns;
                pdfTabBudgetDetails.AddCell(innerTotals);

                doc.Open();
                doc.Add(pdfTabBudgetHeader);
                doc.Add(pdfTabBudgetDetails);
                doc.Close();
                byte[] content = System.IO.File.ReadAllBytes(filePath);
                HttpContext context = System.Web.HttpContext.Current;

                context.Response.BinaryWrite(content);
                context.Response.ContentType = "application/pdf";
                context.Response.AppendHeader("Content-Disposition", "attachment; filename=" + fileName);
                context.Response.End();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException != null ? ex.InnerException.Message : ex.Message, ex);
            }
            finally
            {
                RedirectToAction("Details", new { id = id });
            }
        }

        public ActionResult Export(int? regionId, int? agencyGroupId, int? statusId, int? year, bool? GGOnly)
        {

            var result = GetFilteredBudgets(regionId, agencyGroupId, statusId, year, GGOnly == true).ToList();
            foreach (var r in result)
            {
                if (r.ValidRemarks != null)
                {
                    r.ValidRemarks = Regex.Replace(r.ValidRemarks, @"<[^>]*>", String.Empty);
                    r.ValidRemarks = Regex.Replace(r.ValidRemarks, @"&nbsp;", " ");
                }
                if (r.PoRemarks != null)
                {
                    r.PoRemarks = Regex.Replace(r.PoRemarks, @"<[^>]*>", String.Empty);
                    r.PoRemarks = Regex.Replace(r.PoRemarks, @"&nbsp;", " ");
                }
            }
            var data = (from r in result
                        select new AppBudgetsListRow
                        {
                            Id = r.Id,
                            SerName = r.App.AgencyGroup.Name,
                            FundName = r.App.Fund.Name,
                            MasterFundName = r.App.Fund.MasterFund.Name,
                            AppName = r.App.Name,
                            Start = r.App.StartDate,
                            End = r.App.EndDate.AddDays(-1),
                            Status = r.StatusId == (int)AppBudgetApprovalStatuses.Approved ? "Approved" :
                                (r.StatusId == (int)AppBudgetApprovalStatuses.AwaitingGlobalPoApproval ? "Awaiting Global Program Officer Approval" :
                                (r.StatusId == (int)AppBudgetApprovalStatuses.AwaitingRegionalPoApproval ? "Awaiting Regional Program Officer Approval" :
                                (r.StatusId == (int)AppBudgetApprovalStatuses.Cancelled ? "Cancelled" :
                                (r.StatusId == (int)AppBudgetApprovalStatuses.New ? "New" :
                                (r.StatusId == (int)AppBudgetApprovalStatuses.Rejected ? "Rejected" :
                                "Returned to Agency"))))),
                            Revised = r.Revised ? "Yes" : string.Empty,
                            ValidUntil = r.ValidUntill,
                            ValidRemarks = r.ValidRemarks,
                            PoRemarks = r.PoRemarks,
                            CUR = r.App.CurrencyId,
                            Amount = r.App.CcGrant.Format()
                        }).ToList();
            return this.Excel("App Budgets List", "App Budgets List", data);
        }

        public class AppBudgetsListRow
        {
            [Display(Name = "Id")]
            public int Id { get; set; }
            [Display(Name = "Ser")]
            public string SerName { get; set; }
            [Display(Name = "Fund")]
            public string FundName { get; set; }
            [Display(Name = "Master Fund")]
            public string MasterFundName { get; set; }
            [Display(Name = "App")]
            public string AppName { get; set; }
            [Display(Name = "CUR")]
            public string CUR { get; set; }
            [Display(Name = "Amount")]
            public string Amount { get; set; }
            [Display(Name = "Start Date")]
            public DateTime Start { get; set; }
            [Display(Name = "End Date")]
            public DateTime End { get; set; }
            [Display(Name = "Approval Status")]
            public string Status { get; set; }
            [Display(Name = "Revised")]
            public string Revised { get; set; }
            [Display(Name = "Conditional Approval Valid Until")]
            public DateTime? ValidUntil { get; set; }
            [Display(Name = "Conditional Approval Agency Remarks")]
            public string ValidRemarks { get; set; }
            [Display(Name = "Regional PO Remarks")]
            public string PoRemarks { get; set; }
        }

        private AppBudgetDetailsModel GetDetailsModel(int id)
        {
            AppBudget appbudget = this.GetById(id);

            if (appbudget == null)
            {
                throw new ArgumentException("Budget could not be found");
            }


            ViewData["Data.agencyIdFilter"] = new SelectList(db.AppBudgetServices.Where(f => f.Id == id).Select(f => f.Agency), "Id", "Name", "");

            var model = new AppBudgetDetailsModel(appbudget, this.CcUser);

            model.AgencyGroupIsAudit = (from a in db.AppBudgets
                                        where a.Id == id
                                        select a.App.AgencyGroup.IsAudit).SingleOrDefault();

            model.AppBudgetServices = FetchServiceDetails(id);

            var Errors = ValidationHelper.Validate(appbudget, new System.ComponentModel.DataAnnotations.ValidationContext(appbudget, null, null));
            foreach (var w in Errors)
            {
                ModelState.AddModelError(string.Empty, w.ErrorMessage);
                model.Errors.Add(w);
            }

            //add warnings
            var Warnings = appbudget.GetWarnings(new System.ComponentModel.DataAnnotations.ValidationContext(appbudget, null, null));
            foreach (var w in Warnings)
            {
                ModelState.AddModelError(string.Empty, w.ErrorMessage);
                model.Warnings.Add(w);
            }


            model.CanBeDeleted = this.Permissions.User.RoleId == (int)FixedRoles.Admin && !(db.SubReports.Any(f => f.AppBudgetService.AppBudgetId == id));


            if (CanBeEdited(appbudget.StatusId)) ViewBag.canBeEdited = "Yes";

            model.Data.App.Fund.MasterFund = db.MasterFunds.Where(f => f.Id == model.Data.App.Fund.MasterFundId).FirstOrDefault();

            return model;
        }

        private IQueryable<AppBudget> GetFilteredBudgets(int? regionId, int? agencyGroupId, int? statusId, int? year, bool GGOnly)
        {
            return db.AppBudgets
                .Where(this.Permissions.AppBudgetsFilter)
                .Include(f => f.App.AgencyGroup.Agencies)
                .Include(f => f.User)
                .Include(f => f.App.Fund)
                .Where(f => regionId == null || f.App.AgencyGroup.Country.RegionId == regionId)
                .Where(f => agencyGroupId == null || f.App.AgencyGroup.Id == agencyGroupId)
                .Where(f => statusId == null || f.StatusId == statusId)
                .Where(f => year == null || System.Data.Objects.SqlClient.SqlFunctions.DatePart("year", f.App.StartDate) == year.Value)
                .Where(f => GGOnly == false || f.App.Fund.MasterFundId == 73)
                .OrderByDescending(f => f.UpdatedAt)
                ;
        }

        private IQueryable<AppBudgetServiceDetailsModel> FetchServiceDetails(int id)
        {
            return (from abs in db.AppBudgetServices.Where(this.Permissions.AppBudgetServicesFilter)
                    where abs.AppBudgetId == id && abs.Service.ReportingMethodId != (int)Service.ReportingMethods.ClientEventsCount
                    select new AppBudgetServiceDetailsModel()
                    {
                        Id = abs.Id,
                        AppName = abs.AppBudget.App.Name,
                        AgencyName = abs.Agency.Name,
                        AgencyContribution = abs.AgencyContribution,
                        CCGrant = abs.CcGrant,
                        CurrencyCode = abs.AppBudget.App.CurrencyId,
                        Remarks = abs.Remarks,
                        RequiredMatch = abs.RequiredMatch,
                        ServiceName = abs.Service.Name,
                        ServicePersonnel = abs.Service.Personnel,
                        ServiceType = abs.Service.ServiceType.Name,
                        YtdSpent = (decimal?)abs.SubReports.Sum(sr => ((decimal?)sr.Amount ?? 0)),
                        OriginalCcGrant = abs.OriginalCcGrant
                    }).OrderBy(f => f.ServiceType).ThenBy(f => f.ServiceName).ThenBy(f => f.AgencyName)
                                      ;
        }

        private IQueryable<AppBudgetServiceDetailsModelForBmf> FetchServiceDetailsForBmf(int id)
        {
            return (from abs in db.AppBudgetServices.Where(this.Permissions.AppBudgetServicesFilter)
                    where abs.AppBudgetId == id && abs.Service.ReportingMethodId != (int)Service.ReportingMethods.ClientEventsCount
                    select new AppBudgetServiceDetailsModelForBmf()
                    {
                        Id = abs.Id,
                        AppName = abs.AppBudget.App.Name,
                        AgencyName = abs.Agency.Name,
                        CCGrant = abs.CcGrant,
                        CurrencyCode = abs.AppBudget.App.CurrencyId,
                        Remarks = abs.Remarks,
                        ServiceName = abs.Service.Name,
                        ServiceType = abs.Service.ServiceType.Name,
                        YtdSpent = (decimal?)abs.SubReports.Sum(sr => (decimal?)sr.Amount ?? 0)
                    }).OrderBy(f => f.ServiceType).ThenBy(f => f.ServiceName).ThenBy(f => f.AgencyName);
        }

        public ActionResult Totals(int id)
        {

            var model = getAppBudgetTotalsModel(id);

            return View(model);
        }

        private AppBudgetTotals getAppBudgetTotalsModel(int id)
        {
            var appbudgetServices = from ab in db.AppBudgetServices.Where(this.Permissions.AppBudgetServicesFilter)
                                    where ab.AppBudgetId == id
                                    select ab;

            //get the total sum of the services
            var model = (from ab in appbudgetServices
                         group ab by ab.AppBudgetId into abg
                         select new AppBudgetTotals
                         {
                             AppBudgetId = abg.Key,
                             CcGrant = (decimal?)abg.Sum(f => f.CcGrant) ?? 0,
                             RequiredMatch = (decimal?)abg.Sum(a => a.RequiredMatch) ?? 0,
                             AgencyContribution = (decimal?)abg.Sum(a => a.AgencyContribution) ?? 0,

                         }).Single();

            // values from app
            var app = (from ab in db.AppBudgets.Where(this.Permissions.AppBudgetsFilter)
                       where ab.Id == id
                       select new
                       {
                           Id = ab.App.Id,
                           AppAmount = (decimal?)ab.App.CcGrant ?? 0,
                           AppMatch = (decimal?)ab.App.RequiredMatch ?? 0,
                           AgencyContributionRequired = ab.App.AgencyContribution,
                       }).Single();

            model.AppAmount = app.AppAmount;
            model.AppMatch = app.AppMatch;
            model.AgencyContributionRequired = app.AgencyContributionRequired;


            //total amount from financial repoprts

            model.Spend = (from a in appbudgetServices
                           join v in db.viewSubreportAmounts on a.Id equals v.AppBudgetServiceId
                           join mr in db.MainReports.Where(this.Permissions.MainReportsFilter).Where(MainReport.Submitted) on v.MainReportId equals mr.Id
                           group v by a.AppBudgetId into vg
                           select vg.Sum(f => f.Amount)
                         ).SingleOrDefault();

            model.isLastReport = (from a in appbudgetServices
                                  join v in db.viewSubreportAmounts on a.Id equals v.AppBudgetServiceId
                                  join mr in db.MainReports.Where(this.Permissions.MainReportsFilter).Where(MainReport.Submitted)
                                  .Where(f => f.LastReport) on v.MainReportId equals mr.Id
                                  select mr).Any();

            model.HcCcGrant = (appbudgetServices.Where(f => f.Service.ServiceType.Name == "HomeCare").Sum(f => (decimal?)f.CcGrant) ?? 0);
            model.AdminCcGrant = (appbudgetServices.Where(f => f.Service.ServiceType.Name == "Administrative Overhead").Sum(f => (decimal?)f.CcGrant) ?? 0);


            model.AgencyTotals = (from abs in appbudgetServices
                                  group abs by abs.Agency into g
                                  select new AppBudgetTotals.AppBudgetSubTotals
                                  {
                                      AgencyId = g.Key.Id,
                                      AgencyName = g.Key.Name,
                                      CcGrant = (decimal?)g.Sum(f => f.CcGrant),
                                      RequiredMatch = (decimal?)g.Sum(f => f.RequiredMatch),
                                      AgencyContribution = (decimal?)g.Sum(f => f.AgencyContribution)
                                  }).ToList();

            if (model.Spend.HasValue)
            {
                model.CancellationAmount = model.AppAmount - model.Spend.Value;
            }

            return model;
        }

        public ActionResult AppBudgetServicesDataTable(int appBudgetId, jQueryDataTableParamModel jq)
        {


            var filtered = FetchServiceDetails(appBudgetId);


            if (!string.IsNullOrEmpty(jq.sSearch))
            {
                foreach (var s in jq.sSearch.Split(new char[] { ' ' }).Where(f => !string.IsNullOrWhiteSpace(f)))
                {
                    filtered = filtered.Where(f => f.AgencyName.Contains(s) || f.ServiceName.Contains(s) || f.ServiceType.Contains(s));
                }
            }

            var ordered = filtered.OrderBy(f => f.ServiceType).ThenBy(f => f.ServiceName).ThenBy(f => f.AgencyName);

            var page = ordered.Skip(jq.iDisplayStart).Take(jq.iDisplayLength);


            return this.MyJsonResult(new jQueryDataTableResult<object>()
            {
                sEcho = jq.sEcho

            }, JsonRequestBehavior.AllowGet);

        }


        private AppBudget GetById(int appBudgetId)
        {
            db.ContextOptions.LazyLoadingEnabled = false;
            db.ContextOptions.ProxyCreationEnabled = false;

            var result = db.AppBudgets.Where(this.Permissions.AppBudgetsFilter)
                .Include(f => f.App.AgencyGroup.Agencies)
                .Include(f => f.App.Fund)
                .Include(f => f.App.AgencyGroup.Country)
                .SingleOrDefault(a => a.Id == appBudgetId);
            if (result != null)
            {
                result.AppBudgetServices.Clear();
                /*  
                 result.AppBudgetServices = db.Agencies
                .SelectMany(f => f.AppBudgetServices.Where(a => a.AppBudgetId == result.Id))
                     .Where(this.Permissions.AgencyFilter)
                 .Include(f => f.Agency).Include(f => f.Service.ServiceType).ToList();
          
                Fix EF bug Error: The property 'AppBudgetServices' on type 
                'AppBudget_9D509C38D4BBFAB7BC1A49FDB793B1C1013135FCEA4C288634EDE06D18950F91' 
                cannot be set because the collection is already set to an EntityCollection.
                */
                var appBudgetServices = db.Agencies
                    .Where(this.Permissions.AgencyFilter)
                    .SelectMany(f => f.AppBudgetServices.Where(a => a.AppBudgetId == result.Id))
                    .Include(f => f.Agency).Include(f => f.Service.ServiceType).ToList();

                foreach (var r in appBudgetServices)
                {
                    result.AppBudgetServices.Add(r);
                }
            }



            return result;
        }



        #region Flow    //postbacks from the Details

        [HttpPost]
        [CcAuthorize(FixedRoles.Ser, FixedRoles.SerAndReviewer, FixedRoles.Admin)]
        [ConfirmPassword()]
        public ActionResult Submit(int id)
        {
            db.ContextOptions.LazyLoadingEnabled = false;
            db.ContextOptions.ProxyCreationEnabled = false;

            var appbudget = db.AppBudgets
                .Include(f => f.App)
                .Where(this.Permissions.AppBudgetsFilter).SingleOrDefault(f => f.Id == id);
            if (appbudget == null)
            {
                throw new Exception("Appbudget not found.");
            }

            if (!AppBudget.EditableStatuses.Contains(appbudget.ApprovalStatus))
            {
                throw new Exception("Budget is in status " + appbudget.ApprovalStatus.ToString() + " and can not be submitted");
            }

            var totalsModel = getAppBudgetTotalsModel(id);
            var q = (from app in db.Apps.Where(this.Permissions.AppsFilter).Where(f => f.Id == appbudget.AppId)
                     join fund in db.Funds on app.FundId equals fund.Id
                     select new
                     {
                         OtherServicesMax = (app.OtherServicesMax ?? fund.OtherServicesMax) / 100,
                         HomecareMin = (app.HomecareMin ?? fund.HomecareMin) / 100,
                         AdminMax = (app.AdminMax ?? fund.AdminMax) / 100,
                         app.MaxAdminAmount,
                         app.MaxNonHcAmount
                     }).Single();
            if (totalsModel.AdminPercentage > q.AdminMax || totalsModel.HcPercentage < q.HomecareMin || totalsModel.OtherPercentage > q.OtherServicesMax)
            {
                ModelState.AddModelError("",
                    string.Format("Maximum/minimum percentage not met. Admin Max % is {0}, Homecare Min % is {1}, Other Services Max % is {2} while the allowed Admin Max % is {3}, Homecare Min % is {4} and Other Services Max % is {5}",
                    totalsModel.AdminPercentage * 100, totalsModel.HcPercentage * 100, totalsModel.OtherPercentage * 100,
                    q.AdminMax * 100, q.HomecareMin * 100, q.OtherServicesMax * 100));
            }
            if (totalsModel.AdminCcGrant > q.MaxAdminAmount)
            {
                var msg = string.Format("Admin amount {0} {2} exceeded Maximum allowed Admin amount {1} {2}",
                    totalsModel.AdminCcGrant,
                    appbudget.App.MaxAdminAmount,
                    appbudget.App.CurrencyId);
                ModelState.AddModelError(string.Empty, msg);
            }
            if (totalsModel.CcGrant - totalsModel.HcCcGrant > q.MaxNonHcAmount)
            {
                var msg = string.Format("Non homecare amount {0} {2} exceeded Maximum allowed Non homecare amount {1} {2}",
                    totalsModel.CcGrant - totalsModel.HcCcGrant,
                    appbudget.App.MaxNonHcAmount,
                    appbudget.App.CurrencyId);
                ModelState.AddModelError(string.Empty, msg);
            }
            bool sendApprovalNotification = false;
            if (ModelState.IsValid)
            {
                if (appbudget.Revised && appbudget.App.InterlineTransfer)
                {
                    var interlineLevelViolation = AppBudget.ValidateInterlineTranfer(id);
                    if (interlineLevelViolation.HasValue)
                    {
                        appbudget.ApprovalStatus = AppBudgetApprovalStatuses.AwaitingRegionalPoApproval;
                    }
                    else
                    {
                        sendApprovalNotification = true;
                        appbudget.ApprovalStatus = AppBudgetApprovalStatuses.Approved;
                    }

                }
                else
                {
                    appbudget.ApprovalStatus = AppBudgetApprovalStatuses.AwaitingRegionalPoApproval;
                }
                appbudget.UpdatedAt = DateTime.Now;
                appbudget.UpdatedById = this.CcUser.Id;
                appbudget.UpdatedBySerAt = DateTime.Now;
                appbudget.UpdatedBySerId = this.CcUser.Id;
                TryValidateModel(appbudget);

                if (ModelState.IsValid)
                {
                    db.SaveChanges();
                    if (sendApprovalNotification)
                    {
                        SendGpoApprovalEmailNotification(appbudget);
                    }
                    return RedirectToAction("Details", new { id = id });
                }

            }
            return this.Details(id);
        }

        [HttpPost]
        [CcAuthorize(FixedRoles.Ser, FixedRoles.SerAndReviewer, FixedRoles.Admin)]
        [ValidateInput(false)]
        [ConfirmPassword()]
        public ActionResult SubmitConditional(int id, DateTime date, string remarks)
        {
            AppBudget appbudget = this.GetById(id);
            if (AppBudget.EditableStatuses.Contains(appbudget.ApprovalStatus))
            {
                throw new Exception("Budget is in status " + appbudget.ApprovalStatus.ToString() + " and can not be submitted");
            }

            if (ModelState.IsValid)
            {
                var model = new AppBudgetDetailsModel(appbudget, this.CcUser);
                model.Data.ValidUntill = date;
                model.Data.ValidRemarks = remarks;
                appbudget.ApprovalStatus = AppBudgetApprovalStatuses.AwaitingRegionalPoApproval;

                appbudget.Validate(new System.ComponentModel.DataAnnotations.ValidationContext(appbudget, null, null));

                TryValidateModel(appbudget);
                if (ModelState.IsValid)
                {
                    appbudget.UpdatedAt = DateTime.Now;
                    appbudget.UpdatedById = this.CcUser.Id;
                    appbudget.UpdatedBySerAt = DateTime.Now;
                    appbudget.UpdatedBySerId = this.CcUser.Id;
                    db.SaveChanges();

                    return RedirectToAction("Details", new { id = id });
                }
                else
                {
                    throw ModelState.ex("cc");

                }
            }
            else
            {
                throw ModelState.ex();
                throw new Exception();
            }


        }


        [HttpPost]
        [CcAuthorize(FixedRoles.RegionOfficer, FixedRoles.Admin)]
        [ConfirmPassword()]
        [ValidateInput(false)]
        public ActionResult ApproveByRpo(AppBudget input,bool FormASubmitted = false)
        {
            AppBudget appbudget = this.GetById(input.Id);
            if (ModelState.IsValid)
            {
                
                appbudget.FormASubmitted = input.FormASubmitted;
                appbudget.PoRemarks = input.PoRemarks;

                appbudget.ApprovalStatus = AppBudgetApprovalStatuses.AwaitingGlobalPoApproval;
                appbudget.UpdatedAt = DateTime.Now;
                appbudget.UpdatedById = this.CcUser.Id;

                appbudget.UpdatedByRpoAt = DateTime.Now;
                appbudget.UpdatedByRpoId = this.CcUser.Id;

                TryValidateModel(appbudget);
                if (ModelState.IsValid)
                {
                    db.SaveChanges();
                    this.SendRpoApprovalEmailNotification(appbudget);
                    return RedirectToAction("Details", new { id = appbudget.Id });
                }
            }
            return this.Details(input.Id);
        }

        [HttpPost]
        [CcAuthorize(FixedRoles.RegionOfficer, FixedRoles.Admin)]
        [ConfirmPassword()]
        [ValidateInput(false)]
        public ActionResult RejectByRpo(int id, string poremarks)
        {
            AppBudget appbudget = this.GetById(id);
            var prevUser = appbudget.User;

            if (string.IsNullOrWhiteSpace(poremarks))
            {
                ModelState.AddModelError("remarks", "Remarks are required");
            }

            if (ModelState.IsValid)
            {
                appbudget.ApprovalStatus = AppBudgetApprovalStatuses.Rejected;
                appbudget.PoRemarks = poremarks;
                appbudget.UpdatedAt = DateTime.Now;
                appbudget.UpdatedById = this.CcUser.Id;
                appbudget.UpdatedByRpoAt = DateTime.Now;
                appbudget.UpdatedByRpoId = this.CcUser.Id;

                var rowsUpdated = db.SaveChanges();

                this.SendRpoRejectionEmailNotification(appbudget);

                return RedirectToAction("Details", new { id = id });
            }
            return View("Details", appbudget);

        }

        public ActionResult AwaitingAgencyResponseByRpo(int id)
        {
            AppBudget appbudget = this.GetById(id);
            var prevUser = appbudget.User;

            // if (string.IsNullOrWhiteSpace(poremarks))
            //  {
            //  ModelState.AddModelError("remarks", "Remarks are required");
            //  }

            if (ModelState.IsValid)
            {
                appbudget.ApprovalStatus = AppBudgetApprovalStatuses.AwaitingAgencyResponseRPO;
                // appbudget.PoRemarks = poremarks;
                appbudget.UpdatedAt = DateTime.Now;
                appbudget.UpdatedById = this.CcUser.Id;
                appbudget.UpdatedByRpoAt = DateTime.Now;
                appbudget.UpdatedByRpoId = this.CcUser.Id;

                var rowsUpdated = db.SaveChanges();

                //this.SendRpoRejectionEmailNotification(appbudget);

                return RedirectToAction("Details", new { id = id });
            }
            return View("Details", appbudget);
        }

        public ActionResult AwaitingAgencyResponseByGpo(int id)
        {
            AppBudget appbudget = this.GetById(id);
            var prevUser = appbudget.User;

            // if (string.IsNullOrWhiteSpace(poremarks))
            //  {
            //  ModelState.AddModelError("remarks", "Remarks are required");
            //  }

            if (ModelState.IsValid)
            {
                appbudget.ApprovalStatus = AppBudgetApprovalStatuses.AwaitingAgencyResponseGPO;
                // appbudget.PoRemarks = poremarks;
                appbudget.UpdatedAt = DateTime.Now;
                appbudget.UpdatedById = this.CcUser.Id;
                appbudget.UpdatedByRpoAt = DateTime.Now;
                appbudget.UpdatedByRpoId = this.CcUser.Id;

                var rowsUpdated = db.SaveChanges();

                //this.SendRpoRejectionEmailNotification(appbudget);

                return RedirectToAction("Details", new { id = id });
            }
            return View("Details", appbudget);
        }

        public ActionResult AwaitingRegionalPoApproval(int id)
        {
            AppBudget appbudget = this.GetById(id);
            var prevUser = appbudget.User;

            // if (string.IsNullOrWhiteSpace(poremarks))
            //  {
            //  ModelState.AddModelError("remarks", "Remarks are required");
            //  }

            if (ModelState.IsValid)
            {
                appbudget.ApprovalStatus = AppBudgetApprovalStatuses.AwaitingRegionalPoApproval;
                // appbudget.PoRemarks = poremarks;
                appbudget.UpdatedAt = DateTime.Now;
                appbudget.UpdatedById = this.CcUser.Id;
                appbudget.UpdatedByRpoAt = DateTime.Now;
                appbudget.UpdatedByRpoId = this.CcUser.Id;

                var rowsUpdated = db.SaveChanges();

                //this.SendRpoRejectionEmailNotification(appbudget);

                return RedirectToAction("Details", new { id = id });
            }
            return View("Details", appbudget);
        }

        public ActionResult AwaitingGlobalPoApproval(int id)
        {
            AppBudget appbudget = this.GetById(id);
            var prevUser = appbudget.User;

            // if (string.IsNullOrWhiteSpace(poremarks))
            //  {
            //  ModelState.AddModelError("remarks", "Remarks are required");
            //  }

            if (ModelState.IsValid)
            {
                appbudget.ApprovalStatus = AppBudgetApprovalStatuses.AwaitingGlobalPoApproval;
                // appbudget.PoRemarks = poremarks;
                appbudget.UpdatedAt = DateTime.Now;
                appbudget.UpdatedById = this.CcUser.Id;
                appbudget.UpdatedByRpoAt = DateTime.Now;
                appbudget.UpdatedByRpoId = this.CcUser.Id;

                var rowsUpdated = db.SaveChanges();

                //this.SendRpoRejectionEmailNotification(appbudget);

                return RedirectToAction("Details", new { id = id });
            }
            return View("Details", appbudget);
        }




        [HttpPost]
        [CcAuthorize(FixedRoles.GlobalOfficer, FixedRoles.Admin)]
        [ConfirmPassword()]
        [ValidateInput(false)]
        public ActionResult ApproveByGpo(AppBudget input)
        {
            AppBudget appbudget = this.GetById(input.Id);
            if (ModelState.IsValid)
            {
               // appbudget.FormASubmitted = input.FormASubmitted;
                appbudget.PoRemarks = input.PoRemarks;

                appbudget.ApprovalStatus = AppBudgetApprovalStatuses.Approved;
                appbudget.UpdatedAt = DateTime.Now;
                appbudget.UpdatedById = this.CcUser.Id;

                appbudget.UpdatedByGpoAt = DateTime.Now;
                appbudget.UpdatedByGpoId = this.CcUser.Id;

                TryValidateModel(appbudget);
                if (ModelState.IsValid)
                {
                    db.SaveChanges();
                    this.SendGpoApprovalEmailNotification(appbudget);
                    AddPdfToCteraAndFluxx(appbudget);
                    return RedirectToAction("Details", new { id = appbudget.Id });
                }
            }
            return this.Details(input.Id);
        }

        [HttpPost]
        [CcAuthorize(FixedRoles.GlobalOfficer, FixedRoles.Admin)]
        [ConfirmPassword()]
        [ValidateInput(false)]
        public ActionResult RejectByGpo(int id, string poremarks)
        {
            AppBudget appbudget = this.GetById(id);
            var prevUser = appbudget.User;

            if (string.IsNullOrWhiteSpace(poremarks))
            {
                ModelState.AddModelError("remarks", "Remarks are required");
            }

            if (ModelState.IsValid)
            {
                appbudget.ApprovalStatus = AppBudgetApprovalStatuses.AwaitingRegionalPoApproval;
                appbudget.PoRemarks = poremarks;

                appbudget.UpdatedAt = DateTime.Now;
                appbudget.UpdatedById = this.CcUser.Id;
                appbudget.UpdatedByGpoAt = DateTime.Now;
                appbudget.UpdatedByGpoId = this.CcUser.Id;

                var rowsUpdated = db.SaveChanges();

                return RedirectToAction("Details", new { id = id });
            }
            return View("Details", appbudget);
        }

        [HttpPost]
        [CcAuthorize(FixedRoles.Ser, FixedRoles.SerAndReviewer, FixedRoles.Admin)]
        [ConfirmPassword()]
        [ValidateInput(false)]
        public ActionResult Revise(int id, string poremarks)
        {
            AppBudget appbudget = this.GetById(id);
            var lastUser = appbudget.User;

            bool wasAlreadyRevised = appbudget.Revised;

            // delete the removed appbudgetservices from sql
            try
            {
                db.SaveChanges();
            }
            catch (Exception)
            {
                throw;
            }



            if (appbudget.ApprovalStatus == AppBudgetApprovalStatuses.Approved)
            {
                var date = DateTime.Now;

                SaveSnapshot(appbudget, date);

                //set appbudget as *Revised
                appbudget.ApprovalStatus = AppBudgetApprovalStatuses.New;
                appbudget.Revised = true;

                addNewRows(appbudget);

                RemoveDeleted(appbudget);

                //set the update history
                appbudget.UpdatedAt = date;
                appbudget.UpdatedById = this.CcUser.Id;
                appbudget.UpdatedBySerAt = date;
                appbudget.UpdatedBySerId = this.CcUser.Id;

                var appBudgetServices = db.AppBudgetServices.Include(f => f.Service).Include(f => f.PersonnelReports).Where(f => f.AppBudgetId == appbudget.Id);
                if (!wasAlreadyRevised)
                {
                    foreach (var abs in appBudgetServices)
                    {
                        if (abs.Service.Personnel)
                        {
                            abs.OriginalCcGrant = abs.PersonnelReports.Sum(f => f.Salary);
                        }
                        else
                        {
                            abs.OriginalCcGrant = abs.CcGrant;
                        }
                    }
                }

                //save the changes
                db.SaveChanges();
            }

            if (ModelState.IsValid)
            {
                return this.RedirectToAction(f => f.Details(id));
            }
            else
            {
                return Details(id);
            }
        }

        [HttpPost]
        [CcAuthorize(FixedRoles.Admin)]
        public ActionResult UpdateServices(int id)
        {
            AppBudget appbudget = this.GetById(id);
            if (!AppBudget.IsEditable(appbudget))
            {
                throw new InvalidOperationException("The budget is int status " + appbudget.StatusName + " and can not be edited.");
            }
            addNewRows(appbudget);

            RemoveDeleted(appbudget);

            db.SaveChanges();
            return this.RedirectToAction(f => f.Details(id));
        }

        private void SaveSnapshot(AppBudget appbudget, DateTime date)
        {
            var existing = db.AppBudgetServices.Where(f => f.AppBudgetId == appbudget.Id);
            foreach (var abs in existing)
            {
                db.AppBudgetServiceAudits.AddObject(new AppBudgetServiceAudit()
                {
                    AgencyContribution = abs.AgencyContribution,
                    AgencyId = abs.AgencyId,
                    AppBudgetId = abs.AppBudgetId,
                    CcGrant = abs.CcGrant,
                    Date = date,
                    Remarks = abs.Remarks,
                    RequiredMatch = abs.RequiredMatch,
                    ServiceId = abs.ServiceId
                });
            }
            db.SaveChanges();
        }

        private void RemoveDeleted(AppBudget appbudget)
        {
            var qq = from abs in db.AppBudgetServices.Where(f => f.AppBudgetId == appbudget.Id)
                     join sr in db.SubReports on abs.Id equals sr.AppBudgetServiceId into srg
                     select new
                     {
                         abs = abs,
                         AgencyName = abs.Agency.Name,
                         ServiceName = abs.Service.Name,
                         ServiceType = abs.Service.ServiceType.Name,
                         ServiceId = abs.ServiceId,
                         Id = abs.Id,
                         srs = srg.Any(),
                         aservice = abs.AppBudget.App.Services.Any(f => f.Id == abs.ServiceId),
                     };



            //loop on removed services
            foreach (var s in qq.Where(f => !f.aservice))
            {
                // show errors if there are subreports
                if (s.srs)
                {
                    if (!s.aservice)
                    {
                        ModelState.AddModelError(string.Empty, string.Format("{3}: {0} - {1} (id: {2}) Service is no longer available for the App.", s.ServiceName, s.ServiceType, s.ServiceId, s.AgencyName));
                    }
                }
                else
                {
                    //this will also remove the abs from appbudget.appbudgetservices
                    db.AppBudgetServices.DeleteObject(s.abs);
                }
            }
        }

        private void addNewRows(AppBudget appbudget)
        {
            //get app and ser allowed services that not in current appbudget (newly added)
            var Apps = db.Apps;
            var AppBudgetServices = db.AppBudgetServices;

            var q = from app in Apps
                    where app.Id == appbudget.AppId
                    from appService in app.Services
                    from a in app.AgencyGroup.Agencies
                    join abs in AppBudgetServices.Where(f => f.AppBudgetId == appbudget.Id)
                        on new { sid = appService.Id, aid = a.Id } equals new { sid = abs.ServiceId, aid = abs.AgencyId } into absg
                    from abs in absg.DefaultIfEmpty()
                    where abs == null
                    select new { sid = appService.Id, aid = a.Id };

            foreach (var n in q)
            {
                appbudget.AppBudgetServices.Add(new AppBudgetService()
                {
                    ServiceId = n.sid,
                    AgencyId = n.aid
                });
            }
        }

        #endregion

        /// <summary>
        /// Get /Create - Create new Budget
        /// </summary>
        /// <returns></returns>
        [CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.SerAndReviewer)]
        public ActionResult Create()
        {
            var model = new CC.Web.Models.AppBudgetCreateModel();

            ViewBag.Permissions = this.Permissions;
            model.Agencies = new SelectList(db.Agencies.Where(this.Permissions.AgencyFilter).OrderBy(f => f.Name), "Id", "Name");
            model.Apps = new SelectList(db.Apps.Where(this.Permissions.AppsFilter).OrderBy(f => f.Name), "Id", "Name");
            model.Funds = new SelectList(db.Funds.OrderBy(f => f.Name), "Id", "Name");

            return View(model);
        }

        /// <summary>
        /// Post /Create - Create new Budget
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.SerAndReviewer)]
        public ActionResult Create(AppBudgetCreateModel model)
        {
            var app = db.Apps.Where(this.Permissions.AppsFilter).Single(f => f.Id == model.AppId);

#warning check unique constraint


            //create a new record
            var newAppBudget = new AppBudget()
            {

                AppId = model.AppId,
                ApprovalStatus = AppBudgetApprovalStatuses.New,
                RequiredMatch = app.RequiredMatch,
                UpdatedById = this.CcUser.Id,
                UpdatedAt = DateTime.Now,

            };

            var q = from ag in db.AgencyGroups.Where(Permissions.AgencyGroupsFilter)
                    from a in ag.Agencies
                    from ap in ag.Apps
                    where ap.Id == model.AppId
                    from s in ap.Services
                    select new { sid = s.Id, aid = a.Id };

            foreach (var a in q)
            {
                newAppBudget.AppBudgetServices.Add(new AppBudgetService()
                {
                    AgencyId = a.aid,
                    ServiceId = a.sid
                });
            }

            if (!newAppBudget.AppBudgetServices.Any())
            {
                ModelState.AddModelError(string.Empty, "This App contains no services, therefore a Budget cannot be created.  Please contact your Program Assistant for help.");

            }

            //validate new record
            TryValidateModel(newAppBudget);


            if (ModelState.IsValid)
            {

                db.AppBudgets.AddObject(newAppBudget);
                try
                {
                    var rowsUpdated = db.SaveChanges();

                    return RedirectToAction("Edit", new { id = newAppBudget.Id });
                }
                catch (UpdateException ex)
                {
                    if (ex.InnerException.Message.IndexOf("Violation of UNIQUE KEY") > -1)
                    {

                        ModelState.AddModelError(string.Empty, "The selected App has already a Budget App Approval process initiated");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }


            //fallback to create()

            model.Agencies = new SelectList(db.Agencies.Where(this.Permissions.AgencyFilter), "Id", "Name");
            model.Apps = new SelectList(db.Apps.Where(this.Permissions.AppsFilter), "Id", "Name");
            model.Funds = new SelectList(db.Funds, "Id", "Name");
            return View(model);
        }

        public ActionResult RevisionHistory(AppBudgetRevisionHistoryModel model)
        {
            if (model == null) model = new AppBudgetRevisionHistoryModel();


            model.AppBudget = db.AppBudgets.Where(this.Permissions.AppBudgetsFilter)
                .Include(f => f.App.AgencyGroup.Agencies)
                .Include(f => f.App.Fund)
                .Include(f => f.App.AgencyGroup.Country)
                .Single(f => f.Id == model.Id);
            var q = db.AppBudgetServiceAudits.Where(this.Permissions.AppBudgetServiceAuditsFilter)
                .Where(f => f.AppBudgetId == model.Id)
                .Select(abs => new AppBudgetRevisionHistoryModel.Entry()
                {
                    Date = abs.Date,
                    Id = abs.Id,
                    AgencyId = abs.Agency.Id,
                    AgencyName = abs.Agency.Name,
                    AgencyContribution = abs.AgencyContribution,
                    CCGrant = abs.CcGrant,
                    CurrencyCode = abs.AppBudget.App.CurrencyId,
                    Remarks = abs.Remarks,
                    RequiredMatch = abs.RequiredMatch,
                    ServiceId = abs.ServiceId,
                    ServiceName = abs.Service.Name,
                    ServiceTypeId = abs.Service.TypeId,
                    ServiceType = abs.Service.ServiceType.Name,
                });

            ViewBag.RevisionDates = q.Select(f => f.Date).Distinct().ToList()
                .Select(f => new SelectListItem()
                {
                    Value = (f - new DateTime(1970, 1, 1)).TotalMilliseconds.ToString(),
                    Text = f.ToString()
                }).OrderBy(f => f.Value);

            ViewBag.Agencies = new SelectList(
                q.Select(abs =>
                                new { id = abs.AgencyId, name = abs.AgencyName }).Distinct().OrderBy(f => f.name),
                                "id", "name", model.Filter.AgencyId);
            ViewBag.Services = new SelectList(
                q.Select(abs =>
                                new { id = abs.ServiceId, name = abs.ServiceName }).Distinct().OrderBy(f => f.name),
                                "id", "name", model.Filter.AgencyId);
            ViewBag.ServiceTypes = new SelectList(
                q.Select(abs =>
                                new { id = abs.ServiceTypeId, name = abs.ServiceType }).Distinct().OrderBy(f => f.name),
                                "id", "name", model.Filter.AgencyId);





            model.Rows = q;

            return View(model);
        }

        public ActionResult StatusHistory(int id)
        {
            var model = new AppBudgetStatusHistoryModel();
            model.StatusHistory = (from h in db.Histories
                                   where h.TableName.Equals("AppBudgets") && h.FieldName.Equals("StatusId") && h.ReferenceId == id
                                   select new
                                   {
                                       Date = h.UpdateDate,
                                       UserName = h.User.UserName,
                                       OldValue = h.OldValue,
                                       NewValue = h.NewValue
                                   }).ToList().Select(f =>
                               new AppBudgetStatusHistoryModel.AppBudgetStatusHistoryEntry()
                               {
                                   Date = f.Date,
                                   UserName = f.UserName,
                                   NewStatusId = f.NewValue.Parse<int>().Value,
                                   OldStatusId = f.OldValue.Parse<int>().Value,

                               });
            model.AppBudget = this.GetById(id);

            model.AppBudget.App.Fund.MasterFund = db.MasterFunds.Where(f => f.Id == model.AppBudget.App.Fund.MasterFundId).FirstOrDefault();

            return View(model);
        }

        /// <summary>
        /// Get /Edit/id - input ccgrants and required match for Budget services
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [CcAuthorize(FixedRoles.Admin, FixedRoles.AgencyUser, FixedRoles.AgencyUserAndReviewer, FixedRoles.GlobalOfficer, FixedRoles.RegionAssistant, FixedRoles.RegionOfficer, FixedRoles.Ser, FixedRoles.SerAndReviewer)]
        public ActionResult Edit(int id)
        {
            AppBudget appbudget = this.GetById(id);
            CheckAppBudget(appbudget);

            TryValidateModel(appbudget);
            //add warnings
            var warnings = appbudget.GetWarnings(new System.ComponentModel.DataAnnotations.ValidationContext(appbudget, null, null));
            foreach (var w in warnings)
            {
                ModelState.AddModelError(string.Empty, w.ErrorMessage);
            }

            if (!AppBudget.EditableStatuses.Contains(appbudget.ApprovalStatus))
            {
                ModelState.AddModelError(string.Empty, "The approval details cant't be modified in any status other than “New” / “Returned to Agency");
            }

            var model = new AppBudgetEditModel() { AppBudget = appbudget };
            model.AppBudgetServices = FetchServiceDetails(id);

            return View(model);
        }

        private static void CheckAppBudget(AppBudget appbudget)
        {
            if (appbudget == null)
            {
                throw new Exception("Budget not found");
            }
            else if (!AppBudget.IsEditable(appbudget))
            {
                throw new InvalidOperationException("The budget is int status " + appbudget.StatusName + " and can not be edited.");
            }
        }


        public ActionResult Personnel(int id, bool edit)
        {
            var model = new Tuple<int, bool>(id, edit);
            return View(model);
        }
        public ActionResult PersonnelData(int id, jQueryDataTableParamsWithData<IEnumerable<object>> jqdt)
        {
            var q = getPersonnelQuery(id);

            switch (jqdt.iSortCol_0)
            {

                case 0:
                default:
                    switch (jqdt.sSortDir_0)
                    {
                        case "desc": q = q.OrderByDescending(f => f.Position); break;
                        default: q = q.OrderBy(f => f.Position); break;
                    }
                    break;
                case 2:
                    switch (jqdt.sSortDir_0)
                    {
                        case "desc": q = q.OrderByDescending(f => f.Salary); break;
                        default: q = q.OrderBy(f => f.Salary); break;
                    }
                    break;
                case 3:

                    break;
                case 4:
                    switch (jqdt.sSortDir_0)
                    {
                        case "desc": q = q.OrderByDescending(f => f.PartTimePercentage); break;
                        default: q = q.OrderBy(f => f.PartTimePercentage); break;
                    }
                    break;
                case 5:
                    switch (jqdt.sSortDir_0)
                    {
                        case "desc": q = q.OrderByDescending(f => f.ServicePercentage); break;
                        default: q = q.OrderBy(f => f.ServicePercentage); break;
                    }
                    break;
            }


            var filtered = q;
            if (!string.IsNullOrEmpty(jqdt.sSearch))
            {
                foreach (var s in jqdt.sSearch.Split(new char[] { ' ' }))
                {
                    filtered = filtered.Where(f => f.Position.Contains(s));
                }
            }

            jqdt.iTotalDisplayRecords = filtered.Count();
            jqdt.iTotalRecords = q.Count();
            jqdt.aaData = q.Skip(jqdt.iDisplayStart).Take(jqdt.iDisplayLength)
                .ToList().Select(f => new object[]{
                    f.Position,
                    f.Cur,
                    f.Salary,
                    f.PartTimePercentage,
                    f.ServicePercentage,
                    f.Id
            });

            return this.MyJsonResult(new jQueryDataTableResult<IEnumerable<object>>()
            {
                sEcho = jqdt.sEcho,
                iTotalDisplayRecords = jqdt.iTotalDisplayRecords,
                iTotalRecords = jqdt.iTotalRecords,
                aaData = jqdt.aaData
            }, JsonRequestBehavior.AllowGet);
        }

        private IQueryable<PersonnelRow> getPersonnelQuery(int id)
        {
            return (from p in db.PersonnelReports.Where(this.Permissions.PersonnelReportsFilter)
                    where p.AppBudgetServiceId == id
                    select new PersonnelRow
                    {
                        Position = p.Position,
                        Cur = p.AppBudgetService.AppBudget.App.CurrencyId,
                        Salary = p.Salary,
                        PartTimePercentage = p.PartTimePercentage,
                        ServicePercentage = p.ServicePercentage,
                        Id = p.Id
                    });
        }

        public class PersonnelRow
        {
            public string Position { get; set; }
            public string Cur { get; set; }
            public decimal? Salary { get; set; }
            public decimal? PartTimePercentage { get; set; }
            public decimal? ServicePercentage { get; set; }
            public int Id { get; set; }
        }

        public ActionResult PersonnelTotals(int id)
        {
            var q = db.AppBudgetServices.Where(this.Permissions.AppBudgetServicesFilter)
                .SingleOrDefault(f => f.Id == id);

            return this.MyJsonResult(q.CcGrant, JsonRequestBehavior.AllowGet);
        }
        [CcAuthorize(FixedRoles.Admin, FixedRoles.AgencyUser, FixedRoles.AgencyUserAndReviewer, FixedRoles.GlobalOfficer, FixedRoles.RegionAssistant, FixedRoles.RegionOfficer, FixedRoles.Ser, FixedRoles.SerAndReviewer)]
        public ActionResult SavePersonnel(PersonnelReport model)
        {

            ModelState.Clear();
            TryValidateModel(model);


            if (ModelState.IsValid)
            {
                var appbudgetservice = GetAppBudgetService(model);

                CheckAppBudget(appbudgetservice.AppBudget);

                if (model.Id == default(int))
                {
                    db.PersonnelReports.AddObject(model);
                }
                else
                {
                    db.PersonnelReports.Attach(model);
                    db.ObjectStateManager.ChangeObjectState(model, EntityState.Modified);
                }
                var rowsUpdated = db.SaveChanges();
            }
            else
            {
                return this.MyJsonResult(ModelState.ValidationErrorMessages());
            }
            return new EmptyResult();

        }

        private AppBudgetService GetAppBudgetService(PersonnelReport model)
        {
            var appbudgetservice = db.AppBudgetServices.Where(Permissions.AppBudgetServicesFilter)
                .Include(f => f.AppBudget)
                .SingleOrDefault(f => f.Id == model.AppBudgetServiceId)
                ;
            return appbudgetservice;
        }
        [CcAuthorize(FixedRoles.Admin, FixedRoles.AgencyUser, FixedRoles.AgencyUserAndReviewer, FixedRoles.GlobalOfficer, FixedRoles.RegionAssistant, FixedRoles.RegionOfficer, FixedRoles.Ser, FixedRoles.SerAndReviewer)]
        public ActionResult DeletePersonnel(PersonnelReport model)
        {
            var abs = GetAppBudgetService(model);

            CheckAppBudget(abs.AppBudget);

            var existing = db.PersonnelReports.Where(this.Permissions.PersonnelReportsFilter)
                .SingleOrDefault(f => f.Id == model.Id);
            if (existing == null)
            {
                //

            }
            else
            {

                db.DeleteObject(existing);
                var rowsUpdated = db.SaveChanges();

            }
            return new EmptyResult();
        }

        public ActionResult ExportPersonnelToExcel(int id)
        {
            var model = new PersonnelsDetailModel();
            model.Personnels = FetchPersonnelDetails(id);
            return this.Excel("personnels", "personnels", model.Personnels);
        }

        private IQueryable<PersonnelsDetailModel.PersonnelEntry> FetchPersonnelDetails(int id)
        {

            return (from p in db.PersonnelReports.Where(this.Permissions.PersonnelReportsFilter)
                    where p.AppBudgetServiceId == id

                    select new PersonnelsDetailModel.PersonnelEntry()
                    {
                        Position = p.Position,
                        Currency = p.AppBudgetService.AppBudget.App.CurrencyId,
                        CCGrant = p.Salary,
                        TimeSpentOnCCPercentage = p.PartTimePercentage,
                        ServiceSalaryPortionPercentage = p.ServicePercentage,

                    });

        }

        public ActionResult CCGrant(int id)
        {
            var result = db.AppBudgetServices.Where(this.Permissions.AppBudgetServicesFilter).Where(f => f.Id == id).Select(f => f.CcGrant).SingleOrDefault();

            return Content(result.Format());
        }

        [CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
        public ActionResult UpdateAppBudgetService(AppBudgetServiceDetailsModel input, int appBudgetId)
        {
            var appbudget = db.AppBudgets.Where(this.Permissions.AppBudgetsFilter).Single(f => f.Id == appBudgetId);
            var totalsModel = getAppBudgetTotalsModel(appBudgetId);
            if (appbudget.StatusId.HasValue && AppBudget.EditableStatusIds.Contains(appbudget.StatusId.Value))
            {
                var abs = db.AppBudgetServices.Include(f => f.Service).Where(this.Permissions.AppBudgetServicesFilter).Single(f => f.Id == input.Id);
                if (!abs.Service.Personnel)
                {
                    abs.CcGrant = input.CCGrant;
                }
                abs.RequiredMatch = input.RequiredMatch;
                abs.AgencyContribution = input.AgencyContribution;
                abs.Remarks = input.Remarks;
                var rowsUpdated = db.SaveChanges();
            }
            else
            {
                var coderesult = new HttpStatusCodeResult(550, "Budget can not be edited");
                return coderesult;
            }
            return new EmptyResult();
        }

        [HttpPost]
        [CcAuthorize(FixedRoles.Admin)]
        public ActionResult UpdateStatus(AppBudgetDetailsModel input)
        {
            var ia = input.Data;
            var appbudget = db.AppBudgets.Where(this.Permissions.AppBudgetsFilter).SingleOrDefault(f => f.Id == ia.Id);

            if (appbudget != null)
            {
                appbudget.StatusId = ia.StatusId;

                appbudget.UpdatedAt = DateTime.Now;
                appbudget.UpdatedById = this.Permissions.User.Id;

                ModelState.Clear();

                //no need to validate the appbudget, the budget can be in invalid state 
                //all we need is update status

                db.SaveChanges();

                if (input.SendEmail)
                {
                    switch (appbudget.ApprovalStatus)
                    {
                        case AppBudgetApprovalStatuses.Approved:
                            SendGpoApprovalEmailNotification(appbudget);
                            break;
                        case AppBudgetApprovalStatuses.Rejected:
                            SendRejectionEmail(appbudget);
                            break;
                    }
                }

                AddPdfToCteraAndFluxx(appbudget);

                return this.RedirectToAction(f => f.Details(appbudget.Id));

            }
            else
            {
                throw new Exception("Budget id: " + ia.Id + " not found");
            }
        }

        public ActionResult UpdateStatusApprovalRPO(AppBudgetDetailsModel input)
        {
            var ia = input.Data;
            var appbudget = db.AppBudgets.Where(this.Permissions.AppBudgetsFilter).SingleOrDefault(f => f.Id == 2);

            if (appbudget != null)
            {
                appbudget.StatusId = 2;

                appbudget.UpdatedAt = DateTime.Now;
                appbudget.UpdatedById = this.Permissions.User.Id;

                ModelState.Clear();

                //no need to validate the appbudget, the budget can be in invalid state 
                //all we need is update status


                db.SaveChanges();
                if (input.SendEmail)
                {
                    switch (appbudget.ApprovalStatus)
                    {
                        case AppBudgetApprovalStatuses.Approved:
                            SendGpoApprovalEmailNotification(appbudget);
                            break;
                        case AppBudgetApprovalStatuses.Rejected:
                            SendRejectionEmail(appbudget);
                            break;
                    }
                }

                AddPdfToCteraAndFluxx(appbudget);

                return this.RedirectToAction(f => f.Details(appbudget.Id));

            }
            else
            {
                throw new Exception("Budget id: " + 2 + " not found");
            }
        }



        /// <summary>
        /// Get /Delete/id - delete conformation
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [CcAuthorize(FixedRoles.Admin, FixedRoles.AgencyUser, FixedRoles.AgencyUserAndReviewer, FixedRoles.GlobalOfficer, FixedRoles.RegionAssistant, FixedRoles.RegionOfficer, FixedRoles.Ser, FixedRoles.SerAndReviewer)]
        public ActionResult Delete(AppBudgetDetailsModel input)
        {
            var ab = db.AppBudgets.Where(Permissions.AppBudgetsFilter)
                .FirstOrDefault(f => f.Id == input.Data.Id);


            try
            {

                db.AppBudgets.DeleteObject(ab);
                var rowsUpdated = db.SaveChanges();
                return this.RedirectToAction(f => f.Index(null, null, null, null, User.IsInRole("BMF"), null));
            }
            catch (System.Data.UpdateException)
            {
                ModelState.AddModelError(string.Empty, "Sorry, this Budget can not be deleted.");
                db.Dispose();
                db = new ccEntities();
                return this.Details(ab.Id);
            }
        }



        #region emails

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        ///	"request is approved.  a mail is sent to SER and CC to regional po"
        /// </remarks>
        /// <param name="appBudget"></param>
        private void SendGpoApprovalEmailNotification(AppBudget appBudget)
        {
            MailMessage msg = new MailMessage();

            //load related entities if missing
            if (appBudget.App == null)
            {
                appBudget.App = db.Apps.Include(f => f.AgencyGroup).Single(f => f.Id == appBudget.AppId);
            }
            if (appBudget.App.AgencyGroup == null)
            {
                appBudget.App.AgencyGroup = db.AgencyGroups.Single(f => f.Id == appBudget.App.AgencyGroupId);
            }

            EmailHelper.AddRecipeintsByAppBudgetId(msg, appBudget.Id);

            msg.IsBodyHtml = true;

            if (appBudget.Revised)
            {
                if (appBudget.App.InterlineTransfer)
                {
                    msg.Subject = string.Format("Claims Conference Diamond - Revised Interline Transfer Budget Approval Notification (App {0}, Ser {1})", appBudget.App.Name, appBudget.App.AgencyGroup.Name);
                    msg.Body = this.RenderRazorViewToString("~/Views/AppBudgets/Emails/ApproveInterlineTrnsferRevised.cshtml", appBudget);
                }
                else
                {
                    msg.Subject = string.Format("Claims Conference Diamond- Revised Budget Approval Notification (App {0}, Ser {1})", appBudget.App.Name, appBudget.App.AgencyGroup.Name);
                    msg.Body = this.RenderRazorViewToString("~/Views/AppBudgets/Emails/ApproveRevised.cshtml", appBudget);
                }
            }
            else
            {
                msg.Subject = string.Format("Claims Conference Diamond- Budget Approval Notification (App {0}, SER {1})", appBudget.App.Name, appBudget.App.AgencyGroup.Name);
                msg.Body = this.RenderRazorViewToString("~/Views/AppBudgets/Emails/Approve.cshtml", appBudget);
            }
            Send(msg);
        }

        private void Send(MailMessage msg)
        {
            using (var smtp = new SmtpClientWrapper())
            {
                try
                {
                    smtp.Send(msg);
                }
                catch (Exception ex)
                {
                    log.Error("failed to send an email", ex);
                }
            }
        }

        private string GetEmailByUserId(int? UserId)
        {
            User _ccUser = null;
            if (UserId == null) return "";

            int id = (int)UserId;
            _ccUser = this.db.Users.Single(u => u.Id == id);


            if (_ccUser != null)
                return _ccUser.Email;
            else
                return "";
        }


        /// <summary>
        /// </summary>
        /// <remarks>
        /// "request is returned to region PO with status awaiting regional po approval.  email calling the regional PO to contact the Global PO for any further details."
        /// </remarks>
        /// <param name="appBudget"></param>
        private void SendGpoRejectionEmailNotification(AppBudget appBudget)
        {
            SendRejectionEmail(appBudget);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// "request is rejected by region po and is now ""Rejected"". a mail is sent to SER with remarks All global PO is cc to that mail"
        /// </remarks>
        /// <param name="appBudget"></param>
        private void SendRpoRejectionEmailNotification(AppBudget appBudget)
        {
            SendRejectionEmail(appBudget);
        }

        private void SendRpoApprovalEmailNotification(AppBudget appBudget)
        {
            MailMessage msg = new MailMessage();
            EmailHelper.AddRecipeintsForAppBudgetRpoApproval(msg, appBudget);
            msg.IsBodyHtml = true;
            msg.Body = this.RenderRazorViewToString("~/Views/AppBudgets/Emails/RpoApprove.cshtml", appBudget);
            msg.Subject = string.Format("Claims Conference Diamond - RPO Approval Notification (App {0}, SER {1})", appBudget.App.Name, appBudget.App.AgencyGroup.Name);
            Send(msg);
        }


        private void SendRejectionEmail(AppBudget appBudget)
        {
            MailMessage msg = new MailMessage();
            EmailHelper.AddRecipeintsByAppBudgetId(msg, appBudget.Id);
            msg.IsBodyHtml = true;
            msg.Body = this.RenderRazorViewToString("~/Views/AppBudgets/Emails/Reject.cshtml", appBudget);
            msg.Subject = string.Format("Claims Conference Diamond- Budget Rejection Notification (App {0}, SER {1})", appBudget.App.Name, appBudget.App.AgencyGroup.Name);
            Send(msg);
        }



        #endregion

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }


    }
}