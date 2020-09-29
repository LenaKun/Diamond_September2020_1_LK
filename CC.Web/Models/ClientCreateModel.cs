using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CC.Data;
using System.Text.RegularExpressions;
using System.Globalization;


namespace CC.Web.Models
{
    public class ClientCreateModel : ModelBase, IValidatableObject
    {
        /// <summary>
        /// current data
        /// </summary>
        public Client Data { get; set; }

        public User CcUser { get; set; }

        public bool IsDuplicate { get; set; }
        public bool ForceInsertDuplicate { get; set; }

        public bool isDccSubside { get; set; }

		public int UserRegionId { get; set; }
		public string IsraeliNationalIdTypeName { get; set; }
		public string IsraeliCountryName { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> Countries { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> States { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> Agencies { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> ApprovalStatuses { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> NationalIdTypes { get; set; }
        public IEnumerable<System.Web.Mvc.SelectListItem> LeaveReasons { get; set; }
		public IEnumerable<System.Web.Mvc.SelectListItem> CommunicationsPreferences { get; set; }
		public IEnumerable<System.Web.Mvc.SelectListItem> CareReceivingOptions { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> DccSubsides { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            this.IsDuplicate = Client.CheckForDuplicate(this.Data);

            if (this.IsDuplicate && !this.ForceInsertDuplicate)
            {
                yield return new ValidationResult("Another client with similar details already exist in the system, Are you sure you would like to Add this Client?", new string[] { "IsDuplicate" });

            }

            if (this.Data.DeceasedDate != null && this.Data.DeceasedDate > DateTime.Now)
            {
                yield return new ValidationResult("Deceased Date can't be greater then the date of entry");
            }
            else if (this.Data.LeaveDate != null && this.Data.LeaveDate > DateTime.Now && this.User.RoleId != (int)FixedRoles.Admin && this.User.RoleId != (int)FixedRoles.GlobalOfficer)
            {
                yield return new ValidationResult("Leave Date can't be greater then the date of entry");
            }

			if(this.Data.JoinDate.Date > DateTime.Today)
			{
				yield return new ValidationResult("Join Date can't be greater then the date of entry");
			}

			Regex nameRgx = new Regex(@"^[A-Za-z\s'""-]*$");
			if (!nameRgx.IsMatch(this.Data.FirstName) || !nameRgx.IsMatch(this.Data.LastName))
			{
				yield return new ValidationResult("First Name and Last Name can only contain letters (A-Z a-z), spaces, ', - and \"");
			}

			Regex addressRgx = new Regex(@"^[A-Za-z\s\d.#//-]*$");
			if(!addressRgx.IsMatch(this.Data.Address))
			{
				yield return new ValidationResult("Address can only contain letters (A-Z a-z), numbers, spaces, -, ., # and /");
			}

			using (var db = new ccEntities())
			{
				var dbClient = db.Clients.SingleOrDefault(f => f.Id == this.Data.Id);

				if(this.User.RoleId != (int)FixedRoles.Admin && (dbClient == null || !dbClient.FirstName.Equals(this.Data.FirstName) || (!dbClient.LastName.Equals(this.Data.LastName))) && this.Data.FirstName.Equals(this.Data.LastName))
				{
					yield return new ValidationResult("First Name and Last Name can't be the same");
				}

				if ((dbClient == null || dbClient.BirthDate != this.Data.BirthDate) && this.Data.BirthDate < Client.MinEligibleBirthDate && this.User.RoleId != (int)FixedRoles.Admin)
				{
					yield return new ValidationResult("Birth Date can't be earlier than " + Client.MinEligibleBirthDate.ToShortDateString());
				}

			
				if ((dbClient == null || dbClient.JoinDate != this.Data.JoinDate) && this.Data.JoinDate < Client.MinEligibleJoinDate && this.User.RoleId != (int)FixedRoles.Admin)
				{
					yield return new ValidationResult("Join Date can't be earlier than " + Client.MinEligibleJoinDate.ToShortDateString());
				}
			}

			if(this.Data.NationalIdTypeId.HasValue && string.IsNullOrEmpty(this.Data.NationalId))
			{
				yield return new ValidationResult("Government Issued ID is required when ID Type is selected");
			}

			if (!this.Data.NationalIdTypeId.HasValue && !string.IsNullOrEmpty(this.Data.NationalId))
			{
				yield return new ValidationResult("ID Type is required when Government Issued ID is not empty");
			}

			if(this.Data.JoinDate < new DateTime(1946, 2, 9))
			{
				yield return new ValidationResult("Join Date can't be earlier than 09 Feb 1946");
			}
        }
    }
}