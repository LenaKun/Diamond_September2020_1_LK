using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CC.Data
{
	[MetadataType(typeof(ClientContactMetaData))]
	public partial class ClientContact : IValidatableObject
	{
		public ClientContact()
		{
		}

		public string RelativeFilePath
		{
			get
			{
				return string.Format("~/App_Data/ClientContacts/{0}", this.Id.ToString());
			}
		}

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (this.ContactDate > DateTime.Now)
			{
				yield return new ValidationResult("Date Of Contact must be equal or lower to today");
			}

			if (this.ResponseRecievedDate > DateTime.Now)
			{
				yield return new ValidationResult("Response Received must be equal or lower to today");
			}
		}

		public List<object> ContactedOptions
		{
			get
			{
				return new List<object>
				{
					new {Text="Agency",Value="Agency"},
					new {Text="Client",Value="Client"},
					new {Text="Agency and Client",Value="Agency and Client"}
				};
			}
		}
	}

	public class ClientContactMetaData
	{
		#region Primitive Properties

		public virtual int Id
		{
			get;
			set;
		}

		[DateFormat()]
		[DataType(DataType.Date)]
		[Display(Name = "Date Of Contact")]
		public virtual System.DateTime ContactDate
		{
			get;
			set;
		}

		[Display(Name = "Contacted Using")]
		[System.ComponentModel.DataAnnotations.MaxLength(30)]
		public virtual string ContactedUsing
		{
			get;
			set;
		}

		[MaxLength(30)]
		[Display(Name = "Contacted")]
		public virtual string Contacted
		{
			get;
			set;
		}

		[MaxLength(30)]
		[Display(Name = "CC Staff Contact")]
		public virtual string CcStaffContact
		{
			get;
			set;
		}

		[MaxLength(255)]
		[Display(Name = "Reason for Contact")]
		public virtual string ReasonForContact
		{
			get;
			set;
		}

		[DateFormat()]
		[DataType(DataType.Date)]
		[Display(Name = "Response Received")]
		public virtual System.DateTime ResponseRecievedDate
		{
			get;
			set;
		}

		[DateFormat()]
		[DataType(DataType.Date)]
		[Display(Name = "Entry Date")]
		public virtual System.DateTime EntryDate
		{
			get;
			set;
		}

		[Display(Name = "User")]
		public virtual int UserId
		{
			get;
			set;
		}

		[Display(Name = "File")]
		public virtual string Filename
		{
			get;
			set;
		}

		#endregion
	}
}
