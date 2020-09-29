using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace CC.Data.Services
{
	class BmfPermissions : PermissionsBase
	{
		public BmfPermissions(User user) : base(user) { }
		public override Expression<Func<Agency, bool>> AgencyFilter
		{
			get
			{
				return a => a.AgencyGroup.Apps.Any(app => app.Fund.MasterFundId == 73);
			}
		}
		public override Expression<Func<Client, bool>> ClientsFilter
		{
			get
			{
				return c => c.GGReportedCount > 0;
			
			}
		}
		public override Expression<Func<Client, bool>> CfsClientsFilter
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
				return c => c.Apps.Any(a => a.Fund.MasterFundId == 73 && a.AppBudgets.Any());
			}
		}
		public override Expression<Func<AppBudget, bool>> AppBudgetsFilter
		{
			get
			{
				return a => a.App.Fund.MasterFundId == 73 && a.StatusId == (int)AppBudgetApprovalStatuses.Approved;
			}
		}
		public override Expression<Func<MainReport, bool>> MainReportsFilter
		{
			get
			{
				return f => f.AppBudget.App.Fund.MasterFundId == 73 && f.StatusId == (int)MainReport.Statuses.Approved;
			}
		}
		public override Expression<Func<SubReport, bool>> SubReportsFilter
		{
			get
			{
				return cr => cr.MainReport.AppBudget.App.Fund.MasterFundId == 73
					&& cr.MainReport.StatusId == (int)MainReport.Statuses.Approved 
                    && cr.AppBudgetService.Service.ReportingMethodId != (int)Service.ReportingMethods.ClientEventsCount;

			}
		}
		public override Expression<Func<AppBudgetService, bool>> AppBudgetServicesFilter
		{ get { return s => s.AppBudget.App.Fund.MasterFundId == 73 && s.Service.ReportingMethodId != (int)Service.ReportingMethods.ClientEventsCount; } }
		public override Expression<Func<AppBudgetServiceAudit, bool>> AppBudgetServiceAuditsFilter
		{ get { return s => s.AppBudget.App.Fund.MasterFundId == 73; } }
		public override Expression<Func<App, bool>> AppsFilter
		{
			get
			{
				return a => a.Fund.MasterFundId == 73;
			}
		}
		public override Expression<Func<Fund, bool>> FundsFilter
		{
			get
			{
				return f => f.MasterFundId == 73;
			}
		}
		public override Expression<Func<MasterFund, bool>> MasterFundsFilter
		{
			get
			{
				return f => f.Id == 73;
			}
		}
		public override Expression<Func<ClientReport, bool>> ClientReportsFilter
		{
			get
			{
				return cr => cr.SubReport.MainReport.AppBudget.App.Fund.MasterFundId == 73
					&& cr.SubReport.MainReport.StatusId == (int)MainReport.Statuses.Approved;

			}
		}

        public override Expression<Func<SupportiveCommunitiesReport, bool>> SupportiveCommunitiesReportsFilter
        {
            get
            {
            return cr => false;
            }
        }


        public override Expression<Func<DaysCentersReport, bool>> DaysCentersReportsFilter
        {
            get
            {
                return cr => false;
            }
        }

		public override Expression<Func<SoupKitchensReport, bool>> SoupKitchensReportsFilter
		{
			get
			{
				return cr => false;
			}
		}

		public override Expression<Func<ClientEventsCountReport, bool>> ClientEventsCountReportsFilter
		{
			get
			{
				return cr => false;
			}
		}

		public override Expression<Func<EmergencyReport, bool>> EmergencyReportsFilter
		{
			get
			{
				return cr => cr.SubReport.MainReport.AppBudget.App.Fund.MasterFundId == 73
					&& cr.SubReport.MainReport.StatusId == (int)MainReport.Statuses.Approved;
			}
		}
		public override Expression<Func<MhmReport, bool>> MhmReportsFilter
		{
			get
			{
				return f => f.Client.AgencyId == this.User.AgencyId &&
					f.SubReport.MainReport.AppBudget.App.Fund.MasterFundId == 73;
			}
		}
		public override Expression<Func<PersonnelReport, bool>> PersonnelReportsFilter
		{
			get
			{
				return f => f.AppBudgetService.AppBudget.App.Fund.MasterFundId == 73
					&& f.AppBudgetService.AppBudget.StatusId == (int)AppBudgetApprovalStatuses.Approved;
			}
		}

		public override bool CanCreateNewClient { get { return false; } }
		public override bool CanUpdateExistingClient { get { return false; } }
		public override bool CanCreateMainReport { get { return false; } }
		public override bool CanEditCeefFields { get { return false; } }
	}
}
