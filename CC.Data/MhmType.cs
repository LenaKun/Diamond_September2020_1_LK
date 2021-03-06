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
    public partial class MhmType
    {
        #region Primitive Properties
    
        public virtual int Id
        {
            get;
            set;
        }
    
        public virtual string Name
        {
            get;
            set;
        }

        #endregion

        #region Navigation Properties
    
        public virtual ICollection<MhmReport> MhmReports
        {
            get
            {
                if (_mhmReports == null)
                {
                    var newCollection = new FixupCollection<MhmReport>();
                    newCollection.CollectionChanged += FixupMhmReports;
                    _mhmReports = newCollection;
                }
                return _mhmReports;
            }
            set
            {
                if (!ReferenceEquals(_mhmReports, value))
                {
                    var previousValue = _mhmReports as FixupCollection<MhmReport>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupMhmReports;
                    }
                    _mhmReports = value;
                    var newValue = value as FixupCollection<MhmReport>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupMhmReports;
                    }
                }
            }
        }
        private ICollection<MhmReport> _mhmReports;

        #endregion

        #region Association Fixup
    
        private void FixupMhmReports(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (MhmReport item in e.NewItems)
                {
                    item.MhmType = this;
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (MhmReport item in e.OldItems)
                {
                    if (ReferenceEquals(item.MhmType, this))
                    {
                        item.MhmType = null;
                    }
                }
            }
        }

        #endregion

    }
}
