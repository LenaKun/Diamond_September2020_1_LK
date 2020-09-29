using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using CC.Data.MetaData;
using System.Linq.Expressions;
using CC.Extensions;

namespace CC.Data
{
	[MetadataType(typeof(ClientMetaData))]
	partial class Client : IValidatableObject, INotifyPropertyChanging, INotifyPropertySaving, IIntIdRecord
	{
		public static DateTime JoinDateToCompare = new DateTime(2018, 1, 1);

		public Client()
		{
			//default values
			JoinDate = DateTime.Now;
			UpdatedAt = DateTime.Now;
			this.ApprovalStatusId = (int)ApprovalStatusEnum.New;
			//subscribe to events to perform validations/enforse bl

		}

		void Client__propertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "ApprovalStatusId":
					if (System.Web.HttpContext.Current != null)
					{
						if (!System.Web.HttpContext.Current.User.IsInRole(FixedRoles.Admin.ToString()))
						{

						}
					}

					break;
			}
		}

		#region Deceased
		void Client_DeceasedDateChanged(object sender, ValueChangedEventArgs<DateTime?> e)
		{
			if (e.NewValue.HasValue)
			{
				this.LeaveDate = e.NewValue;
				this.LeaveReasonId = (int)LeaveReasonEnum.Deceased;
			}
		}
		void Client_DeceasedDateChanging(object sender, ValueChangingEventArgs<DateTime?> e)
		{

			if (e.OldValue.HasValue)
			{

				e.Cancel = true;
			}
		}

		void Client_DeceasedChanging(object sender, ValueChangingEventArgs<bool> e)
		{

		}
		void Client_DeceasedChanged(object sender, ValueChangedEventArgs<bool> e)
		{

		}
		#endregion

		#region Enums

		public ApprovalStatusEnum ApprovalStatusEnum
		{
			get
			{
				return (ApprovalStatusEnum)this.ApprovalStatusId;
			}
			set
			{
				this.ApprovalStatusId = (int)value;
			}
		}
		public enum DeleteReasons
		{
			NA = 0,
			Duplicate = 1,
			Ineligible = 2
		}
		public enum Genders
		{
			Male = 0,
			Female = 1
		}
		public static string GetGenderString(int? gender)
		{
			if (gender.HasValue)
			{
				return ((Genders)gender.Value).ToString();
			}
			else
			{
				return null;
			}
		}
		#endregion

		#region Calculated Fields

		public string FullName { get { return this.FirstName + " " + this.LastName; } }
		public bool HomeCareEntitled
		{
			get;
			set;
		}

		[UIHint("boolYesNo")]
		public bool Deceased { get { return this.DeceasedDate.HasValue || this.LeaveReasonId == (int)LeaveReasonEnum.Deceased; } }

		private FunctionalityLevel _currentFunctionalityLevel;

		public FunctionalityLevel CurrentFunctionalityLevel
		{
			get { return _currentFunctionalityLevel; }
			set { _currentFunctionalityLevel = value; }
		}
		private decimal? _currentGovHcHours;

		public decimal? CurrentGovHcHours
		{
			get { return _currentGovHcHours; }
			set { _currentGovHcHours = value; }
		}




		/// <summary>
		/// 1.1.1.1.	The actual Homecare allowed hours will be set by the maximum value of the Grandfathered hours, the Exceptional Hours, and the period related functionality level.
		/// </summary>
		/// <param name="validationContext"></param>
		/// <returns></returns>
		[DisplayName("Current Allowable HC Hours")]
		[UIHint("DecimalNA")]
		[Display(Description = "If the client has CFS record without an End Date then this value is used for CFS level calculation only.")]
		public decimal? HomeCareAllowedHours
		{
			get;
			set;
		}

		public bool HasOpenCfsRecord { get; set; }

		public bool IsIncomeVerificationRequired()
		{
			//the client or the fundstatus is marked ad required (no fund status = required)
			//the country cancels all the requirements
			return Client.IncomeVerificationRuiredExpression.Compile()(this);

		}

		public string GenderString
		{
			get
			{
				return GetGenderString(this.Gender);
			}
		}


		#endregion

		#region IValidatableObject

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			//dates
			if (this.LeaveDate.HasValue && this.LeaveDate < this.JoinDate)
			{
				yield return new ValidationResult("Leave Date must be greater than Join Date");
			}

			if (this.LeaveDate.HasValue && this.LeaveDate < this.BirthDate)
			{
				yield return new ValidationResult("Leave Date must be greater than Birth Date");
			}
			if (this.JoinDate < this.BirthDate)
			{
				yield return new ValidationResult("Join Date must be greater than Birth Date");
			}
			if (this.DeceasedDate.HasValue && this.LeaveDate.HasValue && this.LeaveDate > this.DeceasedDate)
			{
				yield return new ValidationResult("Deceased Date must be greater or equal to Leave Date");
			}


			if (this.DeceasedDate.HasValue)
			{
				if (this.LeaveDate == null)
				{
					yield return new ValidationResult("Leave Date must be specified if Deceased Date is set.");
				}
				if (this.LeaveReasonId == null)
				{
					yield return new ValidationResult("Leave Reason is not specified but Deceased Date is set.");
				}
			}

			if (this.LeaveReasonId == (int)LeaveReasonEnum.Deceased && this.DeceasedDate == null)
			{
				yield return new ValidationResult("Leave reason is set to \"Deceased\" but Client is not marked as \"Deceased\".");
			}

			var instance = validationContext.ObjectInstance as Client;


			if (instance.StateId == null)
			{
				//the following check will work only with lazy loading
				using (var db = new ccEntities())
				{
					var hasStates = db.Countries.Any(f => f.Id == instance.CountryId && f.States.Any());
					if (hasStates)
					{
						yield return new ValidationResult("State is required", new string[] { "StateId" });
					}
				}

			}


			//israely id check validation
            using (var db = new ccEntities())
            {
                var agencygroup = (from ag in db.AgencyGroups
                                   from a in ag.Agencies
                                   where a.Id == this.AgencyId
                                   select ag).FirstOrDefault();

                //day center validation
                if (agencygroup.DayCenter == true && this.DCC_Client == true)
                {
                    if (!this.DCC_Subside.HasValue)
                    {
                        yield return new ValidationResult("DCC Subside is required", new string[] { "DCC_Subside" });
                    }


                    if (!this.DCC_VisitCost.HasValue)
                    {
                        yield return new ValidationResult("Dcc Visit Cost is required", new string[] { "DCC_VisitCost" });
                    }
                    else
                    {

                        decimal vc = 0;
                        if (!Decimal.TryParse(this.DCC_VisitCost.ToString(), out  vc))
                        {

                            yield return new ValidationResult("Dcc Visit Cost must be numeric value", new string[] { "DCC_VisitCost" });

                        }
                    }


                }
                if (agencygroup.SupportiveCommunities == true && this.SC_Client == true)
                {
                    if(agencygroup.ScSubsidyLevelId ==1)
                    { this.SC_MonthlyCost = 25; }
                    else
                    { this.SC_MonthlyCost = 15; }
                    if (!this.SC_MonthlyCost.HasValue)
                    {
						yield return new ValidationResult("CC Subsidy is required", new string[] { "SC_MonthlyCost" });
                    }
                    else
                    {

                        decimal vc = 0;
                        if (!Decimal.TryParse(this.SC_MonthlyCost.ToString(), out  vc))
                        {

							yield return new ValidationResult("CC Subsidy must be numeric value", new string[] { "SC_MonthlyCost" });

                        }
                    }


                }


                if ((this.NationalIdTypeId != null && (this.NationalIdTypeId == 1)) || agencygroup.ForceIsraelID)
                {
                    if (this.NationalId.IsNullOrEmptyHtml())
                    {
                        yield return new ValidationResult("Government Issued ID is required", new string[] { "NationalId" });
                    }
                    else //validate id
                    {
                        if (this.NationalId.Length < 9)
                        {
                            yield return new ValidationResult("Government Issued ID must be 9 digits long", new string[] { "NationalId" });
                        }
                        var vid = ValidateIsraelID(this.NationalId);
                        if (vid == -1)
                            yield return new ValidationResult("Government Issued ID is not a legal israeli ID", new string[] { "NationalId" });
                        else if (vid == 0)
                            yield return new ValidationResult("Government Issued ID is not a valid israeli ID", new string[] { "NationalId" });
                        else //check duplication id in user agency clients
                        {

                            var hasSameNationalid = db.Clients.Where(c => c.AgencyId == this.AgencyId).Any(c => c.NationalId == this.NationalId && c.Id != this.Id);
                            if (hasSameNationalid)
                            {
                                yield return new ValidationResult("Government Issued ID must be unique for this agency", new string[] { "NationalId" });
                            }

                        }
                    }
                }

                var tomorrow = DateTime.Today.AddDays(1);
                var earliestDate = new DateTime(2017, 01, 01);
                if(this.MAFDate.HasValue && (this.MAFDate >= tomorrow || this.MAFDate < earliestDate))
                {
                    yield return new ValidationResult("MAF Date can't be future date or early than " + earliestDate.ToShortDateString());
                }
                if (this.MAF105Date.HasValue && (this.MAF105Date >= tomorrow || this.MAF105Date < earliestDate))
                {
                    yield return new ValidationResult("MAF 105+ Date can't be future date or early than " + earliestDate.ToShortDateString());
                }

                var earliestHAS2Date = new DateTime(2019, 12, 31);
                if (this.HAS2Date.HasValue && (this.HAS2Date >= tomorrow || this.HAS2Date < earliestHAS2Date))
                {
                    yield return new ValidationResult("HAS 2 Date can't be future date or early than " + earliestHAS2Date.ToShortDateString());
                }
            }
			if (this.HomeCareEntitled && this.BirthDate == null)
			{
				yield return new ValidationResult("Birth Date is required if the client is marked as Home Care Entitled",
					new[] { "BirthDate" });
			}

			using (var db = new ccEntities())
			{
#warning todo
				//1.3.2.3.2.1.1.1.1.1.1.1.1.1.	When editing - Leave date could not be before a reported service date that appears in any Financial Report of any status.
				//var relatedReports = db.ClientReports.Where(f => f.StartDate <= this.LeaveDate && this.LeaveDate <= f.EndDate);
				//if (relatedReports.Any())
				//{
				//    yield return new ValidationResult("Leave date could not be before a reported service date that appears in any Financial Report of any status.", new string[] { "LeaveDate" });
				//}
			}

		}



		/// <summary>
		/// method for checking valid Israel ID 
		/// </summary>
		/// <param name="IDNum">the id to validate</param>
		/// <returns>-1 iligal input,0 not valid,1 valid</returns>
		protected int ValidateIsraelID(string IDNum)
		{

			// Validate correct input
			if (!System.Text.RegularExpressions.Regex.IsMatch(IDNum, @"^\d{5,9}$"))
				return -1;

			if (IDNum == "000000000")
				return -1;
			// The number is too short - add leading 0000
			if (IDNum.Length < 9)
			{
				while (IDNum.Length < 9)
				{
					IDNum = '0' + IDNum;
				}
			}

			// CHECK THE ID NUMBER
			int mone = 0;
			int incNum;
			for (int i = 0; i < 9; i++)
			{
				incNum = Convert.ToInt32(IDNum[i].ToString());
				incNum *= (i % 2) + 1;
				if (incNum > 9)
					incNum -= 9;
				mone += incNum;
			}
			if (mone % 10 == 0)
				return 1;
			else
				return 0;
		}


		#endregion

		#region static members
		/// <summary>
		/// check db for duplicate
		/// </summary>
		/// <returns>true if duplicate exists</returns>
		public static bool CheckForDuplicate(Client client)
		{
			if (client == null) return false;
			using (var db = new ccEntities())
			{
				return db.Clients.Any(Client.IsDuplicate(client));

			}
		}

		public static bool CanAddEligibility(Client client)
		{
			var isAdmin = System.Web.HttpContext.Current.User.IsInRole(FixedRoles.Admin.ToString());
			return isAdmin || client.HomeCareEntitledPeriods.Any() && (client.ApprovalStatusId == (int)ApprovalStatusEnum.Approved || client.ApprovalStatusId == (int)ApprovalStatusEnum.ResearchinProgresswithProof);
		}

		public static Expression<Func<Client, bool>> IsDuplicate(Client client)
		{
			return f =>
					f.Id != client.Id
					&& f.LastName.ToLower() == client.LastName.ToLower()
					&& f.FirstName.ToLower() == client.FirstName.ToLower()
					&& f.City.ToLower() == client.City.ToLower()
					&& ((f.NationalId == null && client.NationalId == null) || f.NationalId.Trim().ToLower() == client.NationalId.Trim().ToLower())
					&& ((f.NationalIdTypeId == null && client.NationalIdTypeId == null) || f.NationalIdTypeId == client.NationalIdTypeId);
		}

		/// <summary>
		/// this one performs better than .contains()
		/// </summary>
		public static Expression<Func<Client, bool>> ApprovalStatusFilter
			(DateTime d1, DateTime d2, int serType)
		{



			//if status ApprovedHomecareOnly can reported only for ser type HomeCare
			//or if status updated after end of reporting period

			return (Client c) => c.ApprovalStatusId != (int)ApprovalStatusEnum.ApprovedHomecareOnly ? true :
								 serType == (int)CC.Data.Service.ServiceTypes.Homecare ? true :
								 c.ApprovalStatusUpdated > d2 ? true : c.ApprovalStatusUpdated >= d1 && c.ApprovalStatusUpdated <= d2 ? true : false;
			

		}

		public static Expression<Func<Client, bool>> ApprovalStatusFilter()
		{
			//If a client has any other approval status other than Research in progress with proof or Approved and has a join date of Jan 1st, 2018 and onward, 
			//then this client will not be populated into any new report in the system
			return (Client c) => c.ApprovalStatusId == (int)ApprovalStatusEnum.Approved ||
								c.ApprovalStatusId == (int)ApprovalStatusEnum.ResearchinProgresswithProof ||
								c.JoinDate < JoinDateToCompare;
			
		}

		public static Expression<Func<Client, bool>> IneligibleByApprovalStatus
		{
			get
			{
				//If a client has any other approval status other than Research in progress with proof or Approved and has a join date of Jan 1st, 2018 and onward, 
				//then this client will not be populated into any new report in the system
				return (Client c) => c.ApprovalStatusId != (int)ApprovalStatusEnum.Approved && c.ApprovalStatusId != (int)ApprovalStatusEnum.ResearchinProgresswithProof
										&& c.JoinDate >= JoinDateToCompare;
			}
		}

		public static Expression<Func<Client, bool>> IncomeVerificationRuiredExpression
		{
			get
			{
				return (Client c) => c.Agency.AgencyGroup.Country.IncomeVerificationRequired && (c.FundStatusId == null || c.FundStatus.IncomeVerificationRequired)
					&& !(c.Agency.AgencyGroup.SupportiveCommunities || c.Agency.AgencyGroup.DayCenter);
			}
		}

		#endregion

		#region Eligibility

		public static Expression<Func<Client, string>> FullNameFunc
		{
			get { return c => c.FirstName + " " + c.LastName; }
		}





		public static decimal GetAllowedHours(DateTime start, DateTime end, int clientId, IQueryable<FunctionalityScore> scores, decimal? gfHours, decimal? eHours)
		{
			decimal result = 0;
			var funcs = scores
				.Where(f => f.ClientId == clientId)
				.Where(f => f.StartDate <= end).OrderByDescending(f => f.StartDate)
				.Select(f => new { Start = f.StartDate, Quantity = f.FunctionalityLevel.HcHoursLimit });

			var localEnd = end;
			var localStart = start;
			foreach (var f in funcs)
			{

				localStart = new DateTime(Math.Max(f.Start.Date.Ticks, start.Date.Ticks));

				var days = (localEnd.Date - localStart.Date).TotalDays;

				var localQuantity = (decimal)(f.Quantity * days / 7);

				result += Math.Max(localQuantity, Math.Max(gfHours.HasValue ? gfHours.Value : 0, eHours.HasValue ? eHours.Value : 0));
				localEnd = f.Start;

				if (f.Start < start)
					break;
			}
			return result;
		}




		#endregion

		public List<DuplicateLinks> Duplicates
		{
			get;
			set;
		}
		public event PropertyChangingEventHandler PropertyChanging;
		public virtual void OnPropertyChangig(object sender, PropertyChangingEventArgs e)
		{
			if (PropertyChanging != null)
			{
				PropertyChanging(sender, e);
			}
		}

		public void OnPropertySaving(string propertyName, Services.IPermissionsBase permissions)
		{


			switch (propertyName)
			{
				case "DeceasedDate":
					if (!permissions.CanChangeDeceasedDate)
					{
						throw new PropertyChangeDeniedException();
					}
					break;
			}
			//if (propertyName.Equals(this.GetPropertyInfo(f => f.GfHours).Name))
			//{
			//	if (!permissions.CanChangeGfHours)
			//	{
			//		throw new PropertyChangeDeniedException();
			//	}
			//}
		}
		public override int GetHashCode()
		{
			return Id;
		}

		public class DuplicateLinks
		{
			public int Id { get; set; }
			public string Name { get; set; }
			public bool Allowed { get; set; }

		}

		public Client Clone()
		{
			return (Client)this.MemberwiseClone();
		}

		public static readonly string[] ccidColumnName = new string[] { "CC_ID", "CC ID", "CCID", "CLIENTID", "CLIENT ID" };
		public static DateTime MaxEligibleBirthDate { get { return new DateTime(1946, 2, 8); } }
		public static DateTime MinEligibleBirthDate { get { return new DateTime(1912, 1, 1); } }
        public static DateTime MinEligibleJoinDate
        {
            get
            {
                var min_datetime = DateTime.Now.AddYears(-1).AddDays(1);
                return new DateTime(min_datetime.Year, min_datetime.Month, min_datetime.Day);
            }
        }

        [Display(Name="Homecare Approval Status")]
		public string CurrentHomeCareApprovalStatus { get; set; }
	}

	public interface IIntIdRecord
	{
		int Id { get; set; }
	}
	public interface INotifyPropertySaving
	{

		void OnPropertySaving(string propertyName, Services.IPermissionsBase permissions);

	}

	public class asfd : CancelEventArgs
	{
		public ValidationContext ValidationContext { get; set; }
	}

	public class PropertyChangeDeniedException : Exception
	{
		public PropertyChangeDeniedException() : base() { }
	}





}
