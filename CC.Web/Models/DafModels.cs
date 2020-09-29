using System;
using CC.Web.Controllers.Attributes;
using CC.Data;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CC.Web.Models
{
	public class DafIndexRowModel
	{

		public int DafId { get; set; }

		public int ClientId { get; set; }

		public DateTime Created { get; set; }

		public DateTime? Updated { get; set; }

		public string EvaluatorName { get; set; }

		public string ClientName { get; set; }

		public string StatusName { get; set; }
	}

	public class DafIndexModel
	{
		public DafIndexModel()
		{
			this.Filter = new DafIndexModelFilter();
		}

		public DafIndexModel(System.Security.Principal.IPrincipal User)
		{
			this.Filter = new DafIndexModelFilter(User);
		}
		public DafIndexModelFilter Filter { get; set; }

		/// <summary>
		/// create new daf cc id
		/// </summary>
		[Display(Name = "ClientId", ResourceType = typeof(Resources.Resource))]
		public int? ClientId { get; set; }
	}
	public class DafIndexModelFilter : jQueryDataTableParamModel
	{
		public DafIndexModelFilter()
			: base()
		{

		}

		public DafIndexModelFilter(System.Security.Principal.IPrincipal User)
			: this()
		{
			if (User.IsInRole(FixedRoles.DafEvaluator))
			{
				this.Status = Daf.Statuses.Open;
			}
			else if (User.IsInRole(FixedRoles.DafReviewer) || User.IsInRole(FixedRoles.AgencyUserAndReviewer) || User.IsInRole(FixedRoles.SerAndReviewer))
			{
				this.Status = Daf.Statuses.EvaluatorSigned;
			}
		}

		[Display(Name = "DafStatus", ResourceType = typeof(Resources.Resource))]
		[UIHint("DafStatus")]
		public Daf.Statuses? Status { get; set; }

		[Display(Name = "SER", ResourceType = typeof(Resources.Resource))]
		public int? SerId { get; set; }

		[Display(Name = "Agency", ResourceType = typeof(Resources.Resource))]
		public int? AgencyId { get; set; }

		[Display(Name = "DafId", ResourceType = typeof(Resources.Resource))]
		public int? DafId { get; set; }

		[Display(Name = "ClientId", ResourceType = typeof(Resources.Resource))]
		public new int? ClientId { get; set; }

		[Display(Name = "FirstName", ResourceType = typeof(Resources.Resource))]
		public string FirstName { get; set; }

		[Display(Name = "LastName", ResourceType = typeof(Resources.Resource))]
		public string LastName { get; set; }

		[Display(Name = "Search", ResourceType = typeof(Resources.Resource))]
		public string Search { get; set; }
	}



	/// <summary>
	/// Contains only required props to create and go to details/edit
	/// </summary>
	public class DafDetailsModel : DafCreateModel
	{
		public DafDetailsModel()
			: base()
		{
		}
		[Required]
		public override DateTime? AssessmentDate { get; set; }
		[Required]
		public override DateTime CreateDate { get; set; }
		[Required]
		public override decimal? GovernmentHours { get; set; }

		public int StatusId { get; set; }

		public Daf.Statuses Status { get { return (Daf.Statuses)this.StatusId; } }

		public string RawXml { get; set; }

	}



	public class DafCreateModel : IValidatableObject
	{
		public DafCreateModel()
		{
		}
		#region header details

		public int Id { get; set; }

		[Required]
		[Display(Name = "ClientId", ResourceType = typeof(global::Resources.Resource))]
		public int ClientId { get; set; }

		public string AgencyName { get; set; }

		public int AgencyId { get; set; }

		public string ClientFirstName { get; set; }

		public string ClientLastName { get; set; }

		#endregion

		[Required]
		public int? EvaluatorId { get; set; }

		public bool CanEditEvaluatorId { get; set; }

		[UIHint("Date")]
		public virtual DateTime? AssessmentDate { get; set; }

		public virtual decimal? GovernmentHours { get; set; }

		public virtual DateTime CreateDate { get; set; }

		public decimal? TotalScore { get; set; }

		public int FunctionalityLevelId { get; set; }

		public string FunctionalityLevelName { get; set; }
		public decimal? ExceptionalHours { get; set; }
		public string Comments { get; set; }

		[Display(Name = "EvaluatorName", ResourceType = typeof(Resources.Resource))]
		public string EvaluatorName { get; set; }
		[Display(Name = "EvaluatorPosition", ResourceType = typeof(Resources.Resource))]
		public string EvaluatorPosition { get; set; }

		[Display(Name = "EvaluatorSigned", ResourceType = typeof(Resources.Resource))]
		public DateTime? EvaluatorSignDate { get; set; }

		[Display(Name = "ReviewedBy", ResourceType = typeof(Resources.Resource))]
		public string ReviewerName { get; set; }

		[Display(Name = "Reviewer Signed", ResourceType = typeof(Resources.Resource))]
		public DateTime? ReviewDate { get; set; }
		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			yield break;
		}
		public string FileName { get; set; }
		[Display(Name = "DafCreateDisclaimer", ResourceType = typeof(Resources.Resource))]
		public bool Disclaimer { get; set; }

	}
	public class DafQuestionModel
	{
		public string Question { get; set; }
		public string Answer { get; set; }
		public decimal Score { get; set; }

		public object Answers { get; set; }
	}

}