using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CC.Data;

namespace CC.Web.Models.UniqueClientReports
{
	public class UniqueClientsFilter
	{
		public UniqueClientsFilter() { }

		[Display(Name = "Region")]
		public int[] Regions { get; set; }

		[Display(Name = "Country")]
		public int[] Countries { get; set; }

		[Display(Name = "State/Province")]
		public int[] States { get; set; }

		[Display(Name = "SER")]
		public int[] Sers { get; set; }

		[Display(Name = "Service Type")]
		public int[] ServiceTypes { get; set; }

		[Display(Name = "Service")]
		public int[] Services { get; set; }

		[Display(Name = "Funds")]
		public int[] Funds { get; set; }

		[Display(Name = "Year")]
		public int[] Years { get; set; }

		[Display(Name = "Quarter")]
		public int[] Quarters { get; set; }

		public ConsolidationLevels? ConsolidationLevel { get; set; }
	}

	public class UniqueClientsReportsHelper
	{
		public static IQueryable<UcrSourceRecord> select1(UniqueClientsFilter filter, ccEntities db)
		{
			var seed = from item in
						   (from cr in db.ViewClientReports
							join sr in db.SubReports on cr.SubReportId equals sr.Id
							join c in db.Clients on cr.ClientId equals c.Id
							where !c.Agency.AgencyGroup.ExcludeFromReports
							select new
							{
								SerId = c.Agency.GroupId,
								StateId = c.StateId,
								CountryId = c.Agency.AgencyGroup.CountryId,
								RegionId = c.Agency.AgencyGroup.Country.RegionId,
								ServiceId = sr.AppBudgetService.ServiceId,
								ServiceTypeId = sr.AppBudgetService.Service.TypeId,
								FundId = sr.AppBudgetService.AppBudget.App.FundId,
								clientid = c.Id,
								masterid = c.MasterId,
								date = cr.ReportDate ?? sr.MainReport.Start,

							})
					   select new UcrSourceRecord
					   {
						   SerId = item.SerId,
						   StateId = item.StateId,
						   CountryId = item.CountryId,
						   RegionId = item.RegionId,
						   ServiceId = item.ServiceId,
						   ServiceTypeId = item.ServiceTypeId,
						   FundId = item.FundId,
						   clientid = item.clientid,
						   masterid = item.masterid,
						   Year = System.Data.Objects.SqlClient.SqlFunctions.DatePart("year", item.date) ?? 0,
						   Quarter = System.Data.Objects.SqlClient.SqlFunctions.DatePart("quarter", item.date) ?? 0
					   };
			if (filter.Sers.Any())
			{
				seed = seed.Where(f => filter.Sers.Contains(f.SerId));
			}
			if (filter.States.Any())
			{
				seed = seed.Where(f => filter.States.Contains(f.StateId ?? 0));
			}
			if (filter.Countries.Any())
			{
				seed = seed.Where(f => filter.Countries.Contains(f.CountryId));
			}
			if (filter.Regions.Any())
			{
				seed = seed.Where(f => filter.Regions.Contains(f.RegionId));
			}
			if (filter.Services.Any())
			{
				seed = seed.Where(f => filter.Services.Contains(f.ServiceId));
			}
			if (filter.ServiceTypes.Any())
			{
				seed = seed.Where(f => filter.ServiceTypes.Contains(f.ServiceTypeId));
			}
			if (filter.Funds != null && filter.Funds.Any())
			{
				seed = seed.Where(f => filter.Funds.Contains(f.FundId));
			}
			if (filter.Years != null && filter.Years.Any())
			{
				seed = seed.Where(f => filter.Years.Contains(f.Year));
			}
			if (filter.Quarters != null && filter.Quarters.Any())
			{
				seed = seed.Where(f => filter.Quarters.Contains(f.Quarter));
			}
			return seed;
		}
		public static IEnumerable<T> AppendTotals<T>(IEnumerable<T> data, IQueryable<UcrSourceRecord> source) where T : UcrRowBase, new()
		{
			var count = source.Select(f => f.masterid ?? f.clientid).Distinct().Count();
			var totals = new T() { Count = count };
			foreach (var item in data)
			{
				yield return item;
			}
			yield return totals;

		}
		#region consolidation level methods - quarterly
		public static IEnumerable<UcrqRowSer> ucrqSer(IQueryable<UcrSourceRecord> source, ccEntities db)
		{
			var grouped = source.GroupBy(f => new
			{
				f.SerId,
				f.Year,
				f.Quarter
			});

			var result = from item in grouped
						 join ser in db.AgencyGroups.Where(f => !f.ExcludeFromReports) on item.Key.SerId equals ser.Id
						 orderby ser.Name, item.Key.Year, item.Key.Quarter
						 select new UcrqRowSer
						 {
							 AgencyGroupName = ser.Name,
                             AgencyGroupId = ser.Id,
							 Year = item.Key.Year,
							 Quarter = item.Key.Quarter,
							 Count = item.Select(f => f.masterid ?? f.clientid).Distinct().Count()
						 };
			return AppendTotals(result, source);
		}
		public static IEnumerable<UcrqRowState> ucrqState(IQueryable<UcrSourceRecord> source, ccEntities db)
		{
			var grouped = source.GroupBy(f => new
			{
				f.StateId,
				f.Year,
				f.Quarter
			});

			var result = from item in grouped
						 join state in db.States on item.Key.StateId equals state.Id into g
						 from state in g.DefaultIfEmpty()
						 orderby state.Name, item.Key.Year, item.Key.Quarter
						 select new UcrqRowState
						 {
							 StateName = state.Name ?? "N/A",
							 Year = item.Key.Year,
							 Quarter = item.Key.Quarter,
							 Count = item.Select(f => f.masterid ?? f.clientid).Distinct().Count()
						 };
			return AppendTotals(result, source);
		}
		public static IEnumerable<UcrqRowCountry> ucrqCountry(IQueryable<UcrSourceRecord> source, ccEntities db)
		{
			var grouped = source.GroupBy(f => new
			{
				f.CountryId,
				f.Year,
				f.Quarter
			});

			var result = from item in grouped
						 join country in db.Countries on item.Key.CountryId equals country.Id into g
						 from country in g.DefaultIfEmpty()
						 orderby country.Name, item.Key.Year, item.Key.Quarter
						 select new UcrqRowCountry
						 {
							 CountryName = country.Name,
							 Year = item.Key.Year,
							 Quarter = item.Key.Quarter,
							 Count = item.Select(f => f.masterid ?? f.clientid).Distinct().Count()
						 };
			return AppendTotals(result, source);
		}
		public static IEnumerable<UcrqRowRegion> ucrqRegion(IQueryable<UcrSourceRecord> source, ccEntities db)
		{
			var grouped = source.GroupBy(f => new
			{
				f.RegionId,
				f.Year,
				f.Quarter
			});

			var result = from item in grouped
						 join region in db.Regions on item.Key.RegionId equals region.Id into g
						 from region in g.DefaultIfEmpty()
						 orderby region.Name, item.Key.Year, item.Key.Quarter
						 select new UcrqRowRegion
						 {
							 RegionName = region.Name,
							 Year = item.Key.Year,
							 Quarter = item.Key.Quarter,
							 Count = item.Select(f => f.masterid ?? f.clientid).Distinct().Count()
						 };
			return AppendTotals(result, source);
		}
		public static IEnumerable<UcrqRowService> ucrqService(IQueryable<UcrSourceRecord> source, ccEntities db)
		{
			var grouped = source.GroupBy(f => new
			{
				f.ServiceId,
				f.Year,
				f.Quarter
			});

			var result = from item in grouped
						 join service in db.Services on item.Key.ServiceId equals service.Id into g
						 from service in g.DefaultIfEmpty()
						 orderby service.Name, item.Key.Year, item.Key.Quarter
						 select new UcrqRowService
						 {
							 ServiceName = service.Name,
							 Year = item.Key.Year,
							 Quarter = item.Key.Quarter,
							 Count = item.Select(f => f.masterid ?? f.clientid).Distinct().Count()
						 };
			return AppendTotals(result, source);
		}
		public static IEnumerable<UcrqRowServiceType> ucrqServiceType(IQueryable<UcrSourceRecord> source, ccEntities db)
		{
			var grouped = source.GroupBy(f => new
			{
				f.ServiceTypeId,
				f.Year,
				f.Quarter
			});

			var result = from item in grouped
						 join serviceType in db.ServiceTypes on item.Key.ServiceTypeId equals serviceType.Id into g
						 from serviceType in g.DefaultIfEmpty()
						 orderby serviceType.Name, item.Key.Year, item.Key.Quarter
						 select new UcrqRowServiceType
						 {
							 ServiceTypeName = serviceType.Name,
							 Year = item.Key.Year,
							 Quarter = item.Key.Quarter,
							 Count = item.Select(f => f.masterid ?? f.clientid).Distinct().Count()
						 };
			return AppendTotals(result, source);
		}
		public static IEnumerable<UcrqRowFund> ucrqFund(IQueryable<UcrSourceRecord> source, ccEntities db)
		{
			var grouped = source.GroupBy(f => new
			{
				f.FundId,
				f.Year,
				f.Quarter
			});

			var result = from item in grouped
						 join fund in db.Funds on item.Key.FundId equals fund.Id into g
						 from fund in g.DefaultIfEmpty()
						 orderby fund.Name, item.Key.Year, item.Key.Quarter
						 select new UcrqRowFund
						 {
							 FundName = fund.Name,
							 Year = item.Key.Year,
							 Quarter = item.Key.Quarter,
							 Count = item.Select(f => f.masterid ?? f.clientid).Distinct().Count()
						 };
			return AppendTotals(result, source);
		}
		public static IEnumerable<UcrqRowFull> ucrqFull(IQueryable<UcrSourceRecord> source, ccEntities db)
		{
			var grouped = source.GroupBy(f => new
			{
				f.SerId,
				f.StateId,
				f.CountryId,
				f.RegionId,
				f.ServiceId,
				f.ServiceTypeId,
				f.FundId,
				f.Year,
				f.Quarter
			});

			var result = from item in grouped
						 join ser in db.AgencyGroups.Where(f => !f.ExcludeFromReports) on item.Key.SerId equals ser.Id into serGroup
						 from ser in serGroup.DefaultIfEmpty()
						 join state in db.States on item.Key.RegionId equals state.Id into stateGroup
						 from state in stateGroup.DefaultIfEmpty()
						 join country in db.Countries on item.Key.RegionId equals country.Id into countryGroup
						 from country in countryGroup.DefaultIfEmpty()
						 join region in db.Regions on item.Key.RegionId equals region.Id into regionGroup
						 from region in regionGroup.DefaultIfEmpty()
						 join service in db.Services on item.Key.RegionId equals service.Id into serviceGroup
						 from service in serviceGroup.DefaultIfEmpty()
						 join serviceType in db.ServiceTypes on item.Key.RegionId equals serviceType.Id into serviceTypeGroup
						 from serviceType in serviceTypeGroup.DefaultIfEmpty()
						 join fund in db.Funds on item.Key.RegionId equals fund.Id into fundGroup
						 from fund in fundGroup.DefaultIfEmpty()
						 select new UcrqRowFull
						 {
							 AgencyGroupName = ser.Name,
                             AgencyGroupId = ser.Id,
							 StateName = state.Name ?? "N/A",
							 CountryName = country.Name,
							 RegionName = region.Name,
							 ServiceName = service.Name,
							 ServiceTypeName = serviceType.Name,
							 FundName = fund.Name,
							 Year = item.Key.Year,
							 Quarter = item.Key.Quarter,
							 Count = item.Select(f => f.masterid ?? f.clientid).Distinct().Count()
						 };
			return AppendTotals(result, source);
		}
		#endregion

		#region consolidation level methods - annualy (cp from above)
		public static IEnumerable<UcrRowSer> ucrSer(IQueryable<UcrSourceRecord> source, ccEntities db)
		{
			var grouped = source.GroupBy(f => new
			{
				f.SerId,
				f.Year,
			});

			var result = from item in grouped
						 join ser in db.AgencyGroups.Where(f => !f.ExcludeFromReports) on item.Key.SerId equals ser.Id
						 orderby ser.Name, item.Key.Year
						 select new UcrRowSer
						 {
							 AgencyGroupName = ser.Name,
                             AgencyGroupId = ser.Id,
							 Year = item.Key.Year,
							 Count = item.Select(f => f.masterid ?? f.clientid).Distinct().Count()
						 };
			return AppendTotals(result, source);
		}
		public static IEnumerable<UcrRowState> ucrState(IQueryable<UcrSourceRecord> source, ccEntities db)
		{
			var grouped = source.GroupBy(f => new
			{
				f.StateId,
				f.Year,
			});

			var result = from item in grouped
						 join state in db.States on item.Key.StateId equals state.Id into g
						 from state in g.DefaultIfEmpty()
						 orderby state.Name, item.Key.Year
						 select new UcrRowState
						 {
							 StateName = state.Name ?? "N/A",
							 Year = item.Key.Year,
							 Count = item.Select(f => f.masterid ?? f.clientid).Distinct().Count()
						 };
			return AppendTotals(result, source);
		}
		public static IEnumerable<UcrRowCountry> ucrCountry(IQueryable<UcrSourceRecord> source, ccEntities db)
		{
			var grouped = source.GroupBy(f => new
			{
				f.CountryId,
				f.Year,
			});

			var result = from item in grouped
						 join country in db.Countries on item.Key.CountryId equals country.Id into g
						 from country in g.DefaultIfEmpty()
						 orderby country.Name, item.Key.Year
						 select new UcrRowCountry
						 {
							 CountryName = country.Name,
							 Year = item.Key.Year,
							 Count = item.Select(f => f.masterid ?? f.clientid).Distinct().Count()
						 };
			return AppendTotals(result, source);
		}
		public static IEnumerable<UcrRowRegion> ucrRegion(IQueryable<UcrSourceRecord> source, ccEntities db)
		{
			var grouped = source.GroupBy(f => new
			{
				f.RegionId,
				f.Year,
			});

			var result = from item in grouped
						 join region in db.Regions on item.Key.RegionId equals region.Id into g
						 from region in g.DefaultIfEmpty()
						 orderby region.Name, item.Key.Year
						 select new UcrRowRegion
						 {
							 RegionName = region.Name,
							 Year = item.Key.Year,
							 Count = item.Select(f => f.masterid ?? f.clientid).Distinct().Count()
						 };
			return AppendTotals(result, source);
		}
		public static IEnumerable<UcrRowService> ucrService(IQueryable<UcrSourceRecord> source, ccEntities db)
		{
			var grouped = source.GroupBy(f => new
			{
				f.ServiceId,
				f.Year,
			});

			var result = from item in grouped
						 join service in db.Services on item.Key.ServiceId equals service.Id into g
						 from service in g.DefaultIfEmpty()
						 orderby service.Name, item.Key.Year
						 select new UcrRowService
						 {
							 ServiceName = service.Name,
							 Year = item.Key.Year,
							 Count = item.Select(f => f.masterid ?? f.clientid).Distinct().Count()
						 };
			return AppendTotals(result, source);
		}
		public static IEnumerable<UcrRowServiceType> ucrServiceType(IQueryable<UcrSourceRecord> source, ccEntities db)
		{
			var grouped = source.GroupBy(f => new
			{
				f.ServiceTypeId,
				f.Year,
			});

			var result = from item in grouped
						 join serviceType in db.ServiceTypes on item.Key.ServiceTypeId equals serviceType.Id into g
						 from serviceType in g.DefaultIfEmpty()
						 orderby serviceType.Name, item.Key.Year
						 select new UcrRowServiceType
						 {
							 ServiceTypeName = serviceType.Name,
							 Year = item.Key.Year,
							 Count = item.Select(f => f.masterid ?? f.clientid).Distinct().Count()
						 };
			return AppendTotals(result, source);
		}
		public static IEnumerable<UcrRowFund> ucrFund(IQueryable<UcrSourceRecord> source, ccEntities db)
		{
			var grouped = source.GroupBy(f => new
			{
				f.FundId,
				f.Year,
			});

			var result = from item in grouped
						 join fund in db.Funds on item.Key.FundId equals fund.Id into g
						 from fund in g.DefaultIfEmpty()
						 orderby fund.Name, item.Key.Year
						 select new UcrRowFund
						 {
							 FundName = fund.Name,
							 Year = item.Key.Year,
							 Count = item.Select(f => f.masterid ?? f.clientid).Distinct().Count()
						 };
			return AppendTotals(result, source);
		}
		public static IEnumerable<UcrRowFull> ucrFull(IQueryable<UcrSourceRecord> source, ccEntities db)
		{
			var grouped = source.GroupBy(f => new
			{
				f.SerId,
				f.StateId,
				f.CountryId,
				f.RegionId,
				f.ServiceId,
				f.ServiceTypeId,
				f.FundId,
				f.Year
			});

			var result = from item in grouped
						 join ser in db.AgencyGroups.Where(f => !f.ExcludeFromReports) on item.Key.SerId equals ser.Id into serGroup
						 from ser in serGroup.DefaultIfEmpty()
						 join state in db.States on item.Key.RegionId equals state.Id into stateGroup
						 from state in stateGroup.DefaultIfEmpty()
						 join country in db.Countries on item.Key.RegionId equals country.Id into countryGroup
						 from country in countryGroup.DefaultIfEmpty()
						 join region in db.Regions on item.Key.RegionId equals region.Id into regionGroup
						 from region in regionGroup.DefaultIfEmpty()
						 join service in db.Services on item.Key.RegionId equals service.Id into serviceGroup
						 from service in serviceGroup.DefaultIfEmpty()
						 join serviceType in db.ServiceTypes on item.Key.RegionId equals serviceType.Id into serviceTypeGroup
						 from serviceType in serviceTypeGroup.DefaultIfEmpty()
						 join fund in db.Funds on item.Key.RegionId equals fund.Id into fundGroup
						 from fund in fundGroup.DefaultIfEmpty()

						 select new UcrRowFull
						 {
							 AgencyGroupName = ser.Name,
                             AgencyGroupId = ser.Id,
							 StateName = state.Name ?? "N/A",
							 CountryName = country.Name,
							 RegionName = region.Name,
							 ServiceName = service.Name,
							 ServiceTypeName = serviceType.Name,
							 FundName = fund.Name,
							 Year = item.Key.Year,
							 Count = item.Select(f => f.masterid ?? f.clientid).Distinct().Count()
						 };
			return AppendTotals(result, source);
		}
		#endregion
	}

	public enum ConsolidationLevels
	{
		[Display(Name = "SER")]
		Ser,
		[Display(Name = "State/Province")]
		State,
		[Display(Name = "Country")]
		Country,
		[Display(Name = "Region")]
		Region,
		[Display(Name = "Service")]
		Service,
		[Display(Name = "Service Type")]
		ServiceType,
		[Display(Name = "Fund")]
		Fund
	}

	public class UcrSourceRecord
	{
		public int SerId { get; set; }

		public int? StateId { get; set; }

		public int CountryId { get; set; }

		public int RegionId { get; set; }

		public int ServiceId { get; set; }

		public int ServiceTypeId { get; set; }

		public int FundId { get; set; }

		public int clientid { get; set; }

		public int? masterid { get; set; }

		public DateTime date { get; set; }

		public int Year { get; set; }

		public int Quarter { get; set; }
	}



	#region result records
	public class UcrRowBase
	{
		[Display(Name = "Year", Order = 100)]
		public int? Year { get; set; }

		[Display(Name = "Number of unique clients served", Order = 102)]
		public int Count { get; set; }
	}
	public class UcrRowSer : UcrRowBase
	{
		[Display(Name = "SER")]
		public string AgencyGroupName { get; set; }
        [Display(Name = "SER ID")]
        public int AgencyGroupId { get; set; }
	}
	public class UcrRowState : UcrRowBase
	{
		[Display(Name = "State/Province")]
		public string StateName { get; set; }
	}
	public class UcrRowCountry : UcrRowBase
	{
		[Display(Name = "Country")]
		public string CountryName { get; set; }
	}
	public class UcrRowRegion : UcrRowBase
	{
		[Display(Name = "Region")]
		public string RegionName { get; set; }
	}
	public class UcrRowService : UcrRowBase
	{
		[Display(Name = "Service")]
		public string ServiceName { get; set; }
	}
	public class UcrRowServiceType : UcrRowBase
	{
		[Display(Name = "Service Type")]
		public string ServiceTypeName { get; set; }
	}
	public class UcrRowFund : UcrRowBase
	{
		[Display(Name = "Fund")]
		public string FundName { get; set; }
	}
	public class UcrRowFull : UcrRowBase
	{
		[Display(Name = "SER")]
		public string AgencyGroupName { get; set; }
        [Display(Name = "SER ID")]
        public int AgencyGroupId { get; set; }
		[Display(Name = "State/Province")]
		public string StateName { get; set; }
		[Display(Name = "Country")]
		public string CountryName { get; set; }
		[Display(Name = "Region")]
		public string RegionName { get; set; }
		[Display(Name = "Service")]
		public string ServiceName { get; set; }
		[Display(Name = "Service Type")]
		public string ServiceTypeName { get; set; }
		[Display(Name = "Fund")]
		public string FundName { get; set; }
	}
	#endregion

	#region quarterly
	public class UcrqRowBase : UcrRowBase
	{

		[Display(Name = "Quarter", Order = 101)]
		public int? Quarter { get; set; }

	}
	public class UcrqRowSer : UcrqRowBase
	{
		[Display(Name = "SER")]
		public string AgencyGroupName { get; set; }
        [Display(Name = "SER ID")]
        public int AgencyGroupId { get; set; }
	}
	public class UcrqRowState : UcrqRowBase
	{
		[Display(Name = "State/Province")]
		public string StateName { get; set; }
	}
	public class UcrqRowCountry : UcrqRowBase
	{
		[Display(Name = "Country")]
		public string CountryName { get; set; }
	}
	public class UcrqRowRegion : UcrqRowBase
	{
		[Display(Name = "Region")]
		public string RegionName { get; set; }
	}
	public class UcrqRowService : UcrqRowBase
	{
		[Display(Name = "Service")]
		public string ServiceName { get; set; }
	}
	public class UcrqRowServiceType : UcrqRowBase
	{
		[Display(Name = "Service Type")]
		public string ServiceTypeName { get; set; }
	}
	public class UcrqRowFund : UcrqRowBase
	{
		[Display(Name = "Fund")]
		public string FundName { get; set; }
	}
	public class UcrqRowFull : UcrqRowBase
	{
		[Display(Name = "SER")]
        public string AgencyGroupName { get; set; }
        [Display(Name = "SER ID")]
        public int AgencyGroupId { get; set; }
		[Display(Name = "State/Province")]
		public string StateName { get; set; }
		[Display(Name = "Country")]
		public string CountryName { get; set; }
		[Display(Name = "Region")]
		public string RegionName { get; set; }
		[Display(Name = "Service")]
		public string ServiceName { get; set; }
		[Display(Name = "Service Type")]
		public string ServiceTypeName { get; set; }
		[Display(Name = "Fund")]
		public string FundName { get; set; }
	}
	#endregion


}