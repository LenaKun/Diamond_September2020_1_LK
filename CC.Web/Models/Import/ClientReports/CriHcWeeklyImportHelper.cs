using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CC.Web.Models.Import.ClientReports
{
	class CriHcWeeklyImportHelper : CriHcImportHelper
	{
		public CriHcWeeklyImportHelper(CC.Data.Services.IPermissionsBase permissions)
			: base(permissions)
		{
			this.columns = new List<jQueryDataTableColumn>()
			{
				new jQueryDataTableColumn()	{sName= "RowIndex", sTitle= "Row Index"}, 
				new jQueryDataTableColumn()	{sName= "ClientId", sTitle= "CC ID"}, 
				new jQueryDataTableColumn() {sName= "ClientName", sTitle = "Client Name"},
				new jQueryDataTableColumn() {sName= "Week", sTitle="Week"},
				new jQueryDataTableColumn() {sName= "Rate", sTitle="Rate"},
				new jQueryDataTableColumn() {sName= "Quantity", sTitle="Quantity"},
				new jQueryDataTableColumn() {sName= "Remarks", sTitle="Remarks"},
				new jQueryDataTableColumn() {sName= "Errors", sTitle="Errors"} 
			};
		}
		protected override void SetMapping(CsvHelper.Configuration.CsvConfiguration conf)
		{
			conf.ClassMapping<CriHcWeeklyCsvMap>();
		}
	}

	public class CriHcWeeklyCsvMap : CsvHelper.Configuration.CsvClassMap<CC.Data.CriHc>
	{
		public CriHcWeeklyCsvMap()
		{
			Map(f => f.ClientId).Name("CC_ID", "CCID", "CC ID", "ClientID");
			Map(m => m.Date).Name("Date");
			Map(f => f.Rate).Name("Rate");
			Map(f => f.Quantity).Name("Quantity");
			Map(f => f.Remarks).Name("Remarks");
		}
	}
}