using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CC.Data;
using CsvHelper.Configuration;

namespace CC.Web.Models.Import.ClientReports
{
    class CriSupportiveCommunitiesHelper : ImportHelperBase<CC.Data.CriSupportiveCommunities, CriSupportiveCommunitiesPreview>, IClientReportImportHelper
    {

        public CriSupportiveCommunitiesHelper(CC.Data.Services.IPermissionsBase permissions)
            : base(permissions)
        {
            this.columns = new List<jQueryDataTableColumn>(){
				new jQueryDataTableColumn()	{sName= "RowIndex", sTitle="Row Index"}, 
				new jQueryDataTableColumn()	{sName="ClientId", sTitle= "CC ID"}, 
			    new jQueryDataTableColumn() {sName="HoursHoldCost", sTitle="Household cost"},
				new jQueryDataTableColumn() {sName="MonthsCount", sTitle="Months reported"},
               	new jQueryDataTableColumn() {sName="Errors", sTitle="Errors"} 
			};
        }




        protected override void SetMapping(CsvConfiguration conf)
        {
            conf.ClassMapping<CriSupportiveCommunitiesCsvMap>();
        }

        protected override System.Linq.Expressions.Expression<Func<CriSupportiveCommunitiesPreview, bool>> GetSearchFilter(string s)
        {
            return base.GetSearchFilter(s);
        }
        protected override IQueryable<CriSupportiveCommunitiesPreview> PreviewQuery(Guid id, CC.Data.ccEntities _db)
        {

            var objectSet = _db.CriBases.OfType<CC.Data.CriSupportiveCommunities>();
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
						  let duplicate = objectSet.Where(f => f.ClientId == item.ClientId && f.ImportId == id).Count() > 1
                          select new CriSupportiveCommunitiesPreview
                          {
                              RowIndex = item.RowIndex,
                              ClientId = item.ClientId,

                              HoursHoldCost = item.HoursHoldCost,
							  MonthsCount = item.MonthsCount,
                              ClientName = client.AgencyId != subReport.AppBudgetService.AgencyId ? "" : client.FirstName + " " + client.LastName,


                              Errors = (item.Errors ??
                                (

                                  ((item.HoursHoldCost == null) ? "Household Cost is required." : "") +
                                  (client.JoinDate > end ? "Client has joined after report end date" : "") +
                                  (client.DeceasedDate == null && client.LeaveDate < start ? "Client has left before report start date" : "") +
                                  (client.DeceasedDate != null && client.DeceasedDate < start ? "Client has DOD before report start date" : "") +
                                  ((client == null) ? "Invalid ClientId" : "") +
                                  ((client.AgencyId != subReport.AppBudgetService.AgencyId) ? "Invalid Agency" : "") +
								  (duplicate ? "There is a duplicate row for this client" : "")
                                                ))

                          });



            return source;
        }
        public override void Import(Guid importId)
        {
            using (var db = getDbContext())
            {
                var result = db.ImportCriSupportiveCommunities(importId);
            }
        }
    }

    class CriSupportiveCommunitiesPreview : CC.Data.CriSupportiveCommunities, ICriPreview
    {
        public string ClientName { get; set; }
        public string TypeName { get; set; }
    }

    public class CriSupportiveCommunitiesCsvMap : CsvHelper.Configuration.CsvClassMap<CriSupportiveCommunities>
    {
        public CriSupportiveCommunitiesCsvMap()
            : base()
        {
            Map(m => m.ClientId).Name("CC_ID", "ClientId");
            Map(m => m.HoursHoldCost).Name("Household Monthly Amount");
			Map(m => m.MonthsCount).Name("Months Reported");
        }
    }

}
