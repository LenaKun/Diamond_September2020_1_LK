using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CC.Web.Areas.Admin.Models
{
    public class AutomatedReportsModel : AdminModelBase
    {
        public AutomatedReportsModel() { }

        [Display(Name = "Activate Automated Report")]
        public bool IsActive { get; set; }

        [Display(Name = "Last Email Date")]
        public DateTime? LastEmailDate { get; set; }

        [Display(Name = "Last Email Status")]
        public string Status { get; set; }
    }
}