using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CC.Data.Partials
{
	class SpecialIntConverter: CsvHelper.TypeConversion.DefaultTypeConverter
	{
		public override object ConvertFromString(System.Globalization.CultureInfo culture, string text)
		{

			throw new NotImplementedException();
		}
	}
}
