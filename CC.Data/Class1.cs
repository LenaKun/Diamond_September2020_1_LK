using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Objects;
using System.Linq.Expressions;
using System.Data.Objects.DataClasses;
using System.Reflection;


namespace CC.Data
{
	public class Class1	{
		[EdmFunction("ccModel", "AddDay")]
		public static DateTime? AddDay(DateTime date)
		{
			throw new NotImplementedException("asdf");
			
		}
		[EdmFunction("ccEntities", "YearsSince")]
		public static int YearsSince(DateTime date)
		{
			throw new NotSupportedException("Direct calls are not supported.");
		}
	}

}
