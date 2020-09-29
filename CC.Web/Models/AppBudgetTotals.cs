using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CC.Web.Models
{
	public class AppBudgetTotals
	{
		public decimal AppMatch;
		public decimal CcGrant { get; set; }

		public decimal RequiredMatch { get; set; }

		public decimal AgencyContribution { get; set; }

		public decimal AppAmount { get; set; }

		public decimal? Spend { get; set; }

		public int AppBudgetId { get; set; }

		public bool AgencyContributionRequired { get; set; }

        public decimal? CancellationAmount { get; set; }

        public bool isLastReport { get; set; }

		[UIHint("Percentage")]
		public decimal? HcPercentage
		{
			get
			{
				if (this.CcGrant == 0) return null;
				else return(this.HcCcGrant / this.CcGrant);
			}
		}

		[UIHint("Percentage")]
		public decimal? AdminPercentage
		{
			get
			{
				if (this.CcGrant == 0) return null; else return this.AdminCcGrant / this.CcGrant;
			}
		}

		[UIHint("Percentage")]
		public decimal? OtherPercentage
		{
			get
			{
				 return 1 - this.HcPercentage - this.AdminPercentage;
			}
		}


		public decimal AdminCcGrant { get; set; }

		public decimal HcCcGrant { get; set; }



		public class AppBudgetSubTotals
		{
			public int AgencyId { get; set; }

			public string AgencyName { get; set; }

			public decimal? CcGrant { get; set; }

			public decimal? RequiredMatch { get; set; }

			public decimal? AgencyContribution { get; set; }
		}

		public List<AppBudgetSubTotals> AgencyTotals { get; set; }
	}
}
