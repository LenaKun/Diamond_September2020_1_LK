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
    public partial class ImportHcep
    {
        #region Primitive Properties
    
        public virtual System.Guid ImportId
        {
            get { return _importId; }
            set
            {
                if (_importId != value)
                {
                    if (Import != null && Import.Id != value)
                    {
                        Import = null;
                    }
                    _importId = value;
                }
            }
        }
        private System.Guid _importId;
    
        public virtual int RowIndex
        {
            get;
            set;
        }
    
        public virtual int Id
        {
            get;
            set;
        }
    
        public virtual Nullable<int> ClientId
        {
            get;
            set;
        }
    
        public virtual Nullable<System.DateTime> StartDate
        {
            get;
            set;
        }
    
        public virtual Nullable<System.DateTime> EndDate
        {
            get;
            set;
        }
    
        public virtual Nullable<System.DateTime> UpdatedAt
        {
            get;
            set;
        }
    
        public virtual Nullable<int> UpdatedBy
        {
            get;
            set;
        }
    
        public virtual Nullable<System.DateTime> CfsApproved
        {
            get;
            set;
        }

        #endregion

        #region Navigation Properties
    
    	
    	public virtual Import Import
        {
            get { return _import; }
            set
            {
                if (!ReferenceEquals(_import, value))
                {
                    var previousValue = _import;
                    _import = value;
                    FixupImport(previousValue);
                }
            }
        }
        private Import _import;

        #endregion

        #region Association Fixup
    
        private void FixupImport(Import previousValue)
        {
            if (previousValue != null && previousValue.ImportHceps.Contains(this))
            {
                previousValue.ImportHceps.Remove(this);
            }
    
            if (Import != null)
            {
                if (!Import.ImportHceps.Contains(this))
                {
                    Import.ImportHceps.Add(this);
                }
                if (ImportId != Import.Id)
                {
                    ImportId = Import.Id;
                }
            }
        }

        #endregion

    }
}