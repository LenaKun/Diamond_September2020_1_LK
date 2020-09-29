using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CC.Data;

namespace CC.Web.Areas.Admin.Models
{
	public class ClientsHcStatusChangeEmailModel
	{
		public  int AgencyGroupId {get;set;}
		public IEnumerable<ClientHcStatusChangeEmailModel> Clients { get; set; }
	}
	public class ClientHcStatusChangeEmailModel
	{

		public int ClientId { get; set; }

		public string ClientName { get; set; }

		public string BirthCountryName { get; set; }

		public string CountryName { get; set; }

		public string ApprovalStatusName { get; set; }

		public string HcStatusName { get; set; }

		public int AgencyId { get; set; }
	}

}