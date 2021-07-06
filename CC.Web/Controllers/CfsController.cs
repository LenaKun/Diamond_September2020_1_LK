using CC.Data;
using CC.Web.Controllers.Attributes;
using CC.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.Data.SqlClient;
using MvcContrib;
using MvcContrib.ActionResults;
using System.ComponentModel.DataAnnotations;
using System.Data.Objects.SqlClient;

namespace CC.Web.Controllers
{
	[PasswordExpirationCheckAttribute()]
	public class CfsController : PrivateCcControllerBase
    {
        //
        // GET: /Cfs/

		public ActionResult Default()
		{
			if ((User.IsInRole(FixedRoles.Ser) || User.IsInRole(FixedRoles.SerAndReviewer)) && !CcUser.AgencyGroup.CfsDate.HasValue || 
				(User.IsInRole(FixedRoles.AgencyUser) || User.IsInRole(FixedRoles.AgencyUserAndReviewer)) && !CcUser.Agency.AgencyGroup.CfsDate.HasValue)
			{
				return RedirectToAction("Index", "MainReports");
			}
			var model = new CfsRowDataTableModel();
			int regionId = -1;
			if (User.IsInRole("RegionReadOnly"))
			{
				using (var db = new ccEntities())
				{
					regionId = db.Regions.Where(this.Permissions.RegionsFilter).OrderBy(f => f.Name).ToDictionary(f => f.Id, f => f.Name).Select(f => f.Key).SingleOrDefault();
					model.FilterRegionId = regionId;
				}
			}
			ViewBag.RegionId = regionId;
			return View();
		}

		public JsonResult DefaultData(int? regionId, int? countryId, int? stateId, int? agencyGroupId, int? agencyId, int? clientId, DateTime? from, DateTime? to, jQueryDataTableParamModel model)
		{
			int totalRecords = 0;
			var filtered = GetCfsRows(regionId, countryId, stateId, agencyGroupId, agencyId, clientId, from, to, ref totalRecords);

			if (!string.IsNullOrEmpty(model.sSearch))
			{
				filtered = filtered.Where(f => f.AgencyGroupName.Contains(model.sSearch) || f.CareReceivedVia.Contains(model.sSearch) || f.CommunicationsPreference.Contains(model.sSearch)
								|| f.FirstName.Contains(model.sSearch) || f.LastName.Contains(model.sSearch) || f.SerCurrency.Contains(model.sSearch));
			}

			var sSortCol_0 = Request["mDataProp_" + model.iSortCol_0];
			var bSortAsc_0 = model.sSortDir_0 == "asc";
			var sorted = filtered.OrderByField(sSortCol_0, bSortAsc_0).Skip(model.iDisplayStart).Take(model.iDisplayLength);

			var result = new jQueryDataTableResult
			{
				sEcho = model.sEcho,
				aaData = sorted.ToList(),
				iTotalRecords = totalRecords,
				iTotalDisplayRecords = filtered.Count()
			};
			return this.MyJsonResult(result);
		}

		public ActionResult Export(int? regionId, int? countryId, int? stateId, int? agencyGroupId, int? agencyId, int? clientId, DateTime? from, DateTime? to)
		{
			int totalRecords = 0;
			var q = GetCfsRows(regionId, countryId, stateId, agencyGroupId, agencyId, clientId, from, to, ref totalRecords);
			return this.Excel("CFS", "Sheet1", q.ToList());
		}

		public IQueryable<CfsTableRow> GetCfsRows(int? regionId, int? countryId, int? stateId, int? agencyGroupId, int? agencyId, int? clientId, DateTime? from, DateTime? to, ref int totalRecords)
		{
			var now = DateTime.Now;
			var source = from c in db.CfsRows
						 join client in db.Clients.Where(Permissions.CfsClientsFilter) on c.ClientId equals client.Id
						 where client.Agency.AgencyGroup.CfsDate != null
                          let cfsLevel = ccEntitiesExtensions.CfsLevelForGivenDate(client.Id, now)
                        // let cfsLevel =5

                         let cfsAmount = db.CfsAmounts.FirstOrDefault(f => f.CurrencyId == client.Agency.AgencyGroup.DefaultCurrency && f.Level == cfsLevel && f.Year == now.Year 
                                            && f.Countries.Select(c => c.Id).Contains(client.Agency.AgencyGroup.CountryId))
						 select new
						 {
							 Id = c.Id,
							 ClientId = client.Id,
							 LastName = client.LastName,
							 FirstName = client.FirstName,
							 CfsLevel = cfsLevel,
							 CfsAmount =  cfsAmount != null ? cfsAmount.Amount : 0,
							 SerCurrency = client.Agency.AgencyGroup.DefaultCurrency,
							 StartDate = c.StartDate,
                             EndDate = client.LeaveReasonId != 1 ? (c.EndDate != null ? c.EndDate : client.LeaveDate) : null,//if LeaveReason not "moved away", then if it was cfs end date - then cfs end date or leave date
							 //EndDate = client.LeaveReasonId != 1 ? c.EndDate : null, //Lena_Task32 //c.EndDate, //client.LeaveReasonId != 1 ? c.EndDate : null, //Lena_Task32
                             ClientResponseIsYes = c.ClientResponseIsYes,
							 AgencyOverRide = c.AgencyOverRide,
							 CommunicationsPreference = client.CommunicationsPreference.Name,
							 CareReceivedVia = client.CareReceivingOption.Name,
							 AgencyGroupName = client.Agency.AgencyGroup.Name,
							 RegionId = client.Agency.AgencyGroup.Country.RegionId,
							 CountryId = client.Agency.AgencyGroup.CountryId,
							 States = client.Agency.AgencyGroup.StateId,//,.Country.States,
							 AgencyGroupId = client.Agency.GroupId,
							 AgencyId = client.AgencyId
						 };
			totalRecords = source.Count();

			var filtered = source;
			if (User.IsInRole("RegionReadOnly"))
			{
				regionId = db.Regions.Where(this.Permissions.RegionsFilter).OrderBy(f => f.Name).ToDictionary(f => f.Id, f => f.Name).Select(f => f.Key).SingleOrDefault();
			}
			if(regionId.HasValue)
			{
				filtered = filtered.Where(f => f.RegionId == regionId);
				if(User.IsInRole("RegionReadOnly"))
				{
					totalRecords = filtered.Count();
				}
			}
			if(countryId.HasValue)
			{
				filtered = filtered.Where(f => f.CountryId == countryId);
			}
			if(stateId.HasValue)
			{
				//filtered = filtered.Where(f => f.States.Select(c => c.Id).Contains(stateId.Value));
                filtered =  filtered.Where(f => f.States == stateId);
            }
			if(agencyGroupId.HasValue)
			{
				filtered = filtered.Where(f => f.AgencyGroupId == agencyGroupId);
			}
			if(agencyId.HasValue)
			{
				filtered = filtered.Where(f => f.AgencyId == agencyId);
			}
			if(clientId.HasValue)
			{
				filtered = filtered.Where(f => f.ClientId == clientId);
			}
			if(from.HasValue)
			{
				filtered = filtered.Where(f => f.EndDate == null || f.EndDate > from);
			}
			if(to.HasValue)
			{
				filtered = filtered.Where(f => f.StartDate == null || f.StartDate < to);
			}

			return filtered.Select(f => new CfsTableRow
			{
				Id = f.Id,
				ClientId = f.ClientId,
				LastName = f.LastName,
				FirstName = f.FirstName,
				CfsLevel = f.CfsLevel,
				CfsAmount = f.CfsAmount,
				SerCurrency = f.SerCurrency,
				StartDate = f.StartDate,
				EndDate = f.EndDate,
				ClientResponseIsYes = f.ClientResponseIsYes ? "Yes" : "No",
				AgencyOverRide = f.AgencyOverRide ? "Yes" : "No",
				CommunicationsPreference = f.CommunicationsPreference,
				CareReceivedVia = f.CareReceivedVia,
				AgencyGroupName = f.AgencyGroupName
			});
		}

        public ActionResult Index(int clientId)
        {
			var cfsRows = db.CfsRows.Where(f => f.ClientId == clientId);
			ViewBag.CanAddNew = (cfsRows.Count() == 0 || !cfsRows.Any(f => f.EndDate == null))
									&& db.Clients.Any(f => f.Id == clientId && f.HomeCareEntitledPeriods.Any(h => h.StartDate < DateTime.Now && (h.EndDate == null || DateTime.Now < h.EndDate)));
			return View(clientId);
        }

		public JsonResult Data(jQueryDataTableParamModel model)
		{
            var q = from c in db.CfsRows
                    join client in db.Clients.Where(Permissions.CfsClientsFilter) on c.ClientId equals client.Id
                    where client.Agency.AgencyGroup.CfsDate != null
                    where c.ClientId == model.ClientId
                    select new
                    {
                        Id = c.Id,
                        CreatedAt = c.CreatedAt,
                        ClientResponseIsYes = c.ClientResponseIsYes,
                        StartDate = c.StartDate,
                        EndDate = client.LeaveReasonId != 1 ? (c.EndDate!= null? c.EndDate : client.LeaveDate) : null//if LeaveReason not "moved away", then if it was end date - end date or leave date

                    };


			var result = new jQueryDataTableResult
			{
				sEcho = model.sEcho,
				aaData = q.ToList()
			};
			result.iTotalRecords = result.iTotalDisplayRecords = q.Count();
			return this.MyJsonResult(result);
		}

		[CcAuthorize(FixedRoles.Admin, FixedRoles.CfsAdmin, FixedRoles.AgencyUser, FixedRoles.Ser, FixedRoles.AgencyUserAndReviewer, FixedRoles.SerAndReviewer)]
		public ActionResult Create(int clientId)
		{
			var model = new CfsRow();
			model.ClientId = clientId;
			var clientCareReceivedVia = db.Clients.SingleOrDefault(f => f.Id == clientId).CareReceivedId;
			var privatePay = db.CareReceivingOptions.SingleOrDefault(f => f.Id == 3); //Private Pay Family
			bool isPrivatePay = privatePay != null && privatePay.Id == clientCareReceivedVia;
			ViewBag.IsPrivatePay = isPrivatePay;
			if(isPrivatePay)
			{
				model.ClientResponseIsYes = true;
			}
			return View(model);
		}

		[HttpPost]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.CfsAdmin, FixedRoles.AgencyUser, FixedRoles.Ser, FixedRoles.AgencyUserAndReviewer, FixedRoles.SerAndReviewer)]
		public ActionResult Create(CfsRow model)
		{
			bool isPrivatePay = false;
			if(ModelState.IsValid)
			{
				if(db.CfsRows.Any(f => f.ClientId == model.ClientId && f.EndDate == null))
				{
					ModelState.AddModelError("", "There is an open CFS record for this client. Please close it first");
					return View(model);
				}
				model.CreatedById = Permissions.User.Id;
				if(!model.AgencyOverRide)
				{
					model.OverRideDetails = null;
					model.OverrideAgencyFirstName = null;
					model.OverrideAgencyLastName = null;
					model.OverrideAgencyTitle = null;
				}
				else
				{
					var ids = model.OverRideReasonIds.Split(',').Select(f => int.Parse(f));
					var reasons = db.AgencyOverRideReasons.Where(f => ids.Contains(f.Id));
					foreach (var r in reasons)
					{
						model.AgencyOverRideReasons.Add(r);
					}
				}
				if(!model.EndDate.HasValue)
				{
					model.EndDateReasonId = null;
					model.AgencyRequestorFirstName = null;
					model.AgencyRequestorLastName = null;
					model.AgencyRequestorTitle = null;
					model.EndRequestDate = null;
				}
				if(!string.IsNullOrEmpty(model.CfsAdminRemarks) || model.CfsAdminRejected || !string.IsNullOrEmpty(model.CfsAdminInternalRemarks))
				{
					model.CfsAdminLastUpdate = DateTime.Now;
				}
				var client = db.Clients.Include(f => f.CareReceivingOption).SingleOrDefault(f => f.Id == model.ClientId);
				if(client != null && client.CareReceivingOption != null && client.CareReceivingOption.Id == 3) //Private Pay Family
				{
					isPrivatePay = true;
					if (User.IsInRole(FixedRoles.AgencyUser) || User.IsInRole(FixedRoles.AgencyUserAndReviewer) || User.IsInRole(FixedRoles.Ser) || User.IsInRole(FixedRoles.SerAndReviewer))
					{
						model.ClientResponseIsYes = true;
					}
				}
				db.CfsRows.AddObject(model);
				try
				{
					db.SaveChanges();
					return RedirectToAction("Details", "Clients", new { id = model.ClientId });
				}
				catch (Exception ex)
				{
					var msg = ex.Message;
					if(ex.InnerException != null)
					{
						var inner = ex.InnerException;
						while(inner.InnerException != null)
						{
							inner = inner.InnerException;
						}
						msg = inner.Message;
					}
					ModelState.AddModelError("", msg);
				}
			}
			ViewBag.IsPrivatePay = isPrivatePay;
			return View(model);
		}

		[CcAuthorize(FixedRoles.Admin, FixedRoles.CfsAdmin, FixedRoles.AgencyUser, FixedRoles.Ser, FixedRoles.AgencyUserAndReviewer, FixedRoles.SerAndReviewer)]
		public ActionResult Details(int id)
		{
			var model = db.CfsRows.Include(f => f.AgencyOverRideReasons).SingleOrDefault(f => f.Id == id);
			if(model == null)
			{
				return RedirectToAction("Index", "Clients");
			}
			return View(model);
		}

		[CcAuthorize(FixedRoles.Admin, FixedRoles.CfsAdmin, FixedRoles.AgencyUser, FixedRoles.Ser, FixedRoles.AgencyUserAndReviewer, FixedRoles.SerAndReviewer)]
		public ActionResult Edit(int id)
		{
			var cfs = db.CfsRows.SingleOrDefault(f => f.Id == id);
			if(cfs == null)
			{
				return RedirectToAction("Index", "Clients");
			}
			var clientCareReceivedVia = db.Clients.SingleOrDefault(f => f.Id == cfs.ClientId).CareReceivedId;
			var privatePay = db.CareReceivingOptions.SingleOrDefault(f => f.Id == 3); //Private Pay Family
			ViewBag.IsPrivatePay = privatePay != null && privatePay.Id == clientCareReceivedVia;
			cfs.OverRideReasonIds = string.Join(",", cfs.AgencyOverRideReasons.Select(f => f.Id));
			return View(cfs);
		}

		[CcAuthorize(FixedRoles.Admin, FixedRoles.CfsAdmin, FixedRoles.AgencyUser, FixedRoles.Ser, FixedRoles.AgencyUserAndReviewer, FixedRoles.SerAndReviewer)]
		[HttpPost]
		public ActionResult Edit(CfsRow model)
		{
			var client = db.Clients.Include(f => f.CareReceivingOption).Include(f => f.CfsRows).SingleOrDefault(f => f.Id == model.ClientId);
			bool isPrivatePay = client != null && client.CareReceivingOption != null && client.CareReceivingOption.Id == 3; //Private Pay Family
			ViewBag.IsPrivatePay = isPrivatePay;
			if(ModelState.IsValid)
			{
				var cfs = db.CfsRows.SingleOrDefault(f => f.Id == model.Id);
				if(!model.EndDate.HasValue && cfs.EndDate.HasValue && client.CfsRows.Any(f => f.EndDate == null))
				{
					ModelState.AddModelError("", "Deleting the End Date is not allowed while there is an open CFS record for this client");
					return View(model);
				}
                if (!model.EndDate.HasValue && cfs.EndDate.HasValue && client.CfsRows.Any(f => f.StartDate < cfs.EndDate && (f.EndDate == null || f.EndDate > cfs.EndDate) || f.StartDate >= cfs.EndDate))
                {
                    ModelState.AddModelError("", "Deleting the End Date is not allowed after a new record already was created for later/crossing period");
                    return View(model);
                }
                if (!User.IsInRole(FixedRoles.Admin) && (!model.StartDate.HasValue && cfs.StartDate.HasValue || !model.EndDate.HasValue && cfs.EndDate.HasValue))
				{
					ModelState.AddModelError("", "You are not allowed to delete Start Date/ End Date");
					return View(model);
				}
				if(!User.IsInRole(FixedRoles.Admin) && cfs.StartDate != model.StartDate && model.StartDate < DateTime.Today)
				{
					ModelState.AddModelError("", string.Format("Start date must be greater than or equal to {0}.", DateTime.Today.ToShortDateString()));
					return View(model);
				}
				if (!User.IsInRole(FixedRoles.Admin) && cfs.EndDate != model.EndDate && model.EndDate < DateTime.Today)
				{
					ModelState.AddModelError("", string.Format("End date must be greater than or equal to {0}.", DateTime.Today.ToShortDateString()));
					return View(model);
				}
				ApplyValues(cfs, model);
				try
				{
					db.SaveChanges();
					return RedirectToAction("Details", "Clients", new { id = model.ClientId });
				}
				catch (Exception ex)
				{
					var msg = ex.Message;
					if (ex.InnerException != null)
					{
						var inner = ex.InnerException;
						while (inner.InnerException != null)
						{
							inner = inner.InnerException;
						}
						msg = inner.Message;
					}
					ModelState.AddModelError("", msg);
				}
			}
			return View(model);
		}

		[CcAuthorize(FixedRoles.Admin, FixedRoles.CfsAdmin, FixedRoles.AgencyUser, FixedRoles.Ser, FixedRoles.AgencyUserAndReviewer, FixedRoles.SerAndReviewer)]
		private void ApplyValues(CfsRow existing, CfsRow updated)
		{
			if (!existing.CfsApproved.HasValue || User.IsInRole(FixedRoles.Admin) || User.IsInRole(FixedRoles.CfsAdmin))
			{
				existing.UpdatedById = Permissions.User.Id;
				existing.UpdatedAt = DateTime.Now;
				existing.ClientResponseIsYes = updated.ClientResponseIsYes;
				existing.AgencyOverRide = updated.AgencyOverRide;
				if (!updated.AgencyOverRide)
				{
					existing.OverRideDetails = null;
					existing.OverrideAgencyFirstName = null;
					existing.OverrideAgencyLastName = null;
					existing.OverrideAgencyTitle = null;
				}
				else
				{
					var ids = updated.OverRideReasonIds.Split(',').Select(f => int.Parse(f));
					var reasons = db.AgencyOverRideReasons.Where(f => ids.Contains(f.Id));
					foreach (var r in reasons)
					{
						existing.AgencyOverRideReasons.Add(r);
					}
					existing.OverRideDetails = updated.OverRideDetails;
					existing.OverrideAgencyFirstName = updated.OverrideAgencyFirstName;
					existing.OverrideAgencyLastName = updated.OverrideAgencyLastName;
					existing.OverrideAgencyTitle = updated.OverrideAgencyTitle;
				}
			}
			if (User.IsInRole(FixedRoles.Admin) || User.IsInRole(FixedRoles.CfsAdmin))
			{
				existing.CfsApproved = updated.CfsApproved;
				existing.StartDate = updated.StartDate;
				existing.EndDate = updated.EndDate;
				if (!updated.EndDate.HasValue)
				{
					existing.EndDateReasonId = null;
					existing.AgencyRequestorFirstName = null;
					existing.AgencyRequestorLastName = null;
					existing.AgencyRequestorTitle = null;
					existing.EndRequestDate = null;
				}
				else
				{
					existing.EndDateReasonId = updated.EndDateReasonId;
					existing.AgencyRequestorFirstName = updated.AgencyRequestorFirstName;
					existing.AgencyRequestorLastName = updated.AgencyRequestorLastName;
					existing.AgencyRequestorTitle = updated.AgencyRequestorTitle;
					existing.EndRequestDate = updated.EndRequestDate;
				}
			}
			if(User.IsInRole(FixedRoles.CfsAdmin))
			{
				if (existing.CfsAdminRemarks != updated.CfsAdminRemarks || existing.CfsAdminRejected != updated.CfsAdminRejected
					|| existing.CfsAdminInternalRemarks != updated.CfsAdminInternalRemarks)
				{
					existing.CfsAdminRemarks = updated.CfsAdminRemarks;
					existing.CfsAdminRejected = updated.CfsAdminRejected;
					existing.CfsAdminInternalRemarks = updated.CfsAdminInternalRemarks;
					existing.CfsAdminLastUpdate = DateTime.Now;
				}
			}
		}

		[CcAuthorize(FixedRoles.Admin, FixedRoles.CfsAdmin)]
		public ActionResult CfsWaitingApproval()
		{
			return View();
		}

		[CcAuthorize(FixedRoles.Admin, FixedRoles.CfsAdmin)]
		public JsonResult CfsWaitingApprovalData(int? clientId, jQueryDataTableParamModel model)
		{
			var now = DateTime.Now;
			var q = from cfs in db.CfsRows
					where (cfs.Client.LeaveDate == null || cfs.Client.LeaveDate > now) && cfs.CfsApproved == null
					where !cfs.CfsAdminRejected
					select new
					{
						ClientId = cfs.ClientId,
						LastName = cfs.Client.LastName,
						FirstName = cfs.Client.FirstName,
						AgencyName = cfs.Client.Agency.Name,
						CreateDate = cfs.CreatedAt,
						ClientResponseIsYes = cfs.ClientResponseIsYes ? "Yes" : "No",
						CfsAdminRemarks = cfs.CfsAdminRemarks,
						CfsAdminInternalRemarks = cfs.CfsAdminInternalRemarks,
						CfsAdminLastUpdate = cfs.CfsAdminLastUpdate
					};

			var sSortCol_0 = Request["mDataProp_" + model.iSortCol_0];
			var bSortAsc_0 = model.sSortDir_0 == "asc";
			var filtered = q;
			if(clientId.HasValue)
			{
				filtered = filtered.Where(f => f.ClientId == clientId);
			}
			if(!string.IsNullOrEmpty(model.sSearch))
			{
				filtered = filtered.Where(f => f.AgencyName.Contains(model.sSearch) || f.CfsAdminInternalRemarks.Contains(model.sSearch) || f.CfsAdminRemarks.Contains(model.sSearch)
							|| f.FirstName.Contains(model.sSearch) || f.LastName.Contains(model.sSearch));
			}
			var sorted = filtered.OrderByField(sSortCol_0, bSortAsc_0).Skip(model.iDisplayStart).Take(model.iDisplayLength);

			var result = new jQueryDataTableResult
			{
				sEcho = model.sEcho,
				aaData = sorted.ToList(),
				iTotalRecords = q.Count(),
				iTotalDisplayRecords = filtered.Count()
			};
			return this.MyJsonResult(result);
		}

		#region import
		[CcAuthorize(FixedRoles.Admin, FixedRoles.CfsAdmin)]
		public ActionResult Upload()
		{
			var csvColumnNames = CsvHelper.CsvHelperExtenstions.ColumnHeaderNames<ImportCfsCsvMap>();
			return View(csvColumnNames);
		}
		[HttpPost]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.CfsAdmin)]
		public ActionResult Upload(HttpPostedFileWrapper file)
		{
			if (file == null)
			{
				ModelState.AddModelError(string.Empty, "Please select a file");
				var csvColumnNames = CsvHelper.CsvHelperExtenstions.ColumnHeaderNames<ImportCfsCsvMap>();
				return View(csvColumnNames);
			}

			var ImportId = Guid.NewGuid();

			string fileName = System.IO.Path.Combine(System.IO.Path.GetTempPath(), ImportId.ToString());

			var csvConf = new CsvHelper.Configuration.CsvConfiguration()
			{
				IsStrictMode = false,
				IsCaseSensitive = false,
				SkipEmptyRecords = true,
			};

			csvConf.ClassMapping<ImportCfsCsvMap>();


			var updatedAt = DateTime.Now;
			var updatedBy = this.Permissions.User.Id;
			using (var db = new ccEntities())
			{
				db.Imports.AddObject(new Import()
				{
					Id = ImportId,
					StartedAt = updatedAt,
					UserId = this.Permissions.User.Id
				});
				db.SaveChanges();
			}

			using (var csvReader = new CsvHelper.CsvReader(new System.IO.StreamReader(file.InputStream), csvConf))
			{
				var csvChunkSize = 10000;
				var recordIndex = 1;
				foreach (var csvChunk in csvReader.GetRecords<ImportHcep>().Split(csvChunkSize))
				{
					string connectionString = ConnectionStringHelper.GetProviderConnectionString();

					using (var sqlBulk = new System.Data.SqlClient.SqlBulkCopy(connectionString))
					{
						foreach (var record in csvChunk)
						{
							record.RowIndex = recordIndex++;
							record.ImportId = ImportId;
							record.UpdatedAt = updatedAt;
							record.UpdatedBy = updatedBy;
						}


						sqlBulk.DestinationTableName = "ImportHcep";

						sqlBulk.ColumnMappings.Add("ImportId", "ImportId");
						sqlBulk.ColumnMappings.Add("RowIndex", "RowIndex");
						sqlBulk.ColumnMappings.Add("Id", "Id");
						sqlBulk.ColumnMappings.Add("ClientId", "ClientId");
						sqlBulk.ColumnMappings.Add("StartDate", "StartDate");
						sqlBulk.ColumnMappings.Add("CfsApproved", "CfsApproved");
						sqlBulk.ColumnMappings.Add("UpdatedAt", "UpdatedAt");
						sqlBulk.ColumnMappings.Add("UpdatedBy", "UpdatedBy");

						var reader = new CC.Web.Helpers.IEnumerableDataReader<ImportHcep>(csvChunk);
						sqlBulk.WriteToServer(reader);
					}
				}
			}


			return this.RedirectToAction(f => f.Preview(ImportId));
		}

		[CcAuthorize(FixedRoles.Admin, FixedRoles.CfsAdmin)]
		public ActionResult Preview(Guid id)
		{
			CheckImportIdPermissions(id);

			using (var db = new ccEntities())
			{

				var CfsRows = db.CfsRows;
				var ImportHceps = db.ImportHceps;
				var today = DateTime.Today;
				var isAdmin = User.IsInRole(FixedRoles.Admin);
				var q = from i in ImportHceps
						where i.ImportId == id
						join c in db.Clients.Where(this.Permissions.ClientsFilter) on i.ClientId equals c.Id into cG
						from c in cG.DefaultIfEmpty()
						join ol in CfsRows on i.ClientId equals ol.ClientId into olg
						join sol in ImportHceps.Where(f => f.ImportId == id) on i.ClientId equals sol.ClientId into solg
						let cfs = db.CfsRows.Where(f => f.ClientId == c.Id)
						let cfsWithoutStartDate = cfs.Where(f => f.StartDate == null)
						let allCfsWithStartDate = cfs.Count() > 1 && cfs.All(f => f.StartDate != null)
						where
									c == null ||

									(i.ClientId == null) ||
									(i.StartDate == null) ||
									(!isAdmin && i.StartDate < today) ||
									(cfsWithoutStartDate.Count() > 1) ||
									(allCfsWithStartDate) ||
									(cfs.Count() == 0) ||
									(cfsWithoutStartDate.Count() == 1 && cfsWithoutStartDate.FirstOrDefault().EndDate != null && cfsWithoutStartDate.FirstOrDefault().EndDate <= i.StartDate)

						select i

					;
				if (q.Count() > 0)
				{
					ModelState.AddModelError(string.Empty, "One or more rows contain errors.");
				}
			}

			return View(id);
		}
		private void CheckImportIdPermissions(Guid id)
		{
			using (var db = new ccEntities())
			{
				var import = db.Imports.Where(this.Permissions.ImportsFilter).Where(f => f.Id == id).SingleOrDefault();
				if (import == null)
				{
					throw new InvalidOperationException("Import data not found.");
				}
			}
		}
		public JsonResult PreviewData(Guid id, CC.Web.Models.jQueryDataTableParamModel jq)
		{
			CheckImportIdPermissions(id);

			using (var db = new ccEntities())
			{
				var CfsRows = db.CfsRows;
				var ImportHceps = db.ImportHceps;
				var q = PreviewDataQuery(id, db, CfsRows, ImportHceps);
				var filtered = q;
				var sorted = filtered.OrderByDescending(f => f.Errors.Count());
				if (jq.iSortCol_0 != 1)
				{
					sorted = filtered.OrderByField(Request["mDataProp_" + jq.iSortCol_0], jq.sSortDir_0 == "asc");
				}
				if (Request["iSortCol_1"] != null && jq.iSortCol_0 != 1)
					sorted = sorted.ThenByField(Request["mDataProp_" + Request["iSortCol_1"]], Request["sSortDir_1"] == "asc");
				if (Request["iSortCol_2"] != null && jq.iSortCol_0 != 1)
					sorted = sorted.ThenByField(Request["mDataProp_" + Request["iSortCol_2"]], Request["sSortDir_2"] == "asc");

				var result = new jQueryDataTableResult()
				{
					sEcho = jq.sEcho,
					iTotalDisplayRecords = filtered.Count(),
					iTotalRecords = q.Count(),
					aaData = sorted.Skip(jq.iDisplayStart).Take(jq.iDisplayLength).ToList()

				};
				return this.MyJsonResult(result, JsonRequestBehavior.AllowGet);
			}
		}

		private IQueryable<ImportCfsPreviewRow> PreviewDataQuery(Guid id, ccEntities db, System.Data.Objects.IObjectSet<CfsRow> CfsRows, System.Data.Objects.IObjectSet<ImportHcep> ImportHceps)
		{
			var today = DateTime.Today;
			var isAdmin = User.IsInRole(FixedRoles.Admin);
			var q = from i in ImportHceps
					where i.ImportId == id
					join c in db.Clients.Where(this.Permissions.ClientsFilter) on i.ClientId equals c.Id into cG
					from c in cG.DefaultIfEmpty()
					join ol in CfsRows on i.ClientId equals ol.ClientId into olg
					join sol in ImportHceps.Where(f => f.ImportId == id) on i.ClientId equals sol.ClientId into solg
					let cfs = db.CfsRows.Where(f => f.ClientId == c.Id)
					let cfsWithoutStartDate = cfs.Where(f => f.StartDate == null)
					let allCfsWithStartDate = cfs.Count() > 1 && cfs.All(f => f.StartDate != null)
					select new ImportCfsPreviewRow
					{
						RowIndex = i.RowIndex,
						ClientId = i.ClientId,
						ClientName = c.FirstName + " " + c.LastName,
						StartDate = i.StartDate,
						CFSApproved = i.CfsApproved,
						Warning = i.StartDate != null && cfs.Count() == 1 && cfs.FirstOrDefault().StartDate != null && cfs.FirstOrDefault().StartDate != i.StartDate ? "Please notice that you are about to change an existing start date." : "",
						Errors = new List<string>
							{
								(c==null? "Invalid CC_ID":null),
								(i.ClientId==null?"CC_ID is required":null),
								(i.StartDate==null?"START_DATE is required":null),
								(!isAdmin && i.StartDate < today?"Start date must be greater than or equal to today's date":null),
								(cfsWithoutStartDate.Count() > 1?"the CC ID " + SqlFunctions.StringConvert((decimal?)c.Id) + " has multiple CFS records without CFS start date. Please edit manually":null),
								(allCfsWithStartDate?"the CC ID " + SqlFunctions.StringConvert((decimal?)c.Id) + " has multiple CFS records all with start date. Please edit manually":null),
								(cfs.Count() == 0?"the CC ID " + SqlFunctions.StringConvert((decimal?)c.Id) + " has no CFS records. Please add manually":null),
								(cfsWithoutStartDate.Count() == 1 && cfsWithoutStartDate.FirstOrDefault().EndDate != null && cfsWithoutStartDate.FirstOrDefault().EndDate <= i.StartDate?"Start Date must be earlier than End Date":null)
							}.Where(f => f != null)

					};
			return q;
		}


		[HttpPost]
		[CcAuthorize(FixedRoles.Admin, FixedRoles.CfsAdmin)]
		public ActionResult Import(Guid id)
		{
			using (var db = new ccEntities())
			{
				var res = db.ImportCfsProc(id, Permissions.User.AgencyId, Permissions.User.AgencyGroupId, Permissions.User.RegionId);

				return this.RedirectToAction(f => f.Default());
			}
		}

		#endregion

		#region exports

		[CcAuthorize(FixedRoles.Admin, FixedRoles.CfsAdmin)]
		public ActionResult Exports()
		{
			var model = new CfsExportsModel();
			return View(model);
		}

		#region New CFS Clients export
		[CcAuthorize(FixedRoles.Admin, FixedRoles.CfsAdmin)]
		public ActionResult NewCfsClientsExport()
		{
			var now = DateTime.Now;
			var result = (from cfs in db.CfsRows
						  where cfs.ClientResponseIsYes && cfs.StartDate == null
						  let eligibility = cfs.Client.HomeCareEntitledPeriods.Where(h => h.StartDate < DateTime.Now && (h.EndDate == null || DateTime.Now < h.EndDate)).OrderByDescending(f => f.StartDate).FirstOrDefault()
						  let functionality = cfs.Client.FunctionalityScores.OrderByDescending(f => f.StartDate).FirstOrDefault()
						  let govh = cfs.Client.GovHcHours1.OrderByDescending(f => f.StartDate).FirstOrDefault()
						 select new NewCfsClientsRow
						 {
							 AgencyId = cfs.Client.AgencyId,
							 AgencyName = cfs.Client.Agency.Name,
							 ClientId = cfs.ClientId,
							 JoinDate = cfs.Client.JoinDate,
							 EligibilityStartDate = eligibility != null ? eligibility.StartDate : (DateTime?)null,
							 DafScore = functionality != null ? functionality.DiagnosticScore : (decimal?)null,
							 DafLevel = functionality != null ? functionality.FunctionalityLevel.Name : null,
							 DafEffectiveDate = functionality != null ? functionality.StartDate : (DateTime?)null,
							 GovtHours = govh != null ? govh.Value : (decimal?)null,
							 GovtHoursEffectiveDate = govh != null ? govh.StartDate: (DateTime?)null,
							 CfsLevel = ccEntitiesExtensions.CfsLevelForGivenDate(cfs.ClientId, now),
							 CfsEffectiveDate = now
						 }).ToList();

			return this.Excel("New CFS Awaiting Approval", "Sheet1", result);
		}

		public class CfsClientRowBase
		{
			[Display(Name = "Org ID", Order = 1)]
			public int AgencyId { get; set; }
			[Display(Name = "Agency Name", Order = 2)]
			public string AgencyName { get; set; }
			[Display(Name = "CC ID", Order = 3)]
			public int ClientId { get; set; }
			[Display(Name = "Join Date", Order = 4)]
			public DateTime JoinDate { get; set; }
			[Display(Name = "Eligibiliti Start Date", Order = 5)]
			public DateTime? EligibilityStartDate { get; set; }
			[Display(Name = "DAF Score", Order = 8)]
			public decimal? DafScore { get; set; }
			[Display(Name = "DAF Level", Order = 9)]
			public string DafLevel { get; set; }
			[Display(Name = "DAF Effective Date", Order = 10)]
			public DateTime? DafEffectiveDate { get; set; }
			[Display(Name = "Govt Hours", Order = 11)]
			public decimal? GovtHours { get; set; }
			[Display(Name = "Govt Hours Effective Date", Order = 12)]
			public DateTime? GovtHoursEffectiveDate { get; set; }
		}

		public class NewCfsClientsRow : CfsClientRowBase
		{
			[Display(Name = "CFS Level", Order = 13)]
			public int? CfsLevel { get; set; }
			[Display(Name = "CFS Level Effective Date", Order = 14)]
			public DateTime? CfsEffectiveDate { get; set; }
		}
		#endregion

		#region Approved CFS Clients
		[CcAuthorize(FixedRoles.Admin, FixedRoles.CfsAdmin)]
		public ActionResult ApprovedCfsClients()
		{
			var now = DateTime.Now;
			var result = (from cfs in db.CfsRows
						  where cfs.StartDate != null && cfs.CfsApproved != null
						  where !cfs.Client.Agency.AgencyGroup.ExcludeFromReports
						  let has = cfs.Client.ClientHcStatuses.OrderByDescending(f => f.StartDate).FirstOrDefault()
						  let daf = cfs.Client.FunctionalityScores.OrderByDescending(f => f.StartDate).FirstOrDefault()
						  let hc = (from h in db.HcCapsTableRaws
									where h.ClientId == cfs.ClientId && h.StartDate <= now
										  && (h.EndDate == null || h.EndDate > now)
									select h).OrderByDescending(f => f.StartDate).FirstOrDefault()
						  let cfsLevel = ccEntitiesExtensions.CfsLevelForGivenDate(cfs.ClientId, now)
						  let govh = cfs.Client.GovHcHours1.OrderByDescending(f => f.StartDate).FirstOrDefault()
						  select new ApprovedCfsClientsRow
						  {
							  AgencyId = cfs.Client.AgencyId,
							  AgencyName = cfs.Client.Agency.Name,
							  ClientId = cfs.ClientId,
							  Has = has != null ? has.HcStatusId : (int?)null,
							  DafLevel = daf != null ? daf.FunctionalityLevel.Name : "",
							  HcHours = hc != null ? hc.HcCap : (decimal?)null,
							  CfsLevel = cfsLevel,
							  GovHours = govh != null ? govh.Value : (decimal?)null,
							  MAFDate = cfs.Client.MAFDate,
							  MAF105Date = cfs.Client.MAF105Date,
							  CfsEndDate = cfs.EndDate,
							  CfsEndDateReason = cfs.CfsEndDateReason.Name
						  }).ToList();

			return this.Excel("Approved CFS Clients", "Sheet1", result);
		}

		public class ApprovedCfsClientsRow
		{
			[Display(Name = "Org ID", Order = 1)]
			public int AgencyId { get; set; }
			[Display(Name = "Agency Name", Order = 2)]
			public string AgencyName { get; set; }
			[Display(Name = "CC ID", Order = 3)]
			public int ClientId { get; set; }
			[Display(Name = "HAS", Order = 4)]
			public int? Has { get; set; }
			[Display(Name = "DAF Level", Order = 5)]
			public string DafLevel { get; set; }
			[Display(Name = "Current HC Hours Allowed", Order = 6)]
			public decimal? HcHours { get; set; }
			[Display(Name = "CFS Level", Order = 7)]
			public int CfsLevel { get; set; }
			[Display(Name = "Current Govt Hours", Order = 8)]
			public decimal? GovHours { get; set; }
			[Display(Name = "MAF Date", Order = 9)]
			public DateTime? MAFDate { get; set; }
			[Display(Name = "MAF 105+ Date", Order = 10)]
			public DateTime? MAF105Date { get; set; }
			[Display(Name = "End Date", Order = 11)]
			public DateTime? CfsEndDate { get; set; }
			[Display(Name = "End Date Reason", Order = 11)]
			public string CfsEndDateReason { get; set; }
		}
		#endregion

		#region ACFS Upload
		[CcAuthorize(FixedRoles.Admin, FixedRoles.CfsAdmin)]
		public ActionResult ACFSUpload()
		{
			var now = DateTime.Now;
			var result = (from cfs in db.CfsRows
						  where cfs.CfsApproved != null && cfs.StartDate != null
						  let cfsLevel = ccEntitiesExtensions.CfsLevelForGivenDate(cfs.ClientId, now)
						  select new ACFSUploadRow
						  {
							  AgencyId = cfs.Client.AgencyId,
							  AgencyName = cfs.Client.Agency.Name,
							  ClientId = cfs.ClientId,
							  CfsLevel = cfsLevel,
							  CfsEndDate = cfs.EndDate,
							  CfsEndDateReason = cfs.CfsEndDateReason.Name,
							  ClientLeaveReason = cfs.Client.LeaveReasonId.HasValue ? cfs.Client.LeaveReason.Name : ""
						  }).ToList();

			return this.Excel("ACFS Upload", "Sheet1", result);
		}

		public class ACFSUploadRow
		{
			[Display(Name = "Org ID", Order = 1)]
			public int AgencyId { get; set; }
			[Display(Name = "Agency Name", Order = 2)]
			public string AgencyName { get; set; }
			[Display(Name = "CC ID", Order = 3)]
			public int ClientId { get; set; }
			[Display(Name = "CFS Level", Order = 4)]
			public int CfsLevel { get; set; }
			[Display(Name = "End Date", Order = 5)]
			public DateTime? CfsEndDate { get; set; }
			[Display(Name = "End Date Reason", Order = 6)]
			public string CfsEndDateReason { get; set; }
			[Display(Name = "Leave Reason", Order = 6)]
			public string ClientLeaveReason { get; set; }
		}
		#endregion

		#region New CFS Eligible Clients export
		[CcAuthorize(FixedRoles.Admin, FixedRoles.CfsAdmin)]
		public ActionResult NewCfsEligibleClientsExport(CfsExportsModel model)
		{
			var now = DateTime.Now;
			var month = model.newCfsEligibleClientsFilter.MonthId ?? now.Month + 1;
			var year = model.newCfsEligibleClientsFilter.Year ?? DateTime.Today.Year;
			var startOfMonth = new DateTime(now.Year, month, 1);
			var nextMonth = startOfMonth.AddMonths(1);
			var result = (from cfs in db.CfsRows
						  where SqlFunctions.DatePart("month", cfs.StartDate) == month && SqlFunctions.DatePart("year", cfs.StartDate) == year
						  let eligibility = cfs.Client.HomeCareEntitledPeriods.Where(h => h.StartDate < nextMonth && (h.EndDate == null || startOfMonth < h.EndDate)).OrderByDescending(f => f.StartDate).FirstOrDefault()
						  let functionality = cfs.Client.FunctionalityScores.OrderByDescending(f => f.StartDate).FirstOrDefault()
						  let govh = cfs.Client.GovHcHours1.OrderByDescending(f => f.StartDate).FirstOrDefault()
						  select new NewCfsEligibleClientsRow
						  {
							  AgencyId = cfs.Client.AgencyId,
							  AgencyName = cfs.Client.Agency.Name,
							  ClientId = cfs.ClientId,
							  JoinDate = cfs.Client.JoinDate,
							  EligibilityStartDate = eligibility != null ? eligibility.StartDate : (DateTime?)null,
							  CfsStartDate = cfs.StartDate,
                              CfsEndDate = cfs.EndDate,
							  DafScore = functionality != null ? functionality.DiagnosticScore : (decimal?)null,
							  DafLevel = functionality != null ? functionality.FunctionalityLevel.Name : null,
							  DafEffectiveDate = functionality != null ? functionality.StartDate : (DateTime?)null,
							  GovtHours = govh != null ? govh.Value : (decimal?)null,
							  GovtHoursEffectiveDate = govh != null ? govh.StartDate : (DateTime?)null,
							  CfsLevel = ccEntitiesExtensions.CfsLevelForGivenDate(cfs.ClientId, startOfMonth),
							  CfsEffectiveDate = startOfMonth
						  }).ToList();

			return this.Excel("New CFS Eligible Clients", "Sheet1", result);
		}

		public class NewCfsEligibleClientsRow : NewCfsClientsRow
		{
			[Display(Name = "CFS Eligibility Start Date", Order = 6)]
			public DateTime? CfsStartDate { get; set; }
            [Display(Name = "CFS Eligibility End Date", Order = 7)]
            public DateTime? CfsEndDate { get; set; }
        }
		#endregion

		#region CFS Eligible With Level Change export
		[CcAuthorize(FixedRoles.Admin, FixedRoles.CfsAdmin)]
		public ActionResult CfsEligibleWithLevelChangeExport(CfsExportsModel model)
		{
			var now = DateTime.Now;
			var month = model.cfsEligibleWithLevelChangeFilter.MonthId ?? now.Month;
			var year = model.cfsEligibleWithLevelChangeFilter.Year ?? now.Year;
			var startOfMonth = new DateTime(year, month, 1);
			var nextMonth = startOfMonth.AddMonths(1);
			var data = db.CfsLevelChangesInGivenMonth(startOfMonth, nextMonth).ToList();
			var result = (from c in data
						  let prev = data.Where(f => f.StartDate < c.StartDate).OrderByDescending(f => f.StartDate).FirstOrDefault()
						  where prev != null
						  select new CfsEligibleWithLevelChangeRow
						  {
							  AgencyId = c.AgencyId,
							  AgencyName = c.AgencyName,
							  ClientId = c.ClientId,
							  JoinDate = c.JoinDate,
							  EligibilityStartDate = c.EiligibilityStartDate,
							  CfsStartDate = c.CfsStartDate,
							  DafScore = c.DafScore,
							  DafLevel = c.DafLevel,
							  DafEffectiveDate = c.DafStartDate,
							  GovtHours = c.GovhHours,
							  GovtHoursEffectiveDate = c.GovhStartDate,
							  CfsLevel = c.CfsLevel,
							  CfsEffectiveDate = c.StartDate,
							  PrevCfsLevel = prev != null ? prev.CfsLevel : (int?)null
						  }).ToList();

			return this.Excel("CFS Eligible With Level Changes", "Sheet1", result);
		}

		public class CfsEligibleWithLevelChangeRow : CfsClientRowBase
		{
			[Display(Name = "CFS Eligibility Start Date", Order = 6)]
			public DateTime? CfsStartDate { get; set; }
			[Display(Name = "New CFS Level", Order = 12)]
			public int? CfsLevel { get; set; }
			[Display(Name = "CFS Level Effective Date", Order = 13)]
			public DateTime? CfsEffectiveDate { get; set; }
			[Display(Name = "Previous CFS Level", Order = 14)]
			public int? PrevCfsLevel { get; set; }
		}
		#endregion

		#region CFS Eligible End Date export
		[CcAuthorize(FixedRoles.Admin, FixedRoles.CfsAdmin)]
		public ActionResult CfsEligibleEndDateExport(CfsExportsModel model)
		{
			var now = DateTime.Now;
			var month = model.cfsEligibleEndDateFilter.MonthId ?? now.Month;
			var year = model.cfsEligibleEndDateFilter.Year ?? now.Year;
			var startOfMonth = new DateTime(year, month, 1);
			var nextMonth = startOfMonth.AddMonths(1);
			var result = (from cfs in db.CfsRows
						  where cfs.StartDate != null
						  where cfs.EndDate >= startOfMonth && cfs.EndDate < nextMonth
						  let eligibility = cfs.Client.HomeCareEntitledPeriods.Where(h => h.StartDate < nextMonth && (h.EndDate == null || startOfMonth < h.EndDate)).OrderByDescending(f => f.StartDate).FirstOrDefault()
						  select new CfsEligibleEndDateRow
						  {
							  AgencyId = cfs.Client.AgencyId,
							  AgencyName = cfs.Client.Agency.Name,
							  ClientId = cfs.ClientId,
							  JoinDate = cfs.Client.JoinDate,
							  EligibilityStartDate = eligibility != null ? eligibility.StartDate : (DateTime?)null,
							  CfsStartDate = cfs.StartDate,
							  CfsEndDate = cfs.EndDate,
							  CfsEndDateReason = cfs.CfsEndDateReason.Name
						  }).ToList();

			return this.Excel("CFS Eligible End Date", "Sheet1", result);
		}

		public class CfsEligibleEndDateRow
		{
			[Display(Name = "Org ID")]
			public int AgencyId { get; set; }
			[Display(Name = "Agency Name")]
			public string AgencyName { get; set; }
			[Display(Name = "CC ID")]
			public int ClientId { get; set; }
			[Display(Name = "Join Date")]
			public DateTime JoinDate { get; set; }
			[Display(Name = "Eligibility Start Date")]
			public DateTime? EligibilityStartDate { get; set; }
			[Display(Name = "CFS Eligibility Start Date")]
			public DateTime? CfsStartDate { get; set; }
			[Display(Name = "CFS Eligibility End Date")]
			public DateTime? CfsEndDate { get; set; }
			[Display(Name = "Cfs End Date Reason")]
			public string CfsEndDateReason { get; set; }
		}
		#endregion

		#endregion
	}
}
