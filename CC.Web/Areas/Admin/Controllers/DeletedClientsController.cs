using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CC.Data;
using CC.Web.Models;
using System.Linq.Expressions;
using System.ComponentModel.DataAnnotations;

namespace CC.Web.Areas.Admin.Controllers
{
    [CcAuthorize(CC.Data.FixedRoles.Admin)]
    public class DeletedClientsController : AdminControllerBase
    {
        //
        // GET: /Admin/DeletedClients/

        public ActionResult Index()
        {
            return View();
        }



        public JsonResult IndexData(jQueryDataTableParamModel input)
        {
            using (var db = new ccEntities())
            {
                var q = db.DeletedClients.Select(f => new
                {
                    Id = f.Id,
                    Name = f.Name,
                    Address = f.Address,
                    BirthDate = f.BirthDate,
                    JoinDate = f.JoinDate,
                    LeaveDate = f.LeaveDate,
                    DeletedAt = f.DeletedAt,
                    UserName = f.User.UserName,
                    DeleteReason = f.DeleteRasonId == (int)Client.DeleteReasons.Duplicate ? "Duplicate" :
                    f.DeleteRasonId == (int)Client.DeleteReasons.Ineligible ? "Ineligible" : "N/A"
                });


                var filtered = q;
                if (!string.IsNullOrWhiteSpace(input.sSearch))
                {
                    foreach (var s in input.sSearch.Split(new char[] { ' ' }).Where(f => !string.IsNullOrWhiteSpace(f)))
                    {
                        filtered = filtered.Where(f => f.Address.Contains(s) || f.Name.Contains(s) || f.UserName.Contains(s) || f.DeleteReason.Contains(s));
                    }
                }

                string sortColName = null;
                switch (input.iSortCol_0)
                {
                    case 0: sortColName = "Id"; break;
                    case 1: sortColName = "Name"; break;
                    case 2: sortColName = "Address"; break;
                    case 3: sortColName = "BirthDate"; break;
                    case 4: sortColName = "JoinDate"; break;
                    case 5: sortColName = "LeaveDate"; break;
                    case 6: sortColName = "DeleteReason"; break;
                    case 7: sortColName = "DeletedAt"; break;
                    case 8: sortColName = "UserName"; break;
                }
                var sorted = filtered.OrderByField(sortColName, input.sSortDir_0 == "asc");

                var aaData = sorted.Skip(input.iDisplayStart).Take(input.iDisplayLength).ToList().Select(f =>
                        new object[]{
							f.Id,
							f.Name,
							f.Address,
							f.BirthDate,
							f.JoinDate,
							f.LeaveDate,
							f.DeleteReason,
							f.DeletedAt,
							f.UserName
						});
                return this.MyJsonResult(new jQueryDataTableResult<object>()
                {
                    sEcho = input.sEcho,
                    iTotalRecords = q.Count(),
                    iTotalDisplayRecords = filtered.Count(),
                    aaData = aaData
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Export()
        {
            var delClients = (from f in db.DeletedClients.ToList()
                              select new DeletedClientsListRow
                             {
                                 Id = f.Id,
                                 Name = f.Name,
                                 Address = f.Address,
                                 BirthDate = f.BirthDate.HasValue ? f.BirthDate.Value.ToString("dd MMM yyyy") : "N/A",
                                 JoinDate = f.JoinDate.HasValue ? f.JoinDate.Value.ToString("dd MMM yyyy") : "N/A",
                                 LeaveDate = f.LeaveDate.HasValue ? f.LeaveDate.Value.ToString("dd MMM yyyy") : "N/A",
                                 DeletedAt = f.DeletedAt.ToString("dd MMM yyyy"),
                                 UserName = f.User.UserName,
                                 DeleteReason = f.DeleteRasonId == (int)Client.DeleteReasons.Duplicate ? "Duplicate" :
                                 f.DeleteRasonId == (int)Client.DeleteReasons.Ineligible ? "Ineligible" : "N/A"
                             });
            return this.Excel("Deleted Clients", "Sheet1", delClients);
        }

        private class DeletedClientsListRow
        {
            [Display(Name = "CCID")]
            public int Id { get; set; }
            [Display(Name = "Name")]
            public string Name { get; set; }
            [Display(Name = "Address")]
            public string Address { get; set; }
            [Display(Name = "Birth Date")]
            public string BirthDate { get; set; }
            [Display(Name = "Join Date")]
            public string JoinDate { get; set; }
            [Display(Name = "Leave Date")]
            public string LeaveDate { get; set; }
            [Display(Name = "Delete Reason")]
            public string DeleteReason { get; set; }
            [Display(Name = "Deleted Date")]
            public string DeletedAt { get; set; }
            [Display(Name = "User")]
            public string UserName { get; set; }
        }
    }

}
