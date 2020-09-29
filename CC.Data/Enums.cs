using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CC.Data
{
	public enum FixedRoles
	{
		[Display(Name = "Administrator")]
		Admin = 1,
		
		[Display(Name = "Agency User")]
		AgencyUser = 2,
		
		[Obsolete]
		AgencyOfficer = 4,

		[Display(Name = "Regional Officer")]
		RegionOfficer = 8,

		[Display(Name = "Global Officer")]
		GlobalOfficer = 16,

		[Display(Name = "SER")]
		Ser = 32,

        [Display(Name = "Regional Assistant")]
        RegionAssistant = 64,

        [Display(Name = "BMF Viewing Only")]
        BMF = 128,

        [Display(Name = "Global Read Only")]
        GlobalReadOnly = 256,

        [Display(Name = "Region Read Only")]
        RegionReadOnly = 512,

        [Display(Name = "Auditor Read Only")]
        AuditorReadOnly = 1024,

		[Display(Name="DAF Evaluator")]
		DafEvaluator = 2048,

		[Display(Name="DAF Reviewer")]
		DafReviewer = 4096,

		[Display(Name = "CFS Admin")]
		CfsAdmin = 8192,

		[Display(Name = "Agency User And Reviewer")]
		AgencyUserAndReviewer = 16384,

		[Display(Name = "SER User And Reviewer")]
		SerAndReviewer = 32768,

        [Display(Name = "Maintenance")]
        Maintenance = 32769,

        [Display(Name = "FLUXX User")]
        Fluxx_User = (32768*2) | 1, // 
    }



	public enum GenericLevelEnum
	{
		Low = 1,
		Medium = 2,
		High = 4
	}

	/// <summary>
	/// Clients Approval Statuses
	/// </summary>
	public enum ApprovalStatusEnum
	{
		[Display(Name = "New")]
		New = 1,

		[Display(Name = "Approved")]
		Approved = 2,

		[Display(Name = "Not Eligible")]
		NotEligible = 4,

		[Display(Name = "Processing")]
		Processing = 8,

		[Display(Name = "Pending")]
		Pending = 16,

		[Display(Name = "Need to Apply")]
		NeverAppliedToCc = 32,

		[Display(Name = "Agency Action Needed")]
		ApprovedTemp = 64,

		[Display(Name = "Approved, Needs Client Follow Up")]
		ApprovedFollowUp = 128,

		[Display(Name = "Rejected-Pending")]
		RejectedPending = 256,

        [Display(Name = "Research in Progress")]
		ResearchInProgress = 512,

		[Display(Name = "Approved, Homecare Only")]
		ApprovedHomecareOnly = 1024,

		[Display(Name = "Research in Progress with Proof")]
		ResearchinProgresswithProof = 2048,

		[Display(Name = "Research in Progress with No Proof")]
		ResearchinProgresswithNoProof = 4096,

	}

	public enum AppBudgetApprovalStatuses
	{
		New = 0,
		[Display(Name = "Awaiting Regional Program Officer Approval")]
		AwaitingRegionalPoApproval = 2,
		[Display(Name = "Awaiting Global Program Officer Approval")]
		AwaitingGlobalPoApproval = 3,
		[Display(Name = "Rejected")]
		Rejected = 4,
		Approved = 5,
		Cancelled = 6,
        [Display(Name = "Returned to Agency")]
        ReturnedToAgency = 7,
        [Display(Name = "Awaiting Agency Response by Regional Program Officer ")]
        AwaitingAgencyResponseRPO = 8,
        [Display(Name = "Awaiting Agency Response by Global Program Officer ")]
        AwaitingAgencyResponseGPO = 9,
    }

	public enum LeaveReasonEnum
	{
		MovedAway = 1,
		Deceased = 2,
		Other = 3,
		[Display(Name = "Nursing Home/Old Age Home")]
		NhOah = 8,
        [Display(Name = "Did not Apply")]
        DidNotApply = 9,
        [Display(Name = "Refused to Apply")]
        RefusedToApply = 10,
        [Display(Name = "No Contact")]
        NoContact = 11,
        [Display(Name = "Inactive Client")]
        InactiveClient = 12,
		[Display(Name = "Not Eligible")]
        NotEligible = 13
	}


	public enum Contsts
	{
		HomeCarecCapOddDays = 6
	}

    public enum AutomatedReportsEnum
    {
        [Display(Name = "Financial Report Approval Status Report")]
        FinancialReportApprovalStatusReport,
        [Display(Name = "Functionality Score Change Report")]
        FunctionalityScoreChangeReport,
        [Display(Name = "Deceased Date Entry Report")]
        DeceasedDateEntryReport
    }
}
