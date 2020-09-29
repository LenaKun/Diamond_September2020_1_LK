using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Runtime;
using System.Reflection;
using System.Web.Compilation;

namespace CC.Web.Models
{
	public class CustomBinderAttribute : CustomModelBinderAttribute
	{
		private Type _binderType;
		public CustomBinderAttribute(Type binderType)
		{
			if (typeof(IModelBinder).IsAssignableFrom(binderType))
			{
				_binderType = binderType;
			}
			else
			{
				throw new ArgumentException();
			}
		}
		public override IModelBinder GetBinder()
		{
			return (IModelBinder)System.Activator.CreateInstance(_binderType);
		}
	}

	public class jQueryDataTableParamModelBinder : DefaultModelBinder
	{
		public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
			var result = (jQueryDataTableParamModel)base.BindModel(controllerContext, bindingContext);
			if (result != null)
			{
				var mDataProp=bindingContext.ValueProvider.GetValue("mDataProp_" + result.iSortCol_0);
				if (mDataProp != null)
				{
					result.sSortCol_0 = Convert.ToString(mDataProp.AttemptedValue);
				}
			}
			return result;
		}

	}
}