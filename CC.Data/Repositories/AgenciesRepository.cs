using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CC.Data.Repositories
{
    class AgenciesRepository : Repository<Agency>
    {
        public AgenciesRepository(ccEntities objectContext)
            : base(objectContext)
        {

        }

        public override IQueryable<Agency> Select
        {
            get
            {
                if (this.RepositoryUser != null)
                {
                    var permissions = CC.Data.Services.PermissionsFactory.GetPermissionsFor(this.RepositoryUser);
                    return base.Select.Where(permissions.AgencyFilter);
                }
                else
                {
                    throw new UnauthorizedAccessException();
                }
            }
        }


    }
}
