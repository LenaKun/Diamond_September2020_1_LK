using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CC.Data;

namespace CC.Web.Areas.Admin.Models
{
	public class ClientStatusChangeEmailModel
	{
		public  AgencyGroup Ser {get;set;}
		public IEnumerable<ImportClientFundStatusProc_Result> Clients { get; set; }
	}
}