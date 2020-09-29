using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
	public static class DateTimeExtensions
	{
		public static string ToDateString(this DateTime date)
		{
			return date.ToString("dd-MMM-yyy");
		}
		public static string ToMonthString(this DateTime date)
		{
			return date.ToString("MMM yyyy");
		}

		public static int MonthsTo(this DateTime start, DateTime end)
		{
			return (end.Year * 12 + end.Month) - (start.Year * 12 + start.Month);
		}
		public static DateTime WeekStart(this DateTime start, DayOfWeek weekStart)
		{
			var result = start.AddDays(
				weekStart - start.DayOfWeek

				);
			if (start.DayOfWeek < weekStart)
			{
				result = result.AddDays(-7);
			}
			return result;
		}
	}

	public static class EnumExtensions
	{
		public static string DisplayName(this Enum value)
		{
			string result = value.ToString();
			Type enumType = value.GetType();
			var enumValue = Enum.GetName(enumType, value);
			var members = enumType.GetMember(enumValue);
			if (members.Any())
			{
				MemberInfo member = members.First();

				result = member.Name;
				var attrs = member.GetCustomAttributes(typeof(DisplayAttribute), false);
				if (attrs.Any())
				{
					result = ((DisplayAttribute)attrs[0]).Name;

					if (((DisplayAttribute)attrs[0]).ResourceType != null)
					{
						result = ((DisplayAttribute)attrs[0]).GetName();
					}
				}
			}
			return result;
		}

		public static string DisplayName<TEnum>(this int value)
		{
			Type enumType = typeof(TEnum);
			string result = Enum.GetName(typeof(TEnum), value);
			var enumValue = Enum.GetName(enumType, value);
			var members = enumType.GetMember(enumValue);
			if (members.Any())
			{
				MemberInfo member = members.First();

				result = member.Name;
				var attrs = member.GetCustomAttributes(typeof(DisplayAttribute), false);
				if (attrs.Any())
				{
					result = ((DisplayAttribute)attrs[0]).Name;

					if (((DisplayAttribute)attrs[0]).ResourceType != null)
					{
						result = ((DisplayAttribute)attrs[0]).GetName();
					}
				}
			}
			return result;
		}

		public static List<KeyValuePair<int?, string>> EnumToDictionary<TEnum>()
		{
			var members = typeof(TEnum).GetMembers().Select(f => new
			{
				Name = f.Name,
				Display = f.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.DisplayAttribute), false).OfType<System.ComponentModel.DataAnnotations.DisplayAttribute>().FirstOrDefault()
			})
			.Where(f => f.Display != null)
			.Select(f => new
			{
				Name = f.Name,
				DisplayName = f.Display.Name
			});


			var values = from int e in Enum.GetValues(typeof(TEnum))

						 select new { Id = e, Name = Enum.GetName(typeof(TEnum), e).ToString() };

			var q = from e in values
					join m in members on e.Name equals m.Name into mg
					from m in mg.DefaultIfEmpty()
					select new { Id = e.Id, Name = m == null ? e.Name : m.DisplayName };

			var result = q.Select(f => new KeyValuePair<int?, string>(f.Id, f.Name)).ToList();

			return result;
		}

		public static System.Web.Mvc.SelectList ToSelectList<TEnum>(object enumObj)
		{
			var values = EnumToDictionary<TEnum>();

			return new System.Web.Mvc.SelectList(values, "Key", "Value", enumObj);
		}
		public static System.Web.Mvc.SelectList ToSelectList<TEnum>()
		{
			var values = EnumToDictionary<TEnum>();

			return new System.Web.Mvc.SelectList(values, "Key", "Value");
		}
	}

	public static class StringExtenstions
	{
		/// <summary>
		/// Split a string on each occurrence of a capital (assumed to be a word)
		/// e.g. MyBigToe returns "My Big Toe"
		/// </summary>
		public static string SplitCapitalizedWords(this string source)
		{
			if (string.IsNullOrEmpty(source)) return source;
			var newText = new StringBuilder(source.Length * 2);
			newText.Append(source[0]);
			for (int i = 1; i < source.Length; i++)
			{
				if (char.IsUpper(source[i]))
					newText.Append(' ');
				newText.Append(source[i]);
			}
			return newText.ToString();
		}
		public static string NullIfEmpty(this string source)
		{
			if (string.IsNullOrEmpty(source))
			{
				return null;
			}
			else
			{
				return source;
			}
		}
		public static string NullIfEmptyOrWhiteSpace(this string source)
		{
			if (string.IsNullOrWhiteSpace(source))
			{
				return null;
			}
			else
			{
				return source;
			}
		}
		public static bool IsNullOrEmptyHtml(this string source)
		{
			if (source != null)
			{
				source = System.Web.HttpUtility.HtmlDecode(System.Text.RegularExpressions.Regex.Replace(source, @"<[^>]*>", String.Empty));
			}
			return string.IsNullOrWhiteSpace(source);
		}
	}

	public static class VersionExtensions
	{
		public static DateTime BuildDate(this Version v)
		{
			var d = new DateTime(2000, 1, 1);
			return d.AddDays(v.Build).AddSeconds(v.Revision * 2);
		}
	}


}
