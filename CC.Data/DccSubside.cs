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
    public partial class DccSubside
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
    
        public virtual ICollection<Client> Clients
        {
            get
            {
                if (_clients == null)
                {
                    var newCollection = new FixupCollection<Client>();
                    newCollection.CollectionChanged += FixupClients;
                    _clients = newCollection;
                }
                return _clients;
            }
            set
            {
                if (!ReferenceEquals(_clients, value))
                {
                    var previousValue = _clients as FixupCollection<Client>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupClients;
                    }
                    _clients = value;
                    var newValue = value as FixupCollection<Client>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupClients;
                    }
                }
            }
        }
        private ICollection<Client> _clients;

        #endregion

        #region Association Fixup
    
        private void FixupClients(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Client item in e.NewItems)
                {
                    item.DccSubside = this;
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (Client item in e.OldItems)
                {
                    if (ReferenceEquals(item.DccSubside, this))
                    {
                        item.DccSubside = null;
                    }
                }
            }
        }

        #endregion

    }
}
