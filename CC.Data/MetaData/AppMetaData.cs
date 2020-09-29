using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CC.Data.MetaData
{
	class AppMetaData
	{
		[Required]
		public string Name { get; set; }

		[DateFormat()]
		[DataType(DataType.Date)]
		public DateTime StartDate { get; set; }

		[DateFormat()]
		[DataType(DataType.Date)]
		public DateTime EndDate { get; set; }

		[DateFormat()]
		[DataType(DataType.Date)]
		public DateTime EndDateDisplay { get; set; }

		[Required]
		[Display(Name = "CUR")]
		public string CurrencyId { get; set; }

		[Display(Name = "SER")]
		public int AgencyGroupId { get; set; }

		[Display(Name = "Fund")]
		public int FundId { get; set; }

		[Display(Name = "Only EOY Validation")]
		public bool EndOfYearValidationOnly { get; set; }

		[Display(Name="Interline Transfer")]
		public bool InterlineTransfer { get; set; }

		[Display(Name = "CC Grant")]
		[Range(-922337203685477.5808, 922337203685477.5807)]
		public decimal CcGrant { get; set; }

		[Display(Name = "Agency Contribution")]
		public bool AgencyContribution { get; set; }

		[Display(Name= "Required Match")]
		public virtual decimal RequiredMatch { get; set; }

		[Range(0.01, 922337203685477.58)]
		[Display(Name = "Total all NONE Homecare services amount allowed")]
		public virtual Nullable<decimal> MaxNonHcAmount { get; set; }

		[Range(0.01, 922337203685477.58)]
		[Display(Name = "Total Admin allowed")]
		public virtual Nullable<decimal> MaxAdminAmount { get; set; }

		[Range(0, 922337203685477.58)]
		[Display(Name = "Historical Expenditure Amount")]
		public virtual Nullable<decimal> HistoricalExpenditureAmount { get; set; }

		[Range(0, 922337203685477.58)]
		[Display(Name = "Average Reimbursement Cost")]
		public virtual Nullable<decimal> AvgReimbursementCost { get; set; }
	}
}

