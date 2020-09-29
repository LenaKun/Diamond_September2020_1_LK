using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace CC.Data
{
	[MetadataType(typeof(MasterFundMetadata))]
	public partial class MasterFund
	{


	}
	public class MasterFundMetadata
	{


		[DateFormat()]
		[DataType(DataType.Date)]
		public virtual System.DateTime StartDate
		{
			get;
			set;
		}

		[DateFormat()]
		[DataType(DataType.Date)]
		public virtual System.DateTime EndDate
		{
			get;
			set;
		}
	}
}
