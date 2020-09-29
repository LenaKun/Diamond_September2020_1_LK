using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CC.Web.Models
{
    public class BudgetSummaryModel : BudgetSummaryFilter
    {
        public Models.jQueryDataTableResult GetJqResult(CC.Data.ccEntities db, CC.Data.Services.IPermissionsBase permissions)
        {
            var source = Data(db, permissions);
            var filtered = Filter(source);
            var sorted = filtered.OrderByField(this.sSortCol_0, this.bSortDir_0);
            return new CC.Web.Models.jQueryDataTableResult
            {
                aaData = sorted.Skip(this.iDisplayStart).Take(this.iDisplayLength).ToList(),
                iTotalDisplayRecords = filtered.Count(),
                iTotalRecords = source.Count(),
                sEcho = this.sEcho
            };
        }

        public IQueryable<BudgetSummaryDataRow> Filter(IQueryable<BudgetSummaryDataRow> source)
        {
            var filtered = source;
            if (this.MasterFundId.HasValue)
            {
                filtered  = filtered.Where(f => f.MasterFundId == this.MasterFundId);
            }
            if (this.ServiceTypeId.HasValue)
            {
                filtered = filtered.Where(f => f.ServiceTypeId == this.ServiceTypeId);
            }
            if (this.ServiceId.HasValue)
            {
                filtered = filtered.Where(f => f.ServiceId == this.ServiceId);
            }
            return filtered;
        }

        public IQueryable<BudgetSummaryDataRow> Data(CC.Data.ccEntities db, CC.Data.Services.IPermissionsBase permissions)
        {
            var raw =

                from abs in db.AppBudgetServices
                join aex in db.viewAppExchangeRates on new { appid = abs.AppBudget.AppId, CurId = this.CurId } equals new { appid = aex.AppId, CurId = aex.ToCur } into aexG
                from aex in aexG.DefaultIfEmpty()
                select new
                {                    
                    AgencyId = abs.AgencyId,
                    ServiceId = abs.ServiceId,
                    ServiceTypeId = abs.Service.ServiceType.Id,
                    serviceName = abs.Service.Name,
                    serviceType = abs.Service.ServiceType.Name,
                    FundId = abs.AppBudget.App.FundId,
                    AppExRate = aex.Value,
                    Start = abs.AppBudget.App.StartDate,
                    End = abs.AppBudget.App.EndDate,
                    Amount = abs.CcGrant,
                    AppId = abs.AppBudget.AppId,
                    MasterFundId = abs.AppBudget.App.Fund.MasterFundId
                };

            #region filter
            if (this.AgencyId.HasValue)
            {
                raw = raw.Where(f => f.AgencyId == AgencyId);
            }
            if (this.ServiceTypeId.HasValue)
            {
                raw = raw.Where(f => f.ServiceTypeId == this.ServiceTypeId);
            }
            if (this.ServiceId.HasValue)
            {
                raw = raw.Where(f => f.ServiceId == this.ServiceId);
            }
            if (this.StartDate.HasValue)
            {
                raw = raw.Where(f => f.Start >= this.StartDate);
            }
            if (this.EndDate.HasValue)
            {
                raw = raw.Where(f => f.End <= this.EndDate);
            }            
            if (this.FundId.HasValue)
            {
                raw = raw.Where(f => f.FundId == this.FundId);
            }
            if (this.AppId.HasValue)
            {
                raw = raw.Where(f => f.AppId == this.AppId);
            }
            #endregion

            var source = from item in raw
                         join agency in db.Agencies.Where(permissions.AgencyFilter) on item.AgencyId equals agency.Id
                         join fund in db.Funds.Where(permissions.FundsFilter) on item.FundId equals fund.Id
                         join app in db.Apps.Where(permissions.AppsFilter) on item.AppId equals app.Id
                         select new BudgetSummaryDataRow
                         {
                             AgencyName = agency.Name,
                             ServiceName = item.serviceName,
                             ServiceId = item.ServiceId,
                             ServiceTypeId = item.ServiceTypeId,
                             ServiceTypeName = item.serviceType,
                             AppName = app.Name,
                             Fund = fund.Name,
                             CCGrant = item.Amount * item.AppExRate ?? 0,
                             CurrencyCode = this.CurId,
                             MasterFundId = item.MasterFundId
                         };

            return source;
        }

        public class BudgetSummaryDataRow
        {
            [Display(Name = "Agency")]
            public string AgencyName { get; set; }

            [Display(Name = "Service Type")]
            public string ServiceTypeName { get; set; }

            [Display(Name = "Service")]
            public string ServiceName { get; set; }

            [Display(Name = "App #")]
            public string AppName { get; set; }

            [Display(Name = "Fund")]
            public string Fund { get; set; }

            [Display(Name = "Amount")]
            public decimal CCGrant { get; set; }

            [Display(Name = "CUR")]
            public string CurrencyCode { get; set; }

            [ScaffoldColumn(false)]
            public int MasterFundId { get; set; }

            [ScaffoldColumn(false)]
            public int ServiceId { get; set; }

            [ScaffoldColumn(false)]
            public int ServiceTypeId { get; set; }
        }        
    }

    public abstract class BudgetSummaryFilter : CC.Web.Models.jQueryDataTableParamModel
    {
        [Required]
        [Display(Name = "CUR")]
        public string CurId { get; set; }
        public SelectList Currencies { get; set; }
        [Display(Name = "Start Date")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        public DateTime? StartDate { get; set; }
        [Display(Name = "End Date")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        public DateTime? EndDate { get; set; }
        [Display(Name = "Agency")]
        public int? AgencyId { get; set; }
        public SelectList AgenciesList { get; set; }
        [Display(Name = "Service Type")]
        public int? ServiceTypeId { get; set; }
        public SelectList ServiceTypesList { get; set; }
        [Display(Name = "Service")]
        public int? ServiceId { get; set; }
        public SelectList ServicesList { get; set; }
        [Display(Name = "Master Fund")]
        public int? MasterFundId { get; set; }
        public SelectList MusterFundsList { get; set; }
        [Display(Name = "Fund")]
        public int? FundId { get; set; }
        public SelectList FundsList { get; set; }
        [Display(Name = "App")]
        public int? AppId { get; set; }
        public SelectList AppsList { get; set; }

        public void Load(CC.Data.ccEntities db, CC.Data.Services.IPermissionsBase permissions)
        {
            this.AgenciesList = new SelectList(
                db.Agencies.Where(permissions.AgencyFilter)
                .Select(f => new { Id = f.Id, Name = f.Name })
                .OrderBy(f => f.Name), "id", "name");
            this.AppsList = new SelectList(
                db.Apps.Where(permissions.AppsFilter)
                .Select(f => new { Id = f.Id, Name = f.Name })
                .OrderBy(f => f.Name), "id", "name");
            this.MusterFundsList = new SelectList(
                (from a in db.Apps.Where(permissions.AppsFilter)
                 group a by a.Fund.MasterFund into g
                 select g.Key)
                .Select(f => new { Id = f.Id, Name = f.Name })
                .OrderBy(f => f.Name), "id", "name");
            this.FundsList = new SelectList(
                (from a in db.Apps.Where(permissions.AppsFilter)
                 group a by a.Fund into g
                 select g.Key)
                .Select(f => new { Id = f.Id, Name = f.Name })
                .OrderBy(f => f.Name), "id", "name");
            this.ServicesList = new SelectList(
                (from ag in db.AgencyGroups.Where(permissions.AgencyGroupsFilter)
                 from app in ag.Apps
                 from s in app.Services
                 select s).Distinct()
                .Select(f => new { Id = f.Id, Name = f.Name }).Distinct()
                .OrderBy(f => f.Name), "id", "Name");
            this.ServiceTypesList = new SelectList(
                (from ag in db.AgencyGroups.Where(permissions.AgencyGroupsFilter)
                 from app in ag.Apps
                 from s in app.Services
                 select s.ServiceType)
                .Select(f => new { Id = f.Id, Name = f.Name }).Distinct()
                .OrderBy(f => f.Name), "id", "name");
            this.Currencies = new SelectList(CC.Data.Currency.ConvertableCurrencies);

        }
    }
}