using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace CC.Web.Helpers
{
	public static class GlobalDbSettings
	{
		private static System.Web.Caching.Cache cache = new System.Web.Caching.Cache();

		private static System.Runtime.Caching.MemoryCache x = new System.Runtime.Caching.MemoryCache("GlobalsDbStrings");

		public static string GetString(string name)
		{
			string result = null;
			if (x.Contains(name))
			{
				var obj = x[name];

				result = (string)(obj == DBNull.Value ? null : obj);
			}
			else
			{
				using (var db = new CC.Data.ccEntities())
				{
					try
					{
						var item = db.GlobalStrings.SingleOrDefault(f => f.Name == name);
						if (item != null)
						{
							result = item.Value;
						}

					}
					catch (Exception)
					{
						return name + " - value not found";
					}
				}
				if (result == null)
				{
					x.Set(name, DBNull.Value, new System.Runtime.Caching.CacheItemPolicy());
				}
				else
				{
					x.Set(name, result, new System.Runtime.Caching.CacheItemPolicy());
				}
			}
			return result;
		}
		public static void SetString(string name, string value)
		{
			using (var db = new CC.Data.ccEntities())
			{
				try
				{
					var item = db.GlobalStrings.SingleOrDefault(f => f.Name == name);
					if (item == null && value != null)
					{
						item = new CC.Data.GlobalString
						{
							Name = name,
							Value = value
						};
						db.GlobalStrings.AddObject(item);
					}
					else if (item != null && value == null)
					{
						db.GlobalStrings.DeleteObject(item);
					}
					else if (item != null && value != null)
					{
						item.Value = value;
					}
				}
				catch
				{

				}
				try
				{
					db.SaveChanges();
					if (value == null)
					{
						x.Set(name, DBNull.Value, new System.Runtime.Caching.CacheItemPolicy());
					}
					else
					{
						x.Set(name, value, new System.Runtime.Caching.CacheItemPolicy());
					}
				}
				catch (Exception)
				{
					throw;
				}
			}
		}
		public static string GetString(GlobalStringNames name)
		{
			return GetString(name.ToString());
		}
		public static void SetString(GlobalStringNames name, string value)
		{
			SetString(name.ToString(), value);
		}
		public static Nullable<T> Get<T>(GlobalStringNames name) where T : struct
		{
			var s = GetString(name.ToString());

			if (string.IsNullOrEmpty(s))
			{
				return null;
			}
			else
			{
				var converter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(T));
				Nullable<T> result = null;
				try
				{
					result = (T)converter.ConvertFromInvariantString(s);
				}
				catch (Exception)
				{
					throw;
				}
				return result;
			}
		}
		public static void Set(GlobalStringNames name, object value)
		{
			if (value == null)
			{
				SetString(name, null);
			}
			else
			{
				var converter = System.ComponentModel.TypeDescriptor.GetConverter(value);
				var invariantString = converter.ConvertToInvariantString(value);
				SetString(name, invariantString);
			}
		}

		public enum GlobalStringNames
		{
			NewSerOrgNotifyEmail,
			CfsDailyDigestLastDateTime,
			ExportCfsClientRecordsDateTime,
			LandingPageMessage,
            CfsDailyDigestFireHour
		}
	}
}