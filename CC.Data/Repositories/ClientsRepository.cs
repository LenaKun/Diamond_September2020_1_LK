using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CC.Data;
using System.Data.Objects;

namespace CC.Data.Repositories
{
    public class ClientsRepository : Repository<Client>,IRepository<Client>
    {

        public ClientsRepository(ccEntities objectContext)
            : base(objectContext)
        {
            this._objectContext.ObjectMaterialized += new ObjectMaterializedEventHandler(_objectContext_ObjectMaterialized);
        }
        public ClientsRepository(ccEntities objectContext, CcRepository parent)
            : base(objectContext, parent) { }

        void _objectContext_ObjectMaterialized(object sender, ObjectMaterializedEventArgs e)
        {

        }

        public override IQueryable<Client> Select
        {
            get
            {
                if (this.RepositoryUser != null)
                {

                    var filter = CC.Data.Services.PermissionsFactory.GetPermissionsFor(this.RepositoryUser).ClientsFilter;
                    return base.Select.Where(filter);
                }

                return base.Select;


            }
        }
    }

}
