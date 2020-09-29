using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace CC.Web.Models.Reporting
{
	public class ReportingServiceTypePctFilterBase
	{
		public ReportingServiceTypePctFilterBase()
		{
			this.IgnoreUnsubmitted = true;
		}

		[Required()]
		[Display(Name = "Display Currency")]
		public string DisplayCurrency { get; set; }

		public System.Web.Mvc.SelectList ConvertableCurrencies { get { return new System.Web.Mvc.SelectList(CC.Data.Currency.ConvertableCurrencies); } }

		[Display(Name = "SER")]
		public int? AgencyGroupId { get; set; }

		[Display(Name = "Fund")]
		public int? FundId { get; set; }

		[Display(Name = "Ignore  new/ returened to agency budgets")]
		public bool IgnoreUnsubmitted { get; set; }

		[Display(Name = "Region")]
		public int? RegionId { get; set; }

		[Display(Name = "Country")]
		public int? CountryId { get; set; }

		[Display(Name = "State")]
		public int? StateId { get; set; }

	}
}
