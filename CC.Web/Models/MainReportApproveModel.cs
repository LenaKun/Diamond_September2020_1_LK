using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CC.Data;
using System.ComponentModel.DataAnnotations;


namespace CC.Web.Models
{


	public class SubmissionDetails : IValidatableObject
	{
		public SubmissionDetails()
		{
			ProgramOverviewRequired = true;
			DisclaimerRequired = true;
            DisclaimerRequired1 = true;
        }

		public int Id { get; set; }

		/// <summary>
		///1.4.7.5.        Program Overview Text (rich text)
		///1.4.7.5.1.        In each Main report submitted by the SER (New / Returned to Agency), the SER\agency will be forced to enter a program overview text, in a rich text format.
		///1.4.7.5.1.1.        Users will be able to paste in texts, and the application will auto remove any styling of the pasted texts automatically.
		///1.4.7.5.1.2.        Appears only in reports that contain emergencies services rows.
		/// </summary>
		public bool ProgramOverviewRequired { get; set; }
		public bool AdministrativeOverheadOverflow { get; set; }

		[Display(Name = "Program Overview ")]
		[DataType(DataType.Html)]
		[System.Web.Mvc.AllowHtml]
		public string ProgramOverview { get; set; }

		[Display(Name = "The total Administrative Overhead requested up to date exceeds the estimated amount, please enter a reason.")]
		[DataType(DataType.Html)]
		[System.Web.Mvc.AllowHtml]
		public string AdministrativeOverheadOverflowReason { get; set; }

		[Display(Name = "A meeting of the advisory committee was held within the quarter")]
		public bool AcMeetingHeld { get; set; }

		[Display(Name = "Holocaust Survivor Advisory Committee Minutes (If this report is RETURNED TO AGENCY, and no changes to Minutes then just specify \"No Changes\")")]
		[DataType(DataType.Html)]
		[System.Web.Mvc.AllowHtml]
		public string Mhsa { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public bool RemarksRequired { get; set; }

		[DataType(DataType.Html)]
		[System.Web.Mvc.AllowHtml]
		[Display(Name = "The current calculated Required Match / Agency’s contribution is lower than expected at this point, please specify a reason:")]
		public string Remarks { get; set; }


		/// <summary>
		/// 1.4.7.7.        Disclaimer text and check box (mandatory) “I Approve that the content I provided here is liable, and understands….”
		/// </summary>
		[Display(Name = "I confirm the content of the minutes are a factual representation of the committee meeting.")]
		public bool Disclaimer { get; set; }
		public bool DisclaimerRequired { get; set; }

        //Lena Kunisky additional checkbox for financial reports
        /// 1.4.7.8.        Disclaimer text and check box (mandatory) “I acknowledge that the expenditures submitted in this Diamond….”
		/// </summary>
		[Display(Name = "I acknowledge that the expenditures submitted in this Diamond report were spent as part of the Claims Conference Grant as indicated per individual entries and budget lines,  and not for any other purpose(s).  Further, items charged to this Diamond report have not been charged to any other funding or funding sources..")]
        public bool Disclaimer1 { get; set; }
        public bool DisclaimerRequired1 { get; set; }

        [DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
		[Required]
		public string Password { get; set; }
		[Display(Name = "username")]
		public string UserName { get; set; }

		[Display(Name = "Program Overview Document (pdf only)")]
		public string ProgramOverviewFileName { get; set; }
		public string ProgramOverviewUploadedFile { get; set; }

		[Display(Name = "Holocaust Survivor Advisory Committee Minutes Document (pdf only)")]
		public string MhsaFileName { get; set; }
		public string MhsaUploadedFile { get; set; }



		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{			
			if (this.RemarksRequired)
			{
				if (this.Remarks==null || this.Remarks.IsNullOrEmptyHtml())
				{
					yield return new ValidationResult("The current calculated Required Match / Agency’s contribution is lower than expected at this point, please specify a reason:", new[] { this.PropertyName(f => f.Remarks) });
				}
			}			
			if (this.DisclaimerRequired && !this.Disclaimer)
			{
				yield return new ValidationResult("Please check the disclaimer to proceed...");
			}
            if (this.DisclaimerRequired1 && !this.Disclaimer1)
            {
                yield return new ValidationResult("Please check the second disclaimer(acknowledge that the expenditures submitted in this Diamond report were spent as part of the Claims Conference Grant) to proceed...");
            }
        }
	}


	public class MainReportApproveModel:IValidatableObject
	{
		public MainReportApproveModel() { }
		[Required]
		public int Id { get; set; }
		public MainReport.Statuses NewStatus { get; set; }
		private string _remarks;
		[Required]
		[DataType(System.ComponentModel.DataAnnotations.DataType.Html)]
		[System.Web.Mvc.AllowHtml]
		public string RejectionRemarks { get { return _remarks; } set { _remarks = value.IsNullOrEmptyHtml() ? null : value; } }
		[DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
		[Required]
		public string Password { get; set; }
		[Display(Name = "username")]
		public string UserName { get; set; }
		public bool RequiresAdminApproval { get; set; }
		public FixedRoles UserRole { get; set; }
		public MainReport.Statuses MainReportStatus { get; set; }
		public MainReport.Statuses? PrevMainReportStatus { get; set; }
        [Display(Name = "Last Report")]
        public bool LastReport { get; set; }
        [Display(Name = "Cancellation Amount")]
        public decimal? CancellationAmount { get; set; }
        public string CurrencyId { get; set; }
		public bool AgencyGroupIsAudit { get; set; }
		
		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(NewStatus==MainReport.Statuses.Rejected && this.RejectionRemarks.IsNullOrEmptyHtml())
			{
				yield return new ValidationResult("Rejection remarks are required.");
			}
		}

		public bool ShowPoWarning
		{
			get
			{
				return this.RequiresAdminApproval
					&& (this.UserRole == FixedRoles.RegionOfficer || this.UserRole == FixedRoles.RegionAssistant)
					&& (this.MainReportStatus == MainReport.Statuses.AwaitingProgramAssistantApproval || this.MainReportStatus == MainReport.Statuses.AwaitingProgramOfficerApproval 
						|| this.MainReportStatus == MainReport.Statuses.AwaitingAgencyResponse);
			}
		}
			
	}


}