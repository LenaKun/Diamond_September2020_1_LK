using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CC.Data
{
	[MetadataType(typeof(MetaData.UserMetadata))]
	public partial class User : IValidatableObject
	{

		public User()
		{
			this.UniqueId = Guid.NewGuid();
		}
		public static User CreateUser(string username, string password)
		{

			var user = new User()
			{

				UserName = username,
				Email = password,
			};

			user.MembershipUser = new MembershipUser()
			{
				LoweredEmail = user.Email.ToLower(),
				LoweredUserName = user.UserName.ToLower(),
				CreateDate = DateTime.UtcNow,
				FailedPasswordAnswerAttemptCount = 0,
				FailedPasswordAnswerAttemptWindowStart = null,
				FailedPasswordAttemptCount = 0,
				FailedPasswordAttemptWindowStart = null,
				IsApproved = true,
				IsLockedOut = false,
				LastLockoutDate = null,
				LastLoginDate = null,
                LastPasswordChangedDate = DateTime.UtcNow,

			};

			user.MembershipUser.SetPassword(password);

			return user;
		}

		public void AddMemUser()
		{
			this.MembershipUser = new MembershipUser()
			{
				LoweredEmail = this.Email.ToLower(),
				LoweredUserName = this.UserName.ToLower(),
				CreateDate = DateTime.UtcNow,
				FailedPasswordAnswerAttemptCount = 0,
				FailedPasswordAnswerAttemptWindowStart = null,
				FailedPasswordAttemptCount = 0,
				FailedPasswordAttemptWindowStart = null,
				IsApproved = true,
				IsLockedOut = false,
				LastLockoutDate = null,
				LastLoginDate = null,
				LastPasswordChangedDate = null,

			};
		}

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			using (var db = new ccEntities())
			{
				if (db.MembershipUsers.Any(f => f.LoweredUserName == this.UserName.ToLower() 
					&& f.Id != this.Id))
				{
					yield return new ValidationResult("Duplicate username.");
				}
			}

			if (!Enum.IsDefined(typeof(FixedRoles), this.RoleId))
			{
				yield return new ValidationResult("Invalid Role");
			}
			else
			{
				switch ((FixedRoles)this.RoleId)
				{
					case FixedRoles.AgencyUser:
					case FixedRoles.AgencyUserAndReviewer:
						if (this.AgencyId == null)
						{
							yield return new ValidationResult("Agency is a required field.");
						}
						break;
					case FixedRoles.RegionOfficer:
						if (!this.AgencyGroups.Any())
						{
							yield return new ValidationResult("Region is a required field.");
						}
						break;
					case FixedRoles.Ser:
					case FixedRoles.SerAndReviewer:
						if (this.AgencyGroupId == null)
						{
							yield return new ValidationResult("Ser is a required field.");
						}
						break;
				}
			}
		}
	}



}
