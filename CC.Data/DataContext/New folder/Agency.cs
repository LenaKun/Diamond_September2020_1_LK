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

namespace CC.Data.DataContext
{
    public partial class Agency
    {
        public Agency()
        {
            this.AgenciesServices = new HashSet<AgenciesService>();
            this.AgencyBudgets = new HashSet<AgencyBudget>();
            this.Clients = new HashSet<Client>();
            this.MainReports = new HashSet<MainReport>();
            this.Users = new HashSet<User>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public Nullable<int> RegionId { get; set; }
        public Nullable<int> GroupId { get; set; }
    
        public virtual Region Region { get; set; }
        public virtual AgencyGroup AgencyGroup { get; set; }
        public virtual ICollection<AgenciesService> AgenciesServices { get; set; }
        public virtual ICollection<AgencyBudget> AgencyBudgets { get; set; }
        public virtual ICollection<Client> Clients { get; set; }
        public virtual ICollection<MainReport> MainReports { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
    
}