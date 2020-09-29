using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CC.Web.Models
{
    public class AgencyReportingBudgetModel : jQueryDataTableParamModel
    {
        [Display(Name = "SER")]
        public int? SelectedAgencyGroupId { get; set; }
        [Display(Name = "Year")]
        public int? SelectedYear { get; set; }
        [Display(Name = "Fund")]
        public int? SelectedFundId { get; set; }

        public IQueryable<AgencyReportingBudgetRow> GetAgencyReportingData(CC.Data.ccEntities db, CC.Data.Services.IPermissionsBase permissions)
        {
            var q = from appb in db.AppBudgets.Where(permissions.AppBudgetsFilter)
                    join appbs in db.AppBudgetServices.Where(permissions.AppBudgetServicesFilter) on appb.Id equals appbs.AppBudgetId
                    let app = appb.App
                    let agency = appbs.Agency
                    let agencygroup = appbs.Agency.AgencyGroup
                    let service = appbs.Service
                    let serviceType = appbs.Service.ServiceType
                    select new AgencyReportingBudgetRow
                    {
                        AgencyGroupId = agencygroup.Id,
                        FundId = app.FundId,
                        AgencyGroupName = agencygroup.Name + ", " + app.Fund.Name + ", " + app.Name,
                        Year = System.Data.Objects.SqlClient.SqlFunctions.DatePart("year", appb.App.StartDate),
                        AgencyName = agency.Name,
                        ServiceTypeName = serviceType.Name,
                        ServiceName = service.Name,
                        CcGrant = appbs.CcGrant,
                        Cur = app.CurrencyId,
                        BudgetStatusName =
                            appb.StatusId == (int)CC.Data.AppBudgetApprovalStatuses.Approved ? "Approved" :
                            appb.StatusId == (int)CC.Data.AppBudgetApprovalStatuses.AwaitingGlobalPoApproval ? "Awaiting Global Program Officer Approval" :
                            appb.StatusId == (int)CC.Data.AppBudgetApprovalStatuses.AwaitingRegionalPoApproval ? "Awaiting Regional Program Officer Approval" :
                            appb.StatusId == (int)CC.Data.AppBudgetApprovalStatuses.Cancelled ? "Cancelled" :
                            appb.StatusId == (int)CC.Data.AppBudgetApprovalStatuses.New ? "New" :
                            appb.StatusId == (int)CC.Data.AppBudgetApprovalStatuses.Rejected ? "Rejected" :
                            appb.StatusId == (int)CC.Data.AppBudgetApprovalStatuses.ReturnedToAgency ? "Returned to Agency" : null
                    };
            return q;
        }

        public IQueryable<AgencyReportingBudgetRow> ApplyFilter(IQueryable<AgencyReportingBudgetRow> q)
        {
            var filtered = from item in q
                           where SelectedAgencyGroupId == null || item.AgencyGroupId == SelectedAgencyGroupId
                           where SelectedYear == null || item.Year == SelectedYear
                           where SelectedFundId == null || item.FundId == SelectedFundId
                           select item;
            return filtered;
        }


        internal IOrderedQueryable<AgencyReportingBudgetRow> ApplySort(IQueryable<AgencyReportingBudgetRow> filtered)
        {
            IOrderedQueryable<AgencyReportingBudgetRow> sorted;
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

    public class AgencyReportingBudgetRow
    {
        [Display(Name = "Ser ID")]
        public int AgencyGroupId { get; set; }

        [System.ComponentModel.DataAnnotations.ScaffoldColumn(false)]
        public int FundId { get; set; }

        [Display(Name = "SER")]
        public string AgencyGroupName { get; set; }

        [Display(Name = "Year")]
        public int? Year { get; set; }

        [Display(Name = "Agency")]
        public string AgencyName { get; set; }

        [Display(Name = "Service Type")]
        public string ServiceTypeName { get; set; }

        [Display(Name = "Service")]
        public string ServiceName { get; set; }

        [Display(Name = "Budget")]
        public decimal CcGrant { get; set; }

        [Display(Name = "CUR")]
        public string Cur { get; set; }

        [Display(Name = "Budget Status")]
        public string BudgetStatusName { get; set; }
    }
}