using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CC.Data;

namespace CC.Web.Controllers
{ 
    public class AgenciesController : PrivateCcControllerBase
    {

        public ActionResult Clients(int agencyId)
        {
            if (Request.IsAjaxRequest())
            {
                return this.MyJsonResult(
                    Repo.Clients.Select
                    .Where(f => f.AgencyId == agencyId)
                    .Select(f => new { Id = f.Id, Name = f.FirstName + " " + f.LastName })
                    );
            }
            return new EmptyResult();
        }
    }
}