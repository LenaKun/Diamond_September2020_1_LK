using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CC.Data.Repositories
{
    public class EligibilityPeriodsRepository : Repository<FunctionalityScore>
    {
        //depends on the following:

        public EligibilityPeriodsRepository() { }

        public EligibilityPeriodsRepository(ccEntities objectContext) : base(objectContext) { }

        public EligibilityPeriodsRepository(ccEntities objectContext, CcRepository parent) : base(objectContext, parent) { }

    }
}
