using CC.Data;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Data.Objects.SqlClient;
using System.Linq;
using System.Web;

namespace CC.Web.Models.Import.ClientReports
{
	class CriSoupKitchensHelper : ImportHelperBase<CriSoupKitchens, CriSoupKitchensPreview>, IClientReportImportHelper
	{

		public CriSoupKitchensHelper(CC.Data.Services.IPermissionsBase permissions)
			: base(permissions)
		{
			this.columns = new List<jQueryDataTableColumn>(){
				new jQueryDataTableColumn()	{sName= "RowIndex", sTitle="Row Index"}, 
				new jQueryDataTableColumn()	{sName="ClientId", sTitle= "CC ID"},
				new jQueryDataTableColumn()	{sName="Date", sTitle= "Report Date", sType="date", format="dd MMM yyyy"},
               	new jQueryDataTableColumn() {sName="Errors", sTitle="Errors"} 
			};
		}

		protected override void SetMapping(CsvConfiguration conf)
		{
			conf.ClassMapping<CriSoupKitchensCsvMap>();
		}

		protected override System.Linq.Expressions.Expression<Func<CriSoupKitchensPreview, bool>> GetSearchFilter(string s)
		{
			return base.GetSearchFilter(s);
		}
		protected override IQueryable<CriSoupKitchensPreview> PreviewQuery(Guid id, CC.Data.ccEntities _db)
		{

			var objectSet = _db.CriBases.OfType<CriSoupKitchens>();
			var Clients = this.GetClients(_db);
			var SubReports = this.GetSubReports(_db);
			DateTime JoinDateToCompare = new DateTime(2018, 1, 1);
			var source = (from item in objectSet
						  where item.ImportId == id
						  join client in Clients on item.ClientId equals client.Id into clientsGroup
						  from client in clientsGroup.DefaultIfEmpty()
						  join subReport in SubReports on item.SubReportId equals subReport.Id into srg
						  from subReport in srg.DefaultIfEmpty()
						  let skrCount = (from sk in _db.SoupKitchensReports
										  where sk.Client.MasterIdClcd == client.MasterIdClcd
										  where sk.SubReport.MainReportId != subReport.MainReportId
										  from skv in sk.SKMembersVisits // _db.SKMembersVisits on sk.Id equals skv.SKReportId
										  join mr in _db.MainReports.Where(MainReport.Submitted) on sk.SubReport.MainReportId equals mr.Id
										  where skv.ReportDate >= subReport.MainReport.Start && skv.ReportDate < subReport.MainReport.End
										  where sk.SubReport.AppBudgetService.Service.TypeId == (int)Service.ServiceTypes.SoupKitchens
										  select 1).Count()

						  let skrDuplicates = (from sk in _db.SoupKitchensReports
											   where sk.Client.MasterIdClcd == client.MasterIdClcd
											   where sk.SubReport.MainReportId == subReport.MainReportId
											   from skv in sk.SKMembersVisits
											   where sk.SubReport.AppBudgetService.Service.TypeId == (int)Service.ServiceTypes.SoupKitchens
											   select 1).Count()
						  let reported = (from o in objectSet
										  where o.ImportId == id
										  where o.SubReportId == item.SubReportId
										  where o.ClientId == client.Id
										  where o.Date >= subReport.MainReport.Start && o.Date < subReport.MainReport.End
										  let alreadyReported = (
										  from sk in _db.SoupKitchensReports
										  where sk.SubReportId == o.SubReportId
										  where sk.ClientId == o.ClientId
										  from skv in sk.SKMembersVisits
										  where skv.ReportDate == o.Date
										  select 1)
										  where !alreadyReported.Any()
										  select o.Date).Count()
						  let totalReported = skrCount + skrDuplicates + reported
						  let daysCount = SqlFunctions.DateDiff("DAY", (client.JoinDate > subReport.MainReport.Start ? client.JoinDate : subReport.MainReport.Start),
														(client.LeaveDate.HasValue && client.LeaveDate.Value < subReport.MainReport.End ? client.LeaveDate : subReport.MainReport.End))
						  let hcep = (from h in _db.HomeCareEntitledPeriods
									  where h.ClientId == client.Id && h.StartDate < subReport.MainReport.End && (h.EndDate == null || h.EndDate > subReport.MainReport.Start)
									  let earlyEndDate = h.EndDate < client.LeaveDate || h.EndDate != null && client.LeaveDate == null ? h.EndDate : client.LeaveDate
									  let maxStartDate = h.StartDate >= client.JoinDate ? h.StartDate : client.JoinDate
									  select new
									  {
										  h.ClientId,
										  StartDate = maxStartDate >= subReport.MainReport.Start ? maxStartDate : subReport.MainReport.Start,
										  EndDate = earlyEndDate < subReport.MainReport.End ? earlyEndDate : subReport.MainReport.End
									  } into hc
									  group hc by hc.ClientId into hcg
									  select new
									  {
										  ClientId = hcg.Key,
										  DaysCount = hcg.Sum(f => SqlFunctions.DateDiff("DAY", f.StartDate, f.EndDate))
									  }).FirstOrDefault()
						  let daysCap = hcep.DaysCount < daysCount ? (hcep.DaysCount < 0 ? 0 : hcep.DaysCount) : (daysCount < 0 ? 0 : daysCount)
						  select new CriSoupKitchensPreview
						  {
							  RowIndex = item.RowIndex,
							  ClientId = item.ClientId,
							  Date = item.Date,
							  ClientName = client.AgencyId != subReport.AppBudgetService.AgencyId ? "" : client.FirstName + " " + client.LastName,
							  Errors = (item.Errors ??
								(

								  ((item.Date == null) ? "Report Date is required. " : "") +
								  ((item.Date < subReport.MainReport.Start || item.Date >= subReport.MainReport.End) ? "Invalid Date (report date is outside of the report period). " : "") +
								  (client.JoinDate > subReport.MainReport.End ? "Client has joined after report end date. " : "") +
								  (client.DeceasedDate == null && client.LeaveDate < subReport.MainReport.Start ? "Client has left before report start date. " : "") +
								  (client.DeceasedDate != null && client.DeceasedDate < subReport.MainReport.Start ? "Client has DOD before report start date. " : "") +
								  (client.JoinDate > item.Date ? "Client has joined after report date. " : "") +
								  (client.DeceasedDate == null && client.LeaveDate < item.Date ? "Client has left before report date. " : "") +
								  (client.DeceasedDate != null && client.DeceasedDate < item.Date ? "Client has DOD before report date. " : "") +
								  ((client == null) ? "Invalid ClientId. " : "") +
								  ((totalReported > daysCap) ? 
								  "CC ID " + SqlFunctions.StringConvert((decimal?)client.Id) + " has been reported for more meals than are allowed in this reporting period. Allowed meals – " + SqlFunctions.StringConvert((decimal?)daysCap) + ", Reported meals - " + SqlFunctions.StringConvert((decimal?)totalReported) + ". " 
								  
								  : ""
								  ) +
								  ((client.AgencyId != subReport.AppBudgetService.AgencyId) ? "Invalid Agency" : "")
												))

						  });



			return source;
		}
		public override void Import(Guid importId)
		{
			using (var db = getDbContext())
			{
				var result = db.ImportCriSoupKitchens(importId);
			}
		}
	}

	class CriSoupKitchensPreview : CriSoupKitchens, ICriPreview
	{
		public string ClientName { get; set; }
		public string TypeName { get; set; }
	}

	public class CriSoupKitchensCsvMap : CsvHelper.Configuration.CsvClassMap<CriSoupKitchens>
	{
		public CriSoupKitchensCsvMap()
			: base()
		{
			Map(m => m.ClientId).Name("CC_ID", "ClientId");
			Map(m => m.Date).Name("Report Date");


		}
	}
}