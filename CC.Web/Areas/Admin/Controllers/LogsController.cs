using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CC.Web.Models;
using CC.Data;

namespace CC.Web.Areas.Admin.Controllers
{
    [CcAuthorize(CC.Data.FixedRoles.Admin)]
    public class LogsController : AdminControllerBase
    {
        //
        // GET: /Admin/Logs/

        public ActionResult Index()
        {
            return View();
        }

    }
}
