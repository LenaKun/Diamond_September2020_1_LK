using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace CC.Data.MetaData
{
	class FundMetaData
	{   
		[DateFormat()]
		[DataType(DataType.Date)]
		public DateTime StartDate { get; set; }

		[DateFormat()]
		[DataType(DataType.Date)]
		public DateTime EndDate { get; set; }
	}
}
