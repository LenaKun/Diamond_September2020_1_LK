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
    public partial class RelatedFunctionalityLevel
    {
        public RelatedFunctionalityLevel()
        {
            this.FunctionalityLevels = new HashSet<FunctionalityLevel>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
    
        public virtual ICollection<FunctionalityLevel> FunctionalityLevels { get; set; }
    }
    
}
