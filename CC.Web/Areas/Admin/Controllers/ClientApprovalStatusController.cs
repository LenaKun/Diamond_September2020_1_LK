using System;
using CC.Web.Models;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CC.Data;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using System.Text.RegularExpressions;
using CC.Data.Repositories;
using System.Data.Objects.DataClasses;
using CC.Web.Areas.Admin.Models;
using System.Data.Entity;
using System.Data.SqlClient;
using p4b.Web.Export;
using MvcContrib;
using MvcContrib.ActionResults;
using System.Threading;
using CC.Web.Controllers;

namespace CC.Web.Areas.Admin.Controllers
{
    [CcAuthorize(CC.Data.FixedRoles.Admin)]
	public class ClientApprovalStatusController : AdminControllerBase
	{
		private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(ClientApprovalStatusController));

		public ClientApprovalStatusController() { }

		public ActionResult Index(ClientsIndexModel model)
		{
			model = model ?? new ClientsIndexModel();
            using (var db = new ccEntities())
            {
                var lastRun = db.Globals.Where(f => f.Message == "CFS auto import/upload finished").OrderByDescending(f => f.Date).FirstOrDefault();
                if (lastRun != null)
                {
                    ViewBag.LastRun = lastRun.Date.Value.ToString();
                }
            }
			return View("Index", model);
		}

		public ActionResult UpdateNewToPending(ClientsIndexModel model)
		{
			using (var db = new ccEntities())
			{
				try
				{
					var clients = db.UpdateClientApptovalStatus((int)ApprovalStatusEnum.New, (int)ApprovalStatusEnum.Pending, this.Permissions.User.Id).ToList();
					model.Messages.Add(string.Format("{0} records updated", clients.Count()));
					model.UpdateClientsCount(db);
				}
				catch (System.Data.UpdateException ex)
				{
					model.Exception = ex;
				}

				return Index(model);
			}
		}

		#region Csv exports

        public ActionResult ClientsServiceDate(ClientsIndexModel model)
        {
            ModelState.Clear();
            try
            {
                db.CommandTimeout = 600;
                var m = new ClientsServiceDateExportModel();
                var data = m.GetClientServiceDateReport(db,Permissions);
                return this.Csv(data);
            }
            catch (Exception ex)
            {
				string error = ex.Message;
				if(ex.InnerException != null)
				{
					var inner = ex.InnerException;
					while(inner.InnerException != null)
					{
						inner = inner.InnerException;
					}
					error = inner.Message;
				}
				model.Exception = ex;
				model.Messages.Add(error);
                return View("Index", model);
            }

            throw new NotImplementedException();
        }


        public ActionResult ClientsRejectedPending(ClientsIndexModel model)
        {
            ModelState.Clear();
            try
            {
                db.CommandTimeout = 600;
                var m = new ClientsRejectedPendingReportModel();
                var data = m.GetRejectedPendingData(db, Permissions);
                return this.Excel("output", "Sheet1", data);
            }
            catch
            {
                return View("Index", model);
            }

            throw new NotImplementedException();
        }


		public ActionResult CsvExport(ClientsIndexModel model)
		{
			ModelState.Clear();

			TryValidateModel(model.HistoryExportModel);

			if (ModelState.IsValid)
			{
				var m = model.HistoryExportModel;
				var records = m.GetExportData(db, Permissions);
				var propperties = m.GetExportProperties();
				return this.Csv(records, propperties);
			}
			else
			{
				return View("Index", model);
			}

		}

		public ActionResult ExportNew(ClientsIndexModel model)
		{
			ModelState.Clear();
			try
			{
				db.CommandTimeout = 600;
				var data = db.NewClientsExport((int)ApprovalStatusEnum.New);
				return this.Csv(data);
			}
			catch (Exception)
			{
				return View("Index", model);
			}

			throw new NotImplementedException();
		}

		#endregion

		#region Impoprt
		[HttpPost]
		public ActionResult Upload(HttpPostedFileWrapper file)
		{
			if(file == null)
			{
				var model = new ClientsIndexModel();
				ViewBag.ImportApprovalStatusError = "Please select a file.";
				return View("Index", model);
			}
			var id = Guid.NewGuid();

			string fileName = System.IO.Path.Combine(System.IO.Path.GetTempPath(), id.ToString());

			var csvConf = new CsvHelper.Configuration.CsvConfiguration()
			{
				IsStrictMode = false,
				IsCaseSensitive = false,
				SkipEmptyRecords = true
			};

			csvConf.ClassMapping<ApprovalStatusCsvMap>();



			using (var csvReader = new CsvHelper.CsvReader(new System.IO.StreamReader(file.InputStream), csvConf))
			{

				var updatedAt = DateTime.Now;
				var updatedBy = this.Permissions.User.Id;

				var csvChunkSize = 10000;
				var recordIndex = 1;

				Dictionary<int, int> fsas = new Dictionary<int, int>();

				using (var db = new ccEntities())
				{
					db.Imports.AddObject(new CC.Data.Import()
					{
						Id = id,
						StartedAt = DateTime.Now,
						UserId = this.Permissions.User.Id
					});
					db.SaveChanges();

					var q = (from fs in db.FundStatuses
							 join a in db.ApprovalStatuses on fs.ApprovalStatusName equals a.Name
							 select new
							 {
								 fsid = fs.Id,
								 asid = a.Id
							 }
								);
					foreach (var intem in q)
					{
						fsas.Add(intem.fsid, intem.asid);
					}
				}



				foreach (var csvChunk in csvReader.GetRecords<ImportClient>().Split(csvChunkSize))
				{
					string connectionString = System.Data.SqlClient.ConnectionStringHelper.GetProviderConnectionString();

					using (var sqlBulk = new System.Data.SqlClient.SqlBulkCopy(connectionString, SqlBulkCopyOptions.KeepNulls))
					{
						foreach (var record in csvChunk)
						{
							record.RowIndex = recordIndex++;
							record.ImportId = id;
							if (record.FundStatusId.HasValue && fsas.ContainsKey(record.FundStatusId.Value))
							{
								record.ApprovalStatusId = fsas[record.FundStatusId.Value];
							}
							record.UpdatedAt = updatedAt;
							record.UpdatedById = updatedBy;
						}

						var dataTable = csvChunk.ToDataTable();
						var q = dataTable.Columns.OfType<System.Data.DataColumn>().Where(f => f.DataType == typeof(Int32)).Select(f => new
						{
							c = f.ColumnName,
							values = dataTable.Rows.OfType<System.Data.DataRow>().Select((r, i) => r[f.ColumnName])
						});

						sqlBulk.DestinationTableName = "ImportClients";
						sqlBulk.NotifyAfter = 1000;
						sqlBulk.ColumnMappings.Add("ClientId", "ClientId");
						sqlBulk.ColumnMappings.Add("FundStatusId", "FundStatusId");
						sqlBulk.ColumnMappings.Add("RowIndex", "RowIndex");
						sqlBulk.ColumnMappings.Add("ImportId", "ImportId");
						sqlBulk.ColumnMappings.Add("UpdatedAt", "UpdatedAt");
						sqlBulk.ColumnMappings.Add("UpdatedById", "UpdatedById");

						sqlBulk.SqlRowsCopied += (s, e) =>
						{
							System.Diagnostics.Debug.Write(e.RowsCopied);

						};

						sqlBulk.WriteToServer(dataTable);
					}

				}
			}

			return RedirectToAction("Preview", new { id = id });
		}
		public ActionResult Preview(Guid id)
		{
			var q = from i in db.ImportClients.Where(f => f.ImportId == id)
					join c in db.Clients on i.ClientId equals c.Id into cg
					from c in cg.DefaultIfEmpty()
					join f in db.FundStatuses on i.FundStatusId equals f.Id into fg
					from f in fg.DefaultIfEmpty()
					join a in db.ApprovalStatuses on f.ApprovalStatusName equals a.Name into ag
					from a in ag.DefaultIfEmpty()
					where c == null || f == null || a == null
					select i;
			if (q.Any())
			{
				ModelState.AddModelError(string.Empty, "One or more rows are invalid.");
			}
			return View(id);
		}
		public JsonResult PreviewData(Guid id, jQueryDataTableParamModel jq)
		{
			var q = from i in db.ImportClients.Where(f => f.ImportId == id)
					join c in db.Clients on i.ClientId equals c.Id into cg
					from c in cg.DefaultIfEmpty()
					join f in db.FundStatuses on i.FundStatusId equals f.Id into fg
					from f in fg.DefaultIfEmpty()
					join a in db.ApprovalStatuses on f.ApprovalStatusName equals a.Name into ag
					from a in ag.DefaultIfEmpty()
					select new
					{
						RowIndex = i.RowIndex,
						ClientId = i.ClientId,
						FundStatusName = f.Name,
						ApprovalStatusName = a.Name,
						Errors = (c == null ? "Invalid Client ID. " : "") +
								 (f == null ? "Invalid Fund Status. " : "") +
								 (a == null ? "Invalid Approval Status." : "")
					};

			var filtered = q;
			if (!string.IsNullOrWhiteSpace(jq.sSearch))
			{
				foreach (var s in jq.sSearch.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
				{
					filtered = filtered.Where(f =>
							f.FundStatusName.Contains(s) ||
							f.ApprovalStatusName.Contains(s) ||
							System.Data.Objects.SqlClient.SqlFunctions.StringConvert((double?)f.ClientId) == s ||
							System.Data.Objects.SqlClient.SqlFunctions.StringConvert((double?)f.RowIndex) == s
							);
				}
			}


			var sortColName = Request.Form["mDataProp_" + Request.Form["iSortCol_0"]];
			var sortDir = Request.Form["sSortDir_0"] == "asc";
			var sorted = filtered.OrderByField(sortColName, sortDir);


			return this.MyJsonResult(new jQueryDataTableResult
			{
				aaData = sorted.Skip(jq.iDisplayStart).Take(jq.iDisplayLength),
				iTotalDisplayRecords = filtered.Count(),
				iTotalRecords = q.Count(),
				sEcho = jq.sEcho
			}, JsonRequestBehavior.AllowGet);

		}
		internal class ApprovalStatusCsvMap : CsvHelper.Configuration.CsvClassMap<ImportClient>
		{
			public ApprovalStatusCsvMap()
			{
				Map(f => f.ClientId).Name("CC_ID", "CCID", "CC ID", "ClientID", "Client id", "client_id");
				Map(f => f.FundStatusId).Name("Fund_Status").TypeConverter<FundStatusConverter>();
			}
		}
		class FundStatusConverter : CsvHelper.TypeConversion.ITypeConverter
		{

			List<FundStatus> _fundStatuses;


			public FundStatusConverter()
			{
				using (var db = new ccEntities())
				{
					_fundStatuses = db.FundStatuses.ToList();
				}
			}

			public bool CanConvertFrom(Type type)
			{
				return type == typeof(string);
			}

			public bool CanConvertTo(Type type)
			{
				throw new NotImplementedException();
			}

			public object ConvertFromString(System.Globalization.CultureInfo culture, string text)
			{
				text = text.Trim();

				var fs = _fundStatuses.SingleOrDefault(f => f.Name.Equals(text, StringComparison.InvariantCultureIgnoreCase));
				if (fs != null)
				{
					return fs.Id;
				}
				else
				{
					return null;
					throw new InvalidOperationException("Value \"" + text + "\" can not be converted to Fund Status.");
				}
			}

			public object ConvertFromString(string text)
			{
				return this.ConvertFromString(System.Globalization.CultureInfo.InvariantCulture, text);
			}

			public string ConvertToString(System.Globalization.CultureInfo culture, object value)
			{
				throw new NotImplementedException();
			}

			public string ConvertToString(object value)
			{
				throw new NotImplementedException();
			}

		}

		public ActionResult Import(Guid id)
		{
			try
			{
				var clients = db.ImportClientFundStatusProc(id).ToList();
                var IsraelCashSerNumber = System.Web.Configuration.WebConfigurationManager.AppSettings["IsraelCashSerNumber"].Parse<int>();
                var cfsClients = clients.Where(c=>c.AgencyGroupId == IsraelCashSerNumber).ToList();
				var context = System.Web.HttpContext.Current;

               


                    new Thread(delegate()
                {
                    try
                    {
                        // var sendClients = clients.Where(c=>c.LeaveReasonId == null  ||c.LeaveReasonId != (int)LeaveReasonEnum.Deceased || c.DeceasedDate ==null).ToList();
                        var sendClients = clients.Where(c =>  c.DeceasedDate == null).ToList();
                        var approvalStatusSend = new List<ImportClientFundStatusProc_Result>();
						var hcStatusSend = new List<ImportClientFundStatusProc_Result>();
						foreach (var c in sendClients)
						{
							//sp returns clients with approval OR hc status changed
							if (c.OldHcStatusId != c.NewHcStatusId && c.OldApprovalStatusId == c.NewApprovalStatusId)
							{
								hcStatusSend.Add(c);
							}
							else if(c.OldApprovalStatusId != c.NewApprovalStatusId)
							{
								approvalStatusSend.Add(c);
							}

                           
						}
						
                        if (approvalStatusSend.Count > 0)
                        {
                            SendStatusChangeNotifications(sendClients); //set as job
                        }
						if (hcStatusSend.Any())
						{
							using (var j = new CC.Web.Jobs.HcStatusChangeNotificationsJob())
							{
								var groupedBySer = hcStatusSend.GroupBy(f=> f.AgencyGroupId ).Select(f=>new ClientsHcStatusChangeEmailModel{
									AgencyGroupId = f.Key,
									Clients = f.Select(c=>new ClientHcStatusChangeEmailModel{
										ClientId = c.ClientId ?? 0,
										ClientName = c.FirstName + " " + c.LastName,
										ApprovalStatusName = ((ApprovalStatusEnum)c.NewApprovalStatusId).DisplayName(),
										BirthCountryName = c.BirthCountryName,
										CountryName = c.CountryName,
										HcStatusName = c.HcStatusName,
										AgencyId = c.AgencyId
									})
								});
								j.SendNotifications(groupedBySer);
							}
						}
                        if (cfsClients.Count > 0)
                        {
                            CC.Web.Controllers.ClientsController.ExportCsvForChangedApprovalStatus(cfsClients);
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex);
                    }
                }).Start();

                return RedirectToAction("Index");
			}
			catch (System.Data.EntityCommandExecutionException ex)
			{
				_log.Fatal(ex.Message, ex);
				ModelState.AddModelError(string.Empty, ex.InnerException.Message);
			}
			catch (Exception ex)
			{
				_log.Fatal(ex.Message, ex);
				ModelState.AddModelError(string.Empty, ex);
			}
			return View("Preview", id);
		}
		[HttpPost]
		public ActionResult SendNotifications(DateTime f, DateTime t)
		{
			var q = from h in db.Histories
					where h.TableName == "Clients"
					where h.FieldName == "ApprovalStatusId"
					where h.UpdateDate >= f
					where h.UpdateDate <= t
					join c in db.Clients on h.ReferenceId equals c.Id
                    //where c.LeaveReasonId == null || c.LeaveReasonId != (int)LeaveReasonEnum.Deceased
                    // where c.DeceasedDate != null //Deceased c.LeaveReasonId == null || c.LeaveReasonId != (int)LeaveReasonEnum.Deceased 
                    where c.DeceasedDate == null
                    select new
					{
						ClientId = c.Id,
						AgencyId = c.AgencyId,
						AgencyName = c.Agency.Name,
						AgencyGroupId = c.Agency.GroupId,
						OldStatusId = h.OldValue,
						NewStatusId = h.NewValue,
						CurStatusId = c.ApprovalStatusId,
						c.FirstName,
						c.LastName,
					};
			var a = from item in q.ToList()
					where item.CurStatusId == item.NewStatusId.Parse<int>()
					select new ImportClientFundStatusProc_Result
					{
						AgencyGroupId = item.AgencyGroupId,
						ClientId = item.ClientId,
						FirstName = item.FirstName,
						LastName = item.LastName,
						NewApprovalStatusId = item.NewStatusId.Parse<int>(),
						OldApprovalStatusId = item.OldStatusId.Parse<int>(),
						AgencyId = item.AgencyId
					};
			var b = a.Distinct();
			var result = SendStatusChangeNotifications(b);
            return View("SendNotifications", result);

		}
		[HttpPost]
		public ActionResult SendHasNotifications(DateTime from, DateTime to)
		{
			using (var j = new CC.Web.Jobs.HcStatusChangeNotificationsJob())
			{
				var data = db.spHasNotificationResend(from, to).ToList();
				var groupedBySer = data.GroupBy(f => f.AgencyGroupId).Select(f => new ClientsHcStatusChangeEmailModel
				{
					AgencyGroupId = f.Key,
					Clients = f.Select(c => new ClientHcStatusChangeEmailModel
					{
						ClientId = c.ClientId,
						ClientName = c.FirstName + " " + c.LastName,
						ApprovalStatusName = ((ApprovalStatusEnum)c.NewApprovalStatusId).DisplayName(),
						BirthCountryName = c.BirthCountryName,
						CountryName = c.CountryName,
						HcStatusName = c.HcStatusName,
						AgencyId = c.AgencyId
					})
				});
				j.SendNotifications(groupedBySer);
				return this.Json(groupedBySer);

			}
		}
		private static string msgbody(ClientStatusChangeEmailModel model)
		{
			var sb = new System.Text.StringBuilder();
            //sb.Append(@"<p>Dear Sir/ Madam,</p>
            sb.Append(@"
<p>This email delivers official notice of Change of Status for ").Append(model.Clients.Count()).Append(@" of your agency's clients.  </p>

<p>See list below. </p>

<p>If you have any questions or concerns, please contact your Program Assistant. </p>

<table>
    <thead>
        <tr>
            <th>CC ID</th>
            <th>Name</th>
            <th>Old JNV Approval Status</th>
            <th>New JNV Approval Status</th>
            <th>Homecare Approval Status</th>
        </tr>
    </thead>");
			foreach (var c in model.Clients)
			{
				sb.Append(@"<tbody>
            <tr>
                <td>").Append(c.ClientId).Append(@"</td>
                <td>").Append(c.FirstName).Append(" ").Append(c.LastName).Append(@"</td>
                <td>").Append(((CC.Data.ApprovalStatusEnum)c.OldApprovalStatusId).DisplayName()).Append(@"</td>
                <td>").Append(((CC.Data.ApprovalStatusEnum)c.NewApprovalStatusId).DisplayName()).Append(@"</td>
                <td>").Append(c.HcStatusName).Append(@"</td>
            </tr>
        </tbody>");
			}
			sb.Append(@"</table>
<p>Effective January 1, 2018, Grantees can only serve new clients with Claims Conference grant funds after their confirmation as Jewish Nazi victims. The Claims Conference will no longer allow Grantees to serve clients while their compensation applications are pending. More information will follow in forthcoming Allocation Letters.</p>
<h3>STATUSES REQUIRING AGENCY ACTION:</h3>
 <ul>
<li>
NeverAppliedToCc: Clients did not match to individual CC compensations and clients should submit an application.  Clients joining the program in 2018 cannot be served with CC funds unless Approved in Diamond. A leave date will populate if no action is taken.
</li>
<li>
Agency Action Needed: Clients are matched to rejected or incomplete claims and there is insufficient evidence to verify Jewish Nazi victim status. Clients should  contact Survivor Services. A leave date will populate if the claim is not reopened and clients cannot receive CC funded services. See Diamond Help Screen for more information.
</li>
<li>
Not Eligible: Clients are not eligible to receive CC funded services and must cease receiving them within 3 days of this notification for existing clients (prior to 2018).
</li></ul><p>This is an automated message from Diamond, please do not reply to this email address.</p>");
			return sb.ToString();
		}

		public static Dictionary<int, int> SendStatusChangeNotifications(IEnumerable<ImportClientFundStatusProc_Result> clients)
		{
			var result = new Dictionary<int, int>();
			_log.Debug("Entered " + System.Reflection.MethodBase.GetCurrentMethod().Name);
			using (var db = new ccEntities())
			{
				var gpos = db.Users.Where(f => f.RoleId == (int)FixedRoles.GlobalOfficer || f.RoleId == (int)FixedRoles.GlobalReadOnly || f.RoleId == (int)FixedRoles.AuditorReadOnly);
				var groups = from c in clients
                             
							 group c by c.AgencyId into cg
							 select cg;
				var serUsers = (from s in db.AgencyGroups

								from u in s.Users
								where u.Email != null
								where (u.RoleId == (int)FixedRoles.Ser || u.RoleId == (int)FixedRoles.SerAndReviewer) && !u.Disabled
								select new
								{
									Email = u.Email,
                                    AddToBcc = u.AddToBcc,
									DisplayName = (u.FirstName + " " + u.LastName) ?? u.UserName,
									AgencyGroupId = s.Id
								}).ToList();

                var agencyUsers = (from s in db.Agencies
                                   from u in s.Users
                                   where u.Email != null
								   where (u.RoleId == (int)FixedRoles.AgencyUser || u.RoleId == (int)FixedRoles.AgencyUserAndReviewer) && !u.Disabled
                                   select new
                                   {
                                       Email = u.Email,
                                       AddToBcc = u.AddToBcc,
                                       DisplayName = (u.FirstName + " " + u.LastName) ?? u.UserName,
                                       AgencyId = s.Id,
                                       AgencyGroupId = s.GroupId
                                   }).ToList();

				var poUsers = (from s in db.AgencyGroups
							   from u in s.PoUsers
                               where !u.Disabled
							   select new
							   {
								   Email = u.Email,
                                   AddToBcc = u.AddToBcc,
								   DisplayName = (u.FirstName + " " + u.LastName) ?? u.UserName,
								   AgencyGroupId = s.Id
							   }).ToList();
				foreach (var g in groups.OrderBy(f => f.Key).AsParallel())
				{
					var msg = new System.Net.Mail.MailMessage();
					var groupId = g.FirstOrDefault().AgencyGroupId;
                    //Lena Test Email
                    foreach (var user in serUsers.Where(f => f.AgencyGroupId == groupId))
                    {
                        try
                        {
                            if (!user.AddToBcc)
                            {
                                msg.To.Add(new System.Net.Mail.MailAddress(user.Email, user.DisplayName));
                            }
                            else
                            {
                                msg.Bcc.Add(new System.Net.Mail.MailAddress(user.Email, user.DisplayName));
                            }
                        }
                        catch (Exception ex) { _log.Info(ex.Message, ex); }
                    }

                    foreach (var user in agencyUsers.Where(f => f.AgencyId == g.Key))
                    {
                        try
                        {
                            if (!user.AddToBcc)
                            {
                                msg.CC.Add(new System.Net.Mail.MailAddress(user.Email, user.DisplayName));
                            }
                            else
                            {
                                msg.Bcc.Add(new System.Net.Mail.MailAddress(user.Email, user.DisplayName));
                            }
                        }
                        catch (Exception ex) { _log.Info(ex.Message, ex); }
                    }

                    foreach (var user in poUsers.Where(f => f.AgencyGroupId == groupId))
                    {
                        try
                        {
                            if (!user.AddToBcc)
                            {
                                msg.CC.Add(new System.Net.Mail.MailAddress(user.Email, user.DisplayName));
                            }
                            else
                            {
                                msg.Bcc.Add(new System.Net.Mail.MailAddress(user.Email, user.DisplayName));
                            }
                        }
                        catch (Exception ex) { _log.Info(ex.Message, ex); }
                    }

                    foreach (var user in gpos)
                    {
                        try
                        {
                            msg.Bcc.Add(new System.Net.Mail.MailAddress(user.Email, user.UserName));
                        }
                        catch (Exception ex) { _log.Info(ex.Message, ex); }
                    }
                    //Lena Test Email
                    //msg.To.Add(new System.Net.Mail.MailAddress("Lena.Kunisky@claimscon.org", "Lena Kunisky"));

                    var groupClients = g.Select(f => f);
					var agencygroup = db.AgencyGroups.SingleOrDefault(f => f.Id == groupId);

					msg.IsBodyHtml = true;
					msg.Subject = string.Format("Notification of Client Status Change (SER {0})", agencygroup.DisplayName);
					msg.Body = msgbody(
						new ClientStatusChangeEmailModel { Clients = groupClients });

					using (var smtpClient = new System.Net.Mail.SmtpClient())
					{
						try
						{
							smtpClient.Send(msg);
							result.Add(g.Key, g.Count());
							_log.InfoFormat("sent notification to ser {0} - {1} clients", g.Key, g.Count());
						}
						catch
						{

						}
					}

				}
				return result;
			}
		}
		[HttpPost]
		public ActionResult CancelImport(Guid id)
		{
			try
			{
				var import = new Import { Id = id };
				db.Imports.Attach(import);
				db.Imports.DeleteObject(import);
				db.SaveChanges();
				return RedirectToAction("Index");
			}
			catch (Exception ex)
			{
				_log.Error(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
				return View("Preview", id);
			}
		}
		#endregion

		#region MasterId impoprt

		public ActionResult UploadMasterIds(HttpPostedFileWrapper file)
		{
			if (file == null)
			{
				var model = new ClientsIndexModel();
				ViewBag.ImportMasterIdsError = "Please select a file.";
				return View("Index", model);
			}
			var import = MasterIdsImportModel.Upload(file, this.Permissions.User.Id);

			return this.RedirectToAction(f => f.PreviewMasterIds(import.Id));
		}
		public ActionResult PreviewMasterIds(Guid id)
		{

			var model = db.Imports.Single(f => f.Id == id);
			return View(model);
		}

		[HttpPost]
		public JsonResult PreviewMasterIds(Guid id, CC.Web.Models.jQueryDataTableParamModel jq)
		{
			var source = from i in db.ImportClients
						 where i.ImportId == id
						 join c in db.Clients on i.ClientId equals c.Id into cg
						 from c in cg.DefaultIfEmpty()
						 join nm in db.Clients on i.MasterId equals nm.Id into nmg
						 from nm in nmg.DefaultIfEmpty()
						 join om in db.Clients on c.MasterId equals om.Id into omg
						 from om in omg.DefaultIfEmpty()
						 select new
						 {
							 ClientId = i.ClientId,
							 FirstName = c.FirstName,
							 LastName = c.LastName,

							 MasterId = i.MasterId,
							 NewMasterFirstName = nm.FirstName,
							 NewMasterLastName = nm.LastName,
							 CurrentMasterId = c.MasterId,
							 CurrentMasterFirstName = om.FirstName,
							 CurrentMasterLastName = om.LastName
						 };

			var filtered = source;
			if (jq.sSearch != null)
			{
				foreach (var s in jq.sSearch.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
				{
					filtered = filtered.Where(f =>
						System.Data.Objects.SqlClient.SqlFunctions.StringConvert((double?)f.ClientId).Contains(s) ||
						f.FirstName.Contains(s) ||
						f.LastName.Contains(s));

				}
			}

			var sortCol = Request["mDataProp_" + jq.iSortCol_0.ToString()];
			var sorted = filtered.OrderByField(sortCol, jq.sSortDir_0 == "asc");

			return this.MyJsonResult(new jQueryDataTableResult
			{
				aaData = sorted.Skip(jq.iDisplayStart).Take(jq.iDisplayLength).ToList(),
				iTotalDisplayRecords = filtered.Count(),
				iTotalRecords = source.Count(),
				sEcho = jq.sEcho
			});


		}

		[HttpPost]
		public ActionResult ImportMasterIds(Guid id)
		{
			var rowsUpdated = db.ImportMasterIds(id, this.Permissions.User.Id);
			return this.RedirectToAction(f => f.Index(null));
		}

		#endregion

        [HttpPost]
        public ActionResult ScanAndImportCfsFiles()
        {
            var clientsController = new ClientsController();
            clientsController.ScanAndImportCfsFiles();
            return this.RedirectToAction(f => f.Index(null));
        }

		protected override void Dispose(bool disposing)
		{
			db.Dispose();
			base.Dispose(disposing);
		}


	}
}
