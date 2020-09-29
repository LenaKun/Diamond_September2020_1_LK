using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CC.Web.Models.Reporting
{
	public class AnnualGGReportRow
	{
		[Display(Name="Agency", Order=0)]
		public string AgencyGroupName { get; set; }

		[Display(Name="Country", Order=1)]
		public string CountryName { get; set; }

		[Display(Name="State", Order=2)]
		public string StateName { get; set; }

		[Display(Name="City",Order=3)]
		public string City { get; set; }
		
		[Display(Name="App/s #", Order=4)]
		public string AppIds { get; set; }

		[Display(Name="# Unduplicated Clients",Order=5)]
		public int UniquClientsCount { get; set; }

		[Display(Name="Service",Order=6)]
		public string ServiceName { get; set; }

		[Display(Name="Remarks", Order=7)]
		public string Remarks { get; set; }

		[Display(Name="Total CC Grant", Order=8)]
		public decimal? TotalCcGrant { get; set; }

		[Display(Name = "CUR", Order = 9)]
		public string FundCurId { get; set; }

		[Display(Name="Grant Amount", Order=10)]
		public decimal? TotalAmount { get; set; }

		[Display(Name = "CUR", Order = 11)]
		public string CurId { get; set; }

	}
}