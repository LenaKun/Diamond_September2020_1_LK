using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CC.Data
{
    public partial class SupportiveCommunitiesReport : IValidatableObject
    {
		public SupportiveCommunitiesReport()
		{
		}
		public SupportiveCommunitiesReport(decimal? hoursHoldCost, int monthsCount)
		{
			this.HoursHoldCost = hoursHoldCost;
			this.MonthsCount = monthsCount;
			this.Amount = this.HoursHoldCost * this.MonthsCount;
		}
		public static decimal? PartialHouseholdCost
		{
			get
			{
				return System.Web.Configuration.WebConfigurationManager.AppSettings["SC_subsidy"].Parse<decimal>();
			}
		}
		public static int? SC_min_day
		{
			get
			{
				return System.Web.Configuration.WebConfigurationManager.AppSettings["SC_min_day"].Parse<int>() - 1;  //Min leave join
			}
		}
		public static int? SC_max_day
		{
			get
			{
				return System.Web.Configuration.WebConfigurationManager.AppSettings["SC_max_day"].Parse<int>() - 1;  //Max join date
			}
		}
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            
            if (this.HoursHoldCost==null)
            {
                yield return new ValidationResult("Hours Hold Cost can not be empty");
            }

            
        }

    }


}
