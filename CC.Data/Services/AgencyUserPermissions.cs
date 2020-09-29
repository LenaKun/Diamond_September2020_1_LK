using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace CC.Data.Services
{
	//agency user
	class AgencyUserPermissions : PermissionsBase
	{
		public AgencyUserPermissions(User user) : base(user) { }
		public override Expression<Func<Agency, bool>> AgencyFilter
		{
			get
			{
				return a => a.Users.Any(u => u.Id == this.User.Id);
			}
		}
		public override Expression<Func<Client, bool>> ClientsFilter
		{
			get
			{
				return c => c.Agency.Users.Select(f => f.Id).Contains(this.User.Id);
			}
		}
		public override Expression<Func<Client, bool>> CfsClientsFilter
		{
			get
			{
				return c => c.Agency.Users.Select(f => f.Id).Contains(this.User.Id);
			}
		}
		public override Expression<Func<AgencyGroup, bool>> AgencyGroupsFilter
		{
			get
			{
				return c => c.Agencies.Any(a => a.Users.Any(u => u.Id == this.User.Id));
			}
		}
		public override Expression<Func<Region, bool>> RegionsFilter
		{
			get
			{
				return c => c.Countries.Any(f => f.AgencyGroups.Any(ag => ag.Agencies.Any(a => a.Id == this.User.AgencyId)));
			}
		}
		public override Expression<Func<AppBudget, bool>> AppBudgetsFilter
		{
			get
			{
				return a => a.App.AgencyGroup.Agencies.SelectMany(d => d.Users).Any(f => f.Id == this.User.Id);
			}
		}
		public override Expression<Func<MainReport, bool>> MainReportsFilter
		{
			get
			{
				return f => f.AppBudget.App.AgencyGroup.Agencies.Any(a => a.Id == this.User.AgencyId);
			}
		}
		public override Expression<Func<SubReport, bool>> SubReportsFilter
		{
			get
			{
				return sr => sr.AppBudgetService.AgencyId == this.User.AgencyId;
			}
		}
		public override Expression<Func<AppBudgetService, bool>> AppBudgetServicesFilter
		{ get { return s => s.AgencyId == this.User.AgencyId; } }
		public override Expression<Func<AppBudgetServiceAudit, bool>> AppBudgetServiceAuditsFilter
		{ get { return s => s.AgencyId == this.User.AgencyId; } }
		public override Expression<Func<App, bool>> AppsFilter
		{
			get
			{
				return a => a.AgencyGroup.Agencies.Any(f => f.Id == this.User.AgencyId);
			}
		}
		public override Expression<Func<Fund, bool>> FundsFilter
		{
			get
			{
				return f => f.Apps.Any(a => a.AgencyGroup.Agencies.Any(s => s.Id == this.User.AgencyId));
			}
		}
		public override Expression<Func<ClientReport, bool>> ClientReportsFilter
		{
			get
			{
				return cr => cr.SubReport.AppBudgetService.AgencyId == this.User.AgencyId;
			}
		}
		public override Expression<Func<EmergencyReport, bool>> EmergencyReportsFilter
		{
			get
			{
				return cr => cr.SubReport.AppBudgetService.AgencyId == this.User.AgencyId;
			}
		}


        public override Expression<Func<SupportiveCommunitiesReport, bool>> SupportiveCommunitiesReportsFilter
        {
            get
            {
                return cr => cr.SubReport.AppBudgetService.AgencyId == this.User.AgencyId;
            }
        }

        public override Expression<Func<DaysCentersReport, bool>> DaysCentersReportsFilter
        {
            get
            {
                return cr => cr.SubReport.AppBudgetService.AgencyId == this.User.AgencyId;
            }
        }

		public override Expression<Func<SoupKitchensReport, bool>> SoupKitchensReportsFilter
		{
			get
			{
				return cr => cr.SubReport.AppBudgetService.AgencyId == this.User.AgencyId;
			}
		}

		public override Expression<Func<MhmReport, bool>> MhmReportsFilter
		{
			get
			{
				return f => f.Client.AgencyId == this.User.AgencyId &&
					f.SubReport.AppBudgetService.AgencyId == this.User.AgencyId;
			}
		}
		public override Expression<Func<PersonnelReport, bool>> PersonnelReportsFilter
		{
			get
			{
				return f => f.AppBudgetService.AgencyId == this.User.AgencyId;
			}
		}

		public override bool CanCreateNewClient { get { return true; } }
		public override bool CanUpdateExistingClient { get { return true; } }
		public override bool CanCreateMainReport { get { return true; } }
		public override bool CanEditCeefFields { get { return true; } }
		
		public override Expression<Func<Daf, bool>> DafFilter
		{
			get
			{
				return f=>f.Client.AgencyId == this.User.AgencyId;
			}
		}
		public override bool CanDeleteDaf(Daf.Statuses item)
		{
			return item == Daf.Statuses.Open;
		}
		public override bool CanSeeDcc
		{
			get
			{
				return this.User != null && this.User.Agency != null && this.User.Agency.AgencyGroup != null && this.User.Agency.AgencyGroup.DayCenter;
			}
		}
		public override bool CanSeeSc
		{
			get
			{
				return this.User != null && this.User.Agency != null && this.User.Agency.AgencyGroup != null && this.User.Agency.AgencyGroup.SupportiveCommunities;
			}
		}
	}
}
