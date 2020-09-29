using System;
using System.Linq.Expressions;
using CC.Data;
namespace CC.Data.Services
{
	public interface IPermissionsBase
	{
		Expression<Func<CC.Data.Agency, bool>> AgencyFilter { get; }
		Expression<Func<Client, Boolean>> ClientsFilter { get; }
		Expression<Func<Client, Boolean>> CfsClientsFilter { get; }
		Expression<Func<AgencyGroup, bool>> AgencyGroupsFilter { get; }
		Expression<Func<AppBudget, bool>> AppBudgetsFilter { get; }
		Expression<Func<MainReport, bool>> MainReportsFilter { get; }
		Expression<Func<SubReport, bool>> SubReportsFilter { get; }
		Expression<Func<AppBudgetService, bool>> AppBudgetServicesFilter { get; }
		Expression<Func<AppBudgetServiceAudit, bool>> AppBudgetServiceAuditsFilter { get; }
		Expression<Func<Region, bool>> RegionsFilter { get; }
		Expression<Func<Country, bool>> CountriesFilter { get; }
		Expression<Func<ClientReport, bool>> ClientReportsFilter { get; }
		Expression<Func<EmergencyReport, bool>> EmergencyReportsFilter { get; }
        Expression<Func<SupportiveCommunitiesReport, bool>> SupportiveCommunitiesReportsFilter { get; }
        Expression<Func<DaysCentersReport, bool>> DaysCentersReportsFilter { get; }
		Expression<Func<SoupKitchensReport, bool>> SoupKitchensReportsFilter { get; }
		Expression<Func<ClientEventsCountReport, bool>> ClientEventsCountReportsFilter { get; }
		Expression<Func<MhmReport, bool>> MhmReportsFilter { get; }
		Expression<Func<User, bool>> UsersFilter { get; }

		Expression<Func<PersonnelReport, bool>> PersonnelReportsFilter { get; }
		Expression<Func<App, bool>> AppsFilter { get; }
		Expression<Func<Fund, bool>> FundsFilter { get; }
        Expression<Func<MasterFund, bool>> MasterFundsFilter { get; }
		Expression<Func<Import, bool>> ImportsFilter { get; }

		/// <summary>
		/// Can change approval status of a client
		/// </summary>
		bool CanChangeApprovalStatus { get; }
		bool CanChangeApprovedClientName { get; }
		bool CanCreateNewClient { get; }
		bool CanUpdateExistingClient { get; }
		bool CanEditCeefFields { get; }
		bool CanChangeDeceasedDate { get; }
		//bool CanChangeGfHours { get; }
		bool CanChangeExceptionalHours { get; }
		bool CanDeleteClient { get; }
		bool CanDeleteHcePeriod { get; }
		bool CanDeleteFuncScore { get; }
		bool CanSeeProgramField { get; }
		bool CanChangeUserRole { get; }
		bool CanAccessInternalRemarks { get; }

		bool CanCreateMainReport { get; }
		CC.Data.User User { get; }


		bool ShowDuplicatesNavigation { get; }

		FixedRoles[] AllowedRoles { get; }

		bool CanCreateDaf { get; }
		Expression<Func<Daf, bool>> DafFilter { get; }
		bool CanDeleteDaf(Daf.Statuses item);
		bool CanSeeDcc { get; }
		bool CanSeeSc { get; }
	}
}
