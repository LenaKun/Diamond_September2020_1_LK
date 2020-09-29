using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CC.Data.Services
{
	class DafEvaluatorPermissions : PermissionsBase
	{
		public  DafEvaluatorPermissions(User user)
			: base(user)
		{

		}
		public override System.Linq.Expressions.Expression<Func<Agency, bool>> AgencyFilter
		{
			get { return a => a.Id == this.User.AgencyId; }
		}

		public override System.Linq.Expressions.Expression<Func<Client, bool>> ClientsFilter
		{
			get
			{
				return c => c.AgencyId == this.User.AgencyId;
			}
		}

		public override System.Linq.Expressions.Expression<Func<Client, bool>> CfsClientsFilter
		{
			get
			{
				return c => c.AgencyId == this.User.AgencyId;
			}
		}

		public override System.Linq.Expressions.Expression<Func<AgencyGroup, bool>> AgencyGroupsFilter
		{
			get
			{

				return ag => ag.Agencies.Any(a => a.Id == this.User.AgencyId);
			}
		}

		public override System.Linq.Expressions.Expression<Func<AppBudget, bool>> AppBudgetsFilter
		{
			get
			{
				return a => false;
			}
		}

		public override System.Linq.Expressions.Expression<Func<MainReport, bool>> MainReportsFilter
		{
			get
			{
				return a => false;
			}
		}

		public override System.Linq.Expressions.Expression<Func<SubReport, bool>> SubReportsFilter
		{
			get
			{
				return a => false;
			}
		}

		public override System.Linq.Expressions.Expression<Func<AppBudgetService, bool>> AppBudgetServicesFilter
		{
			get
			{
				return a => false;
			}
		}

		public override System.Linq.Expressions.Expression<Func<AppBudgetServiceAudit, bool>> AppBudgetServiceAuditsFilter
		{
			get
			{
				return a => false;
			}
		}

		public override System.Linq.Expressions.Expression<Func<Region, bool>> RegionsFilter
		{
			get
			{
				return r => r.Countries.SelectMany(c => c.AgencyGroups).SelectMany(ag => ag.Agencies)
					.Any(a => a.Id == this.User.AgencyId);

			}
		}

		public override System.Linq.Expressions.Expression<Func<ClientReport, bool>> ClientReportsFilter
		{
			get { return a => false; }
		}

		public override System.Linq.Expressions.Expression<Func<EmergencyReport, bool>> EmergencyReportsFilter
		{
			get { return a => false; }
		}

		public override System.Linq.Expressions.Expression<Func<SupportiveCommunitiesReport, bool>> SupportiveCommunitiesReportsFilter
		{
			get { return a => false; }
		}

		public override System.Linq.Expressions.Expression<Func<DaysCentersReport, bool>> DaysCentersReportsFilter
		{
			get { return a => false; }
		}
		public override System.Linq.Expressions.Expression<Func<SoupKitchensReport, bool>> SoupKitchensReportsFilter
		{
			get { return a => false; }
		}

		public override System.Linq.Expressions.Expression<Func<MhmReport, bool>> MhmReportsFilter
		{
			get { return a => false; }
		}

		public override System.Linq.Expressions.Expression<Func<User, bool>> UsersFilter
		{
			get { return a => a.Id == this.User.Id; }
		}

		public override System.Linq.Expressions.Expression<Func<PersonnelReport, bool>> PersonnelReportsFilter
		{
			get { return a => false; }
		}

		public override System.Linq.Expressions.Expression<Func<App, bool>> AppsFilter
		{
			get { return a => false; }
		}

		public override System.Linq.Expressions.Expression<Func<Fund, bool>> FundsFilter
		{
			get { return a => false; }
		}

		public override System.Linq.Expressions.Expression<Func<MasterFund, bool>> MasterFundsFilter
		{
			get { return a => false; }
		}

		public override System.Linq.Expressions.Expression<Func<Import, bool>> ImportsFilter
		{
			get { return a => false; }
		}

		public override bool CanChangeApprovalStatus
		{
			get { return false; }
		}

		public override bool CanChangeApprovedClientName
		{
			get { return false; }
		}

		public override bool CanCreateNewClient
		{
			get { return false; }
		}

		public override bool CanUpdateExistingClient
		{
			get { return false; }
		}

		public override bool CanEditCeefFields
		{
			get { return false; }
		}

		public override bool CanChangeDeceasedDate
		{
			get { return false; }
		}

		//public override bool CanChangeGfHours
		//{
		//	get { return false; }
		//}

		public override bool CanChangeExceptionalHours
		{
			get { return false; }
		}

		public override bool CanDeleteClient
		{
			get { return false; }
		}

		public override bool CanDeleteHcePeriod
		{
			get { return false; }
		}

		public override bool CanDeleteFuncScore
		{
			get { return false; }
		}

		public override bool CanSeeProgramField
		{
			get { return false; }
		}

		public override bool CanChangeUserRole
		{
			get { return false; }
		}

		public override bool CanAccessInternalRemarks
		{
			get { return false; }
		}

		public override bool CanCreateMainReport
		{
			get { return false; }
		}

		public override bool ShowDuplicatesNavigation
		{
			get { return false; }
		}
		public override System.Linq.Expressions.Expression<Func<Daf, bool>> DafFilter
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
	}
}
