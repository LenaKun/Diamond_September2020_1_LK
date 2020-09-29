//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace CC.Data
{
    public partial class User
    {
        public User()
        {
            this.AgenciesServices = new HashSet<AgenciesService>();
            this.AgenciesServices1 = new HashSet<AgenciesService>();
            this.ClientReports = new HashSet<ClientReport>();
            this.Clients = new HashSet<Client>();
            this.FunctionalityScores = new HashSet<FunctionalityScore>();
            this.Histories = new HashSet<History>();
            this.HomeCareEntitledPeriods = new HashSet<HomeCareEntitledPeriod>();
        }
    
        public int Id { get; set; }
        public System.Guid UniqueId { get; set; }
        public int RoleId { get; set; }
        public Nullable<int> AgencyId { get; set; }
        public Nullable<int> AgencyGroupId { get; set; }
        public Nullable<int> RegionId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Comment { get; set; }
    
        public virtual Agency Agency { get; set; }
        public virtual ICollection<AgenciesService> AgenciesServices { get; set; }
        public virtual ICollection<AgenciesService> AgenciesServices1 { get; set; }
        public virtual AgencyGroup AgencyGroup { get; set; }
        public virtual ICollection<ClientReport> ClientReports { get; set; }
        public virtual ICollection<Client> Clients { get; set; }
        public virtual ICollection<FunctionalityScore> FunctionalityScores { get; set; }
        public virtual ICollection<History> Histories { get; set; }
        public virtual ICollection<HomeCareEntitledPeriod> HomeCareEntitledPeriods { get; set; }
        public virtual MembershipUser MembershipUser { get; set; }
        public virtual Region Region { get; set; }
        public virtual Role Role { get; set; }
    }
    
}
