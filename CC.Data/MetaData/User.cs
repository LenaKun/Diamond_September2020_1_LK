using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace CC.Data.MetaData
{

	internal sealed class UserMetadata
	{

		public int Id;

		[Required]
		public int RoleId { get; set; }

		[Required(AllowEmptyStrings = false)]
		[Display(Name="Username")]
		public string UserName { get; set; }

		[Required]
		public string Email { get; set; }
		[Display(Name = "Decimal places")]
		public decimal DecimalDisplayDigits { get; set; }

		[Display(Name="First Name")]
		public string FirstName { get; set; }

		[Display(Name="Last Name")]
		public string LastName { get; set; }

	}

}
