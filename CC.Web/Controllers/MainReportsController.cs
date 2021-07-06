using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CC.Data;
using CC.Web.Models;
using CC.Web.Attributes;
using MvcContrib.ActionResults;
using MvcContrib;
using System.Net.Mail;
using System.Configuration;
using System.Linq.Dynamic;
using System.Data.Entity;
using iTextSharp.text;
using System.IO;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Data.Objects.SqlClient;
using CC.Web.Helpers;
using System.Data.Objects;
using System.Text;
using System.Threading;

namespace CC.Web.Controllers
{
    [CcAuthorize(FixedRoles.Admin
        , FixedRoles.AgencyUser
        , FixedRoles.AuditorReadOnly
        , FixedRoles.BMF
        , FixedRoles.GlobalOfficer
        , FixedRoles.GlobalReadOnly
        , FixedRoles.RegionAssistant
        , FixedRoles.RegionOfficer
        , FixedRoles.RegionReadOnly
        , FixedRoles.Ser
        , FixedRoles.SerAndReviewer
        , FixedRoles.AgencyUserAndReviewer)]
    public class MainReportsController : PrivateCcControllerBase
    {
        private readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(MainReportsController));

        static HelpersFluxx.CommandLineApp commandLineApp =
                     new HelpersFluxx.CommandLineApp("IsFluxxAPI_MainReport", "ExeFullName", "Arguments_MainReport");

        public void RunCommandLineApp(object id_)
        {
            try
            {
                var id = (int)id_;
                commandLineApp.Launch(id);
            }
            catch (Exception fluxx_error)
            {
                _log.Error("LaunchCommandLineApp:", fluxx_error);
            }
        }

        void AddOrUpdateMainReportAtFluxx(MainReport report)
        {           
            if (commandLineApp.is_fluxx_api)
            {
                var fundId = report.AppBudget.App?.FundId ?? 0;
                if (report.StatusId != (int)MainReport.Statuses.AwaitingAgencyResponse
                   && (db.Funds.Where(f => f.Id == fundId).Include(f => f.MasterFund)?.FirstOrDefault()?.MasterFund?.FluxxExport ?? false))
                {
                    try
                    {
                      //  Thread.Sleep(60000);//delay 1 minute
                        Thread t = new Thread(this.RunCommandLineApp);
                        t.Start(report.Id);
                    }
                    catch (Exception fluxx_error)
                    {
                        _log.Error("MainReport LaunchCommandLineApp:", fluxx_error);
                    }
                }
            }
        }



        public ActionResult Index(MainReportsListModel model)
        {
            if (User.IsInRole("RegionReadOnly"))
            {
                model.Filter.RegionId = db.Users.Where(f => f.UserName == User.Identity.Name).Select(f => f.RegionId).SingleOrDefault();
            }
            model.LoadData(db, this.Permissions, this.CcUser);
            if (model.Filter.RegionId.HasValue)
            {
                ViewBag.RegionId = model.Filter.RegionId.Value;
            }
            return View(model);
        }

        public ActionResult IndexDataTablesData(int? regionId, int? countryId, int? stateId, int? agencyGroupId, DateTime? start, DateTime? end, int? statusId, bool? GGOnly, jQueryDataTableParamModel jqdt)
        {



          var MainReports = db.MainReports.AsQueryable();
          
            ////filter
            IQueryable<MainReport> source = MainReports.Where(this.Permissions.MainReportsFilter);

            var mainReportDetailsQuery = GetReportDetailsQuery(regionId, countryId, stateId, agencyGroupId, start, end, statusId, GGOnly == true, source);
            if (!string.IsNullOrEmpty(jqdt.sSearch))
            {
                foreach (var s in jqdt.sSearch.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    mainReportDetailsQuery = mainReportDetailsQuery.Where(f =>
                        f.AgencyGroupName.Contains(s) || f.FundName.Contains(s) ||
                        f.AppName.Contains(s) ||
                        f.Status.Contains(s) ||
                        f.CurrencyId.Contains(s));
                }
            }

            IOrderedQueryable<MainReportsListRow> sorted = mainReportDetailsQuery.OrderByField(jqdt.mDataProps[jqdt.iSortCol_0], jqdt.sSortDir_0 == "asc");

            if (jqdt.sSortDir_1 != null)
            {
                sorted = sorted.ThenByField(jqdt.mDataProps[jqdt.iSortCol_1], jqdt.sSortDir_1 == "asc");
            }
            if (jqdt.sSortDir_2 != null)
            {
                sorted = sorted.ThenByField(jqdt.mDataProps[jqdt.iSortCol_2], jqdt.sSortDir_2 == "asc");
            }

            var data = sorted.Skip(jqdt.iDisplayStart).Take(jqdt.iDisplayLength).ToList();
            foreach (var item in data)
            {
                item.End = item.End.AddDays(-1);
               // item.Aitem.s
               
                
            }
            return this.MyJsonResult(
                new jQueryDataTableResult
                {
                    aaData = data,
                    iTotalRecords = source.Count(),
                    iTotalDisplayRecords = mainReportDetailsQuery.Count(),
                    sEcho = jqdt.sEcho
                }, JsonRequestBehavior.AllowGet);
        }

        private IQueryable<MainReportsListRow> GetReportDetailsQuery(int? regionId, int? countryId, int? stateId, int? agencyGroupId, DateTime? start, DateTime? end, int? statusId, bool? GGOnly, IQueryable<MainReport> source)
        {
            var SubReports = db.SubReports.AsQueryable();
            var ClientReports = db.ClientReports.AsQueryable();
            var ClientAmountReports = db.ClientAmountReports.AsQueryable();
            var EmergencyReports = db.EmergencyReports.AsQueryable();
            var MhmReports = db.MhmReports.AsQueryable();
            var MedicalEquipmentReports = db.MedicalEquipmentReports.AsQueryable();
            var AppBudgetServices = db.AppBudgetServices.AsQueryable();
            var ScReports = db.SupportiveCommunitiesReports.AsQueryable();
            var DccReports = db.DaysCentersReports.AsQueryable();
            var Services = db.Services.AsQueryable();

            var filtered = Filter(regionId, countryId, stateId, agencyGroupId, start, end, statusId, GGOnly == true, source);

            return MainReportsListQuery(SubReports, ClientReports, ClientAmountReports, EmergencyReports, MhmReports, MedicalEquipmentReports, AppBudgetServices, ScReports, DccReports, filtered, Services);
            

        }

        private IQueryable<MainReportsListRow> MainReportsListQuery(IQueryable<SubReport> SubReports, IQueryable<ClientReport> ClientReports, IQueryable<ClientAmountReport> ClientAmountReports, IQueryable<EmergencyReport> EmergencyReports, IQueryable<MhmReport> MhmReports, IQueryable<MedicalEquipmentReport> MedicalEquipmentReports, IQueryable<AppBudgetService> AppBudgetServices, IQueryable<SupportiveCommunitiesReport> ScReports, IQueryable<DaysCentersReport> DccReports, IQueryable<MainReport> filtered, IQueryable<Service> Services)
        {

            //var subReportId = ""
            //  var subReportId = from mr in filtered
            // from sr in db.SubReports
            // where sr.MainReportId == mr.Id
            //  join abs in db.AppBudgetServices on sr.AppBudgetServiceId equals abs.Id
            //  join serv in db.Services on abs.ServiceId equals serv.Id
            //  where serv.Id == 454 
            //  select new
            //  {
            //    subrepid = sr.Id
            //  };

            //foreach (var item in subReportId)
            // {
            //  public  test == item.subrepid;
            //};

            // var test = subReportId.AsQueryable().FirstOrDefault();

            // var test = subReportId.Each()

            //FunExpAmount = item.Amount

            //&& serv.Id == 454
            //Where ser.id = 454 //&& sr.MainReportId =
            /// var FunExpAmount = from a in
            // from sr in SubReports
            //  join cr in db.ClientReports on sr.Id equals cr.SubReportId into funexam
            //  select new { Amount = funexam.Sum(f => f.Amount)}
            //   select new { a.Amount };


            // var FunExpAmount = (from mr in filtered
            //   from sr in db.SubReports
            // where sr.MainReportId == mr.Id
            //  join cr in db.ClientReports on sr.Id equals cr.SubReportId  
            //  join abs in db.AppBudgetServices on sr.AppBudgetServiceId equals abs.Id
            // join ser in db.Services on abs.ServiceId equals ser.Id
            // where ser.Id == 454 //&& cr.Amount != null
            // select cr.Amount).Sum();

            //  var FunExpAmount1 = (from cr1 in db.ClientReports
            //  join sr in db.SubReports on cr1.SubReportId equals sr.Id
            // from mr in filtered
            // where sr.MainReportId == mr.Id
            //  select cr1.Amount).Sum();

            //var Subreportid = (from mr in filtered
            //  from sr in db.SubReports
            // where sr.MainReportId == mr.Id
            // select sr.Id);


            var mainReportDetailsQuery = from mr in filtered
                                         let appBudget = mr.AppBudget
                                         let app = appBudget.App
                                         let fund = app.Fund
                                         let masterFund = fund.MasterFund
                                         let agencyGroup = app.AgencyGroup
                                        // let subreport = mr.SubReports.Where(f => f.MainReportId == mr.Id).Select(f => f.Id)
                                        




                                                  select new MainReportsListRow
                                                  {
                                                      AgencyGroupName = agencyGroup.DisplayName,
                                                      FundName = fund.Name,
                                                      MasterFundName = masterFund.Name,
                                                      AppName = app.Name,
                                                      Start = mr.Start,
                                                      End = mr.End,

                                                      Status = mr.StatusId == (int)MainReport.Statuses.Approved ? "Approved" :
                                                           mr.StatusId == (int)MainReport.Statuses.AwaitingProgramOfficerApproval ? "AwaitingProgramOfficerApproval" :
                                                           mr.StatusId == (int)MainReport.Statuses.AwaitingProgramAssistantApproval ? "AwaitingProgramAssistantApproval" :
                                                           mr.StatusId == (int)MainReport.Statuses.Cancelled ? "Cancelled" :
                                                           mr.StatusId == (int)MainReport.Statuses.New ? "New" :
                                                           mr.StatusId == (int)MainReport.Statuses.Rejected ? "Rejected" :
                                                           mr.StatusId == (int)MainReport.Statuses.ReturnedToAgency ? "Returned to Agency" :
                                                           mr.StatusId == (int)MainReport.Statuses.AwaitingAgencyResponse ? "Awaiting Agency Response" :
                                                           string.Empty,
                                                           
                                             Amount = //(db.SubReports.Where(f => f.MainReportId == mr.Id).Sum(f => f.Amount) ?? 0) +
                                                      (db.viewSubreportAmounts.Where(f => f.MainReportId == mr.Id).Sum(f => (decimal?)f.Amount) ?? 0),
                                            // ? (decimal?)subReport.MatchingSum : (decimal?)null,
                                                      // (db.ClientReports.Where(f => f.SubReportId == subreport.Select(f => f.i).Sum(f => f.Amount) ?? 0),
                                                      //(db.ClientReports.Where(f => f.SubReportId == (db.SubReports.Where(f => f.MainReportId == mr.Id).Distinct(f => f.Id)
                                                      //.i.mr.SubReports.Where(f => f.MainReportId==mr.Id)).Sum(f => f.Amount) ?? 0)
                                                      //,
                                                      // (db.ClientReports.Where(f => f.SubReportId == test.subrepid).Sum(f => f.Amount) ?? 0),
                                                      // (db.ClientReports.Where(f => f.SubReportId == subReportId.FirstOrDefault()).Sum((f => f.Amount) ?? 0),
                                                      //ServiceId = ()
                                                      CcGrant = app.CcGrant,
                                             CurrencyId = app.CurrencyId,
                                           

                                             Adjusted = mr.Adjusted,
                                             Revised = mr.Revision,

                                             StatusId = mr.StatusId,

                                             MainReportId = mr.Id,
                                             AgencyGroupId = app.AgencyGroupId,
                                             AppBudgetId = mr.AppBudgetId,
                                             AppId = appBudget.AppId,
                                             FundId =app.FundId,
                                             SubmittedAt = mr.SubmittedAt,
                                            

                                             CanBeRevised =
                                                 (!mr.Revision) && agencyGroup.CanSubmitRevisionReports &&
                                                 (mr.StatusId == (int)MainReport.Statuses.Approved) &&
                                                 (this.Permissions.User.RoleId == (int)FixedRoles.Ser || this.Permissions.User.RoleId == (int)FixedRoles.SerAndReviewer || this.Permissions.User.RoleId == (int)FixedRoles.Admin)
                                         };
            //test = null;
            
            return mainReportDetailsQuery;
             
        }

        private static IQueryable<MainReport> Filter(int? regionId, int? countryId, int? stateId, int? agencyGroupId, DateTime? start, DateTime? end, int? statusId, bool? GGOnly, IQueryable<MainReport> filtered)
        {
            if (regionId.HasValue) { filtered = filtered.Where(f => f.AppBudget.App.AgencyGroup.Country.RegionId == regionId); }
            if (countryId.HasValue) { filtered = filtered.Where(f => f.AppBudget.App.AgencyGroup.CountryId == countryId); }
            if (stateId.HasValue) { filtered = filtered.Where(f => f.AppBudget.App.AgencyGroup.StateId == stateId); }
            if (agencyGroupId.HasValue) { filtered = filtered.Where(f => f.AppBudget.App.AgencyGroupId == agencyGroupId); }
            if (start.HasValue) { filtered = filtered.Where(f => f.End > start); }
            if (end.HasValue) { filtered = filtered.Where(f => f.Start < end); }
            if (statusId.HasValue) { filtered = filtered.Where(f => f.StatusId == statusId); }
            if (GGOnly == true) { filtered = filtered.Where(f => f.AppBudget.App.Fund.MasterFundId == 73); }
            return filtered;
        }


        [HttpGet]
        [CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
        public ActionResult SelectApp()
        {
            var model = new MainReportSelectAppModel(db, CcUser, Permissions);

            return View(model);
        }

        [HttpPost]
        [CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
        public ActionResult SelectApp(MainReportSelectAppModel model)
        {

            if (model.AppId.HasValue)
            {
                var appbudget = db.AppBudgets.Where(this.Permissions.AppBudgetsFilter).SingleOrDefault(f => f.AppId == model.AppId);

                if (appbudget == null)
                {
                    ModelState.AddModelError(string.Empty, "The selected #Budget has not been approved yet by CC, please go to the Budget Approval screen and initiate a new approval process ");
                }
                else if (!AppBudget.ValidStatuses.Contains(appbudget.ApprovalStatus))
                {
                    ModelState.AddModelError(string.Empty, "The selected #Budget has not been approved yet by CC, please go to the Budget Approval screen and follow an existing process");
                }


                if (ModelState.IsValid)
                {
                    return RedirectToAction("Create", new { appBudgetId = appbudget.Id });
                }
            }


            model.LoadData(this.db, this.Permissions, this.CcUser);
            return View(model);
        }

        [HttpGet]
        [CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
        public ActionResult Create(int? appBudgetId)
        {
            var model = new MainReportCreateModel(db, CcUser, Permissions);


            if (appBudgetId.HasValue)
            {
                model.MainReport.AppBudgetId = appBudgetId.Value;
                try
                {
                    model.LoadRelatedData(db, Permissions);
                }
                catch (InvalidOperationException)
                {
                    model.IsValid = false;
                    ModelState.AddModelError(string.Empty, "Budget could not be found");
                }
            }
            else
            {
                model.IsValid = false;
                ModelState.AddModelError(string.Empty, "invalid request");
            }

            return View(model);
        }

        [HttpPost]
        [CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
        public ActionResult Create(MainReportCreateModel model)
        {
            model.LoadRelatedData(db, Permissions);

            //default period duration
            model.MainReport.Start = model.MainReport.Start.Date;
            model.MainReport.End = model.MainReport.Start.Date.AddMonths(model.MainReport.AppBudget.App.AgencyGroup.ReportingPeriodId);

            model.MainReport.UpdatedAt = DateTime.Now;
            model.MainReport.UpdatedById = this.CcUser.Id;


            ModelState.Clear();
            var validationResults = model.MainReport.Validate(new ValidationContext(model.MainReport, null, null));
            foreach (var validationresult in validationResults)
            {
                ModelState.AddModelError(string.Join(",", validationresult.MemberNames), validationresult.ErrorMessage);
            }

            if (ModelState.IsValid)
            {

                db.MainReports.AddObject(model.MainReport);
                try
                {
                    var rowsUpdates = db.SaveChanges();

                    return RedirectToAction("Details", new { id = model.MainReport.Id });
                }
                catch (System.Data.UpdateException ex)
                {
                    if (ex.IsUqViolation())
                    {
                        ModelState.AddModelError(string.Empty, "A report with this start period and this App has already been created, please select another period/ App.");
                    }
                    else
                    {
                        //unique constraint violation
                        _log.Error("failed to create	mainreport", ex);
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            return View(model);
        }


        [HttpGet]
        public ActionResult Details(int id)
        {
            var model = GetDetailsModel(id);

            ViewBag.DayCentersId = 17; //set default as in prod
            var dayCenters = db.Services.FirstOrDefault(f => f.ReportingMethodId == (int)Service.ReportingMethods.DayCenters);
            if (dayCenters != null)
            {
                ViewBag.DayCentersId = dayCenters.TypeId;
            }

            ViewBag.SoupKitchensId = 0;
            var soupKithcens = db.Services.FirstOrDefault(f => f.ReportingMethodId == (int)Service.ReportingMethods.SoupKitchens);
            if (soupKithcens != null)
            {
                ViewBag.SoupKitchensId = soupKithcens.TypeId;
            }

            return View("Details", model);
        }
        public JsonResult DetailsData(int id, jQueryDataTableParamModel jq)
        {

            var q = GetSubreports(id);

            var filtered = AppliyFilter(jq, q);

            var sorted = ApplySoring(jq, filtered);

            var result = new jQueryDataTableResult
            {
                sEcho = jq.sEcho,
                iTotalRecords = q.Count(),
                iTotalDisplayRecords = filtered.Count(),
                aaData = sorted.Skip(jq.iDisplayStart).Take(jq.iDisplayLength).ToList()
            };
            return this.MyJsonResult(result, JsonRequestBehavior.AllowGet);

        }

        private IQueryable<tmpclass> ApplySoring(jQueryDataTableParamModel jq, IQueryable<tmpclass> filtered)
        {
            List<string> orderByStrings = new List<string>(); ;
            for (int i = 0; i < jq.iSortingCols; i++)
            {
                var index = Request["iSortCol_" + i];
                var name = Request["mDataProp_" + index];
                var dir = Request["sSortdir_" + i];
                orderByStrings.Add(name + " " + dir);
            }
            var sorted = filtered.OrderBy(string.Join(",", orderByStrings));
            return sorted;
        }

        private IQueryable<tmpclass> AppliyFilter(jQueryDataTableParamModel jq, IQueryable<tmpclass> q)
        {
            var filtered = q;
            if (!string.IsNullOrEmpty(jq.sSearch))
            {
                foreach (var s in jq.sSearch.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Where(f => !string.IsNullOrEmpty(f)))
                {
                    filtered = filtered.Where(f => f.AgencyName.Contains(s)
                        || f.ServiceTypeName.Contains(s)
                        || f.ServiceName.Contains(s));
                }
            }
            return filtered;
        }

        private IQueryable<tmpclass> GetSubreports(int id)
        {
            var ytdStart = db.MainReports.Where(Permissions.MainReportsFilter)
                .Where(f => f.Id == id).Select(f => f.Start).SingleOrDefault();

           

            

            var SubReports = db.SubReports.Where(this.Permissions.SubReportsFilter).Where(f => f.MainReportId == id);
           // var FunExpAmount = from a in
                                         // from sr in SubReportswher
                                         //  join cr in db.ClientReports on sr.Id equals cr.SubReportId into funexam
                                         //  select new { Amount = funexam.Sum(f => f.Amount)}
                                        //   select new { a.Amount };
             

            var editableStatuses = MainReport.EditableStatuses.Select(f => (int)f).ToArray();
            var q = from mainReport in db.MainReports.Where(this.Permissions.MainReportsFilter)
                    where mainReport.Id == id
                    join appBudgetService in db.AppBudgetServices.Where(this.Permissions.AppBudgetServicesFilter)
                           on mainReport.AppBudgetId equals appBudgetService.AppBudgetId
                    join subReport in db.SubReports.Where(this.Permissions.SubReportsFilter) on new { a = mainReport.Id, b = appBudgetService.Id } equals new { a = subReport.MainReportId, b = subReport.AppBudgetServiceId } into subReportGroup
                    from subReport in subReportGroup.DefaultIfEmpty()
                    //join clientReport in db.ClientReports.Where(this.Permissions.ClientReportsFilter) on new { a = subReport.Id} equals new { a = clientReport.SubReportId} into clientReportGroup
                   // from clientReport  in clientReportGroup.DefaultIfEmpty()//.Where(f => f.Amount != null).DefaultIfEmpty()
                    join appBudgetServiceTotals in
                        (from a in
                             (from item in db.SubReports
                              join innerabs in db.AppBudgetServices on item.AppBudgetServiceId equals innerabs.Id
                              join mr in db.MainReports.Where(MainReport.Submitted) on item.MainReportId equals mr.Id
                              //join cr in db.ClientReports on item.Id equals cr.SubReportId where cr.Amount != null
                              where mr.Id != id
                              where mr.Start < ytdStart
                              select new
                              {
                                  asdf = innerabs,
                                  MainRepLen = ccEntities.DiffMonths(mr.Start, mr.End),
                                  Amount = item.Amount,
                                 // FunExpAmount = cr.Amount,
                                  MatchingSum = item.MatchingSum,
                                  AgencyContribution = item.AgencyContribution
                              })
                         group a by a.asdf into g
                         select new
                         {
                             Key = g.Key.Id,
                             RequiredMatch = g.Key.RequiredMatch * g.Sum(f => f.MainRepLen),
                             Amount = g.Sum(f => f.Amount),
                            // FunExpAmount = g.Sum(f => f.FunExpAmount), 
                             MatchingSum = g.Sum(f => f.MatchingSum),
                             AgencyContribution = g.Sum(f => f.AgencyContribution)

                         }) on appBudgetService.Id equals appBudgetServiceTotals.Key into appBudgetServiceTotalsGroup
                    from appbudgetservicetotals in appBudgetServiceTotalsGroup.DefaultIfEmpty()
                    join FuneralExpAmountTotal in
                    (from a in
                             (from item in db.ClientReports
                              join sr in db.SubReports on item.SubReportId equals sr.Id
                              where sr.MainReportId == id
                              join innerabs in db.AppBudgetServices on sr.AppBudgetServiceId equals innerabs.Id
                              where innerabs.ServiceId== 484 //454 //LenaKFE
                                                             //join serv in db.Services on innerabs.ServiceId equals serv.Id

                              //join mr in db.MainReports.Where(MainReport.Submitted) on sr.MainReportId equals mr.Id
                              // where mr.Id != id
                              //where item.Amount != null && serv.Id == 454 //LenaKFE
                              // where  serv.Id == 454//LenaKFE
                              select new
                              {
                                  asdf = innerabs,
                                  FunExpAmount = item.Amount
                              })
                     group a by a.asdf into g
                     select new
                     {
                         Key = g.Key.Id,
                         FunExpAmount = g.Sum(f => f.FunExpAmount)

                     }) on appBudgetService.Id equals FuneralExpAmountTotal.Key into FuneralExpAmountTotalGroup
                    from FuneralExpAmountTotal in FuneralExpAmountTotalGroup.DefaultIfEmpty()
                    join FuneralExpAmountTotalYTD in
                    (from a in
                             (from item in db.ClientReports
                              join sr in db.SubReports on item.SubReportId equals sr.Id //where sr.MainReportId == id
                              join innerabs in db.AppBudgetServices on sr.AppBudgetServiceId equals innerabs.Id
                              // join mr in db.MainReports.Where(MainReport.Submitted) on sr.MainReportId equals mr.Id
                              join mr in db.MainReports on sr.MainReportId equals mr.Id
                              //join serv in db.Services on innerabs.ServiceId equals serv.Id
                              where mr.Id != id
                              where mr.Start < ytdStart
                              where innerabs.ServiceId == 484 //484454 //item.Amount != null //LenaKMay
                              select new
                              {
                                  asdf = innerabs,
                                  FunExpAmountYTD = item.Amount
                              }) group a by a.asdf into g
                     select new
                     {
                         Key = g.Key.Id,
                         FunExpAmountYTD = g.Sum(f => f.FunExpAmountYTD)
                        
                     }) on appBudgetService.Id equals FuneralExpAmountTotalYTD.Key into FuneralExpAmountTotalYTDGroup
                    from FuneralExpAmountTotalYTD in FuneralExpAmountTotalYTDGroup.DefaultIfEmpty()

                     join smr in db.MainReports
                        .Where(MainReport.CurrentOrSubmitted(id))
                        .Where(f => f.Start <= ytdStart)
                    on appBudgetService.AppBudgetId equals smr.AppBudgetId into smrg
                    where subReport.Id != null || !editableStatuses.Contains(mainReport.StatusId) || appBudgetService.Service.Active
                    select new tmpclass()
                    {
                        Id = (int?)subReport.Id,
                        AppBudgetServiceId = appBudgetService.Id,
                        AgencyName = appBudgetService.Agency.Name,
                        ServiceTypeId = appBudgetService.Service.TypeId,
                        ServiceTypeName = appBudgetService.Service.ServiceType.Name,
                        ServiceName = appBudgetService.Service.Name,
                        CcGrant = appBudgetService.Service.ReportingMethodId != (int)Service.ReportingMethods.ClientEventsCount ? appBudgetService.CcGrant : (decimal?)null,
                       // CcExp = appBudgetService.Service.ReportingMethodId != (int)Service.ReportingMethods.ClientEventsCount ? (decimal?)subReport.Amount : (decimal?)null,
                       // if(appBudgetService.Service.Name == "Funeral Expenses")

                        CcExp = appBudgetService.Service.ReportingMethodId != (int)Service.ReportingMethods.ClientEventsCount ? (subReport.Amount ?? 0) + (FuneralExpAmountTotal.FunExpAmount ?? 0) : (decimal?)null,
                        MatchExp = appBudgetService.Service.ReportingMethodId != (int)Service.ReportingMethods.ClientEventsCount ? (decimal?)subReport.MatchingSum : (decimal?)null,
                        AgencyContribution = appBudgetService.Service.ReportingMethodId != (int)Service.ReportingMethods.ClientEventsCount ? (decimal?)subReport.AgencyContribution : (decimal?)null,
                        RequiredMatch = appBudgetService.Service.ReportingMethodId != (int)Service.ReportingMethods.ClientEventsCount ? (decimal?)appBudgetService.RequiredMatch : (decimal?)null,
                        RequiredAgencyContribution = appBudgetService.Service.ReportingMethodId != (int)Service.ReportingMethods.ClientEventsCount ? (decimal?)appBudgetService.AgencyContribution : (decimal?)null,
                        //YtdCcExp = appBudgetService.Service.ReportingMethodId != (int)Service.ReportingMethods.ClientEventsCount ? (appbudgetservicetotals.Amount ?? 0) + (subReport.Amount ?? 0) + (FuneralExpAmountTotal.FunExpAmount ?? 0) + (FuneralExpAmountTotalYTD.FunExpAmountYTD ?? 0)  : (decimal?)null,
                        YtdCcExp = appBudgetService.Service.ReportingMethodId != (int)Service.ReportingMethods.ClientEventsCount ? (appbudgetservicetotals.Amount ?? 0) +(subReport.Amount ?? 0) + (FuneralExpAmountTotal.FunExpAmount ?? 0) + (FuneralExpAmountTotalYTD.FunExpAmountYTD ?? 0) : (decimal?)null,//(appbudgetservicetotals.Amount ?? 0) : (decimal?)null,// +(subReport.Amount ?? 0) +(FuneralExpAmountTotal.FunExpAmount ?? 0) + (FuneralExpAmountTotalYTD.FunExpAmountYTD ?? 0) : (decimal?)null,
                       // YtdCcExp = appBudgetService.Service.ReportingMethodId != (int)Service.ReportingMethods.ClientEventsCount ? (appbudgetservicetotals.Amount ?? 0) + (FuneralExpAmountTotalYTD.FunExpAmountYTD ?? 0) : (decimal?)null,
                        YtdMatchExp = appBudgetService.Service.ReportingMethodId != (int)Service.ReportingMethods.ClientEventsCount ? (appbudgetservicetotals.MatchingSum ?? 0) + (subReport.MatchingSum ?? 0) : (decimal?)null,
                        YtdAgencyContribution = appBudgetService.Service.ReportingMethodId != (int)Service.ReportingMethods.ClientEventsCount ? (appbudgetservicetotals.AgencyContribution ?? 0) + (subReport.AgencyContribution ?? 0) : (decimal?)null,
                        YtdMatchExpStatus = appBudgetService.Service.ReportingMethodId != (int)Service.ReportingMethods.ClientEventsCount ? (decimal?)
                            (appbudgetservicetotals.MatchingSum ?? 0) + (subReport.MatchingSum ?? 0) -
                             appbudgetservicetotals.RequiredMatch / ccEntities.DiffMonths(mainReport.AppBudget.App.StartDate, mainReport.AppBudget.App.EndDate) :
                             (decimal?)null,

                        AppBudgetServiceRemarks = appBudgetService.Service.ReportingMethodId != (int)Service.ReportingMethods.ClientEventsCount ? appBudgetService.Remarks : "",
                        Cur = appBudgetService.Service.ReportingMethodId != (int)Service.ReportingMethods.ClientEventsCount ? appBudgetService.AppBudget.App.CurrencyId : "",
                        FirstHomecareWeekly = appBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.HomecareWeekly
                            && !db.SubReports.Any(f => f.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.HomecareWeekly && f.MainReport.Start.Year == mainReport.Start.Year
                                && f.AppBudgetService.Agency.GroupId == appBudgetService.Agency.GroupId && f.AppBudgetService.AppBudget.AppId == appBudgetService.AppBudget.AppId) &&
                            !db.AgencyApps.Any(f => f.Agency.GroupId == appBudgetService.Agency.GroupId && f.AppId == appBudgetService.AppBudget.AppId)

                    };
            return q;
        }

        private IQueryable<tmpclassforbmf> GetSubreportsForBmf(int id)
        {
            var ytdStart = db.MainReports.Where(Permissions.MainReportsFilter)
                .Where(f => f.Id == id).Select(f => f.Start).SingleOrDefault();
            var editableStatusIds = MainReport.EditableStatuses.Select(f => (int)f).ToArray();
            var SubReports = db.SubReports.Where(this.Permissions.SubReportsFilter).Where(f => f.MainReportId == id);
            var q = from mainReport in db.MainReports.Where(this.Permissions.MainReportsFilter)
                    where mainReport.Id == id
                    join appBudgetService in db.AppBudgetServices.Where(this.Permissions.AppBudgetServicesFilter)
                           on mainReport.AppBudgetId equals appBudgetService.AppBudgetId
                    join subReport in db.SubReports.Where(this.Permissions.SubReportsFilter) on new { a = mainReport.Id, b = appBudgetService.Id } equals new { a = subReport.MainReportId, b = subReport.AppBudgetServiceId } into subReportGroup
                    from subReport in subReportGroup.DefaultIfEmpty()
                    join subReportAmount in db.viewSubreportAmounts on subReport.Id equals subReportAmount.id into subReportAmountGroup
                    from subReportAmount in subReportAmountGroup.DefaultIfEmpty()
                    join appBudgetServiceTotals in
                        (from a in
                             (from item in db.viewSubreportAmounts
                              join innerabs in db.AppBudgetServices on item.AppBudgetServiceId equals innerabs.Id
                              join mr in db.MainReports.Where(MainReport.Submitted) on item.MainReportId equals mr.Id
                              where mr.Id != id
                              where mr.Start < ytdStart
                              select new
                              {
                                  asdf = innerabs,
                                  MainRepLen = ccEntities.DiffMonths(mr.Start, mr.End),
                                  Amount = item.Amount,
                                  MatchingSum = item.MatchingSum,
                                  AgencyContribution = item.AgencyContribution
                              })
                         group a by a.asdf into g
                         select new
                         {
                             Key = g.Key.Id,
                             RequiredMatch = g.Key.RequiredMatch * g.Sum(f => f.MainRepLen),
                             Amount = g.Sum(f => f.Amount),
                             MatchingSum = g.Sum(f => f.MatchingSum),
                             AgencyContribution = g.Sum(f => f.AgencyContribution)

                         }) on appBudgetService.Id equals appBudgetServiceTotals.Key into appBudgetServiceTotalsGroup
                    from appbudgetservicetotals in appBudgetServiceTotalsGroup.DefaultIfEmpty()
                    join smr in db.MainReports
                        .Where(MainReport.CurrentOrSubmitted(id))
                        .Where(f => f.Start <= ytdStart)
                    on appBudgetService.AppBudgetId equals smr.AppBudgetId into smrg
                    where subReport.Id != null || !editableStatusIds.Contains(mainReport.StatusId) || appBudgetService.Service.Active
                    select new tmpclassforbmf()
                    {
                        Id = (int?)subReport.Id,
                        AppBudgetServiceId = appBudgetService.Id,
                        AgencyName = appBudgetService.Agency.Name,
                        ServiceTypeName = appBudgetService.Service.ServiceType.Name,
                        ServiceName = appBudgetService.Service.Name,
                        CcGrant = appBudgetService.Service.ReportingMethodId != (int)Service.ReportingMethods.ClientEventsCount ? appBudgetService.CcGrant : (decimal?)null,
                        CcExp = appBudgetService.Service.ReportingMethodId != (int)Service.ReportingMethods.ClientEventsCount ? (decimal?)subReportAmount.Amount : (decimal?)null,
                        YtdCcExp = appBudgetService.Service.ReportingMethodId != (int)Service.ReportingMethods.ClientEventsCount ? appbudgetservicetotals.Amount + subReportAmount.Amount : (decimal?)null,
                        AppBudgetServiceRemarks = appBudgetService.Service.ReportingMethodId != (int)Service.ReportingMethods.ClientEventsCount ? appBudgetService.Remarks : "",
                        Cur = appBudgetService.Service.ReportingMethodId != (int)Service.ReportingMethods.ClientEventsCount ? appBudgetService.AppBudget.App.CurrencyId : "",

                    };
            return q;
        }


        [HttpPost]
        [CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
        public ActionResult Delete(int id)
        {
            var mr = new MainReport() { Id = id };
            db.MainReports.Attach(mr);
            db.MainReports.DeleteObject(mr);

            try
            {
                var rowsUpdated = db.SaveChanges();
                return this.RedirectToAction(f => f.Index(null));
            }
            catch (System.Data.UpdateException ex)
            {
                _log.Error(ex);
                ModelState.AddModelError(string.Empty, "Sorry, this report can not be deleted.");
                return Details(id);
            }

        }

        private MainReportDetailsModel GetDetailsModel(int id)
        {
            var model = new MainReportDetailsModel()
            {
                Id = id
            };

            model.mainReportsRepository = new GenericRepository<MainReport>(db);
            model.appBudgetServicesRepository = new GenericRepository<AppBudgetService>(db);
            model.SubReportsRepository = new GenericRepository<SubReport>(db);

            try
            {
                model.LoadData();
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }
                _log.Error(null, ex);
                throw;

            }
            return model;
        }

        [CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
        public ActionResult Revise(int id)
        {
            var mr = db.MainReports.Where(this.Permissions.MainReportsFilter).SingleOrDefault(f => f.Id == id);

            if (mr == null)
            {
                //error
            }
            else
            {
                if (mr.Revision)
                {
                    throw new Exception("The report is already revised");
                }
                else
                {
                    var revised = db.MainReports.Where(this.Permissions.MainReportsFilter).SingleOrDefault(f => f.Start == mr.Start && f.AppBudgetId == mr.AppBudgetId && mr.Revision);
                    if (revised == null)
                    {
                        revised = new MainReport()
                        {
                            AppBudgetId = mr.AppBudgetId,
                            Revision = true,
                            Start = mr.Start,
                            End = mr.End,
                            Status = MainReport.Statuses.New,
                            UpdatedAt = DateTime.Now,
                            UpdatedById = this.Permissions.User.Id
                        };
                        db.MainReports.AddObject(revised);
                        db.SaveChanges();
                    }
                    return this.RedirectToAction(f => f.Details(revised.Id));
                }
            }
            return this.RedirectToAction(f => f.Index(new MainReportsListModel()));
        }


        public ActionResult StatusAudit(int id)
        {

            var model = GetStatusAuditDetails(id);

            try
            {
                model.LoadData();
            }
            catch (Exception ex)
            {
                _log.Error(null, ex);
                return this.RedirectToAction(f => f.Index(new MainReportsListModel()));
            }



            return View("StatusAudit", model);
        }

        private MainReportStatusHistoryModel GetStatusAuditDetails(int id)
        {
            var model = new MainReportStatusHistoryModel()
            {
                Id = id
            };

            model.mainReportsRepository = new GenericRepository<MainReport>(db);
            model.appBudgetServicesRepository = new GenericRepository<AppBudgetService>(db);
            model.SubReportsRepository = new GenericRepository<SubReport>(db);
            model.usersRepostiory = new GenericRepository<User>(db);
            model.StatusHistoryRepository = new GenericRepository<MainReportStatusAudit>(db);

            return model;
        }

        public ActionResult Export(int? regionId, int? countryId, int? stateId, int? agencyGroupId, DateTime? start, DateTime? end, int? statusId, bool? GGOnly)
        {
            var MainReports = db.MainReports.AsQueryable();
            IQueryable<MainReport> source = MainReports.Where(this.Permissions.MainReportsFilter);
            var result = GetReportDetailsQuery(regionId, countryId, stateId, agencyGroupId, start, end, statusId, GGOnly, source).ToList();
            var data = (from r in result
                        select new FinancialReportsListRow
                        {
                            SerName = r.AgencyGroupName,
                            FundName = r.FundName,
                            MasterFundName = r.MasterFundName,
                            AppName = r.AppName,
                            Start = r.Start,
                            End = r.End.AddDays(-1),
                            Status = r.Status,
                            SubmittedAt = r.SubmittedAt,
                            Amount = r.Amount,
                            Currency = r.CurrencyId,
                            CcGrant = r.CcGrant,
                            CurrencyName = r.CurrencyId
                        }).ToList();
            return this.Excel("Financial Reports List", "Financial Reports List", data);
        }

        public ActionResult ExportDetails(int id, jQueryDataTableParamModel jq)
        {
            if (!User.IsInRole("BMF"))
            {
                var data = this.GetSubreports(id);
                return this.Excel("Details", "Data", data);
            }
            else
            {
                var data = this.GetSubreportsForBmf(id);
                return this.Excel("Details", "Data", data);
            }
        }

        [CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer, FixedRoles.RegionOfficer, FixedRoles.RegionAssistant, FixedRoles.GlobalReadOnly, FixedRoles.RegionReadOnly, FixedRoles.AuditorReadOnly)]

        public class ReportInPdf //fluxx
        {
            public byte[] ReportContent;
            public MainReportDetailsModel MainReportDetailsModel;

        }

        //----------------------
        [CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer, FixedRoles.RegionOfficer, FixedRoles.RegionAssistant, FixedRoles.GlobalReadOnly, FixedRoles.RegionReadOnly, FixedRoles.AuditorReadOnly)]
        //fluxx, former method public void ExportToPdf(int id)
        ReportInPdf GetPDF(int id)
        {
           
            db.ContextOptions.LazyLoadingEnabled = true;
            db.ContextOptions.ProxyCreationEnabled = true;
            db.CommandTimeout =10000;

          //  var connectiontimeout = db.Connection.ConnectionTimeout;

            ReportInPdf reportInPdf = null;
            Document doc = new Document(PageSize.A4.Rotate(), 2, 2, 2, 2);
            string filePath = System.IO.Path.GetTempFileName();
            NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;
            int widthPercent = 95;

            try
            {
                PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));

                var financialReportDetails = GetDetailsModel(id);

                PdfPTable pdfTabFinancialDetails = new PdfPTable(8);
                pdfTabFinancialDetails.HorizontalAlignment = 1;
                pdfTabFinancialDetails.SpacingBefore = 5f;
                pdfTabFinancialDetails.SpacingAfter = 5f;
                pdfTabFinancialDetails.WidthPercentage = widthPercent;
                PdfPCell headerCellFinancialDetails = new PdfPCell(new Phrase("Financial Report Details"));
                headerCellFinancialDetails.Colspan = 8;
                headerCellFinancialDetails.Border = 0;
                headerCellFinancialDetails.HorizontalAlignment = 1;
                pdfTabFinancialDetails.AddCell(headerCellFinancialDetails);
                pdfTabFinancialDetails.AddCell("Fund");
                pdfTabFinancialDetails.AddCell("Master Fund");
                pdfTabFinancialDetails.AddCell("App #");
                pdfTabFinancialDetails.AddCell("Month From");
                pdfTabFinancialDetails.AddCell("Month To");
                pdfTabFinancialDetails.AddCell("Ser");
                pdfTabFinancialDetails.AddCell("Exc. Rate");
                pdfTabFinancialDetails.AddCell("Report Status");
                pdfTabFinancialDetails.AddCell(financialReportDetails.FundName);
                pdfTabFinancialDetails.AddCell(financialReportDetails.MasterFundName);
                pdfTabFinancialDetails.AddCell(financialReportDetails.AppName);
                var dateStr = financialReportDetails.Start.ToString("MMM yyyy");
                pdfTabFinancialDetails.AddCell(dateStr);
                dateStr = financialReportDetails.End.ToString("MMM yyyy");
                pdfTabFinancialDetails.AddCell(dateStr);
                pdfTabFinancialDetails.AddCell(financialReportDetails.AgencyGroupName);
                var excRate = financialReportDetails.ExcRate.HasValue ? financialReportDetails.ExcRate.Value.Format() : "N/A";
                pdfTabFinancialDetails.AddCell(excRate + " Source: " + financialReportDetails.ExcRateSource);
                pdfTabFinancialDetails.AddCell(financialReportDetails.Status.DisplayName());

                PdfPTable pdfTabAppTotals = new PdfPTable(6);
                pdfTabAppTotals.HorizontalAlignment = 1;
                pdfTabAppTotals.SpacingBefore = 5f;
                pdfTabAppTotals.SpacingAfter = 5f;
                pdfTabAppTotals.WidthPercentage = widthPercent;
                PdfPCell headerCellAppTotals = new PdfPCell(new Phrase("App Totals"));
                headerCellAppTotals.Colspan = 6;
                headerCellAppTotals.Border = 0;
                headerCellAppTotals.HorizontalAlignment = 1;
                pdfTabAppTotals.AddCell(headerCellAppTotals);
                pdfTabAppTotals.AddCell("App Amount");
                pdfTabAppTotals.AddCell("Total grant as specified by Ser");
                pdfTabAppTotals.AddCell("Total Reported on this Report");
                pdfTabAppTotals.AddCell("Total Reported on this App (including this report)");
                pdfTabAppTotals.AddCell("App Balance");
                pdfTabAppTotals.AddCell("App Match Balance");
                var tempValue = financialReportDetails.Totals.AppAmount.HasValue ? financialReportDetails.Totals.AppAmount.Value.Format() : "0";
                pdfTabAppTotals.AddCell(tempValue + " " + financialReportDetails.Totals.CurrencyId);
                tempValue = financialReportDetails.Totals.CcGrant.HasValue ? financialReportDetails.Totals.CcGrant.Value.Format() : "0";
                pdfTabAppTotals.AddCell(tempValue + " " + financialReportDetails.Totals.CurrencyId);
                tempValue = financialReportDetails.Totals.CcExp.HasValue ? financialReportDetails.Totals.CcExp.Value.Format() : "0";
                pdfTabAppTotals.AddCell(tempValue + " " + financialReportDetails.Totals.CurrencyId);
                // tempValue = financialReportDetails.Totals.YtdCcExp.HasValue ? financialReportDetails.Totals.YtdCcExp.Value.Format() : "0";
                tempValue = financialReportDetails.Totals.TotalReportedApp.HasValue ? financialReportDetails.Totals.TotalReportedApp.Value.Format() : "0";
                pdfTabAppTotals.AddCell(tempValue + " " + financialReportDetails.Totals.CurrencyId);
                tempValue = financialReportDetails.Totals.AppBalance.HasValue ? financialReportDetails.Totals.AppBalance.Value.Format() : "0";
                pdfTabAppTotals.AddCell(tempValue + " " + financialReportDetails.Totals.CurrencyId);
               // tempValue = string.Format("N{0}", 3, financialReportDetails.Totals.AppMatchBalance.Value.Format());
                tempValue = financialReportDetails.Totals.AppMatchBalance.HasValue ? financialReportDetails.Totals.AppMatchBalance.Value.Format() : "0";
                //  tempValue = financialReportDetails.Totals.TotalReportedAppMatchBalance.HasValue ? financialReportDetails.Totals.TotalReportedAppMatchBalance.Value.Format() : "0";//Lena commented for issue 81
                pdfTabAppTotals.AddCell(tempValue + " " + financialReportDetails.Totals.CurrencyId);
                foreach (var total in financialReportDetails.Totals.AgencySubTotals)
                {
                    var ccExpValue = total.CcExp.HasValue ? total.CcExp.Value.Format() : "0";
                    var ccYtdExpValue = total.YtdCcExp.HasValue ? total.YtdCcExp.Value.Format() : "0";
                    //LenaPDF
                   // var ccYtdExpValue = AppBudgetTotals..t.HasValue ? total.YtdCcExp.Value.Format() : "0";
                   // PdfPCell pdfTabSubTotals = new PdfPCell(new Phrase(total.AgencyName + " : " + "CC Exp. " + ccExpValue + " YTD CC Exp. " + ccYtdExpValue));
                    //pdfTabSubTotals.HorizontalAlignment = 1;
                   // pdfTabSubTotals.Colspan = 6;
                    //pdfTabAppTotals.AddCell(pdfTabSubTotals);
                }

                var statusAudit = GetStatusAuditDetails(id);
                try
                {
                    statusAudit.LoadData();
                }
                catch (Exception ex)
                {
                    _log.Error(null, ex);
                }

                PdfPTable pdfTabStatusHistory = new PdfPTable(4);
                pdfTabStatusHistory.HorizontalAlignment = 1;
                pdfTabStatusHistory.SpacingBefore = 5f;
                pdfTabStatusHistory.SpacingAfter = 5f;
                pdfTabStatusHistory.WidthPercentage = widthPercent;
                PdfPCell headerCell = new PdfPCell(new Phrase("Report Status History"));
                headerCell.Colspan = 4;
                headerCell.Border = 0;
                headerCell.HorizontalAlignment = 1;
                pdfTabStatusHistory.AddCell(headerCell);
                pdfTabStatusHistory.AddCell("Date Of Status Change");
                pdfTabStatusHistory.AddCell("Old Status");
                pdfTabStatusHistory.AddCell("New Status");
                pdfTabStatusHistory.AddCell("User Name");

                foreach (var item in statusAudit.Rows)
                {
                    pdfTabStatusHistory.AddCell(item.StatusChangeDate.ToString());
                    pdfTabStatusHistory.AddCell(item.OldStatus.DisplayName());
                    pdfTabStatusHistory.AddCell(item.NewStatus.DisplayName());
                    pdfTabStatusHistory.AddCell(item.UserName);
                }

                var detailsModel = GetSubreports(id).OrderBy(f => f.ServiceName);
                PdfPTable pdfTabDetails = new PdfPTable(11);
                pdfTabDetails.HorizontalAlignment = 1;
                pdfTabDetails.SpacingBefore = 5f;
                pdfTabDetails.SpacingAfter = 5f;
                pdfTabDetails.WidthPercentage = widthPercent;
                PdfPCell headerCellDetails = new PdfPCell(new Phrase("Report Details"));
                headerCellDetails.Colspan = 11;
                headerCellDetails.Border = 0;
                headerCellDetails.HorizontalAlignment = 1;
                pdfTabDetails.AddCell(headerCellDetails);
                pdfTabDetails.AddCell("Agency");
                pdfTabDetails.AddCell("Service Type");
                pdfTabDetails.AddCell("Service");
                pdfTabDetails.AddCell("Budget Remarks");
                pdfTabDetails.AddCell("CUR");
                pdfTabDetails.AddCell("Budget");
                pdfTabDetails.AddCell("Budget Match");
                pdfTabDetails.AddCell("CC Exp.");
                pdfTabDetails.AddCell("Match Exp.");
                pdfTabDetails.AddCell("YTD CC Exp.");
                pdfTabDetails.AddCell("YTD Match Exp.");
                var subReportsTables = new Dictionary<PdfPTable, bool>();
                var clientEventsCountSubReportTables = new Dictionary<PdfPTable, bool>();

                PdfPTable pdfTabYtdSummary = new PdfPTable(6);
                pdfTabYtdSummary.HorizontalAlignment = 1;
                pdfTabYtdSummary.SpacingBefore = 5f;
                pdfTabYtdSummary.SpacingAfter = 5f;
                pdfTabYtdSummary.WidthPercentage = widthPercent;
                PdfPCell headerCellYtdSummary = new PdfPCell(new Phrase("Report YTD Summary"));
                headerCellYtdSummary.Colspan = 6;
                headerCellYtdSummary.Border = 0;
                headerCellYtdSummary.HorizontalAlignment = 1;
                pdfTabYtdSummary.AddCell(headerCellYtdSummary);
                pdfTabYtdSummary.AddCell("Service Type");
                pdfTabYtdSummary.AddCell("Service");
                pdfTabYtdSummary.AddCell("Budget Amount");
                pdfTabYtdSummary.AddCell("YTD CC Exp.");
                pdfTabYtdSummary.AddCell("Exp. as % of the budget");
                pdfTabYtdSummary.AddCell("Unique Clients Count YTD");

                decimal? totalYtdCcExp = null;

                foreach (var item in detailsModel)
                {
                    pdfTabDetails.AddCell(item.AgencyName ?? string.Empty);
                    pdfTabDetails.AddCell(item.ServiceTypeName ?? string.Empty);
                    pdfTabDetails.AddCell(item.ServiceName ?? string.Empty);
                    pdfTabDetails.AddCell(item.AppBudgetServiceRemarks ?? string.Empty);
                    pdfTabDetails.AddCell(item.Cur ?? string.Empty);
                    pdfTabDetails.AddCell(item.CcGrant.Format());
                    var requiredMatch = item.RequiredMatch.HasValue ? item.RequiredMatch.Value.Format() : "N/A";
                    pdfTabDetails.AddCell(requiredMatch);
                    var ccexp = item.CcExp.HasValue ? item.CcExp.Value.Format() : "N/A";
                    pdfTabDetails.AddCell(ccexp);
                    var matchexp = item.MatchExp.HasValue ? item.MatchExp.Value.Format() : "N/A";
                    pdfTabDetails.AddCell(matchexp);
                    var ytdccexp = item.YtdCcExp.HasValue ? item.YtdCcExp.Value.Format() : "N/A";
                    pdfTabDetails.AddCell(ytdccexp);
                    var ytdmatchexp = item.YtdMatchExp.HasValue ? item.YtdMatchExp.Value.Format() : "N/A";
                    pdfTabDetails.AddCell(ytdmatchexp);

                    var subRepId = item.Id != null ? (int)item.Id : 0;
                    var subreport = db.SubReports
                                    .Include(f => f.MainReport)
                                    .Where(Permissions.SubReportsFilter).SingleOrDefault(f => f.Id == subRepId);

                    pdfTabYtdSummary.AddCell(item.ServiceTypeName ?? string.Empty);
                    pdfTabYtdSummary.AddCell(item.ServiceName ?? string.Empty);
                    pdfTabYtdSummary.AddCell(item.CcGrant.Format());
                    pdfTabYtdSummary.AddCell(ytdccexp);
                    var expperc = item.YtdCcExp.HasValue && item.CcGrant.Value > 0 ? (item.YtdCcExp.Value / item.CcGrant.Value).FormatPercentage() : "N/A";
                    pdfTabYtdSummary.AddCell(expperc);

                    if (item.YtdCcExp.HasValue)
                    {
                        if (!totalYtdCcExp.HasValue)
                        {
                            totalYtdCcExp = item.YtdCcExp;
                        }
                        else
                        {
                            totalYtdCcExp += item.YtdCcExp;
                        }
                    }
                    string unqClients = "";

                    if (subreport != null)
                    {
                        db.CommandTimeout = 10000;
                        var repo = new GenericRepository<SubReport>(db);
                        var model = SubReportDetailsModel.LoadData(repo.GetAll(this.Permissions.SubReportsFilter), db.MainReports, db.ClientReports, db.AppBudgetServices, db.SKMembersVisits, this.Permissions, (int)item.Id);
                        unqClients = model.Totals.TotalClients.ToString();

                        model.Filter.ReportedOnly = true;

                        SubReportDetailsJqDt jqmodel = new SubReportDetailsJqDt();
                        jqmodel.Id = (int)model.Id;
                        jqmodel.AgencyId = model.AgencyId;
                        jqmodel.AppBudgetServiceId = (int)model.AppBudgetServiceId;
                        jqmodel.Filter = model.Filter;
                        jqmodel.iDisplayLength = int.MaxValue;
                        jqmodel.sSortDir_0 = "asc";

                        var src = new SubReportsController();
                        src.Permissions = this.Permissions;
                        JsonResult result = null;
                        try
                        {
                            result = src.GetRows(jqmodel);
                        }
                        catch (NotImplementedException)
                        {
                            //total cost no names
                        }
                        catch (Exception exc)
                        {
                            throw new Exception(exc.InnerException != null ? exc.InnerException.Message : exc.Message, exc);
                        }
                        List<object> clients = new List<object>();
                        ReportsRow reportRow = new ReportsRow();

                        int numOfDates = model.Dates.Count();
                        var subReportTotals = model.Totals;
                        var subReportHeader = model.DetailsHeader;
                        PdfPTable pdfSubReportHeader = new PdfPTable(subreport.AppBudgetService.Service.ReportingMethodId != (int)Service.ReportingMethods.ClientEventsCount ? 11 : 8);
                        pdfSubReportHeader.WidthPercentage = widthPercent;
                        pdfSubReportHeader.HorizontalAlignment = 1;
                        pdfSubReportHeader.AddCell("Fund");
                        pdfSubReportHeader.AddCell("App #");
                        pdfSubReportHeader.AddCell("Agency");
                        pdfSubReportHeader.AddCell("Ser");
                        pdfSubReportHeader.AddCell("Service");
                        pdfSubReportHeader.AddCell("Budget Remarks");
                        pdfSubReportHeader.AddCell("Financial Report Status");
                        pdfSubReportHeader.AddCell("Period");
                        if (subreport.AppBudgetService.Service.ReportingMethodId != (int)Service.ReportingMethods.ClientEventsCount)
                        {
                            pdfSubReportHeader.AddCell("Total Amount");
                            pdfSubReportHeader.AddCell("Matching Sum");
                            pdfSubReportHeader.AddCell("Agency Contribution");
                        }

                        pdfSubReportHeader.AddCell(subReportHeader.FundName);
                        pdfSubReportHeader.AddCell(subReportHeader.AppName);
                        pdfSubReportHeader.AddCell(subReportHeader.AgencyName);
                        pdfSubReportHeader.AddCell(subReportHeader.SerName);
                        pdfSubReportHeader.AddCell(subReportHeader.ServiceName);
                        pdfSubReportHeader.AddCell(subReportHeader.AppBudgetServiceCleanRemarks);
                        pdfSubReportHeader.AddCell(subReportHeader.MainReportStatus.DisplayName());
                        pdfSubReportHeader.AddCell(subReportHeader.Period.Start.ToShortDateString() + " - " + subReportHeader.Period.End.AddDays(-1).ToShortDateString());
                        if (subreport.AppBudgetService.Service.ReportingMethodId != (int)Service.ReportingMethods.ClientEventsCount)
                        {
                            if (subReportHeader.Id != default(int))
                            {
                                var amountTemp = subReportHeader.Amount.HasValue ? subReportHeader.Amount.Value.Format() : "0";
                                pdfSubReportHeader.AddCell(amountTemp);
                                amountTemp = subReportHeader.MatchingSum.HasValue ? subReportHeader.MatchingSum.Value.Format() : "0";
                                pdfSubReportHeader.AddCell(amountTemp);
                                amountTemp = subReportHeader.AgencyContribution.HasValue ? subReportHeader.AgencyContribution.Value.Format() : "0";
                                pdfSubReportHeader.AddCell(amountTemp);
                            }
                            else
                            {
                                pdfSubReportHeader.AddCell("N/A");
                                pdfSubReportHeader.AddCell("N/A");
                                pdfSubReportHeader.AddCell("N/A");
                            }
                        }
                        PdfPCell subReportHeaderCell = new PdfPCell(pdfSubReportHeader);
                        subReportHeaderCell.Padding = 0f;

                        var totalsColSpan = subreport.AppBudgetService.Service.ReportingMethodEnum == Service.ReportingMethods.SoupKitchens ? 8 : 6;
                        PdfPTable pdfSubReportTotals = new PdfPTable(totalsColSpan);
                        pdfSubReportTotals.HorizontalAlignment = 1;
                        pdfSubReportTotals.WidthPercentage = widthPercent;
                        PdfPCell subReportTotalsHeader = new PdfPCell(new Phrase(subReportHeader.ServiceName + " - Totals"));
                        subReportTotalsHeader.Colspan = totalsColSpan;
                        subReportTotalsHeader.HorizontalAlignment = 1;
                        pdfSubReportTotals.AddCell(subReportTotalsHeader);
                        pdfSubReportTotals.AddCell("Total Amount");
                        pdfSubReportTotals.AddCell("Total Unique Clients");
                        pdfSubReportTotals.AddCell("App Matching Sum");
                        pdfSubReportTotals.AddCell("Total Matching Sum spent up to date");
                        pdfSubReportTotals.AddCell("Estimated Matching Sum Spent Required up to date (including current period)");
                        pdfSubReportTotals.AddCell("Matching Sum Status");
                        if (subreport.AppBudgetService.Service.ReportingMethodEnum == Service.ReportingMethods.SoupKitchens)
                        {
                            pdfSubReportTotals.AddCell("Total Meal Count");
                            pdfSubReportTotals.AddCell("Total Meal Count up to date");
                        }

                        pdfSubReportTotals.AddCell(subReportTotals.TotalAmountReported.Format());
                        pdfSubReportTotals.AddCell(subReportTotals.TotalClients.ToString());
                        var amountStr = subReportTotals.AppMatchingSum.HasValue ? subReportTotals.AppMatchingSum.Value.Format() : "0";
                        pdfSubReportTotals.AddCell(amountStr);
                        amountStr = subReportTotals.SubReportMatchingSum.HasValue ? subReportTotals.SubReportMatchingSum.Value.Format() : "0";
                        pdfSubReportTotals.AddCell(amountStr);
                        amountStr = subReportTotals.EstimatedMatchingSum.HasValue ? subReportTotals.EstimatedMatchingSum.Value.Format() : "0";
                        pdfSubReportTotals.AddCell(amountStr);
                        pdfSubReportTotals.AddCell(subReportTotals.MatchingSumStatus);
                        if (subreport.AppBudgetService.Service.ReportingMethodEnum == Service.ReportingMethods.SoupKitchens)
                        {
                            pdfSubReportTotals.AddCell(subReportTotals.TotalVisitCount.ToString());
                            pdfSubReportTotals.AddCell(subReportTotals.TotalYTDVisitCount.ToString());
                        }
                        PdfPCell subReportTotalsCell = new PdfPCell(pdfSubReportTotals);
                        subReportTotalsCell.Padding = 0f;

                        // colspan parameters
                        int emergencyColspan = 11;
                        int homecareColspan = 9 + numOfDates;
                        int scColspan = 10;
                        int dccColspan = 10;
                        int clientUnitAmountColspan = 8;
                        int clientNamesAndCosts = 8;
                        int mhmColspan = 7;
                        int restReportsColspan = 7;
                        int soupKitchensColspan = 7;
                        int clientEventsCountColspan = 4;

                        // for emergency report
                        PdfPTable pdfTabSubDetailsEmergency = new PdfPTable(emergencyColspan);
                        pdfTabSubDetailsEmergency.HorizontalAlignment = 1;
                        pdfTabSubDetailsEmergency.SpacingBefore = 5f;
                        pdfTabSubDetailsEmergency.SpacingAfter = 5f;
                        pdfTabSubDetailsEmergency.WidthPercentage = widthPercent;
                        PdfPCell headerCellSubDetails = new PdfPCell(new Phrase(subReportHeader.ServiceName));
                        headerCellSubDetails.Colspan = emergencyColspan;
                        headerCellSubDetails.Border = 0;
                        headerCellSubDetails.HorizontalAlignment = 1;
                        pdfTabSubDetailsEmergency.AddCell(headerCellSubDetails);
                        // for homecare report
                        PdfPTable pdfTabSubDetailsHomcare = new PdfPTable(homecareColspan);
                        pdfTabSubDetailsHomcare.HorizontalAlignment = 1;
                        pdfTabSubDetailsHomcare.SpacingBefore = 5f;
                        pdfTabSubDetailsHomcare.SpacingAfter = 5f;
                        pdfTabSubDetailsHomcare.WidthPercentage = widthPercent;
                        headerCellSubDetails.Colspan = homecareColspan;
                        headerCellSubDetails.Border = 0;
                        headerCellSubDetails.HorizontalAlignment = 1;
                        pdfTabSubDetailsHomcare.AddCell(headerCellSubDetails);
                        // for sc report
                        PdfPTable pdfTabSubDetailsSc = new PdfPTable(scColspan);
                        pdfTabSubDetailsSc.HorizontalAlignment = 1;
                        pdfTabSubDetailsSc.SpacingBefore = 5f;
                        pdfTabSubDetailsSc.SpacingAfter = 5f;
                        pdfTabSubDetailsSc.WidthPercentage = widthPercent;
                        headerCellSubDetails.Colspan = scColspan;
                        headerCellSubDetails.Border = 0;
                        headerCellSubDetails.HorizontalAlignment = 1;
                        pdfTabSubDetailsSc.AddCell(headerCellSubDetails);
                        // for dcc report
                        PdfPTable pdfTabSubDetailsDcc = new PdfPTable(dccColspan);
                        pdfTabSubDetailsDcc.HorizontalAlignment = 1;
                        pdfTabSubDetailsDcc.SpacingBefore = 5f;
                        pdfTabSubDetailsDcc.SpacingAfter = 5f;
                        pdfTabSubDetailsDcc.WidthPercentage = widthPercent;
                        headerCellSubDetails.Colspan = dccColspan;
                        headerCellSubDetails.Border = 0;
                        headerCellSubDetails.HorizontalAlignment = 1;
                        pdfTabSubDetailsDcc.AddCell(headerCellSubDetails);
                        // for clientunitamount report and sc report
                        PdfPTable pdfTabSubDetailsClientUnitAmount = new PdfPTable(clientUnitAmountColspan);
                        pdfTabSubDetailsClientUnitAmount.HorizontalAlignment = 1;
                        pdfTabSubDetailsClientUnitAmount.SpacingBefore = 5f;
                        pdfTabSubDetailsClientUnitAmount.SpacingAfter = 5f;
                        pdfTabSubDetailsClientUnitAmount.WidthPercentage = widthPercent;
                        headerCellSubDetails.Colspan = clientUnitAmountColspan;
                        headerCellSubDetails.Border = 0;
                        headerCellSubDetails.HorizontalAlignment = 1;
                        pdfTabSubDetailsClientUnitAmount.AddCell(headerCellSubDetails);
                        // for ClientNamesAndCosts report
                        PdfPTable pdfTabSubDetailsClientNamesAndCosts = new PdfPTable(clientNamesAndCosts);
                        pdfTabSubDetailsClientNamesAndCosts.HorizontalAlignment = 1;
                        pdfTabSubDetailsClientNamesAndCosts.SpacingBefore = 5f;
                        pdfTabSubDetailsClientNamesAndCosts.SpacingAfter = 5f;
                        pdfTabSubDetailsClientNamesAndCosts.WidthPercentage = widthPercent;
                        headerCellSubDetails.Colspan = clientNamesAndCosts;
                        headerCellSubDetails.Border = 0;
                        headerCellSubDetails.HorizontalAlignment = 1;
                        pdfTabSubDetailsClientNamesAndCosts.AddCell(headerCellSubDetails);
                        // for MHM report
                        PdfPTable pdfTabSubDetailsMhm = new PdfPTable(mhmColspan);
                        pdfTabSubDetailsMhm.HorizontalAlignment = 1;
                        pdfTabSubDetailsMhm.SpacingBefore = 5f;
                        pdfTabSubDetailsMhm.SpacingAfter = 5f;
                        pdfTabSubDetailsMhm.WidthPercentage = widthPercent;
                        headerCellSubDetails.Colspan = mhmColspan;
                        headerCellSubDetails.Border = 0;
                        headerCellSubDetails.HorizontalAlignment = 1;
                        pdfTabSubDetailsMhm.AddCell(headerCellSubDetails);
                        // for soup kitchens report
                        PdfPTable pdfTabSubDetailsSoupKitchens = new PdfPTable(soupKitchensColspan);
                        pdfTabSubDetailsSoupKitchens.HorizontalAlignment = 1;
                        pdfTabSubDetailsSoupKitchens.SpacingBefore = 5f;
                        pdfTabSubDetailsSoupKitchens.SpacingAfter = 5f;
                        pdfTabSubDetailsSoupKitchens.WidthPercentage = widthPercent;
                        headerCellSubDetails.Colspan = soupKitchensColspan;
                        headerCellSubDetails.Border = 0;
                        headerCellSubDetails.HorizontalAlignment = 1;
                        pdfTabSubDetailsSoupKitchens.AddCell(headerCellSubDetails);
                        // for rest reports
                        PdfPTable pdfTabSubDetailsRest = new PdfPTable(restReportsColspan);
                        pdfTabSubDetailsRest.HorizontalAlignment = 1;
                        pdfTabSubDetailsRest.SpacingBefore = 5f;
                        pdfTabSubDetailsRest.SpacingAfter = 5f;
                        pdfTabSubDetailsRest.WidthPercentage = widthPercent;
                        headerCellSubDetails.Colspan = restReportsColspan;
                        headerCellSubDetails.Border = 0;
                        headerCellSubDetails.HorizontalAlignment = 1;
                        pdfTabSubDetailsRest.AddCell(headerCellSubDetails);
                        //for client events count report
                        PdfPTable pdfTabSubDetailsClientEventsCount = new PdfPTable(clientEventsCountColspan);
                        pdfTabSubDetailsClientEventsCount.HorizontalAlignment = 1;
                        pdfTabSubDetailsClientEventsCount.SpacingBefore = 5f;
                        pdfTabSubDetailsClientEventsCount.SpacingAfter = 5f;
                        pdfTabSubDetailsClientEventsCount.WidthPercentage = widthPercent;
                        headerCellSubDetails.Colspan = clientEventsCountColspan;
                        headerCellSubDetails.Border = 0;
                        headerCellSubDetails.HorizontalAlignment = 1;
                        pdfTabSubDetailsClientEventsCount.AddCell(headerCellSubDetails);

                        PdfPCell globalHeader = new PdfPCell(new Phrase(" "));
                        globalHeader.HorizontalAlignment = 1;

                        if (result != null)
                        {
                            switch (subreport.AppBudgetService.Service.ReportingMethodEnum)
                            {
                                case Service.ReportingMethods.ClientNamesAndCosts:
                                    if (subreport.AppBudgetService.Service.TypeId != (int)Service.ServiceTypes.MinorHomeModifications && subreport.AppBudgetService.Service.TypeId != (int)Service.ServiceTypes.MedicalProgram)
                                    {
                                        subReportHeaderCell.Colspan = clientNamesAndCosts;
                                        pdfTabSubDetailsClientNamesAndCosts.AddCell(subReportHeaderCell);
                                        globalHeader.Colspan = clientNamesAndCosts;
                                        pdfTabSubDetailsClientNamesAndCosts.AddCell(globalHeader);
                                        pdfTabSubDetailsClientNamesAndCosts.AddCell("Id");
                                        pdfTabSubDetailsClientNamesAndCosts.AddCell("Client First Name");
                                        pdfTabSubDetailsClientNamesAndCosts.AddCell("Client Last Name");
                                        pdfTabSubDetailsClientNamesAndCosts.AddCell("Client JNV Status");
                                        pdfTabSubDetailsClientNamesAndCosts.AddCell("CC ID");
                                        pdfTabSubDetailsClientNamesAndCosts.AddCell("Allowed Hours/week");
                                        pdfTabSubDetailsClientNamesAndCosts.AddCell("Amount");
                                        pdfTabSubDetailsClientNamesAndCosts.AddCell("Unique Circumstances");
                                    }
                                    else
                                    {
                                        subReportHeaderCell.Colspan = mhmColspan;
                                        pdfTabSubDetailsMhm.AddCell(subReportHeaderCell);
                                        globalHeader.Colspan = mhmColspan;
                                        pdfTabSubDetailsMhm.AddCell(globalHeader);
                                        pdfTabSubDetailsMhm.AddCell("Id");
                                        pdfTabSubDetailsMhm.AddCell("Client First Name");
                                        pdfTabSubDetailsMhm.AddCell("Client Last Name");
                                        pdfTabSubDetailsMhm.AddCell("Client JNV Status");
                                        pdfTabSubDetailsMhm.AddCell("CC ID");
                                        pdfTabSubDetailsMhm.AddCell("Amount");
                                        pdfTabSubDetailsMhm.AddCell("Unique Circumstances");
                                    }
                                    break;
                                case Service.ReportingMethods.TotalCostWithListOfClientNames:
                                    subReportHeaderCell.Colspan = restReportsColspan;
                                    pdfTabSubDetailsRest.AddCell(subReportHeaderCell);
                                    globalHeader.Colspan = restReportsColspan;
                                    pdfTabSubDetailsRest.AddCell(globalHeader);
                                    pdfTabSubDetailsRest.AddCell("Id");
                                    pdfTabSubDetailsRest.AddCell("Client First Name");
                                    pdfTabSubDetailsRest.AddCell("Client Last Name");
                                    pdfTabSubDetailsRest.AddCell("Client JNV Status");
                                    pdfTabSubDetailsRest.AddCell("CC ID");
                                    pdfTabSubDetailsRest.AddCell("Reported");
                                    pdfTabSubDetailsRest.AddCell("Unique Circumstances");
                                    break;
                                case Service.ReportingMethods.ClientUnit:
                                    subReportHeaderCell.Colspan = restReportsColspan;
                                    pdfTabSubDetailsRest.AddCell(subReportHeaderCell);
                                    globalHeader.Colspan = restReportsColspan;
                                    pdfTabSubDetailsRest.AddCell(globalHeader);
                                    pdfTabSubDetailsRest.AddCell("Id");
                                    pdfTabSubDetailsRest.AddCell("Client First Name");
                                    pdfTabSubDetailsRest.AddCell("Client Last Name");
                                    pdfTabSubDetailsRest.AddCell("Client JNV Status");
                                    pdfTabSubDetailsRest.AddCell("CC ID");
                                    pdfTabSubDetailsRest.AddCell("Quantity");
                                    pdfTabSubDetailsRest.AddCell("Unique Circumstances");
                                    break;
                                case Service.ReportingMethods.ClientUnitAmount:
                                    subReportHeaderCell.Colspan = clientUnitAmountColspan;
                                    pdfTabSubDetailsClientUnitAmount.AddCell(subReportHeaderCell);
                                    globalHeader.Colspan = clientUnitAmountColspan;
                                    pdfTabSubDetailsClientUnitAmount.AddCell(globalHeader);
                                    pdfTabSubDetailsClientUnitAmount.AddCell("Id");
                                    pdfTabSubDetailsClientUnitAmount.AddCell("Client First Name");
                                    pdfTabSubDetailsClientUnitAmount.AddCell("Client Last Name");
                                    pdfTabSubDetailsClientUnitAmount.AddCell("Client JNV Status");
                                    pdfTabSubDetailsClientUnitAmount.AddCell("CC ID");
                                    pdfTabSubDetailsClientUnitAmount.AddCell("Amount");
                                    pdfTabSubDetailsClientUnitAmount.AddCell("Quantity");
                                    pdfTabSubDetailsClientUnitAmount.AddCell("Unique Circumstances");
                                    break;
                                case Service.ReportingMethods.Emergency:
                                    subReportHeaderCell.Colspan = emergencyColspan;
                                    pdfTabSubDetailsEmergency.AddCell(subReportHeaderCell);
                                    globalHeader.Colspan = emergencyColspan;
                                    pdfTabSubDetailsEmergency.AddCell(globalHeader);
                                    pdfTabSubDetailsEmergency.AddCell("Client First Name");
                                    pdfTabSubDetailsEmergency.AddCell("Client Last Name");
                                    pdfTabSubDetailsEmergency.AddCell("Client JNV Status");
                                    pdfTabSubDetailsEmergency.AddCell("CC ID");
                                    pdfTabSubDetailsEmergency.AddCell("Date");
                                    pdfTabSubDetailsEmergency.AddCell("Type");
                                    pdfTabSubDetailsEmergency.AddCell("Purpose of grant");
                                    pdfTabSubDetailsEmergency.AddCell("Amount");
                                    pdfTabSubDetailsEmergency.AddCell("Discretionary");
                                    pdfTabSubDetailsEmergency.AddCell("Total");
                                    pdfTabSubDetailsEmergency.AddCell("Unique Circumstances");
                                    break;
                                case Service.ReportingMethods.Homecare:
                                    subReportHeaderCell.Colspan = homecareColspan;
                                    pdfTabSubDetailsHomcare.AddCell(subReportHeaderCell);
                                    globalHeader.Colspan = homecareColspan;
                                    pdfTabSubDetailsHomcare.AddCell(globalHeader);
                                    pdfTabSubDetailsHomcare.AddCell("First Name");
                                    pdfTabSubDetailsHomcare.AddCell("Last Name");
                                    pdfTabSubDetailsHomcare.AddCell("Client JNV Status");
                                    pdfTabSubDetailsHomcare.AddCell("CC ID");
                                    pdfTabSubDetailsHomcare.AddCell("Rate");
                                    pdfTabSubDetailsHomcare.AddCell("Cur");
                                    for (int i = 0; i < numOfDates; i++)
                                    {
                                        pdfTabSubDetailsHomcare.AddCell(string.Format("{0:MMM-yyyy}", model.Dates[i]));
                                    }
                                    pdfTabSubDetailsHomcare.AddCell("Unique Circumstances");
                                    pdfTabSubDetailsHomcare.AddCell("Total Hours");
                                    pdfTabSubDetailsHomcare.AddCell("Allowed Hours/week");
                                    break;
                                case Service.ReportingMethods.HomecareWeekly:
                                    subReportHeaderCell.Colspan = homecareColspan;
                                    pdfTabSubDetailsHomcare.AddCell(subReportHeaderCell);
                                    globalHeader.Colspan = homecareColspan;
                                    pdfTabSubDetailsHomcare.AddCell(globalHeader);
                                    pdfTabSubDetailsHomcare.AddCell("First Name");
                                    pdfTabSubDetailsHomcare.AddCell("Last Name");
                                    pdfTabSubDetailsHomcare.AddCell("HAS");
                                    pdfTabSubDetailsHomcare.AddCell("CC ID");
                                    pdfTabSubDetailsHomcare.AddCell("Rate");
                                    pdfTabSubDetailsHomcare.AddCell("Cur");
                                    DayOfWeek selectedDOW = subreport.MainReport.Start.DayOfWeek;
                                    int? selectedDowDb = GlobalHelper.GetWeekStartDay(model.AgencyGroupId, model.AppId);
                                    if (selectedDowDb.HasValue)
                                    {
                                        selectedDOW = (DayOfWeek)selectedDowDb.Value;
                                    }
                                    DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
                                    dfi.FirstDayOfWeek = selectedDOW;
                                    Calendar cal = dfi.Calendar;
                                    for (int i = 0; i < numOfDates; i++)
                                    {
                                        pdfTabSubDetailsHomcare.AddCell(string.Format("W{0}({1:dd-MMM-yyyy})", cal.GetWeekOfYear(model.Dates[i], dfi.CalendarWeekRule, dfi.FirstDayOfWeek), model.Dates[i]));
                                    }
                                    pdfTabSubDetailsHomcare.AddCell("Unique Circumstances");
                                    pdfTabSubDetailsHomcare.AddCell("Total Hours");
                                    if (subreport.AppBudgetService.Service.ExceptionalHomeCareHours && subreport.AppBudgetService.Service.CoPGovHoursValidation)
                                    {
                                        pdfTabSubDetailsHomcare.AddCell("Allowed Govt Hours/week");
                                    }
                                    else
                                    {
                                        pdfTabSubDetailsHomcare.AddCell("Allowed Hours/week");
                                    }
                                    break;
                                case Service.ReportingMethods.SupportiveCommunities:
                                    subReportHeaderCell.Colspan = scColspan;
                                    pdfTabSubDetailsSc.AddCell(subReportHeaderCell);
                                    globalHeader.Colspan = scColspan;
                                    pdfTabSubDetailsSc.AddCell(globalHeader);
                                    pdfTabSubDetailsSc.AddCell("CC ID");
                                    pdfTabSubDetailsSc.AddCell("Israel ID");
                                    pdfTabSubDetailsSc.AddCell("Client First Name");
                                    pdfTabSubDetailsSc.AddCell("Client Last Name");
                                    pdfTabSubDetailsSc.AddCell("JNV Status");
                                    pdfTabSubDetailsSc.AddCell("Join Date");
                                    pdfTabSubDetailsSc.AddCell("CC Subsidy");
                                    pdfTabSubDetailsSc.AddCell("Months reported");
                                    pdfTabSubDetailsSc.AddCell("Total paid by CC");
                                    pdfTabSubDetailsSc.AddCell("Reported");
                                    break;
                                case Service.ReportingMethods.DayCenters:
                                    subReportHeaderCell.Colspan = dccColspan;
                                    pdfTabSubDetailsDcc.AddCell(subReportHeaderCell);
                                    globalHeader.Colspan = dccColspan;
                                    pdfTabSubDetailsDcc.AddCell(globalHeader);
                                    PdfPCell summaryCell = new PdfPCell(new Phrase("Day Centers - Summary"));
                                    summaryCell.Colspan = dccColspan;
                                    summaryCell.HorizontalAlignment = 1;
                                    pdfTabSubDetailsDcc.AddCell(summaryCell);
                                    pdfTabSubDetailsDcc.AddCell("CC ID");
                                    pdfTabSubDetailsDcc.AddCell("Israel ID");
                                    pdfTabSubDetailsDcc.AddCell("Client First Name");
                                    pdfTabSubDetailsDcc.AddCell("Client Last Name");
                                    pdfTabSubDetailsDcc.AddCell("JNV Status");
                                    pdfTabSubDetailsDcc.AddCell("Join Date");
                                    pdfTabSubDetailsDcc.AddCell("Subsides by CC");
                                    pdfTabSubDetailsDcc.AddCell("Visit Cost");
                                    pdfTabSubDetailsDcc.AddCell("Visits Count");
                                    pdfTabSubDetailsDcc.AddCell("Amount");
                                    break;
                                case Service.ReportingMethods.SoupKitchens:
                                    subReportHeaderCell.Colspan = soupKitchensColspan;
                                    pdfTabSubDetailsSoupKitchens.AddCell(subReportHeaderCell);
                                    globalHeader.Colspan = soupKitchensColspan;
                                    pdfTabSubDetailsSoupKitchens.AddCell(globalHeader);
                                    PdfPCell summarySKCell = new PdfPCell(new Phrase("Soup Kitchens - Summary"));
                                    summarySKCell.Colspan = soupKitchensColspan;
                                    summarySKCell.HorizontalAlignment = 1;
                                    pdfTabSubDetailsSoupKitchens.AddCell(summarySKCell);
                                    pdfTabSubDetailsSoupKitchens.AddCell("CC ID");
                                    pdfTabSubDetailsSoupKitchens.AddCell("Israel ID");
                                    pdfTabSubDetailsSoupKitchens.AddCell("Client First Name");
                                    pdfTabSubDetailsSoupKitchens.AddCell("Client Last Name");
                                    pdfTabSubDetailsSoupKitchens.AddCell("JNV Status");
                                    pdfTabSubDetailsSoupKitchens.AddCell("Join Date");
                                    pdfTabSubDetailsSoupKitchens.AddCell("Meal Count");
                                    break;
                                case Service.ReportingMethods.ClientEventsCount:
                                    subReportHeaderCell.Colspan = clientEventsCountColspan;
                                    pdfTabSubDetailsClientEventsCount.AddCell(subReportHeaderCell);
                                    globalHeader.Colspan = clientEventsCountColspan;
                                    pdfTabSubDetailsClientEventsCount.AddCell(globalHeader);
                                    pdfTabSubDetailsClientEventsCount.AddCell("Date of Event");
                                    pdfTabSubDetailsClientEventsCount.AddCell("Count of JNV attending");
                                    pdfTabSubDetailsClientEventsCount.AddCell("Count of Total Attendees");
                                    pdfTabSubDetailsClientEventsCount.AddCell("Remarks");
                                    break;
                                default:
                                    continue;
                            }

                            switch (subreport.AppBudgetService.Service.ReportingMethodEnum)
                            {
                                case Service.ReportingMethods.ClientNamesAndCosts:
                                    var dataClientNamesAndCosts = (SubReportDetailsJqDt)result.Data;
                                    if (subreport.AppBudgetService.Service.TypeId != (int)Service.ServiceTypes.MinorHomeModifications && subreport.AppBudgetService.Service.TypeId != (int)Service.ServiceTypes.MedicalProgram)
                                    {
                                        foreach (var d in dataClientNamesAndCosts.aaData)
                                        {
                                            reportRow = new ReportsRow();
                                            var datalist = d.ToList();
                                            reportRow.Id = (int?)datalist[0];
                                            reportRow.ClientFirstName = (string)datalist[1];
                                            reportRow.ClientLastName = (string)datalist[2];
                                            reportRow.ClientApprovalStatus = (string)datalist[3];
                                            reportRow.ClientId = (int)datalist[4];
                                            reportRow.HcCaps = (decimal?)datalist[5];
                                            reportRow.Amount = (decimal?)datalist[6];
                                            reportRow.Remarks = (string)datalist[7];
                                            clients.Add((object)reportRow);
                                        }
                                    }
                                    else
                                    {
                                        foreach (var d in dataClientNamesAndCosts.aaData)
                                        {
                                            reportRow = new ReportsRow();
                                            var datalist = d.ToList();
                                            reportRow.Id = (int?)datalist[0];
                                            reportRow.ClientFirstName = (string)datalist[1];
                                            reportRow.ClientLastName = (string)datalist[2];
                                            reportRow.ClientApprovalStatus = (string)datalist[3];
                                            reportRow.ClientId = (int)datalist[4];
                                            reportRow.Amount = (decimal?)datalist[5];
                                            reportRow.Remarks = (string)datalist[6];
                                            clients.Add((object)reportRow);
                                        }
                                    }
                                    break;
                                case Service.ReportingMethods.TotalCostWithListOfClientNames:
                                    var dataTotalCost = (SubReportDetailsJqDt)result.Data;
                                    if (model.ServiceName == "Funeral Expenses")
                                    {
                                        foreach (var d in dataTotalCost.aaData)
                                        {
                                            reportRow = new ReportsRow();
                                            var datalist = d.ToList();
                                            reportRow.Id = (int?)datalist[0];
                                            reportRow.ClientFirstName = (string)datalist[1];
                                            reportRow.ClientLastName = (string)datalist[2];
                                            reportRow.ClientApprovalStatus = (string)datalist[3];
                                            reportRow.ClientId = (int)datalist[4];
                                            reportRow.Reported = (bool)datalist[5];
                                            reportRow.Amount = (decimal?)datalist[6];
                                            reportRow.Remarks = (string)datalist[7];
                                            clients.Add((object)reportRow);
                                        } }
                                    else
                                    {
                                        foreach (var d in dataTotalCost.aaData)
                                        {
                                            reportRow = new ReportsRow();
                                            var datalist = d.ToList();
                                            reportRow.Id = (int?)datalist[0];
                                            reportRow.ClientFirstName = (string)datalist[1];
                                            reportRow.ClientLastName = (string)datalist[2];
                                            reportRow.ClientApprovalStatus = (string)datalist[3];
                                            reportRow.ClientId = (int)datalist[4];
                                            reportRow.Reported = (bool)datalist[5];
                                            reportRow.Remarks = (string)datalist[6];
                                            clients.Add((object)reportRow);
                                        }



                                    }
                                    break;
                                case Service.ReportingMethods.ClientUnit:
                                    var dataClientUnit = (SubReportDetailsJqDt)result.Data;
                                    foreach (var d in dataClientUnit.aaData)
                                    {
                                        reportRow = new ReportsRow();
                                        var datalist = d.ToList();
                                        reportRow.Id = (int?)datalist[0];
                                        reportRow.ClientFirstName = (string)datalist[1];
                                        reportRow.ClientLastName = (string)datalist[2];
                                        reportRow.ClientApprovalStatus = (string)datalist[3];
                                        reportRow.ClientId = (int)datalist[4];
                                        reportRow.Quantity = (decimal?)datalist[5];
                                        reportRow.Remarks = (string)datalist[6];
                                        clients.Add((object)reportRow);
                                    }
                                    break;
                                case Service.ReportingMethods.ClientUnitAmount:
                                    var dataClientUnitAmount = (SubReportDetailsJqDt)result.Data;
                                    if (model.ServiceName == "COVID Basket of Services")
                                    {
                                        foreach (var d in dataClientUnitAmount.aaData)
                                        {
                                            reportRow = new ReportsRow();
                                            var datalist = d.ToList();
                                            reportRow.Id = (int?)datalist[0];
                                            reportRow.ClientFirstName = (string)datalist[1];
                                            reportRow.ClientLastName = (string)datalist[2];
                                            reportRow.ClientApprovalStatus = (string)datalist[3];
                                            reportRow.ClientId = (int)datalist[4];
                                            reportRow.Amount = (decimal?)datalist[6];
                                            reportRow.Quantity = (decimal?)datalist[7];
                                            reportRow.Remarks = (string)datalist[8];
                                            clients.Add((object)reportRow);
                                        }

                                    }
                                    else
                                    {
                                        foreach (var d in dataClientUnitAmount.aaData)
                                        {
                                            reportRow = new ReportsRow();
                                            var datalist = d.ToList();
                                            reportRow.Id = (int?)datalist[0];
                                            reportRow.ClientFirstName = (string)datalist[1];
                                            reportRow.ClientLastName = (string)datalist[2];
                                            reportRow.ClientApprovalStatus = (string)datalist[3];
                                            reportRow.ClientId = (int)datalist[4];
                                            reportRow.Amount = (decimal?)datalist[5];
                                            reportRow.Quantity = (decimal?)datalist[6];
                                            reportRow.Remarks = (string)datalist[7];
                                            clients.Add((object)reportRow);
                                        }
                                    }
                                    break;
                                case Service.ReportingMethods.Emergency:
                                    var dataEmergency = (SubReportDetailsJqDt)result.Data;
                                    foreach (var d in dataEmergency.aaData)
                                    {
                                        reportRow = new ReportsRow();
                                        var datalist = d.ToList();
                                        reportRow.ClientFirstName = (string)datalist[2];
                                        reportRow.ClientLastName = (string)datalist[3];
                                        reportRow.ClientApprovalStatus = (string)datalist[4];
                                        reportRow.ClientId = (int)datalist[5];
                                        reportRow.ReportDate = (DateTime?)datalist[6];
                                        reportRow.TypeName = (string)datalist[7];
                                        reportRow.Remarks = (string)datalist[8];
                                        reportRow.Amount = (decimal?)datalist[9];
                                        reportRow.Discretionary = (decimal?)datalist[10];
                                        reportRow.Total = (decimal?)datalist[11];
                                        reportRow.UniqueCircumstances = (string)datalist[12];
                                        clients.Add((object)reportRow);
                                    }
                                    break;
                                case Service.ReportingMethods.Homecare:
                                case Service.ReportingMethods.HomecareWeekly:
                                case Service.ReportingMethods.ClientEventsCount:
                                    var dataHomCare = (jQueryDataTableResult<object>)result.Data;
                                    clients = dataHomCare.aaData.ToList();
                                    break;
                                case Service.ReportingMethods.SupportiveCommunities:
                                    var dataSupportiveCommunities = (SubReportDetailsJqDt)result.Data;
                                    foreach (var d in dataSupportiveCommunities.aaData)
                                    {
                                        reportRow = new ReportsRow();
                                        var datalist = d.ToList();
                                        reportRow.Id = (int?)datalist[0];
                                        reportRow.ClientId = (int)datalist[1];
                                        reportRow.IsraelId = (string)datalist[2];
                                        reportRow.ClientFirstName = (string)datalist[3];
                                        reportRow.ClientLastName = (string)datalist[4];
                                        reportRow.ClientApprovalStatus = (string)datalist[5];
                                        reportRow.JoinDate = (DateTime)datalist[6];
                                        reportRow.HoursHoldCost = (decimal?)datalist[7];
                                        reportRow.MonthsCount = (int?)datalist[8];
                                        reportRow.Amount = (decimal?)datalist[9];
                                        reportRow.Reported = (bool)datalist[10];
                                        clients.Add((object)reportRow);
                                    }
                                    break;
                                case Service.ReportingMethods.DayCenters:
                                    var dataDayCenters = (SubReportDetailsJqDt)result.Data;
                                    foreach (var d in dataDayCenters.aaData)
                                    {
                                        reportRow = new ReportsRow();
                                        var datalist = d.ToList();
                                        reportRow.Id = (int?)datalist[0];
                                        reportRow.ClientId = (int)datalist[1];
                                        reportRow.IsraelId = (string)datalist[2];
                                        reportRow.ClientFirstName = (string)datalist[3];
                                        reportRow.ClientLastName = (string)datalist[4];
                                        reportRow.ClientApprovalStatus = (string)datalist[5];
                                        reportRow.JoinDate = (DateTime)datalist[6];
                                        reportRow.SubsidesByDcc = (int?)datalist[7];
                                        reportRow.VisitCost = (decimal?)datalist[8];
                                        reportRow.VisitCount = (int?)datalist[9];
                                        reportRow.Amount = (decimal?)datalist[10];
                                        clients.Add((object)reportRow);
                                    }
                                    break;
                                case Service.ReportingMethods.SoupKitchens:
                                    var dataSoupKitchens = (SubReportDetailsJqDt)result.Data;
                                    foreach (var d in dataSoupKitchens.aaData)
                                    {
                                        reportRow = new ReportsRow();
                                        var datalist = d.ToList();
                                        reportRow.ClientId = (int)datalist[1];
                                        reportRow.IsraelId = (string)datalist[2];
                                        reportRow.ClientFirstName = (string)datalist[3];
                                        reportRow.ClientLastName = (string)datalist[4];
                                        reportRow.ClientApprovalStatus = (string)datalist[5];
                                        reportRow.JoinDate = (DateTime)datalist[6];
                                        reportRow.VisitCount = (int?)datalist[7];
                                        clients.Add((object)reportRow);
                                    }
                                    break;
                            }
                        }
                        foreach (var c in clients)
                        {
                            var props = c.GetType().GetProperties();
                            string firstName = "";
                            string lastName = "";
                            string approvalStatus = "";
                            int clientId = 0;
                            decimal? rate = null;
                            string cur = "";
                            List<decimal> quantity = new List<decimal>();
                            decimal? Q1;
                            decimal? Q2;
                            decimal? Q3;
                            decimal? W1;
                            decimal? W2;
                            decimal? W3;
                            decimal? W4;
                            decimal? W5;
                            decimal? W6;
                            decimal? W7;
                            decimal? W8;
                            decimal? W9;
                            decimal? W10;
                            decimal? W11;
                            decimal? W12;
                            decimal? W13;
                            decimal? W14;
                            decimal? W15;
                            string remarks = "";
                            decimal totalQuantity = 0;
                            IEnumerable<object> hccaps = null;
                            DateTime? reportDate = null;
                            decimal? amount = null;
                            string type = "";
                            decimal? discretionary = null;
                            decimal? total = null;
                            string uniqueCirc = "";
                            decimal? quantityNum = null;
                            int? rowId = null;
                            bool? reported = null;
                            string temp = "";
                            string israelId = "";
                            decimal? hoursHoldCost = null;
                            int? monthsCount = null;
                            int? subsidesByDcc = null;
                            decimal? visitCost = null;
                            int? visitCount = null;
                            decimal? hcCapsAllowed = null;
                            DateTime? joinDate = null;
                            int JNVCount = 0;
                            int? TotalCount = null;
                            switch (subreport.AppBudgetService.Service.ReportingMethodEnum)
                            {
                                case Service.ReportingMethods.ClientNamesAndCosts:
                                    if (subreport.AppBudgetService.Service.TypeId != (int)Service.ServiceTypes.MinorHomeModifications && subreport.AppBudgetService.Service.TypeId != (int)Service.ServiceTypes.MedicalProgram)
                                    {
                                        for (int i = 0; i < props.Count(); i++)
                                        {
                                            switch (props[i].Name)
                                            {
                                                case "ClientFirstName":
                                                    firstName = (string)props[i].GetValue(c, null);
                                                    break;
                                                case "ClientLastName":
                                                    lastName = (string)props[i].GetValue(c, null);
                                                    break;
                                                case "ClientApprovalStatus":
                                                    approvalStatus = (string)props[i].GetValue(c, null);
                                                    break;
                                                case "ClientId":
                                                    clientId = (int)props[i].GetValue(c, null);
                                                    break;
                                                case "Id":
                                                    rowId = (int?)props[i].GetValue(c, null);
                                                    break;
                                                case "HcCaps":
                                                    hcCapsAllowed = (decimal?)props[i].GetValue(c, null);
                                                    break;
                                                case "Amount":
                                                    amount = (decimal?)props[i].GetValue(c, null);
                                                    break;
                                                case "Remarks":
                                                    remarks = (string)props[i].GetValue(c, null);
                                                    break;
                                            }
                                        }
                                        temp = rowId != null ? ((int)rowId).ToString() : "";
                                        pdfTabSubDetailsClientNamesAndCosts.AddCell(temp);
                                        pdfTabSubDetailsClientNamesAndCosts.AddCell(firstName);
                                        pdfTabSubDetailsClientNamesAndCosts.AddCell(lastName);
                                        pdfTabSubDetailsClientNamesAndCosts.AddCell(approvalStatus);
                                        pdfTabSubDetailsClientNamesAndCosts.AddCell(clientId.ToString());
                                        temp = hcCapsAllowed.HasValue ? hcCapsAllowed.Value.Format() : "0";
                                        pdfTabSubDetailsClientNamesAndCosts.AddCell(temp);
                                        temp = amount.HasValue ? amount.Value.Format() : "0";
                                        pdfTabSubDetailsClientNamesAndCosts.AddCell(temp);
                                        pdfTabSubDetailsClientNamesAndCosts.AddCell(remarks);
                                    }
                                    else
                                    {
                                        for (int i = 0; i < props.Count(); i++)
                                        {
                                            switch (props[i].Name)
                                            {
                                                case "ClientFirstName":
                                                    firstName = (string)props[i].GetValue(c, null);
                                                    break;
                                                case "ClientLastName":
                                                    lastName = (string)props[i].GetValue(c, null);
                                                    break;
                                                case "ClientApprovalStatus":
                                                    approvalStatus = (string)props[i].GetValue(c, null);
                                                    break;
                                                case "ClientId":
                                                    clientId = (int)props[i].GetValue(c, null);
                                                    break;
                                                case "Id":
                                                    rowId = (int?)props[i].GetValue(c, null);
                                                    break;
                                                case "Amount":
                                                    amount = (decimal?)props[i].GetValue(c, null);
                                                    break;
                                                case "Remarks":
                                                    remarks = (string)props[i].GetValue(c, null);
                                                    break;
                                            }
                                        }
                                        temp = rowId != null ? ((int)rowId).ToString() : "";
                                        pdfTabSubDetailsMhm.AddCell(temp);
                                        pdfTabSubDetailsMhm.AddCell(firstName);
                                        pdfTabSubDetailsMhm.AddCell(lastName);
                                        pdfTabSubDetailsMhm.AddCell(approvalStatus);
                                        pdfTabSubDetailsMhm.AddCell(clientId.ToString());
                                        temp = amount.HasValue ? amount.Value.Format() : "0";
                                        pdfTabSubDetailsMhm.AddCell(temp);
                                        pdfTabSubDetailsMhm.AddCell(remarks);
                                    }
                                    break;
                                case Service.ReportingMethods.TotalCostWithListOfClientNames:
                                    for (int i = 0; i < props.Count(); i++)
                                    {
                                        switch (props[i].Name)
                                        {
                                            case "ClientFirstName":
                                                firstName = (string)props[i].GetValue(c, null);
                                                break;
                                            case "ClientLastName":
                                                lastName = (string)props[i].GetValue(c, null);
                                                break;
                                            case "ClientApprovalStatus":
                                                approvalStatus = (string)props[i].GetValue(c, null);
                                                break;
                                            case "ClientId":
                                                clientId = (int)props[i].GetValue(c, null);
                                                break;
                                            case "Id":
                                                rowId = (int?)props[i].GetValue(c, null);
                                                break;
                                            case "Reported":
                                                reported = (bool?)props[i].GetValue(c, null);
                                                break;
                                            case "Remarks":
                                                remarks = (string)props[i].GetValue(c, null);
                                                break;
                                        }
                                    }
                                    temp = rowId != null ? ((int)rowId).ToString() : "";
                                    pdfTabSubDetailsRest.AddCell(temp);
                                    pdfTabSubDetailsRest.AddCell(firstName);
                                    pdfTabSubDetailsRest.AddCell(lastName);
                                    pdfTabSubDetailsRest.AddCell(approvalStatus);
                                    pdfTabSubDetailsRest.AddCell(clientId.ToString());
                                    temp = reported != null ? reported.ToString() : "N/A";
                                    pdfTabSubDetailsRest.AddCell(temp);
                                    pdfTabSubDetailsRest.AddCell(remarks);
                                    break;
                                case Service.ReportingMethods.ClientUnit:
                                    for (int i = 0; i < props.Count(); i++)
                                    {
                                        switch (props[i].Name)
                                        {
                                            case "ClientFirstName":
                                                firstName = (string)props[i].GetValue(c, null);
                                                break;
                                            case "ClientLastName":
                                                lastName = (string)props[i].GetValue(c, null);
                                                break;
                                            case "ClientApprovalStatus":
                                                approvalStatus = (string)props[i].GetValue(c, null);
                                                break;
                                            case "ClientId":
                                                clientId = (int)props[i].GetValue(c, null);
                                                break;
                                            case "Id":
                                                rowId = (int?)props[i].GetValue(c, null);
                                                break;
                                            case "Quantity":
                                                quantityNum = (decimal?)props[i].GetValue(c, null);
                                                break;
                                            case "Remarks":
                                                remarks = (string)props[i].GetValue(c, null);
                                                break;
                                        }
                                    }
                                    temp = rowId != null ? ((int)rowId).ToString() : "";
                                    pdfTabSubDetailsRest.AddCell(temp);
                                    pdfTabSubDetailsRest.AddCell(firstName);
                                    pdfTabSubDetailsRest.AddCell(lastName);
                                    pdfTabSubDetailsRest.AddCell(approvalStatus);
                                    pdfTabSubDetailsRest.AddCell(clientId.ToString());
                                    temp = quantityNum.HasValue ? quantityNum.Value.Format() : "0";
                                    pdfTabSubDetailsRest.AddCell(temp);
                                    pdfTabSubDetailsRest.AddCell(remarks);
                                    break;
                                case Service.ReportingMethods.ClientUnitAmount:
                                    for (int i = 0; i < props.Count(); i++)
                                    {
                                        switch (props[i].Name)
                                        {
                                            case "ClientFirstName":
                                                firstName = (string)props[i].GetValue(c, null);
                                                break;
                                            case "ClientLastName":
                                                lastName = (string)props[i].GetValue(c, null);
                                                break;
                                            case "ClientApprovalStatus":
                                                approvalStatus = (string)props[i].GetValue(c, null);
                                                break;
                                            case "ClientId":
                                                clientId = (int)props[i].GetValue(c, null);
                                                break;
                                            case "Id":
                                                rowId = (int?)props[i].GetValue(c, null);
                                                break;
                                            case "Amount":
                                                amount = (decimal?)props[i].GetValue(c, null);
                                                break;
                                            case "Quantity":
                                                quantityNum = (decimal?)props[i].GetValue(c, null);
                                                break;
                                            case "Remarks":
                                                remarks = (string)props[i].GetValue(c, null);
                                                break;
                                        }
                                    }
                                    temp = rowId != null ? ((int)rowId).ToString() : "";
                                    pdfTabSubDetailsClientUnitAmount.AddCell(temp);
                                    pdfTabSubDetailsClientUnitAmount.AddCell(firstName);
                                    pdfTabSubDetailsClientUnitAmount.AddCell(lastName);
                                    pdfTabSubDetailsClientUnitAmount.AddCell(approvalStatus);
                                    pdfTabSubDetailsClientUnitAmount.AddCell(clientId.ToString());
                                    temp = amount.HasValue ? amount.Value.Format() : "0";
                                    pdfTabSubDetailsClientUnitAmount.AddCell(temp);
                                    temp = quantityNum.HasValue ? quantityNum.Value.Format() : "0";
                                    pdfTabSubDetailsClientUnitAmount.AddCell(temp);
                                    pdfTabSubDetailsClientUnitAmount.AddCell(remarks);
                                    break;
                                case Service.ReportingMethods.Emergency:
                                    for (int i = 0; i < props.Count(); i++)
                                    {
                                        switch (props[i].Name)
                                        {
                                            case "ClientFirstName":
                                                firstName = (string)props[i].GetValue(c, null);
                                                break;
                                            case "ClientLastName":
                                                lastName = (string)props[i].GetValue(c, null);
                                                break;
                                            case "ClientApprovalStatus":
                                                approvalStatus = (string)props[i].GetValue(c, null);
                                                break;
                                            case "ClientId":
                                                clientId = (int)props[i].GetValue(c, null);
                                                break;
                                            case "ReportDate":
                                                reportDate = (DateTime?)props[i].GetValue(c, null);
                                                break;
                                            case "Amount":
                                                amount = (decimal?)props[i].GetValue(c, null);
                                                break;
                                            case "TypeName":
                                                type = (string)props[i].GetValue(c, null);
                                                break;
                                            case "Remarks":
                                                remarks = (string)props[i].GetValue(c, null);
                                                break;
                                            case "Discretionary":
                                                discretionary = (decimal?)props[i].GetValue(c, null);
                                                break;
                                            case "Total":
                                                total = (decimal?)props[i].GetValue(c, null);
                                                break;
                                            case "UniqueCircumstances":
                                                uniqueCirc = (string)props[i].GetValue(c, null);
                                                break;
                                        }
                                    }
                                    pdfTabSubDetailsEmergency.AddCell(firstName);
                                    pdfTabSubDetailsEmergency.AddCell(lastName);
                                    pdfTabSubDetailsEmergency.AddCell(approvalStatus);
                                    pdfTabSubDetailsEmergency.AddCell(clientId.ToString());
                                    temp = reportDate != null ? ((DateTime)reportDate).ToShortDateString() : "N/A";
                                    pdfTabSubDetailsEmergency.AddCell(temp);
                                    pdfTabSubDetailsEmergency.AddCell(type);
                                    pdfTabSubDetailsEmergency.AddCell(remarks);
                                    temp = amount.HasValue ? amount.Value.Format() : "0";
                                    pdfTabSubDetailsEmergency.AddCell(temp);
                                    temp = discretionary.HasValue ? discretionary.Value.Format() : "0";
                                    pdfTabSubDetailsEmergency.AddCell(temp);
                                    temp = total.HasValue ? total.Value.Format() : "0";
                                    pdfTabSubDetailsEmergency.AddCell(temp);
                                    pdfTabSubDetailsEmergency.AddCell(uniqueCirc);
                                    break;
                                case Service.ReportingMethods.Homecare:
                                    for (int j = 0; j < props.Count(); j++)
                                    {
                                        switch (props[j].Name)
                                        {
                                            case "FirstName":
                                                firstName = (string)props[j].GetValue(c, null);
                                                break;
                                            case "LastName":
                                                lastName = (string)props[j].GetValue(c, null);
                                                break;
                                            case "ApprovalStatus":
                                                approvalStatus = (string)props[j].GetValue(c, null);
                                                break;
                                            case "ClientId":
                                                clientId = (int)props[j].GetValue(c, null);
                                                break;
                                            case "Rate":
                                                rate = (decimal?)props[j].GetValue(c, null);
                                                break;
                                            case "Cur":
                                                cur = (string)props[j].GetValue(c, null);
                                                break;
                                            case "Q1":
                                                Q1 = (decimal?)props[j].GetValue(c, null);
                                                if (Q1 != null)
                                                {
                                                    quantity.Add((decimal)Q1);
                                                }
                                                else
                                                {
                                                    quantity.Add(0);
                                                }
                                                break;
                                            case "Q2":
                                                Q2 = (decimal?)props[j].GetValue(c, null);
                                                if (Q2 != null)
                                                {
                                                    quantity.Add((decimal)Q2);
                                                }
                                                else
                                                {
                                                    quantity.Add(0);
                                                }
                                                break;
                                            case "Q3":
                                                Q3 = (decimal?)props[j].GetValue(c, null);
                                                if (Q3 != null)
                                                {
                                                    quantity.Add((decimal)Q3);
                                                }
                                                else
                                                {
                                                    quantity.Add(0);
                                                }
                                                break;
                                            case "Remarks":
                                                remarks = (string)props[j].GetValue(c, null);
                                                break;
                                            case "HcCaps":
                                                hccaps = (IEnumerable<object>)props[j].GetValue(c, null);
                                                break;
                                        }
                                    }
                                    pdfTabSubDetailsHomcare.AddCell(firstName);
                                    pdfTabSubDetailsHomcare.AddCell(lastName);
                                    pdfTabSubDetailsHomcare.AddCell(approvalStatus);
                                    pdfTabSubDetailsHomcare.AddCell(clientId.ToString());
                                    var ratestr = rate.HasValue ? rate.Value.Format() : "0";
                                    pdfTabSubDetailsHomcare.AddCell(ratestr);
                                    pdfTabSubDetailsHomcare.AddCell(cur);
                                    for (int i = 0; i < numOfDates; i++)
                                    {
                                        pdfTabSubDetailsHomcare.AddCell(quantity[i].Format());
                                        totalQuantity += quantity[i];
                                    }
                                    pdfTabSubDetailsHomcare.AddCell(remarks);
                                    pdfTabSubDetailsHomcare.AddCell(totalQuantity.Format());
                                    List<hcCapPdfRow> hclist = new List<hcCapPdfRow>();
                                    foreach (var h in hccaps)
                                    {
                                        var hcprops = h.GetType().GetProperties();
                                        int hcClientId = 0;
                                        decimal? cap = null;
                                        DateTime StartDate = DateTime.Now;
                                        DateTime? EndDate = null;
                                        for (int i = 0; i < hcprops.Count(); i++)
                                        {
                                            switch (hcprops[i].Name)
                                            {
                                                case "ClientId":
                                                    hcClientId = (int)hcprops[i].GetValue(h, null);
                                                    break;
                                                case "HcCap":
                                                    cap = (decimal?)hcprops[i].GetValue(h, null);
                                                    break;
                                                case "StartDate":
                                                    StartDate = (DateTime)hcprops[i].GetValue(h, null);
                                                    break;
                                                case "EndDate":
                                                    EndDate = (DateTime?)hcprops[i].GetValue(h, null);
                                                    break;
                                            }
                                        }
                                        hclist.Add(new hcCapPdfRow
                                        {
                                            ClientId = hcClientId,
                                            HcCap = cap,
                                            StartDate = StartDate,
                                            EndDate = EndDate
                                        });
                                    }
                                    for (int i = hclist.Count - 1; i > 0; i--)
                                    {
                                        if (hclist[i].HcCap == hclist[i - 1].HcCap && hclist[i].StartDate == hclist[i - 1].EndDate)
                                        {
                                            hclist[i - 1].EndDate = hclist[i].EndDate;
                                            hclist.Remove(hclist[i]);
                                        }
                                    }
                                    if (hclist.Count == 1)
                                    {
                                        var hccapstr = hclist[0].HcCap.HasValue ? hclist[0].HcCap.Value.Format() : "0";
                                        pdfTabSubDetailsHomcare.AddCell(hccapstr);
                                    }
                                    else if (hclist.Count > 1)
                                    {
                                        var hccapstr = "";
                                        foreach (var t in hclist)
                                        {
                                            var capstr = t.HcCap.HasValue ? t.HcCap.Value.Format() : "0";
                                            var enddatestr = t.EndDate != null ? (((DateTime)t.EndDate).AddDays(-1)).ToShortDateString() : "N/A";
                                            hccapstr += t.StartDate.ToShortDateString() + " - " + enddatestr + " : " + capstr + "\n";
                                        }
                                        pdfTabSubDetailsHomcare.AddCell(hccapstr);
                                    }
                                    else
                                    {
                                        pdfTabSubDetailsHomcare.AddCell(string.Empty);
                                    }
                                    break;
                                case Service.ReportingMethods.HomecareWeekly:
                                    for (int j = 0; j < props.Count(); j++)
                                    {
                                        switch (props[j].Name)
                                        {
                                            case "FirstName":
                                                firstName = (string)props[j].GetValue(c, null);
                                                break;
                                            case "LastName":
                                                lastName = (string)props[j].GetValue(c, null);
                                                break;
                                            case "HASName":
                                                approvalStatus = (string)props[j].GetValue(c, null);
                                                break;
                                            case "ClientId":
                                                clientId = (int)props[j].GetValue(c, null);
                                                break;
                                            case "Rate":
                                                rate = (decimal?)props[j].GetValue(c, null);
                                                break;
                                            case "Cur":
                                                cur = (string)props[j].GetValue(c, null);
                                                break;
                                            case "W1":
                                                W1 = (decimal?)props[j].GetValue(c, null);
                                                if (W1 != null)
                                                {
                                                    quantity.Add((decimal)W1);
                                                }
                                                else
                                                {
                                                    quantity.Add(0);
                                                }
                                                break;
                                            case "W2":
                                                W2 = (decimal?)props[j].GetValue(c, null);
                                                if (W2 != null)
                                                {
                                                    quantity.Add((decimal)W2);
                                                }
                                                else
                                                {
                                                    quantity.Add(0);
                                                }
                                                break;
                                            case "W3":
                                                W3 = (decimal?)props[j].GetValue(c, null);
                                                if (W3 != null)
                                                {
                                                    quantity.Add((decimal)W3);
                                                }
                                                else
                                                {
                                                    quantity.Add(0);
                                                }
                                                break;
                                            case "W4":
                                                W4 = (decimal?)props[j].GetValue(c, null);
                                                if (W4 != null)
                                                {
                                                    quantity.Add((decimal)W4);
                                                }
                                                else
                                                {
                                                    quantity.Add(0);
                                                }
                                                break;
                                            case "W5":
                                                W5 = (decimal?)props[j].GetValue(c, null);
                                                if (W5 != null)
                                                {
                                                    quantity.Add((decimal)W5);
                                                }
                                                else
                                                {
                                                    quantity.Add(0);
                                                }
                                                break;
                                            case "W6":
                                                W6 = (decimal?)props[j].GetValue(c, null);
                                                if (W6 != null)
                                                {
                                                    quantity.Add((decimal)W6);
                                                }
                                                else
                                                {
                                                    quantity.Add(0);
                                                }
                                                break;
                                            case "W7":
                                                W7 = (decimal?)props[j].GetValue(c, null);
                                                if (W7 != null)
                                                {
                                                    quantity.Add((decimal)W7);
                                                }
                                                else
                                                {
                                                    quantity.Add(0);
                                                }
                                                break;
                                            case "W8":
                                                W8 = (decimal?)props[j].GetValue(c, null);
                                                if (W8 != null)
                                                {
                                                    quantity.Add((decimal)W8);
                                                }
                                                else
                                                {
                                                    quantity.Add(0);
                                                }
                                                break;
                                            case "W9":
                                                W9 = (decimal?)props[j].GetValue(c, null);
                                                if (W9 != null)
                                                {
                                                    quantity.Add((decimal)W9);
                                                }
                                                else
                                                {
                                                    quantity.Add(0);
                                                }
                                                break;
                                            case "W10":
                                                W10 = (decimal?)props[j].GetValue(c, null);
                                                if (W10 != null)
                                                {
                                                    quantity.Add((decimal)W10);
                                                }
                                                else
                                                {
                                                    quantity.Add(0);
                                                }
                                                break;
                                            case "W11":
                                                W11 = (decimal?)props[j].GetValue(c, null);
                                                if (W11 != null)
                                                {
                                                    quantity.Add((decimal)W11);
                                                }
                                                else
                                                {
                                                    quantity.Add(0);
                                                }
                                                break;
                                            case "W12":
                                                W12 = (decimal?)props[j].GetValue(c, null);
                                                if (W12 != null)
                                                {
                                                    quantity.Add((decimal)W12);
                                                }
                                                else
                                                {
                                                    quantity.Add(0);
                                                }
                                                break;
                                            case "W13":
                                                W13 = (decimal?)props[j].GetValue(c, null);
                                                if (W13 != null)
                                                {
                                                    quantity.Add((decimal)W13);
                                                }
                                                else
                                                {
                                                    quantity.Add(0);
                                                }
                                                break;
                                            case "W14":
                                                W14 = (decimal?)props[j].GetValue(c, null);
                                                if (W14 != null)
                                                {
                                                    quantity.Add((decimal)W14);
                                                }
                                                else
                                                {
                                                    quantity.Add(0);
                                                }
                                                break;
                                            case "W15":
                                                W15 = (decimal?)props[j].GetValue(c, null);
                                                if (W15 != null)
                                                {
                                                    quantity.Add((decimal)W15);
                                                }
                                                else
                                                {
                                                    quantity.Add(0);
                                                }
                                                break;
                                            case "Remarks":
                                                remarks = (string)props[j].GetValue(c, null);
                                                break;
                                            case "HcCaps":
                                                hccaps = (IEnumerable<object>)props[j].GetValue(c, null);
                                                break;
                                            case "LeaveDate":
                                                reportDate = (DateTime?)props[j].GetValue(c, null);
                                                break;
                                        }
                                    }
                                    pdfTabSubDetailsHomcare.AddCell(firstName);
                                    pdfTabSubDetailsHomcare.AddCell(lastName);
                                    pdfTabSubDetailsHomcare.AddCell(approvalStatus);
                                    pdfTabSubDetailsHomcare.AddCell(clientId.ToString());
                                    var rateweeklystr = rate.HasValue ? rate.Value.Format() : "0";
                                    pdfTabSubDetailsHomcare.AddCell(rateweeklystr);
                                    pdfTabSubDetailsHomcare.AddCell(cur);
                                    for (int i = 0; i < numOfDates; i++)
                                    {
                                        pdfTabSubDetailsHomcare.AddCell(quantity[i].Format());
                                        totalQuantity += quantity[i];
                                    }
                                    pdfTabSubDetailsHomcare.AddCell(remarks);
                                    pdfTabSubDetailsHomcare.AddCell(totalQuantity.Format());
                                    List<hcCapPdfRow> hcweeklylist = new List<hcCapPdfRow>();
                                    foreach (var h in hccaps)
                                    {
                                        var hcprops = h.GetType().GetProperties();
                                        int hcClientId = 0;
                                        decimal? cap = null;
                                        DateTime StartDate = DateTime.Now;
                                        DateTime? EndDate = null;
                                        bool AfterReportStart = false;
                                        for (int i = 0; i < hcprops.Count(); i++)
                                        {
                                            switch (hcprops[i].Name)
                                            {
                                                case "ClientId":
                                                    hcClientId = (int)hcprops[i].GetValue(h, null);
                                                    break;
                                                case "HcCap":
                                                    cap = (decimal?)hcprops[i].GetValue(h, null);
                                                    break;
                                                case "StartDate":
                                                    StartDate = (DateTime)hcprops[i].GetValue(h, null);
                                                    break;
                                                case "EndDate":
                                                    EndDate = (DateTime?)hcprops[i].GetValue(h, null);
                                                    break;
                                                case "AfterReportStart":
                                                    AfterReportStart = (bool)hcprops[i].GetValue(h, null);
                                                    break;
                                            }
                                        }
                                        hcweeklylist.Add(new hcCapPdfRow
                                        {
                                            ClientId = hcClientId,
                                            HcCap = cap,
                                            StartDate = StartDate,
                                            EndDate = EndDate,
                                            AfterReportStart = AfterReportStart
                                        });
                                    }
                                    for (int i = hcweeklylist.Count - 1; i > 0; i--)
                                    {
                                        if (hcweeklylist[i].HcCap == hcweeklylist[i - 1].HcCap && hcweeklylist[i].StartDate == hcweeklylist[i - 1].EndDate)
                                        {
                                            hcweeklylist[i - 1].EndDate = hcweeklylist[i].EndDate;
                                            hcweeklylist.Remove(hcweeklylist[i]);
                                        }
                                    }
                                    if (hcweeklylist.Count == 1 && hcweeklylist[0].AfterReportStart)
                                    {
                                        var hccapstr = hcweeklylist[0].StartDate.ToShortDateString() + " - null : " + (hcweeklylist[0].HcCap.HasValue ? hcweeklylist[0].HcCap.Value.Format() : "0");
                                        pdfTabSubDetailsHomcare.AddCell(hccapstr);
                                    }
                                    else if (hcweeklylist.Count == 1 && hcweeklylist[0].EndDate.HasValue)
                                    {
                                        var enddatestr = reportDate.HasValue && reportDate.Value.AddDays(1) == hcweeklylist[0].EndDate ? reportDate.Value.ToShortDateString() : hcweeklylist[0].EndDate.Value.ToShortDateString();
                                        var hccapstr = hcweeklylist[0].StartDate.ToShortDateString() + " - " + enddatestr + " : " + (hcweeklylist[0].HcCap.HasValue ? hcweeklylist[0].HcCap.Value.Format() : "0");
                                        pdfTabSubDetailsHomcare.AddCell(hccapstr);
                                    }
                                    else if (hcweeklylist.Count == 1)
                                    {
                                        var hccapstr = hcweeklylist[0].HcCap.HasValue ? hcweeklylist[0].HcCap.Value.Format() : "0";
                                        pdfTabSubDetailsHomcare.AddCell(hccapstr);
                                    }
                                    else if (hcweeklylist.Count > 1)
                                    {
                                        var hccapstr = "";
                                        foreach (var t in hcweeklylist)
                                        {
                                            var capstr = t.HcCap.HasValue ? t.HcCap.Value.Format() : "0";
                                            var enddatestr = t.EndDate != null ? (((DateTime)t.EndDate).AddDays(-1)).ToShortDateString() : "N/A";
                                            hccapstr += t.StartDate.ToShortDateString() + " - " + enddatestr + " : " + capstr + "\n";
                                        }
                                        pdfTabSubDetailsHomcare.AddCell(hccapstr);
                                    }
                                    else
                                    {
                                        pdfTabSubDetailsHomcare.AddCell(string.Empty);
                                    }
                                    break;
                                case Service.ReportingMethods.SupportiveCommunities:
                                    for (int i = 0; i < props.Count(); i++)
                                    {
                                        switch (props[i].Name)
                                        {
                                            case "ClientFirstName":
                                                firstName = (string)props[i].GetValue(c, null);
                                                break;
                                            case "ClientLastName":
                                                lastName = (string)props[i].GetValue(c, null);
                                                break;
                                            case "ClientApprovalStatus":
                                                approvalStatus = (string)props[i].GetValue(c, null);
                                                break;
                                            case "ClientId":
                                                clientId = (int)props[i].GetValue(c, null);
                                                break;
                                            case "Amount":
                                                amount = (decimal?)props[i].GetValue(c, null);
                                                break;
                                            case "IsraelId":
                                                israelId = (string)props[i].GetValue(c, null);
                                                break;
                                            case "HoursHoldCost":
                                                hoursHoldCost = (decimal?)props[i].GetValue(c, null);
                                                break;
                                            case "MonthsCount":
                                                monthsCount = (int?)props[i].GetValue(c, null);
                                                break;
                                            case "JoinDate":
                                                joinDate = (DateTime)props[i].GetValue(c, null);
                                                break;
                                            case "Reported":
                                                reported = (bool)props[i].GetValue(c, null);
                                                break;
                                        }
                                    }
                                    pdfTabSubDetailsSc.AddCell(clientId.ToString());
                                    pdfTabSubDetailsSc.AddCell(israelId);
                                    pdfTabSubDetailsSc.AddCell(firstName);
                                    pdfTabSubDetailsSc.AddCell(lastName);
                                    pdfTabSubDetailsSc.AddCell(approvalStatus);
                                    temp = joinDate != null ? ((DateTime)joinDate).ToShortDateString() : "N/A";
                                    pdfTabSubDetailsSc.AddCell(temp);
                                    temp = hoursHoldCost.HasValue ? hoursHoldCost.Value.Format() : "0";
                                    pdfTabSubDetailsSc.AddCell(temp);
                                    temp = monthsCount != null ? ((int)monthsCount).ToString() : "";
                                    pdfTabSubDetailsSc.AddCell(temp);
                                    temp = amount.HasValue ? amount.Value.Format() : "0";
                                    pdfTabSubDetailsSc.AddCell(temp);
                                    temp = reported.HasValue && reported.Value ? "True" : "False";
                                    pdfTabSubDetailsSc.AddCell(temp);
                                    break;
                                case Service.ReportingMethods.DayCenters:
                                    for (int i = 0; i < props.Count(); i++)
                                    {
                                        switch (props[i].Name)
                                        {
                                            case "ClientFirstName":
                                                firstName = (string)props[i].GetValue(c, null);
                                                break;
                                            case "ClientLastName":
                                                lastName = (string)props[i].GetValue(c, null);
                                                break;
                                            case "ClientApprovalStatus":
                                                approvalStatus = (string)props[i].GetValue(c, null);
                                                break;
                                            case "ClientId":
                                                clientId = (int)props[i].GetValue(c, null);
                                                break;
                                            case "Amount":
                                                amount = (decimal?)props[i].GetValue(c, null);
                                                break;
                                            case "IsraelId":
                                                israelId = (string)props[i].GetValue(c, null);
                                                break;
                                            case "VisitCost":
                                                visitCost = (decimal?)props[i].GetValue(c, null);
                                                break;
                                            case "SubsidesByDcc":
                                                subsidesByDcc = (int?)props[i].GetValue(c, null);
                                                break;
                                            case "VisitCount":
                                                visitCount = (int?)props[i].GetValue(c, null);
                                                break;
                                            case "JoinDate":
                                                joinDate = (DateTime)props[i].GetValue(c, null);
                                                break;
                                        }
                                    }
                                    pdfTabSubDetailsDcc.AddCell(clientId.ToString());
                                    pdfTabSubDetailsDcc.AddCell(israelId);
                                    pdfTabSubDetailsDcc.AddCell(firstName);
                                    pdfTabSubDetailsDcc.AddCell(lastName);
                                    pdfTabSubDetailsDcc.AddCell(approvalStatus);
                                    temp = joinDate != null ? ((DateTime)joinDate).ToShortDateString() : "N/A";
                                    pdfTabSubDetailsDcc.AddCell(temp);
                                    temp = subsidesByDcc != null ? ((int)subsidesByDcc).ToString() : "";
                                    pdfTabSubDetailsDcc.AddCell(temp);
                                    temp = visitCost.HasValue ? visitCost.Value.Format() : "0";
                                    pdfTabSubDetailsDcc.AddCell(temp);
                                    temp = visitCount != null ? ((int)visitCount).ToString() : "";
                                    pdfTabSubDetailsDcc.AddCell(temp);
                                    temp = amount.HasValue ? amount.Value.Format() : "0";
                                    pdfTabSubDetailsDcc.AddCell(temp);
                                    break;
                                case Service.ReportingMethods.SoupKitchens:
                                    for (int i = 0; i < props.Count(); i++)
                                    {
                                        switch (props[i].Name)
                                        {
                                            case "ClientFirstName":
                                                firstName = (string)props[i].GetValue(c, null);
                                                break;
                                            case "ClientLastName":
                                                lastName = (string)props[i].GetValue(c, null);
                                                break;
                                            case "ClientApprovalStatus":
                                                approvalStatus = (string)props[i].GetValue(c, null);
                                                break;
                                            case "ClientId":
                                                clientId = (int)props[i].GetValue(c, null);
                                                break;
                                            case "Amount":
                                                amount = (decimal?)props[i].GetValue(c, null);
                                                break;
                                            case "IsraelId":
                                                israelId = (string)props[i].GetValue(c, null);
                                                break;
                                            case "VisitCount":
                                                visitCount = (int?)props[i].GetValue(c, null);
                                                break;
                                            case "JoinDate":
                                                joinDate = (DateTime)props[i].GetValue(c, null);
                                                break;
                                        }
                                    }
                                    pdfTabSubDetailsSoupKitchens.AddCell(clientId.ToString());
                                    pdfTabSubDetailsSoupKitchens.AddCell(israelId);
                                    pdfTabSubDetailsSoupKitchens.AddCell(firstName);
                                    pdfTabSubDetailsSoupKitchens.AddCell(lastName);
                                    pdfTabSubDetailsSoupKitchens.AddCell(approvalStatus);
                                    temp = joinDate != null ? ((DateTime)joinDate).ToShortDateString() : "N/A";
                                    pdfTabSubDetailsSoupKitchens.AddCell(temp);
                                    temp = visitCount != null ? ((int)visitCount).ToString() : "";
                                    pdfTabSubDetailsSoupKitchens.AddCell(temp);
                                    break;
                                case Service.ReportingMethods.ClientEventsCount:
                                    for (int i = 0; i < props.Count(); i++)
                                    {
                                        switch (props[i].Name)
                                        {
                                            case "EventDate":
                                                reportDate = (DateTime)props[i].GetValue(c, null);
                                                break;
                                            case "JNVCount":
                                                JNVCount = (int)props[i].GetValue(c, null);
                                                break;
                                            case "TotalCount":
                                                TotalCount = (int?)props[i].GetValue(c, null);
                                                break;
                                            case "Remarks":
                                                remarks = (string)props[i].GetValue(c, null);
                                                break;
                                        }
                                    }
                                    temp = reportDate != null ? ((DateTime)reportDate).ToShortDateString() : "N/A";
                                    pdfTabSubDetailsClientEventsCount.AddCell(temp);
                                    pdfTabSubDetailsClientEventsCount.AddCell(JNVCount.ToString());
                                    temp = TotalCount != null ? ((int)TotalCount).ToString() : "";
                                    pdfTabSubDetailsClientEventsCount.AddCell(temp);
                                    pdfTabSubDetailsClientEventsCount.AddCell(remarks);
                                    break;
                            }
                        }
                        if (clients.Count > 0)
                        {
                            switch (subreport.AppBudgetService.Service.ReportingMethodEnum)
                            {
                                case Service.ReportingMethods.ClientNamesAndCosts:
                                    if (subreport.AppBudgetService.Service.TypeId != (int)Service.ServiceTypes.MinorHomeModifications && subreport.AppBudgetService.Service.TypeId != (int)Service.ServiceTypes.MedicalProgram)
                                    {
                                        subReportTotalsCell.Colspan = clientNamesAndCosts;
                                        pdfTabSubDetailsClientNamesAndCosts.AddCell(subReportTotalsCell);
                                        subReportsTables.Add(pdfTabSubDetailsClientNamesAndCosts, true);
                                    }
                                    else
                                    {
                                        subReportTotalsCell.Colspan = mhmColspan;
                                        pdfTabSubDetailsMhm.AddCell(subReportTotalsCell);
                                        subReportsTables.Add(pdfTabSubDetailsMhm, true);
                                    }
                                    break;
                                case Service.ReportingMethods.TotalCostWithListOfClientNames:
                                case Service.ReportingMethods.ClientUnit:
                                    subReportTotalsCell.Colspan = restReportsColspan;
                                    pdfTabSubDetailsRest.AddCell(subReportTotalsCell);
                                    subReportsTables.Add(pdfTabSubDetailsRest, true);
                                    break;
                                case Service.ReportingMethods.ClientUnitAmount:
                                    subReportTotalsCell.Colspan = clientUnitAmountColspan;
                                    pdfTabSubDetailsClientUnitAmount.AddCell(subReportTotalsCell);
                                    subReportsTables.Add(pdfTabSubDetailsClientUnitAmount, true);
                                    break;
                                case Service.ReportingMethods.SupportiveCommunities:
                                    subReportTotalsCell.Colspan = scColspan;
                                    pdfTabSubDetailsSc.AddCell(subReportTotalsCell);
                                    subReportsTables.Add(pdfTabSubDetailsSc, true);
                                    break;
                                case Service.ReportingMethods.Emergency:
                                    subReportTotalsCell.Colspan = emergencyColspan;
                                    pdfTabSubDetailsEmergency.AddCell(subReportTotalsCell);
                                    subReportsTables.Add(pdfTabSubDetailsEmergency, true);
                                    break;
                                case Service.ReportingMethods.Homecare:
                                case Service.ReportingMethods.HomecareWeekly:
                                    subReportTotalsCell.Colspan = homecareColspan;
                                    pdfTabSubDetailsHomcare.AddCell(subReportTotalsCell);
                                    subReportsTables.Add(pdfTabSubDetailsHomcare, true);
                                    break;
                                case Service.ReportingMethods.DayCenters:
                                    jqmodel.FilterByMonth.SelYear = model.MainReportStart.Year;
                                    var diff = ccEntities.DiffMonths(model.MainReportStart, model.MainReportEnd);
                                    int calendarColspan = 4;
                                    int visitsCountColumn = 36;
                                    PdfPTable calendarTable = new PdfPTable(calendarColspan);
                                    calendarTable.HorizontalAlignment = 1;
                                    calendarTable.WidthPercentage = widthPercent;
                                    PdfPCell calendarMainHeaderCell = new PdfPCell(new Phrase("Day Centers - Calendar"));
                                    calendarMainHeaderCell.Colspan = calendarColspan;
                                    calendarMainHeaderCell.HorizontalAlignment = 1;
                                    calendarTable.AddCell(calendarMainHeaderCell);
                                    for (var i = 0; i < diff; i++)
                                    {
                                        jqmodel.FilterByMonth.SelMonth = model.MainReportStart.Month + i;
                                        JsonResult calresult = null;
                                        try
                                        {
                                            calresult = src.GetCalendarRows(jqmodel, subreport);
                                        }
                                        catch (NotImplementedException)
                                        {
                                            //total cost no names
                                        }
                                        catch (Exception exc)
                                        {
                                            throw new Exception(exc.InnerException != null ? exc.InnerException.Message : exc.Message, exc);
                                        }
                                        if (calresult != null)
                                        {
                                            var calendarData = (SubReportDetailsJqDt)calresult.Data;
                                            if (calendarData.aaData.Any(f => (int)(f.ToList())[visitsCountColumn] > 0))
                                            {
                                                PdfPCell calendarHeaderCell = new PdfPCell(new Phrase(model.MainReportStart.AddMonths(i).ToString("MMMM")));
                                                calendarHeaderCell.Colspan = calendarColspan;
                                                calendarHeaderCell.HorizontalAlignment = 1;
                                                calendarTable.AddCell(calendarHeaderCell);
                                                calendarTable.AddCell("CC ID");
                                                calendarTable.AddCell("Client Name");
                                                calendarTable.AddCell("Client JNV Status");
                                                calendarTable.AddCell("Visit Date");
                                            }
                                            foreach (var d in calendarData.aaData)
                                            {
                                                var datalist = d.ToList();
                                                if ((int)datalist[visitsCountColumn] > 0)
                                                {
                                                    for (var j = 5; j < visitsCountColumn; j++)
                                                    {
                                                        var calDate = model.MainReportStart.AddMonths(i).AddDays(j - 5);
                                                        if ((bool)datalist[j] && calDate.DayOfWeek != DayOfWeek.Saturday)
                                                        {
                                                            calendarTable.AddCell(((int)datalist[0]).ToString());
                                                            calendarTable.AddCell((string)datalist[1]);
                                                            calendarTable.AddCell((string)datalist[4]);
                                                            calendarTable.AddCell(calDate.ToString("dd MMM yyyy"));
                                                        }
                                                    }
                                                    calendarTable.AddCell("Visits Count");
                                                    calendarTable.AddCell(((int)datalist[visitsCountColumn]).ToString());
                                                    calendarTable.AddCell("");
                                                    calendarTable.AddCell("");
                                                }
                                            }
                                        }
                                    }
                                    PdfPCell calendarCell = new PdfPCell(calendarTable);
                                    calendarCell.Colspan = dccColspan;
                                    calendarCell.Padding = 0f;
                                    pdfTabSubDetailsDcc.AddCell(calendarCell);
                                    subReportTotalsCell.Colspan = dccColspan;
                                    pdfTabSubDetailsDcc.AddCell(subReportTotalsCell);
                                    subReportsTables.Add(pdfTabSubDetailsDcc, true);
                                    break;
                                case Service.ReportingMethods.SoupKitchens:
                                    jqmodel.FilterByMonth.SelYear = model.MainReportStart.Year;
                                    var diffSK = ccEntities.DiffMonths(model.MainReportStart, model.MainReportEnd);
                                    int calendarColspanSK = 4;
                                    int visitsCountColumnSK = 36;
                                    PdfPTable calendarTableSK = new PdfPTable(calendarColspanSK);
                                    calendarTableSK.HorizontalAlignment = 1;
                                    calendarTableSK.WidthPercentage = widthPercent;
                                    PdfPCell calendarMainHeaderCellSK = new PdfPCell(new Phrase("Soup Kitchens - Calendar"));
                                    calendarMainHeaderCellSK.Colspan = calendarColspanSK;
                                    calendarMainHeaderCellSK.HorizontalAlignment = 1;
                                    calendarTableSK.AddCell(calendarMainHeaderCellSK);
                                    for (var i = 0; i < diffSK; i++)
                                    {
                                        jqmodel.FilterByMonth.SelMonth = model.MainReportStart.Month + i;
                                        JsonResult calresult = null;
                                        try
                                        {
                                            calresult = src.GetCalendarSaturdaysRows(jqmodel, subreport);
                                        }
                                        catch (NotImplementedException)
                                        {
                                            //total cost no names
                                        }
                                        catch (Exception exc)
                                        {
                                            throw new Exception(exc.InnerException != null ? exc.InnerException.Message : exc.Message, exc);
                                        }
                                        if (calresult != null)
                                        {
                                            var calendarData = (SubReportDetailsJqDt)calresult.Data;
                                            if (calendarData.aaData.Any(f => (int)(f.ToList())[visitsCountColumnSK] > 0))
                                            {
                                                PdfPCell calendarHeaderCell = new PdfPCell(new Phrase(model.MainReportStart.AddMonths(i).ToString("MMMM")));
                                                calendarHeaderCell.Colspan = calendarColspanSK;
                                                calendarHeaderCell.HorizontalAlignment = 1;
                                                calendarTableSK.AddCell(calendarHeaderCell);
                                                calendarTableSK.AddCell("CC ID");
                                                calendarTableSK.AddCell("Client Name");
                                                calendarTableSK.AddCell("Client JNV Status");
                                                calendarTableSK.AddCell("Visit Date");
                                            }
                                            foreach (var d in calendarData.aaData)
                                            {
                                                var datalist = d.ToList();
                                                if ((int)datalist[visitsCountColumnSK] > 0)
                                                {
                                                    for (var j = 5; j < visitsCountColumnSK; j++)
                                                    {
                                                        var calDate = model.MainReportStart.AddMonths(i).AddDays(j - 5);
                                                        if ((bool)datalist[j])
                                                        {
                                                            calendarTableSK.AddCell(((int)datalist[0]).ToString());
                                                            calendarTableSK.AddCell((string)datalist[1]);
                                                            calendarTableSK.AddCell((string)datalist[4]);
                                                            calendarTableSK.AddCell(calDate.ToString("dd MMM yyyy"));
                                                        }
                                                    }
                                                    calendarTableSK.AddCell("Meal Count");
                                                    calendarTableSK.AddCell(((int)datalist[visitsCountColumnSK]).ToString());
                                                    calendarTableSK.AddCell("");
                                                    calendarTableSK.AddCell("");
                                                }
                                            }
                                        }
                                    }
                                    subReportsTables.Add(pdfTabSubDetailsSoupKitchens, true);

                                    subReportsTables.Add(calendarTableSK, false);
                                    var subReportTotalsTable = new PdfPTable(pdfSubReportTotals);
                                    subReportTotalsTable.WidthPercentage = 95;
                                    subReportsTables.Add(subReportTotalsTable, false);
                                    break;
                                case Service.ReportingMethods.ClientEventsCount:
                                    clientEventsCountSubReportTables.Add(pdfTabSubDetailsClientEventsCount, true);
                                    break;
                            }
                        }
                        else
                        {
                            PdfPTable pdfTabOnlyTotals = new PdfPTable(1);
                            pdfTabOnlyTotals.HorizontalAlignment = 1;
                            pdfTabOnlyTotals.SpacingBefore = 5f;
                            pdfTabOnlyTotals.SpacingAfter = 5f;
                            pdfTabOnlyTotals.WidthPercentage = widthPercent;
                            PdfPCell onlyTotalsHeader = new PdfPCell(new Phrase(subReportHeader.ServiceName));
                            onlyTotalsHeader.HorizontalAlignment = 1;
                            onlyTotalsHeader.Border = 0;
                            onlyTotalsHeader.Colspan = 1;
                            subReportHeaderCell.Colspan = 1;
                            subReportTotalsCell.Colspan = 1;
                            pdfTabOnlyTotals.AddCell(onlyTotalsHeader);
                            pdfTabOnlyTotals.AddCell(subReportHeaderCell);
                            pdfTabOnlyTotals.AddCell(subReportTotalsCell);
                            subReportsTables.Add(pdfTabOnlyTotals, true);
                        }
                    } //end of if(subreport != null)
                    pdfTabYtdSummary.AddCell(unqClients);
                } //end of foreach (var item in detailsModel)

                pdfTabYtdSummary.AddCell("Total");
                pdfTabYtdSummary.AddCell("");
                var appAmount = financialReportDetails.Totals.AppAmount;
                var appAmountStr = appAmount.HasValue ? appAmount.Value.Format() : "0";
                pdfTabYtdSummary.AddCell(appAmountStr);
                var totalYtdCcExpStr = totalYtdCcExp.HasValue ? totalYtdCcExp.Value.Format() : "N/A";
                pdfTabYtdSummary.AddCell(totalYtdCcExpStr);
                var totalExpPerc = ((totalYtdCcExp.HasValue ? totalYtdCcExp.Value : 0) / (appAmount.HasValue && appAmount.Value > 0 ? appAmount.Value : 1)).FormatPercentage();
                pdfTabYtdSummary.AddCell(totalExpPerc);

                int totalUniqueClients = 0;
                using (var db = new ccEntities())
                {
                    var subReportsIds = detailsModel.Select(f => f.Id).ToList();
                    var subReports = from sr in db.SubReports
                                     where subReportsIds.Contains(sr.Id)
                                     select sr;
                    totalUniqueClients = subReports.SelectMany(f => f.ClientReports).Select(f => f.ClientId)
                                            .Union(subReports.SelectMany(f => f.EmergencyReports).Select(f => f.ClientId))
                                            .Union(subReports.SelectMany(f => f.SupportiveCommunitiesReports).Select(f => f.ClientId))
                                            .Union(subReports.SelectMany(f => f.DaysCentersReports).Select(f => f.ClientId))
                                            .Union(subReports.SelectMany(f => f.SoupKitchensReports).Select(f => f.ClientId))
                                            .Distinct().Count();

                    pdfTabYtdSummary.AddCell(totalUniqueClients.ToString());
                }


                PdfPTable pdfTabAgencyRemarks = new PdfPTable(1);
                pdfTabAgencyRemarks.HorizontalAlignment = 1;
                pdfTabAgencyRemarks.SpacingBefore = 5f;
                pdfTabAgencyRemarks.SpacingAfter = 5f;
                pdfTabAgencyRemarks.WidthPercentage = widthPercent;
                PdfPCell headerCellAgencyRemarks = new PdfPCell(new Phrase("Agency's Remarks"));
                headerCellAgencyRemarks.Border = 0;
                headerCellAgencyRemarks.HorizontalAlignment = 1;
                pdfTabAgencyRemarks.AddCell(headerCellAgencyRemarks);
                PdfPTable pdfTabPoFile = new PdfPTable(1);
                pdfTabPoFile.HorizontalAlignment = 1;
                pdfTabPoFile.SpacingBefore = 5f;
                pdfTabPoFile.SpacingAfter = 5f;
                pdfTabPoFile.WidthPercentage = widthPercent;
                PdfPCell headerCellPoFile = new PdfPCell(new Phrase("Program Overview Document Content"));
                headerCellPoFile.Border = 0;
                headerCellPoFile.HorizontalAlignment = 1;
                pdfTabPoFile.AddCell(headerCellPoFile);
                PdfPTable pdfTabMhsaFile = new PdfPTable(1);
                pdfTabMhsaFile.HorizontalAlignment = 1;
                pdfTabMhsaFile.SpacingBefore = 5f;
                pdfTabMhsaFile.SpacingAfter = 5f;
                pdfTabMhsaFile.WidthPercentage = widthPercent;
                PdfPCell headerCellMhsaFile = new PdfPCell(new Phrase("Holocaust Survivor Advisory Committee Minutes Document Content"));
                headerCellMhsaFile.Border = 0;
                headerCellMhsaFile.HorizontalAlignment = 1;
                pdfTabMhsaFile.AddCell(headerCellMhsaFile);
                foreach (var item in financialReportDetails.AgencyComments)
                {
                    if (!item.IsFile)
                    {
                        var noHtml = Regex.Replace(item.Content, @"<[^>]+>|&nbsp;", string.Empty).Trim();
                        noHtml = Regex.Replace(noHtml, "&rsquo;", "'");
                        noHtml = Regex.Replace(noHtml, "&rdquo;", "\"");
                        noHtml = Regex.Replace(noHtml, "&ldquo;", "\"");
                        pdfTabAgencyRemarks.AddCell(item.Date.ToString() + ", " + item.Username + " : " + noHtml);
                    }
                }
                bool hasPoFile = false;
                bool hasMhsaFile = false;
                if (financialReportDetails.ProgramOverviewFileName != null && System.IO.Path.GetExtension(financialReportDetails.ProgramOverviewFileName) == ".pdf")
                {
                    hasPoFile = true;
                    pdfTabAgencyRemarks.AddCell("Program Overview Document Name: " + financialReportDetails.ProgramOverviewFileName);
                    pdfTabPoFile.AddCell(ReadPdfFile(programOverviewFileAbsolutePath(financialReportDetails.Id)));
                }
                if (financialReportDetails.MhsaFileName != null && System.IO.Path.GetExtension(financialReportDetails.MhsaFileName) == ".pdf")
                {
                    hasMhsaFile = true;
                    pdfTabAgencyRemarks.AddCell("Holocaust Survivor Advisory Committee Minutes Document Name: " + financialReportDetails.MhsaFileName);
                    pdfTabMhsaFile.AddCell(ReadPdfFile(mhsaFileAbsolutePath(financialReportDetails.Id)));
                }

                PdfPTable pdfTabPoRemarks = new PdfPTable(1);
                pdfTabPoRemarks.HorizontalAlignment = 1;
                pdfTabPoRemarks.SpacingBefore = 5f;
                pdfTabPoRemarks.SpacingAfter = 5f;
                pdfTabPoRemarks.WidthPercentage = widthPercent;
                PdfPCell headerCellPoRemarks = new PdfPCell(new Phrase("PO's Remarks"));
                headerCellPoRemarks.Border = 0;
                headerCellPoRemarks.HorizontalAlignment = 1;
                pdfTabPoRemarks.AddCell(headerCellPoRemarks);
                foreach (var item in financialReportDetails.PoComments)
                {
                    var noHtml = Regex.Replace(item.Content, @"<[^>]+>|&nbsp;", string.Empty).Trim();
                    noHtml = Regex.Replace(noHtml, "&rsquo;", "'");
                    noHtml = Regex.Replace(noHtml, "&rdquo;", "\"");
                    noHtml = Regex.Replace(noHtml, "&ldquo;", "\"");
                    pdfTabPoRemarks.AddCell(item.Date.ToString() + ", " + item.Username + " : " + noHtml);
                }
                doc.Open();
                doc.Add(pdfTabFinancialDetails);
                doc.Add(pdfTabDetails);
                doc.NewPage();
                doc.Add(pdfTabYtdSummary);
                doc.NewPage();
                doc.Add(pdfTabAppTotals);
                doc.Add(pdfTabAgencyRemarks);
                doc.Add(pdfTabPoRemarks);
                doc.NewPage();
                foreach (var t in subReportsTables)
                {
                    doc.Add(t.Key);
                    if (t.Value)
                    {
                        doc.NewPage();
                    }
                }
                doc.NewPage();
                foreach (var t in clientEventsCountSubReportTables)
                {
                    doc.Add(t.Key);
                    if (t.Value)
                    {
                        doc.NewPage();
                    }
                }
                doc.NewPage();
                doc.Add(pdfTabStatusHistory);
                if (hasPoFile)
                {
                    doc.Add(pdfTabPoFile);
                }
                if (hasMhsaFile)
                {
                    doc.Add(pdfTabMhsaFile);
                }
                doc.Close();

                byte[] content = System.IO.File.ReadAllBytes(filePath);
                reportInPdf = new ReportInPdf()
                {
                    MainReportDetailsModel = financialReportDetails,
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
        //fluxx 
        string GetReportName(ReportInPdf content, bool isFullName = false)
        {

            var id = content.MainReportDetailsModel.Id;
            var app_id = content.MainReportDetailsModel.AppId;
            var agencyGroupId = content.MainReportDetailsModel.AgencyGroupId;
            var end_date = content.MainReportDetailsModel.End.ToString("yyyy_MM");

            var uri = System.Configuration.ConfigurationManager.AppSettings["MainReportUri"];
            if (string.IsNullOrEmpty(uri))
            {
                throw new Exception("Missing path for main reports folder \"MainReportUri\"at config");
            }

            var template = System.Configuration.ConfigurationManager.AppSettings["MainReportTemplate"];
            if (string.IsNullOrEmpty(template))
            {
                template = @"{0}\OrgGroupID_{1}\AppID_{2}\ReportID_{3}_EndDate_{4}.pdf";
                throw new Exception("Missing  template for main reports folder \"MainReportTemplate\" at config");
            }

            var diamand_report_name = isFullName ?
                string.Format(template, uri, agencyGroupId, app_id, id, end_date) :
                string.Format(template, "", agencyGroupId, app_id, id, end_date);

            return diamand_report_name;
        }


        //fluxx, save report under different window user
        [CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer, FixedRoles.RegionOfficer, FixedRoles.RegionAssistant, FixedRoles.GlobalReadOnly, FixedRoles.RegionReadOnly, FixedRoles.AuditorReadOnly)]
        public string CreateAndSaveMainReportDocument(int id)
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
                                 " ReportId: " + id, ex);
            }
            return diamand_report_name;
        }

        [CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer, FixedRoles.RegionOfficer, FixedRoles.RegionAssistant, FixedRoles.GlobalReadOnly, FixedRoles.RegionReadOnly, FixedRoles.AuditorReadOnly)]
        public void ExportToPdf(int id, bool ifCtera = false)
        {
            try
            {
                var content = GetPDF(id);
                string fileName = "FluxxExport_" + id + ".pdf";
                //fluxx, ctera
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
                        _log.Error("Update Pdf. ReportId: " + id, ex_ctera);
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


        public class hcCapPdfRow
        {
            public int ClientId { get; set; }
            public decimal? HcCap { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public bool AfterReportStart { get; set; }
        }

        public class ReportsRow
        {
            public int? Id { get; set; }
            public int ClientId { get; set; }
            public string ClientFirstName { get; set; }
            public string ClientLastName { get; set; }
            public string ClientApprovalStatus { get; set; }
            public DateTime? ReportDate { get; set; }
            public string TypeName { get; set; }
            public string Remarks { get; set; }
            public decimal? Amount { get; set; }
            public decimal? Discretionary { get; set; }
            public decimal? Total { get; set; }
            public string UniqueCircumstances { get; set; }
            public decimal? Quantity { get; set; }
            public bool Reported { get; set; }
            public string IsraelId { get; set; }
            public decimal? HoursHoldCost { get; set; }
            public int? MonthsCount { get; set; }
            public int? SubsidesByDcc { get; set; }
            public decimal? VisitCost { get; set; }
            public int? VisitCount { get; set; }
            public decimal? HcCaps { get; set; }
            public DateTime JoinDate { get; set; }
            public int? JNVCount { get; set; }
            public int? TotalCount { get; set; }
        }

        [HttpPost]
        [CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
        public ActionResult Update(MainReport input)
        {
            db.ContextOptions.LazyLoadingEnabled = false;
            db.ContextOptions.ProxyCreationEnabled = false;
            db.CommandTimeout = 400;

            var mr = db.MainReports.Where(this.Permissions.MainReportsFilter)
                .Include(f => f.AppBudget.App.AgencyGroup)
                .SingleOrDefault(f => f.Id == input.Id);
            if (!AppBudget.ValidStatuses.Contains(mr.AppBudget.ApprovalStatus))
            {
                ModelState.AddModelError(string.Empty, "The mainReport can't be updated until the Budget is approved");
            }
            else
            {

                mr.ExcRate = input.ExcRate;
                mr.ExcRateSource = input.ExcRateSource;

                MainReport.Statuses oldStatus = mr.Status;
                if (this.CcUser.RoleId == (int)FixedRoles.Admin)
                {
                    mr.StatusId = input.StatusId;
                }
                switch (mr.Status)
                {
                    case MainReport.Statuses.Approved:
                        mr.ApprovedAt = DateTime.Now;
                        mr.ApprovedById = this.CcUser.Id;
                        if (input.SendEmail)
                        {
                            SendEmailNotification(mr);
                        }
                        try
                        {
                            CreateAndSaveMainReportDocument(input.Id);

                        }
                        catch (Exception ex)
                        {
                            _log.Error("Approve, Saving report in pdf:", ex);
                        }

                        break;
                    case MainReport.Statuses.AwaitingProgramAssistantApproval:
                        mr.SubmittedAt = DateTime.Now;
                        mr.SubmittedById = this.CcUser.Id;
                        break;
                }
                mr.UpdatedAt = DateTime.Now;
                mr.UpdatedById = this.CcUser.Id;

                ModelState.Clear();
                TryValidateModel(mr);
            }
            if (ModelState.IsValid)
            {
                try
                {
                    db.CommandTimeout = 10000;//LF
                    db.SaveChanges();
                    AddOrUpdateMainReportAtFluxx(mr);
                }
                catch (System.Data.UpdateException ex)
                {
                    _log.Error(ex);
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            if (ModelState.IsValid)
            {


                return this.RedirectToAction(f => f.Details(mr.Id));
            }
            else
            {
                return this.Details(input.Id);
            }



        }

        [HttpPost]
        [CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer)]
        public ActionResult SetAdjusted(int id)
        {
            return UpdateAdjusted(id, true);
        }

        [HttpPost]
        [CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer)]
        public ActionResult UnsetAdjusted(int id)
        {
            return UpdateAdjusted(id, false);
        }

        public ActionResult UpdateAdjusted(int id, bool adjusted)
        {
            var mr = db.MainReports.Where(this.Permissions.MainReportsFilter).SingleOrDefault(f => f.Id == id);
            if (mr != null)
            {
                mr.Adjusted = adjusted;
                mr.UpdatedAt = DateTime.Now;
                mr.UpdatedById = this.CcUser.Id;
                mr.PoComments.Add(new Comment()
                {
                    Content = string.Format("The report was {0} as Adjusted", adjusted ? "Set" : "Unset"),
                    Date = DateTime.Now,
                    UserId = this.CcUser.Id
                });
                db.SaveChanges();
                return this.RedirectToAction(f => f.Details(mr.Id));
            }
            return Content("mainreport not found");
        }

        [HttpGet]
        public ActionResult AgencyRemarks(int id)
        {
            var model = new MainReportAgencyCommentsModel();
            model.Load(db, Permissions, id);
            return View("Remarks", model);
        }

        [HttpPost]
        [CcAuthorize(FixedRoles.AgencyUser, FixedRoles.Ser, FixedRoles.Admin, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
        [ValidateInput(false)]
        public ActionResult AgencyRemarks(MainReportAgencyCommentsModel model)
        {
            var mainRep = db.MainReports.Where(this.Permissions.MainReportsFilter).Single(f => f.Id == model.Id);

            if (!MainReport.EditableStatuses.Contains(mainRep.Status))
            {
                throw new Exception("Remarks can be added only if main report status is \"New\" or \"Returned to Agency\".");
            }

            if (string.IsNullOrEmpty(model.NewComment))
            {
                model.Load(db, Permissions, model.Id);
                ModelState.Clear();
                ModelState.AddModelError("", "The New Comment field is required");
                return View("Remarks", model);
            }

            var newComment = new Comment()
            {
                UserId = this.CcUser.Id,
                Date = DateTime.Now,
                Content = model.NewComment
            };

            mainRep.AgencyComments.Add(newComment);

            db.SaveChanges();

            return this.RedirectToAction(f => f.AgencyRemarks(mainRep.Id));
        }

        public ActionResult PoRemarks(int id)
        {
            var model = new MainReportPoCommentsModel();
            model.NewComment = null;
            model.Load(db, Permissions, id);
            return View("Remarks", model);
        }

        [HttpPost]
        [CcAuthorize(FixedRoles.Admin, FixedRoles.RegionOfficer, FixedRoles.RegionAssistant, FixedRoles.GlobalOfficer)]
        [ValidateInput(false)]
        public ActionResult PoRemarks(MainReportAgencyCommentsModel model)
        {
            var mainRep = db.MainReports.Where(this.Permissions.MainReportsFilter).Single(f => f.Id == model.Id);



            ModelState.Clear();
            if (string.IsNullOrEmpty(model.NewComment))
            {
                model.Load(db, Permissions, model.Id);
                ModelState.AddModelError("", "The New Comment field is required");
                return View("Remarks", model);
            }

            switch ((FixedRoles)this.Permissions.User.RoleId)
            {
                case FixedRoles.Admin:
                    break;
                case FixedRoles.RegionOfficer:
                    if (mainRep.Status != MainReport.Statuses.AwaitingProgramOfficerApproval)
                    {
                        ModelState.AddModelError(string.Empty, "PO Remarks can be added by Program Officer and if main report status is \"Awaiting Program Officer approval\".");
                    }
                    break;
                case FixedRoles.RegionAssistant:
                    if (mainRep.Status != MainReport.Statuses.AwaitingProgramAssistantApproval)
                    {
                        ModelState.AddModelError(string.Empty, "PO Remarks can be added by Regional Assistant and if main report status is \"Awaiting Program Assistant approval\".");
                    }
                    break;
                case FixedRoles.GlobalOfficer:
                    if (mainRep.Status != MainReport.Statuses.AwaitingProgramOfficerApproval || mainRep.Status == MainReport.Statuses.AwaitingProgramAssistantApproval)
                    {
                        ModelState.AddModelError(string.Empty,
                            "PO Remarks can be added by Global Officer if main report status is \"Awaiting Program Assistant approval\" or \"Awaiting Program Officer approval\".");
                    }
                    break;
                default:
                    ModelState.AddModelError(string.Empty, "PO Remarks can not be added.");
                    break;
            }

            if (ModelState.IsValid)
            {

                var newComment = new Comment()
                {
                    UserId = this.CcUser.Id,
                    Date = DateTime.Now,
                    Content = model.NewComment
                };

                mainRep.PoComments.Add(newComment);

                db.SaveChanges();

                return this.RedirectToAction(f => f.PoRemarks(mainRep.Id));
            }
            else
            {
                throw new InvalidOperationException(ModelState.ValidationErrorMessages().FirstOrDefault());
            }
        }

        [HttpGet]
        [CcAuthorize(FixedRoles.Admin, FixedRoles.RegionOfficer, FixedRoles.RegionAssistant, FixedRoles.GlobalOfficer)]
        public ActionResult InternalRemarks(int id)
        {
            var model = new MainReportInternalCommentsModel();
            model.NewComment = null;
            model.Load(db, Permissions, id);
            return View("Remarks", model);
        }
        [HttpPost]
        [CcAuthorize(FixedRoles.Admin, FixedRoles.RegionOfficer, FixedRoles.RegionAssistant, FixedRoles.GlobalOfficer)]
        [ValidateInput(false)]
        public ActionResult InternalRemarks(MainReportInternalCommentsModel model)
        {
            var mainRep = db.MainReports.Where(this.Permissions.MainReportsFilter).Single(f => f.Id == model.Id);

            ModelState.Clear();
            if (string.IsNullOrEmpty(model.NewComment))
            {
                model.Load(db, Permissions, model.Id);
                ModelState.AddModelError("", "The New Comment field is required");
                return View("Remarks", model);
            }

            if (this.Permissions.CanAccessInternalRemarks)
            {
                var newComment = new Comment()
                {
                    UserId = this.CcUser.Id,
                    Date = DateTime.Now,
                    Content = model.NewComment
                };

                mainRep.InternalComments.Add(newComment);

                db.SaveChanges();

                return this.RedirectToAction(f => f.InternalRemarks(mainRep.Id));
            }
            else
            {
                throw new Exception("PO Remarks can be added by Global or PO Officer and if main report status is \"Awaiting Program Officer approval\".");
            }
        }

        [HttpGet]
        public ActionResult PostApprovalRemarks(int id)
        {
            var model = new MainReportPostApprovalCommentsModel();
            model.NewComment = null;
            model.Load(db, Permissions, id);
            return View("Remarks", model);
        }
        [HttpPost]
        [ValidateInput(false)]
        [CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer, FixedRoles.RegionAssistant, FixedRoles.RegionOfficer)]
        public ActionResult PostApprovalRemarks(MainReportPostApprovalCommentsModel model)
        {
            var mainRep = db.MainReports.Where(this.Permissions.MainReportsFilter).Single(f => f.Id == model.Id);


            ModelState.Clear();
            if (string.IsNullOrEmpty(model.NewComment))
            {
                model.Load(db, Permissions, model.Id);
                ModelState.AddModelError("", "The New Comment field is required");
                return View("Remarks", model);
            }
            var now = DateTime.Now;
            if (Request.Files.Count > 0)
            {
                for (var i = 0; i < Request.Files.Count; i++)
                {
                    var file = Request.Files[i];
                    if (file.ContentLength == 0)
                    {
                        continue;
                    }
                    if (string.IsNullOrEmpty(file.FileName))
                    {
                        continue;
                    }
                    var entity = new Comment
                    {
                        UserId = this.CcUser.Id,
                        Date = now,
                        Content = file.FileName,
                        IsFile = true,
                    };
                    mainRep.PostApprovalComments.Add(entity);
                    db.SaveChanges();
                    SavePostApprovalCommentFile(mainRep.Id, entity.Id, file);
                }
            }
            var newComment = new Comment()
            {
                UserId = this.CcUser.Id,
                Date = now,
                Content = model.NewComment,
                PostApprovalComment = model.PostApprovalComment
            };

            mainRep.PostApprovalComments.Add(newComment);
            db.SaveChanges();

            return this.RedirectToAction(f => f.PostApprovalRemarks(mainRep.Id));
        }

        [HttpGet]
        public FileResult PostApprovalCommentFile(int id)
        {

            var mainReports = db.MainReports.Where(this.Permissions.MainReportsFilter);
            var q = from m in mainReports
                    from c in m.PostApprovalComments
                    where c.Id == id
                    where c.IsFile
                    select new
                    {
                        FileName = c.Content,
                        CommentId = c.Id,
                        MainReportId = m.Id
                    };
            var comment = q.FirstOrDefault();
            var path = this.PostApprovalCommentFilePath(comment.MainReportId, comment.CommentId);
            return this.File(path, "application/octet-stream", comment.FileName);
        }


        [HttpGet]
        [CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.SerAndReviewer)]
        public ActionResult Submit(int id)
        {
            var model = new MainReportSubmissionScreenModel() { Id = id };

            model.mainReportsRepository = new GenericRepository<MainReport>(db);

            try
            {
                db.CommandTimeout = 10000;
                model.LoadData();
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                throw;
            }


            return View("Submit", model);
        }

        [HttpPost]
        [ConfirmPassword()]
        [CcAuthorize(FixedRoles.Ser, FixedRoles.Admin, FixedRoles.SerAndReviewer)]
        public ActionResult Submit(SubmissionDetails input, HttpPostedFileBase programOverviewFile, HttpPostedFileBase mhsaFile)
        {
            var newStatus = MainReport.Statuses.AwaitingProgramAssistantApproval;
            var mainReport = db.MainReports.Where(Permissions.MainReportsFilter).Single(f => f.Id == input.Id);
            var model = new MainReportSubmissionScreenModel() { Id = input.Id, Ser = input };
           
            model.mainReportsRepository = new GenericRepository<MainReport>(db);
            model.LoadData();
            var b = ModelState.IsValid;
            model.Ser.Remarks = input.Remarks;
            model.Ser.AcMeetingHeld = input.AcMeetingHeld;
            model.Ser.Mhsa = input.Mhsa;
            model.Ser.ProgramOverview = input.ProgramOverview;
            model.Ser.Remarks = input.Remarks;
            model.Ser.AdministrativeOverheadOverflowReason = input.AdministrativeOverheadOverflowReason;
            if (programOverviewFile != null && System.IO.Path.GetExtension(programOverviewFile.FileName) != ".pdf")
            {
                ModelState.AddModelError(string.Empty, "The Program Overview file must be only of type pdf");
            }
            else if (programOverviewFile != null)
            {
                model.Ser.ProgramOverviewFileName = programOverviewFile.FileName;
            }
            if (mhsaFile != null && System.IO.Path.GetExtension(mhsaFile.FileName) != ".pdf")
            {
                ModelState.AddModelError(string.Empty, "The Holocaust Survivor Advisory Committee Minutes file must be only of type pdf");
            }
            else if (mhsaFile != null)
            {
                model.Ser.MhsaFileName = mhsaFile.FileName;
            }

            if (model.Ser.AcMeetingHeld)
            {
                if ((model.Ser.Mhsa == null || model.Ser.Mhsa.IsNullOrEmptyHtml()) && string.IsNullOrEmpty(model.Ser.MhsaFileName) && string.IsNullOrEmpty(model.Ser.MhsaUploadedFile))
                {
                    ModelState.AddModelError(string.Empty, "Holocaust Survivor Advisory Committee Minutes is required, or upload a relevant document");
                }
            }

            if (model.Ser.ProgramOverviewRequired)
            {
                if ((model.Ser.ProgramOverview == null || model.Ser.ProgramOverview.IsNullOrEmptyHtml()) && string.IsNullOrEmpty(model.Ser.ProgramOverviewFileName) && string.IsNullOrEmpty(model.Ser.ProgramOverviewUploadedFile))
                {
                    ModelState.AddModelError(string.Empty, "The program overview is required field, or upload a relevant document");
                }
            }

            
            foreach (var row in model.Errors)
            {
                ModelState.AddModelError(string.Empty, row.Message);
            }

            string internalRemarks = "";
            if (model.Warnings.Any(f => f.MessageTypeName.Contains("Maximum Admin YTD") || f.MessageTypeName.Contains("Maximum non homecare YTD")))
            {
                foreach (var msg in model.Warnings.Where(f => f.MessageTypeName.Contains("Maximum Admin YTD")))
                {
                    internalRemarks += " " + msg.Message;
                }
                foreach (var msg in model.Warnings.Where(f => f.MessageTypeName.Contains("Maximum non homecare YTD")))
                {
                    internalRemarks += " " + msg.Message;
                }
            }

            if (model.Warnings.Any(f => !f.MessageTypeName.Contains("Maximum Admin YTD") && !f.MessageTypeName.Contains("Maximum non homecare YTD")) && string.IsNullOrEmpty(input.Remarks))
            {
                ModelState.AddModelError(string.Empty, "The remarks are missing.");
            }

           


            if (ModelState.IsValid)
            {
                if (newStatus != mainReport.Status)
                {
                    var prevStatus = mainReport.Status;
                    mainReport.Status = newStatus;


                    mainReport.SubmittedById = this.CcUser.Id;
                    mainReport.SubmittedAt = mainReport.UpdatedAt = DateTime.Now;
                    mainReport.AcMeetingHeld = input.AcMeetingHeld;
                    mainReport.Mhsa = input.Mhsa;
                    mainReport.MhsaFileName = model.Ser.MhsaFileName ?? input.MhsaUploadedFile;
                    mainReport.UpdatedById = this.CcUser.Id;
                    mainReport.ProgramOverviewFileName = model.Ser.ProgramOverviewFileName ?? input.ProgramOverviewUploadedFile;

                    if (mainReport.Status == MainReport.Statuses.AwaitingProgramAssistantApproval)
                    {
                        var duplicateErrors = db.spValidateMrHcD(mainReport.Id).ToList();
                        if (duplicateErrors.Any())
                        {
                            var str = "Duplicated clients exceeding cap were found on this report: ";
                            str += string.Join(" | ", duplicateErrors.Select(f => string.Format("{0} ({1})", f.ClientId, f.ServiceName)));
                            mainReport.InternalComments.Add(new Comment
                            {
                                Content = str,
                                UserId = this.CcUser.Id,
                                Date = mainReport.UpdatedAt
                            });
                        }
                        if (db.SubReports.Where(f => f.MainReportId == mainReport.Id).Any(f => f.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.HomecareWeekly))
                        {
                            var subReports = db.SubReports.Where(f => f.MainReportId == mainReport.Id);
                            var appAgency = (from sr in db.SubReports
                                             where sr.MainReportId == mainReport.Id
                                             select new
                                             {
                                                 AgencyId = sr.AppBudgetService.AgencyId,
                                                 AppId = sr.AppBudgetService.AppBudget.AppId,
                                                 AgencyGroupId = sr.AppBudgetService.Agency.GroupId
                                             }).FirstOrDefault();
                            var startingWeek = mainReport.Start;
                            DayOfWeek selectedDOW = startingWeek.DayOfWeek;
                            int? selectedDowDb = GlobalHelper.GetWeekStartDay(appAgency.AgencyGroupId, appAgency.AppId);
                            if (selectedDowDb.HasValue)
                            {
                                selectedDOW = (DayOfWeek)selectedDowDb.Value;
                                if (startingWeek.Month > 1)
                                {
                                    var diff = selectedDowDb.Value - (int)startingWeek.DayOfWeek;
                                    startingWeek = startingWeek.AddDays((double)(diff));
                                    if (startingWeek > mainReport.Start)
                                    {
                                        startingWeek = startingWeek.AddDays(-7);
                                    }
                                }
                            }
                            var dayToSubstruct = (int)selectedDOW + 1;
                            var digits = CCDecimals.GetDecimalDigits();
                            var hvv = (from a in
                                          (from sr in subReports
                                           join cr in db.ClientReports on sr.Id equals cr.SubReportId
                                           where sr.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.HomecareWeekly
                                           where !sr.AppBudgetService.Service.ExceptionalHomeCareHours
                                           from ar in cr.ClientAmountReports
                                           group ar by new { cr.ClientId, ar.ReportDate, MasterId = cr.Client.MasterId ?? cr.Client.Id } into arg
                                           select new { ClientId = arg.Key.ClientId, arg.Key.MasterId, ReportDate = arg.Key.ReportDate, Quantity = arg.Sum(f => f.Quantity) })
                                       join cd in
                                           (from cr in db.ClientReports
                                            join c in db.Clients on cr.ClientId equals c.Id
                                            where c.MasterId != null && c.Id != c.MasterId
                                            where !cr.SubReport.AppBudgetService.Service.ExceptionalHomeCareHours
                                            from ar in cr.ClientAmountReports
                                            where cr.SubReport.MainReport.Start < mainReport.End && cr.SubReport.MainReport.End > mainReport.Start
                                            where cr.SubReport.MainReport.StatusId == (int)MainReport.Statuses.Approved
                                                           || cr.SubReport.MainReport.StatusId == (int)MainReport.Statuses.AwaitingProgramAssistantApproval
                                                           || cr.SubReport.MainReport.StatusId == (int)MainReport.Statuses.AwaitingProgramOfficerApproval
                                                           || cr.SubReport.MainReport.StatusId == (int)MainReport.Statuses.AwaitingAgencyResponse
                                            group ar by new { ClientId = c.Id, MasterId = c.MasterId ?? c.Id, ar.ReportDate } into arg
                                            select new { ClientId = arg.Key.ClientId, MasterId = arg.Key.MasterId, ReportDate = arg.Key.ReportDate, Quantity = arg.Sum(f => f.Quantity) })
                                             on new
                                             {
                                                 MasterId = a.MasterId,
                                                 ReportWeek = SqlFunctions.DatePart("week", SqlFunctions.DateAdd("day", SqlFunctions.DatePart("weekday", a.ReportDate) - dayToSubstruct < 0 ?
                                                     (SqlFunctions.DatePart("weekday", a.ReportDate) - dayToSubstruct + 7) * -1 : (SqlFunctions.DatePart("weekday", a.ReportDate) - dayToSubstruct) * -1, a.ReportDate))
                                             } equals
                                             new
                                             {
                                                 MasterId = (int)cd.MasterId,
                                                 ReportWeek = SqlFunctions.DatePart("week", SqlFunctions.DateAdd("day", SqlFunctions.DatePart("weekday", cd.ReportDate) - dayToSubstruct < 0 ?
                                                         (SqlFunctions.DatePart("weekday", cd.ReportDate) - dayToSubstruct + 7) * -1 : (SqlFunctions.DatePart("weekday", cd.ReportDate) - dayToSubstruct) * -1, cd.ReportDate))
                                             }
                                       where a.ClientId != cd.ClientId
                                       let clientLeaveDate = db.Clients.FirstOrDefault(f => f.Id == a.ClientId).LeaveDate
                                       let clientReportDate = startingWeek.Month > 1 && startingWeek < mainReport.Start && a.ReportDate == mainReport.Start ? startingWeek : a.ReportDate
                                       let hcap = ccEntitiesExtensions.HcCapLeaveDate(a.ClientId, clientReportDate, ccEntitiesExtensions.AddDays(clientReportDate, 7)) * 7
                                       let q = mainReport.Start.Month > 1 && startingWeek < mainReport.Start && a.ReportDate == mainReport.Start ?
                                           (decimal?)(from cr in db.ClientReports
                                                      where cr.ClientId == a.ClientId && cr.SubReport.AppBudgetService.AppBudget.AppId == appAgency.AppId
                                                             && cr.SubReport.AppBudgetService.AgencyId == appAgency.AgencyId
                                                             && !subReports.Any(f => f.Id == cr.SubReportId)
                                                             && cr.SubReport.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.HomecareWeekly
                                                             && !cr.SubReport.AppBudgetService.Service.ExceptionalHomeCareHours
                                                      from ar in cr.ClientAmountReports
                                                      where ar.ReportDate == startingWeek
                                                      select ar.Quantity).Sum() : 0
                                       let hcapRounded = hcap != null ? Math.Round((decimal)hcap, digits) : 0
                                       let quantityRounded = Math.Round(a.Quantity + cd.Quantity + (q ?? 0), digits)
                                       where hcapRounded < quantityRounded
                                       select new { a.ClientId }).Distinct();
                            if (hvv.Any())
                            {
                                var str = "Duplicated clients exceeding cap were found on this report: ";
                                str += string.Join(" | ", hvv.Select(f => f.ClientId));
                                mainReport.InternalComments.Add(new Comment
                                {
                                    Content = str,
                                    UserId = this.CcUser.Id,
                                    Date = mainReport.UpdatedAt
                                });
                            }
                        }
                    }

                    if (!System.StringExtenstions.IsNullOrEmptyHtml(input.Remarks))
                    {
                        mainReport.AgencyComments.Add(new Comment()
                        {
                            UserId = this.CcUser.Id,
                            Date = mainReport.UpdatedAt,
                            Content = input.Remarks,
                            IsFile = false
                        });
                    }
                    if (!System.StringExtenstions.IsNullOrEmptyHtml(input.Mhsa))
                    {
                        mainReport.AgencyComments.Add(new Comment()
                        {
                            UserId = this.CcUser.Id,
                            Date = mainReport.UpdatedAt,
                            Content = "*** Minutes: " + input.Mhsa,
                            IsFile = false
                        });
                    }
                    if (!string.IsNullOrEmpty(model.Ser.MhsaFileName))
                    {
                        mainReport.AgencyComments.Add(new Comment()
                        {
                            UserId = this.CcUser.Id,
                            Date = mainReport.UpdatedAt,
                            Content = "*** Minutes: " + model.Ser.MhsaFileName,
                            IsFile = true
                        });
                    }
                    if (!System.StringExtenstions.IsNullOrEmptyHtml(input.AdministrativeOverheadOverflowReason))
                    {
                        mainReport.AgencyComments.Add(new Comment()
                        {
                            UserId = this.CcUser.Id,
                            Date = mainReport.UpdatedAt,
                            Content = "*** Administrative Overhead: " + input.AdministrativeOverheadOverflowReason,
                            IsFile = false
                        });
                    }

                    mainReport.ProgramOverview = input.ProgramOverview;
                    if (!System.StringExtenstions.IsNullOrEmptyHtml(mainReport.ProgramOverview))
                    {
                        mainReport.AgencyComments.Add(new Comment
                        {
                            UserId = this.CcUser.Id,
                            Date = mainReport.UpdatedAt,
                            Content = "*** Program Overview:" + mainReport.ProgramOverview,
                            IsFile = false
                        });
                    }
                    if (!string.IsNullOrEmpty(model.Ser.ProgramOverviewFileName))
                    {
                        mainReport.AgencyComments.Add(new Comment
                        {
                            UserId = this.CcUser.Id,
                            Date = mainReport.UpdatedAt,
                            Content = "*** Program Overview: " + model.Ser.ProgramOverviewFileName,
                            IsFile = true
                        });
                    }
                    if (!string.IsNullOrEmpty(internalRemarks))
                    {
                        mainReport.InternalComments.Add(new Comment
                        {
                            UserId = this.CcUser.Id,
                            Date = mainReport.UpdatedAt,
                            Content = internalRemarks,
                            IsFile = false
                        });
                    }

                    mainReport.C168OK = null;

                    TryValidateModel(mainReport);

                    if (ModelState.IsValid)
                    {
                        try
                        {
                            if (programOverviewFile != null)
                            {
                                SaveProgramOverviewFile(mainReport.Id, programOverviewFile);
                            }
                            if (mhsaFile != null)
                            {
                                SaveMhsaFile(mainReport.Id, mhsaFile);
                            }
                            db.CommandTimeout = 10000;
                            var rowsUpdated = db.SaveChanges();
                            SendYTDHcExceeding(mainReport.Id, true);
                        }
                        catch (System.Data.UpdateException ex)
                        {
                            _log.Error(null, ex);
                            ModelState.AddModelError(string.Empty, ex.Message);
                            if (programOverviewFile != null)
                            {
                                DeleteProgramOverviewFile(mainReport.Id);
                            }
                            if (mhsaFile != null)
                            {
                                DeleteMhsaFile(mainReport.Id);
                            }
                        }
                    }


                }
                else
                {
                    ModelState.AddModelError(string.Empty, "the report is already submitted");
                }
            }
            if (ModelState.IsValid)
            {
                return this.RedirectToAction(f => f.Details(mainReport.Id));
            }
            else
            {
                return View(model);
            }


        }

        [HttpPost]
        [ConfirmPassword()]
        [CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer, FixedRoles.RegionOfficer, FixedRoles.RegionAssistant)]
        public ActionResult Approve(MainReportApproveModel input)
        {
            return ChangeStatus(input, true);
        }

        [HttpPost]
        [ConfirmPassword()]
        [CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer, FixedRoles.RegionOfficer, FixedRoles.RegionAssistant)]
        public ActionResult Reject(MainReportApproveModel input)
        {
            input.MainReportStatus = MainReport.Statuses.Rejected;
            return ChangeStatus(input, false);
        }

        [HttpPost]
        [ConfirmPassword()]
        [CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer, FixedRoles.RegionOfficer, FixedRoles.RegionAssistant)]
        public ActionResult AwaitingAgencyResponse(MainReportApproveModel input)
        {
            input.MainReportStatus = MainReport.Statuses.AwaitingAgencyResponse;
            return ChangeStatus(input, false);
        }

        [HttpPost]
        [ConfirmPassword()]
        [CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer, FixedRoles.RegionOfficer, FixedRoles.RegionAssistant)]
        public ActionResult ChangeStatusBack(MainReportApproveModel input)
        {
            return ChangeStatus(input, true);
        }

        private ActionResult ChangeStatus(MainReportApproveModel input, bool approve)
        {

            var mainreport = db.MainReports.SingleOrDefault(f => f.Id == input.Id);

            if (mainreport == null)
            {
                //main reports does not exists
                ModelState.AddModelError(string.Empty, string.Format("The main report (id: {0}) can not be found", input.Id));
                return this.Index(new MainReportsListModel());
            }
            else
            {
                var userRole = (FixedRoles)this.CcUser.RoleId;
                MainReport.Statuses? newStatus = null;
                switch (userRole)
                {
                    case FixedRoles.RegionAssistant:
                        if (mainreport.Status == MainReport.Statuses.AwaitingProgramAssistantApproval)
                        {
                            if (approve)
                            {
                                mainreport.LastReport = input.LastReport;
                                newStatus = MainReport.Statuses.AwaitingProgramOfficerApproval;
                            }
                            else if (input.MainReportStatus == MainReport.Statuses.AwaitingAgencyResponse) { newStatus = MainReport.Statuses.AwaitingAgencyResponse; }
                            else { newStatus = MainReport.Statuses.Rejected; }
                        }
                        else if (mainreport.Status == MainReport.Statuses.AwaitingAgencyResponse && approve)
                        {
                            mainreport.LastReport = input.LastReport;
                            newStatus = MainReport.Statuses.AwaitingProgramAssistantApproval;
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                        break;
                    case FixedRoles.GlobalOfficer:
                    case FixedRoles.RegionOfficer:
                    case FixedRoles.Admin:
                        if (mainreport.Status == MainReport.Statuses.AwaitingProgramAssistantApproval)
                        {
                            if (approve)
                            {
                                mainreport.LastReport = input.LastReport;
                                newStatus = MainReport.Statuses.AwaitingProgramOfficerApproval;
                            }
                            else if (input.MainReportStatus == MainReport.Statuses.AwaitingAgencyResponse) { newStatus = MainReport.Statuses.AwaitingAgencyResponse; }
                            else { newStatus = MainReport.Statuses.Rejected; }
                        }
                        else if (mainreport.Status == MainReport.Statuses.AwaitingProgramOfficerApproval)
                        {
                            if (approve)
                            {
                                if (input.LastReport)
                                {
                                    NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;
                                    string amountStr = input.CancellationAmount.HasValue ? input.CancellationAmount.Value.Format() : "N/A";
                                    input.RejectionRemarks = string.Format("The report is approved and marked as last report. The cancellation amount recorded is  {0} {1}", amountStr, input.CurrencyId);
                                }
                                mainreport.LastReport = input.LastReport;
                                newStatus = MainReport.Statuses.Approved;
                            }
                            else if (mainreport.LastReport)
                            {
                                ModelState.AddModelError(string.Empty, "A final report can only be marked upon approval of a report");
                            }
                            else if (input.MainReportStatus == MainReport.Statuses.AwaitingAgencyResponse) { newStatus = MainReport.Statuses.AwaitingAgencyResponse; }
                            else { newStatus = MainReport.Statuses.Rejected; }
                        }
                        else if (mainreport.Status == MainReport.Statuses.AwaitingAgencyResponse && approve)
                        {
                            mainreport.LastReport = input.LastReport;
                            if (input.PrevMainReportStatus.HasValue && input.PrevMainReportStatus.Value == MainReport.Statuses.AwaitingProgramAssistantApproval)
                            {
                                newStatus = MainReport.Statuses.AwaitingProgramAssistantApproval;
                            }
                            else if (input.PrevMainReportStatus.HasValue && input.PrevMainReportStatus.Value == MainReport.Statuses.AwaitingProgramOfficerApproval)
                            {
                                newStatus = MainReport.Statuses.AwaitingProgramOfficerApproval;
                            }
                        }
                        else
                        {
                            if (userRole == FixedRoles.RegionOfficer)
                                ModelState.AddModelError(string.Empty, string.Format("Regional program officer can't change status of the main report in status: {0}", mainreport.Status.ToString()));
                            else
                                ModelState.AddModelError(string.Empty, string.Format("Regional program assistant can't change status of the main report in status: {0}", mainreport.Status.ToString()));
                        }
                        break;

                    default:
                        ModelState.AddModelError(string.Empty, "Only program officer can approve/reject mainreport.");
                        break;
                }

                if (newStatus.HasValue)
                {
                    mainreport.ChangeStatus(newStatus.Value, this.CcUser, input.RejectionRemarks);
                }
                else
                {
                    return Submit(input.Id);
                }

                ModelState.Clear();
                TryValidateModel(mainreport);

                if (ModelState.IsValid)
                {
                    try
                    {
                        var rowsUpdated = db.SaveChanges();
                    }
                    catch (System.Data.UpdateException ex)
                    {
                        _log.Error("Approve:", ex);
                        if (ex.InnerException != null)
                        {
                            ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                        }
                    }
                }

                if (ModelState.IsValid)
                {

                    switch (mainreport.Status)
                    {
                        case MainReport.Statuses.Approved:
                            SendEmailNotification(mainreport);// LenaUncomment
                            //fluxx
                            try
                            {
                                CreateAndSaveMainReportDocument(mainreport.Id);
                            }
                            catch (Exception ex)
                            {
                                _log.Error("Approve, Saving report in pdf:", ex);
                            }
                            break;
                        case MainReport.Statuses.Rejected:
                        case MainReport.Statuses.AwaitingAgencyResponse:
                            SendEmailNotification(mainreport);
                            break;

                    }
                    AddOrUpdateMainReportAtFluxx(mainreport);
                    return this.RedirectToAction(f => f.Details(input.Id));
                }
                else
                {

                    return Submit(input.Id);
                }


            }
        }

        [HttpPost]
       
        [CcAuthorize(FixedRoles.Admin, FixedRoles.RegionOfficer) ]
        public ActionResult UpdateLastReport(MainReportDetailsModel input)
        {
            var mainreport = db.MainReports.SingleOrDefault(f => f.Id == input.Id);
            if (mainreport == null)
            {
                //main reports does not exists
                ModelState.AddModelError(string.Empty, string.Format("The main report (id: {0}) can not be found", input.Id));
                return this.Index(new MainReportsListModel());
            }
            mainreport.LastReport = input.LastReport;
            db.SaveChanges();
            return this.RedirectToAction(f => f.Details(input.Id));
        }

        public string GetMainReportMailText(MainReport mainreport)
        {
            string Header = " <br> ";
            Header += "<br> This is an automated email message, sent from the Claims Conference Reporting System (Diamond). <br><br> ";
            Header += "<br> Please do not reply to this email. <br> ";

            string Footer = "<br> Best Regards, <br><br> Diamond Reporting System";
            string appText = "";
            string appLink = "";
            string currentUrl = Request.UrlReferrer.ToString();
            if (Request.UrlReferrer != null)
            {
                currentUrl = Request.UrlReferrer.ToString();
            }
            string repInfo = GetMainReportInfo(mainreport);
            appLink = string.Format("<a href=\"{0}\">{1}</a> ", currentUrl, "update the report");
            string appLink1 = string.Format("<a href=\"{0}\">{1}</a> ", currentUrl, "reporting");
            if (mainreport.Status == MainReport.Statuses.Approved)
            {
                appText = "<br> We are pleased to inform you, that your " + appLink1 + repInfo + " inclusive, has been approved. <br>";
            }
            else if (mainreport.Status == MainReport.Statuses.Rejected)
            {
                appText = "<br> We regret to inform you, that your reporting for " + repInfo + " inclusive, was rejected for the following reasons: <br>";
                string poComment = mainreport.PoComments.OrderByDescending(f => f.Date).Select(f => f.Content).FirstOrDefault();
                //example 16 Oct 2012 04:21:21 PO has rejected the report for the following reason/s: last po comment reject
                if (string.IsNullOrEmpty(poComment))
                {
                    poComment = "N/A";
                }
                int i = poComment.IndexOf("reason/s:");
                if (i > 0 && i + 10 < poComment.Length)
                {
                    poComment = poComment.Substring(i + 10);
                }
                appText += "<br><b>" + poComment + "</b><br>";
                appText += "<br>Please " + appLink + " accordingly, and re-submit it when done.";

            }
            else if (mainreport.Status == MainReport.Statuses.AwaitingProgramOfficerApproval)
            {
                appText = string.Format("A <a href=\"{0}\">Report</a> is approved by RPA.", currentUrl);
            }
            else if (mainreport.Status == MainReport.Statuses.AwaitingAgencyResponse)
            {
                appText = string.Format("A <a href=\"{0}\">Report</a> status was changed to Awaiting Agency Response.", currentUrl);
            }
            return Header + "<p>" + appText + "<p>" + Footer;
        }

        public string GetMainReportInfo(MainReport mainreport)
        {
            string repInfo = "App# "
                + mainreport.AppBudget.App.Name
                + " SER "
                + mainreport.AppBudget.App.AgencyGroup.DisplayName
                + " starting "
                + String.Format("{0:MMM/yyyy}", mainreport.Start)
                + " and ending "
                + String.Format("{0:MMM/yyyy}", mainreport.End.AddDays(-1));
            return repInfo;
        }

        public void SendEmailNotification(MainReport mainreport)
        {
            var users = mainreport.AppBudget.App.AgencyGroup.Users;
            MailMessage msg = new MailMessage();


            msg.IsBodyHtml = true;
            var prevStatus = db.MainReportStatusAudits.Where(f => f.MainReportId == mainreport.Id).OrderByDescending(f => f.StatusChangeDate).FirstOrDefault();
            

            if (mainreport.Status == MainReport.Statuses.Approved)// && prevStatus.NewStatusId != 2)
            {
                
                CC.Web.Helpers.EmailHelper.AddRecipeintsByMainReportId(msg, mainreport.Id);
                msg.Subject = "Diamond Financial Report Approval for  ";
                msg.Body = GetMainReportMailText(mainreport);
            }
            else if (mainreport.Status == MainReport.Statuses.Rejected)
            {
                CC.Web.Helpers.EmailHelper.AddRecipeintsByMainReportId(msg, mainreport.Id);
                msg.Subject = "Diamond Financial Report Rejection for  ";
                msg.Body = GetMainReportMailText(mainreport);
            }
            else if (mainreport.Status == MainReport.Statuses.AwaitingProgramOfficerApproval)
            {
                CC.Web.Helpers.EmailHelper.AddRecipientsMainReportForRpaApproval(msg, mainreport);
                msg.Subject = "Diamond Financial Report Approval for  ";
                msg.Body = GetMainReportMailText(mainreport);

            }
            else if (mainreport.Status == MainReport.Statuses.AwaitingAgencyResponse)
            {
                CC.Web.Helpers.EmailHelper.AddRecipeintsByMainReportId(msg, mainreport.Id);
                msg.Subject = "Diamond Financial Report Awaiting Agency Response for  ";
                msg.Body = GetMainReportMailText(mainreport);
            }
           // else if (mainreport.Status == MainReport.Statuses.Approved && prevStatus.NewStatusId == 2) //change status from Approved to Approved - don't want to send an email
           // {
               // return;
          //  }
            else
            {
                throw new InvalidOperationException();
            }


            msg.Subject = msg.Subject + GetMainReportInfo(mainreport);

            using (var smtp = new SmtpClientWrapper())
            {
                try
                {
                    smtp.Send(msg);
                }
                catch (Exception ex)
                {

                    _log.Error("failed to send an email", ex);
                }
            }


        }


        public ActionResult HseapDetailed(int id)
        {

            return View(id);
        }
        public JsonResult HseapDetailedData(int id, jQueryDataTableParamModel p)
        {

            var source = HseapDetailedModel.HseapDetailedData(db, this.Permissions, id);

            var filtered = source;
            var ordered = filtered.OrderByField(p.sSortCol_0, p.bSortDir_0);
            var result = new jQueryDataTableResult
            {
                aaData = ordered.Skip(p.iDisplayStart).Take(p.iDisplayLength).ToList(),
                iTotalDisplayRecords = filtered.Count(),
                iTotalRecords = source.Count(),
                sEcho = p.sEcho
            };
            return this.MyJsonResult(result);

        }
        public ActionResult HseapDetailedPrint(int id)
        {
            var model = HseapDetailedModel.HseapDetailedData(db, this.Permissions, id);
            return View(model);
        }
        public ActionResult HseapDetailedExport(int id)
        {
            var model = HseapDetailedModel.HseapDetailedData(db, this.Permissions, id);
            return this.Excel("output", "data", model);
        }

        public ActionResult HseapSummary(int id)
        {
            var model = HseapSummaryModel.HseapDetailedData(this.db, this.Permissions, id);

            return View(model);
        }

        public ActionResult HseapSummaryExport(int id)
        {
            var model = HseapSummaryModel.HseapDetailedExportData(this.db, this.Permissions, id);

            return this.Excel("output", "Sheet1", model);
        }

        private class FinancialReportsListRow
        {
            [Display(Name = "Ser")]
            public string SerName { get; set; }
            [Display(Name = "Fund")]
            public string FundName { get; set; }
            [Display(Name = "Master Fund")]
            public string MasterFundName { get; set; }
            [Display(Name = "App #")]
            public string AppName { get; set; }
            [Display(Name = "From")]
            public DateTime Start { get; set; }
            [Display(Name = "To")]
            public DateTime End { get; set; }
            [Display(Name = "Status")]
            public string Status { get; set; }
            [Display(Name = "Submitted")]
            public DateTime? SubmittedAt { get; set; }
            [Display(Name = "Report Amount")]
            public decimal Amount { get; set; }
            [Display(Name = "CUR")]
            public string Currency { get; set; }
            [Display(Name = "App Amount")]
            public decimal CcGrant { get; set; }
            [Display(Name = "CUR")]
            public string CurrencyName { get; set; }
        }

        private const string ProgramOverviewFilesDirectory = "~/App_Data/ProgramOverview";
        private const string MhsaFilesDirectory = "~/App_Data/Mhsa";
        private const string PostApprovalCommentFilesDirectory = "~/App_Data/PostApprovalCommentFiles";
        private string programOverviewFileAbsolutePath(int id)
        {
            return fileAbsolutePath(id, ProgramOverviewFilesDirectory);
        }
        private string mhsaFileAbsolutePath(int id)
        {
            return fileAbsolutePath(id, MhsaFilesDirectory);
        }
        private string fileAbsolutePath(object id, string FilesDirectory)
        {
            var p1 = VirtualPathUtility.AppendTrailingSlash(FilesDirectory);
            var p2 = VirtualPathUtility.Combine(p1, id.ToString());
            var p3 = Server.MapPath(p2);
            return p3;
        }
        public void SaveProgramOverviewFile(int mrId, HttpPostedFileBase file)
        {
            SaveFile(file, programOverviewFileAbsolutePath(mrId));
        }
        public void SaveMhsaFile(int mrId, HttpPostedFileBase file)
        {
            SaveFile(file, mhsaFileAbsolutePath(mrId));
        }
        private string PostApprovalCommentFilePath(int mainReportId, int commentId)
        {
            var fileName = System.IO.Path.Combine(mainReportId.ToString(), commentId.ToString());
            var path = fileAbsolutePath(fileName, PostApprovalCommentFilesDirectory);
            return path;
        }
        public void SavePostApprovalCommentFile(int mainReportId, int fileId, HttpPostedFileBase file)
        {
            var path = PostApprovalCommentFilePath(mainReportId, fileId);
            SaveFile(file, path);
        }
        public void SaveFile(HttpPostedFileBase file, string path)
        {
            try
            {
                file.SaveAs(path);
            }
            catch (System.IO.DirectoryNotFoundException)
            {
                var directory = System.IO.Path.GetDirectoryName(path);
                System.IO.Directory.CreateDirectory(directory);
                file.SaveAs(path);
            }
        }
        public void DeleteProgramOverviewFile(int mrId)
        {
            DeleteFile(mrId.ToString(), Server.MapPath(VirtualPathUtility.AppendTrailingSlash(ProgramOverviewFilesDirectory)));
        }
        public void DeleteMhsaFile(int mrId)
        {
            DeleteFile(mrId.ToString(), Server.MapPath(VirtualPathUtility.AppendTrailingSlash(MhsaFilesDirectory)));
        }
        public void DeleteFile(string mrId, string path)
        {
            try
            {
                var di = new DirectoryInfo(path);
                var file = di.GetFiles().SingleOrDefault(f => f.Name == mrId);
                if (file != null)
                {
                    file.Delete();
                }
            }
            catch (Exception ex)
            {

            }
        }
        [HttpGet]
        public FileResult ProgramOverviewFile(int id)
        {
            return GetFileByPath(programOverviewFileAbsolutePath(id), false, id);
        }
        [HttpGet]
        public FileResult MhsaFile(int id)
        {
            return GetFileByPath(mhsaFileAbsolutePath(id), true, id);
        }
        public FileResult GetFileByPath(string path, bool mhsa, int id)
        {
            var mainReport = db.MainReports.SingleOrDefault(f => f.Id == id);
            if (mainReport != null)
            {
                if (System.IO.File.Exists(path))
                {
                    // return this.File(path, "application/octet-stream", mhsa ? mainReport.MhsaFileName : mainReport.ProgramOverviewFileName); LenaPDF
                     return this.File(path, "application/octet-stream", mhsa ? mainReport.MhsaFileName : mainReport.ProgramOverviewFileName);
                   //return this.File(path, "application/octet-stream", "GG 2nd Quarter 2020 HSAC - Meeting Minutes.pdf");
                }
            }

            throw new HttpException(404, "File not found");
        }

        public ActionResult SendYTDHcExceeding(int id, bool onSubmit)
        {
            if (!onSubmit && !this.User.IsInRole(FixedRoles.Admin))
            {
                return this.MyJsonResult(new { success = false }); ;
            }
            List<string> Errors = new List<string>();
            int successMsgs = 0;
            IQueryable<HCWeeklyCap> HCWeeklyCaps = db.HCWeeklyCaps.Where(f => f.Active);
            IQueryable<ClientReport> ClientReports = db.ClientReports;
            IQueryable<ClientAmountReport> ClientAmountReports = from cra in db.ClientAmountReports join cr in ClientReports on cra.ClientReportId equals cr.Id select cra;
            IQueryable<Client> Clients = db.Clients;
            var mainReport = db.MainReports.SingleOrDefault(f => f.Id == id);
            var appId = (from mr in db.MainReports
                         where mr.Id == mainReport.Id
                         select mr.AppBudget.AppId).SingleOrDefault();
            var startOfYear = new DateTime(mainReport.Start.Year, 1, 1);
            var ytdq = Queries.YTDHCWeeklyCapSummary(HCWeeklyCaps, Clients, ClientReports, ClientAmountReports, mainReport.Id, startOfYear);
            decimal monthlyPerc = (decimal)(mainReport.End.AddDays(-1).Month / 12.0);
            var capExceedingExpGrouped = ytdq.Where(f => EntityFunctions.Truncate(f.TotalAmount * db.viewAppExchangeRates.FirstOrDefault(c => c.AppId == appId && c.ToCur == f.Cur).Value - (f.CapPerPerson * monthlyPerc), CCDecimals.CompareDigits) > 0).ToList();
            foreach (var capExceedingExp in capExceedingExpGrouped.GroupBy(f => f.AgencyId))
            {
                //Lena
              //  capExceedingExp.ToList().Count = 2;
                if (capExceedingExp.ToList().Count > 0)
                {
                    try
                    {
                        EmailHelper.SendHCYTDExceedingNotification(capExceedingExp.Take(100).ToList(), mainReport.Id, appId, monthlyPerc * 100);
                        successMsgs++;
                    }
                    catch (Exception ex)
                    {
                        var msg = ex.Message;
                        if (ex.InnerException != null)
                        {
                            var inner = ex.InnerException;
                            while (inner.InnerException != null)
                            {
                                inner = inner.InnerException;
                            }
                            msg = inner.Message;
                        }
                        Errors.Add(msg);
                    }
                }
            }
            if (Errors.Count == 0)
            {
                return this.MyJsonResult(new { success = true, numOfMsgs = successMsgs });
            }
            else
            {
                return this.MyJsonResult(new { success = false, errors = Errors.ToArray() });
            }
        }

        public string ReadPdfFile(string fileName)
        {
            StringBuilder text = new StringBuilder();

            if (System.IO.File.Exists(fileName))
            {
                PdfReader pdfReader = new PdfReader(fileName);

                for (int page = 1; page <= pdfReader.NumberOfPages; page++)
                {
                    ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                    string currentText = PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy);

                    currentText = Encoding.UTF8.GetString(ASCIIEncoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(currentText)));
                    text.Append(currentText);
                }
                pdfReader.Close();
            }
            return text.ToString();
        }
    }

}
