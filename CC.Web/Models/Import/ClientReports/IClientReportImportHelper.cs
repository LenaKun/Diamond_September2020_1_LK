using System;
using System.Collections.Generic;
using System.Linq;

namespace CC.Web.Models.Import.ClientReports
{
	public	interface IClientReportImportHelper
	{
		CC.Web.Models.jQueryDataTableResult<object> GetPreviewData(Guid id, CC.Web.Models.jQueryDataTableParamModel jq);
		IEnumerable<CC.Web.Models.jQueryDataTableColumn> PreviewDataColumns();
		void Upload(string filename, Guid importId, int subReportId);
		IEnumerable<object> Errors(Guid importId);
		IEnumerable<string> CsvColumnNames();
		bool IsValid(Guid importId);
		void Import(Guid guid);
	}
}
