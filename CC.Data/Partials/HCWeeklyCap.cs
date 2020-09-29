using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace CC.Data
{
	[MetadataType(typeof(CC.Data.MetaData.HCWeeklyCapMetaData))]
	public partial class HCWeeklyCap : IValidatableObject
	{
		public HCWeeklyCap()
		{
			this.CurrencyId = "USD";
			this.Active = true;
		}

		[Required]
		[Display(Name = "Start Date")]
		[DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
		public DateTime? StartDateDisplay
		{
			get
			{
				if (this.StartDate == default(DateTime)) return null;
				else return this.StartDate;
			}
			set
			{
				if (value.HasValue) this.StartDate = value.Value;
				else this.StartDate = default(DateTime);
			}
		}

		[Required]
		[Display(Name = "End Date")]
		[DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
		public DateTime? EndDateDisplay
		{
			set
			{
				if (value.HasValue) this.EndDate = value.Value.AddDays(1);
				else this.EndDate = default(DateTime);
			}
			get
			{
				if (this.EndDate < default(DateTime).AddDays(1))
				{
					return null;
				}
				else
				{
					return this.EndDate.AddDays(-1).Date;
				}
			}
		}

		public IEnumerable<string> CountryNames
		{
			get
			{
				return this.Countries.Select(f => f.Name);
			}
		}

		public IEnumerable<string> FundNames
		{
			get
			{
				return this.Funds.Select(f => f.Name);
			}
		}

		public IEnumerable<int> cIds
		{
			get { return this.Countries.Select(f => f.Id); }
			set
			{
				foreach (var id in value)
				{
					this.Countries.Add(new Country() { Id = id });
				}
			}
		}
		public IEnumerable<int> fIds
		{
			get
			{
				return this.Funds.Select(f => f.Id);
			}
			set
			{
				foreach (var id in value)
				{
					this.Funds.Add(new Fund() { Id = id });
				}
			}
		}

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (!this.Countries.Any())
			{
				yield return new ValidationResult("Please select at least one Country");
			}

			if (!this.Funds.Any())
			{
				yield return new ValidationResult("Please select at least one Fund");
			}

			if (this.StartDate >= this.EndDate)
			{
				yield return new ValidationResult("The End Date must be greater than the Start Time");
			}

			if (this.CapPerPerson < 0)
			{
				yield return new ValidationResult("The CapPerPerson must be >= 0");
			}

			using (var db = new ccEntities())
			{

				var duplicateName = db.HCWeeklyCaps.Any(f => f.Name == this.Name && f.Id != this.Id);
				if (duplicateName)
				{
					yield return new ValidationResult("Homecare Cap Name must be unique.");
				}
			}

			using (var db = new ccEntities())
			{
				var capFundIds = this.Funds.Select(f => f.Id).ToArray();
				var capCountryIds = this.Countries.Select(f => f.Id).ToArray();
				var q = from hcc in db.HCWeeklyCaps
						from fund in hcc.Funds
						from country in hcc.Countries
						where hcc.Id != this.Id
						where capFundIds.Contains(fund.Id)
						where capCountryIds.Contains(country.Id)
						where hcc.StartDate < this.EndDate && hcc.EndDate > this.StartDate
						select new
						{
							CapId = hcc.Id,
							FundId = fund.Id,
							CountryId = country.Id,
							FundName = fund.Name,
							CountryCode = country.Code,
							StartDate = hcc.StartDate,
							EndDate = hcc.EndDate
						};
				if (q.Any())
				{

					yield return new ValidationResult("A fund can be included in a cap only once for any given date.");
					foreach (var item in q)
					{
						yield return new ValidationResult(string.Format("Conflict with Fund: {0}, Country {1}, Start Date {2}, End Date: {3}", item.FundName, item.CountryCode, item.StartDate, item.EndDate));
					}
				}
			}

		}
	}
}
