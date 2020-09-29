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
    public partial class PersonnelReport
    {
        #region Primitive Properties
    
        public virtual int Id
        {
            get;
            set;
        }
    
        public virtual int AppBudgetServiceId
        {
            get { return _appBudgetServiceId; }
            set
            {
                if (_appBudgetServiceId != value)
                {
                    if (AppBudgetService != null && AppBudgetService.Id != value)
                    {
                        AppBudgetService = null;
                    }
                    _appBudgetServiceId = value;
                }
            }
        }
        private int _appBudgetServiceId;
    
        public virtual string Position
        {
            get;
            set;
        }
    
        public virtual Nullable<decimal> Salary
        {
            get;
            set;
        }
    
        public virtual Nullable<decimal> PartTimePercentage
        {
            get;
            set;
        }
    
        public virtual Nullable<decimal> ServicePercentage
        {
            get;
            set;
        }
    
        public virtual string Remarks
        {
            get;
            set;
        }
    
        public virtual int PositionType
        {
            get;
            set;
        }

        #endregion

        #region Navigation Properties
    
    	
    	public virtual AppBudgetService AppBudgetService
        {
            get { return _appBudgetService; }
            set
            {
                if (!ReferenceEquals(_appBudgetService, value))
                {
                    var previousValue = _appBudgetService;
                    _appBudgetService = value;
                    FixupAppBudgetService(previousValue);
                }
            }
        }
        private AppBudgetService _appBudgetService;

        #endregion

        #region Association Fixup
    
        private void FixupAppBudgetService(AppBudgetService previousValue)
        {
            if (previousValue != null && previousValue.PersonnelReports.Contains(this))
            {
                previousValue.PersonnelReports.Remove(this);
            }
    
            if (AppBudgetService != null)
            {
                if (!AppBudgetService.PersonnelReports.Contains(this))
                {
                    AppBudgetService.PersonnelReports.Add(this);
                }
                if (AppBudgetServiceId != AppBudgetService.Id)
                {
                    AppBudgetServiceId = AppBudgetService.Id;
                }
            }
        }

        #endregion

    }
}