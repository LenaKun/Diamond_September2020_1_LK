using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Mvc;
using System.Linq;
using System.Web.Security;
using CC.Data;

namespace CC.Web.Models
{

	public class ChangePasswordModel
	{
		[Required]
		[DataType(DataType.Password)]
		[Display(Name = "Current password")]
		public string OldPassword { get; set; }

		[Required]
        [RegularExpression(@"^(?=.*[0-9])(?=.*[\W_])[0-9a-zA-Z\W_0-9]{6,}$", ErrorMessage = "The password should be at least 6 characters long, have at least one digit, one uppercase letter and one special character")]
		[DataType(DataType.Password)]
		[Display(Name = "New password")]
		public string NewPassword { get; set; }

		[DataType(DataType.Password)]
		[Display(Name = "Confirm new password")]
		[System.Web.Mvc.Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
		public string ConfirmPassword { get; set; }

        public string UserName { get; set; }
	}

	public class LogOnModel
	{
		[Required]
		[Display(Name = "User name")]
		public string UserName { get; set; }

		[Required]
		[DataType(DataType.Password)]
		[Display(Name = "Password")]
		public string Password { get; set; }

		[Display(Name = "Remember me?")]
		public bool RememberMe { get; set; }
	}

	public class RegisterModelBase : IValidatableObject
	{
		public RegisterModelBase()
		{
			this.DecimalDisplayDigits = 2;
		}

		[Required]
		[Display(Name = "User name")]
		[UIHint("MediumString")]
		public string UserName { get; set; }

		[Display(Name = "Last Name")]
		public string LastName { get; set; }

		[Display(Name = "First Name")]
		public string FirstName { get; set; }

		[Required]
		[DataType(DataType.EmailAddress)]
		[Display(Name = "Email address")]
		[UIHint("MediumString")]
		public string Email { get; set; }

        [Display(Name="Add to BCC?")]
        public bool AddToBcc { get; set; }

		[Display(Name = "Number of digits to display after decimal point.")]
		public int DecimalDisplayDigits { get; set; }

		[UIHint("LongString")]
		public string Comments { get; set; }

        [Display(Name = "Temporary Password ?")]
        public bool TemporaryPassword { get; set; }

        [Display(Name = "Disable User ID")]
        public bool Disabled { get; set; }

		[Required]
		public int? RoleId { get; set; }

		public bool IsRoleEditable
		{
			get
			{
				if (this.Permissions == null)
				{
					return false;
				}
				else
				{
					switch ((FixedRoles)this.Permissions.User.RoleId)
					{
						case FixedRoles.Admin:
							switch ((FixedRoles)this.RoleId)
							{
								case FixedRoles.AgencyUser:
								case FixedRoles.Ser:
								case FixedRoles.AgencyUserAndReviewer:
								case FixedRoles.SerAndReviewer:
								case FixedRoles.RegionOfficer:
								case FixedRoles.RegionAssistant:
								case FixedRoles.GlobalOfficer:
								case FixedRoles.Admin:
                                case FixedRoles.BMF:
                                case FixedRoles.GlobalReadOnly:
                                case FixedRoles.RegionReadOnly:
                                case FixedRoles.AuditorReadOnly:
								default: return true;
							}
						case FixedRoles.GlobalOfficer:
							switch ((FixedRoles)this.RoleId)
							{
								case FixedRoles.AgencyUser:
								case FixedRoles.Ser:
								case FixedRoles.AgencyUserAndReviewer:
								case FixedRoles.SerAndReviewer:
                                case FixedRoles.RegionReadOnly:
								case FixedRoles.RegionOfficer: return true;
								case FixedRoles.RegionAssistant: return true;
								case FixedRoles.GlobalOfficer:
								case FixedRoles.Admin:
                                case FixedRoles.GlobalReadOnly:
                                case FixedRoles.AuditorReadOnly:
                                case FixedRoles.BMF:                                
								default: return false;
							}

						case FixedRoles.RegionOfficer:
							switch ((FixedRoles)this.RoleId)
							{
								case FixedRoles.AgencyUser:
								case FixedRoles.Ser:
								case FixedRoles.AgencyUserAndReviewer:
								case FixedRoles.SerAndReviewer:
									return true;
								case FixedRoles.RegionOfficer:
								case FixedRoles.RegionAssistant:
								case FixedRoles.GlobalOfficer:
                                case FixedRoles.GlobalReadOnly:
                                case FixedRoles.AuditorReadOnly:
                                case FixedRoles.BMF:
								case FixedRoles.Admin:
                                case FixedRoles.RegionReadOnly:
								default: return false;
							}

						case FixedRoles.RegionAssistant:
							switch ((FixedRoles)this.RoleId)
							{
								case FixedRoles.AgencyUser:
								case FixedRoles.Ser:
								case FixedRoles.AgencyUserAndReviewer:
								case FixedRoles.SerAndReviewer:
									return true;
								case FixedRoles.RegionOfficer:
								case FixedRoles.RegionAssistant:
								case FixedRoles.GlobalOfficer:
                                case FixedRoles.GlobalReadOnly:
                                case FixedRoles.AuditorReadOnly:
                                case FixedRoles.BMF:
								case FixedRoles.Admin:
                                case FixedRoles.RegionReadOnly:
								default: return false;
							}

						case FixedRoles.Ser:
						case FixedRoles.SerAndReviewer:
							switch ((FixedRoles)this.RoleId)
							{
								case FixedRoles.AgencyUser:
								case FixedRoles.AgencyUserAndReviewer: return true;
								case FixedRoles.Ser:
								case FixedRoles.SerAndReviewer:
								case FixedRoles.RegionOfficer:
								case FixedRoles.RegionAssistant:
								case FixedRoles.GlobalOfficer:
                                case FixedRoles.GlobalReadOnly:
                                case FixedRoles.AuditorReadOnly:
                                case FixedRoles.BMF:
								case FixedRoles.Admin:
                                case FixedRoles.RegionReadOnly:
								default: return false;
							}

						case FixedRoles.AgencyUser:
						case FixedRoles.AgencyUserAndReviewer:
							switch ((FixedRoles)this.RoleId)
							{
								case FixedRoles.AgencyUser:
								case FixedRoles.Ser:
								case FixedRoles.AgencyUserAndReviewer:
								case FixedRoles.SerAndReviewer:
								case FixedRoles.RegionOfficer:
								case FixedRoles.RegionAssistant:
								case FixedRoles.GlobalOfficer:
                                case FixedRoles.GlobalReadOnly:
                                case FixedRoles.AuditorReadOnly:
                                case FixedRoles.BMF:
								case FixedRoles.Admin:
                                case FixedRoles.RegionReadOnly:
								default: return false;
							}

						default: return false;
					}
				}
			}
		}

		public FixedRoles? Role
		{
			get { if (RoleId.HasValue) { return (FixedRoles)RoleId.Value; } else { return null; } }
			set { if (value == null) { RoleId = null; } else { RoleId = (int)value.Value; } }
		}

		public string RoleName
		{
			get
			{
				if (Role == null) return null;
				else return Role.Value.DisplayName();
			}
		}

		public int? AgencyId { get; set; }

		public int? AgencyGroupId { get; set; }

        public int? RegionId { get; set; }

		public IEnumerable<int> AgencyGroupIds { get; set; }

		public string AgencyGroupIdsString
		{
			get
			{
				if (this.AgencyGroupIds == null) return null;
				else return string.Join(",", this.AgencyGroupIds.Select(f => f.ToString()));
			}
			set
			{
				this.AgencyGroupIds = value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
			}
		}

		public IEnumerable<string> RoleEndName { get; set; }

		/// <summary>
		/// load relation ends
		/// </summary>
		/// <param name="db"></param>
		/// <param name="permissions"></param>
		internal void LoadData(ccEntities db, CC.Data.Services.IPermissionsBase permissions)
		{
			AgenciesSelectList = new SelectList(db.Agencies.Where(permissions.AgencyFilter).Select(f => new { Id = f.Id, Name = f.Name }).OrderBy(f => f.Name), "Id", "Name", this.AgencyId);
			var agencygroups = db.AgencyGroups.Where(permissions.AgencyGroupsFilter).Select(f => new { Id = f.Id, Name = f.DisplayName }).OrderBy(f=>f.Name);
			AgencyGroupsSelectList = new SelectList(agencygroups, "Id", "Name", this.AgencyGroupId);
			POAgencyGroups = new MultiSelectList(agencygroups, "Id", "Name", this.AgencyGroupIds);
            PAAgencyGroups = new MultiSelectList(agencygroups, "Id", "Name", this.AgencyGroupIds);
            RegionsSelectList = new SelectList(db.Regions.Where(permissions.RegionsFilter).Select(f => new { Id = f.Id, Name = f.Name }).OrderBy(f => f.Name), "Id", "Name", this.RegionId);
			RolesSelectList = new SelectList(permissions.AllowedRoles.Select(f => new { Key = (int)f, Value = f.DisplayName() }), "Key", "Value", this.RoleId);
			if(this.RoleId == (int)FixedRoles.Ser || this.RoleId == (int)FixedRoles.SerAndReviewer)
			{
				EditRolesSelectList = new SelectList(permissions.AllowedRoles.Where(f => f == FixedRoles.Ser || f == FixedRoles.SerAndReviewer).Select(f => new { Key = (int)f, Value = f.DisplayName() }), "Key", "Value", this.RoleId);
			}
			else if (this.RoleId == (int)FixedRoles.AgencyUser || this.RoleId == (int)FixedRoles.AgencyUserAndReviewer)
			{
				EditRolesSelectList = new SelectList(permissions.AllowedRoles.Where(f => f == FixedRoles.AgencyUser || f == FixedRoles.AgencyUserAndReviewer).Select(f => new { Key = (int)f, Value = f.DisplayName() }), "Key", "Value", this.RoleId);
			}
		}

		public SelectList AgenciesSelectList { get; private set; }

		public SelectList AgencyGroupsSelectList { get; private set; }

		public SelectList RegionsSelectList { get; private set; }

		public SelectList RolesSelectList { get; private set; }

		public SelectList EditRolesSelectList { get; private set; }

		public CC.Data.Services.IPermissionsBase Permissions { get; set; }

		public MultiSelectList POAgencyGroups { get; set; }

        public MultiSelectList PAAgencyGroups { get; set; }


		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (this.Role != null)
			{
				switch (this.Role)
				{
					case FixedRoles.AgencyUser:
					case FixedRoles.AgencyUserAndReviewer:
						if (this.AgencyId == null)
						{
							yield return new ValidationResult("Please select an agency.");
						}
						break;
					case FixedRoles.Ser:
					case FixedRoles.SerAndReviewer:
						if (this.AgencyGroupId == null)
						{
							yield return new ValidationResult("Please select a SER.");
						}
						break;
					case FixedRoles.RegionOfficer:
						if (this.AgencyGroupIds == null || !this.AgencyGroupIds.Any())
						{
							yield return new ValidationResult("Please select at least one SER.");
						}
                        break;
                    case FixedRoles.RegionAssistant:
                        if (this.AgencyGroupIds == null || !this.AgencyGroupIds.Any())
                        {
                            yield return new ValidationResult("Please select at least one SER.");
                        }
						break;
                    case FixedRoles.RegionReadOnly:
                        if(this.RegionId == null)
                        {
                            yield return new ValidationResult("Please select a Region.");

                        }
                        break;
				}
				if (this.Permissions != null)
				{
					if (!this.Permissions.AllowedRoles.Contains(this.Role.Value) && !this.UserName.Equals(this.Permissions.User.UserName, StringComparison.CurrentCultureIgnoreCase))
					{
						yield return new ValidationResult("Saving this user role is not allowed.");
					}
				}
			}
		}

		/// <summary>
		/// Update user's email and user's role
		/// </summary>
		/// <param name="user"></param>
		public void ApplyValuesTo(User user, ccEntities db)
		{
			user.UserName = this.UserName;
			user.Email = this.Email;
            user.AddToBcc = this.AddToBcc;
			user.FirstName = this.FirstName;
			user.LastName = this.LastName;
			user.DecimalDisplayDigits = this.DecimalDisplayDigits;
            user.TemporaryPassword = this.TemporaryPassword;
			if (!this.Disabled && this.Disabled != user.Disabled)
			{
				user.MembershipUser.FailedPasswordAttemptCount = 0;
			}
            user.Disabled = this.Disabled;			


			if (this.Role != null && this.Permissions != null && this.Permissions.AllowedRoles.Contains(this.Role.Value))
			{
				user.RoleId = this.RoleId.Value;
				user.AgencyId = null;
				user.AgencyGroupId = null;
				user.AgencyGroups.Clear();
				switch (this.Role.Value)
				{
					case FixedRoles.AgencyUser:
					case FixedRoles.DafReviewer:
					case FixedRoles.DafEvaluator:
					case FixedRoles.AgencyUserAndReviewer:
						user.AgencyId = this.AgencyId;
						break;
					case FixedRoles.Ser:
					case FixedRoles.SerAndReviewer:
						user.AgencyGroupId = this.AgencyGroupId;
						break;
					case FixedRoles.RegionOfficer:
						foreach (var item in db.AgencyGroups.Where(f => this.AgencyGroupIds.Contains(f.Id)))
						{
							user.AgencyGroups.Add(item);
						}
						break;
                    case FixedRoles.RegionAssistant:
                        foreach (var item in db.AgencyGroups.Where(f => this.AgencyGroupIds.Contains(f.Id)))
                        {
                            user.AgencyGroups.Add(item);
                        }
                        break;
                    case FixedRoles.RegionReadOnly:
                        user.RegionId = this.RegionId;
                        break;
					case FixedRoles.GlobalOfficer:
						break;
					case FixedRoles.Admin:
						break;
                    case FixedRoles.BMF:
                        break;
                    case FixedRoles.GlobalReadOnly:
                        break;
                    case FixedRoles.AuditorReadOnly:
                        break;
				}
			}
		}




	}

	public class RegisterModel : RegisterModelBase
	{
		[Required]
        [RegularExpression(@"^(?=.*[0-9])(?=.*[\W_])[0-9a-zA-Z\W_0-9]{6,}$", ErrorMessage = "The password should be at least 6 characters long, have at least one digit, one uppercase letter and one special character")]
		[DataType(DataType.Password)]
		[Display(Name = "Password")]
		public virtual string Password { get; set; }

		[Required]
		[DataType(DataType.Password)]
		[Display(Name = "Confirm password")]
		[System.Web.Mvc.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
		public virtual string ConfirmPassword { get; set; }

	}

	public class RegEditModel : RegisterModelBase
	{
		public int Id { get; set; }

        [RegularExpression(@"^(?=.*[0-9])(?=.*[\W_])[0-9a-zA-Z\W_0-9]{6,}$", ErrorMessage = "The password should be at least 6 characters long, have at least one digit, one uppercase letter and one special character")]
		[DataType(DataType.Password)]
		[Display(Name = "Password")]
		[UIHint("ShortString")]
		public string Password { get; set; }

		[DataType(DataType.Password)]
		[Display(Name = "Confirm password")]
		[UIHint("ShortString")]
		[System.Web.Mvc.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
		public string ConfirmPassword { get; set; }

		public IEnumerable<string> AgencyGroupNames { get; set; }

		public string[] AgencyGrupName { get; set; }

		public string AgencyName { get; set; }
	}

}
