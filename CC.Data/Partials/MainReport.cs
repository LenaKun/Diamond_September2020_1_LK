using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace CC.Data
{
	[MetadataType(typeof(CC.Data.MetaData.MainReportMetaData))]
	public partial class MainReport : System.ComponentModel.DataAnnotations.IValidatableObject
	{
		#region Constructors

		public MainReport()
		{
			this.ExcRate = 1;
			this.Status = Statuses.New;
		}

		#endregion

		#region Calculated Public Properties

		public Range<DateTime> Period { get { return new Range<DateTime>() { Start = this.Start, End = this.End }; } }
		public Statuses Status { get { return (Statuses)this.StatusId; } set { this.StatusId = (int)value; } }
		public bool CanBeEdited { get { return MainReport.EditableStatuses.Contains(this.Status); } }
        public bool SendEmail { get; set; }

		#endregion

		#region Methods
		public void ChangeStatus(Statuses newstatus, User user, string remarks)
		{
			var prevStatus = this.Status;
			this.Status = newstatus;
			this.ApprovedAt = this.UpdatedAt = DateTime.Now;
			this.ApprovedById = this.UpdatedById = user.Id;
			if (!string.IsNullOrWhiteSpace(remarks))
			{
				this.PoComments.Add(new Comment()
				{
					Content = newstatus == Statuses.Rejected ? string.Format("{0} PO has rejected the report for the following reason/s: {1}", this.UpdatedAt, remarks) : remarks,
					Date = this.UpdatedAt,
					UserId = this.UpdatedById

				});
			}
		}
		#endregion

		#region Nested Classes/Structs/Enums

		public enum Statuses
		{
			New = 1,

			Approved = 2,

			[Display(Name = "Rejected")]
			Rejected = 4,

			[Display(Name = "Awaiting PO Approval")]
			AwaitingProgramOfficerApproval = 8,

			Cancelled = 16,

            [Display(Name = "Awaiting PA Approval")]
            AwaitingProgramAssistantApproval = 32,

            [Display(Name = "Returned to Agency")]
            ReturnedToAgency = 64,

			[Display(Name = "Awaiting Agency Response")]
			AwaitingAgencyResponse = 128
		}


		public static IEnumerable<Statuses> EditableStatuses
		{
			get
			{
				return new List<Statuses>() { Statuses.New, Statuses.Rejected , Statuses.ReturnedToAgency};
			}
		}

		public static System.Linq.Expressions.Expression<Func<MainReport, bool>> CurrentOrSubmitted(int id)
		{
			return mr => mr.Id == id 
				|| mr.StatusId == (int)MainReport.Statuses.Approved 
				|| mr.StatusId == (int)MainReport.Statuses.AwaitingProgramOfficerApproval
				|| mr.StatusId == (int)MainReport.Statuses.AwaitingProgramAssistantApproval
				|| mr.StatusId == (int)MainReport.Statuses.AwaitingAgencyResponse;
		}
		public static System.Linq.Expressions.Expression<Func<MainReport, bool>> Submitted
		{
			get
			{
				return mr => mr.StatusId == (int)MainReport.Statuses.Approved 
					|| mr.StatusId == (int)MainReport.Statuses.AwaitingProgramOfficerApproval 
					|| mr.StatusId==(int)MainReport.Statuses.AwaitingProgramAssistantApproval
					|| mr.StatusId == (int)MainReport.Statuses.AwaitingAgencyResponse;
			}
		}
		public static int[] SubmittedStatuses
		{
			get
			{
				return new[]{
					(int)MainReport.Statuses.Approved,
					(int)MainReport.Statuses.AwaitingProgramOfficerApproval,
					(int)MainReport.Statuses.AwaitingProgramAssistantApproval,
					(int)MainReport.Statuses.AwaitingAgencyResponse,
				};
			}
		}



		#endregion

		#region Validation

		public virtual IEnumerable<System.ComponentModel.DataAnnotations.ValidationResult> Validate(System.ComponentModel.DataAnnotations.ValidationContext validationContext)
		{
			var obj = validationContext.ObjectInstance as MainReport;

			if (!Enum.IsDefined(typeof(Statuses), this.StatusId))
			{
				yield return new ValidationResult("invalid status id.");
			}
			if (this.Start.Date > this.End.Date)
			{
				yield return new ValidationResult("End time must be greater than start time.");
			}
			if (this.Start.Date.Day != 1 || this.End.Date.Day != 1)
			{
				yield return new ValidationResult("The end date and start date must be the first day of a month.");
			}
			if (AcMeetingHeld && string.IsNullOrWhiteSpace(this.Mhsa) && string.IsNullOrEmpty(this.MhsaFileName))
			{
				yield return new ValidationResult(string.Format("The {0} is required.", "Minutes of the Holocaust Survivor Advisory"), new string[] { "HSAC" });
			}

			if (this.AppBudget == null)
			{
			}
			else
			{
				if (this.AppBudget.ApprovalStatus != AppBudgetApprovalStatuses.Approved)
				{
					yield return new ValidationResult("The Budget is not approved");
				}
				if (this.End < this.Start)
				{
					yield return new ValidationResult("main report end date must be greater than start date");
				}
				if (!(this.Start.Date <= this.AppBudget.App.EndDate.Date && this.End.Date >= this.AppBudget.App.StartDate.Date))
				{
					yield return new ValidationResult(string.Format("The main report start date must be between {0} and {1}", this.AppBudget.App.StartDate, this.AppBudget.App.EndDate), new string[] { "Start" });
				}
				if (this.End <= this.AppBudget.App.StartDate || this.AppBudget.App.EndDate < this.End)
				{
					yield return new ValidationResult(string.Format("The main report end date must be between {0} and {1}", this.AppBudget.App.StartDate, this.AppBudget.App.EndDate), new string[] { "Start" });
				}
			}
		}

		#endregion
	}
}
