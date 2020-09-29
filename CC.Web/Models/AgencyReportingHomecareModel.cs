using CC.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;

namespace CC.Web.Models
{
	public class AgencyReportingHomecareModel : jQueryDataTableParamModel
	{
		[Display(Name = "Region")]
		public int? SelectedRegionId { get; set; }
		[Display(Name = "SER")]
		public int? SelectedAgencyGroupId { get; set; }
		[Display(Name = "Year")]
		public int? SelectedYear { get; set; }

		public IQueryable<AgencyReportingHcRow> GetAgencyReportingData(CC.Data.ccEntities db, CC.Data.Services.IPermissionsBase permissions)
		{
			db.CommandTimeout = 600;
			var q = db.Over105HcHours(this.SelectedRegionId,
						this.SelectedAgencyGroupId,
						this.SelectedYear,
						this.sSearch,
						this.iDisplayLength == int.MaxValue ? int.MaxValue : this.iDisplayLength + 1,
						this.iDisplayStart,
						permissions.User.Id).Select(g => new AgencyReportingHcRow
						{
							AgencyGroupId = g.AgencyGroupId,
							AgencyGroupName = g.AgencyGroupName,
							Year = g.Year,
							Month = g.Month,
							ClientId = g.ClientId,
							ReportedHours = g.ReportedHours,
							GovHours = g.GovHours,
							TotalHours = g.TotalHours,
							RegionId = g.RegionId,
							ReportedDate = g.ReportDate,
							Week = g.Week
						}).Where(f => f.TotalHours > 105).OrderBy(f => f.ReportedDate).ToList();

			return q.AsQueryable();
		}
	}

	public class AgencyReportingHcRow
	{
		[System.ComponentModel.DataAnnotations.ScaffoldColumn(false)]
		public int? RegionId { get; set; }
		[System.ComponentModel.DataAnnotations.ScaffoldColumn(false)]
		public DateTime? ReportedDate { get; set; }

		[Display(Name = "Ser ID")]
		public int? AgencyGroupId { get; set; }

		[Display(Name = "SER")]
		public string AgencyGroupName { get; set; }

		[Display(Name = "Year")]
		public int? Year { get; set; }

		[Display(Name = "Month")]
		public string Month { get; set; }

		[Display(Name = "Week")]
		public int? Week { get; set; }

		[Display(Name = "CC ID")]
		public int ClientId { get; set; }

		[Display(Name = "Reported Hours")]
		public decimal? ReportedHours { get; set; }

		[Display(Name = "Gov Hours")]
		public decimal? GovHours { get; set; }

		[Display(Name = "Total Hours")]
		public decimal? TotalHours { get; set; }
	}
}