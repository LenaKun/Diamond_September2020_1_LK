using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Collections.Generic
{
	public static class IEnumerableExtensions
	{
		public static IEnumerable<IEnumerable<T>> Partition<T>(this IEnumerable<T> c, int count)
		{

			var size = (int)Math.Ceiling(c.Count() / (decimal)count);


			var res = new List<T>();
			foreach (T item in c)
			{
				res.Add(item);
				if(res.Count==size)
				{
					yield return res;
					res.Clear();
				}
			}
			yield return res;

		}
	}
}
