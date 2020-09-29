using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CC.Data;
using System.Data.Objects.SqlClient;
using CC.Web.Helpers;

namespace CC.Web.Models.Import.ClientReports
{

	class CriHcImportHelper : ImportHelperBase<CriHc, CriHcPreview>, IClientReportImportHelper
	{


		public CriHcImportHelper(CC.Data.Services.IPermissionsBase permissions)
			: base(permissions)
		{
			this.columns = new List<jQueryDataTableColumn>()
			{
				new jQueryDataTableColumn()	{sName= "RowIndex", sTitle= "Row Index"}, 
				new jQueryDataTableColumn()	{sName= "ClientId", sTitle= "CC ID"}, 
				new jQueryDataTableColumn() {sName= "ClientName", sTitle = "Client Name"},
				new jQueryDataTableColumn() {sName= "Date", sTitle="Date", sType="date", format="MMM yyyy"},
				new jQueryDataTableColumn() {sName= "Rate", sTitle="Rate"},
				new jQueryDataTableColumn() {sName= "Quantity", sTitle="Quantity"},
				new jQueryDataTableColumn() {sName= "Remarks", sTitle="Remarks"},
				new jQueryDataTableColumn() {sName= "Errors", sTitle="Errors"} 
			};
		}

		protected override System.Linq.Expressions.Expression<Func<CriHcPreview, bool>> GetSearchFilter(string s)
		{
			return base.GetSearchFilter(s);
		}

		protected override void SetMapping(CsvHelper.Configuration.CsvConfiguration conf)
		{
			conf.ClassMapping<CriHcCsvMap>();
		}
		protected override IQueryable<CriHcPreview> PreviewQuery(Guid id, ccEntities _db)
		{
			var objectSet = _db.CriBases.OfType<CriHc>();
			var Clients = this.GetClients(_db);
			var SubReports = this.GetSubReports(_db);

			var subReportId = _db.Imports.Where(f => f.Id == id).Select(f=>f.TargetId).FirstOrDefault();
			var details = (from sr in _db.SubReports
						  where sr.Id == subReportId
						  select new
						  {
							  MrStart = sr.MainReport.Start,
							  MrEnd = sr.MainReport.End,
							  AgencyId = sr.AppBudgetService.AgencyId,
							  AppId = sr.AppBudgetService.AppBudget.AppId,
							  AgencyGroupId = sr.AppBudgetService.Agency.GroupId,
							  sr.AppBudgetService.Service.ReportingMethodId
						  }).SingleOrDefault();

			var startingWeek = details.MrStart;
			DayOfWeek selectedDOW = startingWeek.DayOfWeek;
			int? selectedDowDb = GlobalHelper.GetWeekStartDay(details.AgencyGroupId, details.AppId);
			if (selectedDowDb.HasValue)
			{
				selectedDOW = (DayOfWeek)selectedDowDb.Value;
				if (startingWeek.Month > 1)
				{
					var diff = selectedDowDb.Value - (int)startingWeek.DayOfWeek;
					startingWeek = startingWeek.AddDays((double)(diff));
					if (startingWeek > details.MrStart)
					{
						startingWeek = startingWeek.AddDays(-7);
					}
				}
			}
			var weekToSubstruct = new DateTime(startingWeek.Year, 1, 1);
			weekToSubstruct = weekToSubstruct.AddDays(Math.Abs((int)selectedDOW - (int)weekToSubstruct.DayOfWeek));
			var mrStartDate = details.MrStart;
			if (details.ReportingMethodId == (int)Service.ReportingMethods.HomecareWeekly)
			{
				mrStartDate = startingWeek;
			}
			DateTime JoinDateToCompare = new DateTime(2018, 1, 1);
			var source = (from item in objectSet
						  where item.ImportId == id
						  join client in Clients on item.ClientId equals client.Id into clientsGroup
						  from client in clientsGroup.DefaultIfEmpty()
						  join subReport in SubReports on item.SubReportId equals subReport.Id into srg
						  from subReport in srg.DefaultIfEmpty()
						  let ldx = System.Data.Objects.EntityFunctions.AddDays(client.LeaveDate, client.DeceasedDate == null ? 0 : 90)
						  let start = item.Date ?? subReport.MainReport.Start
                          let mrStart = subReport.MainReport.Start
                          let end = subReport.MainReport.End
						  let diff = SqlFunctions.DateDiff("day", weekToSubstruct, item.Date)
						  select new CriHcPreview
						  {
							  RowIndex = item.RowIndex,
							  ClientId = item.ClientId,
							  ClientName = client.AgencyId != subReport.AppBudgetService.AgencyId ? "" : client.FirstName + " " + client.LastName,
							  Date = item.Date,
							  Week = "W" + SqlFunctions.StringConvert((decimal?)(diff / 7 + (SqlFunctions.DatePart("day", weekToSubstruct) == 1 || diff < 0 ? 1 : 2))),
							  Rate = item.Rate,
							  Quantity = item.Quantity,
							  Remarks = item.Remarks,
							  Errors = (item.Errors ?? "") +
							  (
								  ((client == null) ? "Invalid ClientId. " : "") +
								  ((client.AgencyId != subReport.AppBudgetService.AgencyId) ? "Invalid Agency. " : "") +
                                  ((item.Rate == null || item.Rate <= 0) ? "Rate must be greater than 0. " : "") +
                                  ((item.Quantity == null || item.Quantity < 0) ? "Quantity must be greater or equal to 0. " : "") +
                                  (client.JoinDate > end ? "Client has joined after report end date" : "") +
                                  (client.DeceasedDate == null && client.LeaveDate < mrStart ? "Client has left before report start date" : "") +
                                  (client.DeceasedDate != null && client.DeceasedDate < mrStart ? "Client has DOD before report start date" : "") +
								  ((item.Date < mrStartDate || item.Date >= subReport.MainReport.End) ? "Invalid Date (report date is outside of the report period)" : "") +
								  ((client.AgencyId != subReport.AppBudgetService.AgencyId) ? "Invalid Agency. " : "") +
								  ((client.JoinDate > subReport.MainReport.End) ? "Client cant be reported because of Join Date. " : "") +
									( ldx < start ? "The Client has left the agency." : "" ) +
									( 
										(client.LeaveDate < start && ldx >= start && (item.Remarks == null || item.Remarks.Trim().Length == 0)) ? 
											"Deceased Client needs to be specified with Unique Circumstances." : ""
									) +
								  //client is not marked as complied but the verification is required
								  ((
									!client.IncomeCriteriaComplied &&
									client.Agency.AgencyGroup.Country.IncomeVerificationRequired &&
									(client.FundStatusId == null || client.FundStatus.IncomeVerificationRequired)) ?
									"Income verification required. " : "") +
								
								  (!(client.ExceptionalHours > 0 || client.GrandfatherHours.Any(f => f.StartDate < subReport.MainReport.End) || client.FunctionalityScores.Any(f => f.StartDate < subReport.MainReport.End)) ? "Insufficient Home Care Hours limit. " : "") +
								  (!(client.HomeCareEntitledPeriods.Any(f => f.StartDate < subReport.MainReport.End && (f.EndDate == null || f.EndDate > subReport.MainReport.Start))) ? "Eligibility period does not permit reporting. " : "") +
								  (!(client.FunctionalityScores.Any(f => f.StartDate < subReport.MainReport.End)) ? "Functionality Scores period does not permit reporting. " : "")
							  )

						  });
			return source;
		}
		public override void Import(Guid importId)
		{
			using (var db = getDbContext())
			{
				try
				{
					db.CommandTimeout = 300;
					var res = db.CriHcImport(importId);
				}
				catch (System.Data.UpdateException ex)
				{
					_log.Fatal("failed to import", ex);
					throw new Exception(ex.InnerException.Message);
				}
			}
		}
	}

	public class CriHcPreview : CriHc, ICriPreview
	{
		public string ClientName { get; set; }
		public string Week { get; set; }
	}
	public class CriHcCsvMap : CsvHelper.Configuration.CsvClassMap<CC.Data.CriHc>
	{
		public CriHcCsvMap()
		{
			Map(f => f.ClientId).Name("CC_ID", "CCID", "CC ID", "ClientID");
			Map(m => m.Date).Name("Date").TypeConverter<InvariantMonthTypeConverter>();
			Map(f => f.Rate).Name("Rate");
			Map(f => f.Quantity).Name("Quantity");
			Map(f => f.Remarks).Name("Remarks");
		}
	}
}
