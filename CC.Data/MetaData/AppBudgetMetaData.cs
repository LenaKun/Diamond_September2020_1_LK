using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CC.Data.MetaData
{
    public class AppBudgetMetaData
    {
        [UIHint("AppBudgetStatus")]
        [Required()]
        public int StatusId { get; set; }

        [DisplayName("Program Office Remarks")]
		[DataType(System.ComponentModel.DataAnnotations.DataType.Html)]
        public string PoRemarks { get; set; }

		[DataType(System.ComponentModel.DataAnnotations.DataType.Html)]
        public string ValidRemarks { get; set; }

		[Display(Name="Valid Until")]
		public virtual Nullable<System.DateTime> ValidUntill { get; set; }

    }
}
