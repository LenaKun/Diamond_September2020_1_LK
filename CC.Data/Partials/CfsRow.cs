using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CC.Data.MetaData;
using System.ComponentModel.DataAnnotations;

namespace CC.Data
{
	[MetadataType(typeof(CfsRowMetaData))]
	partial class CfsRow : IValidatableObject
	{
		public CfsRow()
		{
			CreatedAt = DateTime.Now;
		}

		[Display(Name = "Over-Ride Reasons")]
		public string OverRideReasonIds { get; set; }

		public IEnumerable<System.Web.Mvc.SelectListItem> EndDateReasons { get; set; }
		public IEnumerable<System.Web.Mvc.SelectListItem> OverRideReasons { get; set; }

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			int reasonId = 4;
			using(var db = new ccEntities())
			{
				var reason = db.CfsEndDateReasons.SingleOrDefault(f => f.Name == "A leave reason/leave date has been entered in the client details");
				if(reason != null)
				{
					reasonId = reason.Id;
				}
				if(this.StartDate < db.Clients.SingleOrDefault(f => f.Id == this.ClientId).JoinDate)
				{
					yield return new ValidationResult("Start Date must be later than or equal to client's Join Date");
				}
			}
			if(this.EndDate.HasValue && this.StartDate.HasValue && this.EndDate <= this.StartDate)
			{
				yield return new ValidationResult("End Date must be later than Start Date");
			}
			if(this.EndDate.HasValue && this.EndDate.Value.Day < DateTime.DaysInMonth(this.EndDate.Value.Year, this.EndDate.Value.Month))
			{
				yield return new ValidationResult("End Date must be equal to the last day of the month");
			}
			if(this.AgencyOverRide)
			{
				if(string.IsNullOrEmpty(this.OverRideReasonIds))
				{
					yield return new ValidationResult("Over-Ride Reasons is a required field");
				}
				if(string.IsNullOrEmpty(this.OverRideDetails))
				{
					yield return new ValidationResult("Over-Ride Details is a required field");
				}
				if (string.IsNullOrEmpty(this.OverrideAgencyFirstName))
				{
					yield return new ValidationResult("Over-Ride Agency First Name is a required field");
				}
				if (string.IsNullOrEmpty(this.OverrideAgencyLastName))
				{
					yield return new ValidationResult("Over-Ride Agency Last Name is a required field");
				}
				if (string.IsNullOrEmpty(this.OverrideAgencyTitle))
				{
					yield return new ValidationResult("Over-Ride Agency Title is a required field");
				}
			}
			if(this.EndDate.HasValue && this.EndDateReasonId != reasonId)
			{
				if(!this.EndDateReasonId.HasValue)
				{
					yield return new ValidationResult("End Date Reason is a required field");
				}
				if (string.IsNullOrEmpty(this.AgencyRequestorFirstName))
				{
					yield return new ValidationResult("Agency Requestor First Name is a required field");
				}
				if (string.IsNullOrEmpty(this.AgencyRequestorLastName))
				{
					yield return new ValidationResult("Agency Requestor Last Name is a required field");
				}
				if (string.IsNullOrEmpty(this.AgencyRequestorTitle))
				{
					yield return new ValidationResult("Agency Requestor Title is a required field");
				}
				if (!this.EndRequestDate.HasValue)
				{
					yield return new ValidationResult("End Request Date is a required field");
				}
			}
		}
	}
}
