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
    public partial class Country
    {
        #region Primitive Properties
    
        public virtual int Id
        {
            get;
            set;
        }
    
        public virtual string Code
        {
            get;
            set;
        }
    
        public virtual string Name
        {
            get;
            set;
        }
    
        public virtual bool IncomeVerificationRequired
        {
            get;
            set;
        }
    
        public virtual string CcName
        {
            get;
            set;
        }
    
        public virtual string CcCode
        {
            get;
            set;
        }
    
        public virtual int RegionId
        {
            get { return _regionId; }
            set
            {
                try
                {
                    _settingFK = true;
                    if (_regionId != value)
                    {
                        if (Region != null && Region.Id != value)
                        {
                            Region = null;
                        }
                        _regionId = value;
                    }
                }
                finally
                {
                    _settingFK = false;
                }
            }
        }
        private int _regionId;
    
        public virtual string Culture
        {
            get { return _culture; }
            set
            {
                try
                {
                    _settingFK = true;
                    if (_culture != value)
                    {
                        if (Language != null && Language.Id != value)
                        {
                            Language = null;
                        }
                        _culture = value;
                    }
                }
                finally
                {
                    _settingFK = false;
                }
            }
        }
        private string _culture;

        #endregion

        #region Navigation Properties
    
    	
    	public virtual Region Region
        {
            get { return _region; }
            set
            {
                if (!ReferenceEquals(_region, value))
                {
                    var previousValue = _region;
                    _region = value;
                    FixupRegion(previousValue);
                }
            }
        }
        private Region _region;
    
        public virtual ICollection<AgencyGroup> AgencyGroups
        {
            get
            {
                if (_agencyGroups == null)
                {
                    var newCollection = new FixupCollection<AgencyGroup>();
                    newCollection.CollectionChanged += FixupAgencyGroups;
                    _agencyGroups = newCollection;
                }
                return _agencyGroups;
            }
            set
            {
                if (!ReferenceEquals(_agencyGroups, value))
                {
                    var previousValue = _agencyGroups as FixupCollection<AgencyGroup>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupAgencyGroups;
                    }
                    _agencyGroups = value;
                    var newValue = value as FixupCollection<AgencyGroup>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupAgencyGroups;
                    }
                }
            }
        }
        private ICollection<AgencyGroup> _agencyGroups;
    
        public virtual ICollection<EmergencyCap> EmergencyCaps
        {
            get
            {
                if (_emergencyCaps == null)
                {
                    var newCollection = new FixupCollection<EmergencyCap>();
                    newCollection.CollectionChanged += FixupEmergencyCaps;
                    _emergencyCaps = newCollection;
                }
                return _emergencyCaps;
            }
            set
            {
                if (!ReferenceEquals(_emergencyCaps, value))
                {
                    var previousValue = _emergencyCaps as FixupCollection<EmergencyCap>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupEmergencyCaps;
                    }
                    _emergencyCaps = value;
                    var newValue = value as FixupCollection<EmergencyCap>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupEmergencyCaps;
                    }
                }
            }
        }
        private ICollection<EmergencyCap> _emergencyCaps;
    
        public virtual ICollection<State> States
        {
            get
            {
                if (_states == null)
                {
                    var newCollection = new FixupCollection<State>();
                    newCollection.CollectionChanged += FixupStates;
                    _states = newCollection;
                }
                return _states;
            }
            set
            {
                if (!ReferenceEquals(_states, value))
                {
                    var previousValue = _states as FixupCollection<State>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupStates;
                    }
                    _states = value;
                    var newValue = value as FixupCollection<State>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupStates;
                    }
                }
            }
        }
        private ICollection<State> _states;
    
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
    
    		
    	public virtual Language Language
        {
            get { return _language; }
            set
            {
                if (!ReferenceEquals(_language, value))
                {
                    var previousValue = _language;
                    _language = value;
                    FixupLanguage(previousValue);
                }
            }
        }
        private Language _language;
    
        public virtual ICollection<ClientHcStatus> ClientHcStatuses
        {
            get
            {
                if (_clientHcStatuses == null)
                {
                    var newCollection = new FixupCollection<ClientHcStatus>();
                    newCollection.CollectionChanged += FixupClientHcStatuses;
                    _clientHcStatuses = newCollection;
                }
                return _clientHcStatuses;
            }
            set
            {
                if (!ReferenceEquals(_clientHcStatuses, value))
                {
                    var previousValue = _clientHcStatuses as FixupCollection<ClientHcStatus>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupClientHcStatuses;
                    }
                    _clientHcStatuses = value;
                    var newValue = value as FixupCollection<ClientHcStatus>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupClientHcStatuses;
                    }
                }
            }
        }
        private ICollection<ClientHcStatus> _clientHcStatuses;
    
        public virtual ICollection<MhmCap> MhmCaps
        {
            get
            {
                if (_mhmCaps == null)
                {
                    var newCollection = new FixupCollection<MhmCap>();
                    newCollection.CollectionChanged += FixupMhmCaps;
                    _mhmCaps = newCollection;
                }
                return _mhmCaps;
            }
            set
            {
                if (!ReferenceEquals(_mhmCaps, value))
                {
                    var previousValue = _mhmCaps as FixupCollection<MhmCap>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupMhmCaps;
                    }
                    _mhmCaps = value;
                    var newValue = value as FixupCollection<MhmCap>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupMhmCaps;
                    }
                }
            }
        }
        private ICollection<MhmCap> _mhmCaps;
    
        public virtual ICollection<HCWeeklyCap> HCWeeklyCaps
        {
            get
            {
                if (_hCWeeklyCaps == null)
                {
                    var newCollection = new FixupCollection<HCWeeklyCap>();
                    newCollection.CollectionChanged += FixupHCWeeklyCaps;
                    _hCWeeklyCaps = newCollection;
                }
                return _hCWeeklyCaps;
            }
            set
            {
                if (!ReferenceEquals(_hCWeeklyCaps, value))
                {
                    var previousValue = _hCWeeklyCaps as FixupCollection<HCWeeklyCap>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupHCWeeklyCaps;
                    }
                    _hCWeeklyCaps = value;
                    var newValue = value as FixupCollection<HCWeeklyCap>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupHCWeeklyCaps;
                    }
                }
            }
        }
        private ICollection<HCWeeklyCap> _hCWeeklyCaps;
    
        public virtual ICollection<CfsAmount> CfsAmounts
        {
            get
            {
                if (_cfsAmounts == null)
                {
                    var newCollection = new FixupCollection<CfsAmount>();
                    newCollection.CollectionChanged += FixupCfsAmounts;
                    _cfsAmounts = newCollection;
                }
                return _cfsAmounts;
            }
            set
            {
                if (!ReferenceEquals(_cfsAmounts, value))
                {
                    var previousValue = _cfsAmounts as FixupCollection<CfsAmount>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupCfsAmounts;
                    }
                    _cfsAmounts = value;
                    var newValue = value as FixupCollection<CfsAmount>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupCfsAmounts;
                    }
                }
            }
        }
        private ICollection<CfsAmount> _cfsAmounts;

        #endregion

        #region Association Fixup
    
        private bool _settingFK = false;
    
        private void FixupRegion(Region previousValue)
        {
            if (previousValue != null && previousValue.Countries.Contains(this))
            {
                previousValue.Countries.Remove(this);
            }
    
            if (Region != null)
            {
                if (!Region.Countries.Contains(this))
                {
                    Region.Countries.Add(this);
                }
                if (RegionId != Region.Id)
                {
                    RegionId = Region.Id;
                }
            }
        }
    
        private void FixupLanguage(Language previousValue)
        {
            if (previousValue != null && previousValue.Countries.Contains(this))
            {
                previousValue.Countries.Remove(this);
            }
    
            if (Language != null)
            {
                if (!Language.Countries.Contains(this))
                {
                    Language.Countries.Add(this);
                }
                if (Culture != Language.Id)
                {
                    Culture = Language.Id;
                }
            }
            else if (!_settingFK)
            {
                Culture = null;
            }
        }
    
        private void FixupAgencyGroups(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (AgencyGroup item in e.NewItems)
                {
                    item.Country = this;
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (AgencyGroup item in e.OldItems)
                {
                    if (ReferenceEquals(item.Country, this))
                    {
                        item.Country = null;
                    }
                }
            }
        }
    
        private void FixupEmergencyCaps(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (EmergencyCap item in e.NewItems)
                {
                    if (!item.Countries.Contains(this))
                    {
                        item.Countries.Add(this);
                    }
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (EmergencyCap item in e.OldItems)
                {
                    if (item.Countries.Contains(this))
                    {
                        item.Countries.Remove(this);
                    }
                }
            }
        }
    
        private void FixupStates(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (State item in e.NewItems)
                {
                    item.Country = this;
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (State item in e.OldItems)
                {
                    if (ReferenceEquals(item.Country, this))
                    {
                        item.Country = null;
                    }
                }
            }
        }
    
        private void FixupClients(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Client item in e.NewItems)
                {
                    item.Country = this;
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (Client item in e.OldItems)
                {
                    if (ReferenceEquals(item.Country, this))
                    {
                        item.Country = null;
                    }
                }
            }
        }
    
        private void FixupClientHcStatuses(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (ClientHcStatus item in e.NewItems)
                {
                    item.Country = this;
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (ClientHcStatus item in e.OldItems)
                {
                    if (ReferenceEquals(item.Country, this))
                    {
                        item.Country = null;
                    }
                }
            }
        }
    
        private void FixupMhmCaps(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (MhmCap item in e.NewItems)
                {
                    if (!item.Countries.Contains(this))
                    {
                        item.Countries.Add(this);
                    }
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (MhmCap item in e.OldItems)
                {
                    if (item.Countries.Contains(this))
                    {
                        item.Countries.Remove(this);
                    }
                }
            }
        }
    
        private void FixupHCWeeklyCaps(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (HCWeeklyCap item in e.NewItems)
                {
                    if (!item.Countries.Contains(this))
                    {
                        item.Countries.Add(this);
                    }
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (HCWeeklyCap item in e.OldItems)
                {
                    if (item.Countries.Contains(this))
                    {
                        item.Countries.Remove(this);
                    }
                }
            }
        }
    
        private void FixupCfsAmounts(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (CfsAmount item in e.NewItems)
                {
                    if (!item.Countries.Contains(this))
                    {
                        item.Countries.Add(this);
                    }
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (CfsAmount item in e.OldItems)
                {
                    if (item.Countries.Contains(this))
                    {
                        item.Countries.Remove(this);
                    }
                }
            }
        }

        #endregion

    }
}
