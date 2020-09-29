using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CC.Data;
using System.Web.Mvc;
using CC.Web.Models.Reporting;

namespace CC.Web.Models
{
	/// <summary>
	/// Used to render the main reports list
	/// </summary>
	public class ReportingHomeModel : ModelBase
	{
		public ReportingHomeModel()
		{
			this.FundStatusRepfilter = new FundStatusReportFilter();
			this.GgRepFilter = new GgReportFilter();
			this.AnnualGGReportFilter = new GgReportFilter();
			this.LeaveDateFilter = new LeaveDateRemovedFilter();
			this.ReportingServiceTypePctFilter = new YtdReportinServiceTypePctFilter();
			this.QtrReportingServiceTypePctFilter = new QtrReportinServiceTypePctFilter();
			this.HcCostsFilter = new HomecareCostsFilter();
			this.ClientHrsFilter = new ClientHoursFilter();
			this.BSTPFilter = new BudgetServiceTypePercentsFilter();
            this.auditDiagnosticsFilter = new AuditDiagnosticsFilter();
            this.appBudgetFilter = new AppBudgetFilter();
            this.PLDRFilter = new PostLeaveDateReportedFilter();
			this.appBalanceFilter = new AppBalanceFilter();
            this.approvalStatusFilter = new ApprovalStatusFilter();
            this.functionalityChangeFilter = new FunctionalityChangeFilter();
            this.ddeFilter = new DeceasedDateEntryFilter();
            this.hseapFilter = new HseapDetailedFilter();
			this.statusChangeFilter = new StatusChangeReportFilter();
		}

		public FundStatusReportFilter FundStatusRepfilter { get; set; }
		public GgReportFilter GgRepFilter { get; set; }
		public GgReportFilter AnnualGGReportFilter { get; set; }
		public LeaveDateRemovedFilter LeaveDateFilter { get; set; }
		public YtdReportinServiceTypePctFilter ReportingServiceTypePctFilter { get; set; }
		public QtrReportinServiceTypePctFilter QtrReportingServiceTypePctFilter { get; set; }
		public HomecareCostsFilter HcCostsFilter { get; set; }
		public ClientHoursFilter ClientHrsFilter { get; set; }
		public BudgetServiceTypePercentsFilter BSTPFilter { get; set; }
        public AuditDiagnosticsFilter auditDiagnosticsFilter { get; set; }
        public AppBudgetFilter appBudgetFilter { get; set; }
        public PostLeaveDateReportedFilter PLDRFilter { get; set; }
		public AppBalanceFilter appBalanceFilter { get; set; }
        public ApprovalStatusFilter approvalStatusFilter { get; set; }
        public FunctionalityChangeFilter functionalityChangeFilter { get; set; }
        public DeceasedDateEntryFilter ddeFilter { get; set; }
        public HseapDetailedFilter hseapFilter { get; set; }
		public StatusChangeReportFilter statusChangeFilter { get; set; }

		public static IEnumerable<FundStatusReportDataRow> LoadData(CC.Data.ccEntities db, CC.Data.Services.IPermissionsBase permissions, CC.Data.User user, FundStatusReportFilter filter)
		{
			var dbData = from cr in db.ViewClientReports
						 join sra in db.viewSubreportAmounts on cr.SubReportId equals sra.id
						 join abs in db.AppBudgetServices.Where(f => !f.Agency.AgencyGroup.ExcludeFromReports) on sra.AppBudgetServiceId equals abs.Id
						 join mr in db.MainReports on sra.MainReportId equals mr.Id
						 join c in db.Clients on cr.ClientId equals c.Id
						 select new FundStatusReportDataRow
						 {
							 Active = c.DeceasedDate == null && c.LeaveDate == null,
							 AgencyName = c.Agency.Name,
                             AgencyId = c.AgencyId,
							 Amount = cr.Amount ?? sra.EstAmountPerClient,
							 AppId = mr.AppBudget.AppId,
							 AppName = mr.AppBudget.App.Name,
							 BirthDate = c.BirthDate,
							 ClientId = c.Id,
							 CurrencyId = mr.AppBudget.App.CurrencyId,
							 End = mr.End,
							 FirstName = c.FirstName,
							 FundId = mr.AppBudget.App.FundId,
							 FundName = mr.AppBudget.App.Fund.Name,
							 FundStatusId = c.FundStatusId,
							 FundStatusName = c.FundStatus.Name,
							 LastName = c.LastName,
							 SerId = c.Agency.GroupId,
							 SerName = c.Agency.AgencyGroup.Name,
							 ServiceId = abs.ServiceId,
							 ServiceName = abs.Service.Name,
							 Start = mr.Start,
							 Year = System.Data.Objects.SqlClient.SqlFunctions.DatePart("year", mr.Start),
						 };
			if (filter.FundStatusesString.Any())
				dbData = dbData.Where(f => filter.FundStatusesString.Contains(f.FundStatusId??0));
			if (filter.Funds.Any())
                dbData = dbData.Where(f => filter.Funds.Contains(f.FundId??0));
			if (filter.Year.HasValue)
				dbData = dbData.Where(f => f.Year == (int)filter.Year.Value);

			var result = dbData.ToList();

			return result;
		}

		public static IEnumerable<LeaveDateDataRow> LoadLeaveDateData(CC.Data.ccEntities db, CC.Data.Services.IPermissionsBase permissions, CC.Data.User user, LeaveDateRemovedFilter filter)
		{
			var leaveDateHistory = from h in db.viewDatesHistories
								   where h.TableName == "Clients" && h.FieldName == "LeaveDate"
								   select h;

			var q = from h in leaveDateHistory
					where h.UpdateDate > filter.ChangedDate
					where h.NewValue == null
					join c in db.Clients.Where(permissions.ClientsFilter).Where(f => !f.Agency.AgencyGroup.ExcludeFromReports) on h.ReferenceId equals c.Id
					join ch in leaveDateHistory on new { ClientId = c.Id, LeaveDate = c.LeaveDate } equals new { ClientId = ch.ReferenceId, LeaveDate = ch.NewValue } into chGroup
					let ch = chGroup.OrderByDescending(f => f.UpdateDate).FirstOrDefault()
					join s in
						(from cr in db.viewClientReportsEsts
						 where (cr.ReportDate ?? cr.EstReportDate) >= filter.ChangedDate
						 join sr in db.SubReports on cr.SubReportId equals sr.Id
						 select new
						 {
							 ClientId = cr.ClientId,
							 ServiceName = sr.AppBudgetService.Service.ServiceType.Name + sr.AppBudgetService.Service.Name
						 }).Distinct() on c.Id equals s.ClientId into sg
					select new LeaveDateDataRow
						{
							ClientId = c.Id,
							FirstName = c.FirstName,
							LastName = c.LastName,
							AgencyName = c.Agency.Name,
							SerName = c.Agency.AgencyGroup.Name,
							LeaveDateEntered = h.UpdateDate,
							Services = sg.Select(f => f.ServiceName),
							ApprovalStatus = c.ApprovalStatus.Name,
							CurrentLeaveDate = c.LeaveDate,
							CurrentLeaveDateEntered = (DateTime?)ch.UpdateDate,
						};

			return q.ToList();
		}

		#region filter classes
		public class FundStatusReportFilter
		{
			public FundStatusReportFilter()
				: base()
			{
				this.Year = DateTime.Now.Year;
			}

			[Display(Name = "Fund")]
			public int[] Funds { get; set; }


			[Display(Name = "Fund Statuses")]
			public int[] FundStatusesString{get;set;}
			

			[Display(Name = "Year")]
			public int? Year { get; set; }

		}

		public class GgReportFilter
		{
			public GgReportFilter()
			{
				this.Year = DateTime.Now.Year;
			}

			[Display(Name = "Fund")]
			[Required]
			public string Funds { get; set; }

			public IEnumerable<int> FundIds
			{
				get
				{
					if (this.Funds == null) return new List<int>();
					else return this.Funds.Trim().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse);
				}
			}



			[Display(Name = "Year")]
			[Required]
			public int? Year { get; set; }
		}

		public class LeaveDateRemovedFilter
		{
			public LeaveDateRemovedFilter()
			{
				this.ChangedDate = DateTime.Now;
			}

			[Display(Name = "Leave date changed since")]
			[Required]
			[DateFormat()]
			[DataType(DataType.Date)]
			public DateTime ChangedDate { get; set; }
		}

		public class HomecareCostsFilter
		{
			public HomecareCostsFilter()
			{
				this.subtotals = true;
			}

			[Display(Name = "Region")]
			public int? RegionId { get; set; }

			[Display(Name = "Show sub totals")]
			public bool subtotals { get; set; }
		}

		public class ClientHoursFilter : IValidatableObject
		{
			public ClientHoursFilter()
			{
				this.subtotals = true;
			}

			[Display(Name = "Region")]
			public int? RegionId { get; set; }

			[Display(Name = "From (Month)")]
			[Required]
			public int? FromMonth { get; set; }

			[Display(Name = "From (Year)")]
			[Required]
			public int? FromYear { get; set; }

			[Display(Name = "To (Month)")]
			[Required]
			public int? ToMonth { get; set; }

			[Display(Name = "To (Year)")]
			[Required]
			public int? ToYear { get; set; }

			[Display(Name = "Show sub totals")]
			public bool subtotals { get; set; }

			public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
			{
				if (FromMonth > ToMonth || FromYear > ToYear)
					yield return new ValidationResult("End Month/Year must be greater or equal to Start Month/Year.");
			}
		}

		public class BudgetServiceTypePercentsFilter
		{
			public BudgetServiceTypePercentsFilter()
			{
				this.IgnoreUnsubmitted = true;
			}

			[Required()]
			[Display(Name = "Select A Display Currency")]
			public string DisplayCurrency { get; set; }

			public System.Web.Mvc.SelectList ConvertableCurrencies { get { return new System.Web.Mvc.SelectList(CC.Data.Currency.ConvertableCurrencies); } }

			[Display(Name = "SER")]
			public int? AgencyGroupId { get; set; }

			[Display(Name = "Fund")]
			public int? FundId { get; set; }

			[Display(Name = "Ignore  new/ returened to agency budgets")]
			public bool IgnoreUnsubmitted { get; set; }

			[Display(Name = "Region")]
			public int? RegionId { get; set; }

			[Display(Name = "Country")]
			public int? CountryId { get; set; }

			[Display(Name = "State")]
			public int? StateId { get; set; }

			[Display(Name = "Select A Year")]
			public int? Year { get; set; }
		}

        public class AuditDiagnosticsFilter : IValidatableObject
        {
            public AuditDiagnosticsFilter() { }

            [Display(Name = "Agency")]
            public int? AgencyId { get; set; }

            [Display(Name = "From (Month)")]
            [Required]
            public int? FromMonth { get; set; }

            [Display(Name = "From (Year)")]
            [Required]
            public int? FromYear { get; set; }

            [Display(Name = "To (Month)")]
            [Required]
            public int? ToMonth { get; set; }

            [Display(Name = "To (Year)")]
            [Required]
            public int? ToYear { get; set; }

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (FromMonth > ToMonth || FromYear > ToYear)
                    yield return new ValidationResult("End Month/Year must be greater or equal to Start Month/Year.");
            }
        }

        public class AppBudgetFilter
        {
            public AppBudgetFilter() { }

            [Display(Name = "SER")]
            public int? AgencyGroupId { get; set; }

            [Display(Name = "Fund")]
            public int? FundId { get; set; }

            [Display(Name = "Budget Status")]
            public int? StatusId { get; set; }

            public System.Web.Mvc.SelectList Statuses 
            {
                get
                {
                    var statuses = new Dictionary<int, string>();
                    statuses.Add(-1, "");
                    foreach (AppBudgetApprovalStatuses status in Enum.GetValues(typeof(AppBudgetApprovalStatuses)))
                    {
                        statuses.Add((int)status, status.ToString().SplitCapitalizedWords());
                    }

                    return new System.Web.Mvc.SelectList(statuses, "Key", "Value");
                }
            }
        }

        public class PostLeaveDateReportedFilter
        {
            public PostLeaveDateReportedFilter() { }

            [Display(Name = "Type")]
            public int TypeId { get; set; }

            [Display(Name = "From (Month)")]
            public int? FromMonth { get; set; }

            [Display(Name = "From (Year)")]
            public int? FromYear { get; set; }

            [Display(Name = "To (Month)")]
            public int? ToMonth { get; set; }

            [Display(Name = "To (Year)")]
            public int? ToYear { get; set; }

            [Display(Name = "SER")]
            public string AgencyGroups { get; set; }
        }

		public class AppBalanceFilter
		{
			public AppBalanceFilter()
			{
				using (var db = new ccEntities())
				{
					var q = (from app in db.Apps select (int)(System.Data.Objects.SqlClient.SqlFunctions.DatePart("year", app.StartDate)));
					var min = q.Min();
					var max = q.Max();
					Years = Enumerable.Range(min, max - min + 1);
				}
			}

			public IEnumerable<int> Years { get; set; }

			[Display(Name="Fund")]
			public int? FundId { get; set; }
			
			[Display(Name="Year")]
			public int? Year { get; set; }

		}

        public class ApprovalStatusFilter
        {
            public ApprovalStatusFilter()
            {
                DateTime Today = DateTime.Now;
                this.DateFrom = new DateTime(Today.Year, Today.AddMonths(-1).Month, 15);
                this.DateTo = new DateTime(Today.Year, Today.Month, 14);
            }

            [Display(Name = "Region")]
            public int? RegionId { get; set; }

            [Display(Name = "Country")]
            public int? CountryId { get; set; }

            [Display(Name = "Ser")]
            public int? AgencyGroupId { get; set; }

            [Display(Name = "From")]
            [DateFormat()]
            [DataType(DataType.Date)]
            public DateTime? DateFrom { get; set; }

            [Display(Name = "To")]
            [DateFormat()]
            [DataType(DataType.Date)]
            public DateTime? DateTo { get; set; }
        }

        public class FunctionalityChangeFilter
        {
            public FunctionalityChangeFilter() { }

            [Display(Name = "Region")]
            public int? RegionId { get; set; }

            [Display(Name = "Country")]
            public int? CountryId { get; set; }

            [Display(Name = "Ser")]
            public int? AgencyGroupId { get; set; }
        }

        public class DeceasedDateEntryFilter
        {
            public DeceasedDateEntryFilter() { }

            [Display(Name = "Region")]
            public int? RegionId { get; set; }

            [Display(Name = "Country")]
            public int CountryId { get; set; }

            [Display(Name = "Ser")]
            public int? AgencyGroupId { get; set; }
        }

        public class HseapDetailedFilter
        {
            public HseapDetailedFilter()
            {
                using (var db = new ccEntities())
                {
                    var q = (from mr in db.MainReports select (int)(System.Data.Objects.SqlClient.SqlFunctions.DatePart("year", mr.Start)));
                    var min = q.Min();
                    var max = q.Max();
                    Years = Enumerable.Range(min, max - min + 1);
                }
                Year = DateTime.Now.Year;
            }

            public IEnumerable<int> Years { get; set; }

            [Display(Name = "Master Fund")]
            public string MasterFunds { get; set; }

            [Display(Name = "Fund Code")]
            public string Funds { get; set; }

            [Display(Name = "App")]
            public string Apps { get; set; }

            [Display(Name = "Year")]
            public int? Year { get; set; }

            [Display(Name = "Region")]
            public string Regions { get; set; }

            [Display(Name = "Country")]
            public string Countries { get; set; }

            [Display(Name = "SER")]
            public string AgencyGroups { get; set; }

            [Display(Name = "Agency")]
            public string Agencies { get; set; }
        }

		public class StatusChangeReportFilter
		{
			public StatusChangeReportFilter()
			{
				
			}

			[Display(Name = "Region")]
			public int? RegionId { get; set; }

			[Display(Name = "Country")]
			public int? CountryId { get; set; }

			[Display(Name = "Ser")]
			public int? AgencyGroupId { get; set; }

			[Display(Name = "Master Fund")]
			public int? MasterFundId { get; set; }

			[Display(Name = "Fund")]
			public int? FundId { get; set; }

			[Display(Name = "App")]
			public int? AppId { get; set; }

			[Display(Name = "Year")]
			public int? Year { get; set; }

			[Display(Name = "Include Approved Reports?")]
			public bool IncludeApproved { get; set; }
		}

		#endregion

		#region FundStatus rep row
		public class FundStatusReportDataRow
		{
			[ScaffoldColumn(false)]
			public int? SerId { get; set; }
			[ScaffoldColumn(false)]
			public int? FundId { get; set; }
			[ScaffoldColumn(false)]
			public int? FundStatusId { get; set; }
			[ScaffoldColumn(false)]
			public int? AppId { get; set; }
			[ScaffoldColumn(false)]
			public int? ServiceId { get; set; }

			[Display(Name = "CC ID", Order = 4)]
			public int? ClientId { get; set; }

			[Display(Name = "Agency", Order = 1)]
			public string AgencyName { get; set; }

            [Display(Name = "Org ID", Order = 2)]
            public int? AgencyId { get; set; }

			[Display(Name = "App", Order = 9)]
			public string AppName { get; set; }

			[Display(Name = "Ser", Order = 0)]
			public string SerName { get; set; }

			[Display(Name = "Service", Order = 13)]
			public string ServiceName { get; set; }

			[Display(Name = "Last Name", Order = 5)]
			public string LastName { get; set; }

			[Display(Name = "First Name", Order = 6)]
			public string FirstName { get; set; }

			[Display(Name = "DOB", Order = 7)]
			public DateTime? BirthDate { get; set; }

			[Display(Name = "Report Start", Order = 11)]
			public DateTime? Start { get; set; }

			[Display(Name = "Report End", Order = 12)]
			public DateTime? End { get; set; }

			[Display(Name = "Year", Order = 10)]
			public int? Year { get; set; }

			[Display(Name = "Active (Y/N)", Order = 16)]
			public bool Active { get; set; }

			[Display(Name = "Fund Status", Order = 3)]
			public string FundStatusName { get; set; }

			[Display(Name = "Fund", Order = 8)]
			public string FundName { get; set; }

			[Display(Name = "Estimated Amount", Order = 14)]
			public decimal? Amount { get; set; }

			[Display(Name = "App Currency", Order = 15)]
			public string CurrencyId { get; set; }
		}
		#endregion

		#region changed leave date row
		public class LeaveDateDataRow
		{
			[Display(Name = "CC ID", Order = 0)]
			public int? ClientId { get; set; }

			[Display(Name = "First Name", Order = 1)]
			public string FirstName { get; set; }

			[Display(Name = "Last Name", Order = 2)]
			public string LastName { get; set; }

			[Display(Name = "Agency", Order = 3)]
			public string AgencyName { get; set; }

			[Display(Name = "Ser", Order = 4)]
			public string SerName { get; set; }

			[Display(Name = "Empty Leave Date Entered At", Order = 5)]
			public DateTime LeaveDateEntered { get; set; }

			[Display(Name = "Current Leave Date Entered At", Order = 6)]
			public DateTime? CurrentLeaveDateEntered { get; set; }

			[Display(Name = "Current Leave Date", Order = 7)]
			public DateTime? CurrentLeaveDate { get; set; }

			[Display(Name = "Approval Status", Order = 8)]
			public string ApprovalStatus { get; set; }

			[Display(Name = "Services this client is included in during the filter period", Order = 9)]
			[ScaffoldColumn(false)]
			public IEnumerable<string> Services { get; set; }

			public string ServicesString
			{
				get
				{
					if (this.Services == null) return null;
					else return string.Join(", ", Services.OrderBy(f => f));
				}
			}
		}
		#endregion


	}






}