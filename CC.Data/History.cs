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
    public partial class History
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
    
        public virtual string OldValue
        {
            get;
            set;
        }
    
        public virtual string NewValue
        {
            get;
            set;
        }
    
        public virtual int UpdatedBy
        {
            get { return _updatedBy; }
            set
            {
                if (_updatedBy != value)
                {
                    if (User != null && User.Id != value)
                    {
                        User = null;
                    }
                    _updatedBy = value;
                }
            }
        }
        private int _updatedBy;
    
        public virtual System.DateTime UpdateDate
        {
            get;
            set;
        }

        #endregion

        #region Navigation Properties
    
    		
    	public virtual User User
        {
            get { return _user; }
            set
            {
                if (!ReferenceEquals(_user, value))
                {
                    var previousValue = _user;
                    _user = value;
                    FixupUser(previousValue);
                }
            }
        }
        private User _user;

        #endregion

        #region Association Fixup
    
        private void FixupUser(User previousValue)
        {
            if (previousValue != null && previousValue.Histories.Contains(this))
            {
                previousValue.Histories.Remove(this);
            }
    
            if (User != null)
            {
                if (!User.Histories.Contains(this))
                {
                    User.Histories.Add(this);
                }
                if (UpdatedBy != User.Id)
                {
                    UpdatedBy = User.Id;
                }
            }
        }

        #endregion

    }
}