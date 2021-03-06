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
    public partial class RelatedFunctionalityLevel
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
    
        public virtual ICollection<FunctionalityLevel> FunctionalityLevels
        {
            get
            {
                if (_functionalityLevels == null)
                {
                    var newCollection = new FixupCollection<FunctionalityLevel>();
                    newCollection.CollectionChanged += FixupFunctionalityLevels;
                    _functionalityLevels = newCollection;
                }
                return _functionalityLevels;
            }
            set
            {
                if (!ReferenceEquals(_functionalityLevels, value))
                {
                    var previousValue = _functionalityLevels as FixupCollection<FunctionalityLevel>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupFunctionalityLevels;
                    }
                    _functionalityLevels = value;
                    var newValue = value as FixupCollection<FunctionalityLevel>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupFunctionalityLevels;
                    }
                }
            }
        }
        private ICollection<FunctionalityLevel> _functionalityLevels;

        #endregion

        #region Association Fixup
    
        private void FixupFunctionalityLevels(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (FunctionalityLevel item in e.NewItems)
                {
                    item.RelatedFunctionalityLevel = this;
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (FunctionalityLevel item in e.OldItems)
                {
                    if (ReferenceEquals(item.RelatedFunctionalityLevel, this))
                    {
                        item.RelatedFunctionalityLevel = null;
                    }
                }
            }
        }

        #endregion

    }
}
