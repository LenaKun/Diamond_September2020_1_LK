using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CC.Data;
using System.ComponentModel.DataAnnotations;

namespace CC.Web.Models
{
    public class AppBudgetDetailsModel : GenericModel<AppBudget>
    {

        public string Remarks { get; set; }
        public DateTime? Date { get; set; }

        public AppBudgetDetailsModel() : base() { SendEmail = true; }
        public AppBudgetDetailsModel(AppBudget a, User u)
            : base(a, u)
        {
			if ((u.RoleId == (int)FixedRoles.Ser || u.RoleId == (int)FixedRoles.SerAndReviewer) && AppBudget.EditableStatuses.Contains(a.ApprovalStatus))
            {
                this.ConditionalSubmitModel = new ConditionalSubmitModel() { Id = a.Id };

            }
        }

        public bool SendEmail { get; set; }
        public bool CanBeDeleted { get; set; }
		public bool AgencyGroupIsAudit { get; set; }
        public bool CanBeEdited()
        {
            var result = this.AppBudgetServices.Any();
			if ((FixedRoles.AgencyUser | FixedRoles.Ser | FixedRoles.AgencyUserAndReviewer | FixedRoles.SerAndReviewer | FixedRoles.Admin).HasFlag((FixedRoles)this.User.RoleId))
            {
				result &= AppBudget.IsEditable(this.Data);
            }
            else
            {
                return false;
            }
            return result;
        }
        public bool CanBeRevised()
        {
			bool result = (FixedRoles.Ser | FixedRoles.SerAndReviewer | FixedRoles.Admin).HasFlag((FixedRoles)this.User.RoleId);
            result &= new AppBudgetApprovalStatuses[] { AppBudgetApprovalStatuses.Approved }.Contains(this.Data.ApprovalStatus);
            return result;
        }

        public List<System.ComponentModel.DataAnnotations.ValidationResult> Warnings = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        public List<System.ComponentModel.DataAnnotations.ValidationResult> Errors = new List<System.ComponentModel.DataAnnotations.ValidationResult>();


        public bool CanBeSubmitted()
        {
			var result = this.User.RoleId == (int)FixedRoles.Ser || this.User.RoleId == (int)FixedRoles.SerAndReviewer || this.User.RoleId == (int)FixedRoles.Admin;
            result &= this.AppBudgetServices.Any();
			result &= AppBudget.EditableStatuses.Contains(this.Data.ApprovalStatus);
            return result;
        }
        public bool CanBeApprovedByGpo()
        {
            var result = this.User.RoleId == (int)FixedRoles.Admin || this.User.RoleId == (int)FixedRoles.GlobalOfficer;
            result &= this.Data.ApprovalStatus == AppBudgetApprovalStatuses.AwaitingGlobalPoApproval;
            return result;
        }
        public bool CanBeApprovedByRpo()
        {
            var result = this.User.RoleId == (int)FixedRoles.RegionOfficer || this.User.RoleId == (int)FixedRoles.Admin;
            result &= this.Data.ApprovalStatus == AppBudgetApprovalStatuses.AwaitingRegionalPoApproval;
            return result;
        }

        public bool AwaitAgencyResponseRPO()
        {
            var result = this.User.RoleId == (int)FixedRoles.RegionOfficer;
            result &= this.Data.ApprovalStatus == AppBudgetApprovalStatuses.AwaitingAgencyResponseRPO;
            return result;
        }

        public bool AwaitAgencyResponseGPO()
        {
            var result = this.User.RoleId == (int)FixedRoles.GlobalOfficer;
            result &= this.Data.ApprovalStatus == AppBudgetApprovalStatuses.AwaitingAgencyResponseGPO;
            return result;
        }

        public bool CanUpdateStatus()
        {
            return this.Permissions.User.RoleId == (int)FixedRoles.Admin;
        }

        public bool FormARequired
        {
            get
            {
                if (this.Data.App != null && this.Data.App.AgencyGroup != null && this.Data.App.AgencyGroup.Country != null)
                {
                    return this.Data.App.AgencyGroup.Country.Code.Equals("us", StringComparison.CurrentCultureIgnoreCase) || this.Data.App.AgencyGroup.Country.Code.Equals("ca", StringComparison.CurrentCultureIgnoreCase)
						|| this.Data.App.AgencyGroup.Country.Code.Equals("ar", StringComparison.CurrentCultureIgnoreCase) || this.Data.App.AgencyGroup.Country.Code.Equals("au", StringComparison.CurrentCultureIgnoreCase)
						|| this.Data.App.AgencyGroup.Country.Code.Equals("br", StringComparison.CurrentCultureIgnoreCase) || this.Data.App.AgencyGroup.Country.RegionId == 7 /*Western Europe*/;
                }
                else
                {
                    return true;
                }
            }
        }

        public IEnumerable<AppBudgetServiceDetailsModel> AppBudgetServices { get; set; }
        public IEnumerable<AppBudgetServiceDetailsModelForBmf> AppBudgetServicesForBmf { get; set; }
        public ConditionalSubmitModel ConditionalSubmitModel { get; set; }
    }

    public class AppBudgetServiceDetailsModel : AppBudgetServiceDetailsModelForBmf
    {
        [Display(Name = "Required match", Order = 5)]
        public decimal RequiredMatch { get; set; }
        [Display(Name = "Agency contribution", Order = 6)]
        public decimal AgencyContribution { get; set; }
    }

    public class AppBudgetServiceDetailsModelForBmf
    {
        public int Id { get; set; }
        [Display(Name = "Agency", Order = 0)]
        public string AgencyName { get; set; }
        [Display(Name = "Service Type", Order = 1)]
        public string ServiceType { get; set; }
        [Display(Name = "Service", Order = 2)]
        public string ServiceName { get; set; }
        [Display(Name = "Personnel", Order = 3)]
        public bool ServicePersonnel { get; set; }
        [Display(Name = "CC grant", Order = 4)]
        public decimal CCGrant { get; set; }
        [Display(Name = "CUR", Order = 7)]
        public string CurrencyCode { get; set; }
        [Display(Name = "Spent up to date", Order = 8)]
        public decimal? YtdSpent { get; set; }
        [Display(Name = "Remarks", Order = 9)]
        public string Remarks { get; set; }
        [ScaffoldColumn(false)]
        public System.Web.Mvc.MvcHtmlString RemarksShort
        {
            get
            {
                if (this.Remarks == null) return new System.Web.Mvc.MvcHtmlString(string.Empty);
                else
                {
                    var textOnly = new System.Text.RegularExpressions.Regex("<[^>]*>").Replace(this.Remarks, string.Empty);
                    var shortString = new String(textOnly.Take(20).ToArray());
                    var span = string.Format("<span title=\"{0}\">{1}</span>", System.Web.HttpUtility.HtmlEncode(this.Remarks), System.Web.HttpUtility.HtmlEncode(shortString));
                    return new System.Web.Mvc.MvcHtmlString(span);
                }
            }
        }
        [Display(Name = "App #", Order = 10)]
        public string AppName { get; set; }
		[Display(Name = "Original CC grant", Order = 11)]
		public decimal? OriginalCcGrant { get; set; }
	}


    public class ConditionalSubmitModel
    {
        public int Id { get; set; }
        public string Remarks { get; set; }
        public DateTime? Date { get; set; }
        public string Pass { get; set; }
    }

}