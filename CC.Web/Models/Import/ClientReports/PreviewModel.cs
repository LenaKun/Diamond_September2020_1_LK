using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CC.Web.Models.Import.ClientReports
{
	public class PreviewModel:UploadModel
	{
		
		public IEnumerable<jQueryDataTableColumn> Columns { get; set; }

		public bool IsValid { get; set; }

		public bool DeleteAll { get; set; }

		public bool Finished { get; set; }

	}
}