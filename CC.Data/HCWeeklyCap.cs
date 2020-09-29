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
    public partial class HCWeeklyCap
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
    
        public virtual decimal CapPerPerson
        {
            get;
            set;
        }
    
        public virtual string CurrencyId
        {
            get { return _currencyId; }
            set
            {
                try
                {
                    _settingFK = true;
                    if (_currencyId != value)
                    {
                        if (Currency != null && Currency.Id != value)
                        {
                            Currency = null;
                        }
                        _currencyId = value;
                    }
                }
                finally
                {
                    _settingFK = false;
                }
            }
        }
        private string _currencyId;
    
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
    
        public virtual bool Active
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
                try
                {
                    _settingFK = true;
                    if (_updatedBy != value)
                    {
                        if (User != null && User.Id != value)
                        {
                            User = null;
                        }
                        _updatedBy = value;
                    }
                }
                finally
                {
                    _settingFK = false;
                }
            }
        }
        private int _updatedBy;

        #endregion

        #region Navigation Properties
    
    	
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
    
        public virtual ICollection<Country> Countries
        {
            get
            {
                if (_countries == null)
                {
                    var newCollection = new FixupCollection<Country>();
                    newCollection.CollectionChanged += FixupCountries;
                    _countries = newCollection;
                }
                return _countries;
            }
            set
            {
                if (!ReferenceEquals(_countries, value))
                {
                    var previousValue = _countries as FixupCollection<Country>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupCountries;
                    }
                    _countries = value;
                    var newValue = value as FixupCollection<Country>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupCountries;
                    }
                }
            }
        }
        private ICollection<Country> _countries;
    
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

        #endregion

        #region Association Fixup
    
        private bool _settingFK = false;
    
        private void FixupCurrency(Currency previousValue)
        {
            if (previousValue != null && previousValue.HCWeeklyCaps.Contains(this))
            {
                previousValue.HCWeeklyCaps.Remove(this);
            }
    
            if (Currency != null)
            {
                if (!Currency.HCWeeklyCaps.Contains(this))
                {
                    Currency.HCWeeklyCaps.Add(this);
                }
                if (CurrencyId != Currency.Id)
                {
                    CurrencyId = Currency.Id;
                }
            }
            else if (!_settingFK)
            {
                CurrencyId = null;
            }
        }
    
        private void FixupUser(User previousValue)
        {
            if (previousValue != null && previousValue.HCWeeklyCaps.Contains(this))
            {
                previousValue.HCWeeklyCaps.Remove(this);
            }
    
            if (User != null)
            {
                if (!User.HCWeeklyCaps.Contains(this))
                {
                    User.HCWeeklyCaps.Add(this);
                }
                if (UpdatedBy != User.Id)
                {
                    UpdatedBy = User.Id;
                }
            }
        }
    
        private void FixupCountries(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Country item in e.NewItems)
                {
                    if (!item.HCWeeklyCaps.Contains(this))
                    {
                        item.HCWeeklyCaps.Add(this);
                    }
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (Country item in e.OldItems)
                {
                    if (item.HCWeeklyCaps.Contains(this))
                    {
                        item.HCWeeklyCaps.Remove(this);
                    }
                }
            }
        }
    
        private void FixupFunds(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Fund item in e.NewItems)
                {
                    if (!item.HCWeeklyCaps.Contains(this))
                    {
                        item.HCWeeklyCaps.Add(this);
                    }
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (Fund item in e.OldItems)
                {
                    if (item.HCWeeklyCaps.Contains(this))
                    {
                        item.HCWeeklyCaps.Remove(this);
                    }
                }
            }
        }

        #endregion

    }
}