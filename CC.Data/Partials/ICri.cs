using System;
using System.Linq;
using System.Collections.Generic;
namespace CC.Data
{
	public interface ICri
	{
		int? ClientId { get; set; }
		int? ClientReportId { get; set; }
		Guid Id { get; set; }
		Guid ImportId { get; set; }
		string Remarks { get; set; }
		int? SubReportId { get; set; }
		int? RowIndex { get; set; }
	}

}
