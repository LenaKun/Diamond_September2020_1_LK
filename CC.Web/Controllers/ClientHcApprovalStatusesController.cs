using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CC.Web.Models;
using CC.Data;

namespace CC.Web.Controllers
{
	public class ClientHcApprovalStatusesController : PrivateCcControllerBase
	{
		public ActionResult Index(int clientId)
		{
			ViewBag.ShowFundStatusColumn = Permissions.CanSeeProgramField;
			return View(clientId);
		}

		public JsonResult Data(jQueryDataTableParamModel model)
		{
			var ShowFundStatusColumn = Permissions.CanSeeProgramField;

			var q = from c in db.Clients.Where(Permissions.ClientsFilter)

					from a in c.ClientHcStatuses
					where a.ClientId == model.ClientId
					select new
					{
						a.Id,
						a.StartDate,
						HcStatusName = a.HcStatus.Name,
						ApprovalStatusName = a.ApprovalStatus.Name,
						FundStatusName = a.FundStatus.Name
					};


			var result = new jQueryDataTableResult
			{
				sEcho = model.sEcho,
			};
			if (ShowFundStatusColumn)
			{
				result.aaData = q.ToList();
			}
			else
			{
				result.aaData = q.Select(f => new
				{
					f.Id,
					f.StartDate,
					f.HcStatusName,
					f.ApprovalStatusName,
					FundStatusName = (string)null
				}).ToList();				
			}
			result.iTotalRecords = result.iTotalDisplayRecords = q.Count();
			return this.MyJsonResult(result);
		}

		[HttpPost]
		[CcAuthorize(FixedRoles.Admin)]
		public ActionResult Update(Models.EditableDataTableUpdateModel input)
		{

			var item = db.ClientHcStatuses.FirstOrDefault(f => f.Id == input.id);
			var startDate = input.value.Parse<DateTime>();
			if (item != null)
			{
				if (startDate.HasValue)
				{
					var duplicate = db.ClientHcStatuses.FirstOrDefault(f => f.ClientId == item.ClientId && f.StartDate == startDate);
					if (duplicate == null)
					{
						db.Histories.AddObject(new History
						{
							TableName = "Clients",
							FieldName = "Homecare Approval Status Date",
							OldValue = item.StartDate.ToShortDateString(),
							NewValue = startDate.Value.ToShortDateString(),
							ReferenceId = item.ClientId,
							UpdatedBy = Permissions.User.Id,
							UpdateDate = DateTime.Now
						});
						if (startDate.Value.Date < DateTime.Now.Date)
						{
							item.StartDate = startDate.Value;
							db.SaveChanges();
						}
						else
						{
							Response.StatusCode = 400;
							ModelState.AddModelError(string.Empty, "The approval status date can only be revised to any date in the past (excluding today).");
						}
					}
					else
					{
						Response.StatusCode = 400;
						ModelState.AddModelError(string.Empty, "Duplicate entry");
					}

				}
				else
				{
					Response.StatusCode = 400;
					ModelState.AddModelError(string.Empty, "Emplty Start Date");
				}
			}
			else
			{
				Response.StatusCode = 404;
				ModelState.AddModelError(string.Empty, "Entry not found");
			}

			if (ModelState.IsValid)
			{
				return Content("Success");
			}
			else
			{
				return Content(ModelState.ValidationErrorMessages().First());
			}
		}
	}
}