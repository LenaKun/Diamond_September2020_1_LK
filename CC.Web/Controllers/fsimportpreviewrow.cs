using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CC.Web.Controllers
{
	class fsimportpreviewrow
	{
		public int RowIndex { get; set; }

		public int ClientId { get; set; }

		public string ClientName { get; set; }

		public DateTime? StartDate { get; set; }

		public decimal? DiagnosticScore { get; set; }

		public string FunctionalityLevelName { get; set; }

		public string Errors { get; set; }
	}
}
