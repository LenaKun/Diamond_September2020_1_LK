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
    public partial class Comment
    {
        #region Primitive Properties
    
        public virtual int Id
        {
            get;
            set;
        }
    
        public virtual string Content
        {
            get;
            set;
        }
    
        public virtual System.DateTime Date
        {
            get;
            set;
        }
    
        public virtual int UserId
        {
            get { return _userId; }
            set
            {
                if (_userId != value)
                {
                    if (User != null && User.Id != value)
                    {
                        User = null;
                    }
                    _userId = value;
                }
            }
        }
        private int _userId;
    
        public virtual bool IsFile
        {
            get;
            set;
        }
    
        public virtual bool PostApprovalComment
        {
            get;
            set;
        }

        #endregion

        #region Navigation Properties
    
        public virtual ICollection<MainReport> MainReportAgencyComments
        {
            get
            {
                if (_mainReportAgencyComments == null)
                {
                    var newCollection = new FixupCollection<MainReport>();
                    newCollection.CollectionChanged += FixupMainReportAgencyComments;
                    _mainReportAgencyComments = newCollection;
                }
                return _mainReportAgencyComments;
            }
            set
            {
                if (!ReferenceEquals(_mainReportAgencyComments, value))
                {
                    var previousValue = _mainReportAgencyComments as FixupCollection<MainReport>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupMainReportAgencyComments;
                    }
                    _mainReportAgencyComments = value;
                    var newValue = value as FixupCollection<MainReport>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupMainReportAgencyComments;
                    }
                }
            }
        }
        private ICollection<MainReport> _mainReportAgencyComments;
    
        public virtual ICollection<MainReport> MainReportPoComments
        {
            get
            {
                if (_mainReportPoComments == null)
                {
                    var newCollection = new FixupCollection<MainReport>();
                    newCollection.CollectionChanged += FixupMainReportPoComments;
                    _mainReportPoComments = newCollection;
                }
                return _mainReportPoComments;
            }
            set
            {
                if (!ReferenceEquals(_mainReportPoComments, value))
                {
                    var previousValue = _mainReportPoComments as FixupCollection<MainReport>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupMainReportPoComments;
                    }
                    _mainReportPoComments = value;
                    var newValue = value as FixupCollection<MainReport>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupMainReportPoComments;
                    }
                }
            }
        }
        private ICollection<MainReport> _mainReportPoComments;
    
    	
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
    
        public virtual ICollection<MainReport> MainReportInternalComments
        {
            get
            {
                if (_mainReportInternalComments == null)
                {
                    var newCollection = new FixupCollection<MainReport>();
                    newCollection.CollectionChanged += FixupMainReportInternalComments;
                    _mainReportInternalComments = newCollection;
                }
                return _mainReportInternalComments;
            }
            set
            {
                if (!ReferenceEquals(_mainReportInternalComments, value))
                {
                    var previousValue = _mainReportInternalComments as FixupCollection<MainReport>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupMainReportInternalComments;
                    }
                    _mainReportInternalComments = value;
                    var newValue = value as FixupCollection<MainReport>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupMainReportInternalComments;
                    }
                }
            }
        }
        private ICollection<MainReport> _mainReportInternalComments;
    
        public virtual ICollection<MainReport> MainReportPostApprovalComments
        {
            get
            {
                if (_mainReportPostApprovalComments == null)
                {
                    var newCollection = new FixupCollection<MainReport>();
                    newCollection.CollectionChanged += FixupMainReportPostApprovalComments;
                    _mainReportPostApprovalComments = newCollection;
                }
                return _mainReportPostApprovalComments;
            }
            set
            {
                if (!ReferenceEquals(_mainReportPostApprovalComments, value))
                {
                    var previousValue = _mainReportPostApprovalComments as FixupCollection<MainReport>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupMainReportPostApprovalComments;
                    }
                    _mainReportPostApprovalComments = value;
                    var newValue = value as FixupCollection<MainReport>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupMainReportPostApprovalComments;
                    }
                }
            }
        }
        private ICollection<MainReport> _mainReportPostApprovalComments;

        #endregion

        #region Association Fixup
    
        private void FixupUser(User previousValue)
        {
            if (previousValue != null && previousValue.Comments.Contains(this))
            {
                previousValue.Comments.Remove(this);
            }
    
            if (User != null)
            {
                if (!User.Comments.Contains(this))
                {
                    User.Comments.Add(this);
                }
                if (UserId != User.Id)
                {
                    UserId = User.Id;
                }
            }
        }
    
        private void FixupMainReportAgencyComments(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (MainReport item in e.NewItems)
                {
                    if (!item.AgencyComments.Contains(this))
                    {
                        item.AgencyComments.Add(this);
                    }
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (MainReport item in e.OldItems)
                {
                    if (item.AgencyComments.Contains(this))
                    {
                        item.AgencyComments.Remove(this);
                    }
                }
            }
        }
    
        private void FixupMainReportPoComments(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (MainReport item in e.NewItems)
                {
                    if (!item.PoComments.Contains(this))
                    {
                        item.PoComments.Add(this);
                    }
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (MainReport item in e.OldItems)
                {
                    if (item.PoComments.Contains(this))
                    {
                        item.PoComments.Remove(this);
                    }
                }
            }
        }
    
        private void FixupMainReportInternalComments(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (MainReport item in e.NewItems)
                {
                    if (!item.InternalComments.Contains(this))
                    {
                        item.InternalComments.Add(this);
                    }
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (MainReport item in e.OldItems)
                {
                    if (item.InternalComments.Contains(this))
                    {
                        item.InternalComments.Remove(this);
                    }
                }
            }
        }
    
        private void FixupMainReportPostApprovalComments(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (MainReport item in e.NewItems)
                {
                    if (!item.PostApprovalComments.Contains(this))
                    {
                        item.PostApprovalComments.Add(this);
                    }
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (MainReport item in e.OldItems)
                {
                    if (item.PostApprovalComments.Contains(this))
                    {
                        item.PostApprovalComments.Remove(this);
                    }
                }
            }
        }

        #endregion

    }
}
