using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApplication2
{
	class SpecialDateTimeConverter : CsvHelper.TypeConversion.DateTimeConverter
	{
		string format = "dd/MM/yyyy HH:mm";
		public SpecialDateTimeConverter()
		{
		}
		public SpecialDateTimeConverter(string exactFormat) : this() { format = exactFormat; }


		public override object ConvertFromString(string text)
		{
			return this.ConvertFromString(System.Globalization.CultureInfo.CurrentCulture, text);
		}
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
				var s = text as string;
				if (string.IsNullOrEmpty(s))
				{
					return null;
				}

				DateTime result;

				if (string.IsNullOrEmpty(format))
				{
					if (DateTime.TryParse(s, culture, System.Globalization.DateTimeStyles.None, out result))
					{
						return result;
					}
					else
					{
						return null;
					}
				}
				else
				{
					if (DateTime.TryParseExact(s, format, culture, System.Globalization.DateTimeStyles.None, out result))
					{
						return result;
					}
					else
					{
						return null;
					}
				}
			}
			return base.ConvertFromString(culture, text);
		}

	}

}
