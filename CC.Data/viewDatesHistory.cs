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
    public partial class viewDatesHistory
    {
        #region Primitive Properties
    
        public virtual long Id
        {
            get;
            set;
        }
    
        public virtual int ReferenceId
        {
            get;
            set;
        }
    
        public virtual string TableName
        {
            get;
            set;
        }
    
        public virtual string FieldName
        {
            get;
            set;
        }
    
        public virtual Nullable<System.DateTime> OldValue
        {
            get;
            set;
        }
    
        public virtual Nullable<System.DateTime> NewValue
        {
            get;
            set;
        }
    
        public virtual int UpdatedBy
        {
            get;
            set;
        }
    
        public virtual System.DateTime UpdateDate
        {
            get;
            set;
        }

        #endregion

    }
}
