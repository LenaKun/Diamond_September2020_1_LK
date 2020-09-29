using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Data;
using System.ComponentModel;

namespace System.Collections.Generic
{
    public static class IenumerableExtensions
    {
        public static List<TSource> InsertAt<TSource>(this List<TSource> c, int index, TSource entry)
        {
            c.Insert(index, entry);
            return c;
        }
        public static IEnumerable<TSource> InsertAt<TSource>(this IEnumerable<TSource> c, int index, TSource item)
        {
            IList<TSource> list = c as IList<TSource> ?? c.ToList();
            list.Insert(index, item);
            return c;
        }
        public static TSource SingleOrNull<TSource>(this IEnumerable<TSource> c, Func<TSource, bool> predicate) where TSource : class
        {
            try
            {
                return c.Single(predicate);
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }
        public static TSource SingleOrNull<TSource>(this IQueryable<TSource> c, Expression<Func<TSource, bool>> predicate) where TSource : class
        {
            try
            {
                return c.Single(predicate);
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }
        public static TSource FirstOrNull<TSource>(this IQueryable<TSource> c) where TSource : class
        {
            try
            {
                return c.First();
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }
        public static TSource FirstOrNull<TSource>(this IQueryable<TSource> c, Expression<Func<TSource, bool>> predicate) where TSource : class
        {
            try
            {
                return c.First(predicate);
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }
        public static TSource SingleOrAny<TSource>(this IQueryable<TSource> c, Expression<Func<TSource, bool>> predicate) where TSource : class

        {
            try
            {
                return c.Single(predicate);
            }
            catch (InvalidOperationException)
            {
                return c.First();
            }
        }

		
        public static DataTable ToDataTable<Tsource>(this IEnumerable<Tsource> data, string[] ingore=null)
        {
            DataTable dt = new DataTable();

            var props =TypeDescriptor.GetProperties(typeof(Tsource)).OfType<PropertyDescriptor>();

			if (ingore != null)
			{
				props = props.Where(f => !ingore.Contains(f.Name));
			}

            foreach (var property in props)
            {
                DataColumn column = new DataColumn(property.DisplayName);
                
                Type type=property.PropertyType;
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    type=Nullable.GetUnderlyingType(type);
                    column.AllowDBNull = true;
                }
                column.DataType = type;
				dt.Columns.Add(column);
            }
            foreach (var item in data)
            {
                DataRow row = dt.NewRow();
                foreach (var property in props)
                {
                    object value = property.GetValue(item) ?? DBNull.Value;
                    row[property.DisplayName] = value;
                }
                dt.Rows.Add(row);
            }

            return dt;
        }

        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> source, int chunkSize)
        {
            var chunk = new List<T>(chunkSize);
            foreach (var x in source)
            {
                chunk.Add(x);
                if (chunk.Count < chunkSize)
                {
                    continue;
                }
                yield return chunk;
                chunk = new List<T>(chunkSize);
            }
            if (chunk.Any())
            {
                yield return chunk;
            }
        }


		public static void Each<T>(this IEnumerable<T> ie, Action<T, int> action)
		{
			var i = 0;
			foreach (var e in ie) action(e, i++);
		}
    }
}
