using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.IO;

namespace CC.Data.Repositories
{
	public class DafRepository : IDisposable
	{
		public Models.DafDetails DAF_v2(string cultureName)
		{
			var z = new[]
			{
/*1*/				new decimal[] {0,1,1,2,3},
/*2*/				new decimal[] {0,	0.75M,	2,	3},
/*3*/				new decimal[] {0,	1,	2},
/*4*/				new decimal[] {0,	1,	2},
/*5*/				new decimal[] {0,	0.5M,	1,	1,	3,	6},
/*6*/				new decimal[] {0, .5M, 1.5M, 3},
/*7*/				new decimal[] {0,.75M,1.5M},
/*8*/				new decimal[] {0,.75M,1,2,3},
/*9*/				new decimal[] {0,.5M,2,3},
/*10*/				new decimal[] {0,1.5M,3},
/*11*/				new decimal[] {0,.75M,1,2,3},
/*12*/				new decimal[] {0,3,6},
/*13*/				new decimal[] {0,1,2},
/*14*/				new decimal[] {0,1,2},
/*15*/				new decimal[] {0,1,2,6,6},
/*16*/				new decimal[] {0,1,3,6},
/*17*/				new decimal[] {0,1,3,6},
/*18*/				new decimal[] {0,1,3,6},
/*19*/				new decimal[] {0,1,2,3},
			};

			return new Models.DafDetails
			{
				Questions = z.Select((v, i) => new Models.DafQuestion
				{
					Id = i + 1,
					Text = global::Resources.Resource.GetString(string.Format("DafQ{0}", i + 1), cultureName),
					Options = v.Select((w, j) => new Models.DafAnswer
					{
						Id = j + 1,
						Score = w,
						Text = global::Resources.Resource.GetString(string.Format("DafQ{0}A{1}", i + 1, j + 1), cultureName),
					}).ToList()
				}).ToList()
			};
		}
		private History getHistoryBase(Daf item, string FieldName, object oldValue, object newValue)
		{
			return new History
									{
										ReferenceId = item.Id,
										TableName = "DAF",
										FieldName = FieldName,
										OldValue = string.Format("{0}", oldValue),
										NewValue = string.Format("{0}", newValue),
										UpdateDate = item.UpdateAt.Value,
										UpdatedBy = item.UpdatedBy.Value,

									};
		}
		private IEnumerable<History> GetQuestionsHistory(Daf item, object oldValue, object newValue)
		{
			var questions = DAF_v2(null).Questions;
			var oldAnswers = ccEntities.Deserialize<List<CC.Data.Models.DafQuestion>>((string)oldValue) ?? new List<CC.Data.Models.DafQuestion>();
			var newAnswers = ccEntities.Deserialize<List<CC.Data.Models.DafQuestion>>((string)newValue) ?? new List<CC.Data.Models.DafQuestion>();
			var a = from q in questions
					let oldAnswerId = oldAnswers.Where(f => f.Id == q.Id).Select(f => f.SelectedAnswerId).FirstOrDefault()
					let newAnswerId = newAnswers.Where(f => f.Id == q.Id).Select(f => f.SelectedAnswerId).FirstOrDefault()
					where oldAnswerId != newAnswerId
					let oldAnserText = q.Options.Where(f => f.Id == oldAnswerId).Select(f => f.Text).FirstOrDefault()
					let newAnserText = q.Options.Where(f => f.Id == newAnswerId).Select(f => f.Text).FirstOrDefault()
					select getHistoryBase(item, q.Text, oldAnserText, newAnserText);
			return a;
		}
		private IEnumerable<History> Changeset(System.Data.Objects.ObjectStateEntry item)
		{

			var changedFields = (from p in item.GetModifiedProperties()
								 let o = item.OriginalValues[p]
								 let c = item.CurrentValues[p]
								 where !object.Equals(o, c)
								 select new
								 {
									 p = p,
									 o = o == DBNull.Value ? null : o,
									 c = c == DBNull.Value ? null : c,
								 }).ToList();
			var entity = (Daf)item.Entity;

			foreach (var change in changedFields)
			{
				switch (change.p)
				{
					case "StatusId":
						yield return getHistoryBase(entity, global::Resources.Resource.DafStatus, ((Daf.Statuses)(int)change.o), ((Daf.Statuses)(int)change.c));
						break;
					case "EvaluatorId":
						yield return getHistoryBase(entity, global::Resources.Resource.Evaluator,
							 db.Users.Where(f => f.Id == (int)change.o).Select(f => f.FirstName + " " + f.LastName).FirstOrDefault(),
							db.Users.Where(f => f.Id == (int)change.c).Select(f => f.FirstName + " " + f.LastName).FirstOrDefault());
						break;
					case "AssessmentDate":
						yield return getHistoryBase(entity, global::Resources.Resource.AssessmentDate, change.o, change.c);
						break;
					case "EffectiveDate":
						yield return getHistoryBase(entity, global::Resources.Resource.EffectiveDate, change.o, change.c);
						break;
					case "GovernmentHours":
						yield return getHistoryBase(entity, global::Resources.Resource.GovernmentHours, change.o, change.c);
						break;
					case "ExceptionalHours":
						yield return getHistoryBase(entity, global::Resources.Resource.ExceptionalHours, change.o, change.c);
						break;
					case "Xml":
						foreach (var h in GetQuestionsHistory(entity, change.o, change.c))
						{
							yield return h;
						}
						break;
					case "TotalScore":
						yield return getHistoryBase(entity, global::Resources.Resource.TotalScore, change.o, change.c);
						break;
					case "FileName":
						yield return getHistoryBase(entity, global::Resources.Resource.SignedDafUpload, change.o, change.c);
						break;

				}
			}
		}


		public DafRepository(Services.IPermissionsBase Permissions, ccEntities db)
		{
			this.Permissions = Permissions;
			this.db = db;
		}

		public ccEntities db { get; private set; }

		public Services.IPermissionsBase Permissions { get; private set; }

		public void ChangeStatus(Daf item, Daf.Statuses newStatus)
		{
			var now = DateTime.Now;
			item.UpdateAt = now;
			item.UpdatedBy = Permissions.User.Id;

			switch (newStatus)
			{
				case Daf.Statuses.Completed:

					item.EffectiveDate = now;
					item.ReviewedAt = now;
					item.ReviewedBy = this.Permissions.User.Id;
					item.FunctionalityScore = new FunctionalityScore
					{
						ClientId = item.ClientId,
						DiagnosticScore = item.TotlaScore.Value,
						StartDate = item.EffectiveDate,
						UpdatedAt = now,
						UpdatedBy = item.UpdatedBy.Value,
						FunctionalityLevelId = db.FunctionalityLevels
							.Where(f => item.TotlaScore >= f.MinScore && item.TotlaScore <= f.MaxScore)
							.Select(f => f.Id)
							.FirstOrDefault()
					};
					break;
				case Daf.Statuses.EvaluatorSigned:
					item.SignedAt = now;
					item.SignedBy = this.Permissions.User.Id;
					break;
			}

			item.Status = newStatus;

			var entry = db.ObjectStateManager.GetObjectStateEntry(item);
			foreach (var h in Changeset(entry))
			{
				db.Histories.AddObject(h);
			}

			db.SaveChanges();
		}

		public Models.DafDetails Find(int id)
		{
			return this.Details(Query().Where(f => f.Id == id)).FirstOrDefault();
		}
		public IEnumerable<Models.DafDetails> Details(IQueryable<Daf> query)
		{
			var items = (from d in query
						select new 
						{
							Id = d.Id,
							ClientId = d.Client.Id,
							ClientFirstName = d.Client.FirstName,
							ClientLastName = d.Client.LastName,
							AgencyName = d.Client.Agency.Name,
							AgencyId = d.Client.Agency.Id,
							EvaluatorId = d.Evaluator.Id,
							EvaluatorName = (d.Evaluator.FirstName + " " + d.Evaluator.LastName) ?? d.Evaluator.UserName,
							AssessmentDate = d.AssessmentDate,
							EffectiveDate = d.EffectiveDate,
							Comments = d.AdditionalComments,
							CreateDate = d.CreatedAt,
							d.UpdateAt,
							EvaluatorPosition = d.EvaluatorPosition,
							ExceptionalHours = d.ExceptionalHours,
							GovernmentHours = d.GovernmentHours,
							ReviewDate = d.ReviewedAt,
							ReviewerName = (d.Reviewer.FirstName + " " + d.Reviewer.LastName) ?? d.Reviewer.UserName,
							SignerName = (d.Signer.FirstName + " " + d.Signer.LastName) ?? d.Signer.UserName,
							SignDate = d.SignedAt,
							TotalScore = d.TotlaScore,
							StatusId = d.StatusId,
							Culture = d.Client.Agency.AgencyGroup.Culture ?? d.Client.Agency.AgencyGroup.Country.Culture,
							RawXml = d.Xml,
							d.DownloadedAt,
							d.DownloadedBy,
							d.DownloadedTo,
                            d.UserConsentObtainedAt,
							DownloaderUsername = d.Downloader.UserName,
							DownloaderFullName = d.Downloader.FirstName + " " + d.Downloader.LastName,
							d.UploadedAt,
							d.UploadedBy,
							d.UploadedTo,
							d.FileName,
							d.Disclaimer
						});
			foreach (var item in items)
			{
				var questions = DAF_v2(item.Culture);
				var answerIds = ccEntities.Deserialize<List<CC.Data.Models.DafQuestion>>(item.RawXml);
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


				var result = new Models.DafDetails()
				{
					Id = item.Id,
					ClientId = item.ClientId,
					ClientFirstName = item.ClientFirstName,
					ClientLastName = item.ClientLastName,
					AgencyName = item.AgencyName,
					AgencyId = item.AgencyId,
					EvaluatorId = item.EvaluatorId,
					EvaluatorName = item.EvaluatorName,
					AssessmentDate = item.AssessmentDate,
					EffectiveDate = item.EffectiveDate,
					Comments = item.Comments,
					CreateDate = item.CreateDate,
					UpdateDate = item.UpdateAt,
					EvaluatorPosition = item.EvaluatorPosition,
					ExceptionalHours = item.ExceptionalHours,
					GovernmentHours = item.GovernmentHours,
					SignerName = item.SignerName,
					SignDate = item.SignDate,
					ReviewDate = item.ReviewDate,
					ReviewerName = item.ReviewerName,
					StatusId = item.StatusId,
					Questions = questions.Questions,
					Culture = item.Culture,
					DownloadedAt = item.DownloadedAt,
					DownloadedBy = item.DownloadedBy,
					DownloadedTo = item.DownloadedTo,
					DownloaderFullName = item.DownloaderFullName,
					DownloaderUsername = item.DownloaderUsername,
                    UserConsentObtainedAt = item.UserConsentObtainedAt,
					UploadedAt = item.UploadedAt,
					UploadedBy = item.UploadedBy,
					UploadedTo = item.UploadedTo,
					FileName = item.FileName,
					Disclaimer = item.Disclaimer
				};
				if (item.TotalScore.HasValue)
				{
					var fl = db.FunctionalityLevels.Where(f => f.MinScore <= item.TotalScore && f.MaxScore >= item.TotalScore)
						.Select(f => new
						{
							f.Name
						}).FirstOrDefault();
					if (fl != null)
					{
						result.FunctionalityLevelName = fl.Name;
					}
				}
				yield return result;
			}
		}

		public IQueryable<Daf> Query()
		{
			var q = db.Dafs.Where(Permissions.DafFilter);
			return q;
		}
		


		public IQueryable<CC.Data.Models.HistoryRowModel> QueryHistory(int entityId)
		{
			var q = from d in this.Query()
					join h in db.Histories on new { TableName = "DAF", Id = d.Id } equals new { TableName = h.TableName, Id = h.ReferenceId }
					where h.ReferenceId == entityId
					where h.TableName == "DAF"
					select new CC.Data.Models.HistoryRowModel
					{

						UpdateDate = h.UpdateDate,
						UpdatedBy = (h.User.FirstName + " " + h.User.LastName) ?? (h.User.UserName),
						FieldName = h.FieldName,
						OldValue = h.OldValue,
						NewValue = h.NewValue
					};
			return q;
		}

		public void Remove(int id)
		{
			var item = this.Query().FirstOrDefault(f => f.Id == id);
			if (item != null)
			{
				RemoveObject(item);
				db.SaveChanges();
			}
		}
		public void RemoveObject(Daf entity)
		{
			var d = sdf(DateTime.Now, this.Permissions.User.Id)(entity);
			if (entity.FunctionalityScoreId.HasValue)
			{
				var fs = new FunctionalityScore { Id = entity.FunctionalityScoreId.Value };
				db.FunctionalityScores.Attach(fs);
				db.FunctionalityScores.DeleteObject(fs);
			}
			db.DafDeleteds.AddObject(d);
			db.Dafs.DeleteObject(entity);
		}
		public static Func<Daf, DafDeleted> sdf(DateTime now, int UserId)
		{
			return d => new DafDeleted
			{
				AgencyId = d.Client.AgencyId,
				AssessmentDate = d.AssessmentDate,
				ClientId = d.ClientId,
				Comments = d.AdditionalComments,
				CreatedAt = d.CreatedAt,
				CreatedBy = d.CreatedBy,
				DeletedAt = now,
				DeletedBy = UserId,
				EffectiveDate = d.EffectiveDate,
				EvaluatorId = d.EvaluatorId,
				ExceptionalHours = d.ExceptionalHours ?? 0,
				GovernmentHours = d.GovernmentHours ?? 0,
				Id = d.Id,
				UpdatedAt = d.UpdateAt,
				UpdatedBy = d.UpdatedBy,
				EvaluatorPosition = d.EvaluatorPosition,
				Xml = d.Xml
			};
		}


		public void Add(Daf item)
		{
			var now = DateTime.Now;
			item.CreatedAt = DateTime.Now;
			item.CreatedBy = Permissions.User.Id;
			db.Dafs.AddObject(item);
			db.SaveChanges();
		}

		public void Update(Daf item)
		{
			item.CreatedAt = DateTime.Now;
			item.CreatedBy = Permissions.User.Id;
			db.SaveChanges();
		}

		public void Dispose()
		{
			if (this.db != null)
			{
				this.db.Dispose();
			}
		}



		public void Update(Models.DafDetails model)
		{
			var daf = this.Query().FirstOrDefault(f => f.Id == model.Id);
			daf.EvaluatorId = model.EvaluatorId;
			daf.AssessmentDate = model.AssessmentDate;
			daf.EffectiveDate = model.EffectiveDate;
			daf.GovernmentHours = model.GovernmentHours;
			daf.ExceptionalHours = model.ExceptionalHours;
			daf.TotlaScore = model.TotalScore ?? 0;
			var selectedAnswers = model.Questions.Select(f => new CC.Data.Models.DafQuestion { Id = f.Id, SelectedAnswerId = f.SelectedAnswerId }).ToList();
			daf.Xml = ccEntities.Serialize<List<CC.Data.Models.DafQuestion>>(selectedAnswers);
			daf.UpdateAt = DateTime.Now;
			daf.UpdatedBy = Permissions.User.Id;
			daf.EvaluatorPosition = model.EvaluatorPosition;
			daf.AdditionalComments = model.Comments;
			daf.DownloadedAt = model.DownloadedAt;
			daf.DownloadedBy = model.DownloadedBy;
			daf.DownloadedTo = model.DownloadedTo;
			daf.UploadedAt = model.UploadedAt;
			daf.UploadedBy = model.UploadedBy;
			daf.UploadedTo = model.UploadedTo;
			daf.FileName = model.FileName;
			daf.Disclaimer = model.Disclaimer;
            daf.UserConsentObtainedAt = model.UserConsentObtainedAt;
			var entry = db.ObjectStateManager.GetObjectStateEntry(daf);
			foreach (var h in Changeset(entry))
			{
				db.Histories.AddObject(h);
			}
			db.SaveChanges();
		}
	}
}
