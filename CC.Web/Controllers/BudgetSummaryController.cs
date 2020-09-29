using CC.Data;
using CC.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CC.Web.Controllers
{
    [CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer, FixedRoles.GlobalReadOnly, FixedRoles.AuditorReadOnly)]
    public class BudgetSummaryController : PrivateCcControllerBase
    {
        //
        // GET: /BudgetSummary/

        public ActionResult Index(BudgetSummaryModel model)
        {
            model.Load(db, Permissions);
            return View(model);
        }

        public ActionResult Data(BudgetSummaryModel model)
        {
            var result = model.GetJqResult(db, Permissions);
            return this.MyJsonResult(result);
        }

        public ActionResult Export(BudgetSummaryModel model)
        {
            var result = model.Data(db, Permissions);
            result = model.Filter(result);
            return this.Excel("output", "data", result.ToList());
        }

    }
}
