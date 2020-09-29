using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CC.Web.Models
{
	public class ClientEventsCountModel : ModelBase
	{
		public int Id { get; set; }

		public int SubReportId { get; set; }

		public DateTime EventDate { get; set; }

		public int JNVCount { get; set; }

		public int? TotalCount { get; set; }

		public string Remarks { get; set; }
	}
}