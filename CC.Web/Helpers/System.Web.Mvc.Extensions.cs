using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace System.Web.Mvc
{
	public static class SystemWebMvcExtensions
	{
		public static SelectList ToSelectList<TKey,TValue>(this IEnumerable<KeyValuePair<TKey,TValue>> c, object selectedValue)
		{
			return new SelectList(c,"Key","Value",selectedValue);
		}
	}
}