using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using CC.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CC.Data.Services;
using System.Net.Mail;

namespace CC.Web.Models
{
	public class MainReportDetailsModel : MainReportDetailsBase
	{
        public MainReportDetailsModel() { SendEmail = true; }
		public MainReportDetailsModel(int id)
		{
			this.Id = id;
		}

		public IGenericRepository<AppBudgetService> appBudgetServicesRepository { get; set; }
		public IGenericRepository<SubReport> SubReportsRepository { get; set; }
		public bool CanChangeAdjusted()
		{
			return this.User.RoleId == (int)FixedRoles.Admin || (this.User.RoleId == (int)FixedRoles.RegionOfficer) || (this.User.RoleId == (int)FixedRoles.RegionAssistant) || (this.User.RoleId == (int)FixedRoles.GlobalOfficer);
		}

        public bool SendEmail { get; set; }

		public override void LoadData()
		{
			base.LoadData();

			this.StatusEditable = this.User.RoleId == (int)FixedRoles.Admin;



			this.CanAddAgencyRemarks = this.User.RoleId == (int)FixedRoles.Admin ||
				(MainReport.EditableStatuses.Contains(this.Status)
				&& (FixedRoles.AgencyUser | FixedRoles.Ser | FixedRoles.AgencyUserAndReviewer | FixedRoles.SerAndReviewer).HasFlag((FixedRoles)this.User.RoleId));

			this.CanAddPoRemarks = this.User.RoleId == (int)FixedRoles.Admin ||
				(
				(this.Status == MainReport.Statuses.AwaitingProgramOfficerApproval || this.Status == MainReport.Statuses.AwaitingProgramAssistantApproval)
				&& (FixedRoles.GlobalOfficer | FixedRoles.RegionOfficer | FixedRoles.RegionAssistant).HasFlag((FixedRoles)this.User.RoleId));

			using (var db = new ccEntities())
			{
				var subreports = db.SubReports.Where(this.Permissions.SubReportsFilter);
				var prevStatus = db.MainReportStatusAudits.Where(f => f.MainReportId == this.Id).OrderByDescending(f => f.StatusChangeDate).FirstOrDefault();
				if(prevStatus != null)
				{
					this.PrevStatusId = prevStatus.OldStatusId;
					if(this.PoApprovalModel != null)
					{
						this.PoApprovalModel.PrevMainReportStatus = this.PrevStatus;
					}
					if(this.PaApprovalModel != null)
					{
						this.PaApprovalModel.PrevMainReportStatus = this.PrevStatus;
					}
				}

                var SerNameFE = (from cr in db.ClientReports
                                 join sbr in db.SubReports on cr.SubReportId equals sbr.Id
                                 join abs in db.AppBudgetServices on sbr.AppBudgetServiceId equals abs.Id
                                 join ser in db.Services on abs.ServiceId equals ser.Id
                                 where sbr.MainReportId == this.Id && ser.Id == 484//454 //&& cr.Amount != null
                                 select cr.Amount).Sum();

                if (SerNameFE != 0 && SerNameFE != null) //FE there
                {
                    var q = from agency in db.Agencies.Where(Permissions.AgencyFilter)
                            where agency.GroupId == this.AgencyGroupId
                            join a in
                                (from c in
                                     (
                                     from abs in db.AppBudgetServices
                                     where abs.AppBudgetId == this.AppBudgetId
                                     join sr in db.SubReports.Where(f => f.MainReportId == this.Id) on abs.Id equals sr.AppBudgetServiceId into srg
                                     from sr in srg.DefaultIfEmpty()
                                     select new
                                     {
                                         AgencyId = abs.AgencyId,
                                         Amount = sr.Amount,
                                         CcGrant = abs.CcGrant,
                                         MatchExp = sr.MatchingSum
                                     })
                                 group c by c.AgencyId into eg
                                 select new
                                 {
                                     AgencyId = eg.Key,
                                     Amount = eg.Sum(f => f.Amount),
                                     Match = eg.Sum(f => f.MatchExp),
                                     CcGrant = eg.Sum(f => f.CcGrant)

                                 }) on agency.Id equals a.AgencyId into ag
                            from a in ag.DefaultIfEmpty()

                            join b in
                                (
                                 from sra in db.viewSubreportAmounts
                                 join appbs in db.AppBudgetServices on sra.AppBudgetServiceId equals appbs.Id
                                 where appbs.AppBudgetId == this.AppBudgetId
                                 join mr in db.MainReports.Where(MainReport.CurrentOrSubmitted(Id)) on sra.MainReportId equals mr.Id
                                 where mr.Start <= this.Start
                                 where sra.MainReportId == this.Id

                                 group sra by appbs.AgencyId into g
                                 select new
                                 {
                                     AgencyId = g.Key,
                                     Amount = g.Sum(f => f.Amount),
                                     MatchExp = g.Sum(f => f.MatchingSum)

                                 }) on a.AgencyId equals b.AgencyId into bg
                            from b in bg.DefaultIfEmpty()

                            select new MainReportDetailsModel.AgnencyTotalsClass()
                            {
                                AgencyId = agency.Id,
                                AgencyName = agency.Name,
                                //CcExp = (decimal?)a.Amount, //+ (decimal?)b.Amount ,
                                //CcExpFE = (decimal?)a.Amount + (decimal?)b.Amount,
                                CcExp = (decimal?)b.Amount, //+ (decimal?)a.Amount,
                                YtdMatchExp = (decimal?)b.MatchExp,
                                YtdCcExp = (decimal?)b.Amount,
                                CcGrant = (decimal?)a.CcGrant,
                                CurrencyId = this.CurrencyId

                            };

                    var ytdServicesQuery = (from appb in db.AppBudgets
                                            where appb.Id == this.AppBudgetId
                                            from appbs in appb.AppBudgetServices
                                            join sra in db.viewSubreportAmounts on appbs.Id equals sra.AppBudgetServiceId
                                            join mr in db.MainReports.Where(MainReport.CurrentOrSubmitted(Id)) on sra.MainReportId equals mr.Id
                                            where mr.Start <= this.Start
                                            select new YtdServicesQueryRow
                                            {
                                                ServiceId = appbs.ServiceId,
                                                ServiceTypeId = appbs.Service.TypeId,
                                                ReportingMethodId = appbs.Service.ReportingMethodId,
                                                Amount = sra.Amount,
                                                Quantity = sra.Quantity
                                            });
                    var ytdServiceTypesQuery = (from a in
                                                    (from sra in ytdServicesQuery
                                                     group sra by sra.ServiceTypeId into srag
                                                     select new
                                                     {
                                                         ServiceTypeId = srag.Key,
                                                         Amount = srag.Sum(f => f.Amount),
                                                         Quantity = srag.Sum(f => f.Quantity)
                                                     })
                                                join st in db.ServiceTypes on a.ServiceTypeId equals st.Id
                                                select new
                                                {
                                                    ServiceTypeId = st.Id,
                                                    ServiceTypeName = st.Name,
                                                    Amount = a.Amount,
                                                    Quantity = a.Quantity ?? 1
                                                }).ToList();
                    var ytdAppQuery = (from appb in db.AppBudgets
                                       where appb.Id == this.AppBudgetId
                                       from appbs in appb.AppBudgetServices
                                       join sra in db.viewSubreportAmounts on appbs.Id equals sra.AppBudgetServiceId
                                       join mr in db.MainReports on sra.MainReportId equals mr.Id
                                       where mr.Start <= this.Start
                                       select new YtdServicesQueryRow
                                       {
                                           ServiceId = appbs.ServiceId,
                                           ServiceTypeId = appbs.Service.TypeId,
                                           ReportingMethodId = appbs.Service.ReportingMethodId,
                                           Amount = sra.Amount,
                                           AppMatchBal = (decimal?)sra.MatchingSum,
                                           Quantity = sra.Quantity
                                       });

                    var ytdTotalReportedAppQuery = (from a in
                                                   (from sra in ytdAppQuery
                                                    group sra by sra.ServiceTypeId into srag
                                                    select new
                                                    {
                                                        ServiceTypeId = srag.Key,
                                                        Amount = srag.Sum(f => f.Amount),
                                                        AppMatchBal = srag.Sum(f => f.AppMatchBal),
                                                        Quantity = srag.Sum(f => f.Quantity)
                                                    })
                                                    join st in db.ServiceTypes on a.ServiceTypeId equals st.Id
                                                    select new
                                                    {
                                                        ServiceTypeId = st.Id,
                                                        ServiceTypeName = st.Name,
                                                        Amount = a.Amount,
                                                        AppMatchBal = a.AppMatchBal,
                                                        Quantity = a.Quantity ?? 1
                                                    }).ToList();


                    //HcYtdPercentage = ytdServiceTypesQuery.Where(f => f.ServiceTypeId == (int)Service.ServiceTypes.Homecare).Sum(f => f.Amount) / total,
                    // CcExp = CcExp + ytdServicesQuery.y

                    decimal ? total = ytdServiceTypesQuery.Sum(f => f.Amount);
                   
                    if (total == 0) total = null;
                    decimal? totalApp = total;
                    decimal? totalAppMatchBal = ytdTotalReportedAppQuery.Sum(f => f.AppMatchBal);

                    if (totalAppMatchBal == 0) totalAppMatchBal = null;

                    this.Totals = new TotalsClass
                    {
                        AppAmount = this.AppAmount,
                        AppMatch = this.AppMatch,
                        CurrencyId = this.CurrencyId,
                        AgencySubTotals = q.ToList(),
                        HcYtdPercentage = ytdServiceTypesQuery.Where(f => f.ServiceTypeId == (int)Service.ServiceTypes.Homecare).Sum(f => f.Amount) / total,
                        AoYtdPercentage = ytdServiceTypesQuery.Where(f => f.ServiceTypeId == (int)Service.ServiceTypes.AdministrativeOverhead).Sum(f => f.Amount) / total,
                        TotalReportedApp = totalApp,
                        TotalReportedAppMatchBalance = totalAppMatchBal,

                    };
                     
                    this.ReimbursementCosts = GetAvgReimrusement(ytdServicesQuery);
                }

                else
                {

                    var q = from agency in db.Agencies.Where(Permissions.AgencyFilter)
                            where agency.GroupId == this.AgencyGroupId
                            join a in
                                (from c in
                                     (
                                     from abs in db.AppBudgetServices
                                     where abs.AppBudgetId == this.AppBudgetId
                                     join sr in db.SubReports.Where(f => f.MainReportId == this.Id) on abs.Id equals sr.AppBudgetServiceId into srg
                                     from sr in srg.DefaultIfEmpty()
                                     select new
                                     {
                                         AgencyId = abs.AgencyId,
                                         Amount = sr.Amount,
                                         CcGrant = abs.CcGrant,
                                         MatchExp = sr.MatchingSum
                                     })
                                 group c by c.AgencyId into eg
                                 select new
                                 {
                                     AgencyId = eg.Key,
                                     Amount = eg.Sum(f => f.Amount),
                                     Match = eg.Sum(f => f.MatchExp),
                                     CcGrant = eg.Sum(f => f.CcGrant)

                                 }) on agency.Id equals a.AgencyId into ag
                            from a in ag.DefaultIfEmpty()

                            join b in
                                (
                                 from sra in db.viewSubreportAmounts
                                 join appbs in db.AppBudgetServices on sra.AppBudgetServiceId equals appbs.Id
                                 where appbs.AppBudgetId == this.AppBudgetId
                                 join mr in db.MainReports.Where(MainReport.CurrentOrSubmitted(Id)) on sra.MainReportId equals mr.Id
                                 where mr.Start <= this.Start
                                 where sra.MainReportId == this.Id  //LenaK
                                 //join sbr in db.SubReports on sra.id equals sbr.Id

                                 group sra by appbs.AgencyId into g
                                 select new
                                 {
                                     AgencyId = g.Key,
                                     Amount = g.Sum(f => f.Amount),
                                     MatchExp = g.Sum(f => f.MatchingSum)

                                 }) on a.AgencyId equals b.AgencyId into bg
                            from b in bg.DefaultIfEmpty()

                            select new MainReportDetailsModel.AgnencyTotalsClass()
                            {
                                AgencyId = agency.Id,
                                AgencyName = agency.Name,
                                //CcExp = (decimal?)a.Amount, //+ (decimal?)b.Amount ,
                               // CcExpFE = (decimal?)a.Amount + (decimal?)b.Amount,
                                CcExp = (decimal?)a.Amount,
                                YtdMatchExp = (decimal?)b.MatchExp,
                                YtdCcExp =(decimal?)b.Amount,
                                CcGrant = (decimal?)a.CcGrant,
                                CurrencyId = this.CurrencyId

                            };

                    var ytdServicesQuery = (from appb in db.AppBudgets
                                            where appb.Id == this.AppBudgetId
                                            from appbs in appb.AppBudgetServices
                                            join sra in db.viewSubreportAmounts on appbs.Id equals sra.AppBudgetServiceId
                                            join mr in db.MainReports.Where(MainReport.CurrentOrSubmitted(Id)) on sra.MainReportId equals mr.Id
                                            where mr.Start <= this.Start
                                            select new YtdServicesQueryRow
                                            {
                                                ServiceId = appbs.ServiceId,
                                                ServiceTypeId = appbs.Service.TypeId,
                                                ReportingMethodId = appbs.Service.ReportingMethodId,
                                                Amount = sra.Amount,
                                                Quantity = sra.Quantity
                                            });
                    var ytdServiceTypesQuery = (from a in
                                                    (from sra in ytdServicesQuery
                                                     group sra by sra.ServiceTypeId into srag
                                                     select new
                                                     {
                                                         ServiceTypeId = srag.Key,
                                                         Amount = srag.Sum(f => f.Amount),
                                                         Quantity = srag.Sum(f => f.Quantity)
                                                     })
                                                join st in db.ServiceTypes on a.ServiceTypeId equals st.Id
                                                select new
                                                {
                                                    ServiceTypeId = st.Id,
                                                    ServiceTypeName = st.Name,
                                                    Amount = a.Amount,
                                                    Quantity = a.Quantity ?? 1
                                                }).ToList();
                    var ytdAppQuery = (from appb in db.AppBudgets
                                       where appb.Id == this.AppBudgetId
                                       from appbs in appb.AppBudgetServices
                                       join sra in db.viewSubreportAmounts on appbs.Id equals sra.AppBudgetServiceId
                                       join mr in db.MainReports on sra.MainReportId equals mr.Id
                                       where mr.Start <= this.Start
                                       select new YtdServicesQueryRow
                                       {
                                           ServiceId = appbs.ServiceId,
                                           ServiceTypeId = appbs.Service.TypeId,
                                           ReportingMethodId = appbs.Service.ReportingMethodId,
                                           Amount = sra.Amount,
                                           AppMatchBal = (decimal?)sra.MatchingSum,
                                           Quantity = sra.Quantity
                                       });

                     var ytdTotalReportedAppQuery = (from a in
                                                    (from sra in ytdAppQuery
                                                     group sra by sra.ServiceTypeId into srag
                                                     select new
                                                     {
                                                         ServiceTypeId = srag.Key,
                                                         Amount = srag.Sum(f => f.Amount),
                                                         AppMatchBal = srag.Sum(f => f.AppMatchBal),
                                                         Quantity = srag.Sum(f => f.Quantity)
                                                     })
                                                 join st in db.ServiceTypes on a.ServiceTypeId equals st.Id
                                                 select new
                                                 {
                                                     ServiceTypeId = st.Id,
                                                     ServiceTypeName = st.Name,
                                                     Amount = a.Amount,
                                                     AppMatchBal = a.AppMatchBal,
                                                     Quantity = a.Quantity ?? 1
                                                 }).ToList();
                    //HcYtdPercentage = ytdServiceTypesQuery.Where(f => f.ServiceTypeId == (int)Service.ServiceTypes.Homecare).Sum(f => f.Amount) / total,
                    // CcExp = CcExp + ytdServicesQuery.y

                    decimal? total = ytdServiceTypesQuery.Sum(f => f.Amount);
                    decimal? totalApp = ytdTotalReportedAppQuery.Sum(f => f.Amount);
                    decimal? totalAppMatchBal = ytdTotalReportedAppQuery.Sum(f => f.AppMatchBal);

                    if (total == 0) total = null;
                    if (totalAppMatchBal == 0) totalAppMatchBal = null;

                    this.Totals = new TotalsClass
                    {
                        AppAmount = this.AppAmount,
                        AppMatch = this.AppMatch,
                        CurrencyId = this.CurrencyId,
                        AgencySubTotals = q.ToList(),
                        HcYtdPercentage = ytdServiceTypesQuery.Where(f => f.ServiceTypeId == (int)Service.ServiceTypes.Homecare).Sum(f => f.Amount) / total,
                        AoYtdPercentage = ytdServiceTypesQuery.Where(f => f.ServiceTypeId == (int)Service.ServiceTypes.AdministrativeOverhead).Sum(f => f.Amount) / total,
                        TotalReportedApp = totalApp,
                        TotalReportedAppMatchBalance = totalAppMatchBal,

                    };
                    this.ReimbursementCosts = GetAvgReimrusement(ytdServicesQuery);
                }
                
             


				if (this.PoApprovalModel != null || this.PaApprovalModel != null)
                {
                    var appbudgetServices = from ab in db.AppBudgetServices.Where(this.Permissions.AppBudgetServicesFilter)
                                            join mr in db.MainReports on ab.AppBudgetId equals mr.AppBudgetId
                                            where mr.Id == this.Id
                                            select ab;
                    var totalReported = (from a in appbudgetServices
                                         join v in db.viewSubreportAmounts on a.Id equals v.AppBudgetServiceId
                                         join mr in db.MainReports.Where(this.Permissions.MainReportsFilter).Where(MainReport.Submitted) on v.MainReportId equals mr.Id
                                         group v by a.AppBudgetId into vg
                                         select vg.Sum(f => f.Amount)
                                        ).SingleOrDefault();
                    if(this.PaApprovalModel != null)
                    {
                        this.PaApprovalModel.CancellationAmount = this.AppAmount - totalReported;
                        this.PaApprovalModel.CurrencyId = this.CurrencyId;
                        this.PaApprovalModel.LastReport = this.LastReport;
                    }
                    else
                    {
                        this.PoApprovalModel.CancellationAmount = this.AppAmount - totalReported;
                        this.PoApprovalModel.CurrencyId = this.CurrencyId;
                        this.PoApprovalModel.LastReport = this.LastReport;
                    }
                }
			}
		}
		private AvgReimbursementCostClass GetAvgReimrusement(IQueryable<YtdServicesQueryRow> ytdServicesQuery)
		{
			decimal ytdReimbursementCostAmount = 0;
			decimal? ytdReimbursementCostQuantity = 1;
			var q = ytdServicesQuery.Where(f =>
					f.ReportingMethodId == (int)Service.ReportingMethods.Homecare
					|| f.ReportingMethodId == (int)Service.ReportingMethods.HomecareWeekly
				);
			var servicesToExclude = System.Web.Configuration.WebConfigurationManager.AppSettings["ExcludeFromAvgYtdReimbursementCost"].ParseCsvIntegers();
			if (servicesToExclude.Any())
			{
				q = q.Where(f => !servicesToExclude.Contains(f.ServiceId));
			}

			var sumQuery = from a in q
						   group a by 1 into g
						   select new
						   {
							   Amount = g.Sum(f => f.Amount),
							   Quantity = g.Sum(f => f.Quantity),
							   Count = g.Count()
						   };
			var item = sumQuery.FirstOrDefault();
			if (item != null && item.Count > 0)
			{
				ytdReimbursementCostAmount = item.Amount;
				ytdReimbursementCostQuantity = item.Quantity;
			}

			return new AvgReimbursementCostClass
			{
				AvgReimbursementCost = this.AppAvgReimbursementCost,
				AvgYtdReimbursementCost = ytdReimbursementCostAmount / (ytdReimbursementCostQuantity > 0 ? ytdReimbursementCostQuantity : 1)
			};
		}
		private class YtdServicesQueryRow
		{
			public int ServiceId { get; internal set; }
			public int ServiceTypeId { get; internal set; }
			public int ReportingMethodId { get; internal set; }
			public decimal Amount { get; internal set; }
			public decimal? Quantity { get; internal set; }
            public decimal? AppMatchBal { get; internal set; }
            
          }



		public IEnumerable<SubServiceRowModes> Rows { get; set; }
		public bool CanAddAgencyRemarks { get; set; }
		public bool CanAddPoRemarks { get; set; }
		public bool StatusEditable { get; set; }
		public bool CanBeDeleted { get; set; }
		public bool CanBeEdited { get { return MainReport.EditableStatuses.Contains((MainReport.Statuses)this.StatusId); } }
		public TotalsClass Totals { get; set; }
		public AvgReimbursementCostClass ReimbursementCosts { get; set; }




		public class TotalsClass
		{
			[Display(Name = "Total Reported on this App (including this report)")]
            public decimal? YtdCcExp { get { return this.AgencySubTotals.Sum(f => f.YtdCcExp); } }
            
            public decimal? YtdMatch { get { return this.AgencySubTotals.Sum(f => f.YtdMatchExp); } }
			public decimal? CcExp { get { return this.AgencySubTotals.Sum(f => f.CcExp); } }
           // public decimal? CcExpFE { get { return this.AgencySubTotals.Sum(f => f.CcExpFE); } }
            public decimal? CcGrant { get { return this.AgencySubTotals.Sum(f => f.CcGrant); } }
			public decimal? AppAmount { get; set; }

			[Display(Name = "App Balance")]
            //public decimal? AppBalance { get { return this.AppAmount - this.YtdCcExp; } }
            public decimal? AppBalance { get { return this.AppAmount - this.TotalReportedApp; } }

            [Display(Name = "Required Match Balance")]
			public decimal? AppMatchBalance
			{
				get
				{
                    //		var amount = this.AppMatch - this.YtdMatch;
                  
                    var amount = this.AppMatch - this.TotalReportedAppMatchBalance ;
                    if (amount < 0)
						amount = 0;
					return amount;
				}
			}

			public string CurrencyId { get; set; }

			public IEnumerable<AgnencyTotalsClass> AgencySubTotals { get; set; }

			public decimal? AppMatch { get; set; }
            public decimal? TotalReportedApp { get; set; }
            public decimal? TotalReportedAppMatchBalance { get; set; }

            [UIHint("Percentage")]
			[Display(Name = "YTD Home care percentage")]
			public decimal? HcYtdPercentage { get; set; }

			[UIHint("Percentage")]
			[Display(Name = "YTD Administrative Overhead percentage")]
			public decimal? AoYtdPercentage { get; set; }

			[UIHint("Percentage")]
			[Display(Name = "YTD Supplemental Percentage")]
			public decimal? OtherYtdPercentage { get { return 1 - this.HcYtdPercentage - this.AoYtdPercentage; } }
		}

		public class AvgReimbursementCostClass
		{
			[Display(Name = "Allocation Letter Average HC Cost")]
			public decimal? AvgReimbursementCost { get; set; }

			[Display(Name = "Reported Average YTD HC Cost")]
			public decimal? AvgYtdReimbursementCost { get; set; }
		}

		public class AgnencyTotalsClass
		{

			public int AgencyId { get; set; }

			public decimal? CcExp { get; set; }

            //public decimal? CcExpFE { get; set; }

            public decimal? YtdCcExp { get; set; }

			public decimal? CcGrant { get; set; }

			public string CurrencyId { get; set; }

			public string AgencyName { get; set; }

			public decimal? YtdMatchExp { get; set; }
		}
	}


}