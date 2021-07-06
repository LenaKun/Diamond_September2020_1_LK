using CC.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CC.Web.Models.Import.ClientReports
{
	class CriClientNamesAndCostsHelper : ImportHelperBase<CC.Data.CriClientNamesAndCosts, CriClientNamesAndCostsPreview>, IClientReportImportHelper
	{
		public CriClientNamesAndCostsHelper(CC.Data.Services.IPermissionsBase permissions)
			: base(permissions)
		{
			this.columns = new List<jQueryDataTableColumn>(){
				new jQueryDataTableColumn()	{sName= "RowIndex", sTitle="Row Index"}, 
				new jQueryDataTableColumn()	{sName="ClientId", sTitle= "CC ID"}, 
				new jQueryDataTableColumn() {sName="ClientName", sTitle="Client Name"},
				new jQueryDataTableColumn() {sName="TotalAmount", sTitle="Amount"},
				new jQueryDataTableColumn() {sName= "Remarks", sTitle="Remarks"},
				new jQueryDataTableColumn() {sName="Errors", sTitle="Errors"} 
			};
		}


		protected override System.Linq.Expressions.Expression<Func<CriClientNamesAndCostsPreview, bool>> GetSearchFilter(string s)
		{
			return base.GetSearchFilter(s);
		}
		protected override IQueryable<CriClientNamesAndCostsPreview> PreviewQuery(Guid id, CC.Data.ccEntities _db)
		{
			var objectSet = _db.CriBases.OfType<CC.Data.CriClientNamesAndCosts>();
			var Clients = this.GetClients(_db);
			var SubReports = this.GetSubReports(_db);
			DateTime JoinDateToCompare = new DateTime(2018, 1, 1);
			var source = (from item in objectSet
						  where item.ImportId == id
						  join client in Clients on item.ClientId equals client.Id into clientsGroup
						  from client in clientsGroup.DefaultIfEmpty()
						  join subReport in SubReports on item.SubReportId equals subReport.Id into srg
						  from subReport in srg.DefaultIfEmpty()
						  let ldx = System.Data.Objects.EntityFunctions.AddDays(client.LeaveDate, client.DeceasedDate == null ? 0 : 90)
						  let start = subReport.MainReport.Start
                          let end = subReport.MainReport.End
                          let subreportservicetypeid = subReport.AppBudgetService.Service.TypeId
                          select new CriClientNamesAndCostsPreview
						  {
							  RowIndex = item.RowIndex,
							  ClientId = item.ClientId,
							  TotalAmount = item.TotalAmount,
							  ClientName = client.AgencyId != subReport.AppBudgetService.AgencyId ? "" : client.FirstName + " " + client.LastName,
                              Errors = (item.Errors ??
							  (
									((item.TotalAmount == null) ? "Amount is required." : "") +
                                    ((client.ApprovalStatusId == 1024 && subreportservicetypeid != 8) ? "Approved, Homecare Only clients can only be reported for Homecare services " :  "") +
                                    ((client == null) ? "Invalid ClientId" : "") +
                                    (client.JoinDate > end ? "Client has joined after report end date" : "") +
                                    (client.DeceasedDate == null && client.LeaveDate < start ? "Client has left before report start date" : "") +
                                    (client.DeceasedDate != null && client.DeceasedDate < start ? "Client has DOD before report start date" : "") +
                                    ((client.AgencyId != subReport.AppBudgetService.AgencyId) ? "Invalid Agency" : "")+
									( ldx < start ? "The Client has left the agency." : "" ) +
									( 
										(client.LeaveDate < start && ldx >= start && (item.Remarks == null || item.Remarks.Trim().Length == 0)) ? 
											"Deceased Client needs to be specified with Unique Circumstances." : ""
									)
								)
							  )

						  });
			return source;
		}
		public override void Import(Guid importId)
		{
			using (var db = getDbContext())
			{
				var result = db.ImportCri(importId);
			}
		}
		protected override void SetMapping(CsvHelper.Configuration.CsvConfiguration conf)
		{
			conf.ClassMapping<CriClientNamesAndCostsCsvMap>();
		}
	}

	public class CriClientNamesAndCostsCsvMap : CsvHelper.Configuration.CsvClassMap<CC.Data.CriClientNamesAndCosts>
	{
		public CriClientNamesAndCostsCsvMap()
			: base()
		{
			Map(f => f.ClientId).Name("CC_ID", "ClientId");
			Map(f => f.TotalAmount).Name("Amount", "TotalAmount");
			Map(f => f.Remarks).Name("Remarks");
		}
	}

	public class CriClientNamesAndCostsPreview : CC.Data.CriClientNamesAndCosts, ICriPreview
	{
		public string ClientName { get; set; }
	}
}
