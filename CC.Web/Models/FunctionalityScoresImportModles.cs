using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CC.Web.Models
{
	
	
	public class FunctionalityScoresCsvMap : CsvHelper.Configuration.CsvClassMap<CC.Data.ImportFunctionalityScore>
	{
		public FunctionalityScoresCsvMap()
		{
			Map(f=>f.ClientId).Name("CC_ID", "CCID", "CLIENTID", "CLIENT_ID" );
			Map(f => f.DiagnosticScore).Name("DIAGNOSTIC_SCORE", "DIAGNOSTIC SCORE", "SCORE", "DIAGNOSTICSCORE");
			Map(f=>f.StartDate).Name("START_DATE", "DATE", "STARTDATE").TypeConverter<InvariantDateTypeConverter>();
		}
	}
	public class FunctionalityScoresPreView
	{
		public FunctionalityScoresPreView()
		{
			this.Errors = new List<string>();
		}
		public int Id { get; set; }
		public int Index { get; set; }
		public int ClientId { get; set; }
		public decimal Score { get; set; }
		public DateTime StartDate { get; set; }
		public string ClientName { get; set; }
		public int FunctionalityLevelId { get; set; }
		public string FunctionalityLevelName { get; set; }
		public List<string> Errors { get; set; }
	}
}