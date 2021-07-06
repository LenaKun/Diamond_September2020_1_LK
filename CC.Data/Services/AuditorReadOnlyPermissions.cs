using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace CC.Data.Services
{
    class AuditorReadOnlyPermissions : GlobalReadOnlyPermissions
    {
        public AuditorReadOnlyPermissions(User user) : base(user) { }

        public override Expression<Func<Daf, bool>> DafFilter
        {
            get
            {
                return f => true;
            }
        }
    }

   
}
