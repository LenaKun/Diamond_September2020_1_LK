using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CC.Data.MetaData
{
    class HomeCareEntitledPeriodMetaData
    {
        [DisplayName("Start Date")]
        [UIHint("Date")]
        [DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
        public DateTime StartDate { get; set; }
        
        [DisplayName("End Date")]
        [UIHint("Date")]
		[DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
		[UIHint("DateInclusive")]
        public DateTime? EndDate { get; set; }
    }
}
