using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace CC.Data.Services
{
	//ser
	class SerPermissions : AgencyUserPermissions
	{
		public SerPermissions(User user) : base(user) { }
		public override Expression<Func<Agency, bool>> AgencyFilter
		{
			get
			{
				return a => a.GroupId == this.User.AgencyGroupId;
			}
		}
		public override Expression<Func<Client, bool>> ClientsFilter
		{
			get
			{
				return c => c.Agency.AgencyGroup.Users.Any(f => f.Id == this.User.Id);
			}
		}
		public override Expression<Func<Client, bool>> CfsClientsFilter
		{
			get
			{
				return c => c.Agency.AgencyGroup.Users.Any(f => f.Id == this.User.Id);
			}
		}
		public override Expression<Func<AgencyGroup, bool>> AgencyGroupsFilter
		{
			get
			{
				return c => c.Users.Any(u => u.Id == this.User.Id);
			}
		}
		public override Expression<Func<Region, bool>> RegionsFilter
		{
			get
			{
				return c => c.Countries.Any(f => f.AgencyGroups.Any(ag => ag.Id == this.User.AgencyGroupId));
			}
		}
		public override Expression<Func<AppBudget, bool>> AppBudgetsFilter
		{
			get
			{
				return a => a.App.AgencyGroup.Users.Any(f => f.Id == this.User.Id);
			}
		}
		public override Expression<Func<MainReport, bool>> MainReportsFilter
		{
			get
			{
				return f => f.AppBudget.App.AgencyGroupId == this.User.AgencyGroupId;
			}
		}
		public override Expression<Func<SubReport, bool>> SubReportsFilter
		{
			get
			{
				return sr => sr.AppBudgetService.Agency.GroupId == this.User.AgencyGroupId;
			}
		}
		public override Expression<Func<AppBudgetService, bool>> AppBudgetServicesFilter
		{ get { return s => s.Agency.GroupId == this.User.AgencyGroupId; } }
		public override Expression<Func<PersonnelReport, bool>> PersonnelReportsFilter
		{ get { return s => s.AppBudgetService.Agency.GroupId == this.User.AgencyGroupId; } }
		public override Expression<Func<AppBudgetServiceAudit, bool>> AppBudgetServiceAuditsFilter
		{ get { return s => s.Agency.GroupId == this.User.AgencyGroupId; } }
		public override Expression<Func<App, bool>> AppsFilter
		{
			get
			{
				return a => a.AgencyGroupId == this.User.AgencyGroupId;
			}
		}
		public override Expression<Func<Fund, bool>> FundsFilter
		{
			get
			{
				return f => f.Apps.Any(a => a.AgencyGroup.Id == this.User.AgencyGroupId);
			}
		}
		public override Expression<Func<ClientReport, bool>> ClientReportsFilter
		{
			get
			{
				return cr => cr.Client.Agency.GroupId == this.User.AgencyGroupId;
			}
		}
		public override Expression<Func<MhmReport, bool>> MhmReportsFilter
		{
			get
			{
				return f => f.Client.Agency.GroupId == this.User.AgencyGroupId &&
					f.SubReport.AppBudgetService.Agency.GroupId == this.User.AgencyGroupId;
			}
		}
		public override bool CanEditCeefFields
		{
			get
			{
				return true;
			}
		}
		public override bool CanCreateMainReport { get { return true; } }


        public override Expression<Func<SupportiveCommunitiesReport, bool>> SupportiveCommunitiesReportsFilter
		{
			get
			{
				return f => f.SubReport.AppBudgetService.Agency.GroupId == this.User.AgencyGroupId;
			}
		}


        public override Expression<Func<DaysCentersReport, bool>> DaysCentersReportsFilter
        {
            get
            {
                return f => f.SubReport.AppBudgetService.Agency.GroupId == this.User.AgencyGroupId;
            }
        }

		public override Expression<Func<SoupKitchensReport, bool>> SoupKitchensReportsFilter
		{
			get
			{
				return f => f.SubReport.AppBudgetService.Agency.GroupId == this.User.AgencyGroupId;
			}
		}

        public override Expression<Func<EmergencyReport, bool>> EmergencyReportsFilter
        {
            get
            {
                return f => f.SubReport.AppBudgetService.Agency.GroupId == this.User.AgencyGroupId;
            }
        }
		public override Expression<Func<User, bool>> UsersFilter
		{
			get
			{
				return u => (u.RoleId == (int)FixedRoles.AgencyUser
						|| u.RoleId == (int)FixedRoles.DafReviewer
						|| u.RoleId == (int)FixedRoles.AgencyUserAndReviewer
						|| u.RoleId == (int)FixedRoles.DafEvaluator
					) && u.Agency.GroupId == this.User.AgencyGroupId;
			}
		}
		public override FixedRoles[] AllowedRoles
		{
			get
			{
				return new FixedRoles[] { 
					FixedRoles.AgencyUser,
					FixedRoles.DafEvaluator,
					FixedRoles.DafReviewer,
					FixedRoles.AgencyUserAndReviewer
				};
			}
		}
		public override bool CanCreateDaf
		{
			get
			{
				return true;
			}
		}
		public override Expression<Func<Daf, bool>> DafFilter
		{
			get
			{
				return f => f.Client.Agency.GroupId == this.User.AgencyGroupId;
			}
		}
		public override bool CanSeeDcc
		{
			get
			{
				return this.User != null && this.User.AgencyGroup != null && this.User.AgencyGroup.DayCenter;
			}
		}
		public override bool CanSeeSc
		{
			get
			{
				return this.User != null && this.User.AgencyGroup != null && this.User.AgencyGroup.SupportiveCommunities;
			}
		}
	}

}
