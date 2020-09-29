using CC.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;

namespace CC.Web.Models
{
	public class AgencyReportingHomecareDetailsModel : jQueryDataTableParamModel
	{
		[Required]
		[Display(Name = "CUR")]
		public string CurId { get; set; }
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
		[Display(Name = "Fund")]
		public int? FundId { get; set; }
		[Display(Name = "App")]
		public int? AppId { get; set; }
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

		public IQueryable<AgencyReportingHomecareDetailsRow> GetAgencyReportingData(CC.Data.ccEntities db, CC.Data.Services.IPermissionsBase permissions)
		{
			db.CommandTimeout = 600;
			var source = db.spAgencyReportingHomecareDetails(this.CurId, this.StartDate, this.EndDate
				, !this.IncludeNotSubmittedReports
				, this.AgencyId
				, this.RegionId
				, this.CountryId
				, this.StateId
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
				, permissions.User.Id).ToList();

			DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
			Calendar cal = dfi.Calendar;

			var q = from item in source
					select new AgencyReportingHomecareDetailsRow
					{
						AgencyId = item.AgencyId,
						Cur = this.CurId,
						ClientId = item.ClientId,
						FundName = item.FundName,
						AppName = item.AppName,
						ServiceName = item.ServiceName,
						Quantity = item.Quantity,
						Rate = item.Rate,
						Amount = item.Amount,
						Date = item.ReportDate,
						IsWeekly = item.IsWeekly.Value,
						WeekNumber = "W" + (item.IsWeekly.Value ? (cal.GetWeekOfYear(item.ReportDate, dfi.CalendarWeekRule, item.SelectedDayOfWeek != null ? (DayOfWeek)item.SelectedDayOfWeek : item.MrStart.DayOfWeek)).ToString() : item.ReportDate.Month.ToString())
					};
			return q.AsQueryable();
		}
	}

	public class AgencyReportingHomecareDetailsRow
	{
		[System.ComponentModel.DataAnnotations.ScaffoldColumn(false)]
		public DateTime? Date { get; set; }

		[System.ComponentModel.DataAnnotations.ScaffoldColumn(false)]
		public bool IsWeekly { get; set; }

		[Display(Name = "Org ID")]
		public int AgencyId { get; set; }

		[Display(Name = "Fund")]
		public string FundName { get; set; }

		[Display(Name = "App")]
		public string AppName { get; set; }

		[Display(Name = "CC ID")]
		public int ClientId { get; set; }

		[Display(Name = "Service")]
		public string ServiceName { get; set; }

		[Display(Name = "Month Yr")]
		public virtual string DateName { get { return Date.HasValue ? Date.Value.ToMonthString() : null; } }

		[Display(Name = "Week #")]
		public string WeekNumber { get; set; }

		[Display(Name = "Quantity")]
		public decimal Quantity { get; set; }

		[Display(Name = "Rate")]
		public decimal? Rate { get; set; }

		[Display(Name = "Amount")]
		public decimal? Amount { get; set; }

		[Display(Name = "Currency")]
		public string Cur { get; set; }
	}
}