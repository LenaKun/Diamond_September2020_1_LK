using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace CC.Data.MetaData
{
	public class CfsRowMetaData
	{
		[Key]
		public int Id { get; set; }

		[DisplayName("CC ID")]
		public int ClientId { get; set; }

		[DateFormat()]
		[DataType(DataType.Date)]
		[DisplayName("Created At")]
		public DateTime CreatedAt { get; set; }

		[DisplayName("Created By")]
		public int CreatedById { get; set; }

		[DateFormat()]
		[DataType(DataType.Date)]
		[DisplayName("Updated At")]
		public DateTime? UpdatedAt { get; set; }

		[DisplayName("Updated By")]
		public int? UpdatedById { get; set; }

		[DateFormat()]
		[DataType(DataType.Date)]
		[DisplayName("Start Date")]
		public DateTime? StartDate { get; set; }

		[DateFormat()]
		[DataType(DataType.Date)]
		[DisplayName("End Date")]
		public DateTime? EndDate { get; set; }

		[DisplayName("End Date Reason")]
		public int? EndDateReasonId { get; set; }

		[Display(Name = "Client Response", Description = @"You may not uncheck this as an agency/ Ser if client is set to 'Private Pay Family'")]
		[UIHint("boolYesNo")]
		public bool ClientResponseIsYes { get; set; }

		[DisplayName("Agency Over-Ride")]
		[UIHint("boolYesNo")]
		public bool AgencyOverRide { get; set; }

		[DisplayName("Over-Ride Details")]
		public string OverRideDetails { get; set; }

		[DisplayName("Over-Ride Agency First Name")]
		public string OverrideAgencyFirstName { get; set; }

		[DisplayName("Over-Ride Agency Last Name")]
		public string OverrideAgencyLastName { get; set; }

		[DisplayName("Over-Ride Agency Title")]
		public string OverrideAgencyTitle { get; set; }

		[DateFormat()]
		[DataType(DataType.Date)]
		[DisplayName("CFS Approved")]
		public DateTime? CfsApproved { get; set; }

		[DisplayName("Agency Requestor First Name")]
		public string AgencyRequestorFirstName { get; set; }

		[DisplayName("Agency Requestor Last Name")]
		public string AgencyRequestorLastName { get; set; }

		[DisplayName("Agency Requestor Title")]
		public string AgencyRequestorTitle { get; set; }

		[DateFormat()]
		[DataType(DataType.Date)]
		[DisplayName("End Request Date")]
		public DateTime? EndRequestDate { get; set; }

		[DisplayName("CFS Admin Remarks")]
		public string CfsAdminRemarks { get; set; }

		[DisplayName("CFS Admin Rejected")]
		[UIHint("boolYesNo")]
		public bool CfsAdminRejected { get; set; }

		[DisplayName("CFS Admin Internal Remarks")]
		public string CfsAdminInternalRemarks { get; set; }

		[DateFormat()]
		[DataType(DataType.Date)]
		[DisplayName("CFS Admin Last Update")]
		public DateTime? CfsAdminLastUpdate { get; set; }
	}
}
