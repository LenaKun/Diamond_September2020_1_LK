using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CC.Web.Models
{
    public class AgencyReportingSocializationClientsCount : jQueryDataTableParamModel
    {
        [Display(Name = "Region")]
        public int? SelectedRegionId { get; set; }
        [Display(Name = "Country")]
        public int? SelectedCountryId { get; set; }
        [Display(Name = "State")]
        public int? SelectedStateId { get; set; }
        [Display(Name = "Master Fund")]
        public int? SelectedMasterFundId { get; set; }
        [Display(Name = "Fund")]
        public int? SelectedFundId { get; set; }
        [Display(Name = "SER")]
        public int? SelectedAgencyGroupId { get; set; }
        [Display(Name = "Year")]
        public int? SelectedYear { get; set; }        

        public IQueryable<SocializationClientsCountRow> GetAgencyReportingData(CC.Data.ccEntities db, CC.Data.Services.IPermissionsBase permissions)
        {
            var q = from mr in db.MainReports.Where(permissions.MainReportsFilter)
                    join sr in db.SubReports.Where(permissions.SubReportsFilter) on mr.Id equals sr.MainReportId
                    join cec in db.ClientEventsCountReports on sr.Id equals cec.SubReportId
                    select new SocializationClientsCountRow
                    {
                        RegionId = sr.AppBudgetService.Agency.AgencyGroup.Country.RegionId,
                        CountryId = sr.AppBudgetService.Agency.AgencyGroup.CountryId,
                        StateId = sr.AppBudgetService.Agency.AgencyGroup.StateId,
                        MasterFundId = mr.AppBudget.App.Fund.MasterFundId,
                        FundId = mr.AppBudget.App.FundId,
                        AgencyGroupId = sr.AppBudgetService.Agency.GroupId,
                        Year = System.Data.Objects.SqlClient.SqlFunctions.DatePart("year", mr.Start),
                        AgencyGroupName = sr.AppBudgetService.Agency.AgencyGroup.Name,
                        AgencyName = sr.AppBudgetService.Agency.Name,
                        EventDate = cec.EventDate,
                        JNVCount = cec.JNVCount,
                        TotalCount = cec.TotalCount,
                        Remarks = cec.Remarks
                    };
            return q;
        }
        public IQueryable<SocializationClientsCountRow> ApplyFilter(IQueryable<SocializationClientsCountRow> q)
        {
            var filtered = from item in q
                           where SelectedRegionId == null || SelectedRegionId == 0 || item.RegionId == SelectedRegionId
                           where SelectedCountryId == null || item.CountryId == SelectedCountryId
                           where SelectedStateId == null || item.StateId == SelectedStateId
                           where SelectedAgencyGroupId == null || item.AgencyGroupId == SelectedAgencyGroupId
                           where SelectedYear == null || item.Year == SelectedYear
                           where SelectedMasterFundId == null || item.MasterFundId == SelectedMasterFundId
                           where SelectedFundId == null || item.FundId == SelectedFundId
                           select item;
            return filtered;
        }


        internal IOrderedQueryable<SocializationClientsCountRow> ApplySort(IQueryable<SocializationClientsCountRow> filtered)
        {
            IOrderedQueryable<SocializationClientsCountRow> sorted;
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

    public class SocializationClientsCountRow
    {
        [System.ComponentModel.DataAnnotations.ScaffoldColumn(false)]
        public int AgencyGroupId { get; set; }

        [System.ComponentModel.DataAnnotations.ScaffoldColumn(false)]
        public int FundId { get; set; }

        [System.ComponentModel.DataAnnotations.ScaffoldColumn(false)]
        public int MasterFundId { get; set; }

        [System.ComponentModel.DataAnnotations.ScaffoldColumn(false)]
        public int RegionId { get; set; }

        [System.ComponentModel.DataAnnotations.ScaffoldColumn(false)]
        public int CountryId { get; set; }

        [System.ComponentModel.DataAnnotations.ScaffoldColumn(false)]
        public int? StateId { get; set; }

        [Display(Name = "SER")]
        public string AgencyGroupName { get; set; }

        [Display(Name = "Year")]
        public int? Year { get; set; }

        [Display(Name = "Agency")]
        public string AgencyName { get; set; }

        [Display(Name = "Date of Event")]
        public DateTime EventDate { get; set; }

        [Display(Name = "Count of JNV attending")]
        public int JNVCount { get; set; }

        [Display(Name = "Count of Total Attendees")]
        public int? TotalCount { get; set; }

        [Display(Name = "Remarks")]
        public string Remarks { get; set; }
    }
}