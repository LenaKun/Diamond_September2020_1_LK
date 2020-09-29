using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CC.Web.Controllers
{
    public class FundStatusesController : PrivateCcControllerBase
    {
        //
        // GET: /FundStatuses/

        public ActionResult Index(string term)
        {
			if (string.IsNullOrEmpty(term)) { term = null; }

			return this.MyJsonResult(new
			{
				results = db.FundStatuses.Select(f => new { id = f.Id, text = f.Name })
				.Where(f => term == null || f.text.Contains(term))
				.OrderBy(f => f.text)
			});
        }

    }
}
