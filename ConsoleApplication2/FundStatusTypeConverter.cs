using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CsvHelper.TypeConversion;
using CC.Data;

namespace ConsoleApplication2
{
	class FundStatusTypeConverter : DefaultTypeConverter
	{
		public List<FundStatus> fundstatuses = null;
		public FundStatusTypeConverter()
		{
			using (var db = new ccEntities())
			{
				db.ContextOptions.LazyLoadingEnabled = false;
				db.ContextOptions.ProxyCreationEnabled = false;
				fundstatuses = db.FundStatuses.ToList();
			}
		}



		public override bool CanConvertFrom(Type type)
		{
			if (type == typeof(string)) { return true; }
			return base.CanConvertFrom(type);
		}

		Func<string, string> asdf = f => f.ToLower().Replace('_', ' ').Replace(' ', '_').Replace("no_", "not_").Replace("not_", "no_");

		public override object ConvertFromString(string text)
		{
			var culture = System.Globalization.CultureInfo.CurrentCulture;

			return this.ConvertFromString(culture, text);
			return base.ConvertFromString(text);
		}
		public override object ConvertFromString(System.Globalization.CultureInfo culture, string text)
		{
			FundStatus result = null;
			if (text == null) { return null; }
			else
			{

				result = fundstatuses.SingleOrDefault(f => f.Name.Equals(text));

				if (result == null)
				{
					result = fundstatuses.SingleOrDefault(f => f.Name.Replace('_', ' ').Equals(text));
					if (result == null)
					{
						result = fundstatuses.SingleOrDefault(f => asdf(f.Name).Equals(asdf(text)));
						return result;
					}
					else { return result; }
				}
				else { return result; }
			}


			return base.ConvertFromString(culture, text);
		}
	}

#warning junk
	class HelpTableConverter<T> : DefaultTypeConverter where T : class
	{
		List<T> list = null;
		HelpTableConverter()
		{
			using (var db = new ccEntities())
			{
				var objectset = db.CreateObjectSet<T>();
				this.list = objectset.ToList();
			}
		}

		public override bool CanConvertFrom(Type type)
		{
			if (type == typeof(string)) { return true; }
			return base.CanConvertFrom(type);
		}
		public override object ConvertFromString(System.Globalization.CultureInfo culture, string text)
		{

			return base.ConvertFromString(culture, text);
		}
	}
}
