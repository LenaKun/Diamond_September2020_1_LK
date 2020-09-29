using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace CC.Data
{
	[MetadataType(typeof(MetaData.HomeCareEntitledPeriodMetaData))]
	partial class HomeCareEntitledPeriod : IValidatableObject
	{
		public HomeCareEntitledPeriod()
		{

		}

		[DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
		public DateTime? EndDateDisplay
		{
			get
			{
				return this.EndDate.HasValue ? this.EndDate.Value.AddDays(-1) : (DateTime?)null;
			}
			set { this.EndDate = value.HasValue ? value.Value.AddDays(1) : (DateTime?)null; }
		}

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			using (var db = new ccEntities())
			{
                

				//validate start!=end
				if (this.EndDate.HasValue && this.EndDate <= this.StartDate)
				{
					yield return new ValidationResult("The End Date must be greater than the Start date.");
				}

                //validate start <= today
                if (this.StartDate > DateTime.Now)
                {
                    yield return new ValidationResult(string.Format("Start date must be less than or equal to {0}.", DateTime.Now.Date.ToShortDateString()));
                }

				//validate that there are no overlapped periods
				var overlaped = db.HomeCareEntitledPeriods
					.Where(overlappCheckExpression(this.StartDate, this.EndDate))
					.Where(c => c.ClientId == this.ClientId && c.Id != this.Id);
				if (overlaped.Count() > 0)
				{

					yield return new ValidationResult("There is a duplicate period.");
				}

			}


		}

		private Expression<Func<HomeCareEntitledPeriod, bool>> overlappCheckExpression(DateTime startDate, DateTime? endDate)
		{

			return (f) =>
				((f.EndDate == null || startDate < f.EndDate) && (endDate == null || f.StartDate < endDate))
			;
		}

		
	}
}
