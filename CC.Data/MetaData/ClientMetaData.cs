using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;


namespace CC.Data.MetaData
{
	public class ClientMetaData
	{

		[DisplayName("CC ID")]
		[Key]
		public int Id { get; set; }

		[DisplayName("Internal Agency ID")]
		public int? InternalId { get; set; }

		[DisplayName("First Name")]
		[Required(AllowEmptyStrings = false)]
		[RegularExpression(@"^[\u0020-\u003e\u0040-\u007F]+$$")]
		[MaxLength(50)]
		public string FirstName { get; set; }

		[DisplayName("Middle Name")]
		[RegularExpression(@"^[\u0020-\u003e\u0040-\u007F]+$$")]
		[MaxLength(50)]
		public string MiddleName { get; set; }

		[DisplayName("Last Name")]
		[Required(AllowEmptyStrings = false)]
		[RegularExpression(@"^[\u0020-\u003e\u0040-\u007F]+$$")]
		[MaxLength(50)]
		public string LastName { get; set; }

		[MaxLength(50)]
		[RegularExpression(@"^[\u0020-\u003e\u0040-\u007F]+$$")]
		[DisplayName("Government Issued ID")]
		public string NationalId { get; set; }

		[DisplayName("ID Type")]
		[RegularExpression(@"^[\u0020-\u003e\u0040-\u007F]+$$")]
		public string NationalIdType { get; set; }


		[Display(Name="Agency")]
        [Required]
        public int? AgencyId { get; set; }

		public Agency Agency { get; set; }

		[DisplayName("Home Care (HC) Entitled")]
		[RegularExpression(@"^[\u0020-\u003e\u0040-\u007F]+$$")]
		public string HomeCareEntitled { get; set; }

		[DisplayName("State")]
		public int? StateId { get; set; }

		[DisplayName("City")]
		[Required()]
		[MaxLength(50)]
		[RegularExpression(@"^[\u0020-\u003e\u0040-\u007F]+$$")]
		public string City { get; set; }

		[DisplayName("Address")]
		[Required()]
		[MaxLength(255)]
		[UIHint("50")]
		[RegularExpression(@"^[\u0020-\u003e\u0040-\u007F]+$$")]
		public string Address { get; set; }

		[DisplayName("Birth Date")]
		[DateFormat()]
		[DataType(DataType.Date)]
		[UIHint("BirthDate")]
		[Required()]
		[RangeAttributeEx(typeof(DateTime), "1753-01-01", "1946-02-08", "dd-MMM-yyyy")]
		public DateTime? BirthDate { get; set; }


		[DateFormat()]
		[DataType(DataType.Date)]
		
		[Required()]
		[Display(Name = "Join Date", Description = @"Please enter the date client first started receiving services from your agency. 
			If the exact information is not known, you may use January 1st of the year the client started receiving services.")]
		public DateTime JoinDate { get; set; }

		[Display(Name = "Leave Date", Description = @"The Leave Date entered should be when the client stops receiving services from your agency for an extended period of time. 
			Short term hospital stays do not require a leave date entry. 
			If a Client has left your agency and rejoins, keep the existing Join Date, delete the Leave Date and create a new line in the Eligibility section.
			")]
		[DateFormat()]
		[DataType(DataType.Date)]
		public DateTime LeaveDate { get; set; }

		[DateFormat()]
		[DataType(DataType.Date)]
		[Display(Name = "Auto Leave Override Until")]
		public DateTime AutoLeaveOverride { get; set; }

		[DisplayName("Leave Reason")]
		public string LeaveReason { get; set; }

       
        [DisplayName("DCC Subsidy")]
        public string DCC_Subside { get; set; }

        [DisplayName("SC")]
        public bool SC_Client { get; set; }


        [DisplayName("DCC")]
        public bool DCC_Client { get; set; }


        [DisplayName("DCC VisitCost")]
        public decimal DCC_VisitCost { get; set; }

		[DisplayName("CC Subsidy")]
        public decimal SC_MonthlyCost { get; set; }

		[DisplayName("Leave Remarks")]
		[MaxLength(255)]
		public string LeaveRemarks { get; set; }

		[DisplayName("Deceased Date")]
		[DateFormat()]
		[DataType(DataType.Date)]
		public string DeceasedDate { get; set; }

		[UIHint("LongString")]
		[DisplayName("Nazi Persecution Summary (255 chars)")]
		[MaxLength(255)]
		[RegularExpression(@"^[\u0020-\u003e\u0040-\u007F]+$$")]
		public string NaziPersecutionDetails { get; set; }

		[UIHint("LongString")]
		[MaxLength(255)]
		[RegularExpression(@"^[\u0020-\u003e\u0040-\u007F]+$$")]
		public string Remarks { get; set; }

		[DisplayName("Compensation Program")]
		[MaxLength(50)]
		[RegularExpression(@"^[\u0020-\u003e\u0040-\u007F]+$$")]
		public string CompensationProgramName { get; set; }

		[UIHint("Gender")]
        [Required]
		public int? Gender { get; set; }
		#region personal details

		[DisplayName("Birth City")]
		[MaxLength(50)]
		[RegularExpression(@"^[\u0020-\u003e\u0040-\u007F]+$$")]
		public string PobCity { get; set; }

		[Required]
		[DisplayName("Birth country")]
		public int BirthCountryId { get; set; }

        [Required]
        [DisplayName("Country of Residence")]
        [UIHint("CountryIdPicker")]
        public int CountryId { get; set; }

		[DisplayName("Previous first name")]
		[MaxLength(50)]
		[RegularExpression(@"^[\u0020-\u003e\u0040-\u007F]+$$")]
		public string PrevFirstName { get; set; }
		[DisplayName("Previous Last name")]
		[MaxLength(50)]
		[RegularExpression(@"^[\u0020-\u003e\u0040-\u007F]+$$")]
		public string PrevLastName { get; set; }

		[DisplayName("Other first name")]
		[MaxLength(50)]
		[RegularExpression(@"^[\u0020-\u003e\u0040-\u007F]+$$")]
		public string OtherFirstName { get; set; }

		[DisplayName("Other last name")]
		[MaxLength(50)]
		[RegularExpression(@"^[\u0020-\u003e\u0040-\u007F]+$$")]
		public string OtherLastName { get; set; }


		[DisplayName("Other date of birth")]
		[DateFormat()]
		[DataType(DataType.Date)]
		[UIHint("BirthDate")]
		[RangeAttributeEx(typeof(DateTime), "1753-01-01", "1946-02-08", "dd-MMM-yyyy")]
		public DateTime OtherDob { get; set; }

		[DisplayName("Other ID card")]
		[MaxLength(50)]
		[RegularExpression(@"^[\u0020-\u003e\u0040-\u007F]+$$")]
		public string OtherId { get; set; }

		[DisplayName("Other ID type")]
		public int? OtherIdTypeId { get; set; }

		[DisplayName("Other address")]
		[UIHint("50")]
		[MaxLength(255)]
		[RegularExpression(@"^[\u0020-\u003e\u0040-\u007F]+$$")]
		public string OtherAddress { get; set; }

		[DisplayName("Previous address in israel")]
		[UIHint("50")]
		[MaxLength(50)]
		[RegularExpression(@"^[\u0020-\u003e\u0040-\u007F]+$$")]
		public string PreviousAddressInIsrael { get; set; }

		[DisplayName("Homecare Waitlist")]
		public bool HomecareWaitlist { get; set; }

        [DisplayName("Unable To Sign")]
        public bool UnableToSign{ get; set; }

        [DisplayName("Other Services Waitlist")]
		public bool OtherServicesWaitlist { get; set; }

		[DisplayName("Communications Preference")]
		public int? CommPrefsId { get; set; }

		[DisplayName("Care Received Via")]
		public int? CareReceivedId { get; set; }
		#endregion

		#region Eligibility/Disability

		[Display(Name = "Fund Status")]
		[UIHint("FundStatusPicker")]
		public int? FundStatusId { get; set; }

		[Display(Name = "Fund Status")]
		public FundStatus FundStatus { get; set; }

		[DisplayName("Current Functionality Level")]
		[UIHint("FunctionalityLevelPicker")]
		public FunctionalityScore CurrentFunctionalityLevel { get; set; }
		
		//[DisplayName("Grandfathered Hours")]
		//public decimal GfHours { get; set; }

		[DisplayName("Exceptional Hours")]
		public decimal ExceptionalHours { get; set; }

		[DisplayName("Income/Asset Criterea Verification Required")]
		[UIHint("boolYesNo")]
		public bool IncomeVerificationRequired { get; set; }

		[Display(Name = "Income Criteria Complied")]
		[UIHint("boolYesNo")]
		public bool IncomeCriteriaComplied { get; set; }

		[DisplayName("JNV Status")]

		public ApprovalStatusEnum ApprovalStatus { get; set; }

		[UIHint("ApprovalStatusPicker")]
		public int ApprovalStatusId { get; set; }

		[Display(Name = "Article 2 / CEEF recipient?")]
		public bool IsCeefRecipient { get; set; }

		[Display(Name = "Article 2 / CEEF registration number")]
		[MaxLength(50)]
		[RegularExpression(@"^[\u0020-\u003e\u0040-\u007F]+$$")]
		public string CeefId { get; set; }

		[Display(Name = "Name/s of any other compensation program/s")]
		[MaxLength(50)]
		[RegularExpression(@"^[\u0020-\u003e\u0040-\u007F]+$$")]
		public string AddCompName { get; set; }

		[Display(Name = "Registration number/s")]
		[MaxLength(50)]
		[RegularExpression(@"^[\u0020-\u003e\u0040-\u007F]+$$")]
		public string AddCompId { get; set; }

		[Display(Name = "MAF 105+ Date", Description = @"As of 01 January 2017, clients who have a HAS of 1 and are DAF level 6 or who were grandfathered to be allowed more than 105 hours of weekly homecare must have the Medical Assessment Form (MAF) for 105+ on file. If no form has been received from a medical professional, the client must be capped at 105 hours per week. If the form is on file, enter it's effective date here.")]
		[DateFormat()]
		[DataType(DataType.Date)]
		public DateTime? MAF105Date { get; set; }

		[Display(Name = "MAF Date", Description = @"As of 01 January 2017, clients who have a HAS of 1 and are DAF level 4 or 5 or who were grandfathered to be allowed more than 40 hours of weekly homecare must have the Medical Assessment Form (MAF) for 40+ on file. If no form has been received from a medical professional, the client must be capped at 40 hours per week. If the form is on file, enter it's effective date here.")]
		[DateFormat()]
		[DataType(DataType.Date)]
		public DateTime? MAFDate { get; set; }

        [Display(Name = "Additional Hours HAS 2", Description = @"HAS2 clients in Care Level 6 requiring additional hours, with a MAF 105+ on file, are eligible for additional hours above the 56 weekly cap for HAS2 clients with the BMF’s approval.  Clients marked as HAS2 additional hours have been approved by the BMF for these additional hours.")]
        [DateFormat()]
        [DataType(DataType.Date)]
        public DateTime? HAS2Date { get; set; }

        #endregion
    }

}
