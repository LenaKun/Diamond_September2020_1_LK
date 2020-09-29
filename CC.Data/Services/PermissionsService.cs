using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace CC.Data.Services
{
	public class PermissionsFactory
	{

		public static IPermissionsBase GetPermissionsFor(User user)
		{
			IPermissionsBase result;
			switch ((FixedRoles)user.RoleId)
			{
				case FixedRoles.Admin:
					result = new AdminPermissions(user);
					break;
                case FixedRoles.Maintenance:
                    result = new MaintenancePermissions(user);
                    break;
                case FixedRoles.AgencyUser:
					result = new AgencyUserPermissions(user);
					break;
				case FixedRoles.GlobalOfficer:
					result = new GlobalOfficerPermissions(user);
					break;
                case FixedRoles.GlobalReadOnly:
                    result = new GlobalReadOnlyPermissions(user);
                    break;
                case FixedRoles.AuditorReadOnly:
                    result = new AuditorReadOnlyPermissions(user);
                    break;
                case FixedRoles.RegionReadOnly:
                    result = new RegionReadOnlyPermissions(user);
                    break;
				case FixedRoles.RegionOfficer:
					result = new RegionOfficerPremissions(user);
					break;
                case FixedRoles.RegionAssistant:
                    result = new RegionAssistantPermissions(user);
                    break;
				case FixedRoles.Ser:
					result = new SerPermissions(user);
					break;
                case FixedRoles.BMF:
                    result = new BmfPermissions(user);
                    break;
				case FixedRoles.DafEvaluator:
					result = new DafEvaluatorPermissions(user);
					break;
				case FixedRoles.DafReviewer:
					result = new DafReviewerPermissions(user);
					break;
				case FixedRoles.CfsAdmin:
					result = new CfsAdminPermissions(user);
					break;
				case FixedRoles.AgencyUserAndReviewer:
					result = new AgencyUserAndReviewerPermissions(user);
					break;
				case FixedRoles.SerAndReviewer:
					result = new SerAndReviewerPermissions(user);
					break;
				default:
					result = new PermissionsBase(user);
					break;
			}
			return result;
		}

		public static IPermissionsBase GetPermissionsFor(string username)
		{

			using (var db = new ccEntities())
			{
				var user = db.Users.Single(f => f.UserName == username);
				return GetPermissionsFor(user);
			}

		}

	}
}
