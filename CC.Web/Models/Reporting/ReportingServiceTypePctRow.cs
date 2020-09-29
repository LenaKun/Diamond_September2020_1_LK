using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using CC.Data;

namespace CC.Web.Models.Reporting
{
	public class ReportingServiceTypePctRow
	{
		[Display(Name = "Region", Order = 0)]
		public string RegionName { get; set; }

		[Display(Name = "Country", Order = 1)]
		public string CountryCode { get; set; }

		[Display(Name = "State", Order = 2)]
		public string StateCode { get; set; }

		[Display(Name = "Fund", Order = 4)]
		public string FundName { get; set; }

		[Display(Name = "App", Order = 5)]
		public string AppName { get; set; }

		[Display(Name = "Ser Name")]
		public string AgencyGroupName { get; set; }

        [Display(Name = "Ser ID")]
        public int AgencyGroupId { get; set; }

		[Display(Name = "Total App Displayed currency", Order = 7)]
		public decimal? AppAmountDispCur{get;set;}

		[Display(Name = "YTD Report Amount", Order = 8)]
		public decimal? ReportAmountDisp { get ;set;}

		[Display(Name = "Display Currency", Order = 9)]
		public string DispCur { get; set; }

		[Display(Name = "YTD Report Amount", Order = 10)]
		public decimal? ReportAmount { get; set; }

		[Display(Name = "App Currency", Order = 11)]
		public string AppCur { get; set; }

		[Display(Name = "% in Total", Order = 12)]
		public decimal? ReportAmountPct { get {  return this.AppAmountDispCur > 0 ? this.ReportAmountDisp / this.AppAmountDispCur : 0; }  }

		[Display(Name = "HomeCare", Order = 13)]
		public decimal? HomeCareAmountDisp { get ;set;}

		[Display(Name = "Other", Order = 14)]
		public decimal? OtherAmountDisp { get;set;}

		[Display(Name = "Admin", Order = 15)]
		public decimal? AdminAmountDisp { get;set;}

		[Display(Name = "Homecare %", Order = 16)]
		[p4b.Web.Export.ExcelDataType(OpenXmlPowerTools.SpreadsheetWriter.CellDataType.Pct)]
		public decimal? HomecarePct { get { return this.ReportAmountDisp > 0 ? this.HomeCareAmountDisp / this.ReportAmountDisp : 0; } }

		[Display(Name = "Other %", Order = 17)]
		[p4b.Web.Export.ExcelDataType(OpenXmlPowerTools.SpreadsheetWriter.CellDataType.Pct)]
		public decimal? OtherPct { get { return this.ReportAmountDisp > 0 ? this.OtherAmountDisp / this.ReportAmountDisp : 0; } }

		[Display(Name = "Admin %", Order = 18)]
		[p4b.Web.Export.ExcelDataType(OpenXmlPowerTools.SpreadsheetWriter.CellDataType.Pct)]
		public decimal? AdminPct { get { return this.ReportAmountDisp > 0 ? this.AdminAmountDisp / this.ReportAmountDisp : 0; } }

		[ScaffoldColumn(false)]
		public decimal? HcReportAmount { get; set; }

		[ScaffoldColumn(false)]
		public decimal? AdminReportAmount { get; set; }

		[ScaffoldColumn(false)]
		public decimal? ExRate { get; set; }

		[ScaffoldColumn(false)]
		public decimal AppAmount { get; set; }

		[ScaffoldColumn(false)]
		public decimal? OtherReportAmount { get {
			var x = (this.HcReportAmount??0) + (this.AdminReportAmount??0);
			if (this.ReportAmount == x)
			{
				return null;
			}
			else
			{
				return this.ReportAmount - x;
			}
		} }

        [ScaffoldColumn(false)]
        public string Errors { get; set; }
	}
}