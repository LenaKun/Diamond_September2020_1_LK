using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace CC.Data.Services
{
    class RegionReadOnlyPermissions : GlobalOfficerPermissions
    {
        public RegionReadOnlyPermissions(User user) : base(user) { }

        public override Expression<Func<Agency, bool>> AgencyFilter
        {
            get
            {
                return a => a.AgencyGroup.Country.RegionId == this.User.RegionId;
            }
        }
        public override Expression<Func<Client, bool>> ClientsFilter
        {
            get
            {
                return c => c.Agency.AgencyGroup.Country.RegionId == this.User.RegionId;
            }
        }
		public override Expression<Func<Client, bool>> CfsClientsFilter
		{
			get
			{
				return c => c.Agency.AgencyGroup.Country.RegionId == this.User.RegionId;
			}
		}
        public override Expression<Func<AgencyGroup, bool>> AgencyGroupsFilter
        {
            get
            {
                return c => c.Country.RegionId == this.User.RegionId;
            }
        }
        public override Expression<Func<AppBudget, bool>> AppBudgetsFilter
        {
            get
            {
                return a => a.App.AgencyGroup.Country.RegionId == this.User.RegionId;
            }
        }
        public override Expression<Func<MainReport, bool>> MainReportsFilter
        {
            get
            {
                return f => f.AppBudget.App.AgencyGroup.Country.RegionId == this.User.RegionId;
            }
        }
        public override Expression<Func<SubReport, bool>> SubReportsFilter
        {
            get
            {
                return sr => sr.AppBudgetService.Agency.AgencyGroup.Country.RegionId == this.User.RegionId;
            }
        }
        public override Expression<Func<AppBudgetService, bool>> AppBudgetServicesFilter
        { get { return s => s.Agency.AgencyGroup.Country.RegionId == this.User.RegionId; } }
        public override Expression<Func<PersonnelReport, bool>> PersonnelReportsFilter
        { get { return s => s.AppBudgetService.Agency.AgencyGroup.Country.RegionId == this.User.RegionId; } }
        public override Expression<Func<AppBudgetServiceAudit, bool>> AppBudgetServiceAuditsFilter
        { get { return s => s.Agency.AgencyGroup.Country.RegionId == this.User.RegionId; } }
        public override Expression<Func<App, bool>> AppsFilter
        {
            get
            {
                return a => a.AgencyGroup.Country.RegionId == this.User.RegionId;
            }
        }
        public override Expression<Func<Fund, bool>> FundsFilter
        {
            get
            {
                return f => f.Apps.Any(a => a.AgencyGroup.Country.RegionId == this.User.RegionId);
            }
        }
        public override Expression<Func<ClientReport, bool>> ClientReportsFilter
        {
            get
            {
                return cr => cr.Client.Agency.AgencyGroup.Country.RegionId == this.User.RegionId;
            }
        }
        public override Expression<Func<MhmReport, bool>> MhmReportsFilter
        {
            get
            {
                return f => f.Client.Agency.AgencyGroup.Country.RegionId == this.User.RegionId &&
                    f.SubReport.AppBudgetService.Agency.AgencyGroup.Country.RegionId == this.User.RegionId;
            }
        }
        public override bool CanEditCeefFields
        {
            get
            {
                return false;
            }
        }
        public override Expression<Func<SupportiveCommunitiesReport, bool>> SupportiveCommunitiesReportsFilter
        {
            get
            {
                return f => f.SubReport.AppBudgetService.Agency.AgencyGroup.Country.RegionId == this.User.RegionId;
            }
        }


        public override Expression<Func<DaysCentersReport, bool>> DaysCentersReportsFilter
        {
            get
            {
                return f => f.SubReport.AppBudgetService.Agency.AgencyGroup.Country.RegionId == this.User.RegionId;
            }
        }

		public override Expression<Func<SoupKitchensReport, bool>> SoupKitchensReportsFilter
		{
			get
			{
				return f => f.SubReport.AppBudgetService.Agency.AgencyGroup.Country.RegionId == this.User.RegionId;
			}
		}

        public override Expression<Func<EmergencyReport, bool>> EmergencyReportsFilter
        {
            get
            {
                return f => f.SubReport.AppBudgetService.Agency.AgencyGroup.Country.RegionId == this.User.RegionId;
            }
        }

        public override Expression<Func<User, bool>> UsersFilter
        {
            get
            {
                return u => u.Agency.AgencyGroup.Country.RegionId == this.User.RegionId;
            }
        }
        public override Expression<Func<Region, bool>> RegionsFilter
        {
            get { return f => f.Id == this.User.RegionId; }
        }
		public override bool CanSeeProgramField
		{
			get
			{
				return false;
			}
		}
    }
}
