using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CC.Web.Models;
using CC.Data;

namespace CC.Web.Controllers
{
	[CcAuthorize(FixedRoles.Admin)]
	public class DeletedDafController : PrivateCcControllerBase
	{
		private IQueryable<CC.Data.DafDeleted> GetData()
		{
			switch ((FixedRoles)this.Permissions.User.RoleId)
			{
				case FixedRoles.Admin:
					return db.DafDeleteds;
				default:
					throw new HttpException(403, Resources.Resource.NotAllowed);
			}
		}
		private IQueryable<DeletedDafDetailsModel> QueryDetails()
		{
			var openStatusName = Resources.Resource.DafStatusOpen;
			var signedStatusName = Resources.Resource.DafStatusSigned;
			var completedStatusName = Resources.Resource.DafStatusCompleted;
			return (from a in GetData()
					join agency in db.Agencies on a.AgencyId equals agency.Id
					join c in db.Clients.Where(Permissions.ClientsFilter) on a.ClientId equals c.Id into g
					from c in g.DefaultIfEmpty()
					join evaluator in db.Users on a.EvaluatorId equals evaluator.Id into g1
					from evaluator in g1.DefaultIfEmpty()
					join deleter in db.Users on a.DeletedBy equals deleter.Id into g2
					from deleter in g2.DefaultIfEmpty()
					join updater in db.Users on a.UpdatedBy equals updater.Id into g3
					from updater in g3.DefaultIfEmpty()
					join creator in db.Users on a.CreatedBy equals creator.Id into g4
					from creator in g4.DefaultIfEmpty()
					join signer in db.Users on a.SignedBy equals signer.Id into g5
					from signer in g5.DefaultIfEmpty()
					join reviewer in db.Users on a.ReviewedBy equals reviewer.Id into g6
					from reviewer in g6.DefaultIfEmpty()
					select new DeletedDafDetailsModel
					{
						AgencyId = a.AgencyId,
						AgencyGroupId = agency.GroupId,
						AgencyName = agency.Name,
						AssessmentDate = a.AssessmentDate,
						ClientFirstName = c.FirstName,
						ClientId = a.ClientId,
						ClientLastName = c.LastName,
						ClientName = c.FirstName + " " + c.LastName,
						Comments = a.Comments,
						CreatedAt = a.CreatedAt,
						CreatedBy = a.CreatedBy,
						CreatedByName = creator.FirstName + " " + creator.LastName,
						DeletedAt = a.DeletedAt,
						DeletedBy = a.DeletedBy,
						DeletedByName = deleter.FirstName + " " + deleter.LastName,
						EffectiveDate = a.EffectiveDate,
						EvaluatorId = a.EvaluatorId,
						EvaluatorName = evaluator.FirstName + " " + evaluator.LastName,
						ExceptionalHours = a.ExceptionalHours,
						GovernmentHours = a.GovernmentHours,
						EvaluatorPosition = a.EvaluatorPosition,
						Id = a.Id,
						StatusId = a.StatusId,
						StatusName = a.StatusId == (int)Daf.Statuses.Open ? openStatusName :
								 a.StatusId == (int)Daf.Statuses.EvaluatorSigned ? signedStatusName :
								 a.StatusId == (int)Daf.Statuses.Completed ? completedStatusName : null,
						UpdatedAt = a.UpdatedAt,
						UpdatedBy = a.UpdatedBy,
						UpdatedByName = updater.FirstName + " " + updater.LastName,
						SignedAt = a.SignedAt,
						SignedBy = a.SignedBy,
						SignedByName = signer.FirstName + " " + signer.LastName,
						ReviewedAt = a.ReviewedAt,
						ReviewedBy = a.ReviewedBy,
						ReviewedByName = reviewer.FirstName + " " + reviewer.LastName,
						Culture = c.Agency.AgencyGroup.Culture ?? c.Agency.AgencyGroup.Country.Culture,
						Xml = a.Xml,
					});
		}
		// GET: DeletedDaf
		public ActionResult Index()
		{
			var model = new DeletedDafIndexModel();
			return View(model);
		}

		[HttpGet]
		public ActionResult IndexData(DeletedDafIndexModel model)
		{
			TryUpdateModel(model, "Filter");
			var filter = model;

			var source = QueryDetails();
			var filtered = from a in source
						   where filter.SerId == null || filter.SerId == a.AgencyGroupId
						   where filter.AgencyId == null || filter.AgencyId == a.AgencyId
						   where filter.ClientId == null || a.ClientId == filter.ClientId
						   where filter.FirstName == null || a.ClientFirstName.Trim() == filter.FirstName.Trim()
						   where filter.LastName == null || a.ClientLastName.Trim() == filter.LastName.Trim()
						   where filter.Status == null || a.StatusId == (int?)filter.Status
						   where filter.DeletedFrom == null || a.DeletedAt >= filter.DeletedFrom
						   where filter.DeletedTo == null || a.DeletedAt < filter.DeletedTo
						   where filter.Search == null
						   || (a.ClientName).Contains(filter.Search)
						   || (a.EvaluatorName).Contains(filter.Search)
						   select new DeletedDafListRowModel
						   {
							   DeletedAt = a.DeletedAt,
							   DafId = a.Id,
							   ClientId = a.ClientId,
							   ClientName = a.ClientName,
							   StatusName = a.StatusName,
							   CreatedAt = a.CreatedAt,
							   UpdatedAt = a.UpdatedAt,
							   Updated = a.UpdatedAt,
							   EvaluatorName = a.EvaluatorName
						   };
			var result = new CC.Web.Models.jQueryDataTableResult()
			{
				aaData = filtered.OrderByField(model.sSortCol_0, model.sSortDir_0 == "asc").Skip(model.iDisplayStart).Take(model.iDisplayLength),
				sEcho = model.sEcho,
				iTotalRecords = source.Count(),
				iTotalDisplayRecords = filtered.Count()
			};

			return this.MyJsonResult(result);

		}

		[HttpGet]
		public ActionResult Audit(int id, HistoryDataTablesModel param)
		{
			var source =
					from h in db.Histories
					where h.ReferenceId == id
					where h.TableName == "DAF"
					select new CC.Data.Models.HistoryRowModel
					{

						UpdateDate = h.UpdateDate,
						UpdatedBy = (h.User.FirstName + " " + h.User.LastName) ?? (h.User.UserName),
						FieldName = h.FieldName,
						OldValue = h.OldValue,
						NewValue = h.NewValue
					};
			var filtered = source;
			if (!string.IsNullOrEmpty(param.FieldName))
			{
				filtered = filtered.Where(f => f.FieldName.Contains(param.FieldName));
			}
			if (param.fromDate.HasValue)
			{
				filtered = filtered.Where(f => f.UpdateDate >= param.fromDate);
			}
			if (param.toDate.HasValue)
			{
				filtered = filtered.Where(f => f.UpdateDate < param.toDate);
			}
			var ordered = filtered.OrderByField(param.sSortCol_0, param.sSortDir_0 == "asc");
			var result = new jQueryDataTableResult
			{
				aaData = ordered.Skip(param.iDisplayStart).Take(param.iDisplayLength),
				iTotalDisplayRecords = filtered.Count(),
				iTotalRecords = source.Count(),
				sEcho = param.sEcho
			};
			return this.MyJsonResult(result);
		}

		[HttpGet]
		public ActionResult Details(int id)
		{
			var item = QueryDetails().FirstOrDefault(f => f.Id == id);
			if (item == null)
			{
				ModelState.AddModelError(string.Empty, Resources.Resource.NotFound);
			}
			else
			{
				CC.Web.Attributes.LocalizationAttributeBase.SetCulture(item.Culture);
				using (var r = new CC.Data.Repositories.DafRepository(this.Permissions, this.db))
				{
					var questions = r.DAF_v2(item.Culture);
					var answerIds = ccEntities.Deserialize<List<CC.Data.Models.DafQuestion>>(item.Xml);
					if (answerIds != null)
					{
						for (var i = 0; i < questions.Questions.Count && i < answerIds.Count; i++)
						{
							questions.Questions[i].SelectedAnswerId = answerIds
								.Where(f => f.Id == questions.Questions[i].Id)
								.Select(f => f.SelectedAnswerId)
								.FirstOrDefault();
						}
					}
					item.Questions = questions.Questions;
				}
			}
			return View(item);
		}
	}
}