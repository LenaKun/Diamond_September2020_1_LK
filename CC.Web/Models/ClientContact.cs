using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CC.Web.Models
{
	public class ClientContactModel:CC.Data.ClientContact
	{
		public HttpPostedFileBase PostedFile { get; set; }

		

	}
}