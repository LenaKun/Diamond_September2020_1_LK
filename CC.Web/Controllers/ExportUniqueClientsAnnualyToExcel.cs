using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace CC.Web.Controllers
{
	class ExportUniqueClientsAnnualyToExcel
	{
		[Display(Name = "Region")]
		public string RegionName { get; set; }

		[Display(Name = "Country")]
		public string CountryName { get; set; }

		[Display(Name = "State/Province")]
		public string StateName { get; set; }

		[Display(Name = "SER")]
		public string AgencyGroupName { get; set; }

		[Display(Name = "Service Type")]
		public string ServiceTypeName { get; set; }

		[Display(Name = "Service")]
		public string ServiceName { get; set; }

		[Display(Name = "Year")]
		public int? Year { get; set; }

		[Display(Name = "Number of unique clients served")]
		public int Count { get; set; }
	}	
}
