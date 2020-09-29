using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CC.Web.Controllers
{
	class ImportHcepPreviewRow
	{
		public int RowIndex { get; set; }

		public int? ClientId { get; set; }

		public string ClientName { get; set; }

		public DateTime? StartDate { get; set; }

		public DateTime? EndDate { get; set; }

		public IEnumerable<string> Errors { get; set; }
	}
}
