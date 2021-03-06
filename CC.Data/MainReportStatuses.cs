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
    public partial class MainReportStatuses
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
    
        public virtual string FluxxStatusName
        {
            get;
            set;
        }

        #endregion

        #region Navigation Properties
    
        public virtual ICollection<MainReport> MainReports
        {
            get
            {
                if (_mainReports == null)
                {
                    var newCollection = new FixupCollection<MainReport>();
                    newCollection.CollectionChanged += FixupMainReports;
                    _mainReports = newCollection;
                }
                return _mainReports;
            }
            set
            {
                if (!ReferenceEquals(_mainReports, value))
                {
                    var previousValue = _mainReports as FixupCollection<MainReport>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupMainReports;
                    }
                    _mainReports = value;
                    var newValue = value as FixupCollection<MainReport>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupMainReports;
                    }
                }
            }
        }
        private ICollection<MainReport> _mainReports;

        #endregion

        #region Association Fixup
    
        private void FixupMainReports(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (MainReport item in e.NewItems)
                {
                    item.MainReportStatuses = this;
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (MainReport item in e.OldItems)
                {
                    if (ReferenceEquals(item.MainReportStatuses, this))
                    {
                        item.MainReportStatuses = null;
                    }
                }
            }
        }

        #endregion

    }
}
