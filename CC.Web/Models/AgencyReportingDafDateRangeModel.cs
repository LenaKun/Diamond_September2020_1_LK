using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CC.Web.Models
{
	public class AgencyReportingDafDateRangeModel : jQueryDataTableParamModel
	{
		[Display(Name = "Region")]
		public int? SelectedRegionId { get; set; }
		[Display(Name = "Country")]
		public int? SelectedCountryId { get; set; }
		[Display(Name = "SER")]
		public int? SelectedAgencyGroupId { get; set; }
		[DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
		[Display(Name = "Diagnostic Score Start Date From")]
		public DateTime? From { get; set; }
		[DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
		[Display(Name = "Diagnostic Score Start Date To")]
		public DateTime? To { get; set; }

		public IQueryable<DafDateRangeRow> GetAgencyReportingData(CC.Data.ccEntities db, CC.Data.Services.IPermissionsBase permissions)
		{
			var q = from fs in db.FunctionalityScores
					join c in db.Clients.Where(permissions.ClientsFilter) on fs.ClientId equals c.Id
					select new DafDateRangeRow
					{
						RegionId = fs.Client.Agency.AgencyGroup.Country.RegionId,
						CountryId = fs.Client.Agency.AgencyGroup.CountryId,
						AgencyGroupId = fs.Client.Agency.GroupId,
						ClientId = fs.ClientId,
						DiagnosticScore = fs.DiagnosticScore,
						StartDate = fs.StartDate
					};
			return q;
		}
		public IQueryable<DafDateRangeRow> ApplyFilter(IQueryable<DafDateRangeRow> q)
		{
			var filtered = from item in q
						   where SelectedRegionId == null || SelectedRegionId == 0 || item.RegionId == SelectedRegionId
						   where SelectedCountryId == null || item.CountryId == SelectedCountryId
						   where SelectedAgencyGroupId == null || item.AgencyGroupId == SelectedAgencyGroupId
						   where From == null || item.StartDate >= From
						   where To == null || item.StartDate <= To
						   select item;
			return filtered;
		}


		internal IOrderedQueryable<DafDateRangeRow> ApplySort(IQueryable<DafDateRangeRow> filtered)
		{
			IOrderedQueryable<DafDateRangeRow> sorted;
			sorted = filtered.OrderBy(f => f.ClientId).ThenBy(f => f.StartDate);
			return sorted;
		}
	}

	public class DafDateRangeRow
	{
		[System.ComponentModel.DataAnnotations.ScaffoldColumn(false)]
		public int RegionId { get; set; }

		[System.ComponentModel.DataAnnotations.ScaffoldColumn(false)]
		public int CountryId { get; set; }

		[System.ComponentModel.DataAnnotations.ScaffoldColumn(false)]
		public int AgencyGroupId { get; set; }

		[Display(Name = "CC_ID")]
		public int ClientId { get; set; }

		[Display(Name = "DIAGNOSTIC_SCORE")]
		public decimal DiagnosticScore { get; set; }

		[Display(Name = "START_DATE")]
		public DateTime StartDate { get; set; }
	}
}