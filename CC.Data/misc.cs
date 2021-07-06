using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{

	public static class CCDecimals
	{
		public static int CompareDigits = 3;
		//
		// Summary:
		//     Invokes the canonical Truncate function. For information about the canonical
		//     Truncate function, see Math Canonical Functions (Entity SQL).
		//
		// Parameters:
		//   value:
		//     The number to truncate.
		//
		//   digits:
		//     The length or precision to truncate to.
		//
		// Returns:
		//     value truncated to the length or precision specified by digits.
		[System.Data.Objects.DataClasses.EdmFunction("Edm", "Truncate")]
		public static decimal? Truncate(decimal? value, int? digits)
		{
			if (value == null)
			{
				return null;
			}
			else
			{
				return Math.Round(value.Value, digits.Value);
			}
		}
		public static readonly string DecimalDigitsDisplayItemName = "DecimalDisplayDigits";
		public static string Format(this decimal d)
		{
			int digits = GetDecimalDigits();
			return d.ToString(string.Format("N{0}", digits));
		}
		public static string Format(this decimal? d)
		{
			if (d.HasValue)
			{
				return Format(d.Value);
			}
			else
			{
				return "N/A";
			}
		}
		public static string FormatPercentage(this decimal d)
		{
			int digits = GetDecimalDigits();
			return d.ToString(string.Format("P{0}", digits));
		}
		public static int GetDecimalDigits()
		{
			int digits =2;
			if (System.Web.HttpContext.Current != null)
			{
				var obj = System.Web.HttpContext.Current.Items[DecimalDigitsDisplayItemName];
				if (obj is int)
				{
					digits = (int)obj;
				}
			}
			if (digits > 3) { digits = 3; }
			return digits;
		}
	}
}
