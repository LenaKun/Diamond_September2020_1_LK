using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CC.Web.Controllers
{
    public class tmpclass : tmpclassforbmf
    {
        [Display(Name = "Budget Match", Order = 5)]
        public decimal? RequiredMatch { get; set; }

        [Display(Name = "Budget Agency Contrib.", Order = 6)]
        public decimal? RequiredAgencyContribution { get; set; }

        [Display(Name = "Match Exp.", Order = 8)]
        public decimal? MatchExp { get; set; }

        [Display(Name = "Agency Contrib.", Order = 9)]
        public decimal? AgencyContribution { get; set; }

        [Display(Name = "YTD Match Exp.", Order = 11)]
        public decimal? YtdMatchExp { get; set; }

        [Display(Name = "YTD Agency Contrib.", Order = 12)]
        public decimal? YtdAgencyContribution { get; set; }

        [ScaffoldColumn(false)]
        [Display(Name = "YTD Match Exp. Status")]
        public decimal? YtdMatchExpStatus { get; set; }

		public int ServiceTypeId { get; set; }
		public bool FirstHomecareWeekly { get; set; }
	}

    public class tmpclassforbmf
    {
        [ScaffoldColumn(false)]
        public int? Id { get; set; }


        [ScaffoldColumn(false)]
        public int AppBudgetServiceId { get; set; }

        [Display(Name = "Agency", Order = 0)]
        public string AgencyName { get; set; }

        [Display(Name = "Service Type", Order = 1)]
        public string ServiceTypeName { get; set; }

        [Display(Name = "Service", Order = 2)]
        public string ServiceName { get; set; }

        [Display(Name = "Budget Remarks", Order = 3)]
        public string AppBudgetServiceRemarks { get; set; }

        [Display(Name = "Budget", Order = 4)]
        public decimal? CcGrant { get; set; }

        [Display(Name = "CC Exp.", Order = 7)]
        public decimal? CcExp { get; set; }

        [Display(Name = "YTD CC Exp.", Order = 10)]
        public decimal? YtdCcExp { get; set; }

        [Display(Name = "CUR", Order = 13)]
        public string Cur { get; set; }
       // public decimal? FunExpAmount { get; set; }
    }
}
