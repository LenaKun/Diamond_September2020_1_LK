using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Text;

namespace CC.Data.Services
{
    class MaintenancePermissions : SerPermissions
    {
        public MaintenancePermissions(User user) : base(user) { }
        public override Expression<Func<Agency, bool>> AgencyFilter { get { return c => true; } }
        public override Expression<Func<Client, bool>> ClientsFilter { get { return c => true; } }
        public override Expression<Func<Client, bool>> CfsClientsFilter { get { return c => true; } }
        public override Expression<Func<Region, bool>> RegionsFilter { get { return c => true; } }
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
                return c => true;
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
        public override Expression<Func<AppBudgetService, bool>> AppBudgetServicesFilter
        { get { return s => true; } }
        public override Expression<Func<PersonnelReport, bool>> PersonnelReportsFilter
        { get { return f => true; } }
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
        public override Expression<Func<ClientReport, bool>> ClientReportsFilter
        {
            get
            {
                return cr => true;
            }
        }
        public override Expression<Func<MhmReport, bool>> MhmReportsFilter
        {
            get
            {
                return f => true;
            }
        }
        public override Expression<Func<EmergencyReport, bool>> EmergencyReportsFilter { get { return f => true; } }

        public override Expression<Func<SupportiveCommunitiesReport, bool>> SupportiveCommunitiesReportsFilter { get { return f => true; } }

        public override Expression<Func<DaysCentersReport, bool>> DaysCentersReportsFilter { get { return f => true; } }
        public override Expression<Func<SoupKitchensReport, bool>> SoupKitchensReportsFilter { get { return f => true; } }
        public override Expression<Func<Import, bool>> ImportsFilter { get { return f => true; } }

        public override bool CanEditCeefFields { get { return true; } }
        public override bool CanChangeApprovalStatus { get { return true; } }
        public override bool CanChangeDeceasedDate { get { return true; } }
        //public override bool CanChangeGfHours { get { return true; } }
        public override bool CanChangeExceptionalHours { get { return true; } }
        public override bool CanDeleteHcePeriod { get { return true; } }
        public override bool CanDeleteFuncScore { get { return true; } }
        public override bool CanSeeProgramField { get { return true; } }
        public override bool CanCreateMainReport { get { return true; } }
        public override bool CanDeleteClient { get { return true; } }
        public override bool CanChangeApprovedClientName
        {
            get
            {
                return true;
            }
        }
        public override bool ShowDuplicatesNavigation
        {
            get
            {
                return true;
            }
        }
        public override bool CanChangeUserRole
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
                return f => true;
            }
        }
        public override FixedRoles[] AllowedRoles
        {
            get
            {
                return new FixedRoles[] { FixedRoles.AgencyUser,
                    FixedRoles.Ser,
                    FixedRoles.RegionOfficer,
                    FixedRoles.RegionAssistant,
                    FixedRoles.GlobalOfficer,
                    FixedRoles.Admin,
                    FixedRoles.Maintenance,
                    FixedRoles.BMF,
                    FixedRoles.GlobalReadOnly,
                    FixedRoles.RegionReadOnly,
                    FixedRoles.AuditorReadOnly,
                    FixedRoles.DafEvaluator,
                    FixedRoles.DafReviewer,
                    FixedRoles.CfsAdmin,
                    FixedRoles.AgencyUserAndReviewer,
                    FixedRoles.SerAndReviewer
                };
            }
        }
        public override bool CanAccessInternalRemarks
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
                return f => true;
            }
        }
        public override bool CanCreateDaf
        {
            get
            {
                return true;
            }
        }
        public override bool CanDeleteDaf(Daf.Statuses item)
        {
            return true;
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
