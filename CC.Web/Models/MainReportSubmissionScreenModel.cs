using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using CC.Data;
using System.Data.Objects;
using System.Data.Entity;
using System.Globalization;
using System.Data.Objects.SqlClient;
using CC.Web.Helpers;

namespace CC.Web.Models
{
	/// <summary>
	/// Used to render the submission screen
	/// </summary>
	public class MainReportSubmissionScreenModel : MainReportDetailsBase, IValidatableObject
	{
		private readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(MainReportSubmissionScreenModel));
		#region Constructors
		public MainReportSubmissionScreenModel()
			: base()
		{
			this.Errors = new List<Row>();
		}
		#endregion
		#region Methods
		public override void LoadData()
		{
			_log.DebugFormat("Entering {0}", System.Reflection.MethodBase.GetCurrentMethod().Name);
           
			base.LoadData();

			var errs = Validate().Take(errorsLimit + 1).ToList();
			if (errs.Count == errorsLimit + 1)
			{
				errs.InsertAt(0, new Row()
				{
					MessageTypeName = "Maximum errors limit reached.",
					Message = ""

				});
			}
			this.Errors = errs.Take(errorsLimit);
			this.Warnings = GetWarnings(this.Id);

			if (!this.Errors.Any())
			{
				var canbesubmitted = MainReport.EditableStatuses.Contains(this.Status) &&
					(this.User.RoleId == (int)FixedRoles.Admin || ((this.User.RoleId == (int)FixedRoles.Ser || this.User.RoleId == (int)FixedRoles.SerAndReviewer) && this.User.AgencyGroupId == this.AgencyGroupId));

				if (canbesubmitted)
				{
					if (this.Ser == null)
					{
						this.Ser = new SubmissionDetails();
					}

					this.Ser.Id = this.Id;

					this.Ser.RemarksRequired = this.Warnings.Any();
					this.Ser.UserName = this.Permissions.User.UserName;
					this.Ser.DisclaimerRequired = !(this.SC || this.DCC);
					this.Ser.ProgramOverviewUploadedFile = this.ProgramOverviewFileName;
					this.Ser.MhsaUploadedFile = this.MhsaFileName;

					using (var db = new ccEntities())
					{
						var mainreport = new MainReport() { Id = this.Id, AppBudgetId = this.AppBudgetId };
						var mainReports = db.MainReports.Where(Permissions.MainReportsFilter).Where(MainReport.CurrentOrSubmitted(mainreport.Id));
						var subReports = db.SubReports.Where(Permissions.SubReportsFilter)
									 .Where(SubReport.IsAdministrativeOverhead)
                                     .Where(f => f.MainReport.Id == mainreport.Id);
                        var q =
							from a in
								(from ext in
									 (from mr in mainReports
									  from appbs in mr.AppBudget.AppBudgetServices
									  where appbs.Service.TypeId == (int)Service.ServiceTypes.AdministrativeOverhead
									  select new
									  {
										  mrs = mr.Start,
										  mre = mr.End,
										  apps = appbs.AppBudget.App.StartDate,
										  appe = appbs.AppBudget.App.EndDate,
										  appbsid = appbs.Id,
										  ccg = appbs.CcGrant
									  })
								 group ext by new { appbsid = ext.appbsid, ccg = ext.ccg, apps = ext.apps, appe = ext.appe } into extg
								 select new
								 {
									 appbsid = extg.Key.appbsid,
									 ccg = extg.Key.ccg,
									 mrlen = extg.Sum(f => ccEntities.DiffMonths(f.mrs, f.mre)),
									 applen = ccEntities.DiffMonths(extg.Key.apps, extg.Key.appe)
								 })
							join b in
								(from mr in mainReports
								 join sr in subReports on mr.Id equals sr.MainReportId
								 join sra in db.viewSubreportAmounts on sr.Id equals sra.id
								 group sra by sra.AppBudgetServiceId into srag
								 select new
								 {
									 appbsid = srag.Key,
									 Amount = srag.Sum(f => f.Amount),
								 }) on a.appbsid equals b.appbsid
							select new
							{
								appbsid = a.appbsid,
								ccg = a.ccg,
								applen = a.applen,
								mrlen = a.mrlen,
								amount = b.Amount
							};

						this.Ser.AdministrativeOverheadOverflow = q.Any(f => EntityFunctions.Truncate(f.amount - (f.ccg * f.mrlen / f.applen), CCDecimals.CompareDigits) > 25);
					}

				}
				else
				{
					this.Ser = null;
				}
			}
			else
			{
				this.Ser = null;
				foreach (var err in this.Errors.Where(f => f.SubReportId != default(int)))
				{
					using (var db = new ccEntities())
					{
						var reportinMethodId = db.SubReports.Where(f => f.Id == err.SubReportId).Select(f => (int?)f.AppBudgetService.Service.ReportingMethodId).FirstOrDefault();
						if (reportinMethodId == (int)Service.ReportingMethods.TotalCostWithListOfClientNames && err.MessageTypeName != null && err.MessageTypeName.Contains("Client is no longer eligible"))
						{
							err.Message += " Along with the removal of this ineligible client, please remember to deduct any reported expenses associated with this client from the Total Amount.";
						}
					};
				}
			}
		}


		public IEnumerable<Row> Warnings { get; set; }

		public IEnumerable<Row> GetWarnings(int mainReportId)
		{
			foreach (var r in this.GetLumpMatchingSummWarning(mainReportId))
			{ yield return r; }
			foreach (var r in this.GetDetailedMatchingSummWarnigns(mainReportId))
			{ yield return r; }
			foreach (var r in this.GetHistoricalExpenditureAmountWarnigns(mainReportId))
			{ yield return r; }
			foreach (var r in this.GetClientEventsCountWarnigns(mainReportId))
			{ yield return r; }
			foreach (var r in this.GetAdministrativeOverheadWarnings(mainReportId))
			{ yield return r; }
		}

		public IEnumerable<Row> GetLumpMatchingSummWarning(int mainReportId)
		{
#warning LumnpMatchingSummWarnings probably wont show up

			using (var db = new ccEntities())
			{
				var editableStatuses = MainReport.EditableStatuses.Select(f => (int)f).ToArray();
				var q = (from sr in db.SubReports
						 where sr.MainReportId == mainReportId
						 group sr by sr.AppBudgetService.AppBudget.App into srg

						 select new
						 {
							 Actual = srg
									 .Where(sr => sr.MainReportId == mainReportId || !editableStatuses.Contains(sr.MainReport.StatusId))
									 .Sum(sr => sr.MatchingSum),
							 Expected = srg
									 .Where(sr => sr.MainReportId == mainReportId || !editableStatuses.Contains(sr.MainReport.StatusId))
									 .Sum(sr => sr.MatchingSum)
								 -
								 srg
									 .Where(sr => sr.MainReportId == mainReportId || !editableStatuses.Contains(sr.MainReport.StatusId))
									 .Sum(sr => sr.MatchingSum * System.Data.Objects.EntityFunctions.DiffDays(sr.MainReport.Start, sr.MainReport.End))
								 /
								 System.Data.Objects.EntityFunctions.DiffDays(srg.Key.StartDate, srg.Key.EndDate)
						 }).SingleOrDefault();
				if (q != null)
				{
					if (q.Actual < q.Expected)
					{
						yield return new Row()
						{
							Message = string.Format("The current calculated Required Match / Agency’s contribution is lower than expected at this point (expected: {0}, actual: {1}",
							q.Expected.HasValue ? q.Expected.Value.Format() : null,
							q.Actual.HasValue ? q.Actual.Value.Format() : null)
						};

					}
				}
			}
		}

		public IEnumerable<Row> GetDetailedMatchingSummWarnigns(int mainReportId)
		{
			using (var db = new ccEntities())
			{
				var q = from sr in db.SubReports
						where sr.MainReportId == mainReportId
						join app in
							(from a in db.Apps
							 join sre in db.SubReports on a.Id equals sre.AppBudgetService.AppBudget.AppId
							 group sre by sre.AppBudgetService.AppBudget.App into g
							 select new
							 {
								 AppId = g.Key.Id,
								 MatchingSum = g.Sum(f => f.MatchingSum)
							 }) on sr.AppBudgetService.AppBudget.AppId equals app.AppId
						select new
						{
							SubReportId = sr.Id,
							AgencyId = sr.AppBudgetService.AgencyId,
							ServiceId = sr.AppBudgetService.ServiceId,
							AgencyName = sr.AppBudgetService.Agency.Name,
							ServiceName = sr.AppBudgetService.Service.Name,
							ServiceTypeName = sr.AppBudgetService.Service.ServiceType.Name,
							AppBudgetMatchingSum = sr.AppBudgetService.RequiredMatch,
							MrStart = sr.MainReport.Start,
							MrEnd = sr.MainReport.End,
							AppStart = sr.AppBudgetService.AppBudget.App.StartDate,
							AppEnd = sr.AppBudgetService.AppBudget.App.EndDate,
							MatchingSum = app.MatchingSum,
							ExpectedMs = (sr.AppBudgetService.RequiredMatch) * ((decimal)EntityFunctions.DiffDays(sr.AppBudgetService.AppBudget.App.StartDate, sr.MainReport.End) / (decimal)EntityFunctions.DiffDays(sr.AppBudgetService.AppBudget.App.StartDate, sr.AppBudgetService.AppBudget.App.EndDate))
						};



				//only 2 decimal digits are sugnificant
				foreach (var f in q.Where(f => EntityFunctions.Truncate(f.MatchingSum, 2) < EntityFunctions.Truncate(f.ExpectedMs, 2)).ToList())
				{
					yield return new Row()
					{
						SubReportId = f.SubReportId,
						AgencyId = f.AgencyId,
						AgencyName = f.AgencyName,
						ServiceTypeName = f.ServiceTypeName,
						ServiceName = f.ServiceName,
						MessageTypeName = "Required match is not met",
						Message = "Service: " + f.ServiceName +
							", agency: " + f.AgencyName +
							", entered: " + (f.MatchingSum.HasValue ? f.MatchingSum.Value.Format() : string.Empty) +
							", expected: " + f.ExpectedMs.Format(),

					};

				}


			}
		}

		public IEnumerable<Row> GetHistoricalExpenditureAmountWarnigns(int mainReportId)
		{
			using (var db = new ccEntities())
			{
				var h = db.MainReports.Where(f => f.Id == mainReportId).Select(f => new
				{
					AppId = f.AppBudget.AppId,
					FundName = f.AppBudget.App.Fund.Name,
					AppCur = f.AppBudget.App.CurrencyId
				}).SingleOrDefault();

				var p = (from a in db.Apps.Where(f => f.Id == h.AppId)
						 join fund in db.Funds on a.FundId equals fund.Id
						 select new
						 {
							 OtherServicesMax = (a.OtherServicesMax ?? fund.OtherServicesMax) / 100,
							 HomecareMin = (a.HomecareMin ?? fund.HomecareMin) / 100,
							 AdminMax = (a.AdminMax ?? fund.AdminMax) / 100,
							 a.MaxAdminAmount,
							 a.MaxNonHcAmount,
							 a.HistoricalExpenditureAmount
						 }).Single();

				var ytdServiceTypesQuery = (from a in
												(from appb in db.AppBudgets
												 where appb.Id == this.AppBudgetId
												 from appbs in appb.AppBudgetServices
												 join sra in db.viewSubreportAmounts on appbs.Id equals sra.AppBudgetServiceId
												 join mr in db.MainReports.Where(MainReport.CurrentOrSubmitted(Id)) on sra.MainReportId equals mr.Id
												 group sra by appbs.Service.TypeId into srag
												 select new
												 {
													 ServiceTypeId = srag.Key,
													 Amount = srag.Sum(f => f.Amount)
												 })
											join st in db.ServiceTypes on a.ServiceTypeId equals st.Id
											select new
											{
												ServiceTypeId = st.Id,
												ServiceTypeName = st.Name,
												Amount = a.Amount
											}).ToList();

				decimal total = ytdServiceTypesQuery != null ? ytdServiceTypesQuery.Sum(f => f.Amount) : 0;
				var ytdMonths = db.MainReports.SingleOrDefault(f => f.Id == mainReportId).End.AddMonths(-1).Month;

				if (p.HistoricalExpenditureAmount.HasValue && p.HistoricalExpenditureAmount > 0)
				{
					var historicalYtd = p.HistoricalExpenditureAmount.Value * ytdMonths / 12;
					if (total < historicalYtd)
					{
						yield return new Row
						{
							MessageTypeName = string.Format("Historical expenditure amount"),
							Message = string.Format("The YTD Amount ({0}) in this report is under the base historical expenditure amount proportion for this quarter/ month ({1}).",
							total.Format(),
							historicalYtd.Format())
						};
					}
				}
			}
		}

		public IEnumerable<Row> GetClientEventsCountWarnigns(int mainReportId)
		{
			using (var db = new ccEntities())
			{
				var q = from sr in db.SubReports
						where sr.MainReportId == mainReportId
						where sr.AppBudgetService.Service.TypeId == (int)Service.ServiceTypes.Socialization && sr.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.TotalCostNoNames
							|| sr.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.ClientEventsCount
						select new
						{
							SubReportId = sr.Id,
							ReportingMethodId = sr.AppBudgetService.Service.ReportingMethodId,
							Amount = sr.Amount,
							RowsCount = sr.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.ClientEventsCount ? sr.ClientEventsCountReports.Count() : 0
						};

				if (q.Any(f => f.ReportingMethodId == (int)Service.ReportingMethods.TotalCostNoNames && f.Amount > 0) && !q.Any(f => f.ReportingMethodId == (int)Service.ReportingMethods.ClientEventsCount && f.RowsCount > 0))
				{
					yield return new Row
					{
						MessageTypeName = string.Format("Client Events Count Warning"),
						Message = "There are no events reported for Client Events Count report"
					};
				}
			}
		}

		public IEnumerable<Row> GetAdministrativeOverheadWarnings(int mainReportId)
		{
			using (var db = new ccEntities())
			{
				var h = db.MainReports.Where(f => f.Id == mainReportId).Select(f => new
				{
					AppId = f.AppBudget.AppId,
					FundName = f.AppBudget.App.Fund.Name,
					AppCur = f.AppBudget.App.CurrencyId,
					MrEnd = f.End
				}).SingleOrDefault();

				var p = (from a in db.Apps.Where(f => f.Id == h.AppId)
						 join fund in db.Funds on a.FundId equals fund.Id
						 select new
						 {
							 a.MaxAdminAmount,
							 a.MaxNonHcAmount
						 }).Single();

				var ytdServiceTypesQuery = (from a in
													(from appb in db.AppBudgets
													 where appb.Id == this.AppBudgetId
													 from appbs in appb.AppBudgetServices
													 join sra in db.viewSubreportAmounts on appbs.Id equals sra.AppBudgetServiceId
													 join mr in db.MainReports.Where(MainReport.CurrentOrSubmitted(Id)) on sra.MainReportId equals mr.Id
													 group sra by appbs.Service.TypeId into srag
													 select new
													 {
														 ServiceTypeId = srag.Key,
														 Amount = srag.Sum(f => f.Amount)
													 })
											join st in db.ServiceTypes on a.ServiceTypeId equals st.Id
											select new
											{
												ServiceTypeId = st.Id,
												ServiceTypeName = st.Name,
												Amount = a.Amount
											}).ToList();

				var AdminCcGrant = ytdServiceTypesQuery.Where(f => f.ServiceTypeId == (int)Service.ServiceTypes.AdministrativeOverhead) != null ?
								ytdServiceTypesQuery.Where(f => f.ServiceTypeId == (int)Service.ServiceTypes.AdministrativeOverhead).Sum(f => f.Amount) : 0;
				var OtherGrant = ytdServiceTypesQuery.Where(f => f.ServiceTypeId != (int)Service.ServiceTypes.Homecare && f.ServiceTypeId != (int)Service.ServiceTypes.AdministrativeOverhead) != null ?
					ytdServiceTypesQuery.Where(f => f.ServiceTypeId != (int)Service.ServiceTypes.Homecare && f.ServiceTypeId != (int)Service.ServiceTypes.AdministrativeOverhead).Sum(f => f.Amount) : 0;

				var ytdMonths = h.MrEnd.AddMonths(-1).Month;
				if (p.MaxAdminAmount.HasValue)
				{
					var minYtdAdminCcGrant = p.MaxAdminAmount * ytdMonths / 12;
					if (AdminCcGrant > minYtdAdminCcGrant)
					{
						yield return new Row
						{
							MessageTypeName = string.Format("Maximum Admin YTD"),
							Message = string.Format("The Admin YTD ({0}) in this report is greater than allowed amount for this quarter/ month ({1}).",
							AdminCcGrant.Format(),
							minYtdAdminCcGrant.Format())
						};
					}
				}

				if (p.MaxNonHcAmount.HasValue)
				{
					var nonHcAmount = OtherGrant + AdminCcGrant;
					var minYtdNonHcCcGrant = p.MaxNonHcAmount * ytdMonths / 12;
					if (nonHcAmount > minYtdNonHcCcGrant)
					{
						yield return new Row
						{
							MessageTypeName = string.Format("Maximum non homecare YTD"),
							Message = string.Format("The non homecare YTD ({0}) in this report is greater than allowed amount for this quarter/month ({1}).",
							nonHcAmount.Format(),
							minYtdNonHcCcGrant.Format())
						};
					}
				}
			}
		}
		/// <summary>
		/// performs business rules validation. returns detailed errors that appear int the mainreport
		/// </summary>
		/// <returns></returns>
		public IEnumerable<Row> Validate()
		{
			MainReport mainReport;
			using (var db = new ccEntities(false, false))
			{
                db.CommandTimeout = 10000;
				mainReport = db.MainReports.Include(f => f.AppBudget.App).Include(f => f.AppBudget.App.AgencyGroup).Single(f => f.Id == this.Id);
				if (!AppBudget.IsApproved(mainReport.AppBudget))
				{
					yield return new Row()
					{
						Message = "The budget is not approved"
					};
					yield break;
				}
				if (!mainReport.SubReports.Any())
				{
					yield return new Row() { Message = "The main report is empty" };
					yield break;
				}


			}

			using (var db = new ccEntities(false, false))
			{
				var editableStatuses = MainReport.EditableStatuses.Select(f => (int)f).ToArray();
				var q = db.MainReports.Where(f => f.AppBudget.App.AgencyGroupId == mainReport.AppBudget.App.AgencyGroupId)
					.Where(f => f.AppBudget.AppId == mainReport.AppBudget.AppId)
					.Where(f => editableStatuses.Contains(f.StatusId))
					.Where(f => !f.Revision)
					.Where(f => f.End < mainReport.End).Any();
				if (q)
				{
					yield return new Row()
					{
						Message =
						"Please submit earlier main reports.",
						MessageTypeName = "Main reports submission"
					};
					yield break;

				}


			}
            
            using (var db = new ccEntities(false, false))
            {
                

                var FuneralExpensesAmount = (from mr in db.MainReports
                                             join sr in db.SubReports on mr.Id equals sr.MainReportId
                                             join cr in db.ClientReports on sr.Id equals cr.SubReportId
                                             join c in db.Clients on cr.ClientId equals c.Id
                                             join abs in db.AppBudgetServices on sr.AppBudgetServiceId equals abs.Id
                                             join s in db.Services on abs.ServiceId equals s.Id
                                             where mr.Id == this.Id && s.Name == "Funeral Expenses"  && cr.Amount ==null
                                             select new
                                             {
                                                 clientid = c.Id,
                                                 funeralexpamount = cr.Amount,
                                                // israeliId = c.NationalId,
                                                 //masterId = c.MasterId,
                                                 clientName = c.FirstName + " " + c.LastName
                                                 //serviceName = s.Name,
                                                // subReportId = cr.SubReportId,
                                                // agencyName = c.Agency.Name
                                             }).Distinct().ToList();
                foreach (var c in FuneralExpensesAmount)
                {
                    {
                        var prevclientid = 0;
                        for (int i = 0; i < FuneralExpensesAmount.Count; i++)
                        {
                            //var prevclientid = 0;
                            var clientid = c.clientid;
                            if (prevclientid != clientid)
                            {
                                yield return new Row()
                                {
                                    MessageTypeName = "Main reports submission",
                                    Message = "You are required to submit a value for the Amount field for each client reported for Funeral Expenses: " + "CC ID " + c.clientid + " Name " + c.clientName
                                    
                                };
                                prevclientid = clientid;
                            }
                            //else 
                            //yield break;
                        }
                       // yield break;
                    }
                   // yield break;
                }
                }

            var CashForServicesStartYear = System.Web.Configuration.WebConfigurationManager.AppSettings["CashForServicesStartYear"].Parse<int>();
			var IsraelKerenSerNumber = System.Web.Configuration.WebConfigurationManager.AppSettings["IsraelKerenSerNumber"].Parse<int>();
			var IsraelCashSerNumber = System.Web.Configuration.WebConfigurationManager.AppSettings["IsraelCashSerNumber"].Parse<int>();

			if (mainReport.AppBudget.App.AgencyGroupId == IsraelKerenSerNumber && mainReport.Start.Year >= CashForServicesStartYear) //Benefits of Holocaust Victims in Israel SER (“The Keren”
			{
				using (var db = new ccEntities(false, false))
				{
					var q = db.MainReports.Where(f => f.AppBudget.App.AgencyGroupId == IsraelCashSerNumber) //Israel Cash for Services
						.Where(f => f.Start == mainReport.Start && f.StatusId == (int)MainReport.Statuses.Approved);
					if (q.Any())
					{
						var duplicates = (from mr in q
										  join cr in db.ClientReports on mr.Id equals cr.SubReport.MainReportId
										  join c in db.Clients on cr.ClientId equals c.Id
										  select new
										  {
											  clientId = c.Id,
											  masterId = c.MasterId,
											  israeliId = c.NationalId,
											  clientName = c.FirstName + " " + c.LastName
										  }).Distinct().ToList();
						var clients = (from mr in db.MainReports
									   join sr in db.SubReports on mr.Id equals sr.MainReportId
									   join cr in db.ClientReports on sr.Id equals cr.SubReportId
									   join c in db.Clients on cr.ClientId equals c.Id
									   join abs in db.AppBudgetServices on sr.AppBudgetServiceId equals abs.Id
									   join s in db.Services on abs.ServiceId equals s.Id
									   where mr.Id == this.Id && mr.Start.Year >= CashForServicesStartYear && s.TypeId == (int)Service.ServiceTypes.Homecare
									   select new
									   {
										   clientId = c.Id,
										   israeliId = c.NationalId,
										   masterId = c.MasterId,
										   clientName = c.FirstName + " " + c.LastName,
										   serviceName = s.Name,
										   subReportId = cr.SubReportId,
										   agencyName = c.Agency.Name
									   }).Distinct().ToList();

						foreach (var c in clients)
						{
							for (int i = 0; i < duplicates.Count; i++)
							{
								if (c.masterId != null && (c.masterId == duplicates[i].masterId))
								{
									yield return new Row()
									{
										MessageTypeName = "Client already reported",
										Message = "CC ID " + c.clientId + " Name " + c.clientName + " duplicated client was already reported for cash on this period and therefore is not allowed to receive " + c.serviceName,
										ClientId = c.clientId,
										SubReportId = c.subReportId,
										AgencyName = c.agencyName
									};
								}
								else if (c.israeliId == duplicates[i].israeliId)
								{
									yield return new Row()
									{
										MessageTypeName = "Client already reported",
										Message = "CC ID " + c.clientId + " Name " + c.clientName + " was already reported for cash on this period and therefore is not allowed to receive " + c.serviceName,
										ClientId = c.clientId,
										SubReportId = c.subReportId,
										AgencyName = c.agencyName
									};
								}
							}
						}

					}

					else
					{
						yield return new Row() { Message = "A report on the same period from the Cash for Services system has not yet been approved." };
						yield break;
					}
				}
			}


			if (mainReport.AppBudget.App.AgencyGroupId == IsraelCashSerNumber && mainReport.Start.Year >= CashForServicesStartYear) //Israel Cash for Services
			{

				using (var db = new ccEntities(false, false))
				{
					var q = db.MainReports.Where(f => f.AppBudget.App.AgencyGroupId == IsraelKerenSerNumber)  //Benefits of Holocaust Victims in Israel SER (“The Keren”
						.Where(f => f.Start == mainReport.Start && f.StatusId == (int)MainReport.Statuses.Approved);
					if (q.Any())
					{
						var duplicates = (from mr in q
										  join cr in db.ClientReports on mr.Id equals cr.SubReport.MainReportId
										  join c in db.Clients on cr.ClientId equals c.Id
										  select new
										  {
											  clientId = c.Id,
											  israeliId = c.NationalId,
											  masterId = c.MasterId,
											  clientName = c.FirstName + " " + c.LastName
										  }).Distinct().ToList();
						var clients = (from mr in db.MainReports
									   join sr in db.SubReports on mr.Id equals sr.MainReportId
									   join cr in db.ClientReports on sr.Id equals cr.SubReportId
									   join c in db.Clients on cr.ClientId equals c.Id
									   join abs in db.AppBudgetServices on sr.AppBudgetServiceId equals abs.Id
									   join s in db.Services on abs.ServiceId equals s.Id
									   where mr.Id == this.Id && mr.Start.Year >= CashForServicesStartYear && s.TypeId == (int)Service.ServiceTypes.Homecare
									   select new
									   {
										   clientId = c.Id,
										   israeliId = c.NationalId,
										   masterId = c.MasterId,
										   clientName = c.FirstName + " " + c.LastName,
										   serviceName = s.Name,
										   subReportId = cr.SubReportId,
										   agencyName = c.Agency.Name
									   }).Distinct().ToList();

						foreach (var c in clients)
						{
							for (int i = 0; i < duplicates.Count; i++)
							{
								if (c.masterId != null && (c.masterId == duplicates[i].masterId))
								{
									yield return new Row()
									{
										MessageTypeName = "Client already reported",
										Message = "CC ID " + c.clientId + " Name " + c.clientName + " duplicated client was already reported by the Keren for this period." + c.serviceName,
										ClientId = c.clientId,
										SubReportId = c.subReportId,
										AgencyName = c.agencyName
									};
								}
								else if (c.israeliId == duplicates[i].israeliId)
								{
									yield return new Row()
									{
										MessageTypeName = "Client already reported",
										Message = "CC ID " + c.clientId + " Name " + c.clientName + " was already reported by the Keren for this period." + c.serviceName,
										ClientId = c.clientId,
										SubReportId = c.subReportId,
										AgencyName = c.agencyName
									};
								}
							}
						}

					}

				}





			}





			foreach (var r in ValidateApp(mainReport.Id)) { yield return r; }

			foreach (var r in ValidateAppServices(mainReport)) { yield return r; }

			foreach (var r in ValidateCcGrants(mainReport)) { yield return r; }

			foreach (var r in ValidateAgencyServices(mainReport)) { yield return r; }

			foreach (var r in ValidateSubReports(mainReport)) { yield return r; }

			foreach (var r in ValidateEligibilityPeriods(mainReport)) { yield return r; }

			foreach (var r in ValidateAustrianEligible(mainReport)) { yield return r; }

			foreach (var r in ValidateRomanianEligible(mainReport)) { yield return r; }

			foreach (var r in ValidateIncomeCriteriaComplied(mainReport)) { yield return r; }

			foreach (var r in ValidateJoinAndLeaveDates(mainReport.Id)) { yield return r; }

			//foreach (var r in ValidateClientApprovalStatus(mainReport)) { yield return r; }

			foreach (var r in ValidateAnnualHcAssessment(mainReport)) { yield return r; }

			foreach (var r in ValidateHcCaps(mainReport)) { yield return r; }

			foreach (var r in ValidateEmergencyCaps(mainReport)) { yield return r; }

			foreach (var r in ValidateAppBalance(mainReport)) { yield return r; }

			foreach (var r in ValidateRequiredFileds(mainReport.Id)) { yield return r; }

			foreach (var r in ValidateAdministrativeOverhead(mainReport)) { yield return r; }

			//foreach (var r in ValidateApprovedHomecareOnly(mainReport)) { yield return r; }

			foreach (var r in ValidateExceptionalHc(mainReport)) { yield return r; }

			foreach (var r in ValidateSC(mainReport)) { yield return r; }

			foreach (var r in ValidateDcc(mainReport)) { yield return r; }

			foreach (var r in ValidateSoupKitchens(mainReport)) { yield return r; }

			foreach (var r in ValidateClientsReuiredFields(mainReport)) { yield return r; }

			foreach (var r in ValidateMhmCaps(mainReport)) { yield return r; }

			foreach (var r in ValidateHospiceCareReports1(mainReport)) { yield return r; }
			foreach (var r in ValidateHospiceCare2(mainReport)) { yield return r; }
			foreach (var r in ValidateHospiceCareCaps(mainReport)) { yield return r; }
            foreach (var r in ValidateFuneralExpensesCaps(mainReport)) { yield return r; }
        }


		private IEnumerable<Row> ValidateSC(MainReport mainReport)
		{
			using (var db = new ccEntities())
			{
				var SC_min_day = System.Web.Configuration.WebConfigurationManager.AppSettings["SC_min_day"].Parse<int>() - 1;  //Max join date
				var SC_max_day = System.Web.Configuration.WebConfigurationManager.AppSettings["SC_max_day"].Parse<int>() - 1;  //Min leave date

				DateTime myStartDate = mainReport.Start;
				DateTime myEndDate = mainReport.End;

				DateTime firstMonthStart = mainReport.Start;
				DateTime secondMonthStart = mainReport.Start.AddMonths(1);
				DateTime thirdMonthStart = mainReport.Start.AddMonths(2);

				DateTime firstMonthMinLeave = firstMonthStart.AddDays((double)(SC_max_day - 1));
				DateTime secondMonthMinLeave = secondMonthStart.AddDays((double)(SC_max_day - 1));
				DateTime thirdMonthMinLeave = thirdMonthStart.AddDays((double)(SC_max_day - 1));

				DateTime firstMonthMaxJoin = firstMonthStart.AddDays((double)(SC_min_day - 1));
				DateTime secondMonthMaxJoin = secondMonthStart.AddDays((double)(SC_min_day - 1));
				DateTime thirdMonthMaxJoin = thirdMonthStart.AddDays((double)(SC_min_day - 1));

				var dod = from item in
							  (from sr in db.SubReports.Where(Permissions.SubReportsFilter)
                               where sr.MainReportId == mainReport.Id
                               from cr in sr.SupportiveCommunitiesReports
                               let c = cr.Client
                               let vcr = (from v in db.viewScRepSources
                                where v.clientid == cr.ClientId 
                                where v.subreportid == cr.SubReportId
                                select v.MonthsCount).FirstOrDefault()
                               let hcepg = db.HomeCareEntitledPeriods.Where(f => f.ClientId == c.Id && f.StartDate <= thirdMonthMinLeave && (f.EndDate == null || f.EndDate >= firstMonthMaxJoin))
                               select new
                              {
                                  AgencyId = sr.AppBudgetService.AgencyId,
                                  AgencyName = sr.AppBudgetService.Agency.Name,
                                  ClientId = cr.ClientId,
                                  ClientName = c.FirstName + " " + c.LastName,
                                  ServiceName = sr.AppBudgetService.Service.Name,
                                  ServiceTypeId = sr.AppBudgetService.Service.TypeId,
                                  ServiceTypeName = sr.AppBudgetService.Service.ServiceType.Name,
                                  SubReportId = sr.Id,
                                  DeceasedDate = c.DeceasedDate,
                                  LeaveDate = c.LeaveDate,
                                  JoinDate = c.JoinDate,
                                  ReportingMethodId = sr.AppBudgetService.Service.ReportingMethodId,
                                  ReportStart = sr.MainReport.Start,
                                  ReportEnd = sr.MainReport.End,
                                  HasHcep = hcepg.Any(),
                                  HoursHoldCost = cr.HoursHoldCost,
                                  MaxHoursHoldCost = db.ScSubsidyAmounts.OrderByDescending(f => f.StartDate).FirstOrDefault(f => f.LevelId == c.Agency.AgencyGroup.ScSubsidyLevelId).Amount,
                                  MonthsReported = cr.MonthsCount,
                                  MonthsAllowed = vcr ?? 0
                              })
						  select item;


                foreach (var c in dod.Where(c => c.LeaveDate < firstMonthMaxJoin))
                    {
                    
                        yield return new Row
                        {
                            AgencyId = c.AgencyId,
                            AgencyName = c.AgencyName,
                            ClientName = c.ClientName,
                            ServiceName = c.ServiceName,
                            ServiceTypeId = c.ServiceTypeId,
                            ServiceTypeName = c.ServiceTypeName,
                            SubReportId = c.SubReportId,
                            ReportingMethodId = c.ReportingMethodId,
                            MessageTypeName = "Client is no longer eligible (Leave Date)",
                            Message = string.Format("the client (CCID: {0}) can't appear in the report because his Leave Date is less than {1}.", c.ClientId, firstMonthMaxJoin)
                        };
                    }
                
				foreach (var c in dod.Where(c => c.JoinDate > thirdMonthMinLeave))
				{
					yield return new Row
					{
						AgencyId = c.AgencyId,
						AgencyName = c.AgencyName,
						ClientName = c.ClientName,
						ServiceName = c.ServiceName,
						ServiceTypeId = c.ServiceTypeId,
						ServiceTypeName = c.ServiceTypeName,
						SubReportId = c.SubReportId,
						ReportingMethodId = c.ReportingMethodId,
						MessageTypeName = "Client is not eligible (Join Date)",
						Message = string.Format("the client (CCID: {0}) can't appear in the report because his Join Date is more than {1}.", c.ClientId, thirdMonthMinLeave)
					};
				}

				foreach (var c in dod.Where(f => !f.HasHcep))
				{
					yield return new Row
					{
						AgencyId = c.AgencyId,
						AgencyName = c.AgencyName,
						ClientName = c.ClientName,
						ServiceName = c.ServiceName,
						ServiceTypeId = c.ServiceTypeId,
						ServiceTypeName = c.ServiceTypeName,
						SubReportId = c.SubReportId,
						ReportingMethodId = c.ReportingMethodId,
						MessageTypeName = "Client is not eligible (Eligibility)",
						Message = string.Format("the client (CCID: {0}) can't appear in the report because he doeasn't have eligibility in the period from {1} to {2}.", c.ClientId, firstMonthMaxJoin, thirdMonthMinLeave)
					};
				}

				//validate months reported
				foreach (var c in dod.Where(f => f.MonthsReported > f.MonthsAllowed))
				{
					yield return new Row
					{
						AgencyId = c.AgencyId,
						AgencyName = c.AgencyName,
						ClientName = c.ClientName,
						ServiceName = c.ServiceName,
						ServiceTypeId = c.ServiceTypeId,
						ServiceTypeName = c.ServiceTypeName,
						SubReportId = c.SubReportId,
						ReportingMethodId = c.ReportingMethodId,
						MessageTypeName = "Invalid Months Reported",
						Message = string.Format("Invalid number for months reported ({0}) for client {1}. The allowed months count is {2}", c.MonthsReported, c.ClientId, c.MonthsAllowed)
					};
				}
			}


		}


		private IEnumerable<Row> ValidateDcc(MainReport mainReport)
		{
			var dcc_visits_limit = System.Web.Configuration.WebConfigurationManager.AppSettings["DCC_visit_limit"].Parse<int>();
			using (var db = new ccEntities())
			{

				var q = from sr in db.SubReports.Where(this.Permissions.SubReportsFilter)
						where sr.MainReportId == mainReport.Id
						from dcc in sr.DaysCentersReports

						select new Row
						{
							AgencyId = dcc.Client.AgencyId,
							AgencyName = dcc.Client.Agency.Name,
							ClientId = dcc.ClientId,
							ClientName = dcc.Client.FirstName + " " + dcc.Client.LastName,

							ServiceName = sr.AppBudgetService.Service.Name,
							ServiceTypeId = sr.AppBudgetService.Service.TypeId,
							ServiceTypeName = sr.AppBudgetService.Service.ServiceType.Name,
							SubReportId = sr.Id,
							NationalId = dcc.Client.NationalId,
							ReportingMethodId = sr.AppBudgetService.Service.ReportingMethodId

						};


				var startMonth = mainReport.Start;
				for (int i = 0; i < 3; i++)
				{
					foreach (var r in q)
					{
						var q1 = from mv in db.DccMemberVisits

								 where mv.ReportDate.Year == mainReport.Start.Year
								 where mv.ReportDate.Month == mainReport.Start.Month + i
								 where mv.Client.NationalId == r.NationalId
								 select mv;

						var count = q1.Count();
						if (count > dcc_visits_limit)
						{
							string monthName = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(mainReport.Start.Month + i);

							r.MessageTypeName = string.Format("Client has more then {0} visits per months", dcc_visits_limit);
							r.Message = "CC National ID " + r.NationalId + " reported more then " + dcc_visits_limit + " visits in " + monthName + " " + mainReport.Start.Year;

							yield return r;
						}
					}
				}

				var dccNotEligibleJoinDate = (from mv in db.DccMemberVisits
											  join sr in db.SubReports on mv.SubReportId equals sr.Id
											  where sr.MainReportId == mainReport.Id
											  join c in db.Clients on mv.ClientId equals c.Id
											  where c.JoinDate > mv.ReportDate
											  select new Row
											  {
												  ClientId = c.Id,
												  ClientName = c.FirstName + " " + c.LastName,
												  Date = c.JoinDate
											  }).Distinct().ToList();
				foreach (var r in dccNotEligibleJoinDate)
				{
					r.MessageTypeName = "Client is not eligible (Join Date)";
					r.Message = string.Format("Client {0} (CCID: {1}) can not be reported before {2}",
						r.ClientName, r.ClientId, r.Date.Value.ToString("dd MMM yyyy"));
					yield return r;
				}

				var dccNotEligibleLeaveDate = (from mv in db.DccMemberVisits
											   join sr in db.SubReports on mv.SubReportId equals sr.Id
											   where sr.MainReportId == mainReport.Id
											   join c in db.Clients on mv.ClientId equals c.Id
											   where c.LeaveDate != null && c.LeaveDate < mv.ReportDate
											   select new Row
											   {
												   ClientId = c.Id,
												   ClientName = c.FirstName + " " + c.LastName,
												   Date = c.LeaveDate
											   }).Distinct().ToList();
				foreach (var r in dccNotEligibleLeaveDate)
				{
					r.MessageTypeName = "Client is not eligible (Leave Date)";
					r.Message = string.Format("Client {0} (CCID: {1}) can not be reported after {2}",
						r.ClientName, r.ClientId, r.Date.Value.ToString("dd MMM yyyy"));
					yield return r;
				}

                var dccNotEligibleStartDate = (from mv in db.DccMemberVisits
                                               join sr in db.SubReports on mv.SubReportId equals sr.Id
                                               where sr.MainReportId == mainReport.Id
                                               join c in db.Clients on mv.ClientId equals c.Id
                                               join h in  db.HomeCareEntitledPeriods on c.Id equals h.ClientId
                                               where h.EndDate != null && h.StartDate > mv.ReportDate
                                               select new Row
                                               {
                                                   ClientId = c.Id,
                                                   ClientName = c.FirstName + " " + c.LastName,
                                                   Date = h.StartDate
                                                  
                                               }).Distinct().ToList();
                foreach (var r in dccNotEligibleStartDate)
                {
                    r.MessageTypeName = "Client is not eligible (Start Date)";
                    r.Message = string.Format("Client {0} (CCID: {1}) can not be reported after {2} and should have no End Date",
                        r.ClientName, r.ClientId, r.Date.Value.ToString("dd MMM yyyy"));
                    yield return r;
                }
            }
		}


		private IEnumerable<Row> ValidateAustrianEligible(MainReport mainReport)
		{
			var t = DateTime.Now;
			using (var db = new ccEntities())
			{
				var austrianEligibleOnly = (from m in db.AppBudgets
											where m.Id == mainReport.AppBudgetId
											select m.App.Fund.AustrianEligibleOnly).FirstOrDefault();
				if (austrianEligibleOnly)
				{
					var q = from sr in db.SubReports.Where(this.Permissions.SubReportsFilter)
							where sr.MainReportId == mainReport.Id
							from er in sr.EmergencyReports
							where !er.Client.AustrianEligible
							select new Row
							{
								AgencyId = er.Client.AgencyId,
								AgencyName = er.Client.Agency.Name,
								ClientId = er.ClientId,
								ClientName = er.Client.FirstName + " " + er.Client.LastName,
								Date = er.ReportDate,
								ServiceName = sr.AppBudgetService.Service.Name,
								ServiceTypeId = sr.AppBudgetService.Service.TypeId,
								ServiceTypeName = sr.AppBudgetService.Service.ServiceType.Name,
								SubReportId = sr.Id,
								ReportingMethodId = sr.AppBudgetService.Service.ReportingMethodId
							};
					foreach (var r in q)
					{
						r.Message = string.Format("Client (CCID: {0}) can not appear in this report because he is not marked as Austrian Eligible", r.ClientId);
						r.MessageTypeName = "Client is no longer eligible (Austrian Eligibility)";
						yield return r;
					}
				}
			}
			_log.DebugFormat("Validation: finished {0} in {1}", System.Reflection.MethodBase.GetCurrentMethod().Name, (DateTime.Now - t).TotalMilliseconds);
		}



		private IEnumerable<Row> ValidateRomanianEligible(MainReport mainReport)
		{
			var t = DateTime.Now;
			using (var db = new ccEntities())
			{
				var RomanianEligibleOnly = (from m in db.AppBudgets
											where m.Id == mainReport.AppBudgetId
											select m.App.Fund.RomanianEligibleOnly).FirstOrDefault();
				if (RomanianEligibleOnly)
				{
					var q = from sr in db.SubReports.Where(this.Permissions.SubReportsFilter)
							where sr.MainReportId == mainReport.Id
							from er in sr.EmergencyReports
							where !er.Client.RomanianEligible
							select new Row
							{
								AgencyId = er.Client.AgencyId,
								AgencyName = er.Client.Agency.Name,
								ClientId = er.ClientId,
								ClientName = er.Client.FirstName + " " + er.Client.LastName,
								Date = er.ReportDate,
								ServiceName = sr.AppBudgetService.Service.Name,
								ServiceTypeId = sr.AppBudgetService.Service.TypeId,
								ServiceTypeName = sr.AppBudgetService.Service.ServiceType.Name,
								SubReportId = sr.Id,
								ReportingMethodId = sr.AppBudgetService.Service.ReportingMethodId
							};
					foreach (var r in q)
					{
						r.Message = string.Format("Client (CCID: {0} can not appear in this report because he is not marked as Romanian Eligible", r.ClientId);
						r.MessageTypeName = "Client is no longer eligible (Romanian Eligibility)";
						yield return r;
					}
				}
			}

			_log.DebugFormat("Validation: finished {0} in {1}", System.Reflection.MethodBase.GetCurrentMethod().Name, (DateTime.Now - t).TotalMilliseconds);
		}
		public IEnumerable<Row> ValidateExceptionalHc(MainReport mainreport)
		{
			var t = DateTime.Now;
			using (var db = new ccEntities())
			{
				var q =
						from sr in db.SubReports
						where sr.MainReportId == mainreport.Id
						where sr.AppBudgetService.Service.ExceptionalHomeCareHours
						where sr.AppBudgetService.Service.TypeId == (int)Service.ServiceTypes.Homecare
						from cr in sr.ClientReports.Distinct()


						select new Row
						{
							ClientId = cr.ClientId,
							AgencyId = sr.AppBudgetService.AgencyId,
							AgencyName = sr.AppBudgetService.Agency.Name,
							ClientName = cr.Client.FirstName + " " + cr.Client.LastName,
							ClientReportId = cr.Id,
							ServiceName = sr.AppBudgetService.Service.Name,
							ServiceTypeId = sr.AppBudgetService.Service.TypeId,
							ServiceTypeName = sr.AppBudgetService.Service.ServiceType.Name,
							SubReportId = sr.Id,
							ReportingMethodId = sr.AppBudgetService.Service.ReportingMethodId,
							Quantity = ccEntitiesExtensions.HcCap(cr.ClientId, mainreport.Start, mainreport.End)
						};
				foreach (var row in q.Where(f => f.Quantity == null))
				{
					row.MessageTypeName = "Homecare Exceptional Hours";
					row.Message = string.Format("No Homecare hours found (CCID: {0}). Client report is not valid.", row.ClientId);
					yield return row;
				}
			}
			_log.DebugFormat("Validation: finished {0} in {1}", System.Reflection.MethodBase.GetCurrentMethod().Name, (DateTime.Now - t).TotalMilliseconds);
		}

		public IEnumerable<Row> ValidateAgencyServices(MainReport mainreport)
		{
			var t = DateTime.Now;
			using (var db = new ccEntities())
			{
				//orphan services
				var q = db.SubReports
				.Where(f => f.MainReportId == mainreport.Id)
				.Where(f => !f.AppBudgetService.AppBudget.App.Services.Any(s => s.Id == f.AppBudgetService.ServiceId))
				.Select(f => new Row()
				{
					SubReportId = f.Id,
					ServiceName = f.AppBudgetService.Service.Name,
					AgencyId = f.AppBudgetService.Agency.Id,
					AgencyName = f.AppBudgetService.Agency.Name,
					ReportingMethodId = f.AppBudgetService.Service.ReportingMethodId
				}
				);



				foreach (var r in q)
				{
					r.MessageTypeName = "Service is no longer available";
					r.Message = string.Format("The Service ({0}) is no longer available for the Agency ({1})",
						r.ServiceName, r.AgencyName);
					yield return r;
				}


				//find orphan agencies
				var missingAgencies = db.SubReports.Where(f => f.MainReportId == mainreport.Id)
				.Where(f => f.AppBudgetService.Agency.GroupId != f.AppBudgetService.AppBudget.App.AgencyGroupId)
				.Select(f => new Row()
				{
					SubReportId = f.Id,
					AgencyId = f.AppBudgetService.Agency.Id,
					AgencyName = f.AppBudgetService.Agency.Name,
					ReportingMethodId = f.AppBudgetService.Service.ReportingMethodId
				});
				foreach (var r in missingAgencies)
				{
					r.MessageTypeName = "Agency is no longer available for this Ser";
					r.Message = string.Format("Agency: {0}", r.AgencyName);
					yield return r;
				}
			}
			_log.DebugFormat("Validation: finished {0} in {1}", System.Reflection.MethodBase.GetCurrentMethod().Name, (DateTime.Now - t).TotalMilliseconds);
		}

		public IEnumerable<Row> ValidateAppServices(MainReport mainreport)
		{
			var t = DateTime.Now;
			using (var db = new ccEntities())
			{
				var q = db.SubReports
				.Where(f => f.MainReportId == mainreport.Id)
				.Where(f => !f.AppBudgetService.AppBudget.App.Services.Any(s => s.Id == f.AppBudgetService.ServiceId))
				.Select(f => new
				{
					SubReportId = f.Id,
					ServiceName = f.AppBudgetService.Service.Name,
					AgencyId = f.AppBudgetService.Agency.Id,
					AgencyName = f.AppBudgetService.Agency.Name,
					AppName = f.AppBudgetService.AppBudget.App.Name,
					ReportingMethodId = f.AppBudgetService.Service.ReportingMethodId
				}
				);



				foreach (var r in q)
				{
					yield return new Row()
					{
						MessageTypeName = "Service is no longer available",
						SubReportId = r.SubReportId,
						AgencyName = r.AgencyName,
						ReportingMethodId = r.ReportingMethodId,
						Message = string.Format("The Service ({0}) is no longer available for the App ({1})",
							r.ServiceName, r.AppName)
					};

				}
			}
			_log.DebugFormat("Validation: finished {0} in {1}", System.Reflection.MethodBase.GetCurrentMethod().Name, (DateTime.Now - t).TotalMilliseconds);
		}

		//depricated
		public IEnumerable<Row> ValidateApprovedHomecareOnly(MainReport mainreport)
		{
			var t = DateTime.Now;
			using (var db = new ccEntities())
			{
				var q = from cr in db.ViewClientReports
						join s in db.SubReports on cr.SubReportId equals s.Id
						join c in db.Clients.Where(Permissions.ClientsFilter) on cr.ClientId equals c.Id
						where s.MainReportId == mainreport.Id
						where c.ApprovalStatusId == (int)ApprovalStatusEnum.ApprovedHomecareOnly
						where s.AppBudgetService.Service.TypeId != (int)Service.ServiceTypes.Homecare
						select new
						{
							SubReportId = s.Id,
							ClientId = c.Id,
							ReportingMethodId = s.AppBudgetService.Service.ReportingMethodId,
							ApprovalStatusUpdated = c.ApprovalStatusUpdated,
							StartDate = s.MainReport.Start,
							EndDate = s.MainReport.End,
							Remarks = cr.PurposeOfGrant
						};
				foreach (var item in q)
				{
					if (item.ApprovalStatusUpdated < item.StartDate)
					{
						yield return new Row()
						{
							ClientId = item.ClientId,
							SubReportId = item.SubReportId,
							ReportingMethodId = item.ReportingMethodId,
							MessageTypeName = "Approved Homecare Only",
							Message = string.Format("The Client ({0}) Approval Status is Approved - Homecare only or client approval status date is before report start", item.ClientId)
						};
					}
					else if (item.ApprovalStatusUpdated >= item.StartDate && item.ApprovalStatusUpdated < item.EndDate && (item.Remarks == null || item.Remarks == ""))
					{
						yield return new Row()
						{
							ClientId = item.ClientId,
							SubReportId = item.SubReportId,
							ReportingMethodId = item.ReportingMethodId,
							MessageTypeName = "Approved Homecare Only",
							Message = string.Format("The Client ({0}) Approval Status is Approved - Homecare only during this reporting period. Please enter unique circumastances", item.ClientId)
						};
					}
				}
			}
			_log.DebugFormat("Validation: finished {0} in {1}", System.Reflection.MethodBase.GetCurrentMethod().Name, (DateTime.Now - t).TotalMilliseconds);
		}

		/// <summary>
		/// This one checks all min/max percentage set on the servicetype/service level
		/// </summary>
		/// <param name="mainreport"></param>
		/// <returns></returns>
		public IEnumerable<Row> ValidateAdministrativeOverhead(MainReport mainreport)
		{
			var t = DateTime.Now;
			using (var db = new ccEntities())
			{
				bool? eoy = null;
				if (mainreport.AppBudget.App.EndOfYearValidationOnly)
				{
					var reportingPeriod = mainreport.End.Month - mainreport.Start.Month;
					eoy = (mainreport.Start.Month + reportingPeriod) == 13;
				}
				//app not marked as eoy or this is last report in the year
				if (eoy == null || eoy == true)
				{
					var serviceAmounts =
										  from appbs in db.AppBudgetServices
										  where appbs.AppBudgetId == mainreport.AppBudgetId
										  join sra in db.viewSubreportAmounts on appbs.Id equals sra.AppBudgetServiceId
										  join mr in db.MainReports.Where(MainReport.CurrentOrSubmitted(mainreport.Id)) on sra.MainReportId equals mr.Id
										  where mr.Start <= mainreport.Start
										  group sra by new { ServiceId = appbs.ServiceId, ServiceTypeId = appbs.Service.TypeId } into srag

										  select new
										  {
											  ServiceId = srag.Key.ServiceId,
											  ServiceTypeId = srag.Key.ServiceTypeId,
											  Amount = srag.Sum(f => f.Amount)
										  };

					var h = db.MainReports.Where(f => f.Id == mainreport.Id).Select(f => new
					{
						AppId = f.AppBudget.AppId,
						FundName = f.AppBudget.App.Fund.Name,
						AppCur = f.AppBudget.App.CurrencyId
					}).SingleOrDefault();

					var totalAmount = serviceAmounts.Sum(f => f.Amount);

					var servicesCount = (from a in db.AppBudgets
										 where a.Id == mainreport.AppBudgetId
										 from s in a.App.Services
										 select s.Id).Count();

					if (servicesCount > 1)
					{

						var q = from c in Queries.GetServiceConstraints(db, h.AppId)
								join s in db.Services on c.ServiceId equals s.Id
								join a in
									(from subA in serviceAmounts
									 group subA by subA.ServiceId into appBudgetServiceGroup
									 select new
									 {
										 ServiceId = appBudgetServiceGroup.Key,
										 Amount = (decimal?)appBudgetServiceGroup.Sum(f => f.Amount) ?? 0
									 }) on c.ServiceId equals a.ServiceId into ag
								from a in ag.DefaultIfEmpty()
								select new
								{
									ServiceId = c.ServiceId,
									Name = s.Name,
									Max = c.MaxExpPercentage,
									Min = c.MinExpPercentage,
									Amount = ((decimal?)a.Amount) ?? 0,
								};

						foreach (var item in q.Where(f => EntityFunctions.Truncate(f.Amount - (totalAmount * f.Min), CCDecimals.CompareDigits) < 0))
						{
							var currentPercentage = item.Amount / totalAmount;
							yield return new Row()
							{
								MessageTypeName = string.Format("{0} minimum percentage {1} per report not met.", item.Name, item.Min.HasValue ? item.Min.Value.FormatPercentage() : null),
								Message = string.Format("YTD {0}  percentage is {1} while {0} minimum percentage per report is {2} for fund {7}. Other details: YTD {0} amount: {3} {4}, YTD Total Report amount: {5} {6}.",
									item.Name,
									currentPercentage.FormatPercentage(),
									item.Min.HasValue ? item.Min.Value.FormatPercentage() : null,
									 item.Amount.Format(),
									h.AppCur,
									totalAmount.Format(),
									h.AppCur,
									h.AppCur)
							};
						}
						foreach (var item in q.Where(f => EntityFunctions.Truncate(f.Amount - totalAmount * f.Max, CCDecimals.CompareDigits) > 0))
						{
							var currentPercentage = item.Amount / totalAmount;
							yield return new Row()
							{
								MessageTypeName = string.Format("{0} maximum percentage {1} per report not met.", item.Name, item.Max.HasValue ? item.Max.Value.FormatPercentage() : null),
								Message = string.Format("YTD {0}  percentage is {1} while {0} maximum percentage per report is {2} for fund {7}. Other details: YTD {0} amount: {3} {4}, YTD Total Report amount: {5} {6}.",
									item.Name,
									currentPercentage.FormatPercentage(),
									item.Max.HasValue ? item.Max.Value.FormatPercentage() : null,
									item.Amount.Format(),
									h.AppCur,
									totalAmount.Format(),
									h.AppCur,
									h.FundName)
							};

						}
					}

					var ytdServiceTypesQuery = (from a in
													(from appb in db.AppBudgets
													 where appb.Id == this.AppBudgetId
													 from appbs in appb.AppBudgetServices
													 join sra in db.viewSubreportAmounts on appbs.Id equals sra.AppBudgetServiceId
													 join mr in db.MainReports.Where(MainReport.CurrentOrSubmitted(Id)) on sra.MainReportId equals mr.Id
													 group sra by appbs.Service.TypeId into srag
													 select new
													 {
														 ServiceTypeId = srag.Key,
														 Amount = srag.Sum(f => f.Amount)
													 })
												join st in db.ServiceTypes on a.ServiceTypeId equals st.Id
												select new
												{
													ServiceTypeId = st.Id,
													ServiceTypeName = st.Name,
													Amount = a.Amount
												}).ToList();


					decimal total = ytdServiceTypesQuery != null ? ytdServiceTypesQuery.Sum(f => f.Amount) : 0;

					var HcCcGrant = ytdServiceTypesQuery.Where(f => f.ServiceTypeId == (int)Service.ServiceTypes.Homecare) != null ? ytdServiceTypesQuery.Where(f => f.ServiceTypeId == (int)Service.ServiceTypes.Homecare).Sum(f => f.Amount) : 0;
					var AdminCcGrant = ytdServiceTypesQuery.Where(f => f.ServiceTypeId == (int)Service.ServiceTypes.AdministrativeOverhead) != null ?
								ytdServiceTypesQuery.Where(f => f.ServiceTypeId == (int)Service.ServiceTypes.AdministrativeOverhead).Sum(f => f.Amount) : 0;
					var OtherGrant = ytdServiceTypesQuery.Where(f => f.ServiceTypeId != (int)Service.ServiceTypes.Homecare && f.ServiceTypeId != (int)Service.ServiceTypes.AdministrativeOverhead) != null ?
						ytdServiceTypesQuery.Where(f => f.ServiceTypeId != (int)Service.ServiceTypes.Homecare && f.ServiceTypeId != (int)Service.ServiceTypes.AdministrativeOverhead).Sum(f => f.Amount) : 0;

					var AdminPercentage = total != 0 ? AdminCcGrant / total : 0;
					var HcPercentage = total != 0 ? HcCcGrant / total : 0;
					var OtherPercentage = OtherGrant > 0 ? 1 - HcPercentage - AdminPercentage : 0;

					var p = (from a in db.Apps.Where(f => f.Id == h.AppId)
							 join fund in db.Funds on a.FundId equals fund.Id
							 select new
							 {
								 OtherServicesMax = (a.OtherServicesMax ?? fund.OtherServicesMax) / 100,
								 HomecareMin = (a.HomecareMin ?? fund.HomecareMin) / 100,
								 AdminMax = (a.AdminMax ?? fund.AdminMax) / 100,
								 a.MaxNonHcAmount
							 }).Single();
					if (p.MaxNonHcAmount == null && p.AdminMax.HasValue && AdminPercentage > p.AdminMax)
					{
						yield return new Row()
						{
							MessageTypeName = string.Format("Administrative Overhead maximum percentage {0} per report not met.",
							(p.AdminMax.Value).FormatPercentage()),
							Message = string.Format("YTD Administrative Overhead percentage is {0} while Administrative Overhead maximum percentage per report is {1} for fund {6}. Other details: YTD Administrative Overhead amount: {2} {3}, Total Report amount: {4} {5}.",
									AdminPercentage.FormatPercentage(),
									(p.AdminMax.Value).FormatPercentage(),
									AdminCcGrant.Format(),
									h.AppCur,
									totalAmount.Format(),
									h.AppCur,
									h.FundName)
						};
					}
					if (p.HomecareMin.HasValue && HcPercentage < p.HomecareMin)
					{
						yield return new Row()
						{
							MessageTypeName = string.Format("Homecare minimum percentage {0} per report not met.",
							(p.HomecareMin.Value).FormatPercentage()),
							Message = string.Format("YTD Homecare percentage is {0} while Homecare minimum percentage per report is {1} for fund {6}. Other details: YTD Homecare amount: {2} {3}, Total Report amount: {4} {5}.",
									HcPercentage.FormatPercentage(),
									(p.HomecareMin.Value).FormatPercentage(),
									HcCcGrant.Format(),
									h.AppCur,
									totalAmount.Format(),
									h.AppCur,
									h.FundName)
						};
					}
					if (p.OtherServicesMax.HasValue && OtherPercentage > p.OtherServicesMax)
					{
						yield return new Row()
						{
							MessageTypeName = string.Format("Other maximum percentage {0} per report not met.",
							(p.OtherServicesMax.Value).FormatPercentage()),
							Message = string.Format("YTD Other Services percentage is {0} while Other Services maximum percentage per report is {1} for fund {6}. Other details: YTD Other Services amount: {2} {3}, Total Report amount: {4} {5}.",
									OtherPercentage.FormatPercentage(),
									(p.OtherServicesMax.Value).FormatPercentage(),
									OtherGrant.Format(),
									h.AppCur,
									totalAmount.Format(),
									h.AppCur,
									h.FundName)
						};
					}
					
				}
			}
			_log.DebugFormat("Validation: finished {0} in {1}", System.Reflection.MethodBase.GetCurrentMethod().Name, (DateTime.Now - t).TotalMilliseconds);
		}
		/// <summary>
		/// main reports is created for ser1 but the budget was approved for ser2
		/// should be moved to the ivalidate
		/// </summary>
		/// <param name="mainReportId"></param>
		/// <returns></returns>
		public IEnumerable<Row> ValidateApp(int mainReportId)
		{
			var t = DateTime.Now;
			using (var db = new ccEntities())
			{
				var appsConfuse = db.SubReports.Where(sr => sr.MainReportId == mainReportId && sr.AppBudgetService.AppBudgetId != sr.MainReport.AppBudgetId).Select(f => new Row()
				{
					AgencyId = f.AppBudgetService.AgencyId,
					AgencyName = f.AppBudgetService.Agency.Name,

				});

				if (appsConfuse.Any())
				{
					foreach (var c in appsConfuse)
					{
						c.Message = "mismatch of main budget's app and approved agency service's app...";
						yield return c;
					}

				}

			}
			_log.DebugFormat("Validation: finished {0} in {1}", System.Reflection.MethodBase.GetCurrentMethod().Name, (DateTime.Now - t).TotalMilliseconds);

		}
		public IEnumerable<Row> ValidateIncomeCriteriaComplied(MainReport mainReport)
		{
			var t = DateTime.Now;
			using (var db = new ccEntities())
			{
				var q = (from cr in db.ViewClientReports
						group cr by new { ClientId = cr.ClientId, SubReportId = cr.SubReportId } into crg
						join sr in db.SubReports.Where(Permissions.SubReportsFilter) on crg.Key.SubReportId equals sr.Id
						where sr.MainReportId == mainReport.Id
						join c in db.Clients.Where(Client.IncomeVerificationRuiredExpression) on crg.Key.ClientId equals c.Id
						where !c.IncomeCriteriaComplied
						select new Row()
						{
							AgencyId = c.Agency.Id,
							AgencyName = c.Agency.Name,
							ClientId = c.Id,
							ClientName = c.FirstName + " " + c.LastName,
							ServiceName = sr.AppBudgetService.Service.Name,
							ServiceTypeId = sr.AppBudgetService.Service.ServiceType.Id,
							ServiceTypeName = sr.AppBudgetService.Service.ServiceType.Name,
							SubReportId = crg.Key.SubReportId,
							ReportingMethodId = sr.AppBudgetService.Service.ReportingMethodId
						}).Distinct();

				foreach (var r in q)
				{
					yield return new Row()
					{
						AgencyId = r.AgencyId,
						AgencyName = r.AgencyName,
						ClientId = r.ClientId,
						ClientName = r.ClientName,
						ClientReportId = r.ClientReportId,
						MessageTypeName = "Income criteria not met",
						Message = string.Format("The client {0} (ccid: {1}) \"Income Criteria Complied\" is not checked.", r.ClientName, r.ClientId),
						ServiceName = r.ServiceName,
						ServiceTypeId = r.ServiceTypeId,
						ServiceTypeName = r.ServiceTypeName,
						SubReportId = r.SubReportId,
						ReportingMethodId = r.ReportingMethodId

					};
				}
			}
			_log.DebugFormat("Validation: finished {0} in {1}", System.Reflection.MethodBase.GetCurrentMethod().Name, (DateTime.Now - t).TotalMilliseconds);
		}
		/// <summary>
		/// "1.4.15.3.4.        Leave Date- Clients that left the agency could not be reported for hours in months after the month of their leave date."
		/// </summary>
		/// <param name="mainreportid"></param>
		/// <returns></returns>
		public IEnumerable<Row> ValidateHcLeaveDate(int mainreportid)
		{
			var t = DateTime.Now;
			using (var db = new ccEntities())
			{
				var alreadyleaved = db.ClientAmountReports.Where(cr => cr.ClientReport.SubReport.MainReportId == mainreportid)
				.Where(cr => cr.ClientReport.Client.LeaveDate > System.Data.Objects.EntityFunctions.AddMonths(cr.ReportDate, 1)).Select(f => new Row()
				{
					AgencyId = f.ClientReport.SubReport.AppBudgetService.AgencyId,
					AgencyName = f.ClientReport.SubReport.AppBudgetService.Agency.Name,
					ClientId = f.ClientReport.ClientId,
					ClientName = f.ClientReport.Client.FirstName + " " + f.ClientReport.Client.LastName,
					ClientReportId = f.Id,
					ServiceName = f.ClientReport.SubReport.AppBudgetService.Service.Name,
					ServiceTypeId = f.ClientReport.SubReport.AppBudgetService.Service.TypeId,
					ServiceTypeName = f.ClientReport.SubReport.AppBudgetService.Service.ServiceType.Name
				});
				if (alreadyleaved.Any())
				{
					foreach (var c in alreadyleaved)
					{
						c.MessageTypeName = "Client is no longer eligible (Leave Date)";
						c.Message = string.Format("the client (CCID: {0}) can't appear in the report because he has left the agency before main report's start date.", c.ClientId);
						yield return c;
					}

				}

			}


			_log.DebugFormat("Validation: finished {0} in {1}", System.Reflection.MethodBase.GetCurrentMethod().Name, (DateTime.Now - t).TotalMilliseconds);
		}

		/// <summary>
		/// Validates Join, Leave and Deceased dates against report period
		/// </summary>
		/// <param name="mainReportId"></param>
		/// <returns></returns>
		public IEnumerable<Row> ValidateJoinAndLeaveDates(int mainReportId)
		{
			var t = DateTime.Now;
			using (var db = new ccEntities())
			{
				var dod = from item in
							  (from cr in db.ViewClientReports
							   join sr in db.SubReports.Where(Permissions.SubReportsFilter) on cr.SubReportId equals sr.Id
							   join c in db.Clients.Where(Permissions.ClientsFilter) on cr.ClientId equals c.Id
							   join eap in db.EmergencyReports on new { cr.SubReportId, cr.ClientId } equals new { eap.SubReportId, eap.ClientId } into eapg
							   from eap in eapg.DefaultIfEmpty()
							   where sr.MainReportId == mainReportId
							   where eap == null || eap.ReportDate == cr.ReportDate
							   select new
							   {
								   AgencyId = sr.AppBudgetService.AgencyId,
								   AgencyName = sr.AppBudgetService.Agency.Name,
								   ClientId = cr.ClientId,
								   ClientName = c.FirstName + " " + c.LastName,
								   ServiceName = sr.AppBudgetService.Service.Name,
								   ServiceTypeId = sr.AppBudgetService.Service.TypeId,
								   ServiceTypeName = sr.AppBudgetService.Service.ServiceType.Name,
								   SubReportId = sr.Id,
								   DeceasedDate = c.DeceasedDate,
								   LeaveDate = c.LeaveDate,
								   JoinDate = c.JoinDate,
								   Remarks = cr.PurposeOfGrant,
								   UniqueCircumstances = eap != null ? eap.UniqueCircumstances : "",
								   SkipDodCheck = (c.AustrianEligible || c.RomanianEligible) && sr.AppBudgetService.Service.TypeId == (int)Service.ServiceTypes.EmergencyAssistance
													|| sr.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.DayCenters && !db.DccMemberVisits.Any(f => f.ClientId == cr.ClientId && f.SubReportId == sr.Id),
								   ReportingMethodId = sr.AppBudgetService.Service.ReportingMethodId,
								   ReportStart = cr.ReportDate ?? sr.MainReport.Start,
								   ReportEnd = (sr.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.Homecare ? ccEntitiesExtensions.AddMonths(cr.ReportDate, 1) :
												sr.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.HomecareWeekly ? ccEntitiesExtensions.AddDays(c.DeceasedDate ?? cr.ReportDate, 28) :
											   sr.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.Emergency ? ccEntitiesExtensions.AddDays(cr.ReportDate, 1) :
											   sr.MainReport.End
											   ) ?? sr.MainReport.End
							   })
						  select item;




				//non homecare with date - joindate < rep date < leavedate

				//reps without report date - join date < mrend leave date > mrstart


				foreach (var c in dod.Where(c => c.JoinDate > c.ReportEnd))
				{
					yield return new Row
					{
						AgencyId = c.AgencyId,
						AgencyName = c.AgencyName,
						ClientName = c.ClientName,
						ServiceName = c.ServiceName,
						ServiceTypeId = c.ServiceTypeId,
						ServiceTypeName = c.ServiceTypeName,
						SubReportId = c.SubReportId,
						ReportingMethodId = c.ReportingMethodId,
						MessageTypeName = "Client is no longer eligible",
						Message = string.Format("Client (CCID: {0}) can't appear in the report because he has joined the agency after {1}.",
							c.ClientId,
							c.ReportEnd.AddDays(-1).ToString()
						),
					};
				}
				//left before report date/mr start
				foreach (var c in dod.Where(c => c.DeceasedDate == null && c.LeaveDate < c.ReportStart && c.ServiceTypeId !=6))
				{
					yield return new Row
					{
						AgencyId = c.AgencyId,
						AgencyName = c.AgencyName,
						ClientName = c.ClientName,
						ServiceName = c.ServiceName,
						ServiceTypeId = c.ServiceTypeId,
						ServiceTypeName = c.ServiceTypeName,
						SubReportId = c.SubReportId,
						ReportingMethodId = c.ReportingMethodId,
						MessageTypeName = "Client is no longer eligible (Leave Date)",
						Message = string.Format("the client (CCID: {0}) can't appear in the report because his Leave Date is less than {1}.", c.ClientId, c.ReportStart)
					};
				}

				//have a deceased date and is before report date /mr start
				foreach (var c in dod.Where(c => c.DeceasedDate != null && !(c.SkipDodCheck)
					&& (System.Data.Objects.EntityFunctions.AddDays(c.DeceasedDate, c.ServiceTypeId == (int)Service.ServiceTypes.EmergencyAssistance ? SubReport.EAPDeceasedDaysOverhead : 0) < c.ReportStart)))
				{
					bool canBeReported = false;
					//for homecare allowed to report for the next calendaric month after decease date
					if (c.ServiceTypeId == 8) //nursing home
					{
						var leaveDate = ((DateTime)c.DeceasedDate).AddMonths(1);
						var daysInMonth = DateTime.DaysInMonth(leaveDate.Year, leaveDate.Month);
						leaveDate = leaveDate.AddDays(daysInMonth - leaveDate.Day);
						if (leaveDate >= c.ReportStart)
						{
							canBeReported = true;
						}
					}

					if (!canBeReported)
					{
						yield return new Row
						{
							AgencyId = c.AgencyId,
							AgencyName = c.AgencyName,
							ClientName = c.ClientName,
							ServiceName = c.ServiceName,
							ServiceTypeId = c.ServiceTypeId,
							ServiceTypeName = c.ServiceTypeName,
							SubReportId = c.SubReportId,
							ReportingMethodId = c.ReportingMethodId,
							MessageTypeName = "Client is no longer eligible (DOD)",
							Message = string.Format("the client (CCID: {0}) can't appear in the report because hes DOD is less than {1}.", c.ClientId, c.ReportStart)
						};
					}
				}
				//is ok by the dod but invalid on ragular dod + no remarks on the client report
				foreach (var c in dod.Where(f => f.DeceasedDate != null && f.DeceasedDate < f.ReportStart && ((f.Remarks == null || f.Remarks == "")
						|| f.ServiceTypeId == (int)Service.ServiceTypes.EmergencyAssistance && (f.UniqueCircumstances == null || f.UniqueCircumstances == ""))))
				{
					yield return new Row
					{
						AgencyId = c.AgencyId,
						AgencyName = c.AgencyName,
						ClientName = c.ClientName,
						ServiceName = c.ServiceName,
						ServiceTypeId = c.ServiceTypeId,
						ServiceTypeName = c.ServiceTypeName,
						SubReportId = c.SubReportId,
						ReportingMethodId = c.ReportingMethodId,
						MessageTypeName = "Client is no longer eligible (DOD)",
						Message = string.Format("Deceased Client CCID {0} needs to be specified with Unique Circumstances.", c.ClientId)
					};
				}


			}
			_log.DebugFormat("Validation: finished {0} in {1}", System.Reflection.MethodBase.GetCurrentMethod().Name, (DateTime.Now - t).TotalMilliseconds);
		}
		/// <summary>
		/// For service reporting types - total, yes/no - clients can be modified to Yes/No without any amounts being entered at the top	Either create a warning message that clients have been modified but no amount entered or disable the Modify unless an amount is entered	3.12		tabFB21	should have a warning message in the sub report "Warning: Please specify an amount". Should be a new validation on the submission screen with message saying "Service <Service Name> was reported for clients, but with an amount, please specify an amount"
		/// </summary>
		/// <param name="mainreport"></param>
		/// <returns></returns>
		public IEnumerable<Row> ValidateSubReports(MainReport mainreport)
		{
			var t = DateTime.Now;
			using (var db = new ccEntities())
			{
				var q = from f in db.SubReports.Where(this.Permissions.SubReportsFilter)
                        where f.MainReportId == mainreport.Id
						where
							f.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.ClientUnit ||
							f.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.TotalCostNoNames ||
							f.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.TotalCostWithListOfClientNames ||
							f.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.SoupKitchens
                        join cr in db.ClientReports on f.Id equals cr.SubReportId
                        select new
						{
							ReportingMethodId = f.AppBudgetService.Service.ReportingMethodId,
							AgencyId = f.AppBudgetService.Agency.Id,
							AgencyName = f.AppBudgetService.Agency.Name,
							ServiceName = f.AppBudgetService.Service.Name,
							ServiceTypeId = f.AppBudgetService.Service.TypeId,
							ServiceTypeName = f.AppBudgetService.Service.ServiceType.Name,
							SubReportId = f.Id,
							Amount = f.Amount,
                            FEAmount= cr.Amount,
                           	AnyClients = f.ClientReports.Any() || f.SoupKitchensReports.Any(),
							AnyAmount = (f.Amount ?? 0) + (f.MatchingSum ?? 0) + (f.AgencyContribution ?? 0) + (cr.Amount?? 0)
                        };

				var reportsWithClients = q.Where(f => f.ReportingMethodId == (int)Service.ReportingMethods.ClientUnit
					|| f.ReportingMethodId == (int)Service.ReportingMethods.TotalCostWithListOfClientNames
					|| f.ReportingMethodId == (int)Service.ReportingMethods.SoupKitchens
					);

                var FEAmount = from sr in db.SubReports
                               where sr.MainReportId == mainreport.Id
                               join cr in db.ClientReports on sr.Id equals cr.SubReportId into fe
                               select new
                               {
                                   SumFEAmount = fe.Sum(f => f.Amount)
                                   
                               };
            

                var clientsNoAmount = reportsWithClients.Where(f => f.Amount > 0 && !f.AnyClients);
				foreach (var item in clientsNoAmount)
				{
					yield return new Row
					{
						AgencyId = item.AgencyId,
						AgencyName = item.AgencyName,
						ServiceName = item.ServiceName,
						ServiceTypeId = item.ServiceTypeId,
						ServiceTypeName = item.ServiceTypeName,
						SubReportId = item.SubReportId,
						ReportingMethodId = item.ReportingMethodId,
                        MessageTypeName = "No Clients Submitted",
						Message = "You are required to submit names of clients when reporting on this service."
					};
				}
                //Lena Kunisky - this error the error of needing an amount which is only supposed to apply to Funeral Expenses 
                //	var clientsRequired = reportsWithClients.Where(f => f.AnyClients && 
                //  (f.AnyAmount == 0 && f.ReportingMethodId != (int)Service.ReportingMethods.SoupKitchens || 
                //   f.ReportingMethodId == (int)Service.ReportingMethods.SoupKitchens && (!f.Amount.HasValue || f.Amount.Value == 0) 
                //   ));
                var clientsRequired = reportsWithClients.Where(f => f.AnyClients && f.ServiceName == "Funeral Expenses" && (!f.FEAmount.HasValue || f.FEAmount.Value == 0));


                    foreach (var item in clientsRequired)
				{
                   // if (item.ServiceName == "Funeral Expenses")
                    //{
                        string msg = "You are required to submit a value for at least one of these fields: Total Amount, Matching Sum or Agency Contribution.";
                   
					if (item.ReportingMethodId == (int)Service.ReportingMethods.SoupKitchens && (!item.Amount.HasValue || item.Amount.Value == 0))
					{
						msg = "You are required to submit a value for the Total Amount.";
					}
                    
                        yield return new Row
                        {
                            AgencyId = item.AgencyId,
                            AgencyName = item.AgencyName,
                            ServiceName = item.ServiceName,
                            ServiceTypeId = item.ServiceTypeId,
                            ServiceTypeName = item.ServiceTypeName,
                            SubReportId = item.SubReportId,
                            ReportingMethodId = item.ReportingMethodId,
                            MessageTypeName = "Please specify an amount",
                            Message = msg
                        };
                   // }
				}

				var negativeAmounts = reportsWithClients.Where(f => f.Amount < 0);
				foreach (var item in negativeAmounts)
				{
					yield return new Row
					{
						AgencyId = item.AgencyId,
						AgencyName = item.AgencyName,
						ServiceName = item.ServiceName,
						ServiceTypeId = item.ServiceTypeId,
						ServiceTypeName = item.ServiceTypeName,
						SubReportId = item.SubReportId,
						ReportingMethodId = item.ReportingMethodId,
						MessageTypeName = "Negative amount",
						Message = "The amount can not be negative."
					};
				}
			}

		}
		/// <summary>
		/// bulk hc caps check
		/// </summary>
		/// <remarks>
		///		1.4.15.3.5.        Allowed Hours per month:
		///		1.4.15.3.5.1.        Allowed hours per month are calculated per each month, based  on the sum of   (<Daily Cap> * < Month Days>) , where:
		///		1.4.15.3.5.1.1.        Daily Cap is defined as the weekly cap divided by 7.
		///		1.4.15.3.5.1.2.        Weekly Cap is the maximum value of either :
		///		(1)        The allowed hours for the related Functionality Level based on the Diagnostic Score that was up to date at the related report period. This calculation is based on the Diagnostic Score history, and according to related allowed hours in the Functionality Level table in the related period. 
		///		(2)        The Disability Override value.
		///		1.4.15.3.5.1.3.        Month Days: Are defined as the total number of days in the related month where the Diagnostic Score was the same (according to Diagnostic Score history).
		///		1.4.15.3.5.1.4.        Sub periods of scores within a single month: In cases where the Diagnostic Score and/ or the Functionality Level hours changes during the month, the allowed hours per month value will be the sum of each sub periods allowed hours. Where a sub period allowed hours is the sub period length (days) multiplied by the weekly cap of this sub period (as described above) divided by 7.
		/// </remarks>
		/// <param name="mainReport"></param>
		/// <returns></returns>
		public IEnumerable<Row> ValidateHcCaps(MainReport mainReport)
		{
			var t = DateTime.Now;
			//#warning unittests
			using (var db = new ccEntities())
			{
				db.CommandTimeout = 180;

				//validate monthly Homecare caps
				var spresult = db.spValidateMrHc(mainReport.Id);

				foreach (var item in spresult)
				{
					yield return new Row()
					{
						ClientId = (item.clientid ?? 0),
						MessageTypeName = "Homecare Cap exceeded",
						Date = item.reportdate,
						Message = string.Format("CCID: {5}, Month: {0}, Amount Reported:{1} hours, Total Cap: {2} hours (Month's cap: {3} hours, balancing cap: {4} hours)"
										, item.reportdate.Value.ToMonthString()
										, item.q.Format()
										, (item.q + item.co).Format()
										, item.c.Format()
										, (item.q + item.co - item.c).Format()
										, item.clientid
										)
					};
				}

				var selectedDOW = GlobalHelper.GetWeekStartDay(mainReport, db);
				DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
				var dayToSubstruct = (int)selectedDOW + 1;
				dfi.FirstDayOfWeek = (DayOfWeek)selectedDOW;
				Calendar cal = dfi.Calendar;

				var digits = CCDecimals.GetDecimalDigits();

				var allWeeklySubReports = from mr in db.MainReports
										  from sr in mr.SubReports
										  where sr.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.HomecareWeekly
										  where sr.AppBudgetService.Service.TypeId != (int)Service.ServiceTypes.HospiseCare
										  let w = dayToSubstruct
										  select new
										  {
											  SubReportId = sr.Id,
											  MrId = mr.Id,
											  MrStart = mr.Start,
											  MrEnd = mr.End,
											  MrStatusId = mr.StatusId,
											  WeekStartDay = w,
											  ExceptionalHomeCareHours = sr.AppBudgetService.Service.ExceptionalHomeCareHours,
											  CoPGovHoursValidation = sr.AppBudgetService.Service.CoPGovHoursValidation
										  };

				/**** Weekly validation without exceptional hours ****/
				var weeklySubReports = from item in allWeeklySubReports
									   select new
									   {
										   SubReportId = item.SubReportId,
										   MrId = item.MrId,
										   MrStart = item.MrStart,
										   MrEnd = item.MrEnd,
										   MrStatusId = item.MrStatusId,
										   WeekStartDay = item.WeekStartDay,
										   ExceptionalHomeCareHours = item.ExceptionalHomeCareHours,
										   CoPGovHoursValidation = item.CoPGovHoursValidation
									   };
				var quantities = from sr in weeklySubReports
								 join cr in db.ClientReports on sr.SubReportId equals cr.SubReportId
								 from ar in cr.ClientAmountReports
								 let wd = SqlFunctions.DatePart("weekday", ar.ReportDate)
								 let ws = sr.WeekStartDay
								 select new
								 {

									 sr.SubReportId,
									 sr.MrId,
									 sr.MrStart,
									 sr.MrEnd,
									 sr.MrStatusId,
									 cr.ClientId,
									 cr.Client.MasterIdClcd,
									 ar.Quantity,
									 ar.ReportDate,
									 WeekStart = wd < ws ?
										SqlFunctions.DateAdd("day", ws - wd - 7, ar.ReportDate) :
										SqlFunctions.DateAdd("day", ws - wd, ar.ReportDate),
									 sr.ExceptionalHomeCareHours,
									 sr.CoPGovHoursValidation
								 };
				var q1 = from current in
							 (from a in quantities
							  where a.MrId == mainReport.Id && !a.ExceptionalHomeCareHours
							  group a by new { a.ClientId, a.MasterIdClcd, a.WeekStart } into g
							  select new
							  {
								  g.Key.ClientId,
								  g.Key.MasterIdClcd,
								  g.Key.WeekStart,
								  WeekEnd = SqlFunctions.DateAdd("week", 1, g.Key.WeekStart),
								  Quantity = g.Sum(f => f.Quantity)
							  })
						 let submitted = (from a in quantities
										  where a.MrId != mainReport.Id && !a.ExceptionalHomeCareHours
										  where a.MrStatusId == (int)MainReport.Statuses.Approved
														  || a.MrId == (int)MainReport.Statuses.AwaitingProgramAssistantApproval
														  || a.MrId == (int)MainReport.Statuses.AwaitingProgramOfficerApproval
														  || a.MrId == (int)MainReport.Statuses.AwaitingAgencyResponse
										  where a.ReportDate.Year == mainReport.Start.Year
										  where a.ReportDate >= current.WeekStart
										  where a.ReportDate < current.WeekEnd
										  where a.ClientId == current.ClientId
										  select (decimal?)a.Quantity).Sum()

						 let x = (from a in db.HcCapsTableRaws
								  where a.ClientId == current.ClientId
								  let endDate = a.DeceasedDate == null ? a.EndDate : SqlFunctions.DateAdd("day", 28, a.EndDate)
								  where a.StartDate < current.WeekEnd
								  where a.EndDate == null || endDate > current.WeekStart
								  select a.HcCap).Max()
						 let cfsrow = (from cfs in db.CfsRows
									   where cfs.Client.MasterIdClcd == current.MasterIdClcd
									   where cfs.StartDate != null && cfs.StartDate <= current.WeekStart && cfs.EndDate == null
									   select cfs).Any()
						 let totalQuantity = current.Quantity + (submitted ?? 0)
						 let roundedQuantity = (decimal)CCDecimals.Truncate(totalQuantity, digits)
						 let roundedCap = CCDecimals.Truncate(x ?? 0, digits)
						 select new
						 {
							 current.ClientId,
							 Quantity = roundedQuantity,
							 HcCap = cfsrow ? 0 : roundedCap,
							 ReportDate = current.WeekStart,
							 HasCfs = cfsrow
						 };

				var hvv = q1.Where(f => f.Quantity > f.HcCap).Take(100).ToList();

				if (hvv.Any())
				{
					foreach (var item in hvv.ToList())
					{
						var date = item.ReportDate < mainReport.Start ? mainReport.Start : item.ReportDate;
						yield return new Row()
						{
							ClientId = (item.ClientId),
							MessageTypeName = "Homecare Weekly Cap exceeded",
							Date = date,
							Message = string.Format(
									"CCID: {3}, Reporting week: W{0}, Amount Reported: {1} hours (Weekly cap: {2} hours{4})"
									, cal.GetWeekOfYear(date.Value, dfi.CalendarWeekRule, dfi.FirstDayOfWeek)
									, item.Quantity.Format()
									, item.HcCap.Format()
									, item.ClientId
									,item.HasCfs ? ". The client has an open cfs record for this period" : ""
									)
						};
					}
				}

				/**** End of weekly validation without exceptional hours ****/

				/**** Weekly validation with exceptional hours ****/
				var q1Weh = from current in
								(from a in quantities
								 where a.MrId == mainReport.Id && a.ExceptionalHomeCareHours && a.CoPGovHoursValidation
								 group a by new { a.ClientId, a.WeekStart } into g
								 select new
								 {
									 g.Key.ClientId,
									 g.Key.WeekStart,
									 WeekEnd = SqlFunctions.DateAdd("week", 1, g.Key.WeekStart),
									 Quantity = g.Sum(f => f.Quantity)
								 })
							let submitted = (from a in quantities
											 where a.MrId != mainReport.Id && a.ExceptionalHomeCareHours && a.CoPGovHoursValidation
											 where a.MrStatusId == (int)MainReport.Statuses.Approved
															 || a.MrId == (int)MainReport.Statuses.AwaitingProgramAssistantApproval
															 || a.MrId == (int)MainReport.Statuses.AwaitingProgramOfficerApproval
															 || a.MrId == (int)MainReport.Statuses.AwaitingAgencyResponse
											 where a.ReportDate.Year == mainReport.Start.Year
											 where a.ReportDate >= current.WeekStart
											 where a.ReportDate < current.WeekEnd
											 where a.ClientId == current.ClientId
											 select (decimal?)a.Quantity).Sum()
							let x = (from a in db.HcCapsTableRaws
									 where a.ClientId == current.ClientId
									 let endDate = a.DeceasedDate == null ? a.EndDate : SqlFunctions.DateAdd("day", 28, a.EndDate)
									 where a.StartDate < current.WeekEnd
									 where a.EndDate == null || endDate > current.WeekStart
									 select a.GovHcHours).Max()
							let totalQuantity = current.Quantity + (submitted ?? 0)
							let roundedQuantity = (decimal)CCDecimals.Truncate(totalQuantity, digits)
							let roundedCap = CCDecimals.Truncate(x, digits)
							select new
							{
								current.ClientId,
								Quantity = roundedQuantity,
								GovHoursCap = roundedCap,
								ReportDate = current.WeekStart
							};

				var hvvWeh = q1Weh.Where(f => f.Quantity > f.GovHoursCap).Take(100).ToList();

				if (hvvWeh.Any())
				{
					foreach (var item in hvvWeh.ToList())
					{
						var date = item.ReportDate < mainReport.Start ? mainReport.Start : item.ReportDate;
						yield return new Row()
						{
							ClientId = (item.ClientId),
							MessageTypeName = "Gov Hours Cap exceeded",
							Date = date,
							Message = string.Format(
									"CCID: {3}, Reporting week: W{0}, Amount Reported: {1} hours (Weekly cap: {2} hours)"
									, cal.GetWeekOfYear(date.Value, dfi.CalendarWeekRule, dfi.FirstDayOfWeek)
									, item.Quantity.Format()
									, item.GovHoursCap.Format()
									, item.ClientId
									)
						};
					}
				}

				/**** End of weekly validation with exceptional hours ****/

				var negativeRates = (from sr in db.SubReports
									 where sr.MainReportId == mainReport.Id
									 join cr in db.ClientReports on sr.Id equals cr.SubReportId
									 join ar in db.ClientAmountReports on cr.Id equals ar.ClientReportId
									 where cr.Rate < 0 || ar.Quantity < 0
									 select new
									 {
										 ClientId = cr.ClientId,
										 Rate = cr.Rate,
										 Quantity = ar.Quantity
									 }).ToList();
				foreach (var item in negativeRates.Where(f => f.Rate < 0))
				{
					yield return new Row()
					{
						ClientId = item.ClientId,
						MessageTypeName = "Negative Rate",
						Message = "Rate cannot be negative (Rate: " + item.Rate.Format() + ")"
					};
				}
				foreach (var item in negativeRates.Where(f => f.Quantity < 0))
				{
					yield return new Row()
					{
						ClientId = item.ClientId,
						MessageTypeName = "Negative Quantity",
						Message = "Quantity cannot be negative (Quantity: " + item.Quantity.Format() + ")"
					};
				}
			}

			_log.DebugFormat("Validation: finished {0} in {1}", System.Reflection.MethodBase.GetCurrentMethod().Name, (DateTime.Now - t).TotalMilliseconds);
		}

		public IEnumerable<Row> ValidateEmergencyCaps(MainReport mainReport)
		{
			var t = DateTime.Now;
			using (var db = new ccEntities())
			{


				IQueryable<EmergencyCap> EmergencyCaps = db.EmergencyCaps.Where(f => f.Active);
				IQueryable<ClientReport> ClientReports = db.ClientReports;
				IQueryable<Client> Clients = db.Clients;

				var appId = (from mr in db.MainReports
							 where mr.Id == mainReport.Id
							 select mr.AppBudget.AppId).SingleOrDefault();


				var q = Queries.EmergencyCapSummary(EmergencyCaps, Clients, ClientReports, mainReport.Id);
				var cur = db.MainReports.Where(f => f.Id == mainReport.Id).Select(f => f.AppBudget.App.Fund.Currency).Single();
				var capoverflowQuery = q.Where(f => EntityFunctions.Truncate(f.TotalAmount * db.viewAppExchangeRates.FirstOrDefault(c => c.AppId == appId && c.ToCur == f.Cur).Value - f.CapPerPerson, CCDecimals.CompareDigits) > 0);
				foreach (var item in capoverflowQuery)
				{
					var row = new Row()
					{
						ClientId = item.ClientId,
						ClientName = item.ClientFirstName + " " + item.ClientLastName,
						MessageTypeName = "Emergency Cap Exceeded",
						Message = string.Concat(item.CapName, " - ", item.ClientFirstName, item.ClientLastName,
						" (CCID: ", item.ClientId, ") Reported amount so far ", (item.TotalAmount * db.viewAppExchangeRates.SingleOrDefault(c => c.AppId == appId && c.ToCur == item.Cur).Value).Format(), " ", item.Cur.ToString(),
						", cap amount ", item.CapPerPerson.Format(), " ", item.Cur, ".  Please adjust the report and re-submit.")

					};
					yield return row;
				}

				/*
				 * Upon submitting a report with EAP, if any discretionary percentage allowed, the system will sum all the discretionary reported for all submitted reports under the current report app, including current report.
				 * If this total sum exceeds allowed percentage of the app amount, the report will not be allowed to submit with an error message specifying the total discretionary  amount submitted, total discretionary in current report, allowed percentage,  and total amount reflecting this percentage.
				 */
				/*
				 * It should be a percentage of the specific budget line for EAP and not of the total app
				 */
				var qq = from current in
							 (from mr in db.MainReports
							  where mr.Id == mainReport.Id //&& (mr.StatusId != 2 || mr.StatusId !=8 || mr.StatusId != 32 || mr.StatusId != 128) // not Submitted
                              from sr in mr.SubReports
							  from er in sr.EmergencyReports
							  group er by new { sr.MainReportId, sr.AppBudgetServiceId } into g
							  select new
							  {
								  g.Key,
								  Discretionary = g.Sum(f => (decimal?)f.Discretionary)
							  })
						 join ccgrant in db.AppBudgetServices on current.Key.AppBudgetServiceId equals ccgrant.Id
						 join submitted in
							 (from mr in db.MainReports.Where(MainReport.Submitted)
							  where mr.AppBudgetId == mainReport.AppBudgetId
							  from sr in mr.SubReports
							  from er in sr.EmergencyReports
                              where mr.Id != mainReport.Id
							  group er by sr.AppBudgetServiceId into g
							  select new
							  {
								  AppBudgetServiceId = g.Key,
								  Discretionary = g.Sum(f => (decimal?)f.Discretionary)
							  }) on current.Key.AppBudgetServiceId equals submitted.AppBudgetServiceId into g
						 from submitted in g.DefaultIfEmpty()
						 join cap in
							 (
								 from b in db.EmergencyCaps
								 from fund in b.Funds
								 from country in b.Countries
								 select new
								 {
									 FundId = fund.Id,
									 CountryId = country.Id,
									 DiscretionaryPercentage = b.DiscretionaryPercentage / 100
								 }
								 ) on new { ccgrant.AppBudget.App.FundId, ccgrant.AppBudget.App.AgencyGroup.CountryId } equals new { cap.FundId, cap.CountryId }
						 select new
						 {
							 AgencyId = ccgrant.AgencyId,
							 AgencyName = ccgrant.Agency.Name,
							 ServiceId = ccgrant.ServiceId,
							 ServiceTypeId = ccgrant.Service.TypeId,
							 ServiceName = ccgrant.Service.Name,
							 ServiceTypeName = ccgrant.Service.ServiceType.Name,
							 current.Key.MainReportId,
							 ccgrant.AppBudget.App.CurrencyId,
							 Submitted = submitted.Discretionary,
							 Current = current.Discretionary,
							 Total = (current.Discretionary ?? 0) + (submitted.Discretionary ?? 0),
							 Max = cap.DiscretionaryPercentage * ccgrant.CcGrant,
							 Percentage = cap.DiscretionaryPercentage,
							 CcGrant = ccgrant.CcGrant,
						 };
				var ditem = from item in qq
							where item.MainReportId == mainReport.Id
							where item.Total > item.Max
							select item;
				foreach (var item in ditem)
				{
					var row = new Row()
					{
						AgencyId = item.AgencyId,
						AgencyName = item.AgencyName,
						ServiceName = item.ServiceName,
						ServiceTypeId = item.ServiceTypeId,
						ServiceTypeName = item.ServiceTypeName,
						MessageTypeName = "Emergency Cap Exceeded (Discretionary amount)",
						Message = string.Format("Total discretionary amount submitted: {0} {4}, Total discretionary amount in current report: {1} {4}, Allowed percentage: {2:p} ({3} {4}) ",
						(item.Total).Format(),
						item.Current.Format(),
						item.Percentage,
						(item.Max).Format(),
						item.CurrencyId)
					};
					yield return row;
				}
			}
			_log.DebugFormat("Validation: finished {0} in {1}", System.Reflection.MethodBase.GetCurrentMethod().Name, (DateTime.Now - t).TotalMilliseconds);
		}

		//depricated
		public IEnumerable<Row> ValidateClientApprovalStatus(MainReport mainReport)
		{
			var t = DateTime.Now;
			//Approval status validation was turned off
			using (var db = new ccEntities())
			{
				var q = from cr in db.ViewClientReports
						join sr in db.SubReports.Where(Permissions.SubReportsFilter) on cr.SubReportId equals sr.Id
						where sr.MainReportId == mainReport.Id
						join c in db.Clients.Where(Client.IneligibleByApprovalStatus) on cr.ClientId equals c.Id
						select new Row()
						{
							ClientId = cr.ClientId,
							ClientName = c.FirstName + " " + c.LastName,
							ServiceName = sr.AppBudgetService.Service.Name,
							ServiceTypeId = sr.AppBudgetService.Service.TypeId,
							SubReportId = cr.SubReportId,
							ReportingMethodId = sr.AppBudgetService.Service.ReportingMethodId,
							AgencyId = sr.AppBudgetService.Agency.Id,
							AgencyName = sr.AppBudgetService.Agency.Name,
							ServiceTypeName = sr.AppBudgetService.Service.ServiceType.Name
						};

				foreach (var r in q)
				{
					r.MessageTypeName = "Client is no longer eligible (Approval Status)";
					r.Message = string.Format("The client {0} (CCID: {1}) is no longer eligible (approval Status)", r.ClientName, r.ClientId);
					yield return r;
				}
			}
			_log.DebugFormat("Validation: finished {0} in {1}", System.Reflection.MethodBase.GetCurrentMethod().Name, (DateTime.Now - t).TotalMilliseconds);
		}

		public IEnumerable<Row> ValidateEligibilityPeriods(MainReport mainReport)
		{
			var t = DateTime.Now;

			using (var db = new ccEntities())
			{

				var r0 = new[]{ 
					Service.ReportingMethods.ClientNamesAndCosts,
					Service.ReportingMethods.ClientUnit,
					Service.ReportingMethods.ClientUnitAmount,
					Service.ReportingMethods.TotalCostWithListOfClientNames,
					Service.ReportingMethods.Homecare,
					Service.ReportingMethods.HomecareWeekly
				}.Select(f => (int)f).ToArray();
                //}.Select(f => (int?)f.ToArray() ?? 0;
                var q0 = from sr in db.SubReports
                             where r0.Contains(sr.AppBudgetService.Service.ReportingMethodId)
                        //where sr.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.adm
                         where sr.MainReportId == mainReport.Id
						 from cr in sr.ClientReports
						 let e = (from e in db.HomeCareEntitledPeriods
								  where e.ClientId == cr.ClientId
								  where e.StartDate < mainReport.End
								  where e.EndDate == null || e.EndDate > mainReport.Start
								  select e)
						 where !e.Any()
						 select new Row
						 {
							 AgencyId = cr.Client.AgencyId,
							 AgencyName = cr.Client.Agency.Name,
							 ClientId = cr.ClientId,
							 ClientName = cr.Client.FirstName + " " + cr.Client.LastName,
							 ClientReportId = cr.Id,
							 ServiceName = sr.AppBudgetService.Service.Name,
							 ServiceTypeId = sr.AppBudgetService.Service.TypeId,
							 ServiceTypeName = sr.AppBudgetService.Service.ServiceType.Name,
							 SubReportId = sr.Id,
							 ReportingMethodId = sr.AppBudgetService.Service.ReportingMethodId
						 };

                //var result0 = q0.ToArray();
              //  if (result0.Length == 0)
                  //  q0 = null;
                  //q0 =='';


				var q1 = from sr in db.SubReports
						 where sr.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.Emergency
						 where sr.MainReportId == mainReport.Id
						 from cr in sr.EmergencyReports
						 let e = (from e in db.HomeCareEntitledPeriods
								  where e.ClientId == cr.ClientId
								  where e.StartDate < mainReport.End
								  where e.EndDate == null || e.EndDate > mainReport.Start
								  select e)
						 where !e.Any()
						 select new Row
						 {
							 AgencyId = cr.Client.AgencyId,
							 AgencyName = cr.Client.Agency.Name,
							 ClientId = cr.ClientId,
							 ClientName = cr.Client.FirstName + " " + cr.Client.LastName,
							 ServiceName = sr.AppBudgetService.Service.Name,
							 ServiceTypeId = sr.AppBudgetService.Service.TypeId,
							 ServiceTypeName = sr.AppBudgetService.Service.ServiceType.Name,
							 SubReportId = sr.Id,
							 ReportingMethodId = sr.AppBudgetService.Service.ReportingMethodId
						 };

				var q4 = from sr in db.SubReports
						 where sr.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.SupportiveCommunities
						 where sr.MainReportId == mainReport.Id
						 from cr in sr.SupportiveCommunitiesReports
						 let e = (from e in db.HomeCareEntitledPeriods
								  where e.ClientId == cr.ClientId
								  where e.StartDate < mainReport.End
								  where e.EndDate == null || e.EndDate > mainReport.Start
								  select e)
						 where !e.Any()
						 select new Row
						 {
							 AgencyId = cr.Client.AgencyId,
							 AgencyName = cr.Client.Agency.Name,
							 ClientId = cr.ClientId,
							 ClientName = cr.Client.FirstName + " " + cr.Client.LastName,
							 ServiceName = sr.AppBudgetService.Service.Name,
							 ServiceTypeId = sr.AppBudgetService.Service.TypeId,
							 ServiceTypeName = sr.AppBudgetService.Service.ServiceType.Name,
							 SubReportId = sr.Id,
							 ReportingMethodId = sr.AppBudgetService.Service.ReportingMethodId
						 };
                
            //    if (q0 is null)
            //    {
                 //   var all = q1.Distinct().ToList()
                 //     .Concat(q4.Distinct().ToList());
                  //  foreach (var r in all)
                  //  {
                   //     r.MessageTypeName = "Client is no longer eligible (Eligibility Periods)";
                     //   r.Message = string.Format("The client {0} (CCID: {1}) is no longer eligible (eligibility periods)", r.ClientName, r.ClientId);
                     //   yield return r;
                  //  }
              //  }
                //else
               // {
                    var all = q0.Distinct().ToList()
                      .Concat(q1.Distinct().ToList())
                      .Concat(q4.Distinct().ToList());
                    foreach (var r in all)
                    {
                        r.MessageTypeName = "Client is no longer eligible (Eligibility Periods)";
                        r.Message = string.Format("The client {0} (CCID: {1}) is no longer eligible (eligibility periods)", r.ClientName, r.ClientId);
                        yield return r;
                    }
               // }
				//foreach (var r in all)
				//{
				//	r.MessageTypeName = "Client is no longer eligible (Eligibility Periods)";
				//	r.Message = string.Format("The client {0} (CCID: {1}) is no longer eligible (eligibility periods)", r.ClientName, r.ClientId);
				//	yield return r;
				//}
			}

			_log.DebugFormat("Validation: finished {0} in {1}", System.Reflection.MethodBase.GetCurrentMethod().Name, (DateTime.Now - t).TotalMilliseconds);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// 1.4.7.9.2.        “App’s Balance Exceeded” : 
		///1.4.7.9.2.1.        The current remaining amount on the selected app (composed of the total app/ agency amount minus the summary of all reports that are not New / Returned to Agency)  together with the current report amount exceeds the total app/ agency amount.
		///1.4.7.9.2.1.1.        Message: “App Balance Exceeded. Amount Reported <x> <CUR> App’s amount: <x> <CUR>”, where currencies are agency/ app currency. 
		/// </remarks>
		/// <param name="mainreport"></param>
		/// <returns></returns>
		public IEnumerable<Row> ValidateAppBalance(MainReport mainreport)
		{
			var t = DateTime.Now;

			using (var db = new ccEntities())
			{

				//the  permission filters should not be used here, or used with caution
				//worst case: a ser will be able to calculate the total exp on other ser's mainreports
				var MainReports = db.MainReports;
				var Apps = db.Apps;
				var appId = mainreport.AppBudget.AppId;
				var Cur = mainreport.AppBudget.App.CurrencyId;

				var q =
						from mr in
							(
							from mr in
								(
									from mr in MainReports.Where(MainReport.CurrentOrSubmitted(mainreport.Id))
									join sra in db.viewSubreportAmounts on mr.Id equals sra.MainReportId
									select new
									{
										MainReportId = mr.Id,
										AppId = mr.AppBudget.AppId,
										ReportAmount = sra.Amount
									}
								)
							group mr by new { AppId = mr.AppId } into mrg
							select new { AppId = mrg.Key.AppId, GlobalAmount = mrg.Sum(f => f.ReportAmount) }
							)
						join app in Apps on mr.AppId equals app.Id
						where app.Id == appId
						select new { AppId = mr.AppId, AppName = app.Name, AppAmount = app.CcGrant, GlobalAmount = mr.GlobalAmount, Cur = app.CurrencyId };




				foreach (var row in q.Where(f => EntityFunctions.Truncate(f.GlobalAmount - f.AppAmount, CCDecimals.CompareDigits) > 0))
				{
					yield return new Row()
					{
						MessageTypeName = "App Balance Exceeded",
						Message = "Amount Reported " + row.GlobalAmount.Format() + " " + row.Cur +
								" App’s amount: " + row.AppAmount.Format() + " " + row.Cur + ", where currencies are agency/ app currency"
					};
				}

			}
			_log.DebugFormat("Validation: finished {0} in {1}", System.Reflection.MethodBase.GetCurrentMethod().Name, (DateTime.Now - t).TotalMilliseconds);
		}

		/// <summary>
		/// Returns subreports that exceed the ccgrant
		/// </summary>
		/// <param name="mainreport"></param>
		/// <returns></returns>
		public IEnumerable<Row> ValidateCcGrants(MainReport mainreport)
		{
			var t = DateTime.Now;
			using (var db = new ccEntities())
			{
				var q =
						from mr in db.MainReports
								.Where(MainReport.CurrentOrSubmitted(mainreport.Id))
						where mr.AppBudgetId == mainreport.AppBudgetId
						join a in db.viewSubreportAmounts on mr.Id equals a.MainReportId
						group a by a.AppBudgetServiceId into ag
						join csr in db.SubReports.Where(f => f.MainReportId == mainreport.Id) on ag.Key equals csr.AppBudgetServiceId
						join abs in db.AppBudgetServices on ag.Key equals abs.Id
						where EntityFunctions.Truncate(ag.Sum(f => f.Amount) - abs.CcGrant, CCDecimals.CompareDigits) > 0
						select new
						{
							SubReportId = csr.Id,
							ServiceName = abs.Service.Name,
							ServiceId = abs.Service.Id,
							ServiceTypeName = abs.Service.ServiceType.Name,
							ServiceTypeId = abs.Service.ServiceType.Id,
							AgencyName = abs.Agency.Name,
							AgencyId = abs.Agency.Id,
							TotalAmount = ag.Sum(f => f.Amount),
							CCGrant = abs.CcGrant,
							ReportingMethodId = abs.Service.ReportingMethodId
						};

				foreach (var item in q)
				{
					yield return new Row()
					{
						MessageTypeName = "CC Grant exceeded",
						Message = "Amount reported (Agency: " + item.AgencyName + ", Service: " + item.ServiceTypeName + " - " + item.ServiceName + ", Amount: " + item.TotalAmount.Format() + ")  exceeded CC Grant as specified on the budget (" + item.CCGrant.Format() + ").",
						SubReportId = item.SubReportId,
						ReportingMethodId = item.ReportingMethodId,
						AgencyId = item.AgencyId,
						AgencyName = item.AgencyName,
						ServiceTypeId = item.ServiceTypeId,
						ServiceTypeName = item.ServiceTypeName
					};
				}



			}
			_log.DebugFormat("Validation: finished {0} in {1}", System.Reflection.MethodBase.GetCurrentMethod().Name, (DateTime.Now - t).TotalMilliseconds);
		}

		public IEnumerable<Row> ValidateRequiredFileds(int mainReportId)
		{

			var t = DateTime.Now;
			using (var db = new ccEntities())
			{
				var q = db.SubReports
					.Where(f => f.MainReportId == mainReportId)
					.Where(f => f.MatchingSum == null)
					.Where(f => f.AppBudgetService.Agency.AgencyGroup.RequiredMatch)
				.Select(sr => new Row()
				{
					SubReportId = sr.Id,
					AgencyId = sr.AppBudgetService.Agency.Id,
					AgencyName = sr.AppBudgetService.Agency.Name,
					MessageTypeName = "Field is mandatory",
					ServiceTypeId = sr.AppBudgetService.Service.TypeId,
					ReportingMethodId = sr.AppBudgetService.Service.ReportingMethodId
				});
				foreach (var r in q)
				{
					r.Message = string.Format("{0} - Mandatory field not filled- The field {1}  is mandatory. Please fill in the missing data.", r.ServiceName, "Matching Sum");
					yield return r;
				}
			}
			_log.DebugFormat("Validation: finished {0} in {1}", System.Reflection.MethodBase.GetCurrentMethod().Name, (DateTime.Now - t).TotalMilliseconds);
		}

		public IGenericRepository<SubReport> SubReportsRepository { get; set; }

		public IEnumerable<ValidationResult> Validate(ValidationContext context)
		{
			yield break;
		}

		public IEnumerable<Row> ValidateAnnualHcAssessment(MainReport mainreport)
		{
			var t = DateTime.Now;
            using (var db = new ccEntities())
                
            {
                db.CommandTimeout = 10000;
                var appId = db.MainReports.Where(f => f.Id == mainreport.Id).Select(f => f.AppBudget.AppId).SingleOrDefault();
                var q = from cr in db.ClientReports.Where(Permissions.ClientReportsFilter)
                        join mr in db.MainReports.Where(this.Permissions.MainReportsFilter).Where(MainReport.CurrentOrSubmitted(mainreport.Id)) on cr.SubReport.MainReportId equals mr.Id
                        where cr.SubReport.MainReport.AppBudget.AppId == appId
                        where cr.SubReport.AppBudgetService.Service.SingleClientPerYearAgency
                        group cr by new
                        {
                            ServiceId = cr.SubReport.AppBudgetService.ServiceId,
                            ServiceName = cr.SubReport.AppBudgetService.Service.Name,
                            ClientId = cr.ClientId,
                            AgencyId = cr.SubReport.AppBudgetService.AgencyId,
                            Year = System.Data.Objects.SqlClient.SqlFunctions.DatePart("year", cr.SubReport.MainReport.Start)
                        } into crg
                        where crg.Count() > 1
                        select new
                        {
                            ClientId = crg.Key.ClientId,
                            AgencyId = crg.Key.AgencyId,
                            ServiceName = crg.Key.ServiceName,
                            Year = crg.Key.Year,

                        };
                foreach (var item in q)
                {
                    yield return new Row
                    {
                        ClientId = item.ClientId,
                        Message = string.Format("CC ID {0} may only appear in {3} service line once per year {2} /per agency {1}", item.ClientId, item.AgencyId, item.Year, item.ServiceName)
                    };
                }
            }
			_log.DebugFormat("Validation: finished {0} in {1}", System.Reflection.MethodBase.GetCurrentMethod().Name, (DateTime.Now - t).TotalMilliseconds);
		}

		public IEnumerable<Row> ValidateClientsReuiredFields(MainReport mainReport)
		{
			using (var db = new ccEntities())
			{
				var dccClients = from sr in db.SubReports.Where(this.Permissions.SubReportsFilter)
								 where sr.MainReportId == mainReport.Id
								 from dcc in sr.DaysCentersReports
								 select new
								 {
									 AgencyId = dcc.Client.AgencyId,
									 AgencyName = dcc.Client.Agency.Name,
									 ClientId = dcc.ClientId,
									 ClientName = dcc.Client.FirstName + " " + dcc.Client.LastName,
									 ServiceName = sr.AppBudgetService.Service.Name,
									 ServiceTypeId = sr.AppBudgetService.Service.TypeId,
									 ServiceTypeName = sr.AppBudgetService.Service.ServiceType.Name,
									 SubReportId = sr.Id,
									 ReportingMethodId = sr.AppBudgetService.Service.ReportingMethodId,
									 NationalId = dcc.Client.NationalId,
									 IncomeCriteriaCompiled = dcc.Client.IncomeCriteriaComplied,
									 DCC_Client = dcc.Client.DCC_Client,
									 DCC_Subside = dcc.Client.DCC_Subside,
									 DCC_VisitCost = dcc.Client.DCC_VisitCost,
									 SC_Client = dcc.Client.SC_Client,
									 SC_MonthlyCost = dcc.Client.SC_MonthlyCost
								 };
				foreach (var c in dccClients.Where(f => !f.DCC_Client || !f.DCC_Subside.HasValue || !f.DCC_VisitCost.HasValue))
				{
					yield return new Row
					{
						AgencyId = c.AgencyId,
						AgencyName = c.AgencyName,
						ClientName = c.ClientName,
						ServiceName = c.ServiceName,
						ServiceTypeId = c.ServiceTypeId,
						ServiceTypeName = c.ServiceTypeName,
						SubReportId = c.SubReportId,
						ReportingMethodId = c.ReportingMethodId,
						MessageTypeName = "DCC Client is missing required fields",
						Message = string.Format("Client {0} (CCID: {1}) can not be reported because he is either not marked as DCC client or he does not have a subside level or the visit cost was not entered", c.ClientName, c.ClientId)
					};
				}

				var scClients = (from cr in db.SupportiveCommunitiesReports
								 join sr in db.SubReports.Where(Permissions.SubReportsFilter) on cr.SubReportId equals sr.Id
								 join c in db.Clients.Where(Permissions.ClientsFilter) on cr.ClientId equals c.Id
								 where sr.MainReportId == mainReport.Id
								 select new
								 {
									 AgencyId = sr.AppBudgetService.AgencyId,
									 AgencyName = sr.AppBudgetService.Agency.Name,
									 ClientId = cr.ClientId,
									 ClientName = c.FirstName + " " + c.LastName,
									 ServiceName = sr.AppBudgetService.Service.Name,
									 ServiceTypeId = sr.AppBudgetService.Service.TypeId,
									 ServiceTypeName = sr.AppBudgetService.Service.ServiceType.Name,
									 SubReportId = sr.Id,
									 ReportingMethodId = sr.AppBudgetService.Service.ReportingMethodId,
									 NationalId = c.NationalId,
									 IncomeCriteriaCompiled = c.IncomeCriteriaComplied,
									 DCC_Client = c.DCC_Client,
									 DCC_Subside = c.DCC_Subside,
									 DCC_VisitCost = c.DCC_VisitCost,
									 SC_Client = c.SC_Client,
									 SC_MonthlyCost = c.SC_MonthlyCost
								 });
				foreach (var c in scClients.Where(f => !f.SC_Client || !f.SC_MonthlyCost.HasValue))
				{
					yield return new Row
					{
						AgencyId = c.AgencyId,
						AgencyName = c.AgencyName,
						ClientName = c.ClientName,
						ServiceName = c.ServiceName,
						ServiceTypeId = c.ServiceTypeId,
						ServiceTypeName = c.ServiceTypeName,
						SubReportId = c.SubReportId,
						ReportingMethodId = c.ReportingMethodId,
						MessageTypeName = "SC Client is missing required fields",
						Message = string.Format("Client {0} (CCID: {1}) can not be reported because he is either not marked as Sc client or the cc subsidy was not entered", c.ClientName, c.ClientId)
					};
				}

				foreach (var c in scClients.Union(dccClients).Where(f => !f.IncomeCriteriaCompiled))
				{
					yield return new Row
					{
						AgencyId = c.AgencyId,
						AgencyName = c.AgencyName,
						ClientName = c.ClientName,
						ServiceName = c.ServiceName,
						ServiceTypeId = c.ServiceTypeId,
						ServiceTypeName = c.ServiceTypeName,
						SubReportId = c.SubReportId,
						ReportingMethodId = c.ReportingMethodId,
						MessageTypeName = "Income Criteria Complied",
						Message = string.Format("Client {0} (CCID: {1}) can not be reported because his income criteria complied is not checked", c.ClientName, c.ClientId)
					};
				}
			}
		}

		public IEnumerable<Row> ValidateSoupKitchens(MainReport mainReport)
		{
			using (var db = new ccEntities())
			{
				var reportedClients = from skr in db.SoupKitchensReports
									  join sk in db.SKMembersVisits on skr.Id equals sk.SKReportId
									  join sr in db.SubReports on skr.SubReportId equals sr.Id
									  where sr.MainReportId == mainReport.Id && sr.AppBudgetService.Service.TypeId == (int)Service.ServiceTypes.SoupKitchens
									  join c in db.Clients on skr.ClientId equals c.Id
									  select new
									  {
										  ClientId = c.Id,
										  MasterId = c.MasterId,
										  ReportDate = sk.ReportDate,
										  ClientName = c.FirstName + " " + c.LastName,
										  ServiceName = sr.AppBudgetService.Service.Name,
										  AgencyId = c.AgencyId,
										  AgencyName = c.Agency.Name,
										  ServiceId = sr.AppBudgetService.ServiceId,
										  NationalId = c.NationalId,
										  SubReportId = skr.SubReportId,
										  ReportingMethodId = sr.AppBudgetService.Service.ReportingMethodId
									  };

				var alreadyReported = from skr in db.SoupKitchensReports
									  join sk in db.SKMembersVisits on skr.Id equals sk.SKReportId
									  join c in reportedClients on skr.ClientId equals c.ClientId
									  join sr in db.SubReports on skr.SubReportId equals sr.Id
									  where sr.Id != c.SubReportId && sr.AppBudgetService.Service.TypeId == (int)Service.ServiceTypes.SoupKitchens
									  where sk.ReportDate == c.ReportDate
									  select new
									  {
										  ClientId = c.ClientId,
										  ClientName = c.ClientName,
										  ServiceName = c.ServiceName,
										  ReportDate = c.ReportDate,
										  AgencyId = c.AgencyId,
										  AgencyName = c.AgencyName,
										  ServiceTypeId = sr.AppBudgetService.Service.ServiceType.Id,
										  ServiceTypeName = sr.AppBudgetService.Service.ServiceType.Name,
										  SubReportId = sr.Id,
										  ReportingMethodId = sr.AppBudgetService.Service.ReportingMethodId
									  };

				foreach (var c in alreadyReported)
				{
					yield return new Row
					{
						AgencyId = c.AgencyId,
						AgencyName = c.AgencyName,
						ClientName = c.ClientName,
						ServiceName = c.ServiceName,
						ServiceTypeId = c.ServiceTypeId,
						ServiceTypeName = c.ServiceTypeName,
						SubReportId = c.SubReportId,
						ReportingMethodId = c.ReportingMethodId,
						MessageTypeName = "Soup Kitchens - Client Already Reported",
						Message = string.Format("Client {0} (CCID: {1}) was reported for service {2} on the same date {3}", c.ClientName, c.ClientId, c.ServiceName, c.ReportDate.ToShortDateString())
					};
				}

				var nationalReported = from skr in db.SoupKitchensReports
									   join sk in db.SKMembersVisits on skr.Id equals sk.SKReportId
									   join c in db.Clients on skr.ClientId equals c.Id
									   join rc in reportedClients on c.NationalId equals rc.NationalId
									   where c.NationalId != null && !alreadyReported.Select(f => f.ClientId).Contains(rc.ClientId)
									   join sr in db.SubReports on skr.SubReportId equals sr.Id
									   where sr.MainReportId == mainReport.Id && sr.AppBudgetService.Service.TypeId == (int)Service.ServiceTypes.SoupKitchens && sr.AppBudgetService.ServiceId != rc.ServiceId
									   where sk.ReportDate == rc.ReportDate
									   select new
									   {
										   ClientId = rc.ClientId,
										   ClientName = rc.ClientName,
										   ServiceName = rc.ServiceName,
										   ReportDate = rc.ReportDate,
										   AgencyId = c.AgencyId,
										   AgencyName = rc.AgencyName,
										   ServiceTypeId = sr.AppBudgetService.Service.ServiceType.Id,
										   ServiceTypeName = sr.AppBudgetService.Service.ServiceType.Name,
										   SubReportId = sr.Id,
										   ReportingMethodId = sr.AppBudgetService.Service.ReportingMethodId
									   };

				foreach (var c in nationalReported)
				{
					yield return new Row
					{
						AgencyId = c.AgencyId,
						AgencyName = c.AgencyName,
						ClientName = c.ClientName,
						ServiceName = c.ServiceName,
						ServiceTypeId = c.ServiceTypeId,
						ServiceTypeName = c.ServiceTypeName,
						SubReportId = c.SubReportId,
						ReportingMethodId = c.ReportingMethodId,
						MessageTypeName = "Soup Kitchens - Client Already Reported",
						Message = string.Format("Client {0} (CCID: {1}) was reported for service {2} on the same date {3}", c.ClientName, c.ClientId, c.ServiceName, c.ReportDate.ToShortDateString())
					};
				}

				var masterReported = from skr in db.SoupKitchensReports
									 join sk in db.SKMembersVisits on skr.Id equals sk.SKReportId
									 join c in db.Clients on skr.ClientId equals c.Id
									 join rc in reportedClients on c.MasterId equals rc.ClientId
									 where !alreadyReported.Select(f => f.ClientId).Contains(rc.ClientId) && !nationalReported.Select(f => f.ClientId).Contains(rc.ClientId)
									 join sr in db.SubReports on skr.SubReportId equals sr.Id
									 where sr.MainReportId != mainReport.Id && sr.AppBudgetService.Service.TypeId == (int)Service.ServiceTypes.SoupKitchens && sr.AppBudgetService.ServiceId == rc.ServiceId
									 where sk.ReportDate == rc.ReportDate
									 select new
									 {
										 ClientId = rc.ClientId,
										 ClientName = rc.ClientName,
										 ServiceName = rc.ServiceName,
										 ReportDate = rc.ReportDate,
										 AgencyId = c.AgencyId,
										 AgencyName = rc.AgencyName,
										 ServiceTypeId = sr.AppBudgetService.Service.ServiceType.Id,
										 ServiceTypeName = sr.AppBudgetService.Service.ServiceType.Name,
										 SubReportId = sr.Id,
										 ReportingMethodId = sr.AppBudgetService.Service.ReportingMethodId
									 };

				foreach (var c in masterReported)
				{
					yield return new Row
					{
						AgencyId = c.AgencyId,
						AgencyName = c.AgencyName,
						ClientName = c.ClientName,
						ServiceName = c.ServiceName,
						ServiceTypeId = c.ServiceTypeId,
						ServiceTypeName = c.ServiceTypeName,
						SubReportId = c.SubReportId,
						ReportingMethodId = c.ReportingMethodId,
						MessageTypeName = "Soup Kitchens - Client Already Reported",
						Message = string.Format("Client {0} (CCID: {1}) was reported for service {2} on the same date {3}", c.ClientName, c.ClientId, c.ServiceName, c.ReportDate.ToShortDateString())
					};
				}

				var skNotEligibleJoinDate = (from skr in db.SoupKitchensReports
											 join sr in db.SubReports on skr.SubReportId equals sr.Id
											 where sr.MainReportId == mainReport.Id
											 join c in db.Clients on skr.ClientId equals c.Id
											 from sk in skr.SKMembersVisits
											 where c.JoinDate > sk.ReportDate
											 select new Row
											 {
												 ClientId = c.Id,
												 ClientName = c.FirstName + " " + c.LastName,
												 Date = c.JoinDate
											 }).Distinct().ToList();
				foreach (var r in skNotEligibleJoinDate)
				{
					r.MessageTypeName = "Client is not eligible (Join Date)";
					r.Message = string.Format("Client {0} (CCID: {1}) can not be reported before {2}",
						r.ClientName, r.ClientId, r.Date.Value.ToString("dd MMM yyyy"));
					yield return r;
				}

				var skNotEligibleLeaveDate = (from skr in db.SoupKitchensReports
											  join sr in db.SubReports on skr.SubReportId equals sr.Id
											  where sr.MainReportId == mainReport.Id
											  join c in db.Clients on skr.ClientId equals c.Id
											  from sk in skr.SKMembersVisits
											  where c.LeaveDate != null && c.LeaveDate < sk.ReportDate
											  select new Row
											  {
												  ClientId = c.Id,
												  ClientName = c.FirstName + " " + c.LastName,
												  Date = c.LeaveDate
											  }).Distinct().ToList();
				foreach (var r in skNotEligibleLeaveDate)
				{
					r.MessageTypeName = "Client is not eligible (Leave Date)";
					r.Message = string.Format("Client {0} (CCID: {1}) can not be reported after {2}",
						r.ClientName, r.ClientId, r.Date.Value.ToString("dd MMM yyyy"));
					yield return r;
				}

				foreach (var item in ValidateSoupKitchenCaps(mainReport))
				{
					yield return item;
				}

			}
		}
		/// <summary>
		/// Validates the soup kitchens (IL soup programs) cap
		/// The cap equals to the number of days the client was eligible during the report period
		/// The cap is compared to the number of visits of the client (including the duplicate clients)
		/// The clients eligibility is determined based on the Join&Leave date + the HomeCare Eligibility periods
		/// </summary>
		/// <param name="mainReport"></param>
		/// <returns></returns>
		private IEnumerable<Row> ValidateSoupKitchenCaps(MainReport mainReport)
		{
			using (var db = new ccEntities())
			{

				var skCaps = from h in db.HomeCareEntitledPeriods
							 where h.StartDate < mainReport.End && (h.EndDate == null || h.EndDate > mainReport.Start)
							 let earlyEndDate = h.EndDate < h.Client.LeaveDate || h.EndDate != null && h.Client.LeaveDate == null ? h.EndDate : h.Client.LeaveDate
							 let maxStartDate = h.StartDate >= h.Client.JoinDate ? h.StartDate : h.Client.JoinDate
							 select new
							 {
								 h.ClientId,
								 StartDate = maxStartDate >= mainReport.Start ? maxStartDate : mainReport.Start,
								 EndDate = earlyEndDate < mainReport.End ? earlyEndDate : mainReport.End
							 } into hc
							 group hc by hc.ClientId into hcg
							 select new
							 {
								 ClientId = hcg.Key,
								 DaysCount = hcg.Sum(f => SqlFunctions.DateDiff("DAY", f.StartDate, f.EndDate))
							 };
				var skReports = from a in db.MainReports.Where(MainReport.CurrentOrSubmitted(mainReport.Id))
								where a.Start < mainReport.End && a.End > mainReport.Start
								from sr in a.SubReports
								where sr.AppBudgetService.Service.TypeId == (int)Service.ServiceTypes.SoupKitchens
								where sr.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.SoupKitchens
								from cr in sr.SoupKitchensReports
								from ar in cr.SKMembersVisits
								group ar by cr.ClientId into arg
								select new
								{
									ClientId = arg.Key,
									DaysReported = arg.Count()
								} into t
								join c in db.Clients on t.ClientId equals c.Id
								group t by c.MasterIdClcd into tg
								select new
								{
									MasterId = tg.Key,
									DaysReported = tg.Sum(f => f.DaysReported)
								};

				var qqq = (from clientId in
							   (from sr in db.SubReports
								where sr.MainReportId == mainReport.Id
								from cr in sr.SoupKitchensReports
								select cr.ClientId).Distinct()
						   join cap in skCaps on clientId equals cap.ClientId into capG
						   from cap in capG.DefaultIfEmpty()
						   join c in db.Clients on clientId equals c.Id //the MasterId could be retrieved from the 
						   join reportedValue in skReports on c.MasterIdClcd equals reportedValue.MasterId
						   where reportedValue.DaysReported == null || reportedValue.DaysReported > cap.DaysCount
						   select new
						   {
							   ClientId = (int)clientId,
							   TotalDaysReported = reportedValue.DaysReported,
							   DaysCap = cap.DaysCount
						   });


				foreach (var c in qqq)
				{
					yield return new Row
					{
						ClientId = c.ClientId,
						MessageTypeName = "Soup Kitchens - Meals Cap Exceeded",
						Message = string.Format("CC ID {0} has been reported for more meals than are allowed in this reporting period. Allowed meals – {1}, Reported meals - {2}",
						c.ClientId, c.DaysCap, c.TotalDaysReported)
					};
				}
			}
		}

		public IEnumerable<Row> ValidateHCWeeklyCaps(MainReport mainReport)
		{
			var t = DateTime.Now;
			using (var db = new ccEntities())
			{

				var capoverflowQuery = db.spValidateWeeklyHcCaps(mainReport.Id);
				foreach (var item in capoverflowQuery)
				{
					//error message example: Cap Name - John Doe (CCID: 123) Reported amount so far 123 USD, cap amount 123 USD. Please adjust the report and re-submit.

					var row = new Row()
					{
						ClientId = item.ClientId,
						ClientName = item.FirstName + " " + item.LastName,
						MessageTypeName = "Homecare Cap Exceeded",
						Message = string.Format("{0} - {1} {2} (CCID: {3}) Reported amount so far {4} {5}, cap amount {6} {5}.  Please adjust the report and re-submit."
							, item.CapName
							, item.FirstName
							, item.LastName
							, item.ClientId
							, item.Amount.Format()
							, item.CurrencyId
							, item.CapPerPerson.Format()
						)
					};
					yield return row;
				}
			}
			_log.DebugFormat("Validation: finished {0} in {1}", System.Reflection.MethodBase.GetCurrentMethod().Name, (DateTime.Now - t).TotalMilliseconds);
		}

		public IEnumerable<Row> ValidateMhmCaps(MainReport mainReport)
		{
			var t = DateTime.Now;
			using (var db = new ccEntities())
			{
				IQueryable<MhmCap> MhmCaps = db.MhmCaps.Where(f => f.Active);
				IQueryable<ClientReport> ClientReports = db.ClientReports;
				IQueryable<Client> Clients = db.Clients;

				var appId = (from mr in db.MainReports
							 where mr.Id == mainReport.Id
							 select mr.AppBudget.AppId).SingleOrDefault();


				var q = Queries.MhmCapSummary(MhmCaps, Clients, ClientReports, mainReport.Id);
				var cur = db.MainReports.Where(f => f.Id == mainReport.Id).Select(f => f.AppBudget.App.Fund.Currency).Single();
				var capoverflowQuery = q.Where(f => EntityFunctions.Truncate(f.TotalAmount * db.viewAppExchangeRates.FirstOrDefault(c => c.AppId == appId && c.ToCur == f.Cur).Value - f.CapPerPerson, CCDecimals.CompareDigits) > 0);
				foreach (var item in capoverflowQuery)
				{
					var row = new Row()
					{
						ClientId = item.ClientId,
						ClientName = item.ClientFirstName + " " + item.ClientLastName,
						MessageTypeName = "MHM Cap Exceeded",
						Message = string.Concat(item.CapName, " - ", item.ClientFirstName, item.ClientLastName,
						" (CCID: ", item.ClientId, ") Reported amount so far ", (item.TotalAmount * db.viewAppExchangeRates.SingleOrDefault(c => c.AppId == appId && c.ToCur == item.Cur).Value).Format(), " ", item.Cur.ToString(),
						", cap amount ", item.CapPerPerson.Format(), " ", item.Cur, ".  Please adjust the report and re-submit.")

					};
					yield return row;
				}
			}
			_log.DebugFormat("Validation: finished {0} in {1}", System.Reflection.MethodBase.GetCurrentMethod().Name, (DateTime.Now - t).TotalMilliseconds);
		}
		public IEnumerable<Row> ValidateHospiceCareReports1(MainReport mainReport)
		{
			using (var db = new ccEntities())
			{
                var q = from sr in db.SubReports
                                      where sr.MainReportId == mainReport.Id
                                      select new
                                      {
                                          SubReportId = sr.Id,
                                          ServiceTypeId = sr.AppBudgetService.Service.TypeId,
                                          sr.AppBudgetService.Service.ReportingMethodId,
                                          AgencyId = sr.AppBudgetService.AgencyId,
                                      } into a
                                      group a by a.AgencyId into g
                                      select new
                                      {
                                          g.Key,
                                          hospiceCareReportsCount = g.Count(item => item.ServiceTypeId == (int)Service.ServiceTypes.HospiseCare),
                                          hcReportsCount = g.Count(b => b.ServiceTypeId == (int)Service.ServiceTypes.Homecare
                                                       && b.ReportingMethodId == (int)Service.ReportingMethods.HomecareWeekly)
                                      };
                var y = q.ToList();
				var z = y.Any(item => item.hospiceCareReportsCount > 0 && item.hcReportsCount == 0);
				if (z)
				{
					yield return new Row
					{
						Message = "Hospice Care must be reported alongside other types of weekly home care. Please add a homecare service to this report with clients meeting their caps and re-submit."
					};
				}
				var zz = y.Any(item => item.hospiceCareReportsCount > 1);
				if (zz)
				{
					yield return new Row
					{
						Message = "A report may only contain a maximum of one hospice care service"
					};

				}
				yield break;
			}
		}
		public IEnumerable<Row> ValidateHospiceCare2(MainReport mainReport)
		{
			using (var db = new ccEntities())
			{
				var allowedHcStatuses = new int?[] { 2, 3 };
				var reportedClients = (from sr in db.SubReports
									   where sr.MainReportId == mainReport.Id
									   where sr.AppBudgetService.Service.TypeId == (int)Service.ServiceTypes.HospiseCare
									   from cr in sr.ClientReports
									   select cr.Client).Distinct();
				var q = from c in reportedClients
						let hc1 = c.ClientHcStatuses.Where(f => f.StartDate <= mainReport.Start).OrderByDescending(f => f.StartDate).FirstOrDefault()
						let hcs = c.ClientHcStatuses.Where(f => f.StartDate > mainReport.Start && f.StartDate < mainReport.End)
						where hcs.Concat(new[] { hc1 }).Any(f => !allowedHcStatuses.Contains(f.HcStatusId))
						select new
						{
							c.Id,
							FullName = c.FirstName + " " + c.LastName
						};
				foreach(var item in q)
				{
					var msg = string.Format("Client {0} {1} is reported for Hospice Care but is not HAS 2 or 3 throughout the entire reporting period",
						item.Id,
						item.FullName);

					yield return new Row
					{
						Message = msg,
						ClientId = item.Id
					};
				}


			}
		}
		private IEnumerable<Row> ValidateHospiceCareCaps(MainReport mainReport)
		{

			using (var db = new ccEntities())
			{

				var selectedDOW = GlobalHelper.GetWeekStartDay(mainReport, db);
				var dayToSubstruct = (int)selectedDOW + 1;
				DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
				dfi.FirstDayOfWeek = (DayOfWeek)selectedDOW;
				Calendar cal = dfi.Calendar;



				var hospiceSubReports = from mr in db.MainReports
										where mr.Id == mainReport.Id
										from sr in mr.SubReports
										where sr.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.HomecareWeekly
										where sr.AppBudgetService.Service.TypeId == (int)Service.ServiceTypes.HospiseCare
										select sr;
				var q1 = CC.Data.Helpers.HCCapsHelper.GetWeeklyQuantities(hospiceSubReports, dayToSubstruct);
				var hcWeeklySubReports = from sr in db.SubReports
                                         where sr.MainReportId == mainReport.Id
										 where sr.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.HomecareWeekly
										 where sr.AppBudgetService.Service.TypeId == (int)Service.ServiceTypes.Homecare
										 select sr;
				var q2 = CC.Data.Helpers.HCCapsHelper.GetWeeklyQuantities(hcWeeklySubReports, dayToSubstruct);

				var q3 = from hr in q1
						 join hcr in q2 on new { hr.ClientId, hr.ReportDate } equals new { hcr.ClientId, hcr.ReportDate } into hcrg
						 from hcr in hcrg.DefaultIfEmpty()
						 let caps = from cap in db.HcCapsTableRaws
									where cap.ClientId == hr.ClientId
									where cap.StartDate < hr.WeekEnd
									where cap.EndDate == null || cap.EndDate > hr.WeekStart
									select cap
						 let cap = caps.FirstOrDefault()
						 select new
						 {
							 hr.ClientId,
							 hr.ReportDate,
							 hr.WeekStart,
							 hr.WeekEnd,
							 HospiceCareQuantity = (decimal?)hr.Quantity??0,
							 HomeCareQuantity = (decimal?)hcr.Quantity??0,
							 HomecareCap = cap.HcCap,
							 HospiceCareCap = cap.HospiceCareCap

						 };
				foreach (var item in q3.Where(f => f.HomeCareQuantity != f.HomecareCap))
				{
					var date = item.ReportDate < mainReport.Start ? mainReport.Start : item.ReportDate;
					var row = new Row
					{
						ClientId = (item.ClientId),
						MessageTypeName = "Hospice Care - Homecare hours",
						Date = item.WeekStart,
						Message = string.Format(
									"CCID: {3}, Reporting week: W{0}, Amount Reported: {1} hours (Weekly cap: {2})"
									, cal.GetWeekOfYear(item.ReportDate, dfi.CalendarWeekRule, dfi.FirstDayOfWeek)
									, item.HomeCareQuantity.Format()
									, item.HomecareCap.Format()
									, item.ClientId
									)
					};
					yield return row;
				}
				foreach (var item in q3.Where(f => f.HospiceCareQuantity > f.HospiceCareCap))
				{
					var date = item.ReportDate < mainReport.Start ? mainReport.Start : item.ReportDate;
					var row = new Row
					{
						ClientId = (item.ClientId),
						MessageTypeName = "Hospice Care - Cap exceeded",
						Date = item.WeekStart,
						Message = string.Format(
									"CCID: {3}, Reporting week: W{0}, Amount Reported: {1} hours (Weekly cap: {2})"
									, cal.GetWeekOfYear(item.ReportDate, dfi.CalendarWeekRule, dfi.FirstDayOfWeek)
									, item.HospiceCareQuantity.Format()
									, item.HospiceCareCap.Format()
									, item.ClientId
									)
					};
					yield return row;
				}
				yield break;
			}
		}

        private IEnumerable<Row> ValidateFuneralExpensesCaps(MainReport mainReport)
        {
            using (var db = new ccEntities())
            {
                var FuneralExpensesCap =
                                       from ar in db.AppBudgets
                                       where ar.Id == mainReport.AppBudgetId
                                       from br in db.Apps
                                       where br.Id == ar.AppId && br.FuneralExpenses != null
                                       select br.FuneralExpenses;

                foreach (var item in FuneralExpensesCap)
                {
                    if (FuneralExpensesCap.FirstOrDefault().Value > 10)
                    {
                        yield return new Row
                        {
                            MessageTypeName = "Funeral Expenses Cap Exceeded",

                            Message = "Cap for Funeral Expenses is 10. Please adjust the report and re-submit. "
                        };
                    }
                }
            }
            yield break;
        }



        #endregion


        public int errorsLimit = 100;

		public IEnumerable<Row> Errors { get; set; }


		public bool CanBeSubmitted
		{
			get
			{
				return this.Status == MainReport.Statuses.New
					&& (this.Permissions.User.RoleId == (int)FixedRoles.Ser || this.Permissions.User.RoleId == (int)FixedRoles.SerAndReviewer)
					&& this.Permissions.User.AgencyGroupId == this.AgencyGroupId
					&& !this.Errors.Any();
			}
		}
		public bool Approve { get; set; }

		/// <summary>
		/// ser submission details
		/// </summary>
		public SubmissionDetails Ser { get; set; }


		#region Nested Classes
		public class Row
		{
			#region
			//reference for links
			[ScaffoldColumn(false)]
			public int AgencyId { get; set; }
			[ScaffoldColumn(false)]
			public int ServiceTypeId { get; set; }
			[ScaffoldColumn(false)]
			public int SubReportId { get; set; }
			[ScaffoldColumn(false)]
			public int? ClientReportId { get; set; }
			[ScaffoldColumn(false)]
			public int? AmountReportId { get; set; }

			[ScaffoldColumn(false)]
			public decimal? Quantity { get; set; }
			[ScaffoldColumn(false)]
			public decimal? Cap { get; set; }
			[ScaffoldColumn(false)]
			public DateTime? Date { get; set; }

			#endregion

			#region Display Names
			//display data
			[Display(Name = "Agency")]
			public string AgencyName { get; set; }
			[ScaffoldColumn(false)]
			[Display(Name = "Service Type")]
			public string ServiceTypeName { get; set; }
			[ScaffoldColumn(false)]
			[Display(Name = "Service")]
			public string ServiceName { get; set; }
			[ScaffoldColumn(false)]
			[Display(Name = "CC ID")]
			public int ClientId { get; set; }
			[ScaffoldColumn(false)]
			[Display(Name = "Client Name")]
			public string ClientName { get; set; }
			[Display(Name = "Message Type")]
			public string MessageTypeName { get; set; }
			[Display(Name = "Error Details")]
			public string Message { get; set; }


			[Display(Name = "Israel Id")]
			public string NationalId { get; set; }

			[ScaffoldColumn(false)]
			public int? ReportingMethodId { get; set; }
			#endregion
		}


		#endregion
	}
}