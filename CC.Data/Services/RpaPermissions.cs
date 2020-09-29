using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CC.Data.Services
{
    class RegionAssistantPermissions : PermissionsBase
    {
        public RegionAssistantPermissions(User user) : base(user) { }

		public override bool CanEditCeefFields { get { return true; } }
		public override bool CanSeeProgramField { get { return true; } }

		public override Expression<Func<Agency, bool>> AgencyFilter
		{
			get
			{
				return a => a.AgencyGroup.PoUsers.Any(f => f.Id == this.User.Id);
			}
		}
		public override Expression<Func<Client, bool>> ClientsFilter
		{
			get
			{
				return c => c.Agency.AgencyGroup.PoUsers.Any(f => f.Id == this.User.Id);
			}
		}
		public override Expression<Func<Client, bool>> CfsClientsFilter
		{
			get
			{
				return c => c.Agency.AgencyGroup.PoUsers.Any(f => f.Id == this.User.Id);
			}
		}
		public override Expression<Func<ClientReport, bool>> ClientReportsFilter
		{
			get
			{
				return cr => cr.SubReport.MainReport.AppBudget.App.AgencyGroup.PoUsers.Any(f => f.Id == this.User.Id);
			}
		}
		public override Expression<Func<AgencyGroup, bool>> AgencyGroupsFilter
		{
			get
			{
				return c => c.PoUsers.Any(f => f.Id == this.User.Id);
			}
		}
		public override Expression<Func<AppBudget, bool>> AppBudgetsFilter
		{
			get
			{
				return a => a.App.AgencyGroup.PoUsers.Any(f => f.Id == this.User.Id);
			}
		}
		public override Expression<Func<MainReport, bool>> MainReportsFilter
		{
			get
			{
				return a => a.AppBudget.App.AgencyGroup.PoUsers.Any(f => f.Id == this.User.Id);
			}
		}
		public override Expression<Func<SubReport, bool>> SubReportsFilter
		{
			get
			{
				return sr => sr.AppBudgetService.Agency.AgencyGroup.PoUsers.Any(f => f.Id == this.User.Id);
			}
		}
		public override Expression<Func<PersonnelReport, bool>> PersonnelReportsFilter
		{
			get
			{
				return s => s.AppBudgetService.Agency.AgencyGroup.PoUsers.Any(f => f.Id == this.User.Id);
			}
		}
		public override Expression<Func<AppBudgetService, bool>> AppBudgetServicesFilter
		{
			get
			{
				return s => s.Agency.AgencyGroup.PoUsers.Any(f => f.Id == this.User.Id);
			}
		}
		public override Expression<Func<AppBudgetServiceAudit, bool>> AppBudgetServiceAuditsFilter
		{
			get
			{
				return s => s.Agency.AgencyGroup.PoUsers.Any(f => f.Id == this.User.Id);
			}
		}
		public override Expression<Func<App, bool>> AppsFilter
		{
			get
			{
				return a => a.AgencyGroup.PoUsers.Any(f => f.Id == this.User.Id);
			}
		}
		public override Expression<Func<Fund, bool>> FundsFilter
		{
			get
			{
				return f => f.Apps.Any(a => a.AgencyGroup.PoUsers.Any(u=>u.Id==this.User.Id));
			}
		}
        public override Expression<Func<SupportiveCommunitiesReport, bool>> SupportiveCommunitiesReportsFilter
		{
			get
			{
				return a => a.SubReport.AppBudgetService.Agency.AgencyGroup.PoUsers.Any(f => f.Id == this.User.Id);
			}
		}

        public override Expression<Func<DaysCentersReport, bool>> DaysCentersReportsFilter
        {
            get
            {
                return a => a.SubReport.AppBudgetService.Agency.AgencyGroup.PoUsers.Any(f => f.Id == this.User.Id);
            }
        }

		public override Expression<Func<SoupKitchensReport, bool>> SoupKitchensReportsFilter
		{
			get
			{
				return a => a.SubReport.AppBudgetService.Agency.AgencyGroup.PoUsers.Any(f => f.Id == this.User.Id);
			}
		}

        public override Expression<Func<EmergencyReport, bool>> EmergencyReportsFilter
        {
            get
            {
                return a => a.SubReport.AppBudgetService.Agency.AgencyGroup.PoUsers.Any(f => f.Id == this.User.Id);
            }
        }
		public override Expression<Func<MhmReport, bool>> MhmReportsFilter
		{
			get
			{
				return a => a.Client.Agency.AgencyGroup.PoUsers.Any(f => f.Id == this.User.Id);
			}
		}
		public override bool ShowDuplicatesNavigation
		{
			get
			{
				return true;
			}
		}
		public override Expression<Func<User, bool>> UsersFilter
		{
			get
			{
				return u => u.UserName == this.User.UserName
					|| ((u.RoleId == (int)FixedRoles.AgencyUser || u.RoleId == (int)FixedRoles.AgencyUserAndReviewer) && u.Agency.AgencyGroup.PoUsers.Any(f => f.Id == this.User.Id))
					|| ((u.RoleId == (int)FixedRoles.Ser || u.RoleId == (int)FixedRoles.SerAndReviewer) && u.AgencyGroup.PoUsers.Any(f => f.Id == this.User.Id));
			}
		}
		public override FixedRoles[] AllowedRoles
		{
			get
			{
				return new FixedRoles[] { FixedRoles.AgencyUser, FixedRoles.Ser, FixedRoles.AgencyUserAndReviewer, FixedRoles.SerAndReviewer };
			}
		}
		public override bool CanAccessInternalRemarks
		{
			get
			{
				return true;
			}
		}
		public override bool CanSeeDcc
		{
			get
			{
				return true;
			}
		}
		public override bool CanSeeSc
		{
			get
			{
				return true;
			}
		}
    }
}
