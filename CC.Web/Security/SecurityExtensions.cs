using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Principal;

namespace CC.Web
{
    public static class SecurityExtensions
    {
        public static bool IsInRole(this IPrincipal principal, CC.Data.FixedRoles fixedRole)
        {
            return principal.IsInRole(fixedRole.ToString());
        }
    }
}