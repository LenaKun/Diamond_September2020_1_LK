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
    public abstract partial class CriBase
    {
        #region Primitive Properties
    
        public virtual System.Guid Id
        {
            get;
            set;
        }
    
        public virtual System.Guid ImportId
        {
            get;
            set;
        }
    
        public virtual Nullable<int> ClientReportId
        {
            get;
            set;
        }
    
        public virtual Nullable<int> SubReportId
        {
            get;
            set;
        }
    
        public virtual Nullable<int> ClientId
        {
            get;
            set;
        }
    
        public virtual string Remarks
        {
            get;
            set;
        }
    
        public virtual Nullable<int> RowIndex
        {
            get;
            set;
        }
    
        public virtual string Errors
        {
            get;
            set;
        }
    
        public virtual string UniqueCircumstances
        {
            get;
            set;
        }

        #endregion

    }
}
