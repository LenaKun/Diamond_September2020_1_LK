using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CC.Web.Models
{
	public class CfsRowDataTableModel : jQueryDataTableParamsWithData<CfsTableRow>
	{
		public int? FilterRegionId { get; set; }
		public int? FilterCountryId { get; set; }
		public int? FilterStateId { get; set; }
		public int? FilterAgencyGroupId { get; set; }
		public int? FilterAgencyId { get; set; }
		public int? FilterClientId { get; set; }
		[DataType(DataType.Date)]
		public DateTime? FilterFrom { get; set; }
		[DataType(DataType.Date)]
		public DateTime? FilterTo { get; set; }
	}

	public class CfsTableRow
	{
		[ScaffoldColumn(false)]
		public int Id { get; set; }

		[Display(Name="CC ID")]
		public int ClientId { get; set; }

		[Display(Name="Last Name")]
		public string LastName { get; set; }

		[Display(Name="First Name")]
		public string FirstName { get; set; }

		[Display(Name="CFS Level (As of today)")]
		public int CfsLevel { get; set; }

		[Display(Name="CFS Amount (As of today)")]
		public decimal CfsAmount { get; set; }

		[Display(Name="Ser Currency")]
		public string SerCurrency { get; set; }

		[Display(Name="CFS from")]
		public DateTime? StartDate { get; set; }

		[Display(Name="CFS to")]
		public DateTime? EndDate { get; set; }

		[Display(Name="Client Response")]
		public string ClientResponseIsYes { get; set; }

		[Display(Name="Agency Over-Ride")]
		public string AgencyOverRide { get; set; }

		[Display(Name="Communications Preference")]
		public string CommunicationsPreference { get; set; }

		[Display(Name="Care Received Via")]
		public string CareReceivedVia { get; set; }

		[Display(Name="Ser Name")]
		public string AgencyGroupName { get; set; }
	}
}