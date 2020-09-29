using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web;

namespace CC.Web.Helpers
{
	public class StringSplitModelBinder : IModelBinder
	{
		#region Implementation of IModelBinder

		public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
			var v = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
			if (v==null)
			{
				return new string[0];
			}

			string attemptedValue = v.AttemptedValue;
			if (String.IsNullOrEmpty(attemptedValue))
			{
				return new int[0];
			}
			else
			{
				var result = attemptedValue.Split(',').Select(int.Parse);
				return result.ToArray();
			}
		}

		#endregion
	}
}