using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CC.Web.Controllers
{
	class ImportHcepCsvMap:CsvHelper.Configuration.CsvClassMap<CC.Data.ImportHcep>
	{
		public ImportHcepCsvMap()
		{
			Map(f => f.ClientId).Name("CC_ID", "CCID", "CLIENTID");
			Map(f => f.StartDate).Name("START_DATE", "startdate", "start").TypeConverter<CC.Web.Models.InvariantDateTypeConverter>();
			Map(f => f.EndDateDisplay).Name("END_DATE").TypeConverter<CC.Web.Models.InvariantDateTypeConverter>();
		}
	}
}
