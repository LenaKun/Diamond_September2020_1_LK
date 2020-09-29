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
    public partial class DafDeleted
    {
        #region Primitive Properties
    
        public virtual int Id
        {
            get;
            set;
        }
    
        public virtual int ClientId
        {
            get;
            set;
        }
    
        public virtual int AgencyId
        {
            get;
            set;
        }
    
        public virtual int EvaluatorId
        {
            get;
            set;
        }
    
        public virtual Nullable<System.DateTime> AssessmentDate
        {
            get;
            set;
        }
    
        public virtual Nullable<System.DateTime> EffectiveDate
        {
            get;
            set;
        }
    
        public virtual string Xml
        {
            get;
            set;
        }
    
        public virtual Nullable<decimal> GovernmentHours
        {
            get;
            set;
        }
    
        public virtual Nullable<decimal> ExceptionalHours
        {
            get;
            set;
        }
    
        public virtual string Comments
        {
            get;
            set;
        }
    
        public virtual System.DateTime DeletedAt
        {
            get;
            set;
        }
    
        public virtual int DeletedBy
        {
            get;
            set;
        }
    
        public virtual System.DateTime CreatedAt
        {
            get;
            set;
        }
    
        public virtual Nullable<System.DateTime> UpdatedAt
        {
            get;
            set;
        }
    
        public virtual int StatusId
        {
            get;
            set;
        }
    
        public virtual string EvaluatorPosition
        {
            get;
            set;
        }
    
        public virtual Nullable<System.DateTime> SignedAt
        {
            get;
            set;
        }
    
        public virtual Nullable<int> SignedBy
        {
            get;
            set;
        }
    
        public virtual Nullable<System.DateTime> ReviewedAt
        {
            get;
            set;
        }
    
        public virtual Nullable<int> ReviewedBy
        {
            get;
            set;
        }
    
        public virtual int CreatedBy
        {
            get;
            set;
        }
    
        public virtual Nullable<int> UpdatedBy
        {
            get;
            set;
        }

        #endregion

    }
}