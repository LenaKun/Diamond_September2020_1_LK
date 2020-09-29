using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace CC.Data
{
	public partial class GrandfatherHour : IValidatableObject
	{
		public GrandfatherHour()
		{
			this.StartDate = DateTime.Now;
			this.UpdatedAt = DateTime.Now;
		}


		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (this.Value < 0 || this.Value > 168)
				yield return new ValidationResult("Grandfather Hours value must be between 0 and 168.");
		}
	}
}
