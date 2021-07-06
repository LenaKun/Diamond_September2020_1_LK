using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Data;
using System.Data.Entity;
using System.Web;
using System.Web.Mvc;
using CC.Data;
using System.Globalization;
using CC.Web.Helpers;

namespace CC.Web.Models
{




	public class SubReportSelectWeekStartDayModel : SubReportModelBase
	{
		#region Properties

		[Required]
		public int WeekStartDayId { get; set; }
		public SelectList WeekStartDay { get; set; }
		public int AppId { get; set; }

		#endregion

		#region Constructors

		public SubReportSelectWeekStartDayModel() : base()
		{
			this.WeekStartDayId = 0;
		}

		#endregion

		#region Methods

		public override void LoadData()
		{
			var appBudgetService = AppBudgetServicesRepository.GetAll(this.Permissions.AppBudgetServicesFilter).SingleOrDefault(f => f.Id == this.AppBudgetServiceId);
			this.AgencyId = appBudgetService.AgencyId;
			this.AppId = appBudgetService.AppBudget.AppId;
			var list = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>().Select(x => new SelectListItem { Text = x.DisplayName(), Value = ((int)x).ToString() }).ToList();
			this.WeekStartDay = new SelectList(list, "Value", "Text");
		}

		#endregion
	}

	public class SubReportCreateModel : SubReportModelBase, IValidatableObject
	{

		#region Properties

		//the surrogate key - overrriden in order to put the DisplayAttribute
		[Display(Name = "Report ID")]
		public override int? Id { get; set; }
		public int? PrevSubReportId { get; set; }
		public System.Web.Mvc.SelectList PrevReports { get; set; }
		public string UploadViewName { get; set; }		

		public CC.Web.Models.Import.ClientReports.UploadModel UploadModel
		{
			get
			{
				return new Import.ClientReports.UploadModel()
				{
					SubReportId = this.Id,
					AppBudgetServiceId = this.AppBudgetServiceId,
					MainReportId = this.MainReportId
				};
			}
		}
		#endregion

		#region Constructors

		public SubReportCreateModel()
			: base()
		{
		}
		public SubReportCreateModel(SubReportModelBase baseModel) : base(baseModel) { }

		//public Service.ReportingMethods ReportingMethod { get; set; }
		#endregion

		#region Methods

		public override void LoadData()
		{
			this.PrevReports = this.SubReportsRepository.GetAll(this.Permissions.SubReportsFilter) //allowed subreports
				.Where(f => f.AppBudgetServiceId == this.AppBudgetServiceId)	//same appbudgetservice (service + agency)
				.Where(f => f.MainReportId != this.MainReportId) //any but the current
				.Where(f => f.MainReport.StatusId == (int)MainReport.Statuses.Approved)	//only approved
				.Select(f => new { subReportId = f.Id, start = f.MainReport.Start })	//select only nessesary data
				.ToList()	//download
				.Select(f => new KeyValuePair<int, string>(f.subReportId, string.Format("{0}", f.start.ToMonthString())))
				.ToSelectList(this.PrevSubReportId);


			var appBudgetService = AppBudgetServicesRepository.GetAll(this.Permissions.AppBudgetServicesFilter).SingleOrDefault(f => f.Id == this.AppBudgetServiceId);
			this.DetailsHeader.FundName = appBudgetService.AppBudget.App.Fund.Name;
			this.DetailsHeader.AppName = appBudgetService.AppBudget.App.Name;
			this.DetailsHeader.AgencyName = appBudgetService.Agency.Name;
			this.DetailsHeader.SerName = appBudgetService.Agency.AgencyGroup.DisplayName;
			this.DetailsHeader.ServiceName = appBudgetService.Service.Name;
			this.DetailsHeader.ServiceTypeName = appBudgetService.Service.ServiceType.Name;
            //this.DetailsHeader.TotalAmountReported = decimal.

            this.ReportingMethodId = appBudgetService.Service.ReportingMethodId;

			var mainReport = MainReportsRepository.GetAll(this.Permissions.MainReportsFilter).Single(f => f.Id == this.MainReportId);
			this.DetailsHeader.Period = new Range<DateTime>() { Start = mainReport.Start, End = mainReport.End };
			this.DetailsHeader.MainReportStatusId = mainReport.StatusId;



            switch (appBudgetService.Service.ReportingMethodEnum)
			{
				default:
					this.UploadViewName = "Upload";
					break;
			}			

		}

		public void SaveData()
		{
			var subRep = SubReportsRepository.GetAll(this.Permissions.SubReportsFilter).Single(f => f.Id == this.Id);

		}

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{

			//create the store subreport
			var subReport = new SubReport();
			foreach (var result in subReport.Validate(validationContext))
			{
				yield return result;
			}
		}

		#endregion
	}


    public class SubReportCalendarModel : SubReportDetailsModel
    {
        public int j = 0;

        public string[] weekDays = new string[31];

    }

	public class SubReportDetailsModel : SubReportModelBase
	{

		#region Properties

		//required details
		[Display(Name = "Matching Summ")]
		public decimal MatchingSumm { get; set; }

		[Display(Name = "Agency's Contribution")]
		public decimal AgencyContribution { get; set; }


		[Display(Name = "Agency's Id")]
		public override int AgencyId { get; set; }

		[Display(Name = "Agency's Group Id")]
		public int AgencyGroupId { get; set; }

		public int ReportMonthsCount { get; set; }


		public SubReportFilter Filter { get; set; }

        public MonthsFilter FilterByMonth { get; set; }

        public SC_Filter FilterForSC { get; set; }

        public DCC_Filter FilterForDCC { get; set; }

		public List<DateTime> Dates { get; set; }
		public bool CanBeEdited { get; set; }

		//totals:
		public SubReportTotals Totals { get; set; }

		public int WeeksCount { get; set; }
		public DateTime StartingWeek { get; set; }
		public int StartingWeekNumber { get; set; }		

		public int AppId { get; set; }

		public int ServiceTypeId { get; set; }

        public string ServiceName { get; set; }

        public bool ExceptionalHomeCareHours { get; set; }

		public bool CoPGovHoursValidation { get; set; }

		#endregion

		#region Constructors

		public bool IsExportable()
		{

			if (this.User.RoleId == (int)FixedRoles.AgencyUser || this.User.RoleId == (int)FixedRoles.AgencyUserAndReviewer)
				return false;
			else
				return true;

		}

		public SubReportDetailsModel()
			: base()
		{
			this.Filter = new SubReportFilter();
            this.FilterByMonth = new MonthsFilter();
            this.FilterForDCC = new DCC_Filter();
            this.FilterForSC = new SC_Filter();
			this.Dates = new List<DateTime>();
		}

        #endregion

        #region Public Methods
        public static SubReportDetailsModel LoadData(IQueryable<SubReport> subreports, IQueryable<MainReport> MainReports, IQueryable<ClientReport> ClientReports,

            IQueryable<AppBudgetService> AppBudgetServices, IQueryable<SKMembersVisit> skMemberVisist, CC.Data.Services.IPermissionsBase permission, int id)
        {
            var editableMainReportStatusIds = MainReport.EditableStatuses.Select(f => (int)f);
            var currentUser = ((CC.Web.Security.CcPrincipal)System.Web.HttpContext.Current.User).CcUser;

            var source = subreports.Select(f => new SubRepAppBudgetPair() { SubReport = f, AppBudgetService = f.AppBudgetService });
            //var FuneralExpAmount = 0;
            //var FuneralExpAmount = 
            //  where cr.SubReportId == id
            //  join sr in SubReports on Clientreports.SubReportId = SubReports.Id
            //Join AppBudgetServices on SubReports.AppBudgetServiceId = AppBudgetServices.Id
            // join Services on AppBudgetServices.ServiceId = Services.id
            //Where Services.id = 454 and ClientReports.SubReportId =)

            //(from cr in ClientReports
            // where cr.SubReportId == id && cr.Amount != null
            //select cr.Amount).Sum();

            var q = (from sr in SubServiceRowModes.asdf(source)
                     join mr in MainReports on (sr.MainReportId ?? 0) equals mr.Id
                     // join sbr in subreports on (mr.Id) equals sbr.MainReportId
                     //join cr in ClientReports on sr.SubReport.Id  equals cr.SubReportId
                    
                    
            //Where Services.id = 454//&& cr.Amount != null
                     select new SubReportDetailsModel()
                     {
                         Id = sr.Id,
                         AgencyId = sr.AgencyId,
                         AppId = mr.AppBudget.AppId,
                         AgencyGroupId = mr.AppBudget.App.AgencyGroupId,
                         AppBudgetServiceId = sr.AppBudgetServiceId,
                         MainReportId = sr.MainReportId,
                         MainReportStart = mr.Start,
                         MainReportEnd = mr.End,
                         CanBeEdited = editableMainReportStatusIds.Contains(mr.StatusId),
                         ReportingMethodId = sr.SubReport.AppBudgetService.Service.ReportingMethodId,
                         ServiceTypeId = sr.AppBudgetService.Service.TypeId,
                         ExceptionalHomeCareHours = sr.AppBudgetService.Service.ExceptionalHomeCareHours,
                         CoPGovHoursValidation = sr.AppBudgetService.Service.CoPGovHoursValidation,
                         ServiceName = sr.ServiceName,
                         DetailsHeader = new Header()
                         {
                             AgencyName = sr.AgencyName,

                             Id = sr.Id ?? 0,
                             Period = new Range<DateTime>() { Start = mr.Start, End = mr.End },
                             SerId = sr.SerId ?? 0,
                             SerName = sr.SerName,
                             ServiceName = sr.ServiceName,
                             ServiceTypeName = sr.ServiceTypeName,
                             Amount = sr.SubReport.Amount,
                             //FA = cr.Amount,
                             ServiceUnits = sr.SubReport.ServiceUnits,
                             AgencyContribution = sr.AgencyContribution,
                             MatchingSum = sr.MatchingSum,
                             MatchingSumRequired = mr.AppBudget.App.AgencyGroup.RequiredMatch,
                             MainReportStatusId = mr.StatusId,
                             ReportingMethodId = sr.SubReport.AppBudgetService.Service.ReportingMethodId,
                             TotalHouseholdsServed = sr.SubReport.TotalHouseholdsServed,
                             AppBudgetServiceRemarks = sr.AppBudgetService.Remarks,
                             AppName = sr.AppBudgetService.AppBudget.App.Name,
                             FundName = sr.AppBudgetService.AppBudget.App.Fund.Name

                         },
                         Totals = new SubReportTotals()
                         {
                             ReportingMethodId = sr.AppBudgetService.Service.ReportingMethodId,
                             AppMatchingSum = sr.ApprovedRequiredMatch,
                             SubReportMatchingSum = null,

                             YtdMatchExp = sr.YtdMatchExp,
                             EstimatedMatchingSum = sr.RelativeRequiredMatch,
                             TotalClients = sr.TotalClients,
                             //FuneralExpAmount = sr.AmountFuneralExpences ?? 0,
                             // FuneralExpAmount = cr.Amount ?? 0,
                             TotalAmountReported = (sr.CcExp ?? 0), //+ (FuneralExpAmount ?? 0), //+ (sr.AmountFuneralExpences ?? 0), //+ cr.Amount ?? 0,
                             //cr.Amount ?? 0,
                             CurId = mr.AppBudget.App.CurrencyId
                         }
                     }
                );

            var subreport = q.Single(f => f.Id == id);
            //var cr = ClientReports.SingleOrDefault(f => f.SubReportId == id && f.Amount != null);

            //var FuneralExpAmount = ClientReports.Sum(f => f.Amount)
            //.Where(f => f.SubReportId == id && f.Amount != null)

            var FuneralExpAmount = (from cr in ClientReports
                                    where cr.SubReportId == id && cr.Amount != null
                                    select cr.Amount).Sum();



            var ServiceName = subreport.ServiceName;
            if (ServiceName == "Funeral Expenses")
            {
                
                subreport.DetailsHeader.Amount = subreport.Totals.TotalAmountReported + (FuneralExpAmount ?? 0);
            }
            else
            {
                subreport.DetailsHeader.Amount = subreport.Totals.TotalAmountReported;
            }
            if (ServiceName == "Funeral Expenses")
                subreport.Totals.TotalAmountReported = subreport.Totals.TotalAmountReported + (FuneralExpAmount ?? 0);
            else
            {
                subreport.Totals.TotalAmountReported = subreport.Totals.TotalAmountReported;
            }
            //subreport.DetailsHeader.TotalAmountReported = subreport.Totals.TotalAmountReported + (FuneralExpAmount ?? 0);
            if (SubServiceRowModes.asdf(source).Where(f => f.Id == id).Select(f => f.AppBudgetService.Service.ReportingMethodId).Single() == (int)Service.ReportingMethods.SoupKitchens)
            {
              
                var sr = SubServiceRowModes.asdf(source).SingleOrDefault(f => f.Id == id);
                var mr = MainReports.SingleOrDefault(f => f.Id == sr.MainReportId);
                DateTime startOfYear = new DateTime(mr.Start.Year, 1, 1);
                var submittedMainReports = from mrq in MainReports.Where(MainReport.Submitted)
                                           where mrq.SubReports.Any(f => f.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.SoupKitchens)
                                           where mrq.Start < mr.End
                                           select mrq; 

                subreport.Totals.TotalVisitCount = skMemberVisist.Count(f => f.SoupKitchensReport.SubReportId == id);
           
                subreport.Totals.TotalYTDVisitCountExceptThisSr = (from skmv in skMemberVisist
                                                                   join smr in submittedMainReports on skmv.SoupKitchensReport.SubReport.MainReportId equals smr.Id
                                                                   where skmv.ReportDate < mr.End && skmv.ReportDate >= startOfYear && skmv.SoupKitchensReport.SubReportId != id
                                                                     && skmv.SoupKitchensReport.SubReport.AppBudgetService.AgencyId == subreport.AgencyId
                                                                     && skmv.SoupKitchensReport.SubReport.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.SoupKitchens
                                                                   select skmv.ReportDate).Count();
                subreport.Totals.TotalYTDVisitCount = subreport.Totals.TotalYTDVisitCountExceptThisSr + subreport.Totals.TotalVisitCount;
            }

            if (subreport.DetailsHeader.Amount == null)

                if (ServiceName == "Funeral Expenses")
                {
                    subreport.DetailsHeader.Amount = subreport.Totals.TotalAmountReported + (FuneralExpAmount ?? 0); //+ subreport.Totals.FuneralExpAmount ;
                }
                else
                {
                    subreport.DetailsHeader.Amount = subreport.Totals.TotalAmountReported;
                }

            if (subreport.DetailsHeader.MainReportStatusId == (int)MainReport.Statuses.Approved || subreport.DetailsHeader.MainReportStatusId == (int)MainReport.Statuses.AwaitingProgramAssistantApproval
                || subreport.DetailsHeader.MainReportStatusId == (int)MainReport.Statuses.AwaitingProgramOfficerApproval || subreport.DetailsHeader.MainReportStatusId == (int)MainReport.Statuses.AwaitingAgencyResponse)
                subreport.Filter.ReportedOnly = true;

            subreport.CanBeEdited &= (FixedRoles.AgencyUser | FixedRoles.Ser | FixedRoles.AgencyUserAndReviewer | FixedRoles.SerAndReviewer | FixedRoles.Admin).HasFlag((FixedRoles)permission.User.RoleId);
            subreport.DetailsHeader.Editable = subreport.CanBeEdited;

            var start = subreport.DetailsHeader.Period.Start;
            var end = subreport.DetailsHeader.Period.End;

            var mdiff = (12 * end.Year + end.Month) - (12 * start.Year + start.Month);

            if (subreport.ReportingMethodId != (int)Service.ReportingMethods.HomecareWeekly)
            {
                for (int i = 0; i < mdiff; i++)
                {
                    subreport.Dates.Add(subreport.DetailsHeader.Period.Start.AddMonths(i));
                }
            }
            else
            {
                using (var db = new ccEntities())
                {
                    var startingWeek = start;
                    DayOfWeek selectedDOW = startingWeek.DayOfWeek;
                    int? selectedDowDb = GlobalHelper.GetWeekStartDay(subreport.AgencyGroupId, subreport.AppId);
                    if (selectedDowDb.HasValue)
                    {
                        selectedDOW = (DayOfWeek)selectedDowDb.Value;
                        if (startingWeek.Month > 1)
                        {
                            var diff = selectedDowDb.Value - (int)startingWeek.DayOfWeek;
                            startingWeek = startingWeek.AddDays((double)(diff));
                            if (startingWeek > start)
                            {
                                startingWeek = startingWeek.AddDays(-7);
                            }
                        }
                    }
                    DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
                    dfi.FirstDayOfWeek = selectedDOW;
                    Calendar cal = dfi.Calendar;
                    subreport.WeeksCount = cal.GetWeekOfYear(end.AddDays(-1), dfi.CalendarWeekRule, dfi.FirstDayOfWeek) - cal.GetWeekOfYear(startingWeek, dfi.CalendarWeekRule, dfi.FirstDayOfWeek) + 1;
                    DateTime date = startingWeek;
                    for (int i = 0; i < subreport.WeeksCount; i++)
                    {
                        if (i == 0 && date < start)
                        {
                            date = start;
                        }
                        else if (i == 1 && date == start && selectedDOW != start.DayOfWeek)
                        {
                            var diff = (int)selectedDOW - (int)start.DayOfWeek;
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
                        subreport.Dates.Add(date);
                    }
                }
            }

            return subreport;
        }
        #endregion
        public override void LoadData()
		{
			base.LoadData();
		}

		#region Private Methods
		#endregion

		#region Nested Classes
		public class SubReportFilter
		{
			public SubReportFilter()
			{
				var items = new[] {
					new SelectListItem(){Value="", Text= "ShowAll"} ,
					new SelectListItem() { Value= "true", Text= "ShowOnlyReported"},
				};
            
              	this.ReportedOnlySelectList = new SelectList(items, "Value", "Text", this.ReportedOnly);
				this.ReportedOnly = null;
			}
            [Display(Name = "Client Name")]
			public string ClientName { get; set; }
            [Display(Name = "Client ID")]
			public int? ClientId { get; set; }
            [Display(Name = "Reported Only")]
			public bool? ReportedOnly { get; set; }
			public System.Web.Mvc.SelectList ReportedOnlySelectList { get; set; }
           
		}

        public class SC_Filter
        {
            public SC_Filter()
            {
                var items = new[] {
					new SelectListItem(){Value="0", Text= "SC clients and none SC clients"} ,
					new SelectListItem() { Value="1",  Text= "SC clients only"},
				};


                this.selList = new SelectList(items, "Value", "Text", 0);
              
            }

            [Display(Name = "Show clients")]
            public int SC_Only {get; set;}
            public System.Web.Mvc.SelectList selList { get; set; }
        }


        public class DCC_Filter
        {
            public DCC_Filter()
            {
                var items = new[] {
					new SelectListItem(){Value="0", Text= "Show DCC clients+ None DCC clients"} ,
					new SelectListItem() { Value="1",  Text= "Show DCC clients only"},
				};


                this.selList = new SelectList(items, "Value", "Text", 0);
              
            }

            [Display(Name = "Show clients")]
            public int DCC_Only { get; set; }
            public System.Web.Mvc.SelectList selList { get; set; }
        }

        public class MonthsFilter
        {
            public MonthsFilter()
            {
               
                this.SelMonth = 0;
                
            }

            [Display(Name = "Month")]
            public int SelMonth { get; set; }
            public int SelYear  { get; set; }
            public System.Web.Mvc.SelectList MonthsSelectList { get; set; }
		
        }

		public class Row
		{
			public int ClientReportId { get; set; }

			public string ClientName { get; set; }
			public string Rate { get; set; }
			public string Cur { get; set; }
			public string TotalHours { get; set; }

		}

		public class SubReportTotals
		{
			public string ViewName { get { return "SubReportTotals/" + ((Service.ReportingMethods)this.ReportingMethodId).ToString(); } }
			public decimal? SubReportMatchingSum { get; set; }

            public decimal ClientReportAmount { get; set; }
            public string CurId { get; set; }

			[Display(Name = "App Matching Sum")]
			public decimal? AppMatchingSum { get; set; }

			[Display(Name = "Total Matching Sum spent up to date")]
			public decimal? YtdMatchExp { get; set; }

            [Display(Name = "Estimated Matching Sum Spent Required up to date (including current period)")]
			public decimal? EstimatedMatchingSum { get; set; }

			[Display(Name = "Total Amount")]
			public decimal TotalAmountReported { get; set; }

            public decimal FuneralAmount { get; set; }

            [Display(Name = "Total Unique Clients")]
			[UIHint("int")]

			public int TotalClients { get; set; }

            public string ServiceName { get; set; }

            [Display(Name = "Matching Sum Status")]
			public string MatchingSumStatus
			{
				get
				{
					return YtdMatchExp - EstimatedMatchingSum >= 0 ? "OK" : ((YtdMatchExp - EstimatedMatchingSum) ?? 0).Format();
				}
			}

			[Display(Name = "Total funded by CC")]
			public decimal? TotalFundedByCC { get; set; }

			[Display(Name = "App Amount")]
			public decimal AppAmount { get; set; }

			[Display(Name = "App Balance")]
			public decimal AppBalance { get { return this.AppAmount - this.TotalAmountReported; } }

			public int ReportingMethodId { get; set; }

            public decimal FuneralExpAmount { get; set; }

			[Display(Name = "Total Meal Count")]
			public long TotalVisitCount { get; set; }

			[Display(Name = "Total Meal Count up to date")]
			public long TotalYTDVisitCount { get; set; }
			public long TotalYTDVisitCountExceptThisSr { get; set; }
		}

		public class Report
		{
			public int ClientReportId { get; set; }
			public int ClientAmountReportId { get; set; }
		}

		#endregion
	}

	public class SubReportDetailsJqDt : jQueryDataTableParamsWithData<IEnumerable<object>>
	{
		public SubReportDetailsJqDt()
		{
			Filter = new SubReportDetailsModel.SubReportFilter();
            FilterByMonth = new SubReportDetailsModel.MonthsFilter();
            FilterForSC = new SubReportDetailsModel.SC_Filter();
            FilterForDCC = new SubReportDetailsModel.DCC_Filter();
            
		}
		public int Id { get; set; }
		public int AppBudgetServiceId { get; set; }
		public int AgencyId { get; set; }
		public SubReportDetailsModel.SubReportFilter Filter { get; set; }
        public SubReportDetailsModel.MonthsFilter FilterByMonth { get; set; }

        public SubReportDetailsModel.SC_Filter FilterForSC { get; set; }

        public SubReportDetailsModel.DCC_Filter FilterForDCC { get; set; }


	}

   



	public class SubReportModelBase : ModelBase
	{
		#region Properties
		/// <summary>
		/// surrogate key
		/// </summary>
		[Display(Name = "Report ID")]
		public virtual int? Id { get; set; }

		public virtual int? AppBudgetServiceId { get; set; }

		/// <summary>
		/// key
		/// </summary>
		public virtual int AgencyId { get; set; }

		/// <summary>
		/// key
		/// </summary>
		public virtual int ServiceId { get; set; }

		/// <summary>
		/// key
		/// </summary>
		public virtual int? MainReportId { get; set; }

		public virtual DateTime MainReportStart { get; set; }
		public virtual DateTime MainReportEnd { get; set; }

		/// <summary>
		/// Reporting method id
		/// </summary>
		public int ReportingMethodId { get; set; }
		/// <summary>
		/// reporting method enum
		/// </summary>
		public Service.ReportingMethods ReportingMethod { get { return (Service.ReportingMethods)this.ReportingMethodId; } }

		public Header DetailsHeader { get; set; }

		public IGenericRepository<Service> ServicesRepository { get; set; }
		public IGenericRepository<Agency> AgenciesRepository { get; set; }
		public IGenericRepository<MainReport> MainReportsRepository { get; set; }
		public IGenericRepository<SubReport> SubReportsRepository { get; set; }
		public IGenericRepository<AppBudgetService> AppBudgetServicesRepository { get; set; }
		public IGenericRepository<ClientReport> ClientReportsRepository { get; set; }
		public IGenericRepository<Client> ClientsRepository { get; set; }

		#endregion

		#region Ctors, Methods
		public virtual void LoadData() { }

		public SubReportModelBase()
		{
			this.DetailsHeader = new Header();
		}
		public SubReportModelBase(SubReportModelBase modelBase)
		{
			this.Id = modelBase.Id;
			this.MainReportId = modelBase.MainReportId;
			this.AgencyId = modelBase.AgencyId;
			this.ServiceId = modelBase.ServiceId;


		}

		#endregion
		public class Header : SubReport
		{
			public Header() { }
			public Header(SubReport subreport)
			{


			}
			[Display(Name = "Detailed Report Id")]
			public override int Id { get; set; }
			public bool Editable { get; set; }
			//display data
			[Display(Name = "Agency")]
			public string AgencyName { get; set; }



			public int SerId { get; set; }
			[Display(Name = "Ser")]
			public string SerName { get; set; }

			[Display(Name = "Service")]
			public string ServiceName { get; set; }

			public string ServiceTypeName { get; set; }

			[Display(Name = "Period")]
			[UIHint("MonthsRange")]
			public Range<DateTime> Period { get; set; }

			public int MainReportStatusId { get; set; }
			[Display(Name = "Financial Report Status")]
			public MainReport.Statuses? MainReportStatus { get { return Enum.IsDefined(typeof(MainReport.Statuses), this.MainReportStatusId) ? (MainReport.Statuses)this.MainReportStatusId : (MainReport.Statuses?)null; } }


			[Display(Name = "Total Amount")]
			public  decimal? Amount { get; set; } //LenaAmount
            //public decimal TotalAmountReported { get; set; }


            [Display(Name = "Total number of households served")]
			public override int? TotalHouseholdsServed { get; set; }

			/// <summary>
			/// Reporting method id
			/// </summary>
			public int ReportingMethodId { get; set; }
			/// <summary>
			/// reporting method enum
			/// </summary>
			public Service.ReportingMethods ReportingMethod { get { return (Service.ReportingMethods)this.ReportingMethodId; } }

			[Display(Name = "Service Units")]
			public override decimal? ServiceUnits
			{
				get;
				set;
			}

			public bool AmountEnabled
			{
				get
				{
                    if (this.ServiceName != "Funeral Expenses")
                    {
                        return !Service.IsAmountReportedPerClient(this.ReportingMethod);
                    }
                    else
                   
                    { return false; };
                }
                
            }
			public bool ServiceUnitsEnabled
			{
				get
				{
					switch (this.ServiceTypeName.ToLower().Trim())
					{
						case "food programs":
							return true;
						default:
							return false;
					}
				}
			}
			public bool IsWinterRelief
			{
				get
				{
					switch (this.ServiceTypeName.ToLower().Trim())
					{

						case "winter relief": return true;

						default: return false;
					}
				}
			}



			[Display(Name = "Matching Sum")]
			[Range(0, double.MaxValue, ErrorMessage = "Matching Sum cannot be negative")]
			public override decimal? MatchingSum { get; set; }

			[Display(Name = "Agency Contribution")]
			[Range(0, double.MaxValue, ErrorMessage = "Agency Contribution cannot be negative")]
			public override decimal? AgencyContribution { get; set; }

			public string Remarks
			{
				get
				{
					switch (this.ServiceTypeName.ToLower().Trim())
					{
						case "respite care (adult day care)":
							return "Israeli Agency’s will report directly into their system. In later stage after current development scope has finished, the Israeli system will connect to the new system.";
						default:
							return null;
					}
				}
			}


			public bool MatchingSumRequired { get; set; }

			public string AppBudgetServiceRemarks { get; set; }
			public string AppBudgetServiceCleanRemarks
			{
				get
				{
					if (this.AppBudgetServiceRemarks == null) return null;
					var clean = new System.Text.RegularExpressions.Regex("<[^>]*>").Replace(this.AppBudgetServiceRemarks, string.Empty);
					return clean;
				}
			}
			public string AppBudgetServiceShortRemarks
			{
				get
				{
					if (this.AppBudgetServiceCleanRemarks == null) return null;
					else return new String(this.AppBudgetServiceCleanRemarks.Take(20).ToArray());
				}
			}

			[Display(Name="Fund")]
			public string FundName { get; set; }

			[Display(Name="App #")]
			public string AppName { get; set; }

			[Display(Name="Starting Week Day")]
			public DayOfWeek StartWeekDay { get; set; }
		}
	}

}