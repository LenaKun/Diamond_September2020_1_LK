using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CC.Data
{
	[MetadataType(typeof(CC.Data.MetaData.AppMetaData))]
	public partial class App : IValidatableObject
	{
		public App()
		{
			this.CalendaricYear = DateTime.Now.Year;
			this.OtherServicesMax = 100;
			this.HomecareMin = 0;
			this.AdminMax = 100;
			this.AgencyContribution = true;
		}

		public DateTime EndDateDisplay { get { return this.EndDate.AddDays(-1); } set { this.EndDate = value.AddDays(1); } }
		[Display(Name = "Calendaric Year")]

		public int CalendaricYear
		{
			get
			{
				return this.StartDate.Year;
			}
			set
			{
				this.StartDate = new DateTime(value, 1, 1);
				this.EndDate = this.StartDate.AddYears(1);
			}
		}


		public decimal? GetExchangeRate(string curId)
		{
			var dbrate = this.AppExchangeRates.SingleOrDefault(f => f.CurId == curId);
			if (dbrate == null) return null;
			else return dbrate.Value;
		}
		public string GetExchangeRateString(string curId)
		{
			var v = GetExchangeRate(curId);
			return v.HasValue ? v.Value.Format() : null;

		}

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (this.StartDate.Day != 1 || this.StartDate.Month != 1 || (this.EndDate.Year - this.StartDate.Year) != 1)
			{
				yield return new ValidationResult("Apps will have to be define for a calendaric year only.");
			}
			if (this.StartDate.Day + this.StartDate.Month != 2)
			{
				yield return new ValidationResult("Start Date should be 1st day of actual year");
			}
			if (this.StartDate >= this.EndDate)
			{
				yield return new ValidationResult("End Date must be greater than the StartDate");
			}
			if (this.OtherServicesMax < 0 || this.OtherServicesMax > 100)
			{
				yield return new ValidationResult("Other Services Max (%) must be between 0 to 100");
			}
			if (this.HomecareMin < 0 || this.HomecareMin > 100)
			{
				yield return new ValidationResult("Homecare Min (%) must be between 0 to 100");
			}
            if (this.RequiredMatch > this.CcGrant )
            {
                yield return new ValidationResult("Required Match must be less or equal CC Grant");
            }
            if (this.AdminMax < 0 || this.AdminMax > 100)
			{
				yield return new ValidationResult("Admin Max (%) must be between 0 to 100");
			}
			if (!this.MaxAdminAmount.HasValue && this.MaxNonHcAmount.HasValue)
			{
				var msg = "If anything entered (excluding zero) in one of these 2 new fields, then the other field must also be entered with a none zero amount.";
				yield return new ValidationResult(msg, new string[] { "MaxAdminAmount", "MaxNonHcAmount" });
			}
			if (this.MaxAdminAmount.HasValue && !this.MaxNonHcAmount.HasValue)
			{
				var msg = "If anything entered (excluding zero) in one of these 2 new fields, then the other field must also be entered with a none zero amount.";
				yield return new ValidationResult(msg, new string[] { "MaxAdminAmount", "MaxNonHcAmount" });
			}
			if (this.HistoricalExpenditureAmount > this.CcGrant)
			{
				var msg = string.Format("Historical Expenditure Amount must be less than the CC Grant.");
				yield return new ValidationResult(msg, new string[] { "HistoricalExpenditureAmount" });
			}
			using (var db = new ccEntities())
			{
				var ag = db.AgencyGroups.Where(a => a.Id == this.AgencyGroupId).SingleOrDefault();
				if (ag != null)
				{
					if (ag.DayCenter && CurrencyId != "ILS")
					{
						yield return new ValidationResult("For Day Center Only ILS currency is allowed");

					}
					if (ag.SupportiveCommunities && CurrencyId != "ILS")
					{
						yield return new ValidationResult("For SupportiveCommunities Only ILS currency is allowed");

					}
				}


				if (db.Apps.Any(f => f.Name == this.Name && f.Id != this.Id))
				{
					yield return new ValidationResult("App Name must be unique");
				}
			}
		}
	}
}
