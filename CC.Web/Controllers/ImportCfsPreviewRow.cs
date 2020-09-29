using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CC.Web.Controllers
{
	class ImportCfsPreviewRow
	{
		public int RowIndex { get; set; }

		public int? ClientId { get; set; }

		public string ClientName { get; set; }

		public DateTime? StartDate { get; set; }

		public IEnumerable<string> Errors { get; set; }

		public string Warning { get; set; }

		public DateTime? CFSApproved { get; set; }
	}
}