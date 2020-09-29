using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApplication2
{
	class SpecialBoolConverter : CsvHelper.TypeConversion.BooleanConverter
	{
		string[] trueStrings = { "Yes", "Y" };
		string[] falseStrings = { "No", "n" };


		public override bool CanConvertFrom(Type type)
		{
			if (type == typeof(string))
			{
				return true;
			}
			return base.CanConvertFrom(type);
		}
		public override object ConvertFromString(System.Globalization.CultureInfo culture, string text)
		{
			if (text is string)
			{
				bool t;

				var strval = text as string;
				if (bool.TryParse(strval, out t))
				{
					return t;
				}
				else if (trueStrings.Any(f => f.Equals(strval, StringComparison.InvariantCultureIgnoreCase)))
				{
					return true;
				}
				else
				{
					return false;
				}

			}
			return base.ConvertFromString(culture, text);
		}
		public override object ConvertFromString(string text)
		{
			return this.ConvertFromString(System.Globalization.CultureInfo.CurrentCulture, text);
		}
	}
}
