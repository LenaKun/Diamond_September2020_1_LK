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
    public partial class Currency
    {
        #region Primitive Properties
    
        public virtual string Id
        {
            get;
            set;
        }
    
        public virtual string Name
        {
            get;
            set;
        }
    
        public virtual decimal ExcRate
        {
            get;
            set;
        }

        #endregion

        #region Navigation Properties
    
        public virtual ICollection<Fund> Funds
        {
            get
            {
                if (_funds == null)
                {
                    var newCollection = new FixupCollection<Fund>();
                    newCollection.CollectionChanged += FixupFunds;
                    _funds = newCollection;
                }
                return _funds;
            }
            set
            {
                if (!ReferenceEquals(_funds, value))
                {
                    var previousValue = _funds as FixupCollection<Fund>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupFunds;
                    }
                    _funds = value;
                    var newValue = value as FixupCollection<Fund>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupFunds;
                    }
                }
            }
        }
        private ICollection<Fund> _funds;
    
        public virtual ICollection<MasterFund> MasterFunds
        {
            get
            {
                if (_masterFunds == null)
                {
                    var newCollection = new FixupCollection<MasterFund>();
                    newCollection.CollectionChanged += FixupMasterFunds;
                    _masterFunds = newCollection;
                }
                return _masterFunds;
            }
            set
            {
                if (!ReferenceEquals(_masterFunds, value))
                {
                    var previousValue = _masterFunds as FixupCollection<MasterFund>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupMasterFunds;
                    }
                    _masterFunds = value;
                    var newValue = value as FixupCollection<MasterFund>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupMasterFunds;
                    }
                }
            }
        }
        private ICollection<MasterFund> _masterFunds;
    
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
    
        public virtual ICollection<App> Apps
        {
            get
            {
                if (_apps == null)
                {
                    var newCollection = new FixupCollection<App>();
                    newCollection.CollectionChanged += FixupApps;
                    _apps = newCollection;
                }
                return _apps;
            }
            set
            {
                if (!ReferenceEquals(_apps, value))
                {
                    var previousValue = _apps as FixupCollection<App>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupApps;
                    }
                    _apps = value;
                    var newValue = value as FixupCollection<App>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupApps;
                    }
                }
            }
        }
        private ICollection<App> _apps;
    
        public virtual ICollection<AppExchangeRate> AppExchangeRates
        {
            get
            {
                if (_appExchangeRates == null)
                {
                    var newCollection = new FixupCollection<AppExchangeRate>();
                    newCollection.CollectionChanged += FixupAppExchangeRates;
                    _appExchangeRates = newCollection;
                }
                return _appExchangeRates;
            }
            set
            {
                if (!ReferenceEquals(_appExchangeRates, value))
                {
                    var previousValue = _appExchangeRates as FixupCollection<AppExchangeRate>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupAppExchangeRates;
                    }
                    _appExchangeRates = value;
                    var newValue = value as FixupCollection<AppExchangeRate>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupAppExchangeRates;
                    }
                }
            }
        }
        private ICollection<AppExchangeRate> _appExchangeRates;
    
        public virtual ICollection<FundExchangeRate> FundExchangeRates
        {
            get
            {
                if (_fundExchangeRates == null)
                {
                    var newCollection = new FixupCollection<FundExchangeRate>();
                    newCollection.CollectionChanged += FixupFundExchangeRates;
                    _fundExchangeRates = newCollection;
                }
                return _fundExchangeRates;
            }
            set
            {
                if (!ReferenceEquals(_fundExchangeRates, value))
                {
                    var previousValue = _fundExchangeRates as FixupCollection<FundExchangeRate>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupFundExchangeRates;
                    }
                    _fundExchangeRates = value;
                    var newValue = value as FixupCollection<FundExchangeRate>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupFundExchangeRates;
                    }
                }
            }
        }
        private ICollection<FundExchangeRate> _fundExchangeRates;
    
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
    
        public virtual ICollection<UnmetNeedsOther> UnmetNeedsOthers
        {
            get
            {
                if (_unmetNeedsOthers == null)
                {
                    var newCollection = new FixupCollection<UnmetNeedsOther>();
                    newCollection.CollectionChanged += FixupUnmetNeedsOthers;
                    _unmetNeedsOthers = newCollection;
                }
                return _unmetNeedsOthers;
            }
            set
            {
                if (!ReferenceEquals(_unmetNeedsOthers, value))
                {
                    var previousValue = _unmetNeedsOthers as FixupCollection<UnmetNeedsOther>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupUnmetNeedsOthers;
                    }
                    _unmetNeedsOthers = value;
                    var newValue = value as FixupCollection<UnmetNeedsOther>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupUnmetNeedsOthers;
                    }
                }
            }
        }
        private ICollection<UnmetNeedsOther> _unmetNeedsOthers;
    
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
    
        private void FixupFunds(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Fund item in e.NewItems)
                {
                    item.Currency = this;
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (Fund item in e.OldItems)
                {
                    if (ReferenceEquals(item.Currency, this))
                    {
                        item.Currency = null;
                    }
                }
            }
        }
    
        private void FixupMasterFunds(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (MasterFund item in e.NewItems)
                {
                    item.Currency = this;
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (MasterFund item in e.OldItems)
                {
                    if (ReferenceEquals(item.Currency, this))
                    {
                        item.Currency = null;
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
                    item.Currency = this;
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (EmergencyCap item in e.OldItems)
                {
                    if (ReferenceEquals(item.Currency, this))
                    {
                        item.Currency = null;
                    }
                }
            }
        }
    
        private void FixupApps(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (App item in e.NewItems)
                {
                    item.Currency = this;
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (App item in e.OldItems)
                {
                    if (ReferenceEquals(item.Currency, this))
                    {
                        item.Currency = null;
                    }
                }
            }
        }
    
        private void FixupAppExchangeRates(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (AppExchangeRate item in e.NewItems)
                {
                    item.Currency = this;
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (AppExchangeRate item in e.OldItems)
                {
                    if (ReferenceEquals(item.Currency, this))
                    {
                        item.Currency = null;
                    }
                }
            }
        }
    
        private void FixupFundExchangeRates(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (FundExchangeRate item in e.NewItems)
                {
                    item.Currency = this;
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (FundExchangeRate item in e.OldItems)
                {
                    if (ReferenceEquals(item.Currency, this))
                    {
                        item.Currency = null;
                    }
                }
            }
        }
    
        private void FixupAgencyGroups(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (AgencyGroup item in e.NewItems)
                {
                    item.Currency = this;
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (AgencyGroup item in e.OldItems)
                {
                    if (ReferenceEquals(item.Currency, this))
                    {
                        item.Currency = null;
                    }
                }
            }
        }
    
        private void FixupUnmetNeedsOthers(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (UnmetNeedsOther item in e.NewItems)
                {
                    item.Currency = this;
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (UnmetNeedsOther item in e.OldItems)
                {
                    if (ReferenceEquals(item.Currency, this))
                    {
                        item.Currency = null;
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
                    item.Currency = this;
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (MhmCap item in e.OldItems)
                {
                    if (ReferenceEquals(item.Currency, this))
                    {
                        item.Currency = null;
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
                    item.Currency = this;
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (HCWeeklyCap item in e.OldItems)
                {
                    if (ReferenceEquals(item.Currency, this))
                    {
                        item.Currency = null;
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
                    item.Currency = this;
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (CfsAmount item in e.OldItems)
                {
                    if (ReferenceEquals(item.Currency, this))
                    {
                        item.Currency = null;
                    }
                }
            }
        }

        #endregion

    }
}