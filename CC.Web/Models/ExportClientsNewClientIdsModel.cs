using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CC.Web.Models
{
	public class ExportClientsNewClientIdsModel
	{
		public ExportClientsNewClientIdsModel()
		{
			this.CreateDate = DateTime.Now.Date;
		}

		[Required]
		public DateTime CreateDate { get; set; }

		public int? AgencyGroupId { get; set; }
	
		public System.Web.Mvc.SelectList AgencyGroups { get; set; }
	}
}