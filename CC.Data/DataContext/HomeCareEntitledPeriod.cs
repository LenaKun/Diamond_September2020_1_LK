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
    public partial class HomeCareEntitledPeriod
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public System.DateTime StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public Nullable<System.DateTime> UpdatedAt { get; set; }
        public int UpdatedBy { get; set; }
    
        public virtual Client Client { get; set; }
        public virtual User User { get; set; }
    }
    
}