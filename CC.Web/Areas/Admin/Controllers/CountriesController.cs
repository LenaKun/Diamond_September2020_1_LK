using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CC.Data;
using System.ComponentModel.DataAnnotations;


namespace CC.Web.Areas.Admin.Controllers
{
    [CcAuthorize(CC.Data.FixedRoles.Admin)]
	public class CountriesController : AdminControllerBase
	{
		public ViewResult Index()
		{
			return View();
		}
		public JsonResult IndexData(CC.Web.Models.jQueryDataTableParamModel p)
		{
			var source = from ag in db.Countries
						 select new
						 {
							 Id = ag.Id,
							 Name = ag.Name,
							 ag.IncomeVerificationRequired,
							 RegionName = ag.Region.Name,
							 Language = ag.Language.Name
						 };
			var sSortCol_0 = Request["mDataProp_" + p.iSortCol_0];
			var bSortAsc_0 = p.sSortDir_0 == "asc";

			var filtered = source;
			if (!string.IsNullOrEmpty(p.sSearch))
			{
				filtered = filtered.Where(f =>
					f.Name.Contains(p.sSearch)
					|| f.RegionName.Contains(p.sSearch)
					|| f.Language.Contains(p.sSearch)
					);
			}

			var data = filtered.OrderByField(sSortCol_0, bSortAsc_0).Skip(p.iDisplayStart).Take(p.iDisplayLength);

			var result = new CC.Web.Models.jQueryDataTableResult()
			{
				aaData = data,
				sEcho = p.sEcho,
				iTotalRecords = source.Count(),
				iTotalDisplayRecords = filtered.Count()
			};

			return this.MyJsonResult(result);
		}

	}
}