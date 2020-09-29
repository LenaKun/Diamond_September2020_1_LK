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
    public partial class App
    {
        #region Primitive Properties
    
        public virtual int Id
        {
            get;
            set;
        }
    
        public virtual int FundId
        {
            get { return _fundId; }
            set
            {
                if (_fundId != value)
                {
                    if (Fund != null && Fund.Id != value)
                    {
                        Fund = null;
                    }
                    _fundId = value;
                }
            }
        }
        private int _fundId;
    
        public virtual int AgencyGroupId
        {
            get { return _agencyGroupId; }
            set
            {
                if (_agencyGroupId != value)
                {
                    if (AgencyGroup != null && AgencyGroup.Id != value)
                    {
                        AgencyGroup = null;
                    }
                    _agencyGroupId = value;
                }
            }
        }
        private int _agencyGroupId;
    
        public virtual string Name
        {
            get;
            set;
        }
    
        public virtual bool AgencyContribution
        {
            get;
            set;
        }
    
        public virtual decimal CcGrant
        {
            get;
            set;
        }
    
        public virtual decimal RequiredMatch
        {
            get;
            set;
        }
    
        public virtual System.DateTime StartDate
        {
            get;
            set;
        }
    
        public virtual System.DateTime EndDate
        {
            get;
            set;
        }
    
        public virtual string CurrencyId
        {
            get { return _currencyId; }
            set
            {
                if (_currencyId != value)
                {
                    if (Currency != null && Currency.Id != value)
                    {
                        Currency = null;
                    }
                    _currencyId = value;
                }
            }
        }
        private string _currencyId;
    
        public virtual bool EndOfYearValidationOnly
        {
            get;
            set;
        }
    
        public virtual bool InterlineTransfer
        {
            get;
            set;
        }
    
        public virtual Nullable<decimal> OtherServicesMax
        {
            get;
            set;
        }
    
        public virtual Nullable<decimal> HomecareMin
        {
            get;
            set;
        }
    
        public virtual Nullable<decimal> AdminMax
        {
            get;
            set;
        }
    
        public virtual Nullable<decimal> MaxNonHcAmount
        {
            get;
            set;
        }
    
        public virtual Nullable<decimal> MaxAdminAmount
        {
            get;
            set;
        }
    
        public virtual Nullable<decimal> HistoricalExpenditureAmount
        {
            get;
            set;
        }
    
        public virtual Nullable<decimal> AvgReimbursementCost
        {
            get;
            set;
        }
    
        public virtual Nullable<decimal> MaxHcCaseManagementPersonnel
        {
            get;
            set;
        }
    
        public virtual Nullable<decimal> MaxServicesPersonnelOther
        {
            get;
            set;
        }
    
        public virtual Nullable<int> FluxxGrantRequestId
        {
            get;
            set;
        }
    
        public virtual string FluxxGrantRequestError
        {
            get;
            set;
        }
    
        public virtual Nullable<int> FuneralExpenses
        {
            get;
            set;
        }

        #endregion

        #region Navigation Properties
    
    	
    	public virtual AgencyGroup AgencyGroup
        {
            get { return _agencyGroup; }
            set
            {
                if (!ReferenceEquals(_agencyGroup, value))
                {
                    var previousValue = _agencyGroup;
                    _agencyGroup = value;
                    FixupAgencyGroup(previousValue);
                }
            }
        }
        private AgencyGroup _agencyGroup;
    
        public virtual ICollection<AppBudget> AppBudgets
        {
            get
            {
                if (_appBudgets == null)
                {
                    var newCollection = new FixupCollection<AppBudget>();
                    newCollection.CollectionChanged += FixupAppBudgets;
                    _appBudgets = newCollection;
                }
                return _appBudgets;
            }
            set
            {
                if (!ReferenceEquals(_appBudgets, value))
                {
                    var previousValue = _appBudgets as FixupCollection<AppBudget>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupAppBudgets;
                    }
                    _appBudgets = value;
                    var newValue = value as FixupCollection<AppBudget>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupAppBudgets;
                    }
                }
            }
        }
        private ICollection<AppBudget> _appBudgets;
    
    	
    	public virtual Fund Fund
        {
            get { return _fund; }
            set
            {
                if (!ReferenceEquals(_fund, value))
                {
                    var previousValue = _fund;
                    _fund = value;
                    FixupFund(previousValue);
                }
            }
        }
        private Fund _fund;
    
        public virtual ICollection<Service> Services
        {
            get
            {
                if (_services == null)
                {
                    var newCollection = new FixupCollection<Service>();
                    newCollection.CollectionChanged += FixupServices;
                    _services = newCollection;
                }
                return _services;
            }
            set
            {
                if (!ReferenceEquals(_services, value))
                {
                    var previousValue = _services as FixupCollection<Service>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupServices;
                    }
                    _services = value;
                    var newValue = value as FixupCollection<Service>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupServices;
                    }
                }
            }
        }
        private ICollection<Service> _services;
    
    	
    	public virtual Currency Currency
        {
            get { return _currency; }
            set
            {
                if (!ReferenceEquals(_currency, value))
                {
                    var previousValue = _currency;
                    _currency = value;
                    FixupCurrency(previousValue);
                }
            }
        }
        private Currency _currency;
    
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
    
        public virtual ICollection<AgencyApp> AgencyApps
        {
            get
            {
                if (_agencyApps == null)
                {
                    var newCollection = new FixupCollection<AgencyApp>();
                    newCollection.CollectionChanged += FixupAgencyApps;
                    _agencyApps = newCollection;
                }
                return _agencyApps;
            }
            set
            {
                if (!ReferenceEquals(_agencyApps, value))
                {
                    var previousValue = _agencyApps as FixupCollection<AgencyApp>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupAgencyApps;
                    }
                    _agencyApps = value;
                    var newValue = value as FixupCollection<AgencyApp>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupAgencyApps;
                    }
                }
            }
        }
        private ICollection<AgencyApp> _agencyApps;

        #endregion

        #region Association Fixup
    
        private void FixupAgencyGroup(AgencyGroup previousValue)
        {
            if (previousValue != null && previousValue.Apps.Contains(this))
            {
                previousValue.Apps.Remove(this);
            }
    
            if (AgencyGroup != null)
            {
                if (!AgencyGroup.Apps.Contains(this))
                {
                    AgencyGroup.Apps.Add(this);
                }
                if (AgencyGroupId != AgencyGroup.Id)
                {
                    AgencyGroupId = AgencyGroup.Id;
                }
            }
        }
    
        private void FixupFund(Fund previousValue)
        {
            if (previousValue != null && previousValue.Apps.Contains(this))
            {
                previousValue.Apps.Remove(this);
            }
    
            if (Fund != null)
            {
                if (!Fund.Apps.Contains(this))
                {
                    Fund.Apps.Add(this);
                }
                if (FundId != Fund.Id)
                {
                    FundId = Fund.Id;
                }
            }
        }
    
        private void FixupCurrency(Currency previousValue)
        {
            if (previousValue != null && previousValue.Apps.Contains(this))
            {
                previousValue.Apps.Remove(this);
            }
    
            if (Currency != null)
            {
                if (!Currency.Apps.Contains(this))
                {
                    Currency.Apps.Add(this);
                }
                if (CurrencyId != Currency.Id)
                {
                    CurrencyId = Currency.Id;
                }
            }
        }
    
        private void FixupAppBudgets(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (AppBudget item in e.NewItems)
                {
                    item.App = this;
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (AppBudget item in e.OldItems)
                {
                    if (ReferenceEquals(item.App, this))
                    {
                        item.App = null;
                    }
                }
            }
        }
    
        private void FixupServices(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Service item in e.NewItems)
                {
                    if (!item.Apps.Contains(this))
                    {
                        item.Apps.Add(this);
                    }
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (Service item in e.OldItems)
                {
                    if (item.Apps.Contains(this))
                    {
                        item.Apps.Remove(this);
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
                    item.App = this;
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (AppExchangeRate item in e.OldItems)
                {
                    if (ReferenceEquals(item.App, this))
                    {
                        item.App = null;
                    }
                }
            }
        }
    
        private void FixupAgencyApps(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (AgencyApp item in e.NewItems)
                {
                    item.App = this;
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (AgencyApp item in e.OldItems)
                {
                    if (ReferenceEquals(item.App, this))
                    {
                        item.App = null;
                    }
                }
            }
        }

        #endregion

    }
}