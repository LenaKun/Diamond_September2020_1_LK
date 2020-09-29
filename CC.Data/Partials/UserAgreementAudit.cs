using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CC.Data
{
	[MetadataType(typeof(UserAgreementAuditMetadata))]
	public partial class UserAgreementAudit
	{
		
			

	}


	class UserAgreementAuditMetadata
	{
	}

	[MetadataType(typeof(UserAgreementMetadata))]
	public partial class UserAgreement
	{



	}


	public class UserAgreementMetadata
	{
		[DataType(System.ComponentModel.DataAnnotations.DataType.Html)]
		public string Text { get; set; }
	}

	
}
