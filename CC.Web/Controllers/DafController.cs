using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using CC.Data;
using CC.Web.Models;

namespace CC.Web.Controllers
{
	[CC.Web.Attributes.Localization()]
	[CcAuthorize(FixedRoles.DafEvaluator, FixedRoles.DafReviewer, FixedRoles.AgencyUser, FixedRoles.Ser, FixedRoles.AgencyUserAndReviewer, FixedRoles.SerAndReviewer, FixedRoles.AuditorReadOnly, FixedRoles.Admin)]
	public class DafController : PrivateCcControllerBase
	{
		private CC.Data.Repositories.DafRepository _repository;
		private CC.Data.Repositories.DafRepository DafRepository
		{
			get
			{
				if (_repository == null)
				{
					_repository = new CC.Data.Repositories.DafRepository(this.Permissions, this.db);
				}
				return _repository;
			}
		}

		[HttpGet]
		public ActionResult Index(int? id)
		{
			var model = new DafIndexModel(this.User);
			if (id.HasValue)
			{
				model.Filter.ClientId = id;
				model.ClientId = id;
			}
			return View("Index", model);
		}

		[HttpPost]
		[CcAuthorize(FixedRoles.DafEvaluator, FixedRoles.DafReviewer, FixedRoles.AgencyUserAndReviewer, FixedRoles.SerAndReviewer)]
		public ActionResult TakeOffline(int id)
		{
			var daf = DafRepository.Find(id);
			var deviceId = this.GetDeviceId();
			if (string.IsNullOrEmpty(deviceId))
			{
				return this.MyJsonResult(new
				{
					message = "DeviceId is empty"
				}, 400);
			}
            if(daf.Status != Daf.Statuses.Open)
            {
                return this.MyJsonResult(new
                {
                    message = "Only DAFs in status Open can be downloaded"
                }, 400);
            }
			daf.DownloadedAt = DateTime.Now;
			daf.DownloadedBy = this.Permissions.User.Id;
			daf.DownloadedTo = deviceId;
			DafRepository.Update(daf);
			return this.MyJsonResult(daf);
		}
		[HttpGet]
		[CcAuthorize(FixedRoles.DafEvaluator, FixedRoles.DafReviewer, FixedRoles.AgencyUserAndReviewer, FixedRoles.SerAndReviewer)]
		public ActionResult DetailsList()
		{
			var query = from item in DafRepository.Query()
						where item.StatusId == (int)Daf.Statuses.Open
						select item;
			if (User.IsInRole(FixedRoles.DafEvaluator) || User.IsInRole(FixedRoles.SerAndReviewer))
			{
				query = query.Where(item => item.EvaluatorId == Permissions.User.Id);
			}
			var list = DafRepository.Details(query);
			return this.MyJsonResult(list);
		}

		[HttpGet]
       
        public ActionResult IndexData(DafIndexModelFilter model)
		{
			TryUpdateModel(model, "Filter");
			var filter = model;


			var source = from a in this.DafRepository.Query()
						 select new
						 {
							 a.Id,
							 SerId = a.Client.Agency.GroupId,
							 a.Client.AgencyId,
							 a.ClientId,
							 ClientFirstName = a.Client.FirstName,
							 ClientLastName = a.Client.LastName,
							 ClientName = a.Client.FirstName + " " + a.Client.LastName,
							 a.StatusId,
							 StatusName = a.StatusId == (int)Daf.Statuses.Open ? Resources.Resource.DafStatusOpen :
								a.StatusId == (int)Daf.Statuses.EvaluatorSigned ? Resources.Resource.DafStatusSigned :
								a.StatusId == (int)Daf.Statuses.Completed ? Resources.Resource.DafStatusCompleted :
								(string)null,
							 a.CreatedAt,
							 a.UpdateAt,
							 EvaluatorName = a.Evaluator.FirstName + " " + a.Evaluator.LastName
						 };
			var filtered = from a in source
						   where filter.SerId == null || filter.SerId == a.SerId
						   where filter.AgencyId == null || filter.AgencyId == a.AgencyId
						   where filter.DafId == null  || a.Id == filter.DafId
						   where filter.ClientId == null || a.ClientId == filter.ClientId
						   where filter.FirstName == null || a.ClientFirstName.Trim() == filter.FirstName.Trim()
						   where filter.LastName == null || a.ClientLastName.Trim() == filter.LastName.Trim()
						   where filter.Status == null || a.StatusId == (int?)filter.Status
						   where filter.Search == null
						   || (a.ClientName).Contains(filter.Search)
						   || (a.EvaluatorName).Contains(filter.Search)
						   select new DafIndexRowModel
						   {
							   DafId = a.Id,
							   ClientId = a.ClientId,
							   ClientName = a.ClientName,
							   StatusName = a.StatusName,
							   Created = a.CreatedAt,
							   Updated = a.UpdateAt,
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
			var source = DafRepository.QueryHistory(id);
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
		[CcAuthorize(Roles = FixedRoles.DafEvaluator | FixedRoles.Ser | FixedRoles.SerAndReviewer | FixedRoles.Admin, Message = "Only DAF Evaluator may create a new DAF.")]
		public ActionResult Create(int clientId)
		{
			ViewBag.CurrentUserId = Permissions.User.Id;
			ViewBag.IsEvaluator = User.IsInRole(FixedRoles.DafEvaluator) || User.IsInRole(FixedRoles.Ser) || User.IsInRole(FixedRoles.SerAndReviewer);
			var rawData = (from c in db.Clients.Where(Permissions.ClientsFilter)
						   select new
						   {
							   ClientId = c.Id,
							   ClientFirstName = c.FirstName,
							   ClientLastName = c.LastName,
							   AgencyName = c.Agency.Name,
							   AgencyId = c.Agency.Id,
							   Culture = c.Agency.AgencyGroup.Culture ?? c.Agency.AgencyGroup.Country.Culture
						   }).FirstOrDefault(f => f.ClientId == clientId);
			if (rawData == null)
			{
				ModelState.AddModelError(string.Empty, Resources.Resource.ClientNotFound);
				return View();
			}
			else
			{
				CC.Web.Attributes.LocalizationAttributeBase.SetCulture(rawData.Culture);
				var model = new DafDetailsModel
				{
					ClientId = rawData.ClientId,
					ClientFirstName = rawData.ClientFirstName,
					ClientLastName = rawData.ClientLastName,
					AgencyId = rawData.AgencyId,
					AgencyName = rawData.AgencyName,
				};
				if (User.IsInRole(FixedRoles.DafEvaluator) || User.IsInRole(FixedRoles.Ser) || User.IsInRole(FixedRoles.SerAndReviewer))
                  

                {
					model.EvaluatorId = Permissions.User.Id;
					model.EvaluatorName = Permissions.User.FirstName + " " + Permissions.User.LastName;
				}
				return View(model);
			}
		}

		[HttpPost]
		[CcAuthorize(FixedRoles.DafEvaluator, FixedRoles.Ser, FixedRoles.SerAndReviewer,  FixedRoles.Admin)]
		public ActionResult Create(DafCreateModel model, HttpPostedFileBase file)
		{
			ViewBag.CurrentUserId = Permissions.User.Id;
			ModelState.Clear();
			var Client = db.Clients.Where(Permissions.ClientsFilter).Select(f => new
			{
				f.Id,
				f.AgencyId,
				f.FirstName,
				f.LastName,
				Culture = f.Agency.AgencyGroup.Culture ?? f.Agency.AgencyGroup.Country.Culture,
				AgencyName = f.Agency.Name
			}).FirstOrDefault(f => f.Id == model.ClientId);

			if (Client == null)
			{
				ModelState.AddModelError(string.Empty, Resources.Resource.ClientNotFound);
			}
			else
			{
				model.AgencyName = Client.AgencyName;
				model.ClientFirstName = Client.FirstName;
				model.ClientLastName = Client.LastName;
				if ((User.IsInRole(FixedRoles.DafEvaluator)) && Permissions.User.AgencyId != Client.AgencyId)
				{
					ModelState.AddModelError(string.Empty, Resources.Resource.NotAllowed);
				}
				CC.Web.Attributes.LocalizationAttributeBase.SetCulture(Client.Culture);
			}
           
            //if (User.IsInRole(FixedRoles.Ser) || User.IsInRole(FixedRoles.SerAndReviewer))
            //{
            //    var evaluator = EvaluatorsSER(Client.AgencyId).FirstOrDefault(f => f.Id == model.EvaluatorId);
            //    if (evaluator == null)
            //    {
            //        ModelState.AddModelError(string.Empty, Resources.Resource.EvaluatorNotFound);
            //    }
            //    else
            //    {
            //        model.EvaluatorName = evaluator.FirstName + " " + evaluator.LastName;
            //    }
            //    var AgencyId = evaluator.AgencyGroup.Agencies.First().Id;
            //    model.AgencyId = AgencyId;
            //    if (evaluator != null && Client != null && Client.AgencyId != AgencyId)
            //    {
            //        ModelState.AddModelError(string.Empty, Resources.Resource.AgencyMismatch);
            //    }
            //}
            //else
            //{
                var evaluator = Evaluators(Client.AgencyId).FirstOrDefault(f => f.Id == model.EvaluatorId);
                if (evaluator == null)
                {
                    ModelState.AddModelError(string.Empty, Resources.Resource.EvaluatorNotFound);
                }
                else
                {
                    model.EvaluatorName = evaluator.FirstName + " " + evaluator.LastName;
                }
                if (evaluator != null && Client != null && Client.AgencyId != evaluator.AgencyId)
                {
                    ModelState.AddModelError(string.Empty, Resources.Resource.AgencyMismatch);
                }
           // }
			
			//if (evaluator == null)
			//{
			//	ModelState.AddModelError(string.Empty, Resources.Resource.EvaluatorNotFound);
			//}
			//else
			//{
			//	model.EvaluatorName = evaluator.FirstName + " " + evaluator.LastName;
			//}
			//if (evaluator != null && Client != null && Client.AgencyId != evaluator.AgencyId)
			//{
			//	ModelState.AddModelError(string.Empty, Resources.Resource.AgencyMismatch);
			//}

			if (User.IsInRole(FixedRoles.DafEvaluator) || User.IsInRole(FixedRoles.Ser) || User.IsInRole(FixedRoles.SerAndReviewer))
			{
				if (model.EvaluatorId != Permissions.User.Id)
				{
					if (file == null)
					{
						ModelState.AddModelError("FileName", Resources.Resource.FileIsRequired);
					}
					if (!model.Disclaimer)
					{
						ModelState.AddModelError("Disclaimer", Resources.Resource.DafCreateDisclaimerRequired);
					}
				}
			}


			if (ModelState.IsValid)
			{
				var daf = new Daf()
				{
					EvaluatorId = model.EvaluatorId.Value,
					ClientId = model.ClientId,
					Disclaimer = model.Disclaimer
				};
				if (file != null)
				{
					daf.FileName = file.FileName;
				}
				try
				{
					using (var tr = new System.Transactions.TransactionScope())
					{
						DafRepository.Add(daf);
						if (file != null)
						{
							SaveFile(daf.Id, file);
						}
						tr.Complete();
					}
				}
				catch (Exception ex)
				{
					ModelState.AddModelError(string.Empty, ex.Message);
				}
				if (ModelState.IsValid)
				{
					return RedirectToAction("Details", new { id = daf.Id });
				}
			}
			return View(model);
		}

		[HttpGet]
		public ActionResult Details(int id)
		{
			ViewBag.CurrentUserId = Permissions.User.Id;
			var model = DafRepository.Find(id);
			if (model == null)
			{
				return this.RedirectToAction("Index");
			}
			TryValidateModel(model);
			model.UploadedFile = model.FileName;
			ViewBag.IsValid = ModelState.IsValid;
			ViewBag.CanDelete = Permissions.CanDeleteDaf(model.Status);
			if (model == null)
			{
				throw new HttpException(404, Resources.Resource.NotFound);
			}
			else
			{
				CC.Web.Attributes.LocalizationAttributeBase.SetCulture(model.Culture);
				if (Request.IsJsonRequest())
				{
					return this.MyJsonResult(model);
				}
				else
				{
					return View(model);
				}
			}
		}

		[HttpPost]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.DafEvaluator, FixedRoles.SerAndReviewer, FixedRoles.AuditorReadOnly)]
		[ActionName("Details")]
		public ActionResult DetailsPost(int id, CC.Data.Models.DafDetails postData, HttpPostedFileBase file)
		{

			ViewBag.CurrentUserId = Permissions.User.Id;
			ModelState.Clear();

			var model = DafRepository.Find(id);
			if (model == null)
			{
				return this.RedirectToAction("Index");
			}
			else
			{
				CC.Web.Attributes.LocalizationAttributeBase.SetCulture(model.Culture);

				if (postData.Questions.Any(f => !f.SelectedAnswerId.HasValue) && !User.IsInRole(FixedRoles.Admin))
				{
					ModelState.AddModelError(string.Empty, Resources.Resource.FormNotFullyFilled);
				}

				if (model.Status != Daf.Statuses.Open)
				{
					ModelState.AddModelError(string.Empty, Resources.Resource.DafIsNotEditableInCurrentStatus);
				}

				if (Request.IsJsonRequest())
				{
					if (model.DownloadedTo != null)
					{

						var deviceId = this.GetDeviceId();
						if (string.Equals(model.DownloadedTo, deviceId))
						{
							model.UploadedAt = DateTime.Now;
							model.UploadedBy = this.Permissions.User.Id;
							model.UploadedTo = this.GetDeviceId();
							model.DownloadedAt = null;
							model.DownloadedBy = null;
							model.DownloadedTo = null;
						}
						else
						{
							ModelState.AddModelError(string.Empty, Resources.Resource.DafTakenOfflineOnAnotherDevice);
						}
					}
				}

				if (model.EvaluatorId == default(int) && !User.IsInRole(FixedRoles.Admin))
				{
					ModelState.AddModelError(string.Empty, Resources.Resource.OnlyAdminIsAllowedToModifyEvaluatorField);
				}
				if (User.IsInRole(FixedRoles.DafEvaluator))
				{
					if (postData.EvaluatorId != Permissions.User.Id)
					{
						if (file == null && (string.IsNullOrEmpty(postData.UploadedFile) || model.EvaluatorId != postData.EvaluatorId))
						{
							ModelState.AddModelError(string.Empty, Resources.Resource.FileIsRequired);
						}
						if (!postData.Disclaimer)
						{
							model.Disclaimer = postData.Disclaimer;
							ModelState.AddModelError(string.Empty, Resources.Resource.DafCreateDisclaimerRequired);
						}
					}
				}
				if ((User.IsInRole(FixedRoles.Admin) || User.IsInRole(FixedRoles.DafEvaluator)) && postData.EvaluatorId != default(int))
				{
					if (model.Status != Daf.Statuses.Open)
					{
						ModelState.AddModelError(string.Empty, Resources.Resource.EvaluatorCantBeChangedWhenDafIsNotOpen);
					}
					else
					{
						model.EvaluatorId = postData.EvaluatorId;
						var evaluator = db.Users.SingleOrDefault(f => f.Id == model.EvaluatorId);
						if(evaluator != null)
						{
							model.EvaluatorName = (evaluator.FirstName + " " + evaluator.LastName).NullIfEmptyOrWhiteSpace() ?? evaluator.UserName;
						}
					}
				}				
				if (User.IsInRole(FixedRoles.DafEvaluator) && model.AgencyId != Permissions.User.AgencyId)
				{
					throw new HttpException(403, Resources.Resource.NotAllowed);
				}

				if (postData.AssessmentDate.HasValue)
				{
					if ((postData.AssessmentDate.Value.Date > model.CreateDate.Date || postData.AssessmentDate.Value.Date < model.CreateDate.Date.AddDays(-14)) && (!User.IsInRole(FixedRoles.Admin) || postData.Status != Daf.Statuses.Open))
					{
						ModelState.AddModelError(string.Empty, Resources.Resource.AssessmentOutOfRange);
					}
				}

				if (postData.EffectiveDate.Date > DateTime.Today && (!User.IsInRole(FixedRoles.Admin) || postData.Status != Daf.Statuses.Open) || postData.EffectiveDate < postData.AssessmentDate)
				{
					ModelState.AddModelError(string.Empty, Resources.Resource.EffectiveOutOfRange);
				}

				if (User.IsInRole(FixedRoles.Admin))
				{
					var x = /*model.GovernmentHours != postData.GovernmentHours ||
						model.ExceptionalHours != postData.ExceptionalHours ||*/
						!model.Questions.Select(f => f.SelectedAnswerId).SequenceEqual(postData.Questions.Select(f => f.SelectedAnswerId));
					if (x)
					{
						ModelState.AddModelError(string.Empty, Resources.Resource.DafEditIsNotAllowed);
					}

				}

				if (User.IsInRole(FixedRoles.DafEvaluator) && model.Status == Daf.Statuses.Open)
				{
					foreach (var q in model.Questions)
					{
						q.SelectedAnswerId = postData.Questions.Where(f => f.Id == q.Id).Select(f => f.SelectedAnswerId).FirstOrDefault();
					}
					if (file != null)
					{
						model.FileName = file.FileName;
						SaveFile(model.Id, file);
					}

				}
				if (Request.IsJsonRequest())
				{
					var missingAnswers = model.Questions.Where(f => f.SelectedAnswerId == null);
					if (missingAnswers.Any())
					{
						ModelState.AddModelError("", "Please fill in Answers: " + string.Join(", ", missingAnswers.Select(f => f.Text).Take(3)));
					}
				}

				model.AssessmentDate = postData.AssessmentDate;
				if (Request.IsJsonRequest())
				{
					model.EffectiveDate = postData.EffectiveDate;
                    model.UserConsentObtainedAt = postData.UserConsentObtainedAt;
				}
				//model.GovernmentHours = postData.GovernmentHours;
				//model.ExceptionalHours = postData.ExceptionalHours;
				model.EvaluatorPosition = postData.EvaluatorPosition;
				model.Comments = postData.Comments;

				TryValidateModel(model);
				if (ModelState.IsValid)
				{
					DafRepository.Update(model);
				}
			}

			if (Request.IsJsonRequest())
			{
				if (this.ModelState.IsValid)
				{
					return this.MyJsonResult(model);
				}
				else
				{
					return this.MyJsonResult(new
					{
						errors = this.ModelState.ValidationErrorMessages()
					}, 400);
				}
			}
			else
			{
				ViewBag.IsValid = ModelState.IsValid;
				ViewBag.CanDelete = Permissions.CanDeleteDaf(model.Status);
				return View(model);
			}

		}
		private IQueryable<User> Evaluators(int agencyId)
		{

           
			IQueryable<CC.Data.User> users = from u in db.Users
                                            // from ag in db.Agencies 
											 where u.AgencyId == agencyId 
                                             where u.RoleId == (int)(FixedRoles.DafEvaluator)
                                            where !u.Disabled && u.MembershipUser.IsApproved
											 select u;
			return users;
		}

        //private IQueryable<User> EvaluatorsSER(int agencyId)
        //{


        //    IQueryable<CC.Data.User> users = from u in db.Users
        //                                     from ag in db.Agencies
        //                                     where ag.GroupId == u.AgencyGroupId && ag.Id == agencyId
        //                                     where  u.RoleId == (int) FixedRoles.Ser || u.RoleId == (int) FixedRoles.SerAndReviewer
        //                                     where !u.Disabled && u.MembershipUser.IsApproved
        //                                     select u;
        //    return users; 
        //}
        [HttpGet]
		public ActionResult Evaluators(int agencyId, string term, int page)
		{
			var pageSize = 30;
			var users = Evaluators(agencyId);
			var q = from u in users
					select new
					{
						id = u.Id,
						text = (u.FirstName + " " + u.LastName) ?? u.UserName
					};
			if (!string.IsNullOrEmpty(term))
			{
				q = q.Where(f => f.text.Contains(term));
			}

			var data = q.OrderBy(f => f.text).Skip((page - 1) * pageSize).Take(pageSize + 1).ToList();
			var result = new
			{
				results = data.Take(pageSize),
				more = data.Count > pageSize
			};
			return this.MyJsonResult(result);
		}

       
        [HttpPost]
		[CC.Web.Attributes.ConfirmPassword]
		[CcAuthorize(FixedRoles.DafEvaluator)]
		[CC.Web.Attributes.LocalizationByDafId("id")]
        public ActionResult Sign(int id)
		{
            var dafdetails = DafRepository.Find(id);
            TryValidateModel(dafdetails);
			if (dafdetails.Status != Daf.Statuses.Open)
			{
				ModelState.AddModelError(string.Empty, Resources.Resource.OnlyOpenDafCanBeSigned);
				return View();
			}
            if (dafdetails.Questions.Any(f => !f.SelectedAnswerId.HasValue) && !User.IsInRole(FixedRoles.Admin))
            {
                ModelState.AddModelError(string.Empty, Resources.Resource.FormNotFullyFilled);
            }
            if (ModelState.IsValid)
            {
                var daf = db.Dafs.FirstOrDefault(f => f.Id == dafdetails.Id);
                DafRepository.ChangeStatus(daf, Daf.Statuses.EvaluatorSigned);
                if (Request.IsJsonRequest())
                {
                    db.ContextOptions.LazyLoadingEnabled = false;
                    db.ContextOptions.ProxyCreationEnabled = false;
                    return this.MyJsonResult(daf);
                }
                else
                {
                    return RedirectToAction("Details", new { id = id });
                }
            }
            else
            {
                if (Request.IsJsonRequest())
                {
                    return this.MyJsonResult(ModelState, 400);
                }
                else
                {
                    return View("details", dafdetails);
                }
            }
		}

        [HttpPost]
        [CcAuthorize(FixedRoles.DafEvaluator)]
        [CC.Web.Attributes.LocalizationByDafId("id")]
        public ActionResult RegisterUserConsent(int id)
        {
            var daf = db.Dafs.Where(Permissions.DafFilter).FirstOrDefault(f => f.Id == id);
            if (daf == null)
            {
                return this.MyJsonResult(null, 404);
            }
            if(daf.Status != Daf.Statuses.Open)
            {
                return this.MyJsonResult(null, 400);
            }
            daf.UserConsentObtainedAt = DateTime.Now;
            db.SaveChanges();
            //prevent circular references being serialized
            db.ContextOptions.LazyLoadingEnabled = false;
            db.ContextOptions.ProxyCreationEnabled = false;
            return this.MyJsonResult(daf);
        }

        [HttpPost]
		[CC.Web.Attributes.ConfirmPassword]
		[CcAuthorize(FixedRoles.DafReviewer, FixedRoles.AgencyUserAndReviewer, FixedRoles.SerAndReviewer)]
		[CC.Web.Attributes.LocalizationByDafId("id")]
		public ActionResult Complete(int id)
		{
			var daf = db.Dafs.Where(Permissions.DafFilter).FirstOrDefault(f => f.Id == id);
			if (daf.Status != Daf.Statuses.EvaluatorSigned)
			{
				ModelState.AddModelError(string.Empty, Resources.Resource.OnlySignedDafCanBeCompleted);
				return View();
			}
			DafRepository.ChangeStatus(daf, Daf.Statuses.Completed);

			return RedirectToAction("Details", new { id = id });
		}

		[HttpPost]
		[CC.Web.Attributes.ConfirmPassword]
		[CcAuthorize(FixedRoles.DafReviewer, FixedRoles.AgencyUserAndReviewer, FixedRoles.SerAndReviewer)]
		[CC.Web.Attributes.LocalizationByDafId("id")]
		public ActionResult Reject(int id)
		{
			var daf = db.Dafs.Where(Permissions.DafFilter).FirstOrDefault(f => f.Id == id);
			if (daf.Status != Daf.Statuses.EvaluatorSigned)
			{
				ModelState.AddModelError(string.Empty, Resources.Resource.OnlySignedDafCanBeRejected);
				return View();
			}
			DafRepository.ChangeStatus(daf, Daf.Statuses.Open);
			return RedirectToAction("Details", new { id = id });
		}

		[HttpPost]
		[CcAuthorize(FixedRoles.DafEvaluator, FixedRoles.Admin)]
		[CC.Web.Attributes.LocalizationByDafId("id")]
		public ActionResult Delete(int id)
		{
			var model = DafRepository.Find(id);
			if (model == null)
			{
				throw new HttpException(404, Resources.Resource.NotFound);
			}
			var canDelete = Permissions.CanDeleteDaf(model.Status);
			if (!canDelete)
			{
				throw new HttpException(403, Resources.Resource.NotAllowed);
			}

			DafRepository.Remove(id);

			return RedirectToAction("Index");
		}

		private const string FilesDirectory = "~/App_Data/SignedDaf";
		private string fileAbsolutePath(int id)
		{
			var p1 = VirtualPathUtility.AppendTrailingSlash(FilesDirectory);
			var p2 = VirtualPathUtility.Combine(p1, id.ToString());
			var p3 = Server.MapPath(p2);
			return p3;
		}
		public void SaveFile(int dafId, HttpPostedFileBase file)
		{
			var path = fileAbsolutePath(dafId);
			try
			{
				file.SaveAs(path);
			}
			catch (System.IO.DirectoryNotFoundException)
			{
				var directory = System.IO.Path.GetDirectoryName(path);
				System.IO.Directory.CreateDirectory(directory);
				file.SaveAs(path);
			}
		}
		[HttpGet]
		public FileResult SignedDaf(int id)
		{
			var daf = DafRepository.Find(id);
			if (daf != null)
			{
				var path = fileAbsolutePath(id);
				if (System.IO.File.Exists(path))
				{
					return this.File(path, "application/octet-stream", daf.FileName);
				}
			}

			throw new HttpException(404, "File not found");
		}
	}


}