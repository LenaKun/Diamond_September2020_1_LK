using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CC.Web.Areas.Admin.Controllers
{
    public class HomeController : AdminControllerBase
    {
        //
        // GET: /Admin/Home/

        public ActionResult Index()
        {
            return View();
        }

		public ActionResult asdf()
		{
			var j = new Jobs.ResearchInProgressJob();
			j.Execute(null);
			return new EmptyResult();
		}
		
    }

}
