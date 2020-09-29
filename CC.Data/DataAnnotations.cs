using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace CC.Data
{
	/// <summary>
	/// RangeAttribute with option to provide a format string for the error message
	/// </summary>
	public class RangeAttributeEx : RangeAttribute
	{
		private string valueFormatString;
		public RangeAttributeEx(Type type, string min, string max) : base(type, min, max) { }
		public RangeAttributeEx(Type type, string min, string max, string format) : this(type, min, max) { this.valueFormatString = format; }
		public override string FormatErrorMessage(string name)
		{
			return string.Format(this.ErrorMessage ?? "The {0} must be between {1} and {2}", name, toString(this.Minimum), toString(this.Maximum));

		}
		private string toString(object value)
		{
			if (value == null)
			{
				return string.Empty;
			}
			else if (this.valueFormatString != null)
			{
				
				if (value is DateTime)
				{
					return ((DateTime)value).ToString(valueFormatString);
				}
				else
				{
					return valueFormatString.ToString();
				}
			}
			else
			{
				return value.ToString();
			}
		}
	}
}
