using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace CC.Web.Models.Reporting
{
	public class YtdReportinServiceTypePctFilter : ReportingServiceTypePctFilterBase
	{
		[Display(Name = "Year")]
		public int? Year { get; set; }
	}
}