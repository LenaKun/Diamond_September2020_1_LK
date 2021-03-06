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
    public partial class GrandfatherHour
    {
        #region Primitive Properties
    
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
    
        public virtual System.DateTime StartDate
        {
            get;
            set;
        }
    
        public virtual decimal Value
        {
            get;
            set;
        }
    
        public virtual int Type
        {
            get;
            set;
        }
    
        public virtual System.DateTime UpdatedAt
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

        #endregion

        #region Navigation Properties
    
    	
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
    
        private void FixupClient(Client previousValue)
        {
            if (previousValue != null && previousValue.GrandfatherHours.Contains(this))
            {
                previousValue.GrandfatherHours.Remove(this);
            }
    
            if (Client != null)
            {
                if (!Client.GrandfatherHours.Contains(this))
                {
                    Client.GrandfatherHours.Add(this);
                }
                if (ClientId != Client.Id)
                {
                    ClientId = Client.Id;
                }
            }
        }
    
        private void FixupUser(User previousValue)
        {
            if (previousValue != null && previousValue.GrandfatherHours.Contains(this))
            {
                previousValue.GrandfatherHours.Remove(this);
            }
    
            if (User != null)
            {
                if (!User.GrandfatherHours.Contains(this))
                {
                    User.GrandfatherHours.Add(this);
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
