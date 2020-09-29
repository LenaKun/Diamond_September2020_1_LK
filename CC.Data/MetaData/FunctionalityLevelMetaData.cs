using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CC.Data.MetaData
{
    class FunctionalityLevelMetaData
    {
        [Display(Name="HC Hours Limit")]
        public int HcHoursLimit { get; set; }

        
    }
}
