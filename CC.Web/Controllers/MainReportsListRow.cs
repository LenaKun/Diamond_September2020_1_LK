using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CC.Web.Controllers
{
	class MainReportsListRow
	{
		public string AgencyGroupName { get; set; }

		public string FundName { get; set; }
        public string MasterFundName { get; set; }

		public DateTime End { get; set; }

		public string AppName { get; set; }

		public DateTime Start { get; set; }

		public int StatusId { get; set; }

		public decimal Amount { get; set; }

		public decimal CcGrant { get; set; }

		public string CurrencyId { get; set; }

		public bool Adjusted { get; set; }

		public bool Revised { get; set; }

		public int MainReportId { get; set; }

		public int AgencyGroupId { get; set; }

		public int AppBudgetId { get; set; }

		public int AppId { get; set; }

		public int FundId { get; set; }

		public bool CanBeRevised { get; set; }

		public string Status { get; set; }

		public DateTime? SubmittedAt { get; set; }
	}
}
