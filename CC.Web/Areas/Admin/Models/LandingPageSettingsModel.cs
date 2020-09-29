using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CC.Web.Areas.Admin.Models
{
	public class LandingPageSettingsModel
	{
		[Display(Name = "Message")]
		[DataType(DataType.Html)]
		[System.Web.Mvc.AllowHtml]
		public string LandingPageMessageContent { get; set; }
	}
}