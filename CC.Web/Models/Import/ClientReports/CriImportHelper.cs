using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CC.Data;

namespace CC.Web.Models.Import.ClientReports
{
	class CriImportHelper : ImportHelperBase<Cri, CriPreview>, IClientReportImportHelper
	{
		public CriImportHelper(CC.Data.Services.IPermissionsBase permissions)
			: base(permissions)
		{
			this.columns = new List<jQueryDataTableColumn>()
			{
				new jQueryDataTableColumn()	{sName= "RowIndex", sTitle="Row Index"}, 
				new jQueryDataTableColumn()	{sName="ClientId",sTitle= "CC ID"}, 
				new jQueryDataTableColumn(){sName="ClientName", sTitle="Client Name"},
				new jQueryDataTableColumn() {sName= "Remarks", sTitle="Remarks"},
				new jQueryDataTableColumn(){sName="Errors", sTitle="Errors"} 
			};
		}



		protected override System.Linq.Expressions.Expression<Func<CriPreview, bool>> GetSearchFilter(string s)
		{
			return f =>
						(System.Data.Objects.SqlClient.SqlFunctions.StringConvert((decimal)f.RowIndex) ?? "").Contains(s) ||
						(System.Data.Objects.SqlClient.SqlFunctions.StringConvert((decimal)f.ClientId) ?? "").Contains(s) ||
						(f.ClientName ?? "").Contains(s);
		}


		protected override IQueryable<CriPreview> PreviewQuery(Guid id, ccEntities _db)
		{
			var objectSet = _db.CriBases.OfType<Cri>();
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
                          select new CriPreview
						  {
							  RowIndex = item.RowIndex,
							  ClientId = item.ClientId,
							  ClientName = client.AgencyId != subReport.AppBudgetService.AgencyId ? "" : client.FirstName + " " + client.LastName,
							  Remarks = item.Remarks,
							  Errors = item.Errors ??
							  (
									((client == null) ? "Invalid ClientId" : "") +
                                    ((client.ApprovalStatusId == 1024 && subreportservicetypeid != 8) ? "Approved, Homecare Only clients can only be reported for Homecare services." : "") + " " +
                                    ((client.AgencyId != subReport.AppBudgetService.AgencyId) ? "Invalid Agency" : "") +
									( ldx < start ? "The Client has left the agency." : "" ) + " " +
									( 
										(client.LeaveDate < start && ldx >= start && (item.Remarks == null || item.Remarks.Trim().Length == 0)) ? 
											"Deceased Client needs to be specified with Unique Circumstances." : ""
									) +
                                    (client.JoinDate > end ? "Client has joined after report end date" : "") +
                                    (client.DeceasedDate == null && client.LeaveDate < start ? "Client has left before report start date" : "") + " " +
                                    (client.DeceasedDate != null && client.DeceasedDate < start ? "Client has DOD before report start date" : "")

									
							  )

						  });
			return source.OrderByDescending(c => c.Errors); 
		}


		protected override void SetMapping(CsvHelper.Configuration.CsvConfiguration conf)
		{
			conf.ClassMapping<CriCsvMap>();
		}

		public override void Import(Guid importId)
		{
			using (var db = getDbContext())
			{
				var result = db.ImportCri(importId);
			}
		}

	}

	public class CriCsvMap : CsvHelper.Configuration.CsvClassMap<CC.Data.Cri>
	{
		public CriCsvMap()
		{
			Map(f => f.ClientId).Name("CC_ID", "ClientId");
			Map(f => f.Remarks).Name("Remarks");
		}
	}


	class CriPreview : CC.Data.Cri, ICriPreview
	{
		public string ClientName { get; set; }
	}
}
