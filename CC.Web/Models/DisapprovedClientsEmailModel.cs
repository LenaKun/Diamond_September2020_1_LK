using System;
using System.Collections.Generic;
using System.Linq;
using CC.Data;
using System.Web;

namespace CC.Web.Models
{
	public class DisapprovedClientsEmailModel
	{
		public Agency Agency { get; set; }
		public IEnumerable<ClientEntry> Clients { get; set; }

		public class ClientEntry
		{
			public int Id { get; set; }
			public string Name { get; set; }
			public DateTime Date { get; set; }
		}
	}
}