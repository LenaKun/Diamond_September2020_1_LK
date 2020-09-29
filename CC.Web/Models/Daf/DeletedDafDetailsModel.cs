using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CC.Web.Models
{
	public class DeletedDafDetailsModel
	{

		public int AgencyId { get; set; }

		[Display(Name = "AssessmentDate", ResourceType = typeof(Resources.Resource))]
		public DateTime? AssessmentDate { get; set; }

		[Display(Name = "ClientFirstName", ResourceType = typeof(Resources.Resource))]
		public string ClientFirstName { get; set; }

		[Display(Name = "ClientId", ResourceType = typeof(Resources.Resource))]
		public int ClientId { get; set; }

		[Display(Name = "ClientLastName", ResourceType = typeof(Resources.Resource))]
		public string ClientLastName { get; set; }

		public string ClientName { get; set; }

		[Display(Name = "AdditionalComments", ResourceType = typeof(Resources.Resource))]
		public string Comments { get; set; }

		[Display(Name = "CreateDate", ResourceType = typeof(Resources.Resource))]
		public DateTime CreatedAt { get; set; }

		public int CreatedBy { get; set; }

		public string CreatedByName { get; set; }

		public DateTime DeletedAt { get; set; }

		public int DeletedBy { get; set; }

		public string DeletedByName { get; set; }

		[Display(Name = "EffectiveDate", ResourceType = typeof(Resources.Resource))]
		public DateTime? EffectiveDate { get; set; }

		public int EvaluatorId { get; set; }

		[Display(Name = "EvaluatorName", ResourceType = typeof(Resources.Resource))]
		public string EvaluatorName { get; set; }

		[Display(Name = "ExceptionalHours", ResourceType = typeof(Resources.Resource))]
		public decimal? ExceptionalHours { get; set; }

		[Display(Name = "GovernmentHours", ResourceType = typeof(Resources.Resource))]
		public decimal? GovernmentHours { get; set; }

		public int Id { get; set; }

		public int StatusId { get; set; }

		public DateTime? UpdatedAt { get; set; }

		public int? UpdatedById { get; set; }

		public string UpdatedByName { get; set; }

		public string Xml { get; set; }

		public string StatusName { get; set; }

		public string AgencyName { get; set; }

		[Display(Name = "TotalNumericScore", ResourceType = typeof(Resources.Resource))]
		public decimal TotalScore { get; set; }

		[Display(Name = "EvaluatorPosition", ResourceType = typeof(Resources.Resource))]
		public string EvaluatorPosition { get; set; }

		public int? UpdatedBy { get; set; }

		[Display(Name = "EvaluatorSigned", ResourceType = typeof(Resources.Resource))]
		public DateTime? SignedAt { get; set; }

		public int? SignedBy { get; set; }

		public string SignedByName { get; set; }

		public DateTime? ReviewedAt { get; set; }

		[Display(Name = "ReviewerSigned", ResourceType = typeof(Resources.Resource))]
		public int? ReviewedBy { get; set; }

		[Display(Name = "ReviewedBy", ResourceType = typeof(Resources.Resource))]
		public string ReviewedByName { get; set; }

		public int AgencyGroupId { get; set; }

		public List<CC.Data.Models.DafQuestion> Questions { get; set; }

		public string Culture { get; set; }
	}
}