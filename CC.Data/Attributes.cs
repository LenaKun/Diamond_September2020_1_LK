using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.ComponentModel.DataAnnotations
{
	/// <summary>
	/// Regular Expression that is applied to client side validations only - for validating non string fields.
	/// </summary>
	public class ClientOnlyRegexAttribute : System.ComponentModel.DataAnnotations.RegularExpressionAttribute
	{
		public ClientOnlyRegexAttribute(string pattern) : base(pattern) {
			this.ErrorMessage = "Please enter Qtr/Yr";
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public override bool IsValid(object value)
		{
			return true;
		}
	}

	/// <summary>
	/// Regex clientside date input validation
	/// </summary>
	public class DateFormatAttribute : ClientOnlyRegexAttribute
	{
		public DateFormatAttribute()
			: base(@"^(\d{1,2} \w{3,3} \d{4,4}( \d{2,2}:\d{2,2}:\d{2,2})?)?$")
		{
			this.ErrorMessage = "Please enter date in format dd MMM yyyy.";
		}
		
	}
}
