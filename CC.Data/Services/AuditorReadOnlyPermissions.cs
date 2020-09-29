using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CC.Data.Services
{
    class AuditorReadOnlyPermissions : GlobalReadOnlyPermissions
    {
        public AuditorReadOnlyPermissions(User user) : base(user) { }
    }
}
