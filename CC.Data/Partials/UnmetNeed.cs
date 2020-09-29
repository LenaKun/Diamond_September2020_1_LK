using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace CC.Data
{
	public partial class UnmetNeed : IValidatableObject
	{
		public UnmetNeed()
		{
			this.StartDate = DateTime.Now.Date;
		}


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (this.WeeklyHours < 0)
                yield return new ValidationResult("Weekly Hours value must be >=0 and <=168.");
            if (this.StartDate > DateTime.Now)
                yield return new ValidationResult(string.Format("Start date must be less than today's date.", DateTime.Now.Date.ToShortDateString()));
        } 
	}
}
