﻿//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace CC.Data
{
    public partial class ccEntities : DbContext
    {
        public ccEntities()
            : base("name=ccEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<Agency> Agencies { get; set; }
        public DbSet<AgenciesService> AgenciesServices { get; set; }
        public DbSet<AgencyBudget> AgencyBudgets { get; set; }
        public DbSet<AgencyGroup> AgencyGroups { get; set; }
        public DbSet<ApprovalStatus> ApprovalStatuses { get; set; }
        public DbSet<Budget> Budgets { get; set; }
        public DbSet<ClientReport> ClientReports { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<EligibilityProgram> EligibilityPrograms { get; set; }
        public DbSet<FunctionalityLevel> FunctionalityLevels { get; set; }
        public DbSet<FunctionalityScore> FunctionalityScores { get; set; }
        public DbSet<Fund> Funds { get; set; }
        public DbSet<History> Histories { get; set; }
        public DbSet<HomeCareEntitledPeriod> HomeCareEntitledPeriods { get; set; }
        public DbSet<ImpFundStatusTranslation> ImpFundStatusTranslations { get; set; }
        public DbSet<LeaveReason> LeaveReasons { get; set; }
        public DbSet<MainReport> MainReports { get; set; }
        public DbSet<MasterFund> MasterFunds { get; set; }
        public DbSet<MembershipUser> MembershipUsers { get; set; }
        public DbSet<NationalIdType> NationalIdTypes { get; set; }
        public DbSet<Region> Regions { get; set; }
        public DbSet<RelatedFunctionalityLevel> RelatedFunctionalityLevels { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<ServiceReport> ServiceReports { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ServiceType> ServiceTypes { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
