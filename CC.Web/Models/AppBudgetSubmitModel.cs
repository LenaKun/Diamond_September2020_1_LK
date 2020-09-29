using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using CC.Data;

namespace CC.Web.Models
{


    public class AppBudgetSubmitModel : ModelBase, IValidatableObject
    {
        public bool ValidDetailsRequired { get; set; }

        [Required()]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        public DateTime ValidUntil { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string ValidRemarkd { get; set; }

        [Required()]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        public string Pass { get; set; }

        public CC.Data.AppBudget Details { get; set; }

        public bool CanSubmit()
        {
            switch (this.Details.ApprovalStatus)
            {
                case AppBudgetApprovalStatuses.New:
					return this.User.RoleId == (int)FixedRoles.Ser || this.User.RoleId == (int)FixedRoles.SerAndReviewer;
                default:
                    return false;
            }
        }



        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!User.MembershipUser.ValidatePassword(this.Pass))
            {
                yield return new ValidationResult("invalid pass");
            }
        }
    }
}