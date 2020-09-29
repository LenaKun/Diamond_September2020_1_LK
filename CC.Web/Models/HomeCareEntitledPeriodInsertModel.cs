using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CC.Web.Models
{
	public class HomeCareEntitledPeriodInsertModel
	{
		public HomeCareEntitledPeriodInsertModel()
		{
			StartDate = null;
			EndDateDisplay = null;
		}
		[Required]
		public int ClientId { get; set; }

		[UIHint("Date")]
		[Display(Name = "Start Date")]
		public DateTime? StartDate { get; set; }
		[UIHint("Date")]
		[Display(Name = "End Date")]
		public DateTime? EndDateDisplay { get; set; }
	}
}