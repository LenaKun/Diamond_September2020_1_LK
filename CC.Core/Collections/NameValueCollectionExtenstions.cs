using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Collections.Specialized 
{
    public static class NameValueCollectionExtenstions
    {
        public static string ToQueryString(this NameValueCollection parameters)
        {
            var sb = new StringBuilder();

            foreach (String name in parameters)
                sb.Append(String.Concat(name, "=", System.Web.HttpUtility.UrlEncode(parameters[name]), "&"));

            if (sb.Length > 0)
                return sb.ToString(0, sb.Length - 1);

            return String.Empty;
        }

        public static IEnumerable<KeyValuePair<string, string>> ToPairs(this NameValueCollection collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }

            return collection.Cast<string>().Select(key => new KeyValuePair<string, string>(key, collection[key]));
        }

        public class NamedValue<T>
        {
            public string Name{get;set;}
            public T Value {get;set;}
        }
    }
}
