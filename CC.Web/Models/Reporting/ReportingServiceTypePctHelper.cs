using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using CC.Data;

namespace CC.Web.Models.Reporting
{
	public class ReportingServiceTypePctHelper
	{
		internal static IEnumerable<ReportingServiceTypePctRow> getData(CC.Data.ccEntities db, CC.Data.Services.IPermissionsBase Permissions, YtdReportinServiceTypePctFilter filter)
		{
			var MainReports = db.MainReports.Where(Permissions.MainReportsFilter);
			var AppBudgetServices = db.AppBudgetServices.Where(Permissions.AppBudgetServicesFilter);
			var viewSubreportAmounts = db.viewSubreportAmounts;
			var Apps = db.Apps.Where(Permissions.AppsFilter).Where(f => !f.AgencyGroup.ExcludeFromReports);

			if (filter.IgnoreUnsubmitted) MainReports = MainReports.Where(MainReport.Submitted);

			var q = from b in
						(from a in
							 (from mr in MainReports
							  join sra in viewSubreportAmounts on mr.Id equals sra.MainReportId
							  join abs in AppBudgetServices on sra.AppBudgetServiceId equals abs.Id
							  select new
							  {
								  MainReportId = sra.MainReportId,
								  TotalAmount = sra.Amount,
								  ServiceTypeId = abs.Service.TypeId,
								  Year = System.Data.Objects.SqlClient.SqlFunctions.DatePart("Year", mr.Start),
								  StatusId = mr.StatusId,
								  AppId = mr.AppBudget.AppId
							  })
						 group a by new { AppId = a.AppId, Year = a.Year } into ag
						 select new
						 {
							 AppId = ag.Key.AppId,
							 Year = ag.Key.Year,
							 ReportAmount = ag.Sum(f => f.TotalAmount),
							 HcReportAmount = ag.Sum(f => f.ServiceTypeId == (int)Service.ServiceTypes.Homecare ? (decimal?)f.TotalAmount : null),
							 AdminReportAmount = ag.Sum(f => f.ServiceTypeId == (int)Service.ServiceTypes.AdministrativeOverhead ? (decimal?)f.TotalAmount : null)
						 })
					join app in Apps on b.AppId equals app.Id
					join aex in db.viewAppExchangeRates on new { AppId = app.Id, CurId = filter.DisplayCurrency } equals new { AppId = aex.AppId, CurId = aex.ToCur } into aexg
					from aex in aexg.DefaultIfEmpty()
					where filter.AgencyGroupId == null || app.AgencyGroupId == filter.AgencyGroupId
					where filter.FundId == null || app.FundId == filter.FundId
					where filter.RegionId == null || app.AgencyGroup.Country.RegionId == filter.RegionId
					where filter.CountryId == null || app.AgencyGroup.CountryId == filter.CountryId
					where filter.StateId == null || app.AgencyGroup.StateId == filter.StateId
					where filter.Year == null || b.Year == filter.Year
					select new ReportingServiceTypePctRow
					{
						RegionName = app.AgencyGroup.Country.Region.Name,
						CountryCode = app.AgencyGroup.Country.Code,
						StateCode = app.AgencyGroup.State.Code,
						AgencyGroupName = app.AgencyGroup.Name,
                        AgencyGroupId = app.AgencyGroupId,
						FundName = app.Fund.Name,
						AppName = app.Name,
						AppAmount = app.CcGrant,
						AppCur = app.CurrencyId,
						ReportAmount = b.ReportAmount,
						HcReportAmount = b.HcReportAmount,
						AdminReportAmount = b.AdminReportAmount,
						DispCur = filter.DisplayCurrency,
						ExRate = aex.Value,
                        Errors = aex == null ? "No exchange rate is defined for the " + filter.DisplayCurrency + " in the app " + app.Name : ""
					};

			var l = q.ToList();

			NewMethod(l);

			return l;

		}
		internal static IEnumerable<QtrReportingServiceTypePctRow> getQtrData(CC.Data.ccEntities db, CC.Data.Services.IPermissionsBase Permissions, QtrReportinServiceTypePctFilter filter)
		{
			var MainReports = db.MainReports.Where(Permissions.MainReportsFilter);
			var AppBudgetServices = db.AppBudgetServices.Where(Permissions.AppBudgetServicesFilter);
			var viewSubreportAmounts = db.viewSubreportAmounts;
            var Apps = db.Apps.Where(Permissions.AppsFilter).Where(f => !f.AgencyGroup.ExcludeFromReports);

			if (filter.IgnoreUnsubmitted) MainReports = MainReports.Where(MainReport.Submitted);

			var q = from b in
						(from a in
							 (from mr in MainReports
							  join sra in viewSubreportAmounts on mr.Id equals sra.MainReportId
							  join abs in AppBudgetServices on sra.AppBudgetServiceId equals abs.Id
							  select new
							  {
								  MainReportId = sra.MainReportId,
								  TotalAmount = sra.Amount,
								  ServiceTypeId = abs.Service.TypeId,
								  Year = System.Data.Objects.SqlClient.SqlFunctions.DatePart("Year", mr.Start),
								  Quarter = System.Data.Objects.SqlClient.SqlFunctions.DatePart("Quarter", mr.Start),
								  StatusId = mr.StatusId,
								  AppId = mr.AppBudget.AppId
							  })
						 group a by new { AppId = a.AppId, Year = a.Year, Quarter = a.Quarter } into ag
						 select new
						 {
							 AppId = ag.Key.AppId,
							 Year = ag.Key.Year,
							 Quarter = ag.Key.Quarter,
							 ReportAmount = ag.Sum(f => f.TotalAmount),
							 HcReportAmount = ag.Sum(f => f.ServiceTypeId == (int)Service.ServiceTypes.Homecare ? (decimal?)f.TotalAmount : null),
							 AdminReportAmount = ag.Sum(f => f.ServiceTypeId == (int)Service.ServiceTypes.AdministrativeOverhead ? (decimal?)f.TotalAmount : null)
						 })
					join app in Apps on b.AppId equals app.Id
					join aex in db.viewAppExchangeRates on new { AppId = app.Id, CurId = filter.DisplayCurrency } equals new { AppId = aex.AppId, CurId = aex.ToCur } into aexg
					from aex in aexg.DefaultIfEmpty()
					where filter.FundId == null || app.FundId == filter.FundId
					where filter.AgencyGroupId == null || app.AgencyGroupId == filter.AgencyGroupId
					where filter.RegionId == null || app.AgencyGroup.Country.RegionId == filter.RegionId
					where filter.CountryId == null || app.AgencyGroup.CountryId == filter.CountryId
					where filter.StateId == null || app.AgencyGroup.StateId == filter.StateId
					where filter.From == null || (b.Year >= filter.FromYear && b.Quarter >= filter.FromQuarter)
					where filter.To == null || (b.Year <= filter.ToYear && b.Quarter <= filter.ToQuarter)
					select new QtrReportingServiceTypePctRow
					{
						RegionName = app.AgencyGroup.Country.Region.Name,
						CountryCode = app.AgencyGroup.Country.Code,
						StateCode = app.AgencyGroup.State.Code,
						AgencyGroupName = app.AgencyGroup.Name,
                        AgencyGroupId = app.AgencyGroupId,
						FundName = app.Fund.Name,
						AppName = app.Name,
						AppAmount = app.CcGrant,
						AppCur = app.CurrencyId,
						ReportAmount = b.ReportAmount,
						HcReportAmount = b.HcReportAmount,
						AdminReportAmount = b.AdminReportAmount,
						DispCur = filter.DisplayCurrency,
						ExRate = aex.Value,
						Year = b.Year,
						Quarter = b.Quarter,
                        Errors = aex == null ? "No exchange rate is defined for the " + filter.DisplayCurrency + " in the app " + app.Name : ""
					};

			var l = q.ToList();

			NewMethod(l);

			return l;
		}

		/// <summary>
		/// Calculates converted amounts and adds a totals row
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="l"></param>
		private static void NewMethod<T>(List<T> l) where T : ReportingServiceTypePctRow, new()
		{
			var totalsRow = new T
			{
				AgencyGroupName = "Totals",
				AppAmountDispCur = 0,
				ReportAmountDisp = 0,
				AdminAmountDisp = 0,
				OtherAmountDisp = 0,
				HomeCareAmountDisp = 0,
			};
			foreach (var r in l)
			{
				r.AppAmountDispCur = r.AppAmount * r.ExRate;
				r.ReportAmountDisp = r.ReportAmount * r.ExRate;
				r.AdminAmountDisp = r.ExRate * r.AdminReportAmount;
				r.HomeCareAmountDisp = r.HcReportAmount * r.ExRate;
				r.OtherAmountDisp = r.OtherReportAmount * r.ExRate;

				totalsRow.AppAmountDispCur += (r.AppAmountDispCur ?? 0);
				totalsRow.ReportAmountDisp += (r.ReportAmountDisp ?? 0);
				totalsRow.AdminAmountDisp += (r.AdminAmountDisp ?? 0);
				totalsRow.OtherAmountDisp += (r.OtherAmountDisp ?? 0);
				totalsRow.HomeCareAmountDisp += (r.HomeCareAmountDisp ?? 0);
			}

			l.Add(totalsRow);
		}
	}

}