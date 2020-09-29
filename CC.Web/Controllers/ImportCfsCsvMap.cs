using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CC.Web.Controllers
{
	class ImportCfsCsvMap : CsvHelper.Configuration.CsvClassMap<CC.Data.ImportHcep>
	{
		public ImportCfsCsvMap()
		{
			Map(f => f.ClientId).Name("CC_ID", "CCID", "CLIENTID");
			Map(f => f.StartDate).Name("START_DATE", "startdate", "start").TypeConverter<CC.Web.Models.InvariantDateTypeConverter>();
			Map(f => f.CfsApproved).Name("CFS_Approved", "Cfs Approved", "CFS Approved").TypeConverter<CC.Web.Models.InvariantDateTypeConverter>();
		}
	}
}