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
    public partial class Service
    {
        public Service()
        {
            this.AgenciesServices = new HashSet<AgenciesService>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public int TypeId { get; set; }
        public Nullable<int> ReportingMethod { get; set; }
    
        public virtual ICollection<AgenciesService> AgenciesServices { get; set; }
        public virtual ServiceType ServiceType { get; set; }
    }
    
}
