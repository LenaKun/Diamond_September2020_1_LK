using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Text;

namespace CC.Data.Services
{
	class GlobalOfficerPermissions : RegionOfficerPremissions
	{
		public override bool CanEditCeefFields { get { return true; } }
		public GlobalOfficerPermissions(User user) : base(user) { }
		public override Expression<Func<Agency, bool>> AgencyFilter { get { return c => true; } }
		public override Expression<Func<Client, bool>> ClientsFilter { get { return c => true; } }
		public override Expression<Func<Client, bool>> CfsClientsFilter { get { return c => true; } }
		public override Expression<Func<ClientReport, bool>> ClientReportsFilter
		{
			get
			{
				return c => true;
			}
		}
		public override Expression<Func<AgencyGroup, bool>> AgencyGroupsFilter
		{
			get
			{
				return c => true;
			}
		}
		public override Expression<Func<AppBudget, bool>> AppBudgetsFilter
		{
			get
			{
				return a => true;
			}
		}
		public override Expression<Func<MainReport, bool>> MainReportsFilter
		{
			get
			{
				return f => true;
			}
		}
		public override Expression<Func<SubReport, bool>> SubReportsFilter
		{
			get
			{
				return sr => true;
			}
		}
		public override Expression<Func<EmergencyReport, bool>> EmergencyReportsFilter
		{
			get
			{
				return b => true;
			}
		}

        public override Expression<Func<SupportiveCommunitiesReport, bool>> SupportiveCommunitiesReportsFilter
        {
            get
            {
                return b => true;
            }
        }

        public override Expression<Func<DaysCentersReport, bool>> DaysCentersReportsFilter
        {
            get
            {
                return b => true;
            }
        }

		public override Expression<Func<SoupKitchensReport, bool>> SoupKitchensReportsFilter
		{
			get
			{
				return b => true;
			}
		}

		public override Expression<Func<AppBudgetService, bool>> AppBudgetServicesFilter
		{ get { return s => true; } }

		public override Expression<Func<PersonnelReport, bool>> PersonnelReportsFilter
		{ get { return s => true; } }
		public override Expression<Func<AppBudgetServiceAudit, bool>> AppBudgetServiceAuditsFilter
		{ get { return s => true; } }
		public override Expression<Func<App, bool>> AppsFilter
		{
			get
			{
				return a => true;
			}
		}
		public override Expression<Func<Fund, bool>> FundsFilter
		{
			get
			{
				return f => true;
			}
		}
		public override Expression<Func<MhmReport, bool>> MhmReportsFilter
		{
			get
			{
				return f => true;
			}
		}
		public override Expression<Func<User, bool>> UsersFilter
		{
			get
			{
				return f => f.RoleId!=(int)FixedRoles.Admin;
			}
		}
		public override FixedRoles[] AllowedRoles
		{
			get
			{
				return new FixedRoles[] { FixedRoles.AgencyUser, FixedRoles.Ser, FixedRoles.AgencyUserAndReviewer, FixedRoles.SerAndReviewer, FixedRoles.RegionOfficer, FixedRoles.RegionAssistant };
			}
		}
		public override bool CanAccessInternalRemarks
		{
			get
			{
				return true;
			}
		}
		public override bool CanUpdateExistingClient
		{
			get
			{
				return true;
			}
		}
	}
}
