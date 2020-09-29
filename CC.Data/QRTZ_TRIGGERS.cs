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
    public partial class QRTZ_TRIGGERS
    {
        #region Primitive Properties
    
        public virtual string SCHED_NAME
        {
            get { return _sCHED_NAME; }
            set
            {
                if (_sCHED_NAME != value)
                {
                    if (QRTZ_JOB_DETAILS != null && QRTZ_JOB_DETAILS.SCHED_NAME != value)
                    {
                        QRTZ_JOB_DETAILS = null;
                    }
                    _sCHED_NAME = value;
                }
            }
        }
        private string _sCHED_NAME;
    
        public virtual string TRIGGER_NAME
        {
            get;
            set;
        }
    
        public virtual string TRIGGER_GROUP
        {
            get;
            set;
        }
    
        public virtual string JOB_NAME
        {
            get { return _jOB_NAME; }
            set
            {
                if (_jOB_NAME != value)
                {
                    if (QRTZ_JOB_DETAILS != null && QRTZ_JOB_DETAILS.JOB_NAME != value)
                    {
                        QRTZ_JOB_DETAILS = null;
                    }
                    _jOB_NAME = value;
                }
            }
        }
        private string _jOB_NAME;
    
        public virtual string JOB_GROUP
        {
            get { return _jOB_GROUP; }
            set
            {
                if (_jOB_GROUP != value)
                {
                    if (QRTZ_JOB_DETAILS != null && QRTZ_JOB_DETAILS.JOB_GROUP != value)
                    {
                        QRTZ_JOB_DETAILS = null;
                    }
                    _jOB_GROUP = value;
                }
            }
        }
        private string _jOB_GROUP;
    
        public virtual string DESCRIPTION
        {
            get;
            set;
        }
    
        public virtual Nullable<long> NEXT_FIRE_TIME
        {
            get;
            set;
        }
    
        public virtual Nullable<long> PREV_FIRE_TIME
        {
            get;
            set;
        }
    
        public virtual Nullable<int> PRIORITY
        {
            get;
            set;
        }
    
        public virtual string TRIGGER_STATE
        {
            get;
            set;
        }
    
        public virtual string TRIGGER_TYPE
        {
            get;
            set;
        }
    
        public virtual long START_TIME
        {
            get;
            set;
        }
    
        public virtual Nullable<long> END_TIME
        {
            get;
            set;
        }
    
        public virtual string CALENDAR_NAME
        {
            get;
            set;
        }
    
        public virtual Nullable<int> MISFIRE_INSTR
        {
            get;
            set;
        }
    
        public virtual byte[] JOB_DATA
        {
            get;
            set;
        }

        #endregion

        #region Navigation Properties
    
    	
    	public virtual QRTZ_CRON_TRIGGERS QRTZ_CRON_TRIGGERS
        {
            get { return _qRTZ_CRON_TRIGGERS; }
            set
            {
                if (!ReferenceEquals(_qRTZ_CRON_TRIGGERS, value))
                {
                    var previousValue = _qRTZ_CRON_TRIGGERS;
                    _qRTZ_CRON_TRIGGERS = value;
                    FixupQRTZ_CRON_TRIGGERS(previousValue);
                }
            }
        }
        private QRTZ_CRON_TRIGGERS _qRTZ_CRON_TRIGGERS;
    
    		
    	public virtual QRTZ_JOB_DETAILS QRTZ_JOB_DETAILS
        {
            get { return _qRTZ_JOB_DETAILS; }
            set
            {
                if (!ReferenceEquals(_qRTZ_JOB_DETAILS, value))
                {
                    var previousValue = _qRTZ_JOB_DETAILS;
                    _qRTZ_JOB_DETAILS = value;
                    FixupQRTZ_JOB_DETAILS(previousValue);
                }
            }
        }
        private QRTZ_JOB_DETAILS _qRTZ_JOB_DETAILS;
    
    	
    	public virtual QRTZ_SIMPLE_TRIGGERS QRTZ_SIMPLE_TRIGGERS
        {
            get { return _qRTZ_SIMPLE_TRIGGERS; }
            set
            {
                if (!ReferenceEquals(_qRTZ_SIMPLE_TRIGGERS, value))
                {
                    var previousValue = _qRTZ_SIMPLE_TRIGGERS;
                    _qRTZ_SIMPLE_TRIGGERS = value;
                    FixupQRTZ_SIMPLE_TRIGGERS(previousValue);
                }
            }
        }
        private QRTZ_SIMPLE_TRIGGERS _qRTZ_SIMPLE_TRIGGERS;
    
    	
    	public virtual QRTZ_SIMPROP_TRIGGERS QRTZ_SIMPROP_TRIGGERS
        {
            get { return _qRTZ_SIMPROP_TRIGGERS; }
            set
            {
                if (!ReferenceEquals(_qRTZ_SIMPROP_TRIGGERS, value))
                {
                    var previousValue = _qRTZ_SIMPROP_TRIGGERS;
                    _qRTZ_SIMPROP_TRIGGERS = value;
                    FixupQRTZ_SIMPROP_TRIGGERS(previousValue);
                }
            }
        }
        private QRTZ_SIMPROP_TRIGGERS _qRTZ_SIMPROP_TRIGGERS;

        #endregion

        #region Association Fixup
    
        private void FixupQRTZ_CRON_TRIGGERS(QRTZ_CRON_TRIGGERS previousValue)
        {
            if (previousValue != null && ReferenceEquals(previousValue.QRTZ_TRIGGERS, this))
            {
                previousValue.QRTZ_TRIGGERS = null;
            }
    
            if (QRTZ_CRON_TRIGGERS != null)
            {
                QRTZ_CRON_TRIGGERS.QRTZ_TRIGGERS = this;
            }
        }
    
        private void FixupQRTZ_JOB_DETAILS(QRTZ_JOB_DETAILS previousValue)
        {
            if (previousValue != null && previousValue.QRTZ_TRIGGERS.Contains(this))
            {
                previousValue.QRTZ_TRIGGERS.Remove(this);
            }
    
            if (QRTZ_JOB_DETAILS != null)
            {
                if (!QRTZ_JOB_DETAILS.QRTZ_TRIGGERS.Contains(this))
                {
                    QRTZ_JOB_DETAILS.QRTZ_TRIGGERS.Add(this);
                }
                if (SCHED_NAME != QRTZ_JOB_DETAILS.SCHED_NAME)
                {
                    SCHED_NAME = QRTZ_JOB_DETAILS.SCHED_NAME;
                }
                if (JOB_NAME != QRTZ_JOB_DETAILS.JOB_NAME)
                {
                    JOB_NAME = QRTZ_JOB_DETAILS.JOB_NAME;
                }
                if (JOB_GROUP != QRTZ_JOB_DETAILS.JOB_GROUP)
                {
                    JOB_GROUP = QRTZ_JOB_DETAILS.JOB_GROUP;
                }
            }
        }
    
        private void FixupQRTZ_SIMPLE_TRIGGERS(QRTZ_SIMPLE_TRIGGERS previousValue)
        {
            if (previousValue != null && ReferenceEquals(previousValue.QRTZ_TRIGGERS, this))
            {
                previousValue.QRTZ_TRIGGERS = null;
            }
    
            if (QRTZ_SIMPLE_TRIGGERS != null)
            {
                QRTZ_SIMPLE_TRIGGERS.QRTZ_TRIGGERS = this;
            }
        }
    
        private void FixupQRTZ_SIMPROP_TRIGGERS(QRTZ_SIMPROP_TRIGGERS previousValue)
        {
            if (previousValue != null && ReferenceEquals(previousValue.QRTZ_TRIGGERS, this))
            {
                previousValue.QRTZ_TRIGGERS = null;
            }
    
            if (QRTZ_SIMPROP_TRIGGERS != null)
            {
                QRTZ_SIMPROP_TRIGGERS.QRTZ_TRIGGERS = this;
            }
        }

        #endregion

    }
}