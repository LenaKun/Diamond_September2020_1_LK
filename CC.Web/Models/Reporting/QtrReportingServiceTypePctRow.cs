using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace CC.Web.Models.Reporting
{
	class QtrReportingServiceTypePctRow : ReportingServiceTypePctRow
	{
		[ScaffoldColumn(false)]
		public int? Year { get; set; }

		/// <summary>
		/// Qarter - 1..4
		/// </summary>
		[ScaffoldColumn(false)]
		public int? Quarter { get; set; }


		[Display(Name = "From", Order = 6)]
		public string From
		{
			get
			{
				if (Year == null || Quarter == null) return null;
				else
					return new DateTime(Year.Value, (Quarter.Value - 1) * 3 + 1, 1).ToString("MMM-yy");
			}
		}
		[Display(Name = "To", Order = 7)]
		public string To
		{
			get
			{
				if (Year == null || Quarter == null) return null;
				else
					return new DateTime(Year.Value, (Quarter.Value - 1) * 3 + 3, 1).ToString("MMM-yy");
			}
		}

        [ScaffoldColumn(false)]
        public new string Errors { get; set; }
	}
}
