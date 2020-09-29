using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CsvHelper.Configuration;

namespace CC.Data.Partials
{
	class CriMetadata
	{
		[CsvField(Ignore = true)]
		[ScaffoldColumn(false)]
		public Guid Id { get; set; }
		
		[CsvField(Ignore = true)]
		[ScaffoldColumn(false)]
		public Guid ImportId { get; set; }
		
		[CsvField(Ignore = true)]
		[ScaffoldColumn(false)]
		public int? ClientReportId { get; set; }
		
		[CsvField(Ignore = true)]
		[ScaffoldColumn(false)]
		public int? SubReportId { get; set; }

		[CsvField(Names = new[] { "CC_ID", "Client_id", "clientid" })]
		[CsvHelper.TypeConversion.TypeConverter(typeof(SpecialIntConverter))]
		public int ClientId { get; set; }
		
		[CsvField(Ignore = true)]
		[ScaffoldColumn(false)]
		public string Remarks { get; set; }
	}

}
