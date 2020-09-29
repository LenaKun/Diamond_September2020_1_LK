using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CC.Data
{
	public class HcQuantityReported
	{
		public int ClientReportId { get; set; }

		public int SubReportId { get; set; }

		public int MainReportId { get; set; }

		public int ClientId { get; set; }

		public decimal? Rate { get; set; }

		public DateTime? Date { get; set; }

		public decimal Quantity { get; set; }

		public int MainReportStatusId { get; set; }

		public int? ClientMasterId { get; set; }
	}
}
