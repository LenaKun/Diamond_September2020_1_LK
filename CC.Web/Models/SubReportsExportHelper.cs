using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using CC.Data;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace CC.Web.Models
{
	public class SubReportsExportHelper
	{
		public int subReportId;
		public int reportingMethodId;
		public CC.Data.Services.IPermissionsBase permissions;
		public CC.Data.ccEntities db;


		public IEnumerable<crxTotalCostWithListOfClientNames> TotalCostWithListOfClientNames()
		{
			var subReport = db.SubReports.Include(f => f.MainReport).Where(permissions.SubReportsFilter).Single(f => f.Id == subReportId);
			DateTime d1 = subReport.MainReport.Start;
			DateTime d2 = subReport.MainReport.End;
			int serType = subReport.AppBudgetService.Service.ServiceType.Id;
			var q = from c in db.Clients.Where(permissions.ClientsFilter)
					join cr in db.ClientReports.Where(permissions.ClientReportsFilter).Where(f => f.SubReportId == subReportId) on c.Id equals cr.ClientId into crj
					from cr in crj.DefaultIfEmpty()
					where
						(cr != null ||
							(
								c.JoinDate < subReport.MainReport.End &&

							//if client leave reason is deceased then 90 days should be added to the leave date
				//leave date must null or be greater than the report end date
								(c.LeaveDate == null || System.Data.Objects.EntityFunctions.AddDays(c.LeaveDate, c.DeceasedDate == null ? 0 : SubReport.DeceasedDaysOverhead) >= subReport.MainReport.Start)//==true
								&&
								(c.AgencyId == subReport.AppBudgetService.AgencyId && c.ApprovalStatusId != 1024)
							)
						)
					select new crxTotalCostWithListOfClientNames
					{
						FirstName = c.FirstName,
						LastName = c.LastName,
						ApprovalStatus = c.ApprovalStatus.Name,
						ClientId = c.Id,
						Reported = cr != null ? "true" : "false",
						Remarks = cr != null ? cr.Remarks : ""
					};
			return q;

		}
		public IEnumerable<crxClientUnit> ClientUnit()
		{
			var q = from cr in db.ClientReports.Where(permissions.ClientReportsFilter)
					where cr.SubReportId == subReportId
					select new crxClientUnit
					{
						FirstName = cr.Client.FirstName,
						LastName = cr.Client.LastName,
						ApprovalStatus = cr.Client.ApprovalStatus.Name,
						ClientId = cr.ClientId,
						Quantity = cr.Quantity,
						Remarks = cr.Remarks
					};
			return q;
		}
		public IEnumerable<crxClientUnitAmount> ClientUnitAmount()
		{
			var q = from cr in db.ClientReports.Where(permissions.ClientReportsFilter)
					where cr.SubReportId == subReportId
					select new crxClientUnitAmount
					{
						FirstName = cr.Client.FirstName,
						LastName = cr.Client.LastName,
						ApprovalStatus = cr.Client.ApprovalStatus.Name,
						ClientId = cr.ClientId,
						Quantity = cr.Quantity,
						Amount = cr.Amount,
						Remarks = cr.Remarks
					};
			return q;
		}
		public IEnumerable<crxClientNamesAndCosts> ClientNamesAndCosts()
		{
			var q = from cr in db.ClientReports.Where(permissions.ClientReportsFilter)
					where cr.SubReportId == subReportId
					select new crxClientNamesAndCosts
					{
						FirstName = cr.Client.FirstName,
						LastName = cr.Client.LastName,
						ApprovalStatus = cr.Client.ApprovalStatus.Name,
						Remarks = cr.Remarks,
                        HcCaps = db.HcCapsTableRaws
                                            .Where(cap => cap.ClientId == cr.ClientId)
                                            .Where(cap => cap.StartDate <= DateTime.Now)
                                            .Where(cap => cap.EndDate == null || cap.EndDate > DateTime.Now)
                                            .OrderByDescending(cap => cap.StartDate)
                                            .Select(cap => cap.HcCap)
                                            .FirstOrDefault(),
						Amount = cr.Amount,
						ClientId = cr.ClientId
					};
			return q;
		}
		public IEnumerable<crxMhmClientNamesAndCosts> MhmClientNamesAndCosts()
		{
			var q = from cr in db.ClientReports.Where(permissions.ClientReportsFilter)
					where cr.SubReportId == subReportId
					select new crxMhmClientNamesAndCosts
					{
						FirstName = cr.Client.FirstName,
						LastName = cr.Client.LastName,
						ApprovalStatus = cr.Client.ApprovalStatus.Name,
						Remarks = cr.Remarks,
						Amount = cr.Amount,
						ClientId = cr.ClientId
					};
			return q;
		}
		public IEnumerable<crxEmergency> Emergency()
		{
			var q = from cr in db.EmergencyReports.Where(permissions.EmergencyReportsFilter)
					where cr.SubReportId == subReportId
					select new crxEmergency
					{
						FirstName = cr.Client.FirstName,
						LastName = cr.Client.LastName,
						ApprovalStatus = cr.Client.ApprovalStatus.Name,
						ClientId = cr.ClientId,
						ReportDate = cr.ReportDate,
						Type = cr.EmergencyReportType.Name,
						Remarks = cr.Remarks,
						Amount = cr.Amount,
						Discretionary = cr.Discretionary,
						TotalAmount = cr.Total,
                        UniqueCircumstances = cr.UniqueCircumstances
					};
			return q;
		}

        public IEnumerable<crxSupportiveCommunities> SupportiveCommunities()
        {
			var subReport = db.SubReports.Include(f => f.MainReport).Where(permissions.SubReportsFilter).Single(f => f.Id == subReportId);
			DateTime d1 = subReport.MainReport.Start;
			DateTime d2 = subReport.MainReport.End;
			int serType = subReport.AppBudgetService.Service.ServiceType.Id;
			var q = from c in db.Clients.Where(permissions.ClientsFilter)
					where c.AgencyId == subReport.AppBudgetService.AgencyId && c.ApprovalStatusId != 1024

                    join sc in db.SupportiveCommunitiesReports.Where(f => f.SubReportId == subReportId) on c.Id equals sc.ClientId into scg
					from sc in scg.DefaultIfEmpty()
                    let eligible = db.viewScRepSources.Any(f => f.subreportid == subReportId && f.clientid == c.Id)
                    where sc != null || eligible
                    select new crxSupportiveCommunities
                    {
                        ClientId = c.Id,
                        NationalId = c.NationalId,
                        FirstName = c.FirstName,
                        LastName = c.LastName,
                        ApprovalStatus = c.ApprovalStatus.Name,
                        JoinDate = c.JoinDate,
                        HoursHoldCost = sc != null ? sc.HoursHoldCost : (decimal?)null,
                        TotalPaidByCC = sc != null ? sc.Amount : (decimal?)null,
                        MonthsCount = sc != null ? sc.MonthsCount : (int?)null,
						Reported = sc != null ? "True" : "False"
                    };
            return q;


        }


        public IEnumerable<crxDayCenters> DayCenters()
        {
            var q = from cr in db.DaysCentersReports.Where(permissions.DaysCentersReportsFilter)
                    where cr.SubReportId == subReportId
                    select new crxDayCenters
                    {
                        ClientId = cr.ClientId,
                        NationalId = cr.Client.NationalId,
                        FirstName = cr.Client.FirstName,
                        LastName = cr.Client.LastName,
                        ApprovalStatus = cr.Client.ApprovalStatus.Name,
                        JoinDate = cr.Client.JoinDate,
                        SubsideByDcc=cr.SubsidesByDcc,
                        VisitCost=cr.VisitCost,
                        VisitCount=cr.VisitsCount,
                        Amount=cr.Amount
                    };
            return q;
        }

		public IEnumerable<crxSoupKitchens> SoupKitchens()
		{
			var q = from cr in db.SoupKitchensReports.Where(permissions.SoupKitchensReportsFilter)
                   
                    where cr.SubReportId == subReportId 
					select new crxSoupKitchens
					{
						ClientId = cr.ClientId,
						NationalId = cr.Client.NationalId,
						FirstName = cr.Client.FirstName,
						LastName = cr.Client.LastName,
						ApprovalStatus = cr.Client.ApprovalStatus.Name,
						JoinDate = cr.Client.JoinDate,
						VisitCount = cr.SKMembersVisits.Count,
					};
			return q;
		}

		public IEnumerable<crxHomecare> Homecare()
		{
			var q = from cr in db.ClientReports.Where(permissions.ClientReportsFilter)
					where cr.SubReportId == subReportId
					from ar in cr.ClientAmountReports
					select new crxHomecare
					{
						FirstName = cr.Client.FirstName,
						LastName = cr.Client.LastName,
						ApprovalStatus = cr.Client.ApprovalStatus.Name,
						ClientId = cr.ClientId,
						Quantity = ar.Quantity,
						ReportDate = ar.ReportDate,
                        HcCaps = db.HcCapsMonthlyTableRaws
                                            .Where(cap => cap.ClientId == cr.ClientId)
                                            .Where(cap => cap.StartDate < cr.SubReport.MainReport.End)
                                            .Where(cap => cap.EndDate == null || cap.EndDate > cr.SubReport.MainReport.Start)
                                            .OrderByDescending(cap => cap.StartDate)
                                            .Select(cap => cap.HcCap)
                                            .FirstOrDefault(),
						Rate = cr.Rate,
						Remarks = cr.Remarks
					};
			return q;
		}

		public IEnumerable<crxHomecare> HomecareWeekly()
		{
			var q = from cr in db.ClientReports.Where(permissions.ClientReportsFilter)
				   where cr.SubReportId == subReportId
				   from ar in cr.ClientAmountReports
				   select new crxHomecare
				   {
					   FirstName = cr.Client.FirstName,
					   LastName = cr.Client.LastName,
					   ApprovalStatus = cr.Client.ApprovalStatus.Name,
					   ClientId = cr.ClientId,
					   Quantity = ar.Quantity,
					   ReportDate = ar.ReportDate,
					   HcCaps = db.HcCapsTableRaws
										   .Where(cap => cap.ClientId == cr.ClientId)
										   .Where(cap => cap.StartDate < cr.SubReport.MainReport.End)
										   .Where(cap => cap.EndDate == null || cap.EndDate > cr.SubReport.MainReport.Start)
										   .OrderByDescending(cap => cap.StartDate)
										   .Select(cap => cap.HcCap)
										   .FirstOrDefault(),
					   Rate = cr.Rate,
					   Remarks = cr.Remarks
				   };
			return q;
		}

		public IEnumerable<crxHomecareWeh> HomecareWeeklyWeh()
		{
			var q = from cr in db.ClientReports.Where(permissions.ClientReportsFilter)
					where cr.SubReportId == subReportId
					from ar in cr.ClientAmountReports
					select new crxHomecareWeh
					{
						FirstName = cr.Client.FirstName,
						LastName = cr.Client.LastName,
						ApprovalStatus = cr.Client.ApprovalStatus.Name,
						ClientId = cr.ClientId,
						Quantity = ar.Quantity,
						ReportDate = ar.ReportDate,
						GovHcCaps = db.HcCapsTableRaws
											.Where(cap => cap.ClientId == cr.ClientId)
											.Where(cap => cap.StartDate < cr.SubReport.MainReport.End)
											.Where(cap => cap.EndDate == null || cap.EndDate > cr.SubReport.MainReport.Start)
											.OrderByDescending(cap => cap.StartDate)
											.Select(cap => cap.GovHcHours)
											.FirstOrDefault(),
						Rate = cr.Rate,
						Remarks = cr.Remarks
					};
			return q;
		}

		public IEnumerable<crxClientEventsCount> ClientEventsCount()
		{
			var q = from cr in db.ClientEventsCountReports.Where(permissions.ClientEventsCountReportsFilter)
					where cr.SubReportId == subReportId
					select new crxClientEventsCount
					{
						EventDate = cr.EventDate,
						JNVCount = cr.JNVCount,
						TotalCount = cr.TotalCount,
						Remarks = cr.Remarks
					};
			return q;
		}

		#region rowSturcts
		public class crxBase
		{
			[Display(Name = "First Name", Order = 0)]
			public string FirstName { get; set; }
			[Display(Name = "Last Name", Order = 1)]
			public string LastName { get; set; }
			[Display(Name = "Approval Status", Order=2)]
			public string ApprovalStatus { get; set; }
			[Display(Name = "CC ID", Order = 3)]
			public int ClientId { get; set; }
			[Display(Name = "Unique Circumstances", Order = 11)]
			public virtual string Remarks { get; set; }
		}

		public class crxTotalCostWithListOfClientNames : crxBase
		{
			[Display(Name = "Reported", Order = 4)]
			public string Reported { get; set; }
		}

        public class crxSupportiveCommunities
        {
            [Display(Name = "CC ID", Order = 0)]
            public int ClientId { get; set; }

            [Display(Name = "Israel ID", Order = 1)]
            public string NationalId { get; set; }

            [Display(Name = "First Name", Order = 2)]
            public string FirstName { get; set; }
            [Display(Name = "Last Name", Order = 3)]
            public string LastName { get; set; }
            [Display(Name = "Approval Status", Order = 4)]
            public string ApprovalStatus { get; set; }
            [Display(Name = "Join Date", Order = 5)]
            public DateTime JoinDate { get; set; }
            
            [Display(Name = "HouseholdCost", Order = 6)]
            public decimal? HoursHoldCost { get; set; }

            [Display(Name = "TotalPaidByCC", Order = 7)]
            public decimal? TotalPaidByCC { get; set; }

            [Display(Name = "Months Reported", Order = 8)]
            public int? MonthsCount { get; set; }

			[Display(Name = "Reported", Order = 8)]
			public string Reported { get; set; }
        }

        public class crxDayCenters
        {
            [Display(Name = "CC ID", Order = 0)]
            public int ClientId { get; set; }

            [Display(Name = "Israel ID", Order = 1)]
            public string NationalId { get; set; }

            [Display(Name = "First Name", Order = 2)]
            public string FirstName { get; set; }
            [Display(Name = "Last Name", Order = 3)]
            public string LastName { get; set; }
            [Display(Name = "Approval Status", Order = 4)]
            public string ApprovalStatus { get; set; }
            [Display(Name = "Join Date", Order = 5)]
            public DateTime JoinDate { get; set; }

            [Display(Name = "SubsidyByDcc", Order = 6)]
            public decimal? SubsideByDcc { get; set; }

            [Display(Name = "VisitCost", Order = 7)]
            public decimal? VisitCost { get; set; }

            [Display(Name = "VisitCount", Order = 8)]
            public int? VisitCount { get; set; }

            [Display(Name = "Amount", Order = 9)]
            public decimal? Amount { get; set; }

        }

		public class crxSoupKitchens
		{
			[Display(Name = "CC ID", Order = 0)]
			public int ClientId { get; set; }

			[Display(Name = "Israel ID", Order = 1)]
			public string NationalId { get; set; }

			[Display(Name = "First Name", Order = 2)]
			public string FirstName { get; set; }
			[Display(Name = "Last Name", Order = 3)]
			public string LastName { get; set; }
			[Display(Name = "Approval Status", Order = 4)]
			public string ApprovalStatus { get; set; }
			[Display(Name = "Join Date", Order = 5)]
			public DateTime JoinDate { get; set; }

			[Display(Name = "Meal Count", Order = 6)]
			public int? VisitCount { get; set; }

		}

		public class crxClientUnit : crxBase
		{

			[Display(Name = "Quantity", Order = 4)]
			public decimal? Quantity { get; set; }
		}
		public class crxClientUnitAmount : crxClientUnit
		{
			[Display(Name = "Amount", Order = 5)]
			public virtual decimal? Amount { get; set; }
		}
		public class crxClientNamesAndCosts : crxBase
		{
            [Display(Name = "Allowed Hours/week", Order = 4)]
            public decimal? HcCaps { get; set; }
			[Display(Name = "Amount", Order = 5)]
			public decimal? Amount { get; set; }
		}
		public class crxMhmClientNamesAndCosts : crxBase
		{
			[Display(Name = "Amount", Order = 5)]
			public decimal? Amount { get; set; }
		}

		public class crxEmergency : crxBase
		{
			[Display(Name = "Report Date", Order = 5)]
			public DateTime ReportDate { get; set; }
			[Display(Name = "Type", Order = 5)]
			public string Type { get; set; }
			[Display(Name = "Purpose Of Grant", Order = 6)]
			public override string Remarks { get; set; }
			[Display(Name = "Amount", Order = 7)]
			public virtual decimal? Amount { get; set; }
			[Display(Name = "Discretionary", Order = 8)]
			public decimal? Discretionary { get; set; }
			[Display(Name = "Total", Order = 9)]
			public decimal? TotalAmount { get; set; }
            [Display(Name = "Unique Circumstances", Order = 10)]
            public string UniqueCircumstances { get; set; }
		}

		public class crxHomecareBase : crxClientUnitAmount
		{
			[Display(Name = "Rate", Order = 6)]
			public decimal? Rate { get; set; }
			[Display(Name = "Report Date")]
			public DateTime ReportDate { get; set; }
			[Display(Name = "Amount")]
			public override decimal? Amount
			{
				get
				{
					return this.Rate * this.Quantity;
				}
				set
				{
					throw new NotImplementedException();
				}
			}
		}

		public class crxHomecare : crxHomecareBase
		{
			[Display(Name = "Allowed Hours/week", Order = 4)]
			public decimal? HcCaps { get; set; }
		}

		public class crxHomecareWeh : crxHomecareBase
		{
			[Display(Name = "Allowed Govt Hours/week", Order = 4)]
			public decimal? GovHcCaps { get; set; }
		}

		public class crxClientEventsCount
		{
			[Display(Name = "Date of Event", Order = 0)]
			public DateTime EventDate { get; set; }

			[Display(Name = "Count of JNV attending", Order = 1)]
			public int JNVCount { get; set; }

			[Display(Name = "Count of Total Attendees", Order = 2)]
			public int? TotalCount { get; set; }
			[Display(Name = "Remarks", Order = 3)]
			public string Remarks { get; set; }

		}
		#endregion
	}
}