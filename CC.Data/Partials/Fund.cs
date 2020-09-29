using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CC.Data
{
    [MetadataType(typeof(CC.Data.MetaData.FundMetaData))]
    public partial class Fund : IValidatableObject
    {
		public Fund()
		{
			this.StartDate = new DateTime(DateTime.Now.Year, 1, 1);
			this.EndDate = StartDate.AddYears(1);
            this.OtherServicesMax = 100;
            this.HomecareMin = 0;
            this.AdminMax = 100;
		}
		public FundExchangeRate GetExchangeRate(string curId)
		{
			FundExchangeRate fundExchangeRate = null;
			if (this.FundExchangeRates != null)
			{
				fundExchangeRate = this.FundExchangeRates.SingleOrDefault(f => f.CurId == curId);
			}
			if (fundExchangeRate == null)
			{
				fundExchangeRate = new FundExchangeRate { FundId = this.Id, CurId = curId, Value = 1 };
			}
			return fundExchangeRate;
		}

		public ServiceConstraint AdministrativeOverheadConstraint
		{
			get { return this.GetBy(Service.ServiceTypes.AdministrativeOverhead); }
			set { this.SetBy(Service.ServiceTypes.AdministrativeOverhead, value); }
		}
		public ServiceConstraint HomeCareConstraint
		{
			get { return this.GetBy(Service.ServiceTypes.AdministrativeOverhead); }
			set { this.SetBy(Service.ServiceTypes.AdministrativeOverhead, value); }
		}
		public ServiceConstraint OtherServicesConstraint
		{
			get { return this.GetBy(Service.ServiceTypes.AdministrativeOverhead); }
			set { this.SetBy(Service.ServiceTypes.AdministrativeOverhead, value); }
		}
		

		private ServiceConstraint GetBy(Service.ServiceTypes st)
		{
			return this.ServiceConstraints.SingleOrDefault(f => f.ServiceTypeId == (int)st);
		}
		private void SetBy(Service.ServiceTypes st, ServiceConstraint sc)
		{
			var existing = GetBy(st);
			if (existing == null)
			{
				this.ServiceConstraints.Add(sc);
			}
			else if (sc == null)
			{
				this.ServiceConstraints.Remove(existing);
			}
			else
			{
				existing = sc;
			}
		}

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (this.OtherServicesMax < 0 || this.OtherServicesMax > 100)
            {
                yield return new ValidationResult("Other Services Max (%) must be between 0 to 100");
            }
            if (this.HomecareMin < 0 || this.HomecareMin > 100)
            {
                yield return new ValidationResult("Homecare Min (%) must be between 0 to 100");
            }
            if (this.AdminMax < 0 || this.AdminMax > 100)
            {
                yield return new ValidationResult("Admin Max (%) must be between 0 to 100");
            }
        }
    }

}
