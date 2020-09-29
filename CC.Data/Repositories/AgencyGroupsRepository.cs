using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CC.Data.Repositories
{
    class AgencyGroupsRepository: Repository<AgencyGroup>
    {
        public AgencyGroupsRepository(ccEntities objectContext)
            : base(objectContext)
        {

        }
        public override IQueryable<AgencyGroup> Select
        {
            get
            {
                var permissions = CC.Data.Services.PermissionsFactory.GetPermissionsFor(this.RepositoryUser);
                return base.Select.Where(permissions.AgencyGroupsFilter);
            }
        }
    }
}
