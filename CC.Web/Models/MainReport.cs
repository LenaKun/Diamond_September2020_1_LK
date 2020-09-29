using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CC.Web.Models
{
	public class MainReportRowModel
	{
		public MainReportRowModel() { }

		[ScaffoldColumn(false)]
		public int MainReportId { get; set; }
		[ScaffoldColumn(false)]
		public int AgencyGroupId { get; set; }
		[ScaffoldColumn(false)]
		public int AppBudgetId { get; set; }
		[ScaffoldColumn(false)]
		public int FundId { get; set; }
		[ScaffoldColumn(false)]
		public bool Adjusted { get; set; }
		[ScaffoldColumn(false)]
		public int AppId { get; set; }

		public long StartSort { get;set;}
		public long EndSort { get;set;}

		[Display(Name = "Ser")]
		public string AgencyGroupName { get; set; }

		[Display(Name = "Fund")]
		public string FundName { get; set; }
		[Display(Name = "App")]
		public string AppName { get; set; }

		[Display(Name = "From")]
		[DataType("Month")]

		[UIHint("Month")]
		public DateTime StartDate { get; set; }

		[Display(Name = "To")]
		[UIHint("Month")]
		public DateTime EndDate { get; set; }

		[Display(Name = "Status")]
		public string StatusString
		{
			get
			{
				var res = Status.ToString();
				if (this.Revised) { res += " *Revised"; };
				return res;
			}
		}

		[ScaffoldColumn(false)]
		public CC.Data.MainReport.Statuses Status { get; set; }
		[ScaffoldColumn(false)]
		public bool Revised{get;set;}

		[Display(Name = "Remarks")]
		public string Remarks
		{
			get
			{
				var remarks = string.Empty;
				if (this.Adjusted) remarks += "*Adjusted";
				return remarks;
			}
		}

		public bool CanBeRevised { get; set; }
		[Display(Name = "Report Amount")]
		public decimal? Amount { get; set; }

		[Display(Name = "Cur")]
		public string CurrencyId { get; set; }

		[Display(Name = "App Amount")]
		public decimal? AppAmount { get; set; }

	}

}