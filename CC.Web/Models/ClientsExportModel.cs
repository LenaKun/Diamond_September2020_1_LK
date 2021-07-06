using System;
using CC.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CC.Web.Models
{

	/// </summary>
	public class ClientsExportModelBMF
	{
		[Display(Name = "Internal Agency ID", Order = 57)]
		public string InternalAgencyID { get; set; }
		[Display(Name = "ORG ID", Order = 0)]
		public int? ORGID { get; set; }
		[Display(Name = "Agency", Order = 1)]
		public string Agency { get; set; }
		[Display(Name = "First Name", Order = 5)]
		public string FirstName { get; set; }
		[Display(Name = "Last Name", Order = 4)]
		public string LastName { get; set; }
		[Display(Name = "Middle Name", Order = 43)]
		public string MiddleName { get; set; }
		[Display(Name = "Other First Name", Order = 44)]
		public string OtherFirstName { get; set; }
		[Display(Name = "Other Last Name", Order = 45)]
		public string OtherLastName { get; set; }
		[Display(Name = "Previous First Name", Order = 46)]
		public string PreviousFirstName { get; set; }
		[Display(Name = "Previous Last Name", Order = 47)]
		public string PreviousLastName { get; set; }
		[Display(Name = "Address", Order = 48)]
		public string Address { get; set; }
		[Display(Name = "City", Order = 49)]
		public string City { get; set; }
		[Display(Name = "ZIP", Order = 51)]
		public string ZIP { get; set; }
		[Display(Name = "State", Order = 50)]
		public string State { get; set; }
		[Display(Name = "Country of Residence", Order = 8)]
		public string CountryName { get; set; }
		[Display(Name = "Birth Date", Order = 9)]
		public DateTime? BirthDate { get; set; }
		[Display(Name = "Other date of birth", Order = 52)]
		public DateTime? Otherdateofbirth { get; set; }
		[Display(Name = "Birth City", Order = 10)]
		public string BirthCity { get; set; }
		[Display(Name = "Birth Country", Order = 11)]
		public string BirthCountry { get; set; }
		[Display(Name = "ID Type", Order = 58)]
		public string IDType { get; set; }
		[Display(Name = "Other ID card", Order = 61)]
		public string OtherIDcard { get; set; }
		[Display(Name = "Other ID type", Order = 60)]
		public string OtherIDtype { get; set; }
		[Display(Name = "Government Issued ID", Order = 59)]
		public string GovernmentIssuedID { get; set; }
		[Display(Name = "Deceased Date", Order = 32)]
		public DateTime? DeceasedDate { get; set; }
		[Display(Name = "Leave Date", Order = 31)]
		public DateTime? LeaveDate { get; set; }
		[Display(Name = "Join Date", Order = 13)]
		public DateTime? JoinDate { get; set; }
		[Display(Name = "Leave Reason", Order = 29)]
		public string LeaveReason { get; set; }
        [Display(Name = "Remarks", Order = 30)]
        public string Remarks { get; set; }
		[Display(Name = "Income Criteria Complied", Order = 62)]
		public bool IncomeCriteriaComplied { get; set; }
		[Display(Name = "Gender", Order = 56)]
		public string Gender { get; set; }
		[Display(Name = "CC ID", Order = 2)]
		public int CCID { get; set; }
	//	[Display(Name = "MasterId", Order = 3)]
	//	public int? MasterId { get; set; }
		[Display(Name = "Create Date", Order = 12)]
		public DateTime? CreateDate { get; set; }
		[Display(Name = "FunctionalityLevelName", Order = 20)]
		public string FunctionalityLevelName { get; set; }
		[Display(Name = "HC Hours", Order = 21)]
		public decimal? HCHours { get; set; }
		[Display(Name = "GF Hours", Order = 22)]
		public decimal? GrandfatheredHours { get; set; }
		[Display(Name = "Legacy Start Date", Order = 23)]
		public DateTime? GFStartDate { get; set; }		
		[Display(Name = "Legacy Type", Order = 24)]
		public string GFType { get; set; }
		[Display(Name = "Approval Status", Order = 6)]
		public string ApprovalStatus { get; set; }
		[Display(Name = "Nazi Persecution Details", Order = 42)]
		public string NaziPersecutionDetails { get; set; }
		[Display(Name = "Compensation Program", Order = 41)]
		public string CompensationProgram { get; set; }
		[Display(Name = "Service Eligibility Start Date (highest dateif multiple entries)", Order = 14)]
		public DateTime? HomecareEligibilityStartDate { get; set; }
		[Display(Name = "Service Eligibility End Date (highest dateif multiple entries)", Order = 34)]
		public DateTime? HomecareEligibilityEndDate { get; set; }
		[Display(Name = "Govt HC hours (highest date if multiple entries)", Order = 25)]
		public decimal? GovtHChours { get; set; }
		[Display(Name = "Govt HC hours start date", Order = 26)]
		public DateTime? GovtHChoursStartDate { get; set; }
		[Display(Name = "Diagnostic Score (highest dateif multiple entries)", Order = 15)]
		public decimal? DiagnosticScore { get; set; }
		[Display(Name = "Highest Start Date of Diagnostic Score", Order = 16)]
		public DateTime? HighestStartDateofDiagnosticScore { get; set; }
		[Display(Name = "HomeCare Entitled", Order = 17)]
		public string HomeCareEntitled { get; set; }
		[Display(Name = "Deceased", Order = 33)]
		public bool Deceased { get; set; }
		[Display(Name = "Other address", Order = 53)]
		public string Otheraddress { get; set; }
		[Display(Name = "Previous address in Israel", Order = 54)]
		public string PreviousaddressinIsrael { get; set; }
		[Display(Name = "Phone", Order = 55)]
		public string Phone { get; set; }
		//[Display(Name = "Austrian Eligible", Order = 37)]
		//public bool AustrianEligible { get; set; }
		//[Display(Name = "Romanian Eligible", Order = 36)]
		//public bool RomanianEligible { get; set; }		
		[Display(Name = "Unmet Needs Start Date (highest date if multiple entries)", Order = 27)]
		public DateTime? UnmetNeedsStartDate { get; set; }
		[Display(Name = "Unmet Needs Weekly Hours", Order = 28)]
		public decimal? UnmetNeedsValue { get; set; }
        [Display(Name = "Appeared at least once", Order = 36)]
        public string AppearedAtLeastOnce { get; set; }
		[Display(Name = "Homecare Approval Status", Order = 7)]
		public string HomecareApprovalStatusName { get; set; }
		[Display(Name = "Homecare Waitlist", Order = 63)]
		public string HomecareWaitlist { get; set; }
        [Display(Name = "Unable To Sign", Order = 67)]
        public string UnableToSign { get; set; }
        [Display(Name = "Nursing Home", Order = 69)]
        public string NursingHome { get; set; }

        [Display(Name = "Assisted Living", Order = 70)]
        public string AssistedLiving { get; set; }
        [Display(Name = "Other Services Waitlist", Order = 64)]
        public string OtherServicesWaitlist { get; set; }
		[Display(Name = "MAF Date", Order = 18)]
		public DateTime? MAFDate { get; set; }
		[Display(Name = "MAF 105+ Date", Order = 19)]
		public DateTime? MAF105Date { get; set; }
       // [Display(Name = "HAS2 Date", Order = 68)] 
       // public DateTime? HAS2Date { get; set; }
		[Display(Name = "Comm Prefs", Order = 65)]
		public string CommPrefs { get; set; }
		[Display(Name = "Care Received Via", Order = 66)]
		public string CareReceivedVia { get; set; }
      //  [Display(Name = "DAF ID", Order = 68)]
       // public int? DAFID { get; set; }
       
        // [Display(Name = "GG Reported?", Order = 35)]
        // public string GGReportedOnly { get; set; }

        // [Display(Name = "Article 2 / CEEF recipient?", Order = 39)]
        // public string IsCeefRecipient { get; set; }

        //  [Display(Name = "Article 2 / CEEF registration number", Order = 40)]
        //  public string CeefId { get; set; }
    }

    
    public class ClientsExportModel : ClientsExportModelBMF
    {
    	[Display(Name = "GG Reported?", Order = 35)]
    	public string GGReportedOnly { get; set; }		

    	[Display(Name = "Article 2 / CEEF recipient?", Order = 39)]
    	public string IsCeefRecipient { get; set; }

    	[Display(Name = "Article 2 / CEEF registration number", Order = 40)]
    	public string CeefId { get; set; }

        [Display(Name = "Austrian Eligible", Order = 37)]
         public bool AustrianEligible { get; set; }

         [Display(Name = "Romanian Eligible", Order = 36)]
         public bool RomanianEligible { get; set; }

        [Display(Name = "MasterId", Order = 3)]
        public int? MasterId { get; set; }
    }

}