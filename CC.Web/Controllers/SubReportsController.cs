using System;

using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CC.Data;
using CC.Web.Models;
using MvcContrib.ActionResults;
using MvcContrib;
using System.Data.Entity;
using System.Web.Script.Serialization;
using System.Globalization;
using CC.Web.Helpers;
using System.Data.Objects.SqlClient;

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
	public class SubReportsController : PrivateCcControllerBase
	{
		//
		// GET: /SubReports/

		#region properties

		private readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(SubReportsController));

		public IGenericRepository<Service> ServicesRepository { get; set; }
		public IGenericRepository<Agency> AgenciesRepository { get; set; }
		public IGenericRepository<MainReport> MainReportsRepository { get; set; }
		public IGenericRepository<SubReport> SubReportsRepository { get; set; }
		public IGenericRepository<ClientReport> ClientReportsRepository { get; set; }
		public IGenericRepository<ClientAmountReport> ClientAmountReportsRepository { get; set; }

		#endregion

		#region Constructors

		public SubReportsController()
			: base()
		{
			db = new ccEntities();
			ServicesRepository = new GenericRepository<Service>(db);
			AgenciesRepository = new GenericRepository<Agency>(db);
			MainReportsRepository = new GenericRepository<MainReport>(db);
			SubReportsRepository = new GenericRepository<SubReport>(db);
		}
		#endregion


		/// <summary>
		/// display the details
		/// </summary>
		/// <param name="model">
		///		includes
		/// </param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult Details(int id)
		{
			var existing = db.SubReports.Where(this.Permissions.SubReportsFilter).Where(f => f.Id == id).SingleOrDefault();
			if (existing == null)
			{
				throw new ArgumentException("subreport does not exists");
			}

			var mainReport = db.MainReports.Where(Permissions.MainReportsFilter).SingleOrDefault(f => f.Id == existing.MainReportId);
			if (mainReport == null)
			{
				//mainreport does not exists
				throw new ArgumentException("mainreport does not exists");
			}


			var repo = new GenericRepository<SubReport>(db);
            //db.CommandTimeout = 1000;
			var model = SubReportDetailsModel.LoadData(repo.GetAll(this.Permissions.SubReportsFilter), db.MainReports, db.ClientReports,  db.AppBudgetServices, db.SKMembersVisits,this.Permissions, id);

			if (model.ReportingMethodId == (int)Service.ReportingMethods.ClientEventsCount && this.User.IsInRole(FixedRoles.BMF))
			{
				throw new ArgumentException("not allowed");
			}

			var appBudgetService = db.AppBudgetServices.Include(f => f.AppBudget).SingleOrDefault(f => f.Id == model.AppBudgetServiceId);
			var startingWeek = mainReport.Start;
			DayOfWeek selectedDOW = startingWeek.DayOfWeek;
			int? selectedDowDb = GlobalHelper.GetWeekStartDay(model.AgencyGroupId, appBudgetService.AppBudget.AppId);
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
			model.DetailsHeader.StartWeekDay = selectedDOW;
			model.StartingWeek = startingWeek;
			DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
			dfi.FirstDayOfWeek = selectedDOW;
			Calendar cal = dfi.Calendar;
			model.StartingWeekNumber = cal.GetWeekOfYear(startingWeek, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);
			model.WeeksCount = cal.GetWeekOfYear(mainReport.End.AddDays(-1), dfi.CalendarWeekRule, dfi.FirstDayOfWeek) - model.StartingWeekNumber + 1;

			ViewBag.Services = (from abs in db.AppBudgetServices
								from mr in abs.AppBudget.MainReports
								from sr in mr.SubReports
								where sr.Id == id

								select new { Id = abs.Service.Id, Name = abs.Service.ServiceType.Name + " - " + abs.Service.Name }
				).Distinct()
				.OrderBy(f => f.Name)
				.ToDictionary(f => f.Id, f => f.Name);
			var IsraelKerenSerNumber = System.Web.Configuration.WebConfigurationManager.AppSettings["IsraelKerenSerNumber"].Parse<int>();
			ViewBag.isCfs = IsraelKerenSerNumber == model.AgencyGroupId && model.ReportingMethodId == (int)Service.ReportingMethods.Homecare;
			return View(model.ReportingMethod.ToString(), model);

		}



		[HttpGet]
		public ActionResult Calendar(int id)
		{
			var existing = db.SubReports.Where(this.Permissions.SubReportsFilter).Where(f => f.Id == id).SingleOrDefault();
			if (existing == null)
			{
				throw new ArgumentException("subreport does not exists");
			}

			var mainReport = db.MainReports.Where(Permissions.MainReportsFilter).SingleOrDefault(f => f.Id == existing.MainReportId);
			if (mainReport == null)
			{
				//mainreport does not exists
				throw new ArgumentException("mainreport does not exists");
			}


			var repo = new GenericRepository<SubReport>(db);

			var model = SubReportDetailsModel.LoadData(repo.GetAll(this.Permissions.SubReportsFilter), db.MainReports, db.ClientReports, db.AppBudgetServices, db.SKMembersVisits, this.Permissions,  id);




			if (model.MainReportStart.Month + 1 < model.MainReportEnd.AddDays(-1).Month)
			{
				var months = new[] {
                    new SelectListItem() { Value= mainReport.Start.Month.ToString(), Text=System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(mainReport.Start.Month)},
					new SelectListItem() { Value= (mainReport.Start.Month + 1).ToString(), Text=System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(mainReport.Start.Month + 1)},
					new SelectListItem() { Value= (mainReport.Start.Month + 2).ToString(), Text=System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(mainReport.Start.Month + 2)}
                };
				model.FilterByMonth.MonthsSelectList = new SelectList(months, "Value", "Text", model.FilterByMonth.SelMonth);
			}
			else
			{
				var months = new[] {
                    new SelectListItem() { Value= mainReport.Start.Month.ToString(), Text=System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(mainReport.Start.Month)}
                };
				model.FilterByMonth.MonthsSelectList = new SelectList(months, "Value", "Text", model.FilterByMonth.SelMonth);
			}

			model.FilterByMonth.SelMonth = mainReport.Start.Month; //first
			model.FilterByMonth.SelYear = mainReport.Start.Year;


			if (model.ReportingMethodId != (int)Service.ReportingMethods.DayCenters && model.ReportingMethodId != (int)Service.ReportingMethods.SoupKitchens)
			{

				throw new ArgumentException("Calendar screen can be only for Days Centers or Soup Kitchens Reporting Methods");
			}

			ViewBag.Services = (from abs in db.AppBudgetServices
								from mr in abs.AppBudget.MainReports
								from sr in mr.SubReports
								where sr.Id == id

								select new { Id = abs.Service.Id, Name = abs.Service.ServiceType.Name + " - " + abs.Service.Name }
				).Distinct()
				.OrderBy(f => f.Name)
				.ToDictionary(f => f.Id, f => f.Name);

			int j = model.FilterByMonth.SelMonth - 1;
			ViewBag.j = j;



			DateTime startOfMonth = mainReport.Start.AddMonths(j);
			ViewBag.Month = startOfMonth.Month;
			ViewBag.Year = startOfMonth.Year;
			DateTime endOfMonth = mainReport.End.AddMonths(1).AddDays(-1);
			int n = DateTime.DaysInMonth(startOfMonth.Year, startOfMonth.Month);





			return View(model.ReportingMethod.ToString() + "Calendar", model);

		}




		/// <summary>
		/// export reportd rows to excel - required for emergency reports only
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[HttpGet]
		public ActionResult Export(int id)
		{
			var repDetails = (from sr in db.SubReports.Where(this.Permissions.SubReportsFilter)
							  where sr.Id == id
							  select new
							  {
								  ReportingMethodId = sr.AppBudgetService.Service.ReportingMethodId,
								  ServiceTypeId = sr.AppBudgetService.Service.TypeId,
								  ExceptionalHomeCareHours = sr.AppBudgetService.Service.ExceptionalHomeCareHours,
								  CoPGovHoursValidation = sr.AppBudgetService.Service.CoPGovHoursValidation
							  }).SingleOrDefault();

			if (repDetails == null)
			{
				throw new Exception("Report not found.");
			}
			else
			{

				var helper = new SubReportsExportHelper()
				{
					db = this.db,
					permissions = this.Permissions,
					subReportId = id
				};
				switch ((Service.ReportingMethods)repDetails.ReportingMethodId)
				{
					case Service.ReportingMethods.ClientNamesAndCosts:
						if (repDetails.ServiceTypeId != (int)Service.ServiceTypes.MinorHomeModifications && repDetails.ServiceTypeId != (int)Service.ServiceTypes.MedicalProgram)
						{
							return this.Excel(id.ToString(), "Sheet1", helper.ClientNamesAndCosts());
						}
						else
						{
							return this.Excel(id.ToString(), "Sheet1", helper.MhmClientNamesAndCosts());
						}
					case Service.ReportingMethods.ClientUnit:
						return this.Excel(id.ToString(), "Sheet1", helper.ClientUnit());
					case Service.ReportingMethods.ClientUnitAmount:
						return this.Excel(id.ToString(), "Sheet1", helper.ClientUnitAmount());
					case Service.ReportingMethods.Emergency:
						return this.Excel(id.ToString(), "Sheet1", helper.Emergency());
					case Service.ReportingMethods.Homecare:
						return this.Excel(id.ToString(), "Sheet1", helper.Homecare());
					case Service.ReportingMethods.HomecareWeekly:
						if(repDetails.ExceptionalHomeCareHours && repDetails.CoPGovHoursValidation)
						{
							return this.Excel(id.ToString(), "Sheet1", helper.HomecareWeeklyWeh());
						}
						return this.Excel(id.ToString(), "Sheet1", helper.HomecareWeekly());
					case Service.ReportingMethods.TotalCostWithListOfClientNames:
						return this.Excel(id.ToString(), "Sheet1", helper.TotalCostWithListOfClientNames());
					case Service.ReportingMethods.SupportiveCommunities:
						return this.Excel(id.ToString(), "Sheet1", helper.SupportiveCommunities());
					case Service.ReportingMethods.DayCenters:
						return this.Excel(id.ToString(), "Sheet1", helper.DayCenters());
					case Service.ReportingMethods.SoupKitchens:
						return this.Excel(id.ToString(), "Sheet1", helper.SoupKitchens());
					case Service.ReportingMethods.ClientEventsCount:
						return this.Excel(id.ToString(), "Sheet1", helper.ClientEventsCount());
					default:
						throw new NotImplementedException();
				}
			}
		}

		#region Export sub types


		#endregion

		[HttpPost]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult UpdateSubReport(SubReport input)
		{
			var sr = db.SubReports.Where(this.Permissions.SubReportsFilter)
				.Include(f => f.AppBudgetService.Service)
				.SingleOrDefault(f => f.Id == input.Id);

			if (sr == null)
			{
				ModelState.AddModelError(string.Empty, "Report could not be found.");
			}

			if (input.Amount < 0)
			{
				ModelState.AddModelError(string.Empty, "Total Amount cannot be negative.");
			}

			sr.ServiceUnits = input.ServiceUnits;
			if (!Service.IsAmountReportedPerClient(sr.AppBudgetService.Service.ReportingMethodEnum))
			{
				sr.Amount = input.Amount;
			}
			sr.MatchingSum = input.MatchingSum;
			sr.AgencyContribution = input.AgencyContribution;
			sr.TotalHouseholdsServed = input.TotalHouseholdsServed;

			if (ModelState.IsValid)
			{
				try
				{
					var rowsUpdated = db.SaveChanges();
					ModelState.AddModelError(string.Empty, "Matching sum was Updated.");
				}
				catch (System.Data.UpdateException ex)
				{
					_log.Error(null, ex);
				}
			}



			if (sr.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.DayCenters || sr.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.SoupKitchens)
			{
				if (HttpContext.Request.UrlReferrer.AbsoluteUri.Contains("Calendar"))
				{
					return this.RedirectToAction(f => f.Calendar(sr.Id));
				}
			}
			return this.RedirectToAction(f => f.Details(sr.Id));
		}

		#region subreport data

		private class ClientWithClientReport
		{
			public Client Client { get; set; }
			public ClientReport ClientReport { get; set; }
		}


		/// <summary>
		/// Home care subreport data
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpGet]
		public JsonResult ClientReportsList(SubReportDetailsJqDt model)
		{
			string sortColName;
			bool sortAsc = model.sSortDir_0 == "asc";
			switch (model.iSortCol_0)
			{
				default:
				case 0: sortColName = "firstname"; break;
				case 1: sortColName = "lastname"; break;
			}
			var sr = (from a in db.SubReports.Where(Permissions.SubReportsFilter)
					  where a.Id == model.Id
					  select new
					  {
						  a.Id,
						  mrstart = a.MainReport.Start,
						  mrend = a.MainReport.End,
						  ExceptionalHomeCareHours = a.AppBudgetService.Service.ExceptionalHomeCareHours,
						  CoPGovHoursValidation = a.AppBudgetService.Service.CoPGovHoursValidation
					  }).Single();
			var c1p = new System.Data.Objects.ObjectParameter("displayCount", typeof(int));
			var c2p = new System.Data.Objects.ObjectParameter("totalCount", typeof(int));
			var curParamenter = new System.Data.Objects.ObjectParameter("Cur", typeof(string));
            db.CommandTimeout = 300;
			var objectReslut = db.spSrHcDetails(
				Permissions.User.Id,
				model.Id,
				model.Filter.ClientName,
				model.Filter.ClientId,
				model.Filter.ReportedOnly,
				sortColName,
				sortAsc,
				model.sSearch,
				model.iDisplayStart,
				model.iDisplayLength,
				c1p,
				c2p,
				curParamenter
			);
			var data = objectReslut.ToList();
			return this.MyJsonResult(new jQueryDataTableResult<object>
			{
				aaData = from f in data
						 select new
						 {
							 f.ClientReportId,
							 f.ClientId,
							 f.FirstName,
							 f.LastName,
							 ApprovalStatus = f.ApprovalStatusName,
							 f.Remarks,
							 f.Cfs,
							 f.Rate,
							 f.Q1,
							 f.Q2,
							 f.Q3,
							 Cur = curParamenter.Value,
							 SubReportId = model.Id,
                              HcCaps = (from a in db.HcCapsMonthlyTableRaws
									   where a.ClientId == f.ClientId && a.StartDate < sr.mrend && (a.EndDate == null || a.EndDate > sr.mrstart)
									   orderby a.StartDate
									   select new
										 {
											 ClientId = a.ClientId,
											 HcCap = a.HcCap,
											 StartDate = a.StartDate,
											 EndDate = a.EndDate
										 }),
						 },
				iTotalDisplayRecords = c1p.Value.As<int>(),
				iTotalRecords = c2p.Value.As<int>(),
				sEcho = model.sEcho
			});


		}

        /// <summary>
        /// Home care subreport data
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult ClientReportsListWeekly(SubReportDetailsJqDt model)
        {
            string sortColName;
            bool sortAsc = model.sSortDir_0 == "asc";
            switch (model.iSortCol_0)
            {
                default:
                case 0: sortColName = "firstname"; break;
                case 1: sortColName = "lastname"; break;
            }
            
            var sr = (from a in db.SubReports.Where(Permissions.SubReportsFilter)
                      where a.Id == model.Id
                      select new
                      {
                          a.Id,
                          mrstart = a.MainReport.Start,
                          mrend = a.MainReport.End,
                          a.AppBudgetService.Service.TypeId,
                          ExceptionalHomeCareHours = a.AppBudgetService.Service.ExceptionalHomeCareHours,
                          CoPGovHoursValidation = a.AppBudgetService.Service.CoPGovHoursValidation,
                          FundStartDate = a.AppBudgetService.AppBudget.App.Fund.StartDate
                      }).Single();
            var appBudgetService = db.AppBudgetServices.Include(f => f.AppBudget).SingleOrDefault(f => f.Id == model.AppBudgetServiceId);
            var c1p = new System.Data.Objects.ObjectParameter("displayCount", typeof(int));
            var c2p = new System.Data.Objects.ObjectParameter("totalCount", typeof(int));
            var curParamenter = new System.Data.Objects.ObjectParameter("Cur", typeof(string));
            db.CommandTimeout = 300;
            var objectReslut = db.spSrHcDetailsWeekly(
                Permissions.User.Id,
                model.Id,
                model.Filter.ClientName,
                model.Filter.ClientId,
                model.Filter.ReportedOnly,
                sortColName,
                sortAsc,
                model.sSearch,
                model.iDisplayStart,
                model.iDisplayLength,
                c1p,
                c2p,
                curParamenter
            );
            var data = objectReslut.ToList();
           
            if (sr.FundStartDate.Year >= 2021) //new calculations
            {
                return this.MyJsonResult(new jQueryDataTableResult<object>
                {
                    aaData = from f in data
                             select new
                             {
                                 f.ClientReportId,
                                 f.ClientId,
                                 f.FirstName,
                                 f.LastName,
                                 HASName = f.HASName,
                                 f.Remarks,
                                 f.Cfs,
                                 f.Rate,
                                 f.W1,
                                 f.W2,
                                 f.W3,
                                 f.W4,
                                 f.W5,
                                 f.W6,
                                 f.W7,
                                 f.W8,
                                 f.W9,
                                 f.W10,
                                 f.W11,
                                 f.W12,
                                 f.W13,
                                 f.W14,
                                 f.W15,
                                 Cur = curParamenter.Value,
                                 SubReportId = model.Id,
                                 LeaveDate = f.LeaveDate,
                                 CfsDate = db.CfsRows.Where(c => c.Client.MasterIdClcd == f.MasterIdClcd && c.StartDate < sr.mrend && c.EndDate == null).OrderByDescending(c => c.StartDate).Select(c => c.StartDate).FirstOrDefault(),
                                 HcCaps = GetHcCaps(f.ClientId, sr.mrstart, sr.mrend, sr.ExceptionalHomeCareHours, sr.CoPGovHoursValidation, sr.TypeId)
                                 
                                 
                             },
                    iTotalDisplayRecords = c1p.Value.As<int>(),
                    iTotalRecords = c2p.Value.As<int>(),
                    sEcho = model.sEcho
                });
            }

            else
            {
              return this.MyJsonResult(new jQueryDataTableResult<object>
                {
                    // if (sr.FundStartDate.Year >= 2021)
                    // { }
                    // else
        //            // {
                    aaData = from f in data
                             select new
                             {
                                 f.ClientReportId,
                                 f.ClientId,
                                 f.FirstName,
                                 f.LastName,
                                 HASName = f.HASName,
                                 f.Remarks,
                                 f.Cfs,
                                 f.Rate,
                                 f.W1,
                                 f.W2,
                                 f.W3,
                                 f.W4,
                                 f.W5,
                                 f.W6,
                                 f.W7,
                                 f.W8,
                                 f.W9,
                                 f.W10,
                                 f.W11,
                                 f.W12,
                                 f.W13,
                                 f.W14,
                                 f.W15,
                                 Cur = curParamenter.Value,
                                 SubReportId = model.Id,
                                 LeaveDate = f.LeaveDate,
                                 CfsDate = db.CfsRows.Where(c => c.Client.MasterIdClcd == f.MasterIdClcd && c.StartDate < sr.mrend && c.EndDate == null).OrderByDescending(c => c.StartDate).Select(c => c.StartDate).FirstOrDefault(),
                                 HcCaps = GetHcCapsBefore2021(f.ClientId, sr.mrstart, sr.mrend, sr.ExceptionalHomeCareHours, sr.CoPGovHoursValidation, sr.TypeId)
                             },
                    iTotalDisplayRecords = c1p.Value.As<int>(),
                    iTotalRecords = c2p.Value.As<int>(),
                    sEcho = model.sEcho
                });
            }
        }

        /// <summary>
        /// Retrieves data shown in "Hccaps" column on hc subreport details.
        /// One db call per each row (client).
        /// The actual value that needs to be shown depends on the type of report, e.g. hospicecare vs CoPGovHours&Exceptional vs regular homecare.
        /// </summary>
        /// <param name="ClientId"></param>
        /// <param name="mrstart"></param>
        /// <param name="mrend"></param>
        /// <param name="ExceptionalHomeCareHours"></param>
        /// <param name="CoPGovHoursValidation"></param>
        /// <param name="serviceTypeId"></param>
        /// <returns></returns>
        private object GetHcCaps(int? ClientId
            , DateTime mrstart
            , DateTime mrend
            , bool ExceptionalHomeCareHours
            , bool CoPGovHoursValidation
            , int serviceTypeId)

        {
            var q = (from a in db.spHcCapsTableRaw(ClientId, mrstart, mrend)
                     where  a.StartDate < mrend && (a.EndDate == null || a.EndDate > mrstart)
                     orderby a.StartDate
                     select a).ToList();
            
            if (ExceptionalHomeCareHours && CoPGovHoursValidation )
               
            {
                return from a in q
                       group a by new { a.ClientId, a.GovHcHours, a.DeceasedDate } into ag
                       let DOD = ag.Key.DeceasedDate
                       let minStartDate = ag.Min(c => c.StartDate)
                      
                       select new
                       {
                           ClientId = ag.Key.ClientId,
                           HcCap = ag.Key.GovHcHours,
                           StartDate = minStartDate,
                          
                          // EndDate = ag.Any(c => c.EndDate == null ) ? (DateTime?)null : ag.Max(c => c.EndDate),
                           EndDate = ag.Any(c => c.EndDate == null) ? (DateTime?)null : DOD != null ? DOD : ag.Max(c => c.EndDate),
                           //status == 1 ? "Approved" : status == 2 ? "Reject" : "Pending";
                           AfterReportStart = minStartDate > mrstart
                           
                            

                       };
            }
            else if (serviceTypeId == (int)Service.ServiceTypes.HospiseCare) {
                return from a in q
                       group a by new { a.ClientId, a.HospiceCareCap, a.DeceasedDate } into ag

                       let minStartDate = ag.Min(c => c.StartDate)
                       let DOD = ag.Key.DeceasedDate
                       select new
                       {
                           ClientId = ag.Key.ClientId,
                           HcCap = ag.Key.HospiceCareCap,
                           StartDate = minStartDate,
                           EndDate = ag.Any(c => c.EndDate == null ) ? (DateTime?)null : DOD != null ? DOD : ag.Max(c => c.EndDate) ,
                                          // EndDate = ag.Any(c => c.EndDate == null) ? (DateTime?)null : ag.Max(c => c.EndDate),
                           AfterReportStart = minStartDate > mrstart
                       };

            }
            else
            {
                return from a in q
                       group a by new { a.ClientId, a.HcCap, a.DeceasedDate } into ag

                       let minStartDate = ag.Min(c => c.StartDate)
                       let DOD = ag.Key.DeceasedDate
                       select new
                       {
                           ClientId = ag.Key.ClientId,
                           HcCap = ag.Key.HcCap,
                           StartDate = minStartDate,
                           // EndDate = ag.Any(c => c.EndDate == null) ? (DateTime?)null : ag.Max(c => c.EndDate),
                           EndDate = ag.Any(c => c.EndDate == null) ? (DateTime?)null : DOD != null ? DOD : ag.Max(c => c.EndDate),
                           AfterReportStart = minStartDate > mrstart
                       };

            }
           

        }

        private object GetHcCapsBefore2021(int? ClientId
            , DateTime mrstart
            , DateTime mrend
            , bool ExceptionalHomeCareHours
            , bool CoPGovHoursValidation
            , int serviceTypeId)

        {
            var q = (from a in db.spHcCapsTableRawBefore2021(ClientId, mrstart, mrend)
                     where a.StartDate < mrend && (a.EndDate == null || a.EndDate > mrstart)
                     orderby a.StartDate
                     select a).ToList();
            if (ExceptionalHomeCareHours && CoPGovHoursValidation)
            {
                return from a in q
                       group a by new { a.ClientId, a.GovHcHours, a.DeceasedDate } into ag
                       let DOD = ag.Key.DeceasedDate
                       let minStartDate = ag.Min(c => c.StartDate)
                       select new
                       {
                           ClientId = ag.Key.ClientId,
                           HcCap = ag.Key.GovHcHours,
                           StartDate = minStartDate,
                           EndDate = ag.Any(c => c.EndDate == null) ? (DateTime?)null : DOD != null ? DOD : ag.Max(c => c.EndDate),
                          // EndDate = ag.Any(c => c.EndDate == null) ? (DateTime?)null : ag.Max(c => c.EndDate),
                           AfterReportStart = minStartDate > mrstart
                       };
            }
            else if (serviceTypeId == (int)Service.ServiceTypes.HospiseCare)
            {
                return from a in q
                       group a by new { a.ClientId, a.HospiceCareCap, a.DeceasedDate } into ag
                       let DOD = ag.Key.DeceasedDate
                       let minStartDate = ag.Min(c => c.StartDate)
                       select new
                       {
                           ClientId = ag.Key.ClientId,
                           HcCap = ag.Key.HospiceCareCap,
                           StartDate = minStartDate,
                           EndDate = ag.Any(c => c.EndDate == null) ? (DateTime?)null : DOD != null ? DOD : ag.Max(c => c.EndDate),
                           //EndDate = ag.Any(c => c.EndDate == null) ? (DateTime?)null : ag.Max(c => c.EndDate),
                           AfterReportStart = minStartDate > mrstart
                       };

            }
            else
            {
                return from a in q
                       group a by new { a.ClientId, a.HcCap, a.DeceasedDate } into ag
                       let DOD = ag.Key.DeceasedDate
                       let minStartDate = ag.Min(c => c.StartDate)
                       select new
                       {
                           ClientId = ag.Key.ClientId,
                           HcCap = ag.Key.HcCap,
                           StartDate = minStartDate,
                           //EndDate = ag.Any(c => c.EndDate == null) ? (DateTime?)null : ag.Max(c => c.EndDate),
                           EndDate = ag.Any(c => c.EndDate == null) ? (DateTime?)null : DOD != null ? DOD : ag.Max(c => c.EndDate),
                           AfterReportStart = minStartDate > mrstart
                       };

            }


        }

        /// <summary>
        /// subreport data for all but personnel and homecare datatables
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet]
		public JsonResult GetRows(SubReportDetailsJqDt model)
		{
			var subreport = db.SubReports
				.Include(f => f.MainReport)
				.Where(Permissions.SubReportsFilter).Single(f => f.Id == model.Id);

			switch (subreport.AppBudgetService.Service.ReportingMethodEnum)
			{
				case Service.ReportingMethods.ClientNamesAndCosts:
					return getchildren(model, toobjArray(subreport.AppBudgetService.Service.ReportingMethodId, subreport.AppBudgetService.Service.TypeId, subreport.AppBudgetService.Service.Name));
				case Service.ReportingMethods.TotalCostWithListOfClientNames:
					return getchildren(model, toobjArray(subreport.AppBudgetService.Service.ReportingMethodId, subreport.AppBudgetService.Service.TypeId, subreport.AppBudgetService.Service.Name));
				case Service.ReportingMethods.ClientUnit:
					return getchildren(model, toobjArray(subreport.AppBudgetService.Service.ReportingMethodId, subreport.AppBudgetService.Service.TypeId, subreport.AppBudgetService.Service.Name));
				case Service.ReportingMethods.ClientUnitAmount:
					return getchildren(model, toobjArray(subreport.AppBudgetService.Service.ReportingMethodId, subreport.AppBudgetService.Service.TypeId, subreport.AppBudgetService.Service.Name));
				case Service.ReportingMethods.Emergency:
					return GetEmergencyReportRows(model, subreport);
				case Service.ReportingMethods.Homecare:
					return ClientReportsList(model);
                case Service.ReportingMethods.ClientAmount:
                    return getchildren(model, toobjArray(subreport.AppBudgetService.Service.ReportingMethodId, subreport.AppBudgetService.Service.TypeId, subreport.AppBudgetService.Service.Name));
                //return ClientReportsList(model);
                case Service.ReportingMethods.HomecareWeekly:
					return ClientReportsListWeekly(model);
				case Service.ReportingMethods.SupportiveCommunities:
					return GetSupportiveCommunities(model, subreport);
				case Service.ReportingMethods.DayCenters:
					return GetDayCenters(model, subreport);
				case Service.ReportingMethods.SoupKitchens:
					return GetSoupKitchens(model, subreport);
				case Service.ReportingMethods.ClientEventsCount:
					return GetClientEventsCount(model);
				default:
					throw new NotImplementedException();
			}
		}

		private Func<GetChildrenWithoutNameSubClass, object[]> toobjArray(int ReportingMethodId, int serviceTypeId, string ServiceName)
		{
			switch ((Service.ReportingMethods)ReportingMethodId)
			{

				case Service.ReportingMethods.ClientNamesAndCosts:
					if (serviceTypeId != (int)Service.ServiceTypes.MinorHomeModifications && serviceTypeId != (int)Service.ServiceTypes.MedicalProgram)
					{
						return f => new object[]{ 
							f.Id,
							f.ClientFirstName,
							f.ClientLastName,
							f.ClientApprovalStatus,
							f.ClientId, 
							f.HcCaps,
							f.Amount, 
							f.Remarks,
							null
						};
					}
					return f => new object[]{ 
							f.Id,
							f.ClientFirstName,
							f.ClientLastName,
							f.ClientApprovalStatus,
							f.ClientId,
							f.Amount, 
							f.Remarks,
							null
						};
				case Service.ReportingMethods.TotalCostWithListOfClientNames:
                    if (ServiceName == "Funeral Expenses")
                    {
                        return f => new object[]{
                        f.Id,
                        f.ClientFirstName,
                        f.ClientLastName,
                        f.ClientApprovalStatus,
                        f.ClientId,
                        f.Id!=null,
                        f.Amount,
                        f.Remarks,
                        null
                    };
                    }
                    else
                    {
                        return f => new object[]{
                        f.Id,
                        f.ClientFirstName,
                        f.ClientLastName,
                        f.ClientApprovalStatus,
                        f.ClientId,
                        f.Id!=null,
                        //f.Amount,
                        f.Remarks,
                        null
                    };

                    }

				case Service.ReportingMethods.ClientUnit:
					return f => new object[]{
						f.Id,
                        f.ClientFirstName,
                        f.ClientLastName,
						f.ClientApprovalStatus,
						f.ClientId,
						f.Quantity,
						f.Remarks,
						null
					};
				case Service.ReportingMethods.ClientUnitAmount:
					return f => new object[]{
						f.Id,
						f.ClientFirstName,
                        f.ClientLastName,
						f.ClientApprovalStatus,
						f.ClientId,
                        f.Id!=null,
                        f.Amount,
                        f.Quantity,
						f.Remarks,
						null
					};
                case Service.ReportingMethods.ClientAmount:
                    return f => new object[]{
                        f.Id,
                        f.ClientFirstName,
                        f.ClientLastName,
                        f.ClientApprovalStatus,
                        f.ClientId,
                        f.Id!=null,
                        f.Amount,
                        f.Remarks,
                        null
                    };
                case Service.ReportingMethods.Emergency:
				case Service.ReportingMethods.Homecare:
				case Service.ReportingMethods.HomecareWeekly:
                
                default:
					return null;
			}
		}


		private JsonResult GetSupportiveCommunities(SubReportDetailsJqDt model, SubReport subReport)
		{
			var mainReport = subReport.MainReport;
			var SC_min_day = System.Web.Configuration.WebConfigurationManager.AppSettings["SC_min_day"].Parse<int>() - 1;  //Min leave join
			var SC_max_day = System.Web.Configuration.WebConfigurationManager.AppSettings["SC_max_day"].Parse<int>() - 1;  //Max join date
           

            //-1 to be correct data, for example SC_min_day=10. firstMonthMinLeave must be 10 Apr, but 1 apr+10=11 apr, so take 1 apr+9


            DateTime myStartDate = mainReport.Start;
			DateTime myEndDate = mainReport.End;

			int serType = subReport.AppBudgetService.Service.ServiceType.Id; //supportive communities only

			bool subsidy = subReport.MainReport.AppBudget.App.AgencyGroup.SC_FullSubsidy;
            var subLevelId = subReport.MainReport.AppBudget.App.AgencyGroup.ScSubsidyLevelId;
           


            var SC_subsidy = System.Web.Configuration.WebConfigurationManager.AppSettings["SC_subsidy"].Parse<int>();

            if (subLevelId == 1)
            { SC_subsidy = 25; }
            else
            { SC_subsidy = 15; }

            var q = from c in db.Clients.Where(Permissions.ClientsFilter)
					where c.AgencyId == model.AgencyId && c.ApprovalStatusId != 1024
                    join a in db.viewScRepSources.Where(f => f.subreportid == model.Id) on c.Id equals a.clientid into ag
					from a in ag.DefaultIfEmpty()
					join sc in db.SupportiveCommunitiesReports.Where(f => f.SubReportId == model.Id) on c.Id equals sc.ClientId into scg
					from sc in scg.DefaultIfEmpty()
					where sc != null || a != null
					select new
					{
						Id = sc != null ? (int?)sc.Id : (int?)null,
						ClientId = c.Id,
						IsraelId = c.NationalId,
						ClientFirstName = c.FirstName,
						ClientLastName = c.LastName,
						ClientName = c.FirstName + " " + c.LastName,
						ApprovalStatus = c.ApprovalStatus.Name,
						JoinDate = c.JoinDate,
						SubReportId = subReport.Id,
                        //HoursHoldCost = sc != null ? sc.HoursHoldCost ?? sc.Amount / sc.MonthsCount : (decimal?)null,
                        HoursHoldCost = sc != null ? SC_subsidy : (decimal?)null,
                        //Amount = sc != null ? sc.Amount : (decimal?)null,
                        Amount = sc != null ? SC_subsidy * sc.MonthsCount : (decimal?)null,
                        MonthsCount = sc != null ? sc.MonthsCount : (int?)null,
						SC_Client = c.SC_Client,
						Reported = sc != null
					};


			var filtered = q;
			if (!string.IsNullOrWhiteSpace(model.Filter.ClientName)) { filtered = filtered.Where(f => f.ClientName.Contains(model.Filter.ClientName)); }
			if (model.Filter.ClientId.HasValue) { filtered = filtered.Where(f => f.ClientId == model.Filter.ClientId); }
			if (model.Filter.ReportedOnly == true) { filtered = filtered.Where(f => f.Id != null); }
			if (model.FilterForSC != null && model.FilterForSC.SC_Only == 1)
			{
				filtered = filtered.Where(f => f.SC_Client == true);
			}
			if (!string.IsNullOrWhiteSpace(model.sSearch))
			{
				foreach (var s in model.sSearch.Split(new char[] { ' ' }).Where(f => !string.IsNullOrWhiteSpace(f)))
				{
					filtered = filtered.Where(f => f.ClientName.Contains(s) || f.IsraelId.Contains(s));
				}
			}


			bool sortAsc = Request != null ? Request["sSortDir_0"] == "asc" : false;
			int? sortColIndex = Request != null ? Request["iSortCol_0"].Parse<int>() : null;


			#region Sort
			switch (sortColIndex)
			{
				case 3:
					if (!sortAsc) { filtered = filtered.OrderByDescending(f => f.ClientFirstName); }
					else { filtered = filtered.OrderBy(f => f.ClientFirstName); }
					break;
				case 4:
					if (!sortAsc) { filtered = filtered.OrderByDescending(f => f.ClientLastName); }
					else { filtered = filtered.OrderBy(f => f.ClientLastName); }
					break;
				default:
					filtered = filtered.OrderBy(f => f.ClientFirstName);
					break;
			}
			#endregion

			model.iTotalDisplayRecords = filtered.Count();
			model.iTotalRecords = filtered.Count();
			model.aaData = filtered.Skip(model.iDisplayStart).Take(model.iDisplayLength).ToList()
				.Select(f => new object[]{
				f.Id,
                f.ClientId,
				f.IsraelId,
                f.ClientFirstName,
                f.ClientLastName,
                f.ApprovalStatus,
                f.JoinDate,
				f.HoursHoldCost,
                f.MonthsCount,
				f.Amount,
				f.Reported
				});
			return this.MyJsonResult(model, JsonRequestBehavior.AllowGet);
		}

		private JsonResult GetDayCenters(SubReportDetailsJqDt model, SubReport subReport)
		{
			var DCC_subsidy = System.Web.Configuration.WebConfigurationManager.AppSettings["DCC_subsidy"].Parse<int>();


			var mainReport = subReport.MainReport;
            


			DateTime myStartDate = mainReport.Start;
			DateTime myEndDate = mainReport.End;
			int serType = subReport.AppBudgetService.Service.ServiceType.Id; //DCC

           


            var q = from c in db.Clients.Where(Permissions.ClientsFilter)
					where c.AgencyId == model.AgencyId && c.ApprovalStatusId != 1024
                     join a in db.DaysCentersReports.Where(f => f.SubReportId == model.Id && f.VisitsCount !=0) on c.Id equals a.ClientId into ag
                    //join a in db.DaysCentersReports.Where(f => f.SubReportId == model.Id ) on c.Id equals a.ClientId into ag
                    from a in ag.DefaultIfEmpty()
                    join hce in db.HomeCareEntitledPeriods.Where(f => f.StartDate >= myStartDate && (f.EndDate >= myStartDate || f.EndDate == null)) on c.Id equals hce.ClientId into hceg
                    from hce in hceg.DefaultIfEmpty()
                    where (a != null) ||
                    (c.JoinDate <= myEndDate) &&                              //Join Date in report range
                                                                              // (c.LeaveDate == null) &&//|| c.LeaveDate >= myStartDate) &&     //Leave Date null or in report range
                                                                              // (c.DeceasedDate == null)
                   (c.LeaveDate == null || c.LeaveDate >= myStartDate) &&     //Leave Date null or in report range
                    (c.DeceasedDate == null || c.DeceasedDate >= myStartDate)
                    // if reported - cliets will show does not matter what; if not reported below criterias  //(a != null || hce != null) || 
                    // ((hce.StartDate < myEndDate) &&                       //Eligibility start date in report range 
                    // (hce.EndDate >= myStartDate || hce.EndDate == null) && // Eligibility end date is open/null or in report range(later than report start date)/Was eligible whe n report starts
                    // (c.JoinDate < myEndDate) &&                              //Join Date in report range
                    // (c.LeaveDate == null || c.LeaveDate >= myStartDate) &&     //Leave Date null or in report range
                    // (c.DeceasedDate == null))
                    //(a != null || hce != null) //||
                    // (hce.StartDate < myEndDate)
                    //( (c.JoinDate < myEndDate) && //
                    // (c.LeaveDate == null || c.LeaveDate >= myStartDate) &&
                    // ( c.DeceasedDate == null) && (hce.StartDate < myEndDate))
                    //where hce.StartDate < myEndDate    

                    // where hce.StartDate < myEndDate
                    //&& 
                    // hce.StartDate < myEndDate
                    //||
                    // ((c.JoinDate < myEndDate) && //
                    // (c.LeaveDate == null || c.LeaveDate >= myStartDate) &&
                    //  hce.StartDate < myEndDate// &&
                    // ((hce.EndDate == null && hce.StartDate < myEndDate) || (hce.EndDate >= myStartDate))  //==true


                    //((c.JoinDate < myEndDate) &&
                    //(c.LeaveDate == null || c.LeaveDate >= myStartDate )// && 
                    // (hce.StartDate != null)// && hce.StartDate < myEndDate && hce.EndDate == null)  //==true

                    // )
                    select new 
                    {
                        Id = (int?)a.Id,
                        ClientId = c.Id,
                        IsraelId = c.NationalId,
                        ClientFirstName = c.FirstName,
                        ClientLastName = c.LastName,
                        ClientName = c.FirstName + " " + c.LastName,
                        ApprovalStatus = c.ApprovalStatus.Name,
                        JoinDate = c.JoinDate,
                        SubReportId = (int?)a.SubReportId,
                        SubsidesByDcc = DCC_subsidy,
                        VisitCost = c.DCC_VisitCost.HasValue ? (decimal?)c.DCC_VisitCost : 0,
                        VisitCount = a.VisitsCount.HasValue ? a.VisitsCount : 0, //temporal
                        Amount = ((decimal?)a.Amount ?? 0),
                        DCC_Client = c.DCC_Client
                    };
           

			var filtered = q;
			if (!string.IsNullOrWhiteSpace(model.Filter.ClientName)) { filtered = filtered.Where(f => f.ClientName.Contains(model.Filter.ClientName)); }
			if (model.Filter.ClientId.HasValue) { filtered = filtered.Where(f => f.ClientId == model.Filter.ClientId); }
			if (model.Filter.ReportedOnly == true) { filtered = filtered.Where(f => f.Id != null); }
			if (model.FilterForDCC.DCC_Only == 1) { filtered = filtered.Where(f => f.DCC_Client == true); }
			if (!string.IsNullOrWhiteSpace(model.sSearch))
			{
				foreach (var s in model.sSearch.Split(new char[] { ' ' }).Where(f => !string.IsNullOrWhiteSpace(f)))
				{
					filtered = filtered.Where(f => f.ClientName.Contains(s));
				}
			}


			bool sortAsc = Request != null ? Request["sSortDir_0"] == "asc" : false;
            var sortCol = Request != null ? Request["iSortCol_0"] : null;


            #region Sort
            switch (sortCol)
			{
				case "firstName":
					if (!sortAsc) { filtered = filtered.OrderByDescending(f => f.ClientFirstName); }
					else { filtered = filtered.OrderBy(f => f.ClientFirstName); }
					break;
				case "lastName":
					if (!sortAsc) { filtered = filtered.OrderByDescending(f => f.ClientLastName); }
					else { filtered = filtered.OrderBy(f => f.ClientLastName); }
					break;
				default:
					filtered = filtered.OrderBy(f => f.ClientFirstName);
					break;
			}
			#endregion

			model.iTotalDisplayRecords = filtered.Count();
			model.iTotalRecords = filtered.Count();
			model.aaData = filtered.Skip(model.iDisplayStart).Take(model.iDisplayLength).ToList()
				.Select(f => new object[]{
				f.Id,
                f.ClientId,
				f.IsraelId,
                f.ClientFirstName,
                f.ClientLastName,
                f.ApprovalStatus,
                f.JoinDate,
				f.SubsidesByDcc,
				f.VisitCost,
                f.VisitCount,
                f.Amount
               
				
				});
			return this.MyJsonResult(model, JsonRequestBehavior.AllowGet);
		}

		private JsonResult GetSoupKitchens(SubReportDetailsJqDt model, SubReport subReport)
		{
			var mainReport = subReport.MainReport;


			DateTime myStartDate = mainReport.Start;
			DateTime myEndDate = mainReport.End;
			int serType = subReport.AppBudgetService.Service.ServiceType.Id; //DCC






			var q = from c in db.Clients.Where(Permissions.ClientsFilter)
					where c.AgencyId == model.AgencyId && c.ApprovalStatusId != 1024
                    join a in db.SoupKitchensReports.Where(f => f.SubReportId == model.Id) on c.Id equals a.ClientId into ag
					from a in ag.DefaultIfEmpty()
					where a != null ||
						((c.JoinDate < myEndDate) &&
						(c.LeaveDate == null || c.LeaveDate >= myStartDate)//==true
						)
					select new
					{
						Id = (int?)a.Id,
						ClientId = c.Id,
						IsraelId = c.NationalId,
						ClientFirstName = c.FirstName,
						ClientLastName = c.LastName,
						ClientName = c.FirstName + " " + c.LastName,
						ApprovalStatus = c.ApprovalStatus.Name,
						JoinDate = c.JoinDate,
						SubReportId = (int?)a.SubReportId,
						VisitCount = a.SKMembersVisits.Count,
					};


			var filtered = q;
			if (!string.IsNullOrWhiteSpace(model.Filter.ClientName)) { filtered = filtered.Where(f => f.ClientName.Contains(model.Filter.ClientName)); }
			if (model.Filter.ClientId.HasValue) { filtered = filtered.Where(f => f.ClientId == model.Filter.ClientId); }
			if (model.Filter.ReportedOnly == true) { filtered = filtered.Where(f => f.Id != null); }
			if (!string.IsNullOrWhiteSpace(model.sSearch))
			{
				foreach (var s in model.sSearch.Split(new char[] { ' ' }).Where(f => !string.IsNullOrWhiteSpace(f)))
				{
					filtered = filtered.Where(f => f.ClientName.Contains(s));
				}
			}


			bool sortAsc = Request != null ? Request["sSortDir_0"] == "asc" : false;
			int? sortColIndex = Request != null ? Request["iSortCol_0"].Parse<int>() : null;


			#region Sort
			switch (sortColIndex)
			{
				case 2:
					if (!sortAsc) { filtered = filtered.OrderByDescending(f => f.ClientFirstName); }
					else { filtered = filtered.OrderBy(f => f.ClientFirstName); }
					break;
				case 3:
					if (!sortAsc) { filtered = filtered.OrderByDescending(f => f.ClientLastName); }
					else { filtered = filtered.OrderBy(f => f.ClientLastName); }
					break;
				default:
					filtered = filtered.OrderBy(f => f.ClientFirstName);
					break;
			}
			#endregion

			model.iTotalDisplayRecords = filtered.Count();
			model.iTotalRecords = filtered.Count();
			model.aaData = filtered.Skip(model.iDisplayStart).Take(model.iDisplayLength).ToList()
				.Select(f => new object[]{
				f.Id,
                f.ClientId,
				f.IsraelId,
                f.ClientFirstName,
                f.ClientLastName,
                f.ApprovalStatus,
                f.JoinDate,
                f.VisitCount,
				});
			return this.MyJsonResult(model, JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		public JsonResult GetClientEventsCount(SubReportDetailsJqDt model)
		{
			var q = from cec in db.ClientEventsCountReports.Where(Permissions.ClientEventsCountReportsFilter)
					where cec.SubReportId == model.Id
					select cec;

			var filtered = q;
			if (!string.IsNullOrWhiteSpace(model.sSearch))
			{
				filtered = filtered.Where(f => f.Remarks == null || f.Remarks.Contains(model.sSearch));
			}

			bool sortAsc = Request != null ? Request["sSortDir_0"] == "asc" : false;
			int? sortColIndex = Request != null ? Request["iSortCol_0"].Parse<int>() : null;

			#region Sort
			switch (sortColIndex)
			{
				case 1:
					if (!sortAsc) { filtered = filtered.OrderByDescending(f => f.EventDate); }
					else { filtered = filtered.OrderBy(f => f.EventDate); }
					break;
				case 2:
					if (!sortAsc) { filtered = filtered.OrderByDescending(f => f.JNVCount); }
					else { filtered = filtered.OrderBy(f => f.JNVCount); }
					break;
				case 3:
					if (!sortAsc) { filtered = filtered.OrderByDescending(f => f.TotalCount); }
					else { filtered = filtered.OrderBy(f => f.TotalCount); }
					break;
				case 4:
					if (!sortAsc) { filtered = filtered.OrderByDescending(f => f.Remarks); }
					else { filtered = filtered.OrderBy(f => f.Remarks); }
					break;
				default:
					filtered = filtered.OrderBy(f => f.EventDate);
					break;
			}
			#endregion

			return this.MyJsonResult(new jQueryDataTableResult<object>
			{
				aaData = from f in filtered
						 select new
						 {
							 f.Id,
							 f.EventDate,
							 f.JNVCount,
							 f.TotalCount,
							 f.Remarks
						 },
				iTotalDisplayRecords = filtered.Count(),
				iTotalRecords = q.Count(),
				sEcho = model.sEcho
			});
		}


        public JsonResult GetCalendarRows(SubReportDetailsJqDt model, SubReport sr)
        {

            var subReport = db.SubReports
               .Include(f => f.MainReport)
               .Where(Permissions.SubReportsFilter).Single(f => f.Id == sr.Id);

            var mainReport = subReport.MainReport;

            int j = model.FilterByMonth.SelMonth - mainReport.Start.Month;

            DateTime myStartDate = mainReport.Start;
            DateTime myEndDate = mainReport.End;

            DateTime startOfMonth = mainReport.Start.AddMonths(j);
            DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
            model.FilterByMonth.SelYear = mainReport.Start.AddMonths(j).Year;




            int serType = subReport.AppBudgetService.Service.ServiceType.Id; //DCC

            var clients = db.Clients.Where(Permissions.ClientsFilter);
            if (model.FilterForDCC != null)
            {
                if (model.FilterForDCC.DCC_Only == 1)
                {
                    clients = clients.Where(f => f.DCC_Client);
                }
            }





            //var q = from c in clients
            //where c.AgencyId == model.AgencyId
            //join a in db.DaysCentersReports.Where(f => f.SubReportId == model.Id) on c.Id equals a.ClientId into ag
            //from a in ag.DefaultIfEmpty()
            //  join hce in db.HomeCareEntitledPeriods.Where(f => f.EndDate >= myStartDate || f.EndDate == null) on c.Id equals hce.ClientId into hceg 
            //  from hce in hceg.DefaultIfEmpty()
            //  where (a != null && hce != null) ||

            //((c.JoinDate < myEndDate) ||
            //  (c.LeaveDate == null || c.LeaveDate >= myStartDate) //||
            // // (hce.EndDate ==null && hce.StartDate < myEndDate)//==true
            db.CommandTimeout = 300;
            var q = from c in db.Clients.Where(Permissions.ClientsFilter)
                    where c.AgencyId == model.AgencyId && c.ApprovalStatusId != 1024
                    join a in db.DaysCentersReports.Where(f => f.SubReportId == model.Id && f.VisitsCount != 0) on c.Id equals a.ClientId into ag
                    from a in ag.DefaultIfEmpty()
                    join hce in db.HomeCareEntitledPeriods.Where(f => f.StartDate <= myEndDate && (f.EndDate >= myStartDate || f.EndDate == null)) on c.Id equals hce.ClientId into hceg
                    // join hce in db.HomeCareEntitledPeriods.Where(f => f.StartDate >= myStartDate && (f.EndDate >= myStartDate || f.EndDate == null)) on c.Id equals hce.ClientId into hceg
                    from hce in hceg.DefaultIfEmpty()
                    where (a != null) || // if reported - cliets will show does not matter what; a - covers reported clients 
                    ((hce != null) && //|| // if not reported - then below criterias ; hce - eligibility date
                                      // ((hce.StartDate <= myEndDate) &&                       //Eligibility start date in report range 
                                      // (hce.EndDate >= myStartDate || hce.EndDate == null) && // Eligibility end date is open/null or in report range(later than report start date)/Was eligible whe n report starts
                    ((c.JoinDate <= myEndDate) &&                              //Join Date in report range
                    //(c.LeaveDate == null) &&//|| c.LeaveDate >= myStartDate) &&     //Leave Date null or in report range
                    ((c.LeaveDate == null || c.LeaveDate >= myStartDate) &&     //Leave Date null or in report range, after report SatrtDate
                    (c.DeceasedDate == null || c.DeceasedDate >= myStartDate))  //Deceased Date null or in report range, after report SatrtDate






                     ))
                    select new
					{
						Id = (int?)a.Id,
						ClientId = c.Id,
						ClientName = c.FirstName + " " + c.LastName,
						SubReportId = (int?)a.SubReportId,
						mv = from v in db.DccMemberVisits.Where(f => f.SubReport.Id == subReport.Id && f.ClientId == c.Id && f.ReportDate >= startOfMonth && f.ReportDate <= endOfMonth)
							 select new
								{
									ReportDate = v.ReportDate,

								}
							   ,
						ClientFirstName = c.FirstName,
						ClientLastName = c.LastName,
						VisitCost = 0,
						ApprovalStatus = c.ApprovalStatus.Name,
                        DCC_Client = c.DCC_Client
                    };


			var filtered = q;
			if (!string.IsNullOrWhiteSpace(model.Filter.ClientName)) { filtered = filtered.Where(f => f.ClientName.Contains(model.Filter.ClientName)); }
			if (model.Filter.ClientId.HasValue) { filtered = filtered.Where(f => f.ClientId == model.Filter.ClientId); }
			if (model.Filter.ReportedOnly == true) { filtered = filtered.Where(f => f.Id != null); }
            if (model.FilterForDCC.DCC_Only == 1) { filtered = filtered.Where(f => f.DCC_Client == true); }
            if (!string.IsNullOrWhiteSpace(model.sSearch))
			{
				foreach (var s in model.sSearch.Split(new char[] { ' ' }).Where(f => !string.IsNullOrWhiteSpace(f)))
				{
					filtered = filtered.Where(f => f.ClientName.Contains(s));
				}
			}


			bool sortAsc = Request != null ? Request["sSortDir_0"] == "asc" : false;
            var sortCol = Request != null ? Request["iSortCol_0"] : null;


            #region Sort
            switch (sortCol)
			{
				case "firstName":
					if (!sortAsc) { filtered = filtered.OrderByDescending(f => f.ClientFirstName).ThenByDescending(f => f.ClientId); }
					else { filtered = filtered.OrderBy(f => f.ClientFirstName).ThenBy(f => f.ClientId); }
					break;
				case "lastName":
					if (!sortAsc) { filtered = filtered.OrderByDescending(f => f.ClientLastName).ThenByDescending(f => f.ClientId); }
					else { filtered = filtered.OrderBy(f => f.ClientLastName).ThenBy(f => f.ClientId); }
					break;
				default:
					filtered = filtered.OrderBy(f => f.ClientFirstName).ThenBy(f => f.ClientId);
					break;
			}
			#endregion


			var data = filtered.Skip(model.iDisplayStart).Take(model.iDisplayLength).ToList();

			List<ClientsVisitsModel> cvm = new List<ClientsVisitsModel>();
			foreach (var d in data)
			{


				var item = new ClientsVisitsModel();
				item.ClientId = d.ClientId;
				item.ClientName = d.ClientName;
				item.ClientLastName = d.ClientLastName;
				item.ClientFirstName = d.ClientFirstName;
				item.ClientApprovalStatus = d.ApprovalStatus;
				item.VisitsCount = 0;
				item.Id = (int)(d.Id.HasValue ? d.Id : 0);
				for (int i = 0; i < 31; i++)
				{
					item.dlist.Add(false);

				}

				foreach (var v in d.mv)
				{

					var i = v.ReportDate.Day;
					item.dlist[i - 1] = true;
					item.VisitsCount++;

				}

				cvm.Add(item);

			}

			if (model.Filter.ReportedOnly.HasValue && model.Filter.ReportedOnly.Value)
			{
				cvm = cvm.Where(f => f.VisitsCount > 0).ToList();
			}

			var aaData = new List<object[]>();
			foreach (var l in cvm)
			{
				aaData.Add(new object[] { l.ClientId, l.ClientName, l.ClientFirstName, l.ClientLastName, l.ClientApprovalStatus, l.dlist[0],
            l.dlist[1],l.dlist[2],l.dlist[3],
            l.dlist[4],l.dlist[5],l.dlist[6],
            l.dlist[7],l.dlist[8],l.dlist[9],
            l.dlist[10],l.dlist[11],l.dlist[12],
            l.dlist[13],l.dlist[14],l.dlist[15],
            l.dlist[16],l.dlist[17],l.dlist[18],
            l.dlist[19],l.dlist[20],l.dlist[21],
            l.dlist[22],l.dlist[23],l.dlist[24],
            l.dlist[25],l.dlist[26],l.dlist[27],
            l.dlist[28],l.dlist[29], l.dlist[30], 
            l.VisitsCount,l.Id});

			}

			model.iTotalDisplayRecords = filtered.Count();
			model.iTotalRecords = filtered.Count();
			model.aaData = aaData;



			return this.MyJsonResult(model, JsonRequestBehavior.AllowGet);
		}

		public JsonResult GetCalendarSaturdaysRows(SubReportDetailsJqDt model, SubReport sr)
		{

			var subReport = db.SubReports
			   .Include(f => f.MainReport)
			   .Where(Permissions.SubReportsFilter).Single(f => f.Id == sr.Id);

			var mainReport = subReport.MainReport;

			int j = model.FilterByMonth.SelMonth - mainReport.Start.Month;

			DateTime myStartDate = mainReport.Start;
			DateTime myEndDate = mainReport.End;

			DateTime startOfMonth = mainReport.Start.AddMonths(j);
			DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
			model.FilterByMonth.SelYear = mainReport.Start.AddMonths(j).Year;




			int serType = subReport.AppBudgetService.Service.ServiceType.Id; //DCC

			var clients = db.Clients.Where(Permissions.ClientsFilter);

			var q = from c in clients
					where c.AgencyId == model.AgencyId
					join a in db.SoupKitchensReports.Where(f => f.SubReportId == model.Id) on c.Id equals a.ClientId into ag
					from a in ag.DefaultIfEmpty()
					where a != null ||
					((c.JoinDate < myEndDate) &&
						(c.LeaveDate == null || c.LeaveDate >= myStartDate)//==true
					)
					select new
					{
						Id = (int?)a.Id,
						ClientId = c.Id,
						ClientName = c.FirstName + " " + c.LastName,
						SubReportId = (int?)a.SubReportId,
						mv = from v in a.SKMembersVisits.Where(f => f.ReportDate >= startOfMonth && f.ReportDate <= endOfMonth)
							 select new
							 {
								 ReportDate = v.ReportDate,

							 }
							   ,
						ClientFirstName = c.FirstName,
						ClientLastName = c.LastName,
						ApprovalStatus = c.ApprovalStatus.Name,
					};


			var filtered = q;
			if (!string.IsNullOrWhiteSpace(model.Filter.ClientName)) { filtered = filtered.Where(f => f.ClientName.Contains(model.Filter.ClientName)); }
			if (model.Filter.ClientId.HasValue) { filtered = filtered.Where(f => f.ClientId == model.Filter.ClientId); }
			if (model.Filter.ReportedOnly == true) { filtered = filtered.Where(f => f.Id != null); }
			if (!string.IsNullOrWhiteSpace(model.sSearch))
			{
				foreach (var s in model.sSearch.Split(new char[] { ' ' }).Where(f => !string.IsNullOrWhiteSpace(f)))
				{
					filtered = filtered.Where(f => f.ClientName.Contains(s));
				}
			}


			bool sortAsc = Request != null ? Request["sSortDir_0"] == "asc" : false;
			int? sortColIndex = Request != null ? Request["iSortCol_0"].Parse<int>() : null;


			#region Sort
			switch (sortColIndex)
			{
				case 2:
					if (!sortAsc) { filtered = filtered.OrderByDescending(f => f.ClientFirstName).ThenByDescending(f => f.ClientId); }
					else { filtered = filtered.OrderBy(f => f.ClientFirstName).ThenBy(f => f.ClientId); }
					break;
				case 3:
					if (!sortAsc) { filtered = filtered.OrderByDescending(f => f.ClientLastName).ThenByDescending(f => f.ClientId); }
					else { filtered = filtered.OrderBy(f => f.ClientLastName).ThenBy(f => f.ClientId); }
					break;
				default:
					filtered = filtered.OrderBy(f => f.ClientFirstName).ThenBy(f => f.ClientId);
					break;
			}
			#endregion


			var data = filtered.Skip(model.iDisplayStart).Take(model.iDisplayLength).ToList();

			List<ClientsVisitsModel> cvm = new List<ClientsVisitsModel>();
			foreach (var d in data)
			{


				var item = new ClientsVisitsModel();
				item.ClientId = d.ClientId;
				item.ClientName = d.ClientName;
				item.ClientLastName = d.ClientLastName;
				item.ClientFirstName = d.ClientFirstName;
				item.ClientApprovalStatus = d.ApprovalStatus;
				item.VisitsCount = 0;
				item.Id = (int)(d.Id.HasValue ? d.Id : 0);
				for (int i = 0; i < 31; i++)
				{
					item.dlist.Add(false);

				}

				foreach (var v in d.mv)
				{

					var i = v.ReportDate.Day;
					item.dlist[i - 1] = true;
					item.VisitsCount++;

				}

				cvm.Add(item);

			}

			if (model.Filter.ReportedOnly.HasValue && model.Filter.ReportedOnly.Value)
			{
				cvm = cvm.Where(f => f.VisitsCount > 0 || f.Amount != null).ToList();
			}

			var aaData = new List<object[]>();
			foreach (var l in cvm)
			{
				aaData.Add(new object[] { l.ClientId, l.ClientName, l.ClientLastName, l.ClientFirstName, l.ClientApprovalStatus, l.dlist[0],
            l.dlist[1],l.dlist[2],l.dlist[3],
            l.dlist[4],l.dlist[5],l.dlist[6],
            l.dlist[7],l.dlist[8],l.dlist[9],
            l.dlist[10],l.dlist[11],l.dlist[12],
            l.dlist[13],l.dlist[14],l.dlist[15],
            l.dlist[16],l.dlist[17],l.dlist[18],
            l.dlist[19],l.dlist[20],l.dlist[21],
            l.dlist[22],l.dlist[23],l.dlist[24],
            l.dlist[25],l.dlist[26],l.dlist[27],
            l.dlist[28],l.dlist[29], l.dlist[30], 
            l.VisitsCount,l.Amount,l.Id});

			}

			model.iTotalDisplayRecords = filtered.Count();
			model.iTotalRecords = filtered.Count();
			model.aaData = aaData;



			return this.MyJsonResult(model, JsonRequestBehavior.AllowGet);
		}

		private JsonResult GetEmergencyReportRows(SubReportDetailsJqDt model, SubReport subReport)
		{
			var mainReport = subReport.MainReport;


			DateTime myStartDate = mainReport.Start;
			DateTime myEndDate = mainReport.End;
			int serType = subReport.AppBudgetService.Service.ServiceType.Id;





			var q = from c in db.Clients.Where(Permissions.ClientsFilter)
					where c.AgencyId == model.AgencyId  && c.ApprovalStatusId != 1024 //&& c.DeceasedDate == null //remove Approved HomecareOnly LK
                    join a in db.EmergencyReports.Where(f => f.SubReportId == model.Id) on c.Id equals a.ClientId into ag
					from a in ag.DefaultIfEmpty()
					where !(c.Agency.AgencyGroup.Country.IncomeVerificationRequired && (c.FundStatusId == null || c.FundStatus.IncomeVerificationRequired)
							&& !(c.Agency.AgencyGroup.SupportiveCommunities || c.Agency.AgencyGroup.DayCenter)) || c.IncomeCriteriaComplied || a != null
					where a != null ||
					(
                        (c.DeceasedDate >= subReport.MainReport.Start && c.DeceasedDate < subReport.MainReport.End) ||
                       (c.JoinDate < subReport.MainReport.End) || 
						(c.AustrianEligible || c.RomanianEligible || 
                        c.LeaveDate == null ||
                        //(c.LeaveDate != null && c.LeaveReasonId==8 )  || //LK_NursingHome
                        System.Data.Objects.EntityFunctions.AddDays(c.LeaveDate, c.DeceasedDate == null ? 0 : SubReport.EAPDeceasedDaysOverhead) >= subReport.MainReport.Start) //||
                         
                    //System.Data.Objects.EntityFunctions.AddDays(c.LeaveDate, c.DeceasedDate == null ? 0 : SubReport.EAPDeceasedDaysOverhead) >= subReport.MainReport.Start)
                        //&& (c.LeaveDate !=null && c.LeaveReasonId ==8) &&
                        //==true
                    )
					select new
					{
						Id = (int?)a.Id,
						ClientId = c.Id,
						ClientFirstName = c.FirstName,
						ClientLastName = c.LastName,
						ClientApprovalStatus = c.ApprovalStatus.Name,
						ClientName = c.FirstName + " " + c.LastName,
						SubReportId = (int?)a.SubReportId,
						ReportDate = (DateTime?)a.ReportDate,
						Amount = (decimal?)a.Amount,
						Discretionary = (decimal?)a.Discretionary,
						Total = (decimal?)a.Total,
						TypeId = (int?)a.TypeId,
						TypeName = a.EmergencyReportType.Name,
						Remarks = a.Remarks,
						UniqueCircumstances = a.UniqueCircumstances,
						AustrianEligible = c.AustrianEligible,
						RomanianEligible = c.RomanianEligible
					};


			var filtered = q;
			if (!string.IsNullOrWhiteSpace(model.Filter.ClientName)) { filtered = filtered.Where(f => f.ClientName.Contains(model.Filter.ClientName)); }
			if (model.Filter.ClientId.HasValue) { filtered = filtered.Where(f => f.ClientId == model.Filter.ClientId); }
			if (model.Filter.ReportedOnly == true) { filtered = filtered.Where(f => f.Id != null); }
			if (!string.IsNullOrWhiteSpace(model.sSearch))
			{
				foreach (var s in model.sSearch.Split(new char[] { ' ' }).Where(f => !string.IsNullOrWhiteSpace(f)))
				{
					filtered = filtered.Where(f => f.ClientName.Contains(s) || f.TypeName.Contains(s));
				}
			}
           // if (model.Filter.ReportedOnly == null)
          //  {
              //  if (subReport.AppBudgetService.AppBudget.App.Fund.AustrianEligibleOnly)
                //{
                //	filtered = filtered.Where(f => f.AustrianEligible);
               // }


               // if (subReport.AppBudgetService.AppBudget.App.Fund.RomanianEligibleOnly)
              //  {
               // 	filtered = filtered.Where(f => f.RomanianEligible);
              // }
           // }

                bool sortAsc = Request != null ? Request["sSortDir_0"] == "asc" : false;
			int? sortColIndex = Request != null ? Request["iSortCol_0"].Parse<int>() : null;


			#region Sort
			switch (sortColIndex)
			{
				case 2:
					if (!sortAsc) { filtered = filtered.OrderByDescending(f => f.ClientFirstName); }
					else { filtered = filtered.OrderBy(f => f.ClientFirstName); }
					break;
				case 3:
					if (!sortAsc) { filtered = filtered.OrderByDescending(f => f.ClientLastName); }
					else { filtered = filtered.OrderBy(f => f.ClientLastName); }
					break;
				default:
					filtered = filtered.OrderBy(f => f.ClientFirstName);
					break;
			}
			#endregion

			model.iTotalDisplayRecords = filtered.Count();
			model.iTotalRecords = filtered.Count();
			model.aaData = filtered.Skip(model.iDisplayStart).Take(model.iDisplayLength).ToList()
				.Select(f => new object[]{
				f.Id,
				f.TypeId,
                f.ClientFirstName,
                f.ClientLastName,
				f.ClientApprovalStatus,
				f.ClientId,
				f.ReportDate,
				f.TypeName,
				f.Remarks,
				f.Amount,
				f.Discretionary,
				f.Total,
                f.UniqueCircumstances
				});
			return this.MyJsonResult(model, JsonRequestBehavior.AllowGet);
		}

		public JsonResult EmergencyTypes(int clientId, int subReportId)
		{
			var q = from t in db.EmergencyReportTypes
					where !t.EmergencyReports.Any(f => f.SubReportId == subReportId)
					select t;
			return this.MyJsonResult(q.ToList(), JsonRequestBehavior.AllowGet);
		}


		[CcAuthorize(FixedRoles.Admin, FixedRoles.AgencyUser, FixedRoles.Ser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult SaveDaysCentersReport(ClientsVisitsByDays report)
		{
			db.ContextOptions.LazyLoadingEnabled = false;
			db.ContextOptions.ProxyCreationEnabled = false;

			//fetch the subreport and mainreport for validate()
			var subReport = db.SubReports
				.Include(f => f.MainReport).Include(f => f.MainReport.AppBudget).Include(f => f.MainReport.AppBudget.App).Include(f => f.MainReport.AppBudget.App.AgencyGroup)
				.Where(this.Permissions.SubReportsFilter).Single(f => f.Id == report.SubReportId);

			var Client = db.Clients.Where(c => c.Id == report.ClientId).Include(f => f.ApprovalStatus).SingleOrDefault();

			var days = new List<bool>();
			days.Add(report.dlist1);
			days.Add(report.dlist2);
			days.Add(report.dlist3);
			days.Add(report.dlist4);
			days.Add(report.dlist5);
			days.Add(report.dlist6);
			days.Add(report.dlist7);
			days.Add(report.dlist8);
			days.Add(report.dlist9);
			days.Add(report.dlist10);
			days.Add(report.dlist11);
			days.Add(report.dlist12);
			days.Add(report.dlist13);
			days.Add(report.dlist14);
			days.Add(report.dlist15);
			days.Add(report.dlist16);
			days.Add(report.dlist17);
			days.Add(report.dlist18);
			days.Add(report.dlist19);
			days.Add(report.dlist20);
			days.Add(report.dlist21);
			days.Add(report.dlist22);
			days.Add(report.dlist23);
			days.Add(report.dlist24);
			days.Add(report.dlist25);
			days.Add(report.dlist26);
			days.Add(report.dlist27);
			days.Add(report.dlist28);
			days.Add(report.dlist29);
			days.Add(report.dlist30);
			days.Add(report.dlist31);

			////clear
			ModelState.Clear();

			////and validate again
			TryValidateModel(report);

			if (ModelState.IsValid)
			{
				try
				{
					DateTime startOfMonth = subReport.MainReport.Start.AddMonths(report.selMonth - subReport.MainReport.Start.Month);
					DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

					DateTime d = startOfMonth;
					var Visits = new List<DateTime>();
					for (int i = 1; i <= DateTime.DaysInMonth(startOfMonth.Year, startOfMonth.Month); i++)
					{
						if (days[i - 1])
							Visits.Add(new DateTime(startOfMonth.Year, startOfMonth.Month, i));
					}

					var DCC_subsidy = System.Web.Configuration.WebConfigurationManager.AppSettings["DCC_subsidy"].Parse<int>();
                    using (var ts = new System.Transactions.TransactionScope())
                    {
                        var q = db.DccMemberVisits.Where(f => f.SubReport.Id == subReport.Id && f.ClientId == report.ClientId && f.ReportDate >= startOfMonth && f.ReportDate <= endOfMonth);

                        if (q.Any())
                        {
                            foreach (var v in q)
                            {
                                db.DccMemberVisits.DeleteObject(v);
                            }
                        }

                        foreach (DateTime visitDate in Visits)
                        {
                            DccMemberVisit dmv = new DccMemberVisit();
                            dmv.SubReport = subReport;
                            dmv.Client = Client;
                            dmv.Id = report.Id;
                            dmv.ReportDate = visitDate;
                            dmv.SubReportId = report.SubReportId;
                            dmv.ClientId = report.ClientId;
                            db.DccMemberVisits.AddObject(dmv);
                        }

                        db.SaveChanges();
                        

                        DaysCentersReport dcr = new DaysCentersReport();
                        dcr.SubReport = subReport;
                        dcr.Client = Client;
                        dcr.ClientId = report.ClientId;
                        dcr.SubReportId = report.SubReportId;
                        dcr.Id = report.Id;
                        dcr.SubsidesByDcc = DCC_subsidy;
                        dcr.VisitsCount = db.DccMemberVisits.Where(v => v.ClientId == report.ClientId && v.SubReportId == report.SubReportId).Count();
                        dcr.VisitCost = Client.DCC_VisitCost;
                        dcr.Amount = dcr.SubsidesByDcc * dcr.VisitsCount;

                        if (dcr.VisitsCount > 0)
                        {
                            if (!db.DaysCentersReports.Any(f => f.ClientId == report.ClientId && f.SubReportId == report.SubReportId))
                            {
                                db.DaysCentersReports.AddObject(dcr);
                            }
                            else
                            {
                                db.DaysCentersReports.Attach(dcr);
                                db.ObjectStateManager.ChangeObjectState(dcr, System.Data.EntityState.Modified);
                            }
                        }
                        else if (db.DaysCentersReports.Any(f => f.ClientId == report.ClientId && f.SubReportId == report.SubReportId))
                        {
                            db.DaysCentersReports.Attach(dcr);
                            db.ObjectStateManager.ChangeObjectState(dcr, System.Data.EntityState.Deleted);
                        }

                        db.SaveChanges();
                        ts.Complete();

                        var row = new ClientsVisitsByDays();
                        var rep = dcr;

                        var dto = new object[]
                        {
                        rep.ClientId,
                        Client.FirstName + ' ' + Client.LastName,
                        Client.LastName,
                        Client.FirstName,
                        Client.ApprovalStatus.Name,
                        report.dlist1,
                        report.dlist2,
                        report.dlist3,
                        report.dlist4,
                        report.dlist5,
                        report.dlist6,
                        report.dlist7,
                        report.dlist8,
                        report.dlist9,
                        report.dlist10,
                        report.dlist11,
                        report.dlist12,
                        report.dlist13,
                        report.dlist14,
                        report.dlist15,
                        report.dlist16,
                        report.dlist17,
                        report.dlist18,
                        report.dlist19,
                        report.dlist20,
                        report.dlist21,
                        report.dlist22,
                        report.dlist23,
                        report.dlist24,
                        report.dlist25,
                        report.dlist26,
                        report.dlist27,
                        report.dlist28,
                        report.dlist29,
                        report.dlist30,
                        report.dlist31,
                        Visits.Count(),
                        rep.Id
                        };

                        return this.MyJsonResult(new { success = true, data = dto });
                    }
				}
				catch (Exception ex)
				{
					_log.Debug(ex.Message, ex);
					return this.MyJsonResult(new { success = false, errors = new[] { ex.Message } });
				}
			}
			else
			{
				return this.MyJsonResult(new { success = false, errors = ModelState.ValidationErrorMessages() });
			}

		}

		[CcAuthorize(FixedRoles.Admin, FixedRoles.AgencyUser, FixedRoles.Ser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult DeleteDaysCentersClient(ClientsVisitsByDays report)
		{
			var dcr = db.DaysCentersReports.SingleOrDefault(f => f.SubReportId == report.SubReportId && f.ClientId == report.ClientId);
			var client = db.Clients.SingleOrDefault(f => f.Id == report.ClientId);
			var mainReport = (from mr in db.MainReports
							  join sr in db.SubReports on mr.Id equals sr.MainReportId
							  where sr.Id == report.SubReportId
							  select mr).SingleOrDefault();

			try
			{
				if (db.DccMemberVisits.Where(f => f.SubReportId == report.SubReportId && f.ClientId == report.ClientId).Count() > 0)
				{
					var visitsCount = db.DccMemberVisits.Where(f => f.SubReportId == report.SubReportId && f.ClientId == report.ClientId).Count();
					for (int i = 0; i < visitsCount; i++)
					{
						db.DccMemberVisits.DeleteObject(db.DccMemberVisits.Where(f => f.SubReportId == report.SubReportId && f.ClientId == report.ClientId).OrderBy(f => f.ReportDate).Skip(i).FirstOrDefault());
					}
				}
				if (dcr != null)
				{
					db.DaysCentersReports.DeleteObject(dcr);
				}
				string content = string.Format("The client {0} (CCID: {1}) was deleted from detailed report (id: {2})", client.FirstName + " " + client.LastName, client.Id, report.SubReportId);
				mainReport.InternalComments.Add(new Comment
				{
					Content = content,
					UserId = this.CcUser.Id,
					Date = DateTime.Now
				});
				db.SaveChanges();
				return this.MyJsonResult(new { success = true });
			}
			catch (Exception ex)
			{
				_log.Debug(ex.Message, ex);
				return this.MyJsonResult(new { success = false, errors = new[] { ex.Message } });
			}

		}

		[CcAuthorize(FixedRoles.Admin, FixedRoles.AgencyUser, FixedRoles.Ser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult SaveSoupKitchensReport(ClientsVisitsByDays report)
		{
			db.ContextOptions.LazyLoadingEnabled = false;
			db.ContextOptions.ProxyCreationEnabled = false;

			//fetch the subreport and mainreport for validate()
			var subReport = db.SubReports
				.Include(f => f.AppBudgetService)
				.Include(f => f.MainReport).Include(f => f.MainReport.AppBudget).Include(f => f.MainReport.AppBudget.App).Include(f => f.MainReport.AppBudget.App.AgencyGroup)
				.Where(this.Permissions.SubReportsFilter).Single(f => f.Id == report.SubReportId);

			var Client = db.Clients.Where(c => c.Id == report.ClientId).Include(f => f.ApprovalStatus).SingleOrDefault();

			var days = new List<bool>();
			days.Add(report.dlist1);
			days.Add(report.dlist2);
			days.Add(report.dlist3);
			days.Add(report.dlist4);
			days.Add(report.dlist5);
			days.Add(report.dlist6);
			days.Add(report.dlist7);
			days.Add(report.dlist8);
			days.Add(report.dlist9);
			days.Add(report.dlist10);
			days.Add(report.dlist11);
			days.Add(report.dlist12);
			days.Add(report.dlist13);
			days.Add(report.dlist14);
			days.Add(report.dlist15);
			days.Add(report.dlist16);
			days.Add(report.dlist17);
			days.Add(report.dlist18);
			days.Add(report.dlist19);
			days.Add(report.dlist20);
			days.Add(report.dlist21);
			days.Add(report.dlist22);
			days.Add(report.dlist23);
			days.Add(report.dlist24);
			days.Add(report.dlist25);
			days.Add(report.dlist26);
			days.Add(report.dlist27);
			days.Add(report.dlist28);
			days.Add(report.dlist29);
			days.Add(report.dlist30);
			days.Add(report.dlist31);

			////clear
			ModelState.Clear();

			////and validate again
			TryValidateModel(report);

			if (ModelState.IsValid)
			{
				try
				{
					DateTime startOfMonth = subReport.MainReport.Start.AddMonths(report.selMonth - subReport.MainReport.Start.Month);
					DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

					DateTime d = startOfMonth;
					var Visits = new List<DateTime>();
					for (int i = 1; i <= DateTime.DaysInMonth(startOfMonth.Year, startOfMonth.Month); i++)
					{
						if (days[i - 1])
							Visits.Add(new DateTime(startOfMonth.Year, startOfMonth.Month, i));
					}

					var soupKitchenReport = db.SoupKitchensReports.Where(Permissions.SoupKitchensReportsFilter)
						.Include(f => f.SKMembersVisits)
						.SingleOrDefault(f => f.SubReportId == subReport.Id && f.ClientId == report.ClientId);

					if (soupKitchenReport == null)
					{
						soupKitchenReport = new SoupKitchensReport()
						{
							SubReportId = subReport.Id,
							ClientId = report.ClientId
						};
						db.SoupKitchensReports.AddObject(soupKitchenReport);
					}

					if (soupKitchenReport.SKMembersVisits.Any(f => f.ReportDate.Month == startOfMonth.Month))
					{
						var skVisits = soupKitchenReport.SKMembersVisits.Where(f => f.ReportDate.Month == startOfMonth.Month).Count();
						for (var i = 0; i < skVisits; i++)
						{
							db.SKMembersVisits.DeleteObject(soupKitchenReport.SKMembersVisits.Where(f => f.ReportDate.Month == startOfMonth.Month).FirstOrDefault());
						}
					}

					List<DateTime> reportedDates = new List<DateTime>();
					foreach (DateTime visitDate in Visits)
					{
						var alreadyReported = from sr in db.SubReports
											  join skr in db.SoupKitchensReports on sr.Id equals skr.SubReportId
											  join c in db.Clients on skr.ClientId equals c.Id
											  where sr.AppBudgetService.Service.TypeId == (int)Service.ServiceTypes.SoupKitchens && sr.Id != subReport.Id
														&& c.Id == Client.Id && skr.SKMembersVisits.Any(f => f.ReportDate == visitDate)
											  select skr;
						if (alreadyReported.Any())
						{
							reportedDates.Add(visitDate);
						}
						else
						{
							var skMembersVisits = soupKitchenReport.SKMembersVisits.SingleOrDefault(f => f.ReportDate == visitDate);
							if (skMembersVisits == null)
							{
								skMembersVisits = new SKMembersVisit() { ReportDate = visitDate };
								soupKitchenReport.SKMembersVisits.Add(skMembersVisits);
							}
						}
					}

					if (reportedDates.Count > 0)
					{
						return this.MyJsonResult(new { success = false, errors = new[] { "This client was reported on this date/s (" + string.Join(", ", reportedDates.Select(f => f.ToShortDateString())) + ")" } });
					}

					// total reported in this main report including duplicates, but not in the same sub report
					var totalReportedThis = (from skr in db.SoupKitchensReports
											 join sk in db.SKMembersVisits on skr.Id equals sk.SKReportId
											 where skr.SubReportId != subReport.Id && skr.SubReport.MainReportId == subReport.MainReportId
												   && skr.SubReport.AppBudgetService.Service.TypeId == (int)Service.ServiceTypes.SoupKitchens
													&& skr.Client.MasterIdClcd == Client.MasterIdClcd
											 select new
											 {
												 sk.ReportDate
											 }).Count();

					// total reported not in this main report including duplicates
					var totalReported = (from skr in db.SoupKitchensReports
										 join sk in db.SKMembersVisits on skr.Id equals sk.SKReportId
										 join sr in db.SubReports on skr.SubReportId equals sr.Id
										 join mr in db.MainReports.Where(MainReport.Submitted) on sr.MainReportId equals mr.Id
										 where sk.ReportDate < subReport.MainReport.End && sk.ReportDate >= subReport.MainReport.Start && skr.SubReport.MainReportId != subReport.MainReportId
												&& skr.SubReport.AppBudgetService.Service.TypeId == (int)Service.ServiceTypes.SoupKitchens
												&& skr.Client.MasterIdClcd == Client.MasterIdClcd
										 select new
										 {
											 sk.ReportDate
										 }).Count();
					totalReported += totalReportedThis + Visits.Count() +
						db.SoupKitchensReports.Where(f => f.SubReportId == subReport.Id && f.ClientId == report.ClientId).Select(f => f.SKMembersVisits.Where(sk => sk.ReportDate < startOfMonth || sk.ReportDate > endOfMonth).Count()).SingleOrDefault();
					//var startOfYear = new DateTime(subReport.MainReport.Start.Year, 1, 1);
					var hcep = (from h in db.HomeCareEntitledPeriods
								where h.ClientId == Client.Id && h.StartDate < subReport.MainReport.End && (h.EndDate == null || h.EndDate > subReport.MainReport.Start)
								let earlyEndDate = h.EndDate < Client.LeaveDate || h.EndDate != null && Client.LeaveDate == null ? h.EndDate : Client.LeaveDate
								let maxStartDate = h.StartDate >= Client.JoinDate ? h.StartDate : Client.JoinDate
								select new
								{
									h.ClientId,
									StartDate = maxStartDate >= subReport.MainReport.Start ? maxStartDate : subReport.MainReport.Start,
									EndDate = earlyEndDate < subReport.MainReport.End ? earlyEndDate : subReport.MainReport.End
								} into hc
								group hc by hc.ClientId into hcg
								select new
								{
									ClientId = hcg.Key,
									DaysCount = (double)hcg.Sum(f => SqlFunctions.DateDiff("DAY", f.StartDate, f.EndDate))
								}).SingleOrDefault();
					var daysCount = ((Client.LeaveDate.HasValue && Client.LeaveDate.Value < subReport.MainReport.End ? Client.LeaveDate.Value : subReport.MainReport.End) - (Client.JoinDate > subReport.MainReport.Start ? Client.JoinDate : subReport.MainReport.Start)).TotalDays;
					var hcepDaysCount = hcep != null ? hcep.DaysCount : 0;
					var daysCap = hcepDaysCount < daysCount ? (hcepDaysCount < 0 ? 0 : hcepDaysCount) : (daysCount < 0 ? 0 : daysCount);
					if (totalReported > daysCap)
					{
						return this.MyJsonResult(new
						{
							success = false,
							errors = new[] { string.Format("CC ID {0} has been reported for more meals than are allowed in this reporting period. Allowed meals – {1}, Reported meals - {2}", 
							report.ClientId, daysCap, totalReported) }
						});
					}

					if (!soupKitchenReport.SKMembersVisits.Any())
					{
						db.SoupKitchensReports.DeleteObject(soupKitchenReport);
					}

					db.SaveChanges();

					var row = new ClientsVisitsByDays();

					var dto = new object[]
                    {
                        report.ClientId,
                        Client.FirstName + ' ' + Client.LastName,
						Client.LastName,
						Client.FirstName,
                        Client.ApprovalStatus.Name,
                        report.dlist1, 
                        report.dlist2,
                        report.dlist3,
                        report.dlist4,
                        report.dlist5,
                        report.dlist6,
                        report.dlist7,
                        report.dlist8,
                        report.dlist9,
                        report.dlist10,
                        report.dlist11, 
                        report.dlist12,
                        report.dlist13,
                        report.dlist14,
                        report.dlist15,
                        report.dlist16,
                        report.dlist17,
                        report.dlist18,
                        report.dlist19,
                        report.dlist20,
                        report.dlist21, 
                        report.dlist22,
                        report.dlist23,
                        report.dlist24,
                        report.dlist25,
                        report.dlist26,
                        report.dlist27,
                        report.dlist28,
                        report.dlist29,
                        report.dlist30,
                        report.dlist31,
                        Visits.Count(),
                        soupKitchenReport.Id
                    };

					return this.MyJsonResult(new { success = true, data = dto });
				}
				catch (Exception ex)
				{
					_log.Debug(ex.Message, ex);
					return this.MyJsonResult(new { success = false, errors = new[] { ex.Message } });
				}
			}
			else
			{
				return this.MyJsonResult(new { success = false, errors = ModelState.ValidationErrorMessages() });
			}

		}

		[CcAuthorize(FixedRoles.Admin, FixedRoles.AgencyUser, FixedRoles.Ser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult DeleteSoupKitchensClient(ClientsVisitsByDays report)
		{
			var skreport = db.SoupKitchensReports.Where(Permissions.SoupKitchensReportsFilter).Include(f => f.SKMembersVisits).SingleOrDefault(f => f.SubReportId == report.SubReportId && f.ClientId == report.ClientId);
			var client = db.Clients.SingleOrDefault(f => f.Id == report.ClientId);
			var mainReport = (from mr in db.MainReports
							  join sr in db.SubReports on mr.Id equals sr.MainReportId
							  where sr.Id == report.SubReportId
							  select mr).SingleOrDefault();
			if (skreport != null)
			{
				try
				{
					DateTime startOfMonth = mainReport.Start.AddMonths(report.selMonth - mainReport.Start.Month);
					DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
					var visits = skreport.SKMembersVisits.Where(f => f.ReportDate >= startOfMonth && f.ReportDate <= endOfMonth).OrderBy(f => f.ReportDate);
					var visitsCount = visits.Count();
					if (visitsCount > 0)
					{
						for (int i = 0; i < visitsCount; i++)
						{
							db.SKMembersVisits.DeleteObject(visits.FirstOrDefault());
						}
					}
					if (skreport != null && skreport.SKMembersVisits.Count == 0)
					{
						db.SoupKitchensReports.DeleteObject(skreport);
					}

					db.SaveChanges();
					return this.MyJsonResult(new { success = true });
				}
				catch (Exception ex)
				{
					_log.Debug(ex.Message, ex);
					return this.MyJsonResult(new { success = false, errors = new[] { ex.Message } });
				}
			}
			return this.MyJsonResult(new { success = true });
		}

		[CcAuthorize(FixedRoles.Admin, FixedRoles.AgencyUser, FixedRoles.Ser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult SaveSupportiveCommunitiesReport(SupportiveCommunitiesReport report)
		{
			db.ContextOptions.LazyLoadingEnabled = false;
			db.ContextOptions.ProxyCreationEnabled = false;

			//fetch the subreport and mainreport for the emergencyreport.validate()
			var subReport = db.SubReports
				.Include(f => f.MainReport).Include(f => f.MainReport.AppBudget).Include(f => f.MainReport.AppBudget.App).Include(f => f.MainReport.AppBudget.App.AgencyGroup)
				.Where(this.Permissions.SubReportsFilter).Single(f => f.Id == report.SubReportId);
			report.SubReport = subReport;
			var SC_subsidy = System.Web.Configuration.WebConfigurationManager.AppSettings["SC_subsidy"].Parse<int>();

			var Client = db.Clients.SingleOrDefault(f => f.Id == report.ClientId);
            var CurrentClientId = report.ClientId;
			var vcr = db.viewScRepSources.SingleOrDefault(f => f.clientid == report.ClientId && f.subreportid == report.SubReportId);
			var months = vcr != null ? vcr.MonthsCount : 0;
			if (report.MonthsCount < 0 || report.MonthsCount > months)
			{
				return this.MyJsonResult(new { success = false, errors = new[] { "Invallid number of months. The months allowed for this client is between 0 and " + months } });
			}
            //LenaK
            var NationalId = Client.NationalId;
            var ClientsPerNationalId = (from cl in db.Clients
                                        where cl.NationalId == NationalId
                                        select cl.Id).ToList();
            
            var ClientsCount = ClientsPerNationalId.Count();
            var ReportStart = subReport.MainReport.Start;
            int MonthCountClientPerNationalIdFinal = 0;
            
            for (int i = 0; i < ClientsCount; i++)
            {
               
                var Clientid = ClientsPerNationalId[i];
                var MonthCountPerNationalId = (from sucr in db.SupportiveCommunitiesReports//sucr in db.viewScRepSources//from sucr in db.SupportiveCommunitiesReports
                                               join sr in db.SubReports on sucr.SubReportId equals sr.Id
                                               join mr in db.MainReports on sr.MainReportId equals mr.Id
                                               where sucr.ClientId == Clientid && Clientid!= CurrentClientId && mr.Start == ReportStart 
                                               select sucr.MonthsCount).Sum();
                MonthCountPerNationalId = MonthCountPerNationalId != null ? MonthCountPerNationalId : 0;
                MonthCountClientPerNationalIdFinal = MonthCountPerNationalId.Value + MonthCountClientPerNationalIdFinal;
            }
            MonthCountClientPerNationalIdFinal = MonthCountClientPerNationalIdFinal + report.MonthsCount.Value;
            if (MonthCountClientPerNationalIdFinal > 3)
            {
                return this.MyJsonResult(new { success = false, errors = new[] { "Israel Id: " + NationalId + " exceedes 3 visits per Quater"} });
                
            }
            //LenaK



            var scr = db.SupportiveCommunitiesReports.SingleOrDefault(f => f.Id == report.Id);
			if (!User.IsInRole(FixedRoles.Admin) && !User.IsInRole(FixedRoles.Ser) && !User.IsInRole(FixedRoles.AgencyUser) && !User.IsInRole(FixedRoles.SerAndReviewer) && !User.IsInRole(FixedRoles.AgencyUserAndReviewer) && scr.MonthsCount != report.MonthsCount)
			{
				return this.MyJsonResult(new { success = false, errors = new[] { "You are not allowed to change the months reported" } });
			}
			report.Amount = null;
			if (!report.HoursHoldCost.HasValue || !vcr.HoursHoldCost.HasValue   && !User.IsInRole(FixedRoles.Admin))
			{
				report.HoursHoldCost = scr.HoursHoldCost;
			}

			//clear
			ModelState.Clear();

			//and validate again
			TryValidateModel(report);

			if (report.Id == default(int))
			{
				db.SupportiveCommunitiesReports.AddObject(report);
			}
			else
			{
				db.SupportiveCommunitiesReports.Detach(scr);
				db.SupportiveCommunitiesReports.Attach(report);
				db.ObjectStateManager.ChangeObjectState(report, System.Data.EntityState.Modified);
			}
			if (ModelState.IsValid)
			{
				try
				{
					db.SaveChanges();
					var rep = db.SupportiveCommunitiesReports.Where(this.Permissions.SupportiveCommunitiesReportsFilter).Select(a => new
					{
						Id = (int?)a.Id,
						ClientId = a.ClientId,
						IsraelId = a.Client.NationalId,
						ClientFirstName = a.Client.FirstName,
						ClientLastName = a.Client.LastName,
						ClientApprovalStatus = a.Client.ApprovalStatus.Name,
						JoinDate = a.Client.JoinDate,
						SubReportId = (int?)a.SubReportId,
						HoursHoldCost = (a.HoursHoldCost) ?? (a.Amount / a.MonthsCount),
						Amount = (decimal?)a.Amount,
						MonthsCount = a.MonthsCount

					}).Single(f => f.Id == report.Id);

					var dto = new object[]
					{
						rep.Id,
					    rep.ClientId,
                        rep.IsraelId,
                        rep.ClientFirstName,
                        rep.ClientLastName,
                        rep.ClientApprovalStatus,
						rep.JoinDate,
						rep.HoursHoldCost,
                        rep.MonthsCount,
						rep.Amount,
						true //reported
				       
					};

					return this.MyJsonResult(new { success = true, data = dto });
				}
				catch (Exception ex)
				{
					_log.Debug(ex.Message, ex);
					return this.MyJsonResult(new { success = false, errors = new[] { ex.Message } });
				}
			}
			else
			{
				return this.MyJsonResult(new { success = false, errors = ModelState.ValidationErrorMessages() });
			}

		}

		[CcAuthorize(FixedRoles.Admin, FixedRoles.AgencyUser, FixedRoles.Ser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult DeleteSupportiveCommunitiesClient(SupportiveCommunitiesReport report)
		{
			var screport = db.SupportiveCommunitiesReports.SingleOrDefault(f => f.ClientId == report.ClientId && f.SubReportId == report.SubReportId);
			try
			{
				if (screport != null)
				{
					db.SupportiveCommunitiesReports.DeleteObject(screport);
					var client = db.Clients.SingleOrDefault(f => f.Id == report.ClientId);
					var mainReport = (from mr in db.MainReports
									  join sr in db.SubReports on mr.Id equals sr.MainReportId
									  where sr.Id == report.SubReportId
									  select mr).SingleOrDefault();
					string content = string.Format("The client {0} (CCID: {1}) was deleted from detailed report (id: {2})", client.FirstName + " " + client.LastName, client.Id, report.SubReportId);
					mainReport.InternalComments.Add(new Comment
					{
						Content = content,
						UserId = this.CcUser.Id,
						Date = DateTime.Now
					});
					db.SaveChanges();
				}
				return this.MyJsonResult(new { success = true });
			}
			catch (Exception ex)
			{
				_log.Debug(ex.Message, ex);
				return this.MyJsonResult(new { success = false, errors = new[] { ex.Message } });
			}
		}

		[CcAuthorize(FixedRoles.Admin, FixedRoles.AgencyUser, FixedRoles.Ser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult AddSupportiveCommunitiesClient(SupportiveCommunitiesReport report)
		{
			try
			{
				var screport = new SupportiveCommunitiesReport();
				var viewScReport = db.viewScRepSources.SingleOrDefault(f => f.subreportid == report.SubReportId && f.clientid == report.ClientId);
                var Client = db.Clients.SingleOrDefault(f => f.Id == report.ClientId);
                var subReport = db.SubReports
                .Include(f => f.MainReport).Include(f => f.MainReport.AppBudget).Include(f => f.MainReport.AppBudget.App).Include(f => f.MainReport.AppBudget.App.AgencyGroup)
                .Where(this.Permissions.SubReportsFilter).Single(f => f.Id == report.SubReportId);

                screport.ClientId = report.ClientId;
				screport.Amount = viewScReport.Amount;
				screport.HoursHoldCost = viewScReport.HoursHoldCost;
				screport.MonthsCount = viewScReport.MonthsCount;
				screport.SubReportId = report.SubReportId;

                var NationalId = Client.NationalId;
                var ClientsPerNationalId = (from cl in db.Clients
                                            where cl.NationalId == NationalId
                                            select cl.Id).ToList();

                var ClientsCount = ClientsPerNationalId.Count();
                var ReportStart = subReport.MainReport.Start;



                int MonthCountClientPerNationalIdFinal = 0;
                for (int i = 0; i < ClientsCount; i++)
                {

                    var Clientid = ClientsPerNationalId[i];
                    var MonthCountPerNationalId = (from sucr in db.SupportiveCommunitiesReports //sucr in db.viewScRepSources//from sucr in db.SupportiveCommunitiesReports
                                                   join sr in db.SubReports on sucr.SubReportId equals sr.Id
                                                   join mr in db.MainReports on sr.MainReportId equals mr.Id
                                                   where sucr.ClientId == Clientid && mr.Start == ReportStart
                                                   select sucr.MonthsCount).Sum();
                    MonthCountPerNationalId = MonthCountPerNationalId != null ? MonthCountPerNationalId : 0;
                    MonthCountClientPerNationalIdFinal = MonthCountPerNationalId.Value + MonthCountClientPerNationalIdFinal;
                }
                MonthCountClientPerNationalIdFinal = MonthCountClientPerNationalIdFinal + screport.MonthsCount.Value;
                if (MonthCountClientPerNationalIdFinal > 3)
                {
                    return this.MyJsonResult(new { success = false, errors = new[] { "Israel Id: " + NationalId + " exceedes 3 visits per Quater" } });
                    
                }
                else
                { 


                db.SupportiveCommunitiesReports.AddObject(screport);
				db.SaveChanges();

				var rep = db.SupportiveCommunitiesReports.Where(this.Permissions.SupportiveCommunitiesReportsFilter).Select(a => new
				{
					Id = (int?)a.Id,
					ClientId = a.ClientId,
					IsraelId = a.Client.NationalId,
					ClientFirstName = a.Client.FirstName,
					ClientLastName = a.Client.LastName,
					ClientApprovalStatus = a.Client.ApprovalStatus.Name,
					JoinDate = a.Client.JoinDate,
					SubReportId = (int?)a.SubReportId,
					HoursHoldCost = (a.HoursHoldCost) ?? (a.Amount / a.MonthsCount),
					Amount = (decimal?)a.Amount,
					MonthsCount = a.MonthsCount

				}).Single(f => f.Id == screport.Id);
				var dto = new object[]
				{
					rep.Id,
					rep.ClientId,
                    rep.IsraelId,
                    rep.ClientFirstName,
                    rep.ClientLastName,
                    rep.ClientApprovalStatus,
					rep.JoinDate,
					rep.HoursHoldCost,
                    rep.MonthsCount,
					rep.Amount,
					true //reported
				};


                
                    return this.MyJsonResult(new { success = true, data = dto });
                }
			}
			catch (Exception ex)
			{
				_log.Debug(ex.Message, ex);
				return this.MyJsonResult(new { success = false, errors = new[] { ex.Message } });
			}
		}

		[CcAuthorize(FixedRoles.Admin, FixedRoles.AgencyUser, FixedRoles.Ser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult SaveEmergencyReport(EmergencyReport report)
		{
			db.ContextOptions.LazyLoadingEnabled = false;
			db.ContextOptions.ProxyCreationEnabled = false;

			//fetch the subreport and mainreport for the emergencyreport.validate()
			var subReport = db.SubReports
				.Include(f => f.MainReport)
				.Where(this.Permissions.SubReportsFilter).Single(f => f.Id == report.SubReportId);
			report.SubReport = subReport;

			var Client = db.Clients.SingleOrDefault(f => f.Id == report.ClientId);

			//clear
			ModelState.Clear();

			//and validate again
			TryValidateModel(report);

			if (report.Id == default(int))
			{
				db.EmergencyReports.AddObject(report);
			}
			else
			{
				db.EmergencyReports.Attach(report);
				db.ObjectStateManager.ChangeObjectState(report, System.Data.EntityState.Modified);
			}
			if (ModelState.IsValid)
			{
				try
				{
					db.SaveChanges();
					var rep = db.EmergencyReports.Where(this.Permissions.EmergencyReportsFilter).Select(a => new
					{
						Id = (int?)a.Id,
						ClientId = a.ClientId,
						ClientFirstName = a.Client.FirstName,
						ClientLastName = a.Client.LastName,
						ClientApprovalStatus = a.Client.ApprovalStatus.Name,
						SubReportId = (int?)a.SubReportId,
						ReportDate = (DateTime?)a.ReportDate,
						Amount = (decimal?)a.Amount,
						Discretionary = (decimal?)a.Discretionary,
						Total = (decimal?)a.Total,
						TypeId = (int?)a.TypeId,
						TypeName = a.EmergencyReportType.Name,
						Remarks = a.Remarks,
						UniqueCircumstances = a.UniqueCircumstances
					}).Single(f => f.Id == report.Id);

					var dto = new object[]
					{
						rep.Id,
						rep.TypeId,
                        rep.ClientFirstName,
                        rep.ClientLastName,
						rep.ClientApprovalStatus,
						rep.ClientId,
						rep.ReportDate,
						rep.TypeName,
						rep.Remarks,
						rep.Amount,
						rep.Discretionary,
						rep.Total,
                        rep.UniqueCircumstances
					};

					return this.MyJsonResult(new { success = true, data = dto });
				}
				catch (Exception ex)
				{
					_log.Debug(ex.Message, ex);
					return this.MyJsonResult(new { success = false, errors = new[] { ex.Message } });
				}
			}
			else
			{
				return this.MyJsonResult(new { success = false, errors = ModelState.ValidationErrorMessages() });
			}

		}


		[HttpPost]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.AgencyUser, FixedRoles.Ser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult DeleteSupportiveCommunitiesReport(int id)
		{
			var rep = db.SupportiveCommunitiesReports.Where(this.Permissions.SupportiveCommunitiesReportsFilter).SingleOrDefault(f => f.Id == id);
			if (rep != null)
			{
				db.SupportiveCommunitiesReports.DeleteObject(rep);
				db.SaveChanges();
				return new EmptyResult();
			}
			else
			{
				throw new Exception("The report could not be found");
			}
		}

		[HttpPost]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.AgencyUser, FixedRoles.Ser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult DeleteEmergencyReport(int id)
		{
			var rep = db.EmergencyReports.Where(this.Permissions.EmergencyReportsFilter).SingleOrDefault(f => f.Id == id);
			if (rep != null)
			{
				db.EmergencyReports.DeleteObject(rep);
				db.SaveChanges();
				return new EmptyResult();
			}
			else
			{
				throw new Exception("The report could not be found");
			}
		}


		[HttpPost]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.AgencyUser, FixedRoles.Ser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult DeleteDCCReport(int id)
		{

			var q = db.DccMemberVisits.Where(f => f.SubReport.Id == id);

			if (q.Any())
			{
				foreach (var v in q)
				{
					db.DccMemberVisits.DeleteObject(v);
				}
			}

			var rep = db.DaysCentersReports.Where(this.Permissions.DaysCentersReportsFilter).SingleOrDefault(f => f.Id == id);
			if (rep != null)
			{
				db.DaysCentersReports.DeleteObject(rep);
				db.SaveChanges();
				return new EmptyResult();
			}
			else
			{
				throw new Exception("The report could not be found");
			}
		}

		[HttpPost]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.AgencyUser, FixedRoles.Ser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult SaveClientEventsCountReport(ClientEventsCountModel report)
		{
			var mainReport = (from sr in db.SubReports
							  where sr.Id == report.SubReportId
							  select sr.MainReport).SingleOrDefault();
			if (report.EventDate == null || mainReport.Start > report.EventDate || mainReport.End <= report.EventDate)
			{
				return this.MyJsonResult(new
				{
					success = false,
					errors = new[] { "Date of Event is mandatory and must be within the report period" }
				});
			}
			if (report.JNVCount <= 0)
			{
				return this.MyJsonResult(new
				{
					success = false,
					errors = new[] { "Count of JNV attending is mandatory and must be bigger than zero" }
				});
			}
			if (report.TotalCount != null && report.TotalCount < report.JNVCount)
			{
				return this.MyJsonResult(new
				{
					success = false,
					errors = new[] { "Count of Total Attendees must be greater or equal to Count of JNV attending" }
				});
			}
			var rep = db.ClientEventsCountReports.SingleOrDefault(f => f.Id == report.Id);
			if (rep != null)
			{
				rep.EventDate = report.EventDate;
				rep.JNVCount = report.JNVCount;
				rep.TotalCount = report.TotalCount;
				rep.Remarks = report.Remarks;
			}
			else
			{
				rep = new ClientEventsCountReport
				{
					SubReportId = report.SubReportId,
					EventDate = report.EventDate,
					JNVCount = report.JNVCount,
					TotalCount = report.TotalCount,
					Remarks = report.Remarks
				};
				db.ClientEventsCountReports.AddObject(rep);
			}
			try
			{
				db.SaveChanges();
				var dto = new object[]
				{
					rep.Id,
					rep.EventDate,
					rep.JNVCount,
					rep.TotalCount,
					rep.Remarks
				};
				return this.MyJsonResult(new { success = true, data = dto });
			}
			catch (Exception ex)
			{
				_log.Debug(ex.Message, ex);
				return this.MyJsonResult(new { success = false, errors = new[] { ex.Message } });
			}
		}

		[HttpPost]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.AgencyUser, FixedRoles.Ser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult DeleteClientEventsCountRow(int id)
		{
			var rep = db.ClientEventsCountReports.Where(this.Permissions.ClientEventsCountReportsFilter).SingleOrDefault(f => f.Id == id);
			if (rep != null)
			{
				db.ClientEventsCountReports.DeleteObject(rep);
				db.SaveChanges();
				return new EmptyResult();
			}
			else
			{
				throw new Exception("The report could not be found");
			}
		}

        #region subreport specific methods


        private JsonResult getchildren(SubReportDetailsJqDt model, Func<GetChildrenWithoutNameSubClass, object[]> selectFunc)
        {


            var subReport = db.SubReports.Include(f => f.MainReport).Where(Permissions.SubReportsFilter).Single(f => f.Id == model.Id);
            var mainReport = subReport.MainReport;


            DateTime d1 = mainReport.Start;
            DateTime d2 = mainReport.End;
            int serType = subReport.AppBudgetService.Service.ServiceType.Id;
            int ApprovalStatusId;
            if (serType != 8)
            {
                ApprovalStatusId = 1024;
            }
            else
            {
                ApprovalStatusId = 6666; 
            }
                var clientreports = from c in db.Clients.Where(Permissions.ClientsFilter)
                                    where c.ApprovalStatusId != ApprovalStatusId //!= 1024
                                    join cr in db.ClientReports.Where(f => f.SubReportId == subReport.Id) on c.Id equals cr.ClientId into crj
                                    from cr in crj.DefaultIfEmpty()
                                    where
                                    (cr != null ||
                                        (
                                            c.JoinDate < mainReport.End &&

                                            //if client leave reason is deceased then 90 days should be added to the leave date
                                            //leave date must null or be greater than the report end date
                                            //nursing home check box should not be checked
                                            (c.LeaveDate == null || System.Data.Objects.EntityFunctions.AddDays(c.LeaveDate, c.DeceasedDate == null ? 0 : SubReport.DeceasedDaysOverhead) >= subReport.MainReport.Start)//==true
                                            &&


                                            (c.AgencyId == model.AgencyId) &&
                                            (c.AgencyId == subReport.AppBudgetService.AgencyId)
                                           // &&
                                          //  (c.NursingHome != true) ///Nursing home check box is not checked
                                        // &&
                                        // (c.LeaveDate != null && c.LeaveReasonId == 8)
                                        )
                                    )

                                    select new GetChildrenSubClass()
                                    {
                                        Id = (int?)cr.Id,
                                        ClientId = c.Id,
                                        ClientFirstName = c.FirstName,
                                        ClientLastName = c.LastName,
                                        ClientApprovalStatus = c.ApprovalStatus.Name,
                                        ClientName = c.FirstName + " " + c.LastName,
                                        Quantity = cr.Quantity,
                                        Amount = (decimal?)cr.Amount,
                                        HcCaps = db.HcCapsTableRaws
                                                .Where(cap => cap.ClientId == c.Id)
                                                .Where(cap => cap.StartDate <= DateTime.Now)
                                                .Where(cap => cap.EndDate == null || cap.EndDate > DateTime.Now)
                                                .OrderByDescending(cap => cap.StartDate)
                                                .Select(cap => cap.HcCap)
                                                .FirstOrDefault(),
                                        Remarks = cr.Remarks,
                                    };



                var filtered = clientreports
                .WhereIf(f => f.ClientName.Contains(model.Filter.ClientName), !string.IsNullOrWhiteSpace(model.Filter.ClientName))
                .WhereIf(f => f.ClientId == model.Filter.ClientId, model.Filter.ClientId.HasValue)
                .WhereIf(f => f.Id.HasValue, model.Filter.ReportedOnly == true);

                if (!string.IsNullOrWhiteSpace(model.sSearch))
                {
                    foreach (var s in model.sSearch.Split(new char[] { ' ' }).Where(f => !string.IsNullOrWhiteSpace(f)))
                    {
                        filtered = filtered.Where(f => f.ClientFirstName.Contains(s) || f.ClientLastName.Contains(s) || f.Remarks.Contains(s));
                    }
                }

                var afterfilt = from f in filtered
                                select new GetChildrenWithoutNameSubClass()
                                {
                                    Id = (int?)f.Id,
                                    ClientId = f.ClientId,
                                    ClientFirstName = f.ClientFirstName,
                                    ClientLastName = f.ClientLastName,
                                    ClientApprovalStatus = f.ClientApprovalStatus,
                                    Quantity = f.Quantity,
                                    HcCaps = f.HcCaps,
                                    Amount = (decimal?)f.Amount,
                                    Remarks = f.Remarks
                                };


                bool sortAsc = Request != null ? Request["sSortDir_0"] == "asc" : false;
                int? sortColIndex = Request != null ? Request["iSortCol_0"].Parse<int>() : null;


                #region Sort
                switch (sortColIndex)
                {
                    case 1:
                        if (!sortAsc) { afterfilt = afterfilt.OrderByDescending(f => f.ClientFirstName); }
                        else { afterfilt = afterfilt.OrderBy(f => f.ClientFirstName); }
                        break;
                    case 2:
                        if (!sortAsc) { afterfilt = afterfilt.OrderByDescending(f => f.ClientLastName); }
                        else { afterfilt = afterfilt.OrderBy(f => f.ClientLastName); }
                        break;
                    default:
                        afterfilt = afterfilt.OrderBy(f => f.ClientFirstName);
                        break;
                }
                #endregion


                model.aaData = afterfilt
                    .Skip(model.iDisplayStart).Take(model.iDisplayLength)
                    .ToList().Select(selectFunc);
                model.iTotalRecords = clientreports.Count();
                model.iTotalDisplayRecords = filtered.Count();


                //if (serviceName == "Funeral Expenses")
                // {
                //  var UpdatedAmount = (from cr in db.ClientReports
                // where cr.SubReportId == input.SubReportId && cr.Amount != null
                // select cr.Amount).Sum();
                // existing.Amount =UpdatedAmount;
                //var repo = new GenericRepository<SubReport>(db);
                //var model1 = SubReportDetailsModel.LoadData(repo.GetAll(this.Permissions.SubReportsFilter), db.MainReports, db.ClientReports, db.AppBudgetServices, db.SKMembersVisits, this.Permissions, model.Id);
                // model1.ReportingMethodId = (int)Service.ReportingMethods.TotalCostWithListOfClientNames;
                //return View(model1.ReportingMethod.ToString(), model1);
                //model.DetailsHeader.Amount = UpdatedAmount;



                // }
                return this.MyJsonResult(
                    model,
                    JsonRequestBehavior.AllowGet);

            }
        

		private JsonResult GetMhmRows(SubReportDetailsJqDt model)
		{
			throw new NotImplementedException();
		}

		private JsonResult GetProgramCostRows(SubReportDetailsJqDt model)
		{
			var agc = (from sr in db.SubReports
					   where sr.Id == model.Id
					   select sr.AppBudgetService.AppBudget.App.Currency).Single();

			var q = from pct in db.ProgramCostTypes
					join pt in db.ProgramCosts.Where(f => f.SubReportId == model.Id)
						on pct.Id equals pt.ProgramCostTypeId into ptg
					from pt in ptg.DefaultIfEmpty()
					select new
					{
						Id = (int?)pt.Id,
						CostTypeId = pct.Id,
						Name = pct.Name,
						Amount = (decimal?)pt.Amount,
						PercentFundedByCC = (decimal?)pt.PercentFundedByCC
					};
			var filtered = q.WhereIf(f => f.Name.Contains(model.sSearch), !string.IsNullOrWhiteSpace(model.sSearch));
			var page = filtered.OrderBy(f => f.Name).Skip(model.iDisplayStart).Take(model.iDisplayLength);

			model.iTotalDisplayRecords = filtered.Count();
			model.iTotalRecords = q.Count();

			model.aaData = page.ToList().Select(f => new object[]{
				f.Id,
				f.Name,
				f.Amount,
				f.PercentFundedByCC,
				f.Amount * f.PercentFundedByCC / 100,
				agc.Id,
				null,
				f.CostTypeId
			});

			return this.MyJsonResult(model, JsonRequestBehavior.AllowGet);

		}

		#endregion

		[HttpGet]
		public ActionResult GetTotals(int id, long? totalYtdSk)
		{
			var source = db.SubReports.Select(f => new SubRepAppBudgetPair() { SubReport = f, AppBudgetService = f.AppBudgetService });

			var submittedMainReports = db.MainReports.Where(MainReport.Submitted);

			DateTime startOfYear = new DateTime(DateTime.Today.Year, 1, 1);
			var subReport = db.SubReports.Include(f => f.AppBudgetService).Include(f => f.AppBudgetService.Service).Include(f => f.MainReport).SingleOrDefault(f => f.Id == id);
			bool isSK = subReport.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.SoupKitchens;

            // var FuneralAmount = db.ClientReports.SingleOrDefault(f => f.SubReportId == id && f.Amount != null);
            var ServiceName = (from sr in SubServiceRowModes.asdf(source)
                               join mr in db.MainReports on (sr.MainReportId ?? 0) equals mr.Id
                               //join cr in db.ClientReports on (sr.Id ?? 0) equals cr.SubReportId

                               where sr.Id == id
                               select sr.ServiceName);
            
            var FuneralAmount = (from cr in db.ClientReports
                                 join sbr in db.SubReports on cr.SubReportId equals sbr.Id
                                 join abs in db.AppBudgetServices on sbr.AppBudgetServiceId equals abs.Id
                                 join ser in db.Services on abs.ServiceId equals ser.Id 
                                 where cr.SubReportId == id && ser.Id == 484 //454 //&& cr.Amount != null
                                 select cr.Amount).Sum();

           


            var model = (from sr in SubServiceRowModes.asdf(source)
						 join mr in db.MainReports on (sr.MainReportId ?? 0) equals mr.Id
                         //join cr in db.ClientReports on (sr.Id ?? 0) equals cr.SubReportId

                         where sr.Id == id
						 let totalMC = isSK ? db.SKMembersVisits.Count(f => f.SoupKitchensReport.SubReportId == sr.Id) : 0
                         
						 select new SubReportDetailsModel.SubReportTotals()
						 {
							 ReportingMethodId = sr.AppBudgetService.Service.ReportingMethodId,
							 AppMatchingSum = sr.ApprovedRequiredMatch,
							 YtdMatchExp = sr.YtdMatchExp,
							 EstimatedMatchingSum = sr.RelativeRequiredMatch,
							 TotalClients = sr.TotalClients,
                             //TotalAmountReported = sr.CcExp + cr.Amount,
                             FuneralAmount = FuneralAmount ?? 0,
                             TotalAmountReported = (sr.CcExp ?? 0) + (FuneralAmount ?? 0),
                             CurId = mr.AppBudget.App.CurrencyId,
							 TotalFundedByCC = ((decimal?)sr.SubReport.ProgramCosts.Sum(f => f.Amount * f.PercentFundedByCC / 100M)) ?? 0,
							 AppAmount = sr.AppBudgetService.CcGrant,
							 TotalVisitCount = totalMC,
							 TotalYTDVisitCount = isSK && totalYtdSk.HasValue ? totalYtdSk.Value + totalMC : 0
						 }).Single();


            //var repo = new GenericRepository<SubReport>(db);
           // var model1 = SubReportDetailsModel.LoadData(repo.GetAll(this.Permissions.SubReportsFilter), db.MainReports, db.ClientReports, db.AppBudgetServices, db.SKMembersVisits, this.Permissions, id);
            //model1.DetailsHeader.Amount = TotalAmountReported;
           // return View("Details", model1);

            return View(model.ViewName, model);
           

        }

		/// <summary>
		///		populates drop down on medical equipment
		/// </summary>
		/// <param name="subreportid"></param>
		/// <param name="clientid"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		[HttpGet]
		public JsonResult GetMedicalEquipmentItemTypes(int subreportid, int clientid, int? id)
		{
			var q = from t in db.MedicalEquipmentTypes
					join mer in db.MedicalEquipmentReports.Where(f => f.SubReportId == subreportid && f.ClientId == clientid) on t.Id equals mer.EquipmentTypeId into merg
					from mer in merg.DefaultIfEmpty()
					where mer == null || mer.Id == id
					select new { Id = t.Id, Name = t.Name };

			return this.MyJsonResult(q, JsonRequestBehavior.AllowGet);
		}
		/// <summary>
		/// provides data for populating ddl
		/// </summary>
		/// <param name="subreportid"></param>
		/// <param name="clientid"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		[HttpGet]
		public JsonResult GetMhmTypes(int subreportid, int clientid, int? id)
		{
			var q = from t in db.MhmTypes
					join mer in db.MhmReports.Where(f => f.SubReportId == subreportid && f.ClientId == clientid) on t.Id equals mer.TypeId into merg
					from mer in merg.DefaultIfEmpty()
					where mer == null || mer.Id == id
					select new { Id = t.Id, Name = t.Name };

			return this.MyJsonResult(q, JsonRequestBehavior.AllowGet);
		}


		#endregion

		#region update subreport rows

		[HttpPost]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult SaveMedicalEquipmentReport(MedicalEquipmentReport input)
		{
			if (input.Amount < 0 || input.Quantity < 0)
			{
				throw new Exception("The amount/quantity cannot be negative");
			}
			var mer = db.MedicalEquipmentReports.Where(f => f.Id == input.Id).SingleOrDefault();
			if (mer == null)
			{
				mer = new MedicalEquipmentReport()
				{
					SubReportId = input.SubReportId,
					ClientId = input.ClientId,
				};
				db.MedicalEquipmentReports.AddObject(mer);
			}

			mer.EquipmentTypeId = input.EquipmentTypeId;
			mer.Amount = input.Amount;
			mer.Quantity = input.Quantity;

			db.SaveChanges();
			return new EmptyResult();

		}

		[HttpPost]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult SaveMhmReport(MhmReport input)
		{
			if (input.Amount < 0 || input.Quantity < 0)
			{
				throw new Exception("The amount/quantity cannot be negative");
			}

			var Client = db.Clients.SingleOrDefault(f => f.Id == input.ClientId);

			var mer = db.MhmReports.Where(this.Permissions.MhmReportsFilter).Where(f => f.Id == input.Id).SingleOrDefault();
			if (mer == null)
			{
				mer = new MhmReport()
				{
					SubReportId = input.SubReportId,
					ClientId = input.ClientId,
				};
				db.MhmReports.AddObject(mer);
			}

			mer.TypeId = input.TypeId;
			mer.Amount = input.Amount;
			mer.Quantity = input.Quantity;

			db.SaveChanges();
			return new EmptyResult();

		}

		[HttpPost]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult UpdateProgramCost(ProgramCost input)
		{
			if (input.Amount < 0)
			{
				throw new Exception("The amount cannot be negative");
			}
			var existing = db.ProgramCosts.SingleOrDefault(f => f.Id == input.Id);
			if (existing == null)
			{
				existing = new ProgramCost() { SubReportId = input.SubReportId, ProgramCostTypeId = input.ProgramCostTypeId };
				db.ProgramCosts.AddObject(existing);
			}

			var sr = db.SubReports.Where(this.Permissions.SubReportsFilter)
				.SingleOrDefault(f => f.Id == existing.SubReportId);
			if (sr == null)
			{
				throw new Exception("Detailed Report not found.");
			}

			existing.Amount = input.Amount;
			existing.PercentFundedByCC = input.PercentFundedByCC;

			var rowsUpdated = db.SaveChanges();

			return new EmptyResult();
		}

		[HttpPost]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult DeleteChildReportRows(int id)
		{
			try
			{
				var existing = db.SubReports.SingleOrDefault(f => f.Id == id);

				if (existing != null)
				{

					existing.Amount = existing.AgencyContribution = existing.MatchingSum = null;
					db.SaveChanges();

					IEnumerable<int?> rowCount = null;
					switch (existing.AppBudgetService.Service.ReportingMethodEnum)
					{
						case Service.ReportingMethods.Emergency:
							rowCount = db.DeleteEmergencyReport(id);
							break;
						case Service.ReportingMethods.SupportiveCommunities:
							db.DeleteSCReport(id);
							break;
						case Service.ReportingMethods.DayCenters:
							rowCount = db.DeleteDCCReport(id);
							break;
						case Service.ReportingMethods.SoupKitchens:
							rowCount = db.DeleteSKReport(id);
							break;
						default:
							rowCount = db.DeleteClientReport(id);
							break;
					}

				}
				return this.RedirectToAction(f => this.Details(id));
			}
			catch
			{
				throw;
			}
		}




		[HttpPost]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult DeleteProgramCost(int id)
		{
			var existing = db.ProgramCosts.SingleOrDefault(f => f.Id == id);
			if (existing != null)
			{
				db.ProgramCosts.DeleteObject(existing);
				var rowsUpdated = db.SaveChanges();
			}

			return new EmptyResult();
		}
		[HttpPost]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult DeleteMhmRow(int id)
		{
			var pr = db.MhmReports.SingleOrDefault(f => f.Id == id);
			if (pr != null)
			{

				db.MhmReports.DeleteObject(pr);
				var rowsUpdated = db.SaveChanges();
			}
			return new EmptyResult();
		}
		[HttpPost]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult DeleteMedicalEquipmentRow(int id)
		{
			var pr = db.MedicalEquipmentReports.SingleOrDefault(f => f.Id == id);
			if (pr != null)
			{

				db.MedicalEquipmentReports.DeleteObject(pr);
				var rowsUpdated = db.SaveChanges();
			}
			return new EmptyResult();
		}
		[HttpPost]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult SaveClientReport(ClientReport input)
		{
            // if (input.SubReport.)
                    if (input.Amount < 0)
			{
				throw new Exception("The amount cannot be negative");
			}

			var Client = db.Clients.SingleOrDefault(f => f.Id == input.ClientId);

			ModelState.Clear();
			var q = (from subreport in db.SubReports.Where(Permissions.SubReportsFilter)
					 where subreport.Id == input.SubReportId
					 select new { sr = subreport, reportingMethodId = subreport.AppBudgetService.Service.ReportingMethodId, serviceTypeId = subreport.AppBudgetService.Service.TypeId, serviceName = subreport.AppBudgetService.Service.Name}).SingleOrDefault();
			if (q == null)
			{
				throw new InvalidOperationException("Subreport not found");
			}
			var sr = q.sr;
			var reportingMethodId = q.reportingMethodId;
			var serviceTypeId = q.serviceTypeId;
           
            var serviceName = q.serviceName;
            var ClientId = input.ClientId;
            if (serviceName == "Funeral Expenses")
            {
                if (input.Amount == null)
                {
                    // throw new InvalidOperationException("Please enter amount; The amount is required field.");
                    ModelState.AddModelError(string.Empty, "Could not save this data." + "CCID:" + " " + ClientId + " " + "Error: " + "You are required to submit a value for the Amount field for each client reported for Funeral Expenses");
                }
            }

            ClientReport existing = null;
			if (input.Id == default(int))
			{
				existing = db.ClientReports.Where(this.Permissions.ClientReportsFilter)
				.SingleOrDefault(f => f.ClientId == input.ClientId && f.SubReportId == input.SubReportId);
			}
			else
			{
				existing = db.ClientReports.Where(this.Permissions.ClientReportsFilter).SingleOrDefault(f => f.Id == input.Id);
			}
			if (existing == null)
			{
				existing = input;
				existing.SubReport = sr;
				db.ClientReports.AddObject(existing);
			}

			if (ModelState.IsValid)
			{
				existing.Amount = input.Amount;
				existing.Quantity = input.Quantity;
				if (string.IsNullOrWhiteSpace(input.Remarks)) existing.Remarks = null;
				else existing.Remarks = input.Remarks;
				//add client for verification by status
				existing.Client = db.Clients.Where(c => c.Id == existing.ClientId).SingleOrDefault();

				TryValidateModel(existing);
				if (ModelState.IsValid)
				{

					try
					{

						var rowsUpdated = db.SaveChanges();
                        
					}
					catch (Exception ex)
					{
						throw ex;
					}
				}
               
                // {
                //  var UpdatedAmount = (from cr in db.ClientReports
                // where cr.SubReportId == input.SubReportId && cr.Amount != null
                //select cr.Amount).Sum();
                // existing.Amount =UpdatedAmount;
                // var repo = new GenericRepository<SubReport>(db);
                // var model = SubReportDetailsModel.LoadData(repo.GetAll(this.Permissions.SubReportsFilter), db.MainReports, db.ClientReports, db.AppBudgetServices, db.SKMembersVisits, this.Permissions, input.SubReportId);
                // model.DetailsHeader.Amount = UpdatedAmount;
                // model.ReportingMethodId = (int)Service.ReportingMethods.TotalCostWithListOfClientNames;
                //  return View(model.ReportingMethod.ToString(), model);



                // }
            }
			return this.MyJsonResult(new
			{

				success = ModelState.IsValid,
				errors = ModelState.ValidationErrorMessages(),
				report = toobjArray(reportingMethodId, serviceTypeId, serviceName)(new  GetChildrenWithoutNameSubClass()
				{
                  
            Amount = existing.Amount, 

					ClientId = existing.ClientId,
					ClientFirstName = null,
					ClientLastName = null,
					//ClientName = null,
					Id = existing.Id,
					Quantity = existing.Quantity,
				})
			}, JsonRequestBehavior.DenyGet);

            

        }

    [HttpPost]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult Modify(ClientReport input)
		{
			using (var db = new ccEntities())
			{
				ModelState.Clear();
				var existing = db.ClientReports.Where(this.Permissions.ClientReportsFilter)
					.SingleOrDefault(f => f.Id == input.Id);
				if (existing == null)
				{
					existing = input;
					db.ClientReports.AddObject(existing);
				}
				else
				{
					db.DeleteObject(existing);
				}

				var rowUpdated = db.SaveChanges();

				return this.MyJsonResult(new
				{
					success = ModelState.IsValid,
					errors = ModelState.ValidationErrorMessages()
				});
			}
		}
		[HttpPost]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult DeleteClientReport(ClientReport input)
		{
			ModelState.Clear();
			var cr = db.ClientReports
						.Where(this.Permissions.ClientReportsFilter)
						.Include(f => f.ClientAmountReports)
						.Single(f => f.Id == input.Id);

			foreach (var ar in cr.ClientAmountReports.ToList())
			{
				db.ClientAmountReports.DeleteObject(ar);
			}
			db.ClientReports.DeleteObject(cr);

			var rowsUpdated = db.SaveChanges();
			return this.MyJsonResult(new
			{
				success = ModelState.IsValid,
				errors = ModelState.ValidationErrorMessages()
			}, JsonRequestBehavior.DenyGet);
		}

		/// <summary>
		/// updates client report (homecare)
		/// </summary>
		/// <param name="id"></param>
		/// <param name="subReportId"></param>
		/// <param name="clientId"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		[HttpPost]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult Update(int? id, int subReportId, int clientId, decimal? rate, decimal?[] quantity, string remarks)
		{
			if ((!rate.HasValue || rate.Value >= 0) && !quantity.Any(f => f < 0))
			{
				var client = db.Clients.Single(f => f.Id == clientId);

				var subReport = db.SubReports.Where(this.Permissions.SubReportsFilter)
					.Select(f => new
					{
						Id = f.Id,
						MainReportId = f.MainReportId,
						ReportingPeriodId = SqlFunctions.DateDiff("month", f.MainReport.Start, f.MainReport.End),
						MainReportStart = f.MainReport.Start,
						MainReportStatusId = f.MainReport.StatusId,
						ExceptionalHours = f.AppBudgetService.Service.ExceptionalHomeCareHours

					})
					.Single(f => f.Id == subReportId);

				if (MainReport.EditableStatuses.Contains((MainReport.Statuses)subReport.MainReportStatusId))
				{

					//check client report - add if missing
					var clientreport = db.ClientReports
						.Include(f => f.ClientAmountReports)
						.SingleOrDefault(f => f.Id == id);

					if (clientreport == null)
					{
						clientreport = new ClientReport()
						{
							SubReportId = subReport.Id,
							ClientId = client.Id,
						};
						db.ClientReports.AddObject(clientreport);
					}

					//parse the data:

					clientreport.Rate = rate;
					clientreport.Remarks = remarks;


					Dictionary<DateTime, decimal> diffs = new Dictionary<DateTime, decimal>();

					for (int i = 0; i < subReport.ReportingPeriodId; i++)
					{
						var inputQuantity = quantity[i];
						var date = subReport.MainReportStart.AddMonths(i);

						var amountReport = clientreport.ClientAmountReports.SingleOrDefault(f => f.ReportDate == date.Date);
						if ((inputQuantity ?? 0) != 0)
						{
							if (amountReport == null)
							{
								amountReport = new ClientAmountReport() { ReportDate = date };
								clientreport.ClientAmountReports.Add(amountReport);
							}
							amountReport.Quantity = inputQuantity.Value;
						}
						else if (amountReport != null)
						{
							db.ClientAmountReports.DeleteObject(amountReport);
						}
					}

					if (!clientreport.ClientAmountReports.Any())
					{
						db.ClientReports.DeleteObject(clientreport);
					}

					//validate the clientreport
					if (client.ApprovalStatus.Id == (int)CC.Data.ApprovalStatusEnum.ApprovedHomecareOnly)
					{
						clientreport.Client = client;
						clientreport.SubReport = db.SubReports.Where(this.Permissions.SubReportsFilter).Where(s => s.Id == clientreport.SubReportId).SingleOrDefault();
					}

					ModelState.Clear();
					TryValidateModel(clientreport);

					if (ModelState.IsValid)
					{

						try
						{
							db.Connection.Open();
							using (var trans = db.Connection.BeginTransaction())
							{
								var rowsUpdated = db.SaveChanges();
								if (subReport.ExceptionalHours)
								{
									var q = from a in
												(from cr in db.ClientReports
												 where cr.Id == clientreport.Id
												 from ar in cr.ClientAmountReports
												 group ar by new { cr.ClientId, ar.ReportDate } into arg
												 select arg.Key)
											let hcap = ccEntitiesExtensions.HcCap(a.ClientId, a.ReportDate, ccEntitiesExtensions.AddMonths(a.ReportDate, 1))
											where hcap == null
											select a.ReportDate;
									var invalidDates = q.ToList();
									if (invalidDates.Any())
									{
										ModelState.AddModelError(String.Empty,
											string.Format("CCID: {0}. Invalid HC cap (N/A) for Exceptional Hours report ({1})."
												, clientreport.ClientId
												, string.Join(", ", invalidDates.Select(f => f.ToString("MMM")))));
									}

								}
								var hvv = db.spValidateCrHc(subReport.MainReportId, clientreport.ClientId).ToList();
								if (hvv.Any())
								{

									foreach (var item in hvv)
									{
										ModelState.AddModelError(string.Empty, string.Format(
											"CCID: {5}, Month: {0}, Amount Reported:{1} hours, Total Cap: {2} hours (Month's cap: {3} hours, balancing cap: {4} hours)"
											, item.reportdate.Value.ToMonthString()
											, item.q.Format()
											, (item.q + item.co).Format()
											, item.c.Format()
											, (item.q + item.co - item.c).Format()
											, item.clientid
											));
									}
								}

								if (ModelState.IsValid)
								{
									trans.Commit();
									return Content(rowsUpdated.ToString());
								}
								else
								{
									trans.Rollback();
								}
							}


						}
						catch (System.Data.UpdateException ex)
						{
							ModelState.AddModelError(string.Empty,
									ex.InnerException.Message);
						}
					}
				}
				else
				{
					ModelState.AddModelError(string.Empty, string.Format("The main report is in status {0} and can't be updated", (MainReport.Statuses)subReport.MainReportStatusId));
				}
			}
			else
			{
				ModelState.AddModelError(string.Empty, "Rate/quantity cannot be negative");
			}

			return this.MyJsonResult(new
			{
				success = ModelState.IsValid,
				errors = ModelState.ValidationErrorMessages()
			}, JsonRequestBehavior.DenyGet);
		}

		/// <summary>
		/// updates client report weekly (homecare)
		/// </summary>
		/// <param name="id"></param>
		/// <param name="subReportId"></param>
		/// <param name="clientId"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		[HttpPost]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult UpdateWeekly(int? id, int subReportId, int clientId, decimal? rate, decimal?[] quantity, string remarks)
		{
			if ((!rate.HasValue || rate.Value >= 0) && !quantity.Any(f => f < 0))
			{
				var client = db.Clients.Single(f => f.Id == clientId);

				var subReport = db.SubReports.Where(this.Permissions.SubReportsFilter)
					.Select(f => new
					{
						Id = f.Id,
						MainReportId = f.MainReportId,
						MainReportStart = f.MainReport.Start,
						MainReportEnd = f.MainReport.End,
						MainReportStatusId = f.MainReport.StatusId,
						ExceptionalHours = f.AppBudgetService.Service.ExceptionalHomeCareHours,
						CoPGovHoursValidation = f.AppBudgetService.Service.CoPGovHoursValidation,
						ServiceTypeId = f.AppBudgetService.Service.TypeId,
						AppId = f.AppBudgetService.AppBudget.AppId,
						AgencyId = f.AppBudgetService.AgencyId,
						AgencyGroupId = f.AppBudgetService.Agency.GroupId

					})
					.Single(f => f.Id == subReportId);

				if (MainReport.EditableStatuses.Contains((MainReport.Statuses)subReport.MainReportStatusId))
				{

					//check client report - add if missing
					var clientreport = db.ClientReports
						.Include(f => f.ClientAmountReports)
						.Include(f => f.Client)
						.SingleOrDefault(f => f.Id == id);

					if (clientreport == null)
					{
						clientreport = new ClientReport()
						{
							SubReportId = subReport.Id,
							ClientId = client.Id,
						};
						db.ClientReports.AddObject(clientreport);
					}

					//parse the data:

					clientreport.Rate = rate;
					clientreport.Remarks = remarks;

					var startingWeek = subReport.MainReportStart;
					DayOfWeek selectedDOW = startingWeek.DayOfWeek;
					int? selectedDowDb = GlobalHelper.GetWeekStartDay(subReport.AgencyGroupId, subReport.AppId);
					if (selectedDowDb.HasValue)
					{
						selectedDOW = (DayOfWeek)selectedDowDb.Value;
						if (startingWeek.Month > 1)
						{
							var diff = selectedDowDb.Value - (int)startingWeek.DayOfWeek;
							startingWeek = startingWeek.AddDays((double)(diff));
							if (startingWeek > subReport.MainReportStart)
							{
								startingWeek = startingWeek.AddDays(-7);
							}
						}
					}
					DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
					dfi.FirstDayOfWeek = selectedDOW;
					Calendar cal = dfi.Calendar;
					int weeksCount = cal.GetWeekOfYear(subReport.MainReportEnd.AddDays(-1), dfi.CalendarWeekRule, dfi.FirstDayOfWeek) - cal.GetWeekOfYear(startingWeek, dfi.CalendarWeekRule, dfi.FirstDayOfWeek) + 1;

					Dictionary<DateTime, decimal> diffs = new Dictionary<DateTime, decimal>();

					DateTime date = startingWeek;
					for (int i = 0; i < weeksCount; i++)
					{
						var inputQuantity = quantity[i];
						if (i == 0 && date < subReport.MainReportStart)
						{
							date = subReport.MainReportStart;
						}
						else if (i == 1 && date == subReport.MainReportStart && selectedDOW != subReport.MainReportStart.DayOfWeek)
						{
							var diff = (int)selectedDOW - (int)subReport.MainReportStart.DayOfWeek;
							if (diff < 0)
							{
								diff += 7;
							}
							date = date.AddDays(diff);
						}
						else if (i > 0)
						{
							date = date.AddDays(7);
						}

						var amountReport = clientreport.ClientAmountReports.SingleOrDefault(f => f.ReportDate == date);
						if ((inputQuantity ?? 0) != 0)
						{
							if (amountReport == null)
							{
								amountReport = new ClientAmountReport() { ReportDate = date };
								clientreport.ClientAmountReports.Add(amountReport);
							}
							amountReport.Quantity = inputQuantity.Value;
						}
						else if (amountReport != null)
						{
							db.ClientAmountReports.DeleteObject(amountReport);
						}
					}

					if (!clientreport.ClientAmountReports.Any())
					{
						db.ClientReports.DeleteObject(clientreport);
					}

					//validate the clientreport
					if (client.ApprovalStatus.Id == (int)CC.Data.ApprovalStatusEnum.ApprovedHomecareOnly)
					{
						clientreport.Client = client;
						clientreport.SubReport = db.SubReports.Where(this.Permissions.SubReportsFilter).Where(s => s.Id == clientreport.SubReportId).SingleOrDefault();
					}

					ModelState.Clear();
					TryValidateModel(clientreport);

					if (ModelState.IsValid)
					{

						try
						{
							db.Connection.Open();
							using (var trans = db.Connection.BeginTransaction())
							{
								var rowsUpdated = db.SaveChanges();
								var digits = CCDecimals.GetDecimalDigits();
								if (subReport.ExceptionalHours && subReport.CoPGovHoursValidation && subReport.ServiceTypeId!= (int)Service.ServiceTypes.HospiseCare)
								{
									var hvvWeh = from a in
													 (from cr in db.ClientReports
													  where cr.ClientId == clientreport.ClientId
													  where cr.SubReport.AppBudgetService.Service.ExceptionalHomeCareHours && cr.SubReport.AppBudgetService.Service.CoPGovHoursValidation
													  from ar in cr.ClientAmountReports
													  where cr.SubReport.MainReportId == subReport.MainReportId
													  group ar by new { cr.ClientId, ar.ReportDate } into arg
													  select new { ClientId = arg.Key.ClientId, ReportDate = arg.Key.ReportDate, Quantity = arg.Sum(f => f.Quantity) })
												 let clientLeaveDate = db.Clients.FirstOrDefault(f => f.Id == a.ClientId).LeaveDate
												 let clientReportDate = startingWeek.Month > 1 && startingWeek < subReport.MainReportStart && a.ReportDate == subReport.MainReportStart ? startingWeek : a.ReportDate
												 let q = subReport.MainReportStart.Month > 1 && startingWeek < subReport.MainReportStart && a.ReportDate == subReport.MainReportStart ?
													 (decimal?)(from cr in db.ClientReports
																where cr.SubReport.AppBudgetService.Service.ExceptionalHomeCareHours && cr.SubReport.AppBudgetService.Service.CoPGovHoursValidation
																where cr.ClientId == clientreport.ClientId && cr.SubReport.AppBudgetService.AppBudget.AppId == subReport.AppId
																		 && cr.SubReport.AppBudgetService.AgencyId == subReport.AgencyId
																		 && cr.SubReportId != clientreport.SubReportId
																		 && cr.SubReport.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.HomecareWeekly
																from ar in cr.ClientAmountReports
																where ar.ReportDate == startingWeek
																select ar.Quantity).Sum() : 0
                                                 let cfsrow = (from cfs in db.CfsRows
                                                               where cfs.Client.MasterIdClcd == clientreport.Client.MasterIdClcd
                                                               where cfs.StartDate != null && cfs.StartDate <= a.ReportDate && cfs.EndDate == null
                                                               select cfs).Any()
                                                 let govhcap = ccEntitiesExtensions.GovHcCapLeaveDate(a.ClientId, clientReportDate, ccEntitiesExtensions.AddDays(clientReportDate, 7)) * 7
												 let govhcaprounded = govhcap != null ? Math.Round((decimal)govhcap, digits) : 0
												 let quantityRounded = Math.Round(a.Quantity + (q ?? 0), digits)
												 where (cfsrow ? 0 : govhcaprounded) < quantityRounded
												 select new { a.ClientId, a.ReportDate, Quantity = quantityRounded, govhcaprounded = (cfsrow ? 0 : govhcaprounded) };

									if (hvvWeh.Any())
									{

										foreach (var item in hvvWeh.ToList())
										{
											ModelState.AddModelError(string.Empty, string.Format(
												"CCID: {3}, Reporting week: W{0}, Amount Reported: {1} hours (Weekly cap: {2} hours)"
												, cal.GetWeekOfYear(item.ReportDate, dfi.CalendarWeekRule, dfi.FirstDayOfWeek)
												, item.Quantity.Format()
												, item.govhcaprounded.Format()
												, item.ClientId
												));
										}
									}
								}
								else if (subReport.ExceptionalHours && subReport.ServiceTypeId != (int)Service.ServiceTypes.HospiseCare)
								{
									var q = from a in
												(from cr in db.ClientReports
												 where cr.Id == clientreport.Id
												 from ar in cr.ClientAmountReports
												 group ar by new { cr.ClientId, ar.ReportDate } into arg
												 select arg.Key)
											let hcap = ccEntitiesExtensions.HcCapLeaveDate(a.ClientId, a.ReportDate, ccEntitiesExtensions.AddDays(a.ReportDate, 7)) * 7
											where hcap == 0
											select a.ReportDate;
									var invalidDates = q.ToList();
									if (invalidDates.Any())
									{
										ModelState.AddModelError(String.Empty,
											string.Format("CCID: {0}. Invalid HC cap (N/A) for Exceptional Hours report ({1})."
												, clientreport.ClientId
												, string.Join(", ", invalidDates.Select(f => f.ToString("dd MMM yyyy")))));
									}

								}
								else if(subReport.ServiceTypeId != (int)Service.ServiceTypes.HospiseCare)
								{
									var hvv = from a in
												  (from cr in db.ClientReports
												   where cr.ClientId == clientreport.ClientId
												   where !cr.SubReport.AppBudgetService.Service.ExceptionalHomeCareHours
												   where cr.SubReport.AppBudgetService.Service.TypeId != (int)Service.ServiceTypes.HospiseCare
												   from ar in cr.ClientAmountReports
												   where cr.SubReport.MainReportId == subReport.MainReportId
												   group ar by new { cr.ClientId, ar.ReportDate } into arg
												   select new { ClientId = arg.Key.ClientId, ReportDate = arg.Key.ReportDate, Quantity = arg.Sum(f => f.Quantity) })
											  let clientLeaveDate = db.Clients.FirstOrDefault(f => f.Id == a.ClientId).LeaveDate
											  let clientReportDate = startingWeek.Month > 1 && startingWeek < subReport.MainReportStart && a.ReportDate == subReport.MainReportStart ? startingWeek : a.ReportDate
                                              let clientReportDate1 = ccEntitiesExtensions.AddDays(clientReportDate, 7) 

                                              let hcap = ccEntitiesExtensions.HcCapLeaveDate(a.ClientId, clientReportDate, ccEntitiesExtensions.AddDays(clientReportDate, 7)) * 7
											  let q = subReport.MainReportStart.Month > 1 && startingWeek < subReport.MainReportStart && a.ReportDate == subReport.MainReportStart ?
												  (decimal?)(from cr in db.ClientReports
															 where !cr.SubReport.AppBudgetService.Service.ExceptionalHomeCareHours
															 where cr.ClientId == clientreport.ClientId && cr.SubReport.AppBudgetService.AppBudget.AppId == subReport.AppId
																	  && cr.SubReport.AppBudgetService.AgencyId == subReport.AgencyId
																	  && cr.SubReportId != clientreport.SubReportId
																	  && cr.SubReport.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.HomecareWeekly
																	  && cr.SubReport.AppBudgetService.Service.TypeId != (int)Service.ServiceTypes.HospiseCare
															 from ar in cr.ClientAmountReports
															 where ar.ReportDate == startingWeek
															 select ar.Quantity).Sum() : 0
                                              let cfsrow = (from cfs in db.CfsRows
                                                            where cfs.Client.MasterIdClcd == clientreport.Client.MasterIdClcd
                                                            where cfs.StartDate != null && cfs.StartDate <= a.ReportDate && cfs.EndDate == null
                                                            select cfs).Any()
                                              let hcapRounded = hcap != null ? Math.Round((decimal)hcap, digits) : 0
											  let quantityRounded = Math.Round(a.Quantity + (q ?? 0), digits)
											  where (cfsrow ? 0 : hcapRounded) < quantityRounded
											  select new { a.ClientId, a.ReportDate, Quantity = quantityRounded, hcapRounded = (cfsrow ? 0 : hcapRounded), clientReportDate, clientReportDate1, hcap };

									if (hvv.Any())
									{

										foreach (var item in hvv.ToList())
										{
											ModelState.AddModelError(string.Empty, string.Format(
												"CCID: {3}, Reporting week: W{0}, Amount Reported: {1} hours (Weekly cap: {2} hours)"
												, cal.GetWeekOfYear(item.ReportDate, dfi.CalendarWeekRule, dfi.FirstDayOfWeek)
												, item.Quantity.Format()
												, item.hcapRounded.Format()
												, item.ClientId
												));
										}
									}
								}

								if (ModelState.IsValid)
								{
									trans.Commit();
									return Content(rowsUpdated.ToString());
								}
								else
								{
									trans.Rollback();
								}
							}
						}
						catch (System.Data.UpdateException ex)
						{
							ModelState.AddModelError(string.Empty,
									ex.InnerException.Message);
						}
					}
				}
				else
				{
					ModelState.AddModelError(string.Empty, string.Format("The main report is in status {0} and can't be updated", (MainReport.Statuses)subReport.MainReportStatusId));
				}
			}
			else
			{
				ModelState.AddModelError(string.Empty, "Rate/quantity cannot be negative");
			}
           
			return this.MyJsonResult(new
			{
				success = ModelState.IsValid,
				errors = ModelState.ValidationErrorMessages()
			}, JsonRequestBehavior.DenyGet);
		}

		#endregion

		#region create

		[CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult Edit(int id)
		{
			var repo = new GenericRepository<SubReport>(db);
			var model = SubReportDetailsModel.LoadData(repo.GetAll(this.Permissions.SubReportsFilter), db.MainReports, db.ClientReports, db.AppBudgetServices, db.SKMembersVisits, this.Permissions, id);
			return View("Details", model);
		}

		[HttpGet]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult SelectWeekStartDay(int mainReportId, int appBudgetServiceId)
		{
			var model = new SubReportSelectWeekStartDayModel() { MainReportId = mainReportId, AppBudgetServiceId = appBudgetServiceId };
			model.SubReportsRepository = this.SubReportsRepository;
			model.MainReportsRepository = new GenericRepository<MainReport>(db);
			model.AppBudgetServicesRepository = new GenericRepository<AppBudgetService>(db);
			model.LoadData();
			return View(model);
		}

		[HttpPost]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult SelectWeekStartDay(SubReportSelectWeekStartDayModel model)
		{
			AgencyApp item = new AgencyApp
			{
				AgencyId = model.AgencyId,
				AppId = model.AppId,
				WeekStartDay = model.WeekStartDayId
			};
			if (db.AgencyApps.Any(f => f.AgencyId == item.AgencyId && f.AppId == item.AppId))
			{
				var dbItem = db.AgencyApps.SingleOrDefault(f => f.AgencyId == item.AgencyId && f.AppId == item.AppId);
				db.AgencyApps.DeleteObject(dbItem);
			}
			db.AgencyApps.AddObject(item);
			try
			{
				db.SaveChanges();
			}
			catch (Exception ex)
			{
				ModelState.AddModelError(string.Empty, "Could not save week start day. Error: " + ex.Message);
				model.SubReportsRepository = this.SubReportsRepository;
				model.MainReportsRepository = new GenericRepository<MainReport>(db);
				model.AppBudgetServicesRepository = new GenericRepository<AppBudgetService>(db);
				model.LoadData();
				return View(model);
			}
			return RedirectToAction("Create", new { mainReportId = model.MainReportId, appBudgetServiceId = model.AppBudgetServiceId });
		}

		/// <summary>
		/// Displays options for populating subreport
		/// </summary>
		/// <param name="mainReportId"></param>
		/// <param name="appBudgetServiceId"></param>
		/// <returns></returns>
		[HttpGet]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult Create(int mainReportId, int appBudgetServiceId)
		{
			var model = new SubReportCreateModel() { MainReportId = mainReportId, AppBudgetServiceId = appBudgetServiceId };
			model.SubReportsRepository = this.SubReportsRepository;
			model.MainReportsRepository = new GenericRepository<MainReport>(db);
			model.AppBudgetServicesRepository = new GenericRepository<AppBudgetService>(db);
			model.LoadData();

			var mr = db.MainReports.SingleOrDefault(f => f.Id == mainReportId);
			if (mr == null)
			{
				throw new InvalidOperationException("Main report does not exist.");
			}
			else if (!MainReport.EditableStatuses.Contains(mr.Status))
			{
				throw new InvalidOperationException(string.Format("The report can't be edited when it's status is {0}", mr.Status));
			}
			else
			{
				var abs = db.AppBudgetServices.Where(this.Permissions.AppBudgetServicesFilter).SingleOrDefault(f => f.Id == appBudgetServiceId);
				if (abs == null)
				{
					//exception
				}
				else
				{
					var service = abs.Service;
					switch (service.ReportingMethodEnum)
					{
						case Service.ReportingMethods.TotalCostNoNames:
							var subReport = new SubReport() { MainReportId = mainReportId, AppBudgetServiceId = appBudgetServiceId };
							return Create(subReport);
					}
				}
			}

			return View("Create", model);
		}

		/// <summary>
		/// Inserts subreport
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		[HttpPost]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.Ser, FixedRoles.AgencyUser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult Create(SubReport model)
		{

			var x = (from a in db.AppBudgetServices
					 select new
					 {
						 a.Id,
						 a.Service.TypeId,
						 a.Service.ReportingMethodId
					 }).FirstOrDefault(f => f.Id == model.AppBudgetServiceId);
			var existing = this.SubReportsRepository.GetAll(this.Permissions.SubReportsFilter).SingleOrDefault(f => f.MainReportId == model.MainReportId && f.AppBudgetServiceId == model.AppBudgetServiceId);

			//the subreport already exists
			if (existing != null)
			{
				switch ((Service.ReportingMethods)x.ReportingMethodId)
				{
					case Service.ReportingMethods.DayCenters:
					case Service.ReportingMethods.SoupKitchens:
						return this.RedirectToAction(f => f.Calendar(existing.Id));
					default:
						return this.RedirectToAction(f => f.Details(existing.Id));
				}
			}

			if (ModelState.IsValid)
			{
				this.SubReportsRepository.Add(model);
				try
				{
					var rowsUpdated = this.SubReportsRepository.SaveChanges();
					//if (x.ReportingMethodId == (int)Service.ReportingMethods.SupportiveCommunities)
					//{
					//	var sc = db.PopulateScReport(model.Id);
					//}
				}
				catch (System.Data.UpdateException ex)
				{
					_log.Error("an error occured while adding new subreport", ex);
					ModelState.AddModelError(string.Empty, ex);
				}

				if (ModelState.IsValid)
				{
					switch ((Service.ReportingMethods)x.ReportingMethodId)
					{
						case Service.ReportingMethods.DayCenters:
						case Service.ReportingMethods.SoupKitchens:
							return this.RedirectToAction(f => f.Calendar(model.Id));
						default:
							return this.RedirectToAction(f => f.Details(model.Id));
					}

				}

			}

			//return to the previous screen and display the errors
			return Create(model.MainReportId, model.AppBudgetServiceId);
		}



		#endregion

		#region Populate

		//start in the create.cshtml
		//create -> manual edit

		//create -> copy


		[CcAuthorize(FixedRoles.Admin, FixedRoles.AgencyUser, FixedRoles.Ser, FixedRoles.SerAndReviewer, FixedRoles.AgencyUserAndReviewer)]
		public ActionResult Copy(SubReportCreateModel model)
		{
			ModelState.Clear();
			SubReport nsr = null;
			SubReport prevSubReport = null;
			MainReport mr = null;
			try
			{
				prevSubReport = db.SubReports.Where(this.Permissions.SubReportsFilter).Where(f => f.AppBudgetServiceId == model.AppBudgetServiceId && f.Id == model.PrevSubReportId).Single();
			}
			catch (InvalidOperationException)
			{
				ModelState.AddModelError(string.Empty, "previous report not found");
			}

			try
			{
				mr = db.MainReports.Where(this.Permissions.MainReportsFilter).Where(f => f.Id == model.MainReportId).Single();
			}
			catch (InvalidOperationException)
			{
				ModelState.AddModelError(string.Empty, "current report not found");
				return View("Create", model);
			}

			if (ModelState.IsValid)
			{

				nsr = db.SubReports
					.Include(f => f.ClientReports.Select(cr => cr.ClientAmountReports))
					.Where(f => f.AppBudgetServiceId == model.AppBudgetServiceId && f.MainReportId == model.MainReportId).SingleOrDefault()
					;
				if (nsr == null)
				{

					nsr = new SubReport()
						{
							MainReportId = model.MainReportId.Value,
							AppBudgetServiceId = model.AppBudgetServiceId.Value,
							MainReport = mr,
							AppBudgetService = prevSubReport.AppBudgetService
						};
				}

				nsr.Amount = prevSubReport.Amount;
				nsr.MatchingSum = prevSubReport.MatchingSum;
				nsr.AgencyContribution = prevSubReport.AgencyContribution;

				var md = prevSubReport.MainReport.Start.MonthsTo(mr.Start);

				var clientReports = db.ClientReports
										.Include(f => f.ClientAmountReports)
										.Where(f => f.SubReportId == prevSubReport.Id);

				foreach (var cr in clientReports)
				{
					var ncr = nsr.ClientReports.SingleOrDefault(f => f.ClientId == cr.ClientId && f.Rate == cr.Rate);
					if (ncr == null)
					{
						ncr = new ClientReport()
						{
							ClientId = cr.ClientId
						};
					}

					ncr.ApplyValues(cr, md);
					nsr.ClientReports.Add(ncr);

					var validateSucceded = TryValidateModel(ncr);
					if (!ModelState.IsValid)
					{
						ModelState.Clear();
						ncr.SubReport = null;
						ncr = null;
					}
				}

				//medical equipment reports
				var months = (nsr.MainReport.Start.Month - prevSubReport.MainReport.Start.Month);
				foreach (var r in prevSubReport.EmergencyReports)
				{
					var newDate = r.ReportDate.AddMonths(months);
					var existing = nsr.EmergencyReports.SingleOrDefault(f => f.ClientId == r.ClientId && f.ReportDate == newDate);
					if (existing == null)
					{
						existing = new EmergencyReport()
						{
							ClientId = r.ClientId,
							ReportDate = newDate,
						};
						nsr.EmergencyReports.Add(existing);
					}
					existing.Remarks = r.Remarks;
					existing.Amount = r.Amount;
					existing.Discretionary = r.Discretionary;
					existing.TypeId = r.TypeId;
				}

			}

			if (ModelState.IsValid)
			{
				try
				{
					var rowsUpdated = db.SaveChanges();

				}
				catch (System.Data.UpdateException ex)
				{
					_log.Error("failed to save clone of previous subreport", ex);
					ModelState.AddModelError(string.Empty, ex.Message);

				}
			}

			if (ModelState.IsValid)
			{
				return this.RedirectToAction(f => f.Details(nsr.Id));
			}
			else
			{
				model.SubReportsRepository = this.SubReportsRepository;
				model.MainReportsRepository = new GenericRepository<MainReport>(db);
				model.AppBudgetServicesRepository = new GenericRepository<AppBudgetService>(db);
				model.LoadData();
				return View("Create", model);
			}
		}

		#endregion

		#region nested
		private class GetChildrenSubClass
		{
			public int? Id { get; set; }
			public int ClientId { get; set; }
			public decimal? Quantity { get; set; }
			public string ClientName { get; set; }
			public string ClientFirstName { get; set; }
			public string ClientLastName { get; set; }
			public string ClientApprovalStatus { get; set; }
			public decimal? Amount { get; set; }
			public decimal? HcCaps { get; set; }
			public string Remarks { get; set; }
		}

		private class GetChildrenWithoutNameSubClass
		{
			public int? Id { get; set; }
			public int ClientId { get; set; }
			public decimal? Quantity { get; set; }
			public string ClientFirstName { get; set; }
			public string ClientLastName { get; set; }
			public string ClientApprovalStatus { get; set; }
			public decimal? Amount { get; set; }
			public decimal? HcCaps { get; set; }
			public string Remarks { get; set; }
		}
		#endregion


	}


}

public class hcum
{
	public string clientId;
	public string subReportId;
	public string[] data;
}
