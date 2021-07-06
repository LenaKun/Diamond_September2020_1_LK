using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CC.Data;
using CsvHelper.Configuration;

namespace CC.Web.Models.Import.ClientReports
{
    class CriClientEventsCountHelper : ImportHelperBase<CC.Data.CriClientEventsCount, CriClientEventsCountPreview>, IClientReportImportHelper
    {
        public CriClientEventsCountHelper(CC.Data.Services.IPermissionsBase permissions)
            : base(permissions)
        {
            this.columns = new List<jQueryDataTableColumn>(){
				new jQueryDataTableColumn()	{sName= "RowIndex", sTitle="Row Index"}, 
				new jQueryDataTableColumn()	{sName="EventDate", sTitle= "Date of Event"}, 
			    new jQueryDataTableColumn() {sName="JNVCount", sTitle="Count of JNV attending"},
                new jQueryDataTableColumn() {sName="TotalCount", sTitle="Count of Total Attendees"},
                new jQueryDataTableColumn() {sName="Remarks", sTitle="Remarks"},
               	new jQueryDataTableColumn() {sName="Errors", sTitle="Errors"} 
			};
        }




        protected override void SetMapping(CsvConfiguration conf)
        {
            conf.ClassMapping<CriClientEventsCountCsvMap>();
        }

        protected override System.Linq.Expressions.Expression<Func<CriClientEventsCountPreview, bool>> GetSearchFilter(string s)
        {
            return base.GetSearchFilter(s);
        }
        protected override IQueryable<CriClientEventsCountPreview> PreviewQuery(Guid id, CC.Data.ccEntities _db)
        {

            var objectSet = _db.CriBases.OfType<CC.Data.CriClientEventsCount>();
            var Clients = this.GetClients(_db);
            var SubReports = this.GetSubReports(_db);
            var source = (from item in objectSet
                          where item.ImportId == id
                          join subReport in SubReports on item.SubReportId equals subReport.Id into srg
                          from subReport in srg.DefaultIfEmpty()
                          
                          select new CriClientEventsCountPreview
                          {
                              RowIndex = item.RowIndex,
                              EventDate = item.EventDate,
                              JNVCount = item.JNVCount,
                              TotalCount = item.TotalCount,
                              Remarks = item.Remarks,
                              Errors = (item.Errors ??
                                (
                                  ((item.EventDate == null) ? "Date of Event is required." : "") +
                                  ((item.JNVCount == null) ? "Count of JNV attending is required." : "") +
                                  ((item.EventDate < subReport.MainReport.Start || item.EventDate >= subReport.MainReport.End) ? "Date of Event must be within the reporting period." : "") +
                                  ((item.JNVCount <= 0) ? "Count of JNV attending must be bigger than zero." : "") +
                                  ((item.TotalCount != null && item.TotalCount < item.JNVCount) ? "Count of Total Attendees must be greater or equal to Count of JNV attending." : "")
                                                ))

                          });



            return source;
        }
        public override void Import(Guid importId)
        {
            using (var db = getDbContext())
            {
                var result = db.ImportCriClientEventsCount(importId);
            }
        }
    }

    class CriClientEventsCountPreview : CC.Data.CriClientEventsCount, ICriPreview
    {
        public DateTime? EventDate { get; set; }
        public int? JNVCount { get; set; }
        public int? TotalCount { get; set; }
        public string Remarks { get; set; }
    }

    public class CriClientEventsCountCsvMap : CsvHelper.Configuration.CsvClassMap<CriClientEventsCount>
    {
        public CriClientEventsCountCsvMap()
            : base()
        {
            Map(m => m.EventDate).Name("EventDate", "Date of Event");
            Map(m => m.JNVCount).Name("JNVCount", "Count of JNV attending");
            Map(m => m.TotalCount).Name("TotalCount", "Count of Total Attendees");
            Map(m => m.Remarks).Name("EventDate", "Remarks");
        }
    }
}