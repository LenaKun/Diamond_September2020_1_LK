using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace CC.Data.MetaData
{
	public class HCWeeklyCapMetaData
	{
		[Required]
		public string Name { get; set; }
		[Required]
		public decimal CapPerPerson { get; set; }
		[Required]
		[DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
		public DateTime StartDate { get; set; }
		[Required]
		[DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
		public DateTime EndDate { get; set; }
	}
}
