using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CC.Data.Models
{

	[XmlRoot("root")]
	public class DafDetails
	{

		public DafDetails()
		{
		}

		[XmlElement("q")]
		[Required]
		public List<DafQuestion> Questions { get; set; }

		[Display(Name = "DafId", ResourceType = typeof(global::Resources.Resource))]
		public int Id { get; set; }

		[Display(Name = "ClientId", ResourceType = typeof(global::Resources.Resource))]
		public int ClientId { get; set; }

		[Display(Name = "ClientFirstName", ResourceType = typeof(global::Resources.Resource))]
		public string ClientFirstName { get; set; }

		[Display(Name = "ClientLastName", ResourceType = typeof(global::Resources.Resource))]
		public string ClientLastName { get; set; }

		[Display(Name = "Agency", ResourceType = typeof(global::Resources.Resource))]
		public string AgencyName { get; set; }

		public int AgencyId { get; set; }

		[Required]
		[Display(Name = "Evaluator", ResourceType = typeof(global::Resources.Resource))]
		public int EvaluatorId { get; set; }

		[Display(Name = "Evaluator", ResourceType = typeof(global::Resources.Resource))]
		public string EvaluatorName { get; set; }

		[DataType(System.ComponentModel.DataAnnotations.DataType.Html)]
		[System.Web.Mvc.AllowHtml]
		[Display(Name = "AdditionalComments", ResourceType = typeof(global::Resources.Resource))]
		public string Comments { get; set; }

		[Display(Name = "CreateDate", ResourceType = typeof(global::Resources.Resource))]
		public DateTime CreateDate { get; set; }

		[Display(Name = "Updated", ResourceType = typeof(global::Resources.Resource))]
		public DateTime? UpdateDate { get; set; }

		[Display(Name = "EvaluatorPosition", ResourceType = typeof(global::Resources.Resource))]
		public string EvaluatorPosition { get; set; }

		[Display(Name = "ExceptionalHours", ResourceType = typeof(global::Resources.Resource))]
		[System.ComponentModel.DataAnnotations.Range(0, 100)]
		public decimal? ExceptionalHours { get; set; }

		[Display(Name = "GovernmentHours", ResourceType = typeof(global::Resources.Resource))]
		public decimal? GovernmentHours { get; set; }

		[Display(Name = "ReviewerSigned", ResourceType = typeof(global::Resources.Resource))]
		public DateTime? ReviewDate { get; set; }

		[Display(Name = "ReviewedBy", ResourceType = typeof(global::Resources.Resource))]
		public string ReviewerName { get; set; }

		[Display(Name = "TotalScore", ResourceType = typeof(global::Resources.Resource))]
		public decimal? TotalScore
		{
			get
			{
				if (Questions == null)
				{
					return null;
				}
				else
				{
					var score = (from q in Questions
								 let a = q.Options.Where(o => o.Id == q.SelectedAnswerId).Select(f => f.Score).FirstOrDefault()
								 select a).Sum();
					return score;
				}
			}
		}

		public int StatusId { get; set; }

		[Display(Name = "DafStatus", ResourceType = typeof(global::Resources.Resource))]
		public Daf.Statuses Status { get { return (Daf.Statuses)StatusId; } }

		public string StatusName { get { return this.Status.DisplayName(); } }

		[Required]
		[UIHint("Date")]
		[Display(Name = "AssessmentDate", ResourceType = typeof(global::Resources.Resource))]
		public DateTime? AssessmentDate { get; set; }

		[Required]
		[UIHint("Date")]
		[Display(Name = "EffectiveDate", ResourceType = typeof(global::Resources.Resource))]
		public DateTime EffectiveDate { get; set; }

		[Display(Name = "EvaluatorName", ResourceType = typeof(global::Resources.Resource))]
		public string SignerName { get; set; }

		[Display(Name = "EvaluatorSigned", ResourceType = typeof(global::Resources.Resource))]
		public DateTime? SignDate { get; set; }



		[Display(Name = "FunctionalCapacity", ResourceType = typeof(global::Resources.Resource))]
		public string FunctionalityLevelName { get; set; }

		public string Culture { get; set; }

		public DateTime? DownloadedAt { get; set; }

		public int? DownloadedBy { get; set; }

		public string DownloadedTo { get; set; }
		public DateTime? UploadedAt { get; set; }

		public int? UploadedBy { get; set; }

		public string UploadedTo { get; set; }


		public string DownloaderFullName { get; set; }

		public string DownloaderUsername { get; set; }

		[Display(Name = "SignedDafUpload", ResourceType = typeof(global::Resources.Resource))]
		public string FileName { get; set; }

		public string UploadedFile { get; set; }

		[Display(Name = "DafCreateDisclaimer", ResourceType = typeof(global::Resources.Resource))]
		public bool Disclaimer { get; set; }
        public DateTime? UserConsentObtainedAt { get; set; }
    }
	public class DafQuestion
	{
		public DafQuestion()
		{
			this.Options = new List<DafAnswer>();
		}

		[XmlAttribute("id")]
		public int Id { get; set; }

		[XmlAttribute("text")]
		public string Text { get; set; }

		[XmlElement("a")]
		public List<DafAnswer> Options { get; set; }
		
		public int? SelectedAnswerId { get; set; }

		public string SelectedText
		{
			get
			{
				if (this.Options == null)
				{
					return null;
				}
				else
				{
					return this.Options.Where(f => f.Id == this.SelectedAnswerId).Select(f => f.Text).FirstOrDefault();
				}
			}
		}
	}
	public class DafAnswer
	{
		[XmlAttribute("id")]
		public int Id { get; set; }

		[XmlAttribute("score")]
		public decimal Score { get; set; }

		[XmlText]
		public string Text { get; set; }
	}

}
