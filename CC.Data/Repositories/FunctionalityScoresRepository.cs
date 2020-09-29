using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace CC.Data.Repositories
{
    public class FunctionalityScoresRepository : Repository<FunctionalityScore>
    {
        public FunctionalityScoresRepository(ccEntities dataContext) : base(dataContext) { }
        public FunctionalityScoresRepository(ccEntities dataContext, CcRepository parent)
            : base(dataContext, parent)
        {

        }
        public override void Add(FunctionalityScore entity)
        {

            base.Add(entity);
        }
    }
}
