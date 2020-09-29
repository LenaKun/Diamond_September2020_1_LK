using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace CC.Data.MetaData
{
	class CfsAmountMetaData
	{
		[Required]
		public int Year { get; set; }
		[Required]
		public string CurrencyId { get; set; }
		[Required]
		public int Level { get; set; }
		[Required]
		public decimal Amount { get; set; }
	}
}
