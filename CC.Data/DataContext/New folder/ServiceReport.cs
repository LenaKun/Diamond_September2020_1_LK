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
    public partial class ServiceReport
    {
        public ServiceReport()
        {
            this.ClientReports = new HashSet<ClientReport>();
        }
    
        public int Id { get; set; }
        public int MainReportId { get; set; }
        public Nullable<int> TotalUnits { get; set; }
        public Nullable<int> TotalCost { get; set; }
        public decimal MatchingSum { get; set; }
        public decimal AgencyContribution { get; set; }
    
        public virtual ICollection<ClientReport> ClientReports { get; set; }
        public virtual MainReport MainReport { get; set; }
    }
    
}
