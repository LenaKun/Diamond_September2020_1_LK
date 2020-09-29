using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace CC.Data.MetaData
{
	class AppBudgetServiceMetaData
	{
		
		[Display(Name="Budget Remarks")]
		public string Remarks { get; set; }
	}
}
