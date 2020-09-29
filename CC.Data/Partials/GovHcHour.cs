using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace CC.Data
{
	public partial class GovHcHour:IValidatableObject
	{
		public GovHcHour()
		{
			this.StartDate = DateTime.Now.Date;
		}


		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (this.Value < 0)
				yield return new ValidationResult("Gov HC Hours value must be greatert or equal to 0.");
			if(this.StartDate > DateTime.Now)
				yield return new ValidationResult(string.Format("Start date must be less than or equal to {0}.", DateTime.Now.Date.ToShortDateString()));
		}
	}
}
