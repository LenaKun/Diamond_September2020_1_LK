using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace CC.Data
{
	public partial class UnmetNeedsOther : IValidatableObject
	{
		public UnmetNeedsOther()
		{
			
		}


		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (this.Amount < 0)
				yield return new ValidationResult("Amount value must be >=0.");
		}
	}
}
