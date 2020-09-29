using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.ComponentModel;

namespace CC.Extensions.Reflection
{
    public static class ReflectionExtensions
    {
        public static PropertyInfo GetPropertyInfo<TSource,TTarget>(
        TSource source,
        Expression<Func<TSource, TTarget>> propertyLambda)
        {
            Type type = typeof(TSource);

            MemberExpression member = propertyLambda.Body as MemberExpression;
            
            if (member == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a method, not a property.",
                    propertyLambda.ToString()));

            PropertyInfo propInfo = member.Member as PropertyInfo;
            
            if (propInfo == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a field, not a property.",
                    propertyLambda.ToString()));

            if (type != propInfo.ReflectedType &&
                !type.IsSubclassOf(propInfo.ReflectedType))
                throw new ArgumentException(string.Format(
                    "Expresion '{0}' refers to a property that is not from type {1}.",
                    propertyLambda.ToString(),
                    type));

            return propInfo;
        }
		public static PropertyInfo GetPropertyInfo<TSource, TTarget>(
		
		Expression<Func<TSource, TTarget>> propertyLambda)
		{
			Type type = typeof(TSource);

			MemberExpression member = propertyLambda.Body as MemberExpression;

			if (member == null)
				throw new ArgumentException(string.Format(
					"Expression '{0}' refers to a method, not a property.",
					propertyLambda.ToString()));

			PropertyInfo propInfo = member.Member as PropertyInfo;

			if (propInfo == null)
				throw new ArgumentException(string.Format(
					"Expression '{0}' refers to a field, not a property.",
					propertyLambda.ToString()));

			if (type != propInfo.ReflectedType &&
				!type.IsSubclassOf(propInfo.ReflectedType))
				throw new ArgumentException(string.Format(
					"Expresion '{0}' refers to a property that is not from type {1}.",
					propertyLambda.ToString(),
					type));

			return propInfo;
		}
		public static string GetPropertyName<TSource, TTarget>(
		
		Expression<Func<TSource, TTarget>> propertyLambda)
		{
			Type type = typeof(TSource);

			MemberExpression member = propertyLambda.Body as MemberExpression;

			if (member == null)
				throw new ArgumentException(string.Format(
					"Expression '{0}' refers to a method, not a property.",
					propertyLambda.ToString()));

			PropertyInfo propInfo = member.Member as PropertyInfo;

			if (propInfo == null)
				throw new ArgumentException(string.Format(
					"Expression '{0}' refers to a field, not a property.",
					propertyLambda.ToString()));

			if (type != propInfo.ReflectedType &&
				!type.IsSubclassOf(propInfo.ReflectedType))
				throw new ArgumentException(string.Format(
					"Expresion '{0}' refers to a property that is not from type {1}.",
					propertyLambda.ToString(),
					type));

			return propInfo.Name;
		}
    }
}

namespace System
{
    public static class ObjectExtensions
    {
        public static PropertyInfo GetPropertyInfo<TSource,TTarget>(this object obj, Expression<Func<TSource, TTarget>> expression)
        {
            return CC.Extensions.Reflection.ReflectionExtensions.GetPropertyInfo<TSource,TTarget>((TSource)obj, expression);
        }
		public static string PropertyName<T>(this T obj, Expression<Func<T,object>> expression)
		{
			return CC.Extensions.Reflection.ReflectionExtensions.GetPropertyInfo<T,object>(obj,expression).Name;
		}
		public static string DisplayName(this PropertyInfo prop)
		{
			if (prop.IsDefined(typeof(DisplayNameAttribute), false))
			{
				return prop.GetCustomAttributes(false).OfType<DisplayNameAttribute>().First().DisplayName;
			}
			else if (prop.IsDefined(typeof(System.ComponentModel.DataAnnotations.DisplayAttribute), false))
			{
				return prop.GetCustomAttributes(false).OfType<System.ComponentModel.DataAnnotations.DisplayAttribute>().First().Name;
			}
			else
			{
				return prop.Name;
			}
		}
    }
}
