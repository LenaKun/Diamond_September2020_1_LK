using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CC.Web.Models
{
	public class AgencyReportingIndexModel : jQueryDataTableParamModel
	{
        [Display(Name = "Region")]
        public int? SelectedRegionId { get; set; }
		[Display(Name = "SER")]
		public int? SelectedAgencyGroupId { get; set; }
		[Display(Name = "Year")]
		public int? SelectedYear { get; set; }
		[Display(Name = "Fund")]
		public int? SelectedFundId { get; set; }

		public IQueryable<AgencyReportingRow> GetAgencyReportingData(CC.Data.ccEntities db, CC.Data.Services.IPermissionsBase permissions)
		{
			var q = from mr in db.MainReports.Where(permissions.MainReportsFilter)
					join sr in db.SubReports.Where(permissions.SubReportsFilter) on mr.Id equals sr.MainReportId
					join sra in db.viewSubreportAmounts on sr.Id equals sra.id
					let app = mr.AppBudget.App
					let appbs = sr.AppBudgetService
					let agency = sr.AppBudgetService.Agency
					let agencygroup = mr.AppBudget.App.AgencyGroup
					let service = sr.AppBudgetService.Service
					let serviceType = sr.AppBudgetService.Service.ServiceType
					select new AgencyReportingRow
					{
                        RegionId = agencygroup.Country.RegionId,
						AgencyGroupId = agencygroup.Id,
						FundId = app.FundId,
						AgencyGroupName = agencygroup.Name + ", " + app.Fund.Name + ", " + app.Name,
						Year = System.Data.Objects.SqlClient.SqlFunctions.DatePart("year", mr.Start),
						Quarter = System.Data.Objects.SqlClient.SqlFunctions.DatePart("quarter", mr.Start),
						AgencyName = agency.Name,
						ServiceTypeName = serviceType.Name,
						ServiceName = service.Name,
						CcGrant = appbs.CcGrant,
						CcExp = sra.Amount,
						Cur = app.CurrencyId,
						ReportStatusName = 
							mr.StatusId == (int)CC.Data.MainReport.Statuses.Approved ? "Approved" :
							mr.StatusId == (int)CC.Data.MainReport.Statuses.AwaitingProgramAssistantApproval ? "Awaiting PA Approval" :
							mr.StatusId == (int)CC.Data.MainReport.Statuses.AwaitingProgramOfficerApproval ? "Awaiting PO Approval" :
							mr.StatusId == (int)CC.Data.MainReport.Statuses.AwaitingAgencyResponse ? "Awaiting Agency Response" :
							mr.StatusId == (int)CC.Data.MainReport.Statuses.Cancelled ? "Cancelled" :
							mr.StatusId == (int)CC.Data.MainReport.Statuses.New ? "New" :
							mr.StatusId == (int)CC.Data.MainReport.Statuses.Rejected ? "Rejected":
							mr.StatusId == (int)CC.Data.MainReport.Statuses.ReturnedToAgency? "Returned To Agency" : null
					};
			return q;
		}
		public IQueryable<AgencyReportingRow> ApplyFilter(IQueryable<AgencyReportingRow> q)
		{
			var filtered = from item in q
                           where SelectedRegionId == null || SelectedRegionId == 0 || item.RegionId == SelectedRegionId
						   where SelectedAgencyGroupId == null || item.AgencyGroupId == SelectedAgencyGroupId
						   where SelectedYear == null || item.Year == SelectedYear
						   where SelectedFundId == null || item.FundId == SelectedFundId
						   select item;
			return filtered;
		}


		internal IOrderedQueryable<AgencyReportingRow> ApplySort(IQueryable<AgencyReportingRow> filtered)
		{
			IOrderedQueryable<AgencyReportingRow> sorted;
			if (!string.IsNullOrEmpty(sSortCol_0))
			{
				sorted = filtered.OrderByField(this.sSortCol_0, this.bSortDir_0);
			}
			else
			{
				sorted = filtered.OrderBy(f => f.AgencyGroupName);
			}
			return sorted;
		}
	}

	public class AgencyReportingRow
	{
		[System.ComponentModel.DataAnnotations.ScaffoldColumn(false)]
		public int AgencyGroupId { get; set; }

		[System.ComponentModel.DataAnnotations.ScaffoldColumn(false)]
		public int FundId { get; set; }

        [System.ComponentModel.DataAnnotations.ScaffoldColumn(false)]
        public int RegionId { get; set; }

		[Display(Name = "SER")]
		public string AgencyGroupName { get; set; }

		[Display(Name = "Year")]
		public int? Year { get; set; }

		[Display(Name = "Quarter")]
		public int? Quarter { get; set; }

		[Display(Name = "Agency")]
		public string AgencyName { get; set; }

		[Display(Name = "Service Type")]
		public string ServiceTypeName { get; set; }

		[Display(Name = "Service")]
		public string ServiceName { get; set; }

		[Display(Name = "Budget")]
		public decimal CcGrant { get; set; }

		[Display(Name = "CC Exp.")]
		public decimal? CcExp { get; set; }

		[Display(Name = "CUR")]
		public string Cur { get; set; }

		[Display(Name = "Report Status")]
		public string ReportStatusName { get; set; }
	}


}