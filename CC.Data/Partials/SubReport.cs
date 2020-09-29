using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using CC.Data.Partials;

namespace CC.Data
{
	public partial class SubReport : IValidatableObject
	{



		/// <summary>
		/// Validate Primitive Properties
		/// </summary>
		/// <param name="validationContext"></param>
		/// <returns></returns>
		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{


			//check if clientreports have been loaded

			//Validate each clientreport
			if (validationContext.ObjectInstance == null)
			{
				foreach (var clientReport in this.ClientReports.AsParallel())
				{
					foreach (var vr in clientReport.Validate(new ValidationContext(clientReport, null, null)))
					{
						yield return vr;
					}
				}
			}



			yield break;
		}




		#region Validations

		//"1.4.15.3.3.        Join Date- Clients that joined the agency could not be reported for hours in months before the month of their join date.  "
		public bool ValidateJoinDate()
		{
			using (var db = new ccEntities())
			{
				return true;
			}
		}
		/// <summary>
		/// "1.4.15.3.4.        Leave Date- Clients that left the agency could not be reported for hours in months after the month of their leave date."
		/// </summary>
		/// <returns></returns>
		public bool ValidateLeaveDate()
		{
			return true;
		}


		#endregion

		public static readonly int DeceasedDaysOverhead = 0;

        public static readonly int EAPDeceasedDaysOverhead = 90;

		public static readonly System.Linq.Expressions.Expression<Func<SubReport, bool>> IsAdministrativeOverhead = (sr) =>
			sr.AppBudgetService.Service.ServiceType.Name.Equals("Administrative Overhead", StringComparison.CurrentCultureIgnoreCase);

		public static readonly System.Linq.Expressions.Expression<Func<SubReport, bool>> IsHomeCare = (sr) =>
			sr.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.Homecare
			|| sr.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.HomecareWeekly;

	}

	public class SubReportWithTotalAmount
	{
		public SubReport SubReport { get; set; }
		public decimal TotalAmount { get; set; }
        //public ClientReport ClientReport { get; set; }
    }



	public class SubServiceRowModes
	{
		[ScaffoldColumn(false)]
		public AppBudgetService AppBudgetService { get; set; }
		[ScaffoldColumn(false)]
		public SubReport SubReport { get; set; }
		[ScaffoldColumn(false)]
       // public ClientReport ClientReport { get; set; }
        //[ScaffoldColumn(false)]
        public int? Id { get; set; }
		[ScaffoldColumn(false)]
		public int AgencyId { get; set; }
		[ScaffoldColumn(false)]
		public int? SerId { get; set; }
		[ScaffoldColumn(false)]
		public string SerName { get; set; }
		[ScaffoldColumn(false)]
		public int ServiceId { get; set; }
		[ScaffoldColumn(false)]
		public int? MainReportId { get; set; }
		[ScaffoldColumn(false)]
		public decimal? ExcRate { get; set; }
		[ScaffoldColumn(false)]
		public int AppBudgetServiceId { get; set; }
		[ScaffoldColumn(false)]
		public string Cur { get; set; }

		[ScaffoldColumn(false)]
		public decimal? AgencyContribution { get; set; }
		[ScaffoldColumn(false)]
		public decimal? ApprovedRequiredMatch { get; set; }
		[ScaffoldColumn(false)]
        public decimal? AmountFuneralExpences { get; set; }
        [ScaffoldColumn(false)]

        public int TotalClients { get; set; }
		[ScaffoldColumn(false)]
		public decimal? MatchingSum { get; set; }
		[ScaffoldColumn(false)]
		public decimal? RelativeRequiredMatch { get; set; }

		[DisplayName("Agency")]
		public string AgencyName { get; set; }

		[Display(Name = "Service Type")]
		public string ServiceTypeName { get; set; }

		[Display(Name = "Service Name")]
		public string ServiceName { get; set; }


		#region mainreport list

		[Display(Name = "CC Exp")]
		[Description("Amount of current subreport in Ser currency")]
		public decimal? CcExp { get; set; }

        public decimal? CcExpTotal { get; set; }

        [DisplayName("CUR")]
		public string CcExpCur { get { return this.Cur; } }

		[Display(Name = "Match Exp.")]
		[Description("Matching Sum of current subreport")]
		public decimal? MatchExp { get; set; }
		[DisplayName("CUR")]
		public string MatchExpCur { get { return this.Cur; } }

		[Display(Name = "YTD CC Exp.")]
		[Description("Total CC Exp. reported on this Budget including current report")]
		public decimal? YtdCcExp { get; set; }
		[DisplayName("CUR")]
		public string YtdCcExpCur { get { return this.Cur; } }

		[Display(Name = "YTD Matching Exp")]
		[Description("Total Matching sum reported on this app including current report")]
		public decimal? YtdMatchExp { get; set; }
		[DisplayName("CUR")]
		public string YtdMatchExpCur { get { return this.Cur; } }

		[Display(Name = "Matching Sum Status")]
		public string MatchingSummStatusName
		{
			get
			{
				if (MatchingSumStatus == null) return "";
				else if (MatchingSumStatus > 0) return "OK";
				else return MatchingSumStatus.Value.Format();
			}
		}

		#endregion

		[ScaffoldColumn(false)]
		[Display(Name = "Estimated Matching Sum Spent Required up to date (including current period)")]
		[Description("Relative Matching Sum of associated approved App")]
		public decimal YtdAppMatchingSum { get; set; }

		[ScaffoldColumn(false)]
		[Description("Relative Ytd CC Grant")]
		public decimal YtdAppCcGrant { get; set; }

		[ScaffoldColumn(false)]
		public decimal? MatchingSumStatus { get { return RelativeRequiredMatch.HasValue ? YtdMatchExp.GetValueOrDefault() - RelativeRequiredMatch.GetValueOrDefault() : (decimal?)null; } }

		[ScaffoldColumn(false)]
		public decimal CcGrantStatus { get; set; }

		[ScaffoldColumn(false)]
		public int? UtdMatch { get; set; }

        public static IQueryable<SubServiceRowModes> asdf(IQueryable<SubRepAppBudgetPair> source)
        {
            var q = source.Select(a =>
                 new
                 {
                     SubReport = a.SubReport,
                     AppBudgetService = a.AppBudgetService,
                     CcExp = (a.SubReport.Amount ?? 0),
                    // CCExpTotal = (a.ClientReport.Amount ?? 0),
                     MatchingSum = a.SubReport.MatchingSum,
                     MatchExp = (a.SubReport.MatchingSum ?? 0),
                     AppLen = System.Data.Objects.EntityFunctions.DiffDays(a.SubReport.AppBudgetService.AppBudget.App.StartDate, a.SubReport.AppBudgetService.AppBudget.App.EndDate),
                     MrLen = a.SubReport.AppBudgetService.SubReports
                            .Where(sr => sr.MainReportId == a.SubReport.MainReportId
                                    || sr.MainReport.StatusId == (int)MainReport.Statuses.Approved
                                    || sr.MainReport.StatusId == (int)MainReport.Statuses.AwaitingProgramAssistantApproval
                                    || sr.MainReport.StatusId == (int)MainReport.Statuses.AwaitingProgramOfficerApproval
                                    || sr.MainReport.StatusId == (int)MainReport.Statuses.AwaitingAgencyResponse)
                            .Where(sr => sr.MainReport.Start <= a.SubReport.MainReport.Start)
                         .Select(sr => sr.MainReport).Distinct()
                         .Sum(mr => System.Data.Objects.EntityFunctions.DiffDays(mr.Start, mr.End))
                     ,
                     YtdSubReports = a.AppBudgetService.SubReports
                         .Where(sr => sr.MainReportId == a.SubReport.MainReportId
                                    || sr.MainReport.StatusId == (int)MainReport.Statuses.Approved
                                    || sr.MainReport.StatusId == (int)MainReport.Statuses.AwaitingProgramAssistantApproval
                                    || sr.MainReport.StatusId == (int)MainReport.Statuses.AwaitingProgramOfficerApproval
                                    || sr.MainReport.StatusId == (int)MainReport.Statuses.AwaitingAgencyResponse)
                         .Where(sr => sr.MainReport.Start <= a.SubReport.MainReport.Start)
                         .Select(sr => new
                         {
                             ExcRate = sr.MainReport.ExcRate,
                             Amount = ((decimal?)sr.Amount ?? 0)
                         })
                        ,

                     YtdMatchingSum = (a.AppBudgetService.SubReports
                        .Where(sr => sr.MainReportId == a.SubReport.MainReportId
                            || sr.MainReport.StatusId == (int)MainReport.Statuses.Approved
                            || sr.MainReport.StatusId == (int)MainReport.Statuses.AwaitingProgramAssistantApproval
                            || sr.MainReport.StatusId == (int)MainReport.Statuses.AwaitingProgramOfficerApproval
                            || sr.MainReport.StatusId == (int)MainReport.Statuses.AwaitingAgencyResponse)
                        .Where(sr => sr.MainReport.Start <= a.SubReport.MainReport.Start)
                        .Sum(sr => sr.MatchingSum) ?? 0),
                     AppCcGrant = a.AppBudgetService.AppBudget.App.CcGrant,
                     AppMatchingSum = a.AppBudgetService.AppBudget.App.RequiredMatch,
                     ApprovedCcGrant = a.AppBudgetService.CcGrant,
                     TotalClients = a.SubReport.ClientReports.Select(f => f.ClientId)
                        .Union(a.SubReport.EmergencyReports.Select(f => f.ClientId))
                        .Union(a.SubReport.SupportiveCommunitiesReports.Select(f => f.ClientId))
                        .Union(a.SubReport.DaysCentersReports.Select(f => f.ClientId))
                        .Union(a.SubReport.SoupKitchensReports.Select(f => f.ClientId))
                        .Distinct().Count(),
                     ApprovedMatchingSum = a.AppBudgetService.AppBudget.StatusId == (int)AppBudgetApprovalStatuses.Approved ? a.AppBudgetService.RequiredMatch : (decimal?)null,
                     // AmountFuneralExpences = (a.ClientReport.Amount ?? 0) //a.SubReport.ClientReports.Select(f => f.Amount)
                     //AmountFuneralExpences = a.ClientReport.Amount
                     
                 }
            ).Select(a => new SubServiceRowModes()
            {

                Id = (int?)a.SubReport.Id,
                SubReport = a.SubReport,
                AppBudgetService = a.AppBudgetService,
                MainReportId = (int?)a.SubReport.MainReportId,
                ExcRate = (Decimal?)a.SubReport.MainReport.ExcRate,
                AppBudgetServiceId = (int?)a.AppBudgetService.Id ?? 0,
                AgencyId = (int?)a.AppBudgetService.Agency.Id ?? 0,
                SerId = (int?)a.AppBudgetService.Agency.AgencyGroup.Id,
                SerName = a.AppBudgetService.Agency.AgencyGroup.DisplayName,
                ServiceId = (int?)a.AppBudgetService.Service.Id ?? 0,
                ServiceName = a.AppBudgetService.Service.Name,
                ServiceTypeName = a.AppBudgetService.Service.ServiceType.Name,
                AgencyName = a.AppBudgetService.Agency.Name,
                Cur = a.AppBudgetService.AppBudget.App.CurrencyId,
                CcExp = (decimal?)a.CcExp ?? 0,
               // CcExpTotal = (decimal?)a.CCExpTotal ?? 0,
               // AmountFuneralExpences = a.AmountFuneralExpences,
                MatchExp = (decimal)a.MatchExp,
                MatchingSum = a.MatchingSum,
                YtdCcExp = (decimal?)a.YtdSubReports.Sum(f => f.Amount) ?? 0,
                YtdMatchExp = (decimal)a.YtdMatchingSum,
                RelativeRequiredMatch = a.ApprovedMatchingSum * ((decimal?)a.MrLen / (decimal?)a.AppLen),
                AgencyContribution = a.SubReport.AgencyContribution,
                ApprovedRequiredMatch = a.ApprovedMatchingSum,
                TotalClients = a.TotalClients
               
            }
             );  

            return q;
		}
	}
	public class SubRepAppBudgetPair
	{
		public AppBudgetService AppBudgetService { get; set; }
		public SubReport SubReport { get; set; }
        public ClientReport ClientReport { get; set; }
    }
}
