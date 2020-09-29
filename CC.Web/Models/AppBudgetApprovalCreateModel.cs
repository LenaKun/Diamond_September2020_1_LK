using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CC.Data;

namespace CC.Web.Models
{

    public class AppBudgetCreateModel : IValidatableObject
    {
        [DisplayName("Fund")]
        public int? FundId { get; set; }
        public System.Web.Mvc.SelectList Funds { get; set; }

        [DisplayName("App")]
        [Required]
        public int AppId { get; set; }
        public System.Web.Mvc.SelectList Apps { get; set; }

        [DisplayName("Agency")]
        public int? AgencyId { get; set; }
        public System.Web.Mvc.SelectList Agencies { get; set; }

        [DisplayName("Revised")]
        public bool? revised { get; set; }

        
        public App App { get; set; }

    
        public int ExistingAppBudgetId()
        {
            using (var db = new ccEntities())
            {
                var duplicates = db.AppBudgets.Where(f => f.AppId == this.AppId);
                if (duplicates.Any())
                {
                    return duplicates.Single().Id;
                }
                return 0;
            }
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {

		   using(var db = new ccEntities())
		   {

				#warning duplicate appbudgets check missing/incomple
                var duplicates = db.AppBudgets.Where(f => f.AppId == this.AppId);
                var approvedDuplicates = duplicates.Where(f => (f.StatusId == (int)AppBudgetApprovalStatuses.Approved));

                if (approvedDuplicates.Any() && (this.revised == null || !this.revised.Value))
                {
                    yield return new ValidationResult("There is an approved duplicate Budget. Please mark it as revised if you want to revise it.");
                }
                else if (duplicates.Any())
                {
                    yield return new ValidationResult(string.Format("There is a duplicate budget id {0} status", duplicates.Single().ApprovalStatus));
                }
            }
        }
    }






    public class DbServiceServiceProvider : IServiceProvider
    {
        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(CC.Data.ccEntities))

                return new CC.Data.ccEntities();
            else
                return null;
        }
    }

    public static class IspE
    {
        public static T GetService<T>(this ValidationContext vc)
            where T : class
        {
            return vc.GetService(typeof(T)) as T;
        }
    }
}
