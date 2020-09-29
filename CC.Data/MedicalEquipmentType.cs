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
    public partial class MedicalEquipmentType
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

        #endregion

        #region Navigation Properties
    
        public virtual ICollection<MedicalEquipmentReport> MedicalEquipmentReports
        {
            get
            {
                if (_medicalEquipmentReports == null)
                {
                    var newCollection = new FixupCollection<MedicalEquipmentReport>();
                    newCollection.CollectionChanged += FixupMedicalEquipmentReports;
                    _medicalEquipmentReports = newCollection;
                }
                return _medicalEquipmentReports;
            }
            set
            {
                if (!ReferenceEquals(_medicalEquipmentReports, value))
                {
                    var previousValue = _medicalEquipmentReports as FixupCollection<MedicalEquipmentReport>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupMedicalEquipmentReports;
                    }
                    _medicalEquipmentReports = value;
                    var newValue = value as FixupCollection<MedicalEquipmentReport>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupMedicalEquipmentReports;
                    }
                }
            }
        }
        private ICollection<MedicalEquipmentReport> _medicalEquipmentReports;

        #endregion

        #region Association Fixup
    
        private void FixupMedicalEquipmentReports(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (MedicalEquipmentReport item in e.NewItems)
                {
                    item.MedicalEquipmentType = this;
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (MedicalEquipmentReport item in e.OldItems)
                {
                    if (ReferenceEquals(item.MedicalEquipmentType, this))
                    {
                        item.MedicalEquipmentType = null;
                    }
                }
            }
        }

        #endregion

    }
}
