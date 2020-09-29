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
    public partial class ClientContact
    {
        #region Primitive Properties
    
        public virtual int Id
        {
            get;
            set;
        }
    
        public virtual Nullable<System.DateTime> ContactDate
        {
            get;
            set;
        }
    
        public virtual string ContactedUsing
        {
            get;
            set;
        }
    
        public virtual string Contacted
        {
            get;
            set;
        }
    
        public virtual string CcStaffContact
        {
            get;
            set;
        }
    
        public virtual string ReasonForContact
        {
            get;
            set;
        }
    
        public virtual Nullable<System.DateTime> ResponseRecievedDate
        {
            get;
            set;
        }
    
        public virtual System.DateTime EntryDate
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
    
        public virtual string Filename
        {
            get;
            set;
        }
    
        public virtual int ClientId
        {
            get { return _clientId; }
            set
            {
                if (_clientId != value)
                {
                    if (Client != null && Client.Id != value)
                    {
                        Client = null;
                    }
                    _clientId = value;
                }
            }
        }
        private int _clientId;

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
    
    	
    	public virtual Client Client
        {
            get { return _client; }
            set
            {
                if (!ReferenceEquals(_client, value))
                {
                    var previousValue = _client;
                    _client = value;
                    FixupClient(previousValue);
                }
            }
        }
        private Client _client;

        #endregion

        #region Association Fixup
    
        private void FixupUser(User previousValue)
        {
            if (previousValue != null && previousValue.ClientContacts.Contains(this))
            {
                previousValue.ClientContacts.Remove(this);
            }
    
            if (User != null)
            {
                if (!User.ClientContacts.Contains(this))
                {
                    User.ClientContacts.Add(this);
                }
                if (UserId != User.Id)
                {
                    UserId = User.Id;
                }
            }
        }
    
        private void FixupClient(Client previousValue)
        {
            if (previousValue != null && previousValue.ClientContacts.Contains(this))
            {
                previousValue.ClientContacts.Remove(this);
            }
    
            if (Client != null)
            {
                if (!Client.ClientContacts.Contains(this))
                {
                    Client.ClientContacts.Add(this);
                }
                if (ClientId != Client.Id)
                {
                    ClientId = Client.Id;
                }
            }
        }

        #endregion

    }
}