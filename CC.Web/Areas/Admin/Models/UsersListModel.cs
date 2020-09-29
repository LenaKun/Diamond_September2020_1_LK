using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CC.Web.Areas.Admin.Models
{
	public class UsersListModel
	{
		[Display(Name = "Region")]
		public int? SelectedRegionId { get; set; }

		[Display(Name = "Country")]
		public int? SelectedCountryId { get; set; }

		[Display(Name = "Ser")]
		public int? SelectedAgencyGroupId { get; set; }

		[Display(Name = "Agency")]
		public int? SelectedAgencyId { get; set; }

		public List<int> SelectedUserIds { get; set; }

		public UsersListModel()
        {
            SelectedUserIds = new List<int>();
        }

		[Required(ErrorMessage = "To is a required field")]
		[Display(Name = "To")]
		public string EmailTo { get; set; }
		[Display(Name = "CC")]
		public string EmailCc { get; set; }
		[Display(Name = "BCC")]
		public string EmailBcc { get; set; }
		[Display(Name = "Subject")]
		public string Subject { get; set; }
		[Display(Name = "Body")]
		public string Body { get; set; }
	}
}