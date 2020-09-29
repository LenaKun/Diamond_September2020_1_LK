using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Objects;

namespace CC.Web.Models
{
	public class Cache
	{
		public static List<T> GetCachedList<T>() where T : class
		{
			List<T> result = null;
			string name = typeof(T).ToString();
			var cache = HttpContext.Current.Cache;
			if (cache[name] == null)
			{
				using (var db = new CC.Data.ccEntities())
				{
					db.ContextOptions.LazyLoadingEnabled = false;
					db.ContextOptions.ProxyCreationEnabled = false;

					var objectset = db.CreateObjectSet<T>();
					result = objectset.ToList();
					cache[name] = result;
				}
			}
			else
			{
				result = (List<T>)cache[name];
			}
			return result;
		}
	}
}