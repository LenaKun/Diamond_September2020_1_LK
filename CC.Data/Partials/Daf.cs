using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace CC.Data
{
	[MetadataType(typeof(DafMetadata))]
	public partial class Daf : IValidatableObject
	{
		public Daf()
		{
			this.CreatedAt = DateTime.Now;
			this.EffectiveDate = DateTime.Today;
			this.Status = Statuses.Open;
		}

		public Statuses Status { get { return (Statuses)this.StatusId; } set { this.StatusId = (int)value; } }

		public enum Statuses
		{
			[Display(Name = "DafStatusOpen", ResourceType = typeof(global::Resources.Resource))]
			Open = 0,

			[Display(Name = "DafStatusSigned", ResourceType = typeof(global::Resources.Resource))]
			EvaluatorSigned = 1,

			[Display(Name = "DafStatusCompleted", ResourceType = typeof(global::Resources.Resource))]
			Completed = 2
		}


		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			yield break;

		}


	}
	#region metadata
	public class DafMetadata
	{
		[Display(Name = "DAF ID")]
		public virtual int Id
		{
			get;
			set;
		}


		public virtual int EvaluatorId
		{
			get;
			set;
		}

		public virtual int ClientId
		{
			get;
			set;
		}

		public virtual System.DateTime CreateDate
		{
			get;
			set;
		}

		public virtual Nullable<System.DateTime> UpdateDate
		{
			get;
			set;
		}

		public virtual System.DateTime AssessmentDate
		{
			get;
			set;
		}

		public virtual int GovernmentHours
		{
			get;
			set;
		}

		public virtual int ExceptionalHours
		{
			get;
			set;
		}

		public virtual string AdditionalComments
		{
			get;
			set;
		}

		public virtual string EvaluatorPosition
		{
			get;
			set;
		}

		public virtual Nullable<System.DateTime> EvaluatorSignedDate
		{
			get;
			set;
		}

		public virtual Nullable<int> ReviewerId
		{
			get;
			set;
		}

		public virtual Nullable<System.DateTime> ReviewerSignDate
		{
			get;
			set;
		}

		public virtual Nullable<System.DateTime> EffectiveDate
		{
			get;
			set;
		}

		public virtual Nullable<int> FunctionalityScoreId
		{
			get;
			set;
		}

	}
	#endregion
}
