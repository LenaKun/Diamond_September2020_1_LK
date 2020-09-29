using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace CC.Web.Models
{
	public class NullStringTypeConverter : CsvHelper.TypeConversion.ITypeConverter
	{
		public static Regex nameRgx = new Regex(@"^[A-Za-z\s'""-]*$");
		public static Regex addressRgx = new Regex(@"^[A-Za-z\s\d.#//-]*$");
		public bool CanConvertFrom(Type type)
		{
			return type == typeof(string);
		}

		public bool CanConvertTo(Type type)
		{
			throw new NotImplementedException();
		}

		public virtual object ConvertFromString(System.Globalization.CultureInfo culture, string text)
		{
			if (string.IsNullOrWhiteSpace(text)) return null;
			else
			{
				return text.Trim();
			}

		}

		public object ConvertFromString(string text)
		{
			throw new NotImplementedException();
		}

		public string ConvertToString(System.Globalization.CultureInfo culture, object value)
		{
			throw new NotImplementedException();
		}

		public string ConvertToString(object value)
		{
			throw new NotImplementedException();
		}
	}

	public class StringTypeConverter : NullStringTypeConverter
	{
		public override object ConvertFromString(System.Globalization.CultureInfo culture, string text)
		{
			var s = (string)base.ConvertFromString(culture, text);

			if (s != null)
			{
				//must match the regex in the client metadata
				if (s.Select(c => (int)c).Any(i => i > 125 || i < 32))
				{
					throw new ArgumentException("Invalid characters.");
				}
			}

			return s;
		}

	}

	public class NullBoolTypeConverter : CsvHelper.TypeConversion.ITypeConverter
	{
		public bool CanConvertFrom(Type type)
		{
			return type == typeof(string);
		}

		public bool CanConvertTo(Type type)
		{
			throw new NotImplementedException();
		}

		public virtual object ConvertFromString(System.Globalization.CultureInfo culture, string text)
		{
			if (string.IsNullOrWhiteSpace(text)) return null;
			else
			{
				bool result = false;
				if(bool.TryParse(text, out result))
				{
					return result;
				}
				return null;
			}

		}

		public object ConvertFromString(string text)
		{
			throw new NotImplementedException();
		}

		public string ConvertToString(System.Globalization.CultureInfo culture, object value)
		{
			throw new NotImplementedException();
		}

		public string ConvertToString(object value)
		{
			throw new NotImplementedException();
		}
	}

	public class NameRegexStringConverter : NullStringTypeConverter
	{
		public override object ConvertFromString(System.Globalization.CultureInfo culture, string text)
		{
			var s = (string)base.ConvertFromString(culture, text);

			if (s != null)
			{
				//must match the regex in the client metadata
				if (s.Select(c => (int)c).Any(i => i > 125 || i < 32))
				{
					throw new ArgumentException("Invalid characters.");
				}

				if (!nameRgx.IsMatch(s))
				{
					throw new ArgumentException("First Name and Last Name can only contain letters (A-Z a-z), spaces, ', - and \".");
				}
			}			

			return s;
		}
	}

	public class AddressRegexStringConverter : NullStringTypeConverter
	{
		public override object ConvertFromString(System.Globalization.CultureInfo culture, string text)
		{
			var s = (string)base.ConvertFromString(culture, text);

			if (s != null)
			{
				//must match the regex in the client metadata
				if (s.Select(c => (int)c).Any(i => i > 125 || i < 32))
				{
					throw new ArgumentException("Invalid characters.");
				}

				if (!addressRgx.IsMatch(s))
				{
					throw new ArgumentException("Address can only contain letters (A-Z a-z), numbers, spaces, -, ., # and /.");
				}
			}			

			return s;
		}
	}

	class StateIdTypeConverter : CsvHelper.TypeConversion.Int32Converter
	{
        private List<CC.Data.State> states;
        public StateIdTypeConverter()
        {
            using (var db = new CC.Data.ccEntities())
            {
                states = db.States.ToList();
            }
        }
		public override object ConvertFromString(System.Globalization.CultureInfo culture, string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return null;
			}
			else
			{
                CC.Data.State state;
                if (HttpContext.Current == null)
                {
                    state = states.Where(s => s.Code.Equals(text, StringComparison.InvariantCultureIgnoreCase)).SingleOrDefault();
                }
                else
                {
                    state = Cache.GetCachedList<CC.Data.State>().Where(s => s.Code.Equals(text, StringComparison.InvariantCultureIgnoreCase)).SingleOrDefault();
                }
				if (state != null)
				{
					return state.Id;
				}
			}

			return null;
		}

	}	

	class AgencyIdTypeConverter : CsvHelper.TypeConversion.Int32Converter
	{
		private HashSet<int> ids;
		public AgencyIdTypeConverter()
		{
			using (var db = new CC.Data.ccEntities())
			{
				if (System.Web.HttpContext.Current != null)
				{
					var username = System.Web.HttpContext.Current.User.Identity.Name;
					var permissions = CC.Data.Services.PermissionsFactory.GetPermissionsFor(username);
					

					if (permissions != null)
					{
						var ags = db.Agencies.Where(permissions.AgencyFilter).ToList();
						this.ids = new HashSet<int>(ags.Select(f => f.Id));
					}
				}
                else
                {
                    var permissions = CC.Data.Services.PermissionsFactory.GetPermissionsFor("sysadmin");

                    if (permissions != null)
                    {
                        var ags = db.Agencies.Where(permissions.AgencyFilter).ToList();
                        this.ids = new HashSet<int>(ags.Select(f => f.Id));
                    }
                }
			}

		}
		public override object ConvertFromString(System.Globalization.CultureInfo culture, string text)
		{
			int tmp;
			if (string.IsNullOrEmpty(text))
			{
				return null;
			}
			else if (int.TryParse(text, out tmp))
			{
				if (ids.Contains(tmp))
				{
					return tmp;
				}
			}

			return null;
		}

	}

	class NationalIdTypeConverter : CsvHelper.TypeConversion.Int32Converter
	{
        private List<CC.Data.NationalIdType> nationalidtypes;
        public NationalIdTypeConverter()
        {
            using (var db = new CC.Data.ccEntities())
            {
                nationalidtypes = db.NationalIdTypes.ToList();
            }
        }
		public override object ConvertFromString(System.Globalization.CultureInfo culture, string text)
		{
			int tmp;

			if (string.IsNullOrEmpty(text))
			{
				return null;
			}
			else if (int.TryParse(text, out tmp))
			{
                CC.Data.NationalIdType obj;
                if (HttpContext.Current == null)
                {
                    obj = nationalidtypes.Where(s => s.Id == tmp).SingleOrDefault();
                }
                else
                {
                    obj = Cache.GetCachedList<CC.Data.NationalIdType>().Where(s => s.Id == tmp).SingleOrDefault();
                }
				if (obj != null)
				{
					return obj.Id;
				}
			}
			else
			{
                CC.Data.NationalIdType obj;
                if (HttpContext.Current == null)
                {
                    obj = nationalidtypes.Where(s => s.Name.Equals(text, StringComparison.InvariantCultureIgnoreCase)).SingleOrDefault();
                }
                else
                {
                    obj = Cache.GetCachedList<CC.Data.NationalIdType>().Where(s => s.Name.Equals(text, StringComparison.InvariantCultureIgnoreCase)).SingleOrDefault();
                }
				if (obj != null)
				{
					return obj.Id;
				}
			}
			return null;
		}
	}

	class LeaveReasonIdConverter : CsvHelper.TypeConversion.Int32Converter
	{
        private List<CC.Data.LeaveReason> leavereasons;
        public LeaveReasonIdConverter()
        {
            using (var db = new CC.Data.ccEntities())
            {
                leavereasons = db.LeaveReasons.ToList();
            }
        }
		public override object ConvertFromString(System.Globalization.CultureInfo culture, string text)
		{
			int tmp;

			if (string.IsNullOrEmpty(text))
			{
				return null;
			}
			else if (int.TryParse(text, out tmp))
			{
                CC.Data.LeaveReason obj;
                if (HttpContext.Current == null)
                {
                    obj = leavereasons.Where(s => s.Id == tmp).SingleOrDefault();
                }
                else
                {
                    obj = Cache.GetCachedList<CC.Data.LeaveReason>().Where(s => s.Id == tmp).SingleOrDefault();
                }
				if (obj != null)
				{
					return obj.Id;
				}
			}
			else
			{
                CC.Data.LeaveReason obj;
                if (HttpContext.Current == null)
                {
                    obj = leavereasons.Where(s => s.Name.Equals(text, StringComparison.InvariantCultureIgnoreCase)).SingleOrDefault();
                }
                else
                {
                    obj = Cache.GetCachedList<CC.Data.LeaveReason>().Where(s => s.Name.Equals(text, StringComparison.InvariantCultureIgnoreCase)).SingleOrDefault();
                }
				if (obj != null)
				{
					return obj.Id;
				}
			}

			return null;
		}
	}

	class BirthCountryIdConverter : CsvHelper.TypeConversion.Int32Converter
	{
        private List<CC.Data.BirthCountry> birthcountries;
        public BirthCountryIdConverter()
        {
            using (var db = new CC.Data.ccEntities())
            {
                birthcountries = db.BirthCountries.ToList();
            }
        }
		public override object ConvertFromString(System.Globalization.CultureInfo culture, string text)
		{
			int tmp;

			if (int.TryParse(text, out tmp))
			{
                CC.Data.BirthCountry obj;
                if (HttpContext.Current == null)
                {
                    obj = birthcountries.Where(s => s.Id == tmp).SingleOrDefault();
                }
                else
                {
                    obj = Cache.GetCachedList<CC.Data.BirthCountry>().Where(s => s.Id == tmp).SingleOrDefault();
                }
				if (obj != null)
				{
					return obj.Id;
				}
			}
			else
			{
                CC.Data.BirthCountry obj;
                if (HttpContext.Current == null)
                {
                    obj = birthcountries.Where(s => s.Name.Equals(text, StringComparison.InvariantCultureIgnoreCase)).SingleOrDefault();
                }
                else
                {
                    obj = Cache.GetCachedList<CC.Data.BirthCountry>().Where(s => s.Name.Equals(text, StringComparison.InvariantCultureIgnoreCase)).SingleOrDefault();
                }
				if (obj != null)
				{
					return obj.Id;
				}
			}

			return null;
		}
	}

    class CountryIdConverter : CsvHelper.TypeConversion.Int32Converter
    {
        private List<CC.Data.Country> countries;
        public CountryIdConverter()
        {
            using (var db = new CC.Data.ccEntities())
            {
                countries = db.Countries.ToList();
            }
        }
        public override object ConvertFromString(System.Globalization.CultureInfo culture, string text)
        {
            int tmp;

            if (int.TryParse(text, out tmp))
            {
                CC.Data.Country obj;
                if (HttpContext.Current == null)
                {
                    obj = countries.Where(s => s.Id == tmp).SingleOrDefault();
                }
                else
                {
                    obj = Cache.GetCachedList<CC.Data.Country>().Where(s => s.Id == tmp).SingleOrDefault();
                }
                if (obj != null)
                {
                    return obj.Id;
                }
            }
            else
            {
                CC.Data.Country obj;
                if (HttpContext.Current == null)
                {
                    obj = countries.Where(s => s.Name.Equals(text, StringComparison.InvariantCultureIgnoreCase)).SingleOrDefault();
                }
                else
                {
                    obj = Cache.GetCachedList<CC.Data.Country>().Where(s => s.Name.Equals(text, StringComparison.InvariantCultureIgnoreCase)).SingleOrDefault();
                }
                if (obj != null)
                {
                    return obj.Id;
                }
            }

            return null;
        }
    }


	/// <summary>
	/// Convert from string using culture invariant formats.
	/// Intended to prevent confusion between dd/MM and MM/dd.
	/// </summary>
	public class InvariantDateTypeConverter : CsvHelper.TypeConversion.ITypeConverter
	{
		public InvariantDateTypeConverter()
			: base()
		{
			formats = new string[] { "dd MMM yyyy", "dd-MMM-yyyy", "d MMM yyyy", "d-MMM-yyyy", "yyyy-MM-dd" };
		}
		protected string[] formats = null;

		public virtual object ConvertFromString(string text)
		{
			return ConvertFromString(System.Globalization.CultureInfo.InvariantCulture, text);
		}

		public virtual object ConvertFromString(System.Globalization.CultureInfo culture, string text)
		{
			DateTime tmp;
			if (string.IsNullOrEmpty(text))
			{
				return null;
			}
			else if (DateTime.TryParseExact(text, formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out tmp))
			{
				return tmp.Date;
			}
			else
			{
				throw new NotSupportedException("Value " + text + " can not be converted");
			}
		}

		public bool CanConvertFrom(Type type)
		{
			return type == typeof(string);
		}

		public bool CanConvertTo(Type type)
		{
			throw new NotImplementedException();
		}

		public string ConvertToString(System.Globalization.CultureInfo culture, object value)
		{
			throw new NotImplementedException();
		}

		public string ConvertToString(object value)
		{
			throw new NotImplementedException();
		}
	}
	public class InvariantMonthTypeConverter : InvariantDateTypeConverter
	{
		public InvariantMonthTypeConverter()
			: base()
		{
			formats = new string[] { "MMM-yyyy", "MMM yyyy", "yyyy-MM" };
		}

		/// <summary>
		/// Set the date to the 1st day of the mmonth if it the text was converted to datetime
		/// </summary>
		/// <param name="culture"></param>
		/// <param name="text"></param>
		/// <returns></returns>
		public override object ConvertFromString(System.Globalization.CultureInfo culture, string text)
		{
			var converted = base.ConvertFromString(culture, text);
			if (converted is DateTime)
			{
				var dt = (DateTime)converted;
				return new DateTime(dt.Year, dt.Month, 1);
			}
			else
			{
				return converted;
			}
		}
	}

	public class GenderConverter : NullStringTypeConverter 
	{
		public override object ConvertFromString(System.Globalization.CultureInfo culture, string text)
		{
			CC.Data.Client.Genders result;
			if (Enum.TryParse(text, true, out result))
			{
				return (int)result;
			}
			else
			{
				return null;
			}
		}
	}

	public class CommPrefConverter : CsvHelper.TypeConversion.Int32Converter
	{
		private List<CC.Data.CommunicationsPreference> commprefs;
		public CommPrefConverter()
		{
			using (var db = new CC.Data.ccEntities())
			{
				commprefs = db.CommunicationsPreferences.ToList();
			}
		}
		public override object ConvertFromString(System.Globalization.CultureInfo culture, string text)
		{
			int tmp;

			if (string.IsNullOrEmpty(text))
			{
				return null;
			}
			else if (int.TryParse(text, out tmp))
			{
				CC.Data.CommunicationsPreference obj;
				if (HttpContext.Current == null)
				{
					obj = commprefs.Where(s => s.Id == tmp).SingleOrDefault();
				}
				else
				{
					obj = Cache.GetCachedList<CC.Data.CommunicationsPreference>().Where(s => s.Id == tmp).SingleOrDefault();
				}
				if (obj != null)
				{
					return obj.Id;
				}
			}
			else
			{
				CC.Data.CommunicationsPreference obj;
				if (HttpContext.Current == null)
				{
					obj = commprefs.Where(s => s.Name.Equals(text, StringComparison.InvariantCultureIgnoreCase)).SingleOrDefault();
				}
				else
				{
					obj = Cache.GetCachedList<CC.Data.CommunicationsPreference>().Where(s => s.Name.Equals(text, StringComparison.InvariantCultureIgnoreCase)).SingleOrDefault();
				}
				if (obj != null)
				{
					return obj.Id;
				}
			}

			return null;
		}
	}

	public class CareReceivedConverter : CsvHelper.TypeConversion.Int32Converter
	{
		private List<CC.Data.CareReceivingOption> careoptions;
		public CareReceivedConverter()
		{
			using (var db = new CC.Data.ccEntities())
			{
				careoptions = db.CareReceivingOptions.ToList();
			}
		}
		public override object ConvertFromString(System.Globalization.CultureInfo culture, string text)
		{
			int tmp;

			if (string.IsNullOrEmpty(text))
			{
				return null;
			}
			else if (int.TryParse(text, out tmp))
			{
				CC.Data.CareReceivingOption obj;
				if (HttpContext.Current == null)
				{
					obj = careoptions.Where(s => s.Id == tmp).SingleOrDefault();
				}
				else
				{
					obj = Cache.GetCachedList<CC.Data.CareReceivingOption>().Where(s => s.Id == tmp).SingleOrDefault();
				}
				if (obj != null)
				{
					return obj.Id;
				}
			}
			else
			{
				CC.Data.CareReceivingOption obj;
				if (HttpContext.Current == null)
				{
					obj = careoptions.Where(s => s.Name.Equals(text, StringComparison.InvariantCultureIgnoreCase)).SingleOrDefault();
				}
				else
				{
					obj = Cache.GetCachedList<CC.Data.CareReceivingOption>().Where(s => s.Name.Equals(text, StringComparison.InvariantCultureIgnoreCase)).SingleOrDefault();
				}
				if (obj != null)
				{
					return obj.Id;
				}
			}

			return null;
		}
	}
}
