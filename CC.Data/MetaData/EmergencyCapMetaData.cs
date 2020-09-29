using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace CC.Data.MetaData
{
	public class EmergencyCapMetaData
	{
		[Required]
		public string Name { get; set; }
		[Required]
		public decimal CapPerPerson { get; set; }
		[Required]
		public decimal DiscretionaryPercentage { get; set; }
		[Required]
		[DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
		public DateTime StartDate { get; set; }
		[Required]
		[DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
		public DateTime EndDate { get; set; }
	}
}
