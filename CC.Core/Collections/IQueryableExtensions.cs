using System;

using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace System.Collections.Generic
{
	public static class IQueryableExtensions
	{
		/// <summary>
		/// Usage:
		/// .WhereIf(f=>f.id==paramId, paramId.HasValue)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="c"></param>
		/// <param name="expression">the collection to be filtered</param>
		/// <param name="condition">the collection is sorted if the condition evaluates to true</param>
		/// <returns></returns>
		public static IQueryable<T> WhereIf<T>(this IQueryable<T> c, Expression<Func<T, bool>> expression, bool condition)
		{
			if (condition)
				return c.Where(expression);
			else
				return c;
		}
        public static IOrderedQueryable<T> OrderByField<T>(this IQueryable<T> q, string SortField, bool Ascending)
        {
			string method = Ascending ? "OrderBy" : "OrderByDescending";
			return NewMethod<T>(q, SortField, method);
        }
		public static IOrderedQueryable<T> ThenByField<T>(this IOrderedQueryable<T> q, string SortField, bool Ascending)
		{
			string method = Ascending ? "ThenBy" : "ThenByDescending";
			return NewMethod<T>(q, SortField, method);
		}
		private static IOrderedQueryable<T> NewMethod<T>(IQueryable<T> q, string SortField, string method)
		{
			var param = Expression.Parameter(typeof(T), "p");
			var prop = Expression.Property(param, SortField);
			var exp = Expression.Lambda(prop, param);

			Type[] types = new Type[] { q.ElementType, exp.Body.Type };
			var mce = Expression.Call(typeof(Queryable), method, types, q.Expression, exp);
			return (IOrderedQueryable<T>)q.Provider.CreateQuery<T>(mce);
		}
		

		public static IOrderedQueryable<T> OrderBy<T,TKey>(this IQueryable<T> source, Expression<Func<T, TKey>> predicate, bool asc)
		{
			if (asc)
			{
				return source.OrderBy(predicate);
			}
			else
			{
				return source.OrderByDescending(predicate);
			}
		}
	}
}
