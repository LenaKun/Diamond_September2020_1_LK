using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CC.Data;
using CsvHelper.Configuration;

namespace CC.Web.Models.Import.ClientReports
{
	class CriEmergencyHelper : ImportHelperBase<CC.Data.CriEmergency, CriEmergencyPreview>, IClientReportImportHelper
	{

		public CriEmergencyHelper(CC.Data.Services.IPermissionsBase permissions)
			: base(permissions)
		{
			this.columns = new List<jQueryDataTableColumn>(){
				new jQueryDataTableColumn()	{sName= "RowIndex", sTitle="Row Index"}, 
				new jQueryDataTableColumn()	{sName="ClientId", sTitle= "CC ID"}, 
				new jQueryDataTableColumn() {sName="ClientName", sTitle="Client Name"},
				new jQueryDataTableColumn()	{sName="Date", sTitle= "Date", sType="date", format="dd MMM yyyy"}, 
				new jQueryDataTableColumn()	{sName="TypeName", sTitle= "Type Name"}, 
				new jQueryDataTableColumn() {sName="Amount", sTitle="Amount"},
				new jQueryDataTableColumn() {sName="Remarks", sTitle="Purpose of grant"},
				new jQueryDataTableColumn() {sName="Discretionary", sTitle="Discretionary"},
                new jQueryDataTableColumn() {sName="UniqueCircumstances", sTitle="Unique Circumstances"},
				new jQueryDataTableColumn() {sName="Errors", sTitle="Errors"} 
			};
		}

		protected override void SetMapping(CsvConfiguration conf)
		{
			conf.ClassMapping<CriEmergencyCsvMap>();
		}

		protected override System.Linq.Expressions.Expression<Func<CriEmergencyPreview, bool>> GetSearchFilter(string s)
		{
			return base.GetSearchFilter(s);
		}
		protected override IQueryable<CriEmergencyPreview> PreviewQuery(Guid id, CC.Data.ccEntities _db)
		{
			
			var objectSet = _db.CriBases.OfType<CC.Data.CriEmergency>();
			var Clients = this.GetClients(_db);
			var SubReports = this.GetSubReports(_db);
			DateTime JoinDateToCompare = new DateTime(2018, 1, 1);
			var source = (from item in objectSet
						  where item.ImportId == id
						  join t in _db.EmergencyReportTypes on item.TypeId equals t.Id into tGroup
						  from t in tGroup.DefaultIfEmpty()
						  join client in Clients on item.ClientId equals client.Id into clientsGroup
						  from client in clientsGroup.DefaultIfEmpty()
						  join subReport in SubReports on item.SubReportId equals subReport.Id into srg
						  from subReport in srg.DefaultIfEmpty()
						  let ldx = System.Data.Objects.EntityFunctions.AddDays(client.LeaveDate, client.DeceasedDate == null ? 0 : SubReport.EAPDeceasedDaysOverhead)
						  let start = item.Date ?? subReport.MainReport.Start
                          let mrStart = subReport.MainReport.Start
                          let end = subReport.MainReport.End
                          let subreportservicetypeid = subReport.AppBudgetService.Service.TypeId
                          select new CriEmergencyPreview
						  {
							  RowIndex = item.RowIndex,
							  ClientId = item.ClientId,
							  Amount = item.Amount,
							  Discretionary = item.Discretionary,
							  Date = item.Date,
							  TypeName = t.Name,
							  ClientName = client.AgencyId != subReport.AppBudgetService.AgencyId ? "" : client.FirstName + " " + client.LastName,
							  Remarks = item.Remarks,
                              UniqueCircumstances = item.UniqueCircumstances,

							  Errors = (item.Errors??
								(
								  ((item.Amount == null) ? "Amount is required." : "") +
                                  ((client.ApprovalStatusId == 1024 && subreportservicetypeid != 8) ? "Approved, Homecare Only clients can only be reported for Homecare services." : "") +
                                  ((item.Discretionary == null) ? "Discretionary amount is required." : "") +
								  ((item.Date == null) ? "Date is required." : "") +
								  ((t == null) ? "Type is required." : "") +
								  ((item.Remarks == null || item.Remarks.Trim()=="")?  "Purpose of grant":"")+
								  ((client == null) ? "Invalid ClientId" : "") +
                                  (client.JoinDate > end ? "Client has joined after report end date" : "") +
                                  (client.DeceasedDate == null && client.LeaveDate < mrStart ? "Client has left before report start date" : "") +
								  ((item.Date < subReport.MainReport.Start || item.Date >= subReport.MainReport.End) ? "Invalid Report Date" : "") +
								  ((client.AgencyId != subReport.AppBudgetService.AgencyId) ? "Invalid Agency" : "") +
                                  ((subReport.AppBudgetService.AppBudget.App.Fund.AustrianEligibleOnly && !client.AustrianEligible) ? "Client Not Austrian Eligible" : "") +
								   ((subReport.AppBudgetService.AppBudget.App.Fund.RomanianEligibleOnly && !client.RomanianEligible) ? "Client Not Romanian Eligible" : "") +
                                  ((client.AustrianEligible && item.Date > client.LeaveDate && (item.UniqueCircumstances == null || item.UniqueCircumstances == "")) ? "Unique Circumstances must be filled" : "") +
								  ((client.RomanianEligible && item.Date > client.LeaveDate && (item.UniqueCircumstances == null || item.UniqueCircumstances == "")) ? "Unique Circumstances must be filled" : "") +
								
								  ((client.JoinDate > subReport.MainReport.End) ? "Client cant be reported because of Join Date" : "") +
									( ldx < start && !client.AustrianEligible && !client.RomanianEligible ? "The Client has left the agency." : "" ) +
									( 
										(client.LeaveDate < start && ldx >= start && (item.Remarks == null || item.Remarks.Trim().Length == 0)) ? 
											"Deceased Client needs to be specified with Unique Circumstances." : ""
									)
								))

						  });
			return source;
		}
		public override void Import(Guid importId)
		{
			using (var db = getDbContext())
			{
				var result = db.ImportCriEmergency(importId);
			}
		}
	}

	class CriEmergencyPreview : CC.Data.CriEmergency, ICriPreview
	{
		public string ClientName { get; set; }
		public string TypeName { get; set; }
	}

	public class CriEmergencyCsvMap : CsvHelper.Configuration.CsvClassMap<CriEmergency>
	{
		public CriEmergencyCsvMap()
			: base()
		{
			Map(m => m.ClientId).Name("CC_ID", "ClientId");
			Map(m => m.Date).Name("Date").TypeConverter<InvariantDateTypeConverter>();
			Map(m => m.Amount).Name("Amount");
			Map(m => m.Discretionary).Name("Discretionary");
			Map(m => m.TypeId).Name("TypeId");
			Map(m => m.Remarks).Name("Purpose of grant").TypeConverter<NullStringTypeConverter>();
            Map(m => m.UniqueCircumstances).Name("Unique Circumstances").TypeConverter<NullStringTypeConverter>();
		}
	}

}
