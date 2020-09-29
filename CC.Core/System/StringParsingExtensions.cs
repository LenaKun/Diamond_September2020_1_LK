using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace System
{
	public static class StringParsingExtensions
	{
		public static Nullable<T> Parse<T>(this string input) where T : struct
		{
			var converter = TypeDescriptor.GetConverter(typeof(T));
			if (converter == null) return null;
			if (input == null) return null;

			try
			{
				return (T)converter.ConvertFromString(input);
			}
			catch (Exception)
			{
				return null;
			}

		}
		public static IEnumerable<int> ParseCsvIntegers(this string input)
		{
			if (string.IsNullOrEmpty(input))
			{
				return new int[0];
			}
			else
			{
				try
				{
					return input.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(f => int.Parse(f)).ToList();
				}
				catch
				{
					return new int[0];
				}
			}
		}

		public static object GetProp(this object obj, string propName)
		{
			var prop = TypeDescriptor.GetProperties(obj).Cast<PropertyDescriptor>().Single(f => f.Name == propName);
			return prop.GetValue(obj);
		}

		public static void SetProp(this object obj, string propName, object value)
		{
			var prop = TypeDescriptor.GetProperties(obj).Cast<PropertyDescriptor>().Single(f => f.Name == propName);
			prop.SetValue(obj, value);
		}
		public static void SetProp(this object obj, string propName, string value)
		{
			var prop = TypeDescriptor.GetProperties(obj).Cast<PropertyDescriptor>().Single(f => f.Name == propName);
			var converter = TypeDescriptor.GetConverter(prop.PropertyType);

			prop.SetValue(obj, converter.ConvertFromString(value));
		}
		public static bool TrySetProp(this object obj, string propName, string value)
		{

			try
			{
				var prop = TypeDescriptor.GetProperties(obj).Cast<PropertyDescriptor>().Single(f => f.Name == propName);
				var converter = TypeDescriptor.GetConverter(prop.GetType());

				prop.SetValue(obj, converter.ConvertFromString(value));
				return true;
			}
			catch (ArgumentNullException)
			{
				return false;
			}
			catch (InvalidOperationException)
			{
				return false;
			}
			catch (NotSupportedException)
			{
				return false;
			}
		}

	}

	public static class BoolExtensions
	{
		public static string ToYesNo(this bool b)
		{
			return b == true ? "Yes" : "No";
		}
		public static string ToYesNo(this Nullable<bool> b)
		{
			return b == true ? "Yes" : "No";
		}
	}
	
}
