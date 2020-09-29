using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace CC.Web.Helpers
{
	public static class ValidationHelper
	{
		public static IEnumerable<ValidationResult> Validate<T>(T obj, ValidationContext validationContext)
		{
			var type = typeof(T);
			var meta = type.GetCustomAttributes(false).OfType<MetadataTypeAttribute>().FirstOrDefault();
			if (meta != null)
			{
				type = meta.MetadataClassType;
			}
			var propertyInfo = type.GetProperties();
			foreach (var info in propertyInfo)
			{
				var attributes = info.GetCustomAttributes(true).OfType<ValidationAttribute>();
				foreach (var attribute in attributes)
				{
					var objPropInfo = obj.GetType().GetProperty(info.Name);
					var value = objPropInfo.GetValue(obj,null);
					ValidationResult vr = null;
					try{
						attribute.Validate(value,info.Name);
					}
					catch(ValidationException ex)
					{
						vr= ex.ValidationResult;
					}
					if(vr!=null)
					yield return vr;

				}
			}

			var validatable = obj as IValidatableObject;
			if (validatable != null)
			{
				foreach (var v in validatable.Validate(validationContext))
				{	
					yield return v;
				}
			}
		}
	}
}