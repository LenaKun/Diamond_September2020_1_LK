using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace CC.Data.Repositories
{
    class HomeCareEntitledPeriodsRepository : Repository<HomeCareEntitledPeriod>
    {
        public HomeCareEntitledPeriodsRepository(ccEntities objectContext)
            : base(objectContext)
        {
        }
        public HomeCareEntitledPeriodsRepository(ccEntities objectContext, CcRepository parent)
            : base(objectContext, parent)
        {
        }

        protected override void OnInserting(IEnumerable<System.Data.Objects.ObjectStateEntry> entries)
        {
            base.OnInserting(entries);
        }

        protected override void OnUpdating(IEnumerable<System.Data.Objects.ObjectStateEntry> entries)
        {
            base.OnUpdating(entries);
        }
    }
}
