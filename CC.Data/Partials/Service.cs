using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CC.Data
{
	[System.ComponentModel.DataAnnotations.MetadataType(typeof(ServiceMetaData))]
	public partial class Service
	{
		public Service()
		{
			//set difaults
			this.Active = true;
		}

		public ReportingMethods ReportingMethodEnum { get { return (ReportingMethods)(this.ReportingMethodId); } }

		public ServiceConstraint DefaultConstraint {get;set;}

		public override bool Equals(object obj)
		{
			var s = obj as Service;
			if (s == null) return false;
			return s.Id == this.Id;
		}
		public override int GetHashCode()
		{
			return this.Id;
		}
        
		public enum ReportingMethods
		{
			[Display(Name="Client names and costs")]
			ClientNamesAndCosts = 1,
			[Display(Name="Total cost with list of client names")]
			TotalCostWithListOfClientNames = 2,
			[Display(Name="Total cost no names")]
			TotalCostNoNames = 3,
			[Display(Name="Homecare")]
			Homecare = 5,
			[Display(Name="Emergency")]
			Emergency = 6,
			[Display(Name = "Client Unit")]
			ClientUnit = 8,
			[Display(Name = "Client Unit Amount")]
			ClientUnitAmount = 9,
			[Display(Name="Supportive Communities")]
            SupportiveCommunities=12,
			[Display(Name="Day Centers")]
            DayCenters=13,
			[Display(Name="Homecare weekly reporting")]
			HomecareWeekly = 14,
			[Display(Name = "Calendar (Saturdays Inc)- Client unit by date, amount per report, validate out israeli ID duplicates")]
			SoupKitchens = 15,
			[Display(Name="Client Events Count")]
			ClientEventsCount = 16,
            [Display(Name = "Client Amount")]
            ClientAmount = 17
        }
		public enum ServiceTypes
		{
			AdministrativeOverhead = 1,
			AnnualHomeCareAssessmentFee = 2,
			CaseManagement = 3,
			ClientTransportation = 4,
			DentalProgram = 5,
			EmergencyAssistance = 6,
			FoodPrograms = 7,
			Homecare = 8,
			MedicalEquipment = 9,
			MedicalProgram = 10,
			Medicine = 11,
			MinorHomeModifications = 12,
			OtherServices = 13,
			Socialization = 14,
			WinterRelief = 15,
            SupportiveCommunities = 16,
            DayCenters = 17,
			/// <summary>
			/// aka IL Food Programs
			/// </summary>
			SoupKitchens = 19,
			HearingAid = 20,
			HospiseCare = 21,
            
        }
		public static bool IsAmountReportedPerClient(ReportingMethods reportingMethod)
		{
			switch (reportingMethod)
			{

                case Service.ReportingMethods.TotalCostWithListOfClientNames:
                    
                case Service.ReportingMethods.TotalCostNoNames:
				case Service.ReportingMethods.ClientUnit:
				case Service.ReportingMethods.SoupKitchens:
				case Service.ReportingMethods.ClientEventsCount:
                
                    return false;
				default:
					return true;
			}
		}

	}

	public class ServiceMetaData
	{
        
		[EnumDataType(typeof(Service.ReportingMethods))]
		[Display(Name = "ReportingMethod")]
		public int ReportingMethodId { get; set; }

		[Display(Name="Enforce Service Type constraints")]
		public bool EnforceTypeConstraints { get; set; }

		[Display(Name="Active")]
		public bool Active { get; set; }

		[Display(Name = "Exceptional Home Care Hours")]
		public bool ExceptionalHomeCareHours { get; set; }

		[Display(Name = "CC ID may only appear on service line once per year/per agency")]
		public bool SingleClientPerYearAgency { get; set; }

		[Display(Name = "Level")]
		[UIHint("ServiceLevel")]
		public int ServiceLevel { get; set; }

		[Display(Name = "Co-P Gov Hours Validation")]
		public bool CoPGovHoursValidation { get; set; }
	}
}
