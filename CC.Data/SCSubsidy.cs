using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;



    namespace CC.Data
{
    public partial class SCSubsiidy
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
            //if (e.NewItems != null)
          //  {
             // //  foreach (Client item in e.NewItems)
               // {
               //     item.SCSubside = this;
               // }
          //  }

           // if (e.OldItems != null)
          //  {
             //   foreach (Client item in e.OldItems)
              //  {
               //     if (ReferenceEquals(item.SCSubside, this))
               //     {
                //        item.SCSubside = null;
                //    }
               // }
           // }
        }

        #endregion

    }
}

