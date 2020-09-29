using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CC.Web.Models
{
    public class PersonnelsDetailModel
    {
        public IEnumerable<PersonnelEntry> Personnels { get; set; }

        public class PersonnelEntry
        {
           
            [Display(Name = "Position")]
            public string Position { get; set; }
            [Display(Name = "Currency")]
            public string Currency { get; set; }
            [Display(Name = "CC grant")]
            public decimal? CCGrant { get; set; }
             [Display(Name = "% of time spent on CC program")]
            public decimal? TimeSpentOnCCPercentage {get;set;}
             [Display(Name = "Agency Portion of Salary")]
             public decimal? ServiceSalaryPortionPercentage { get; set; }
         
        }
    }
}