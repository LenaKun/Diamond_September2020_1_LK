using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CC.Data
{
	[MetadataType(typeof(PersonnelReportMetadata))]
	public partial class PersonnelReport
	{
		
			
		//public enum PositionTypes
		//{
		//	PartTime = 1,
		//	FullTime = 2,
		//	Contractor = 3
		//}

	}

	public class PersonnelReportMetadata
	{
		
		public decimal PartTimePercentage { get; set; }
	}
}

