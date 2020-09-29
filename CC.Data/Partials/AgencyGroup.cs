using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace CC.Data
{
	[MetadataType(typeof(AgencyGroupMetaData))]
	public partial class AgencyGroup:IValidatableObject
	{

		public ReportingPeriods ReportingPeriod { get { return (ReportingPeriods)this.ReportingPeriodId; } set { this.ReportingPeriodId = (int)value; } }
		public enum ReportingPeriods
		{
			Monthly = 1,
			Quarterly = 3,
		}



		public static int[] TestIds
		{
			get
			{
				
				return new[]{70823, 70824, 70825};
			}
		}
		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (!Enum.IsDefined(typeof(ReportingPeriods), this.ReportingPeriodId))
			{
				yield return new ValidationResult("Invalid Reporting Period value. Allowed values are: 1, 3.");
			}
			if (string.IsNullOrWhiteSpace(this.Name))
			{
				yield return new ValidationResult("Name can not be empty");
			}
            if ((this.DayCenter || this.SupportiveCommunities) && this.ReportingPeriodId!=3)
            {
                yield return new ValidationResult("Quarterly reporting only is allowed. Reporting Period value must be 3");
            }
		}
		public static IEnumerable<object> GetScSubsidyLevels()
		{
			var items = new []{
				new {id=(int?)null, text="N/A" },
				new {id=(int?)1, text = "1"},
				new {id=(int?)2, text="2"}
			};
			return items;

		}
	}
	public class AgencyGroupMetaData
	{
		[Display(Name = "SER ID")]
		[Required(ErrorMessage = "The Ser ID field is required.")]
		[Range(1, int.MaxValue, ErrorMessage = "Ser ID must be greater than zero")]
		public int Id { get; set; }

		[UIHint("LongString")]
		public string Name { get; set; }

		[Display(Name="SER Display Name with City")]
		public string DisplayName { get; set; }
		
		[UIHint("LongString")]
		public string Addr1 { get; set; }
		
		[UIHint("LongString")]
		public string Addr2 { get; set; }
		
		[Display(Name="Matching Sum is required")]
		public bool RequiredMatch { get; set; }
        
		[Display(Name = "Israel ID is required")]
        public bool ForceIsraelID { get; set; }
		
		[Display(Name = "Reporting Period")]
		public int ReportingPeriodId { get; set; }

        [Display(Name = "Exclude From Reporting/Financial Summary")]
        public bool ExcludeFromReports { get; set; }

        [Display(Name = "SC (Supportive Communities)")]
        public bool SupportiveCommunities { get; set; }

        [Display(Name = "Supportive Communities - Full Subsidy")]
        public bool SC_FullSubsidy { get; set; }

        [Display(Name = "DCC (Day Center)")]
        public bool DayCenter { get; set; }

		[Display(Name = "SC Subsidy Level")]
		public int? ScSubsidyLevelId { get; set; }

		[Display(Name = "Default Currency")]
		public string DefaultCurrency { get; set; }

		[Display(Name = "CFS Date")]
		[DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
		public DateTime? CfsDate { get; set; }
	}

	public partial class AgencyGroupComparer : IEqualityComparer<AgencyGroup>
	{



		public bool Equals(AgencyGroup x, AgencyGroup y)
		{

			//Check whether the compared objects reference the same data.
			if (Object.ReferenceEquals(x, y)) return true;

			//Check whether any of the compared objects is null.
			if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null)) return false;

			return x.Name.Equals(y.Name, StringComparison.InvariantCultureIgnoreCase);

		}

		public int GetHashCode(AgencyGroup obj)
		{
			//Check whether the object is null
			if (Object.ReferenceEquals(obj, null)) return 0;

			//Get hash code for the Name field if it is not null.
			int hashProductName = obj.Name == null ? 0 : obj.Name.GetHashCode();

			//Get hash code for the Code field.
			int hashProductCode = obj.Id.GetHashCode();

			//Calculate the hash code for the obj.
			return hashProductName ^ hashProductCode;
		}
	}
}
