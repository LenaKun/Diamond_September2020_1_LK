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
    public partial class spFinancialSummaryOverview_Result
    {
        #region Primitive Properties
    
        public int AgencyId
        {
            get;
            set;
        }
    
        public string AgencyName
        {
            get;
            set;
        }
    
        public int ServiceId
        {
            get;
            set;
        }
    
        public string ServiceName
        {
            get;
            set;
        }
    
        public string ServiceTypeName
        {
            get;
            set;
        }
    
        public Nullable<decimal> Amount
        {
            get;
            set;
        }
    
        public Nullable<int> ClientsCount
        {
            get;
            set;
        }
    
        public Nullable<int> UniqueClientsCount
        {
            get;
            set;
        }
    
        public Nullable<int> FundsCount
        {
            get;
            set;
        }
    
        public string MasterFundName
        {
            get;
            set;
        }
    
        public Nullable<decimal> CcGrant
        {
            get;
            set;
        }
    
        public Nullable<decimal> AverageCostPerUnduplicatedClient
        {
            get;
            set;
        }

        #endregion

    }
}
