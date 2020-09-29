using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CC.Web.Helpers
{
	public class ImportHelper
	{
		public static IEnumerable<string> GetCsvFileHeaders(Type type)
		{
			return type.GetProperties().Select(f => new { p = f, a = f.GetCustomAttributes(true).OfType<CsvHelper.Configuration.CsvFieldAttribute>().FirstOrDefault() })
					.Where(f => f.a == null || f.a.Ignore == false).Select(f =>
					{
						string name = null;
						if (f.a == null)
						{
							name = f.p.Name;
						}
						else if (f.a.Name != null)
						{
							name = f.a.Name;
						}
						else if (f.a.Names.Any())
						{
							name = f.a.Names.First();
						}
						else
						{
							name = f.p.Name;
						}

						return name;
					});
		}
		public static IEnumerable<string> GetCsvFileHeaders<TMap>() where TMap: CsvHelper.Configuration.CsvClassMap
		{
			var conf = new CsvHelper.Configuration.CsvConfiguration();
			conf.ClassMapping<TMap>();
			return conf.Properties.Select(f => f.NameValue);
		}
		public static IEnumerable<System.Reflection.PropertyInfo> GetIncludedProperties(Type type)
		{
			return type.GetProperties().Select(f => new { p = f, a = f.GetCustomAttributes(true).OfType<CsvHelper.Configuration.CsvFieldAttribute>().FirstOrDefault() })
					.Where(f => f.a == null || f.a.Ignore == false).Select(f => f.p);
		}

		private static Random rnd = new Random();
		public static object GetRandomValue(Type type)
		{
			//bool nullable = false;
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				//nullable=true;
				type = Nullable.GetUnderlyingType(type);
			}

			if (type == typeof(int))
			{
				return rnd.Next();
			}
			else if (type == typeof(decimal))
			{
				return (decimal)(rnd.NextDouble() * 100);
			}
			else if (type == typeof(string))
			{
				var wordsCount = loremIpsum.Split(new char[] { ' ' });
				return wordsCount.Skip(rnd.Next(wordsCount.Count() - 1)).First();
			}
			else if (type == typeof(bool))
			{
				return (rnd.Next(1) == 1);
			}
			else if (type == typeof(DateTime))
			{
				return new DateTime(rnd.Next());
			}
			else
			{
				return type.ToString();
			}



		}

		public static readonly string loremIpsum = "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";

	}
}

namespace CsvHelper
{
	public static class CsvHelperExtenstions
	{
		public static string PrettyMessage(this CsvReaderException ex)
		{
			return "Invalid field value at Row: " + ex.Row + ", column name: " + ex.FieldName + ", column index: " + ex.FieldIndex + ", field value: \"" + ex.FieldValue + "\".";
		}


		public static IEnumerable<string> ColumnHeaderNames<TMap>() where TMap: Configuration.CsvClassMap
		{
			var conf = new CsvHelper.Configuration.CsvConfiguration();
			conf.ClassMapping<TMap>();
			return conf.Properties.Select(f => f.NameValue);
		}
	}

	
}