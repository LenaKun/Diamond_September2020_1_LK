using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CC.Web.Controllers
{
	public class FundsController : PrivateCcControllerBase
	{
		//
		// GET: /Funds/

		public ActionResult Index(string term)
		{
			var data = db.Funds.Where(Permissions.FundsFilter)
				.Select(f => new { id = f.Id, text = f.Name });
				
			if (!string.IsNullOrEmpty(term))
			{
				data = data.Where(f => f.text.Contains(term));
			}


			return this.MyJsonResult(new
			{
				results = data.OrderBy(f => f.text)
			});
		}

	}
}
