using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace CC.Data
{
    [MetadataType(typeof(CC.Data.MetaData.CfsAmountMetaData))]
    public partial class CfsAmount : IValidatableObject
    {
        public CfsAmount()
        {
            this.Year = DateTime.Today.Year;
        }

        public IEnumerable<System.Web.Mvc.SelectListItem> Years { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> Levels { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> Currencies { get; set; }
        public IEnumerable<string> CountryNames
        {
            get
            {
                return this.Countries.Select(f => f.Name);
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
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
			if (!this.Countries.Any())
			{
				yield return new ValidationResult("Please select at least one Country");
			}
			using (var db = new ccEntities())
            {
                var cfsCountryIds = this.Countries.Select(f => f.Id).ToArray();				
                var q = from ca in db.CfsAmounts
                        from country in ca.Countries
                        where ca.Id != this.Id
                        where cfsCountryIds.Contains(country.Id)
                        where ca.Year == this.Year && ca.CurrencyId == this.CurrencyId && ca.Level == this.Level
                        select new
                        {
                            CfsAmountId = ca.Id,
                            CountryId = country.Id,
                            CountryCode = country.Code,
                            Year = ca.Year,
                            CurrencyId = ca.CurrencyId,
                            Level = ca.Level
                        };
                foreach (var item in q)
                {
                    yield return new ValidationResult(string.Format("Conflict with Country {0}, Year {1}, Currency {2}, Level {3}", item.CountryCode, item.Year, item.CurrencyId, item.Level));
                }
            }
        }
    }
}
