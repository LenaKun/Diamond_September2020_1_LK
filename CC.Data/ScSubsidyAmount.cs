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
    public partial class ScSubsidyAmount
    {
        #region Primitive Properties
    
        public virtual int LevelId
        {
            get { return _levelId; }
            set
            {
                if (_levelId != value)
                {
                    if (ScSubsidyLevel != null && ScSubsidyLevel.Id != value)
                    {
                        ScSubsidyLevel = null;
                    }
                    _levelId = value;
                }
            }
        }
        private int _levelId;
    
        public virtual bool FullSubsidy
        {
            get;
            set;
        }
    
        public virtual System.DateTime StartDate
        {
            get;
            set;
        }
    
        public virtual Nullable<decimal> Amount
        {
            get;
            set;
        }

        #endregion

        #region Navigation Properties
    
    		
    	public virtual ScSubsidyLevel ScSubsidyLevel
        {
            get { return _scSubsidyLevel; }
            set
            {
                if (!ReferenceEquals(_scSubsidyLevel, value))
                {
                    var previousValue = _scSubsidyLevel;
                    _scSubsidyLevel = value;
                    FixupScSubsidyLevel(previousValue);
                }
            }
        }
        private ScSubsidyLevel _scSubsidyLevel;

        #endregion

        #region Association Fixup
    
        private void FixupScSubsidyLevel(ScSubsidyLevel previousValue)
        {
            if (previousValue != null && previousValue.ScSubsidyAmounts.Contains(this))
            {
                previousValue.ScSubsidyAmounts.Remove(this);
            }
    
            if (ScSubsidyLevel != null)
            {
                if (!ScSubsidyLevel.ScSubsidyAmounts.Contains(this))
                {
                    ScSubsidyLevel.ScSubsidyAmounts.Add(this);
                }
                if (LevelId != ScSubsidyLevel.Id)
                {
                    LevelId = ScSubsidyLevel.Id;
                }
            }
        }

        #endregion

    }
}
