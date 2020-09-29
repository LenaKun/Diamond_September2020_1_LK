using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CC.Web.Models
{
	public class LandingPageModel
	{
		[DataType(DataType.Html)]
		[System.Web.Mvc.AllowHtml]
		public string TopMessageContent { get; set; }
		public List<CC.Data.File> FilesList { get; set; }
	}
}