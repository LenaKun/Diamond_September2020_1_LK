using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using CC.Data;

namespace CC.Web.Models
{

	interface IFinancialSummaryModel
	{
		void LoadSelectLists(CC.Data.ccEntities db, CC.Data.Services.IPermissionsBase permissions);
		CC.Web.Models.jQueryDataTableResult GetJqResult();
	}

	public class FinancialSummaryOverviewModel : OverViewFilter
	{

		public override string ActionName
		{
			get
			{
				return "Overview";
			}

		}

		public Models.jQueryDataTableResult GetJqResult(CC.Data.ccEntities db, CC.Data.Services.IPermissionsBase permissions)
		{
			var source = Data(db, permissions);
			return new CC.Web.Models.jQueryDataTableResult
			{
				aaData = source.Take(this.iDisplayLength),
				iTotalDisplayRecords = source.Count() > this.iDisplayLength ? this.iDisplayStart + this.iDisplayLength + 1 : this.iDisplayStart + 1,
				sEcho = this.sEcho
			};
		}
		public IEnumerable<OverViewDataRow> LinqData(CC.Data.ccEntities db, CC.Data.Services.IPermissionsBase permissions)
		{
			var source = this.Filter(db, permissions);
			var g1 = from a in source
					 group a by new
					 {
						 a.AgencyId,
						 a.ServiceId,
						 a.MasterFundId,
						 a.AppId
					 } into g
					 select new
					 {
						 g.Key.AgencyId,
						 g.Key.ServiceId,
						 g.Key.MasterFundId,
						 g.Key.AppId,
						 Amount = g.Sum(f => f.Amount)
					 };
			var g1ex = from a in source
					   let ex = db.viewAppExchangeRates.Where(f => f.AppId == a.AppId && f.ToCur == this.CurId).Select(f => f.Value).FirstOrDefault()
					   select new
					   {
						   a.AgencyId,
						   a.ServiceId,
						   a.MasterFundId,
						   a.AppId,
						   CcGrant = ex * (from t in source
								   where t.AgencyId == a.AgencyId
								   where t.ServiceId == a.ServiceId
								   where t.AppId == a.AppId
								   select new { t.ServiceId, t.AppId, t.AgencyId, t.CcGrant })
								   .Distinct().Sum(f=>f.CcGrant),
						   Amount = a.Amount * ex,
					   };
			var g2 = from a in g1ex
					 group a by new
					 {
						 a.AgencyId,
						 a.ServiceId,
						 a.MasterFundId,
					 } into g
					 select new
					 {
						 g.Key.AgencyId,
						 g.Key.ServiceId,
						 g.Key.MasterFundId,
						 CcGrant = g.Sum(f=>f.CcGrant),
						 Amount = g.Sum(f => f.Amount)
					 };
			var q = from f in g2
					join s in db.Services on f.ServiceId equals s.Id
					join a in db.Agencies on f.AgencyId equals a.Id
					join mf in db.MasterFunds on f.MasterFundId equals mf.Id
					select new OverViewDataRow
					{
						AgencyId = f.AgencyId,
						AgencyName = a.Name,
						ServiceId = f.ServiceId,
						ServiceName = s.Name,
						ServiceTypeName = s.ServiceType.Name,
						Amount = f.Amount ?? 0,
						ClientsCount = (from t in source
										where t.AgencyId == f.AgencyId
										where t.ServiceId == f.ServiceId
										where t.MasterFundId == f.MasterFundId
										select t.ClientId).Distinct().Count(),
						UniqueClientsCount = (from t in source
											  where t.AgencyId == f.AgencyId
											  where t.ServiceId == f.ServiceId
											  where t.MasterFundId == f.MasterFundId
											  select t.MasterId).Distinct().Count(),
						CurId = this.CurId,
						FundsCount = (from t in source
									  where t.AgencyId == f.AgencyId
									  where t.ServiceId == f.ServiceId
									  where t.MasterFundId == f.MasterFundId
									  select t.FundId).Distinct().Count(),
						MasterFundName = mf.Name,
						CcGrant = f.CcGrant,
					};
			if (this.iDisplayLength != int.MaxValue)
			{
				q=q
					.OrderBy(f => f.AgencyId)
					.Skip(this.iDisplayStart).Take(this.iDisplayLength + 1);
			}
			return q.ToList();
		}
		public IEnumerable<OverViewDataRow> Data(CC.Data.ccEntities db, CC.Data.Services.IPermissionsBase permissions)
		{
			return this.SpData(db, permissions);
		}
		public IEnumerable<OverViewDataRow> SpData(CC.Data.ccEntities db, CC.Data.Services.IPermissionsBase permissions)
		{
			var data = db.spFinancialSummaryOverview(this.CurId, this.StartDate, this.EndDate
				, !this.IncludeNotSubmittedReports
				, !this.HideEstimatedAmounts
				, this.AgencyId
				, this.RegionId
				, this.CountryId
				, this.StateId
				, this.ServiceTypeId
				, this.ServiceId
				, this.MasterFundId
				, this.FundId
				, this.AppId
				, this.ClientId
				, this.sSearch
				, this.sSortCol_0
				, this.sSortDir_0 == "asc"
				, this.iDisplayLength == int.MaxValue ? int.MaxValue : this.iDisplayLength + 1
				, this.iDisplayStart
				, permissions.User.Id);

			return data.Select(f => new OverViewDataRow
			{
				AgencyId = f.AgencyId,
				AgencyName = f.AgencyName,
				ServiceId = f.ServiceId,
				ServiceName = f.ServiceName,
				ServiceTypeName = f.ServiceTypeName,
				Amount = f.Amount ?? 0,
				ClientsCount = f.ClientsCount,
				UniqueClientsCount = f.UniqueClientsCount,
				AvgAmountPerMaster = f.AverageCostPerUnduplicatedClient ?? 0,
				CurId = this.CurId,
				FundsCount = f.FundsCount ?? 0,
				MasterFundName = f.MasterFundName,
				CcGrant = f.CcGrant,
			}).ToList();
		}

		public class OverViewDataRow
		{
			[Display(Name = "Org ID")]
			public int AgencyId { get; set; }

			[ScaffoldColumn(false)]
			public int ServiceTypeId { get; set; }

			[ScaffoldColumn(false)]
			public int? RegionId { get; set; }
			[ScaffoldColumn(false)]
			public int? CountryId { get; set; }
			[ScaffoldColumn(false)]
			public int? StateId { get; set; }

			[ScaffoldColumn(false)]
			public int ServiceId { get; set; }

			[ScaffoldColumn(false)]
			public int? AppId { get; set; }

			[ScaffoldColumn(false)]
			public string AppName { get; set; }

			[Display(Name = "Agency")]
			public string AgencyName { get; set; }

			[Display(Name = "Service Type")]
			public string ServiceTypeName { get; set; }

			[Display(Name = "Service")]
			public string ServiceName { get; set; }

			[Display(Name = "Clients")]
			public int? ClientsCount { get; set; }

			[Display(Name = "Unique Clients")]
			public int? UniqueClientsCount { get; set; }

			[Display(Name = "Average Cost Per Unduplicated  Client")]
			public decimal? AvgAmountPerMaster { get; set; }

			[Display(Name = "Funds")]
			public int FundsCount { get; set; }

			[Display(Name = "Master Fund")]
			public string MasterFundName { get; set; }

			[Display(Name = "Amount")]
			public decimal? Amount { get; set; }

			[Display(Name = "CUR")]
			public string CurId { get; set; }

			public decimal? CcGrant { get; set; }
		}

	}
	public class FinancialSummaryIndexModel : OverViewFilter
	{
		public override string ActionName
		{
			get
			{
				return "Index";
			}
		}



		public Models.jQueryDataTableResult GetJqResult(CC.Data.ccEntities db, CC.Data.Services.IPermissionsBase permissions)
		{
			var source = Data(db, permissions);
			return new CC.Web.Models.jQueryDataTableResult
			{
				aaData = source.Take(this.iDisplayLength),
				iTotalDisplayRecords = source.Count() > this.iDisplayLength ? this.iDisplayStart + this.iDisplayLength + 1 : this.iDisplayStart + 1,
				sEcho = this.sEcho
			};

		}

		public IEnumerable<OverViewDataRow> LinqData(CC.Data.ccEntities db, CC.Data.Services.IPermissionsBase permissions)
		{
			var source = this.Filter(db, permissions);

			var q = from a in source
					group a by new
					{
						a.AppId,
						a.ServiceId,
					} into g
					select new
					{
						g.Key.AppId,
						g.Key.ServiceId,
						Quantity = g.Sum(f => f.Quantity),
						Amount = g.Sum(f => f.Amount),
						ClientsCount = g.Select(f => f.ClientId).Distinct().Count(),
						CcGrant = g.Select(f => new
						{
							f.AppId,
							f.ServiceId,
							f.CcGrant
						}).Distinct().Sum(f => f.CcGrant)
					};
			var r = from f in q
					join b in db.viewAppExchangeRates on new { AppId = f.AppId, ToCur = this.CurId }
					equals new { AppId = b.AppId, ToCur = b.ToCur }
					join s in db.Services on f.ServiceId equals s.Id
					join a in db.Apps on f.AppId equals a.Id
					select new OverViewDataRow
					{
						ServiceTypeName = s.ServiceType.Name,
						ServiceName = s.Name,
						FundName = a.Fund.Name,
						AppName = a.Name,
						Quantity = f.Quantity,
						Amount = b.Value * f.Amount,
						ClientsCount = f.ClientsCount,
						AverageCostPerClient = b.Value * f.Amount / (f.ClientsCount == 0 ? (int?)null : f.ClientsCount),
						AverageCostPerUnit = b.Value * f.Amount / (f.Quantity == 0 ? (int?)null : f.Quantity),
						MasterFundName = a.Fund.MasterFund.Name,
						CcGrant = b.Value * f.CcGrant,
						Cur = this.CurId,
						ServiceId = f.ServiceId,
						AppId = f.AppId
					};
			if (this.iDisplayLength != int.MaxValue)
			{
				r = r
					.OrderBy(f => f.AppId)
					.Skip(this.iDisplayStart).Take(this.iDisplayLength + 1);
			}
			return r.ToList();
		}
		public IEnumerable<OverViewDataRow> Data(CC.Data.ccEntities db, CC.Data.Services.IPermissionsBase permissions)
		{
			return SpData(db, permissions);
		}
		public IEnumerable<OverViewDataRow> SpData(CC.Data.ccEntities db, CC.Data.Services.IPermissionsBase permissions)
		{
			var data = db.spFinancialSummarySummary(this.CurId, this.StartDate, this.EndDate
				, !this.IncludeNotSubmittedReports
				, !this.HideEstimatedAmounts
				, this.AgencyId
				, this.RegionId
				, this.CountryId
				, this.StateId
				, this.ServiceTypeId
				, this.ServiceId
				, this.MasterFundId
				, this.FundId
				, this.AppId
				, this.ClientId
				, this.sSearch
				, this.sSortCol_0
				, this.sSortDir_0 == "asc"
				, this.iDisplayLength == int.MaxValue ? int.MaxValue : this.iDisplayLength + 1
				, this.iDisplayStart
				, permissions.User.Id);
			return data.Select(f => new OverViewDataRow
			{
				ServiceTypeName = f.ServiceType,
				ServiceName = f.Service,
				FundName = f.Fund,
				AppName = f.App,
				Quantity = f.Quantity,
				Amount = f.Amount,
				ClientsCount = f.ClientsCount,
				AverageCostPerClient = f.AverageCostPerClient,
				AverageCostPerUnit = f.AverageCostPerUnit,
				MasterFundName = f.MasterFund,
				CcGrant = f.CcGrant,
				Cur = this.CurId,
				ServiceId = f.ServiceId,
				AppId = f.AppId
			}).ToList();
		}

		public class OverViewDataRow
		{
			[Display(Name = "Service Type")]
			public string ServiceTypeName { get; set; }

			[Display(Name = "Service")]
			public string ServiceName { get; set; }

			[Display(Name = "Quantity")]
			public decimal? Quantity { get; set; }

			[Display(Name = "Amount")]
			public decimal? Amount { get; set; }

			[Display(Name = "Clients")]
			public int? ClientsCount { get; set; }

			[Display(Name = "Average Cost Per Client")]
			public decimal? AverageCostPerClient { get; set; }

			[Display(Name = "Average Cost Per Unit")]
			public decimal? AverageCostPerUnit { get; set; }

			[Display(Name = "CUR")]
			public string Cur { get; set; }

			[Display(Name = "Fund")]
			public string FundName { get; set; }

			[Display(Name = "Master Fund")]
			public string MasterFundName { get; set; }

			[Display(Name = "App")]
			public string AppName { get; set; }

			[Display(Name = "CC Grant")]
			public decimal? CcGrant { get; set; }

			[ScaffoldColumn(false)]
			public int ServiceId { get; set; }

			[ScaffoldColumn(false)]
			public int AppId { get; set; }
		}
	}
	public class FinancialSummaryDetailsModel : OverViewFilter
	{
		public override string ActionName
		{
			get
			{
				return "Details";
			}
		}

		public Models.jQueryDataTableResult GetJqResult(CC.Data.ccEntities db, CC.Data.Services.IPermissionsBase permissions)
		{
			var source = Data(db, permissions);

			return new CC.Web.Models.jQueryDataTableResult
			{
				aaData = source.Take(this.iDisplayLength),
				iTotalDisplayRecords = source.Count() > this.iDisplayLength ? this.iDisplayStart + this.iDisplayLength + 1 : this.iDisplayStart + 1,
				sEcho = this.sEcho
			};
		}



		public IEnumerable<OverViewDataRow> Data(CC.Data.ccEntities db, CC.Data.Services.IPermissionsBase permissions)
		{
			var data = db.spFinancialSummaryDetails(this.CurId, this.StartDate, this.EndDate
				, !this.IncludeNotSubmittedReports
				, !this.HideEstimatedAmounts
				, this.AgencyId
				, this.RegionId
				, this.CountryId
				, this.StateId
				, this.ServiceTypeId
				, this.ServiceId
				, this.MasterFundId
				, this.FundId
				, this.AppId
				, this.ClientId
				, this.sSearch
				, this.sSortCol_0
				, this.sSortDir_0 == "asc"
				, this.iDisplayLength == int.MaxValue ? int.MaxValue : this.iDisplayLength + 1
				, this.iDisplayStart
				, permissions.User.Id);
			return data.Select(f => new OverViewDataRow
			{
				SubReportId = f.SubReportId,
				FirstName = f.FirstName,
				LastName = f.LastName,
				ClientId = f.ClientId,
				AgencyId = f.AgencyId,
				ServiceTypeName = f.ServiceTypeName,
				ServiceName = f.ServiceName,
				ReportStart = f.ReportStart,
				ReportEnd = f.ReportEnd,
				FundName = f.FundName,
				AppName = f.AppName,
				Quantity = f.Quantity,
				Amount = f.Amount,
				Cur = this.CurId,
				IsEstimated = f.IsEstimated ?? false,
                SubReportId1 = f.SubReportId
                
            }).ToList();
		}

		public class OverViewDataRow
		{
			[Display(Name = "First Name")]
			public string FirstName { get; set; }

			[Display(Name = "Last Name")]
			public string LastName { get; set; }

			[Display(Name = "CC ID")]
			public int ClientId { get; set; }

			[Display(Name = "ORG_ID")]
			public int? AgencyId { get; set; }

			[Display(Name = "Service Type")]
			public string ServiceTypeName { get; set; }

			[Display(Name = "Service")]
			public string ServiceName { get; set; }

			[Display(Name = "Report Start")]
			public DateTime ReportStart { get; set; }

			[Display(Name = "Report End")]
			public DateTime ReportEnd { get; set; }

			[Display(Name = "Fund")]
			public string FundName { get; set; }

			[Display(Name = "App")]
			public string AppName { get; set; }

			[Display(Name = "Grant Amount")]
			public decimal AppAmount { get; set; }

			[Display(Name = "Quantity")]
			public decimal? Quantity { get; set; }

			[Display(Name = "Amount")]
			public decimal? Amount { get; set; }

			[Display(Name = "Estimated Amount")]
			public bool IsEstimated { get; set; }

			[Display(Name = "Amount per Unit")]
			public decimal? AmountPerUnit
			{
				get
				{
					if (Quantity.HasValue && Quantity != 0)
						return Amount / Quantity;
					else
						return null;
				}
			}

			[Display(Name = "CUR")]
			public string Cur { get; set; }

			[NotMapped]
			public int SubReportId { get; set; }
            [NotMapped]
            public int SubReportId1 { get; set; }
        }
	}
	public abstract class OverViewFilter : CC.Web.Models.jQueryDataTableParamModel
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
		[Display(Name = "Region")]
		public int? RegionId { get; set; }
		[Display(Name = "Service Type")]
		public int? ServiceTypeId { get; set; }
		[Display(Name = "Service")]
		public int? ServiceId { get; set; }
		[Display(Name = "Master Fund")]
		public int? MasterFundId { get; set; }
		public SelectList MusterFundsList { get; set; }
		[Display(Name = "Fund")]
		public int? FundId { get; set; }
		public SelectList FundsList { get; set; }
		[Display(Name = "App")]
		public int? AppId { get; set; }
		public SelectList AppsList { get; set; }
		[Display(Name = "Show only clients with amounts applicable")]
		public bool HideEstimatedAmounts { get; set; }
		[Display(Name = "Include not submitted reports")]
		public bool IncludeNotSubmittedReports { get; set; }
		[Display(Name = "CC ID")]
		new public int? ClientId { get; set; }
		[Display(Name = "Country")]
		public int? CountryId { get; set; }
		[Display(Name = "State")]
		public int? StateId { get; set; }

		public abstract string ActionName { get; }
		public IQueryable<viewFinSumDet> Filter(CC.Data.ccEntities db, CC.Data.Services.IPermissionsBase permissions)
		{
			var source = db.viewFinSumDets.AsQueryable();
			switch ((FixedRoles)permissions.User.RoleId)
			{
				case FixedRoles.AgencyUser:
				case FixedRoles.AgencyUserAndReviewer:
					source = from a in source
							 where a.AgencyId == permissions.User.AgencyId
							 select a;
					break;
				case FixedRoles.RegionOfficer:
				case FixedRoles.RegionAssistant:
					var agencyGroups = db.Users.Where(f => f.Id == permissions.User.Id).Select(f => f.AgencyGroup.Id);
					source = from a in source
							 where agencyGroups.Contains(a.AgencyGroupId)
							 select a;
					break;
				case FixedRoles.Ser:
				case FixedRoles.SerAndReviewer:
					source = from a in source
							 where a.AgencyGroupId == permissions.User.AgencyGroupId
							 select a;
					break;
				case FixedRoles.BMF:
					source = from a in source
							 where a.MasterFundId == 73
							 where MainReport.SubmittedStatuses.Contains(a.MainReportStatusId)
							 select a;
					break;
				case FixedRoles.RegionReadOnly:
					source = from a in source
							 where a.RegionId == permissions.User.RegionId
							 select a;
					break;
			}
			if (this.StartDate.HasValue)
			{
				source = source.Where(f => f.ReportStart >= this.StartDate);
			}
			if (this.EndDate.HasValue)
			{
				source = source.Where(f => f.ReportStart < this.EndDate);
			}
			if (this.RegionId.HasValue)
			{
				source = source.Where(f => f.RegionId == this.RegionId);
			}
			if (this.CountryId.HasValue)
			{
				source = source.Where(f => f.CountryId == this.CountryId);
			}
			if (this.StateId.HasValue)
			{
				source = source.Where(f => f.StateId == this.StateId);
			}
			if (!this.IncludeNotSubmittedReports)
			{
				source = source.Where(f => MainReport.SubmittedStatuses.Contains(f.MainReportStatusId));
			}
			if (this.HideEstimatedAmounts)
			{
				source = source.Where(f => f.IsEstimated == false);
			}
			if (this.AgencyId.HasValue)
			{
				source = source.Where(f => f.AgencyId == this.AgencyId);
			}
			if (this.ServiceTypeId.HasValue)
			{
				source = source.Where(f => f.ServiceTypeId == this.ServiceTypeId);
			}
			if (this.ServiceId.HasValue)
			{
				source = source.Where(f => f.ServiceId == this.ServiceId);
			}
			if (this.MasterFundId.HasValue)
			{
				source = source.Where(f => f.MasterFundId == this.MasterFundId);
			}
			if (this.FundId.HasValue)
			{
				source = source.Where(f => f.FundId == this.FundId);
			}
			if (this.AppId.HasValue)
			{
				source = source.Where(f => f.AppId == this.AppId);
			}
			if (!string.IsNullOrEmpty(this.sSearch))
			{
				source = source.Where(f => f.FirstName.Contains(this.sSearch) || f.LastName.Contains(this.sSearch));
			}
			if (this.ClientId.HasValue)
			{
				source = source.Where(f => f.ClientId == this.ClientId);
			}
			return source;
		}

		public void Load(CC.Data.ccEntities db, CC.Data.Services.IPermissionsBase permissions)
		{
			this.Currencies = new SelectList(CC.Data.Currency.ConvertableCurrencies);
		}

	}

}