using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CC.Data
{
	public partial class ImportHcep
	{
		[System.ComponentModel.DataAnnotations.ScaffoldColumn(false)]
		public DateTime? EndDateDisplay
		{
			get { return this.EndDate.HasValue ? this.EndDate.Value.AddDays(-1) : (DateTime?)null; }
			set { this.EndDate = value.HasValue ? value.Value.AddDays(1) : (DateTime?)null; }
		}
	}
}
