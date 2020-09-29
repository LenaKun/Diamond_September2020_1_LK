using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace System.ComponentModel.DataAnnotations
{

	public class EnumValidatorAttribute:ValidationAttribute
	{
		public Type EnumType{get;set;}
		public EnumValidatorAttribute(Type enumType)
		{
			this.EnumType=EnumType;
		}
		public override bool IsValid(object value)
		{
			return Enum.IsDefined(this.EnumType,value);
		}
	}

}
