using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CC.Web.Areas.Admin.Controllers
{

	[CcAuthorize(CC.Data.FixedRoles.Admin, CC.Data.FixedRoles.GlobalOfficer, CC.Data.FixedRoles.CfsAdmin)]
    public class AdminControllerBase: CC.Web.Controllers.PrivateCcControllerBase
    {
        //
        // GET: /Admin/AdminControllerBase/
    }
}
