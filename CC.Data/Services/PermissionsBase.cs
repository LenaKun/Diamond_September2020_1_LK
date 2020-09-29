using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace CC.Data.Services
{
	class PermissionsBase : CC.Data.Services.IPermissionsBase
	{
		public PermissionsBase(User user) { this.User = user; }
		public User User { get; private set; }
		public virtual bool CanChangeApprovalStatus { get { return false; } }
		public virtual bool CanCreateNewClient { get { return false; } }
		public virtual bool CanUpdateExistingClient { get { return false; } }
		public virtual bool CanEditCeefFields { get { return false; } }
		public virtual bool CanDeleteClient { get { return false; } }
		public virtual bool CanDeleteHcePeriod { get { return false; } }
		public virtual bool CanDeleteFuncScore { get { return false; } }
		public virtual bool CanSeeProgramField { get { return false; } }

		public virtual Expression<Func<Agency, bool>> AgencyFilter { get { return f => false; } }
		public virtual Expression<Func<AgencyGroup, bool>> AgencyGroupsFilter { get { return f => false; } }

		public virtual Expression<Func<Client, bool>> ClientsFilter
		{
			get { return f => false; }
		}
		public virtual Expression<Func<Client, bool>> CfsClientsFilter
		{
			get { return f => false; }
		}
		public virtual Expression<Func<AppBudget, bool>> AppBudgetsFilter
		{
			get { return f => false; }
		}

		public virtual bool CanChangeDeceasedDate
		{
			get { return false; }
		}


		//public virtual bool CanChangeGfHours
		//{
		//	get { return false; }
		//}

		public virtual bool CanChangeExceptionalHours { get { return false; } }





		public virtual Expression<Func<MainReport, bool>> MainReportsFilter
		{
			get { return f => false; }
		}


		public virtual Expression<Func<SubReport, bool>> SubReportsFilter
		{
			get { return sr => false; }
		}


		public virtual Expression<Func<AppBudgetService, bool>> AppBudgetServicesFilter
		{
			get { return s => false; }
		}

		public virtual Expression<Func<AppBudgetServiceAudit, bool>> AppBudgetServiceAuditsFilter { get { return f => false; } }

		public virtual Expression<Func<Region, bool>> RegionsFilter
		{
			get { return c => c.Id != 0; }
		}

		public Expression<Func<Country, bool>> CountriesFilter
		{
			get { return c => true; }
		}


		public virtual bool CanCreateMainReport
		{
			get
			{
				return false;
			}
		}


		public virtual Expression<Func<App, bool>> AppsFilter
		{
			get { return a => false; }
		}

		public virtual Expression<Func<Fund, bool>> FundsFilter { get { return f => false; } }

		public virtual Expression<Func<ClientReport, bool>> ClientReportsFilter
		{
			get { return cr => false; }
		}


		public virtual Expression<Func<MhmReport, bool>> MhmReportsFilter
		{
			get { return f => false; }
		}




		public virtual Expression<Func<PersonnelReport, bool>> PersonnelReportsFilter
		{
			get { return f => false; }
		}


		public virtual Expression<Func<EmergencyReport, bool>> EmergencyReportsFilter
		{
			get { return f => false; }
		}


        public virtual Expression<Func<SupportiveCommunitiesReport, bool>> SupportiveCommunitiesReportsFilter
        {
            get { return f => false; }
        }


        public virtual Expression<Func<DaysCentersReport, bool>> DaysCentersReportsFilter
        {
            get { return f => false; }
        }

		public virtual Expression<Func<SoupKitchensReport, bool>> SoupKitchensReportsFilter
		{
			get { return f => false; }
		}

		public virtual Expression<Func<ClientEventsCountReport, bool>> ClientEventsCountReportsFilter
		{
			get { return f => true; }
		}

		public System.Data.SqlClient.SqlParameter[] ClientsFilterParams()
		{
			return new[]
			{
				new System.Data.SqlClient.SqlParameter{ ParameterName="AgencyId", Value=(FixedRoles)this.User.RoleId== FixedRoles.AgencyUser||(FixedRoles)this.User.RoleId== FixedRoles.AgencyUserAndReviewer? this.User.AgencyId:null},
				new System.Data.SqlClient.SqlParameter{ ParameterName="AgencyGroupId", Value=(FixedRoles)this.User.RoleId== FixedRoles.Ser||(FixedRoles)this.User.RoleId== FixedRoles.SerAndReviewer? this.User.AgencyGroupId:null},
				new System.Data.SqlClient.SqlParameter{ ParameterName="RegionId", Value=(FixedRoles)this.User.RoleId== FixedRoles.RegionOfficer? this.User.RegionId:null},
			};
		}


		public virtual Expression<Func<Import, bool>> ImportsFilter
		{
			get
			{
				return f => f.UserId == this.User.Id;
			}
		}


		public virtual bool CanChangeApprovedClientName
		{
			get { return false; }
		}


		public virtual bool ShowDuplicatesNavigation
		{
			get { return false; }
		}


		public virtual Expression<Func<User, bool>> UsersFilter
		{
			get { return f => false; }
		}


		public virtual bool CanChangeUserRole
		{
			get
			{
				return false;
			}
		}


		public virtual FixedRoles[] AllowedRoles
		{
			get { return new FixedRoles[0]; }
		}


		public virtual bool CanAccessInternalRemarks
		{
			get { return false; }
		}


        public virtual Expression<Func<MasterFund, bool>> MasterFundsFilter
        {
            get { return f => true; }
        }


		public virtual bool CanCreateDaf
		{
			get { return false; }
		}

		public virtual Expression<Func<Daf, bool>> DafFilter
		{
			get { return f => false; }
		}


		public virtual bool CanDeleteDaf(Daf.Statuses item)
		{
			return false;
		}
		public virtual bool CanSeeDcc { get { return false; } }
		public virtual bool CanSeeSc { get { return false; } }
	}
}
