//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace CC.Data
{
    public partial class ApprovedAppBudgetService
    {
        #region Primitive Properties
    
        public virtual int Id
        {
            get;
            set;
        }
    
        public virtual int AppBudgetId
        {
            get;
            set;
        }
    
        public virtual int ServiceId
        {
            get;
            set;
        }
    
        public virtual int AgencyId
        {
            get;
            set;
        }
    
        public virtual decimal CcGrant
        {
            get;
            set;
        }
    
        public virtual decimal RequiredMatch
        {
            get;
            set;
        }
    
        public virtual decimal AgencyContribution
        {
            get;
            set;
        }
    
        public virtual System.DateTime RecordDate
        {
            get;
            set;
        }

        #endregion

    }
}
