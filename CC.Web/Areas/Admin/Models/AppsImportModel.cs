using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using CC.Data;
using CsvHelper;
using System.Web;

namespace CC.Web.Areas.Admin.Models
{

	public class AppsImportModel
	{
		public AppsImportModel()
		{
			Id = Guid.NewGuid();
		}

		public AppsImportModel(Guid id)
		{
			this.Id = id;
		}
		public Guid Id { get; set; }
		public string tempTableName { get { return string.Format("##{0}", Id); } }

		public void Upload(HttpPostedFileBase file)
		{
			var csvconf = new CsvHelper.Configuration.CsvConfiguration()
			{
				IsStrictMode = false,
				SkipEmptyRecords = true,
				IsCaseSensitive = false
			};

			csvconf.SkipEmptyRecords = true;
			csvconf.ClassMapping<AppImportRowModel.CsvMap>();
			
			var csvReader = new CsvReader(new System.IO.StreamReader(file.InputStream), csvconf);

			var csvRows = csvReader.GetRecords<AppsImport>().Select(f =>
			{
				f.Id = this.Id;
				f.InterlineTransfer = f.InterlineTransfer ?? false;
				f.EndOfYearValidationOnly = f.EndOfYearValidationOnly ?? false;
				return f;
			});


			var idatareader = CC.Web.Helpers.idrf.GetReader(csvRows);

			var sqlBulkCopy = GetSqlBulkCopy();

			sqlBulkCopy.WriteToServer(idatareader);

		}

		private static SqlBulkCopy GetSqlBulkCopy()
		{
			var sqlBulkCopy = new SqlBulkCopy(ConnectionStringHelper.GetProviderConnectionString());

			sqlBulkCopy.DestinationTableName = "AppsImport";
			sqlBulkCopy.ColumnMappings.Add("Id", "Id");
			sqlBulkCopy.ColumnMappings.Add("FundId", "FundId");
			sqlBulkCopy.ColumnMappings.Add("AgencyGroupId", "AgencyGroupId");
			sqlBulkCopy.ColumnMappings.Add("Name", "Name");
			sqlBulkCopy.ColumnMappings.Add("AgencyContribution", "AgencyContribution");
			sqlBulkCopy.ColumnMappings.Add("CcGrant", "CcGrant");
			sqlBulkCopy.ColumnMappings.Add("RequiredMatch", "RequiredMatch");
			sqlBulkCopy.ColumnMappings.Add("StartDate", "StartDate");
			sqlBulkCopy.ColumnMappings.Add("EndDate", "EndDate");
			sqlBulkCopy.ColumnMappings.Add("CurrencyId", "CurrencyId");
			sqlBulkCopy.ColumnMappings.Add("USDRate", "USDRate");
			sqlBulkCopy.ColumnMappings.Add("ILSRate", "ILSRate");
			sqlBulkCopy.ColumnMappings.Add("EURRate", "EURRate");
			sqlBulkCopy.ColumnMappings.Add("MaxNonHcAmount", "MaxNonHcAmount");
			sqlBulkCopy.ColumnMappings.Add("MaxAdminAmount", "MaxAdminAmount");
			sqlBulkCopy.ColumnMappings.Add("HistoricalExpenditureAmount", "HistoricalExpenditureAmount");
			sqlBulkCopy.ColumnMappings.Add("AvgReimbursementCost", "AvgReimbursementCost");
			sqlBulkCopy.ColumnMappings.Add("EndOfYearValidationOnly", "EndOfYearValidationOnly");
			sqlBulkCopy.ColumnMappings.Add("InterlineTransfer", "InterlineTransfer");

			return sqlBulkCopy;
		}


		public object GetPreview(ccEntities db, CC.Web.Models.jQueryDataTableParamModel jq)
		{

			var q = PreviewData(db);


			var filtered = q;
			if (!string.IsNullOrEmpty(jq.sSearch))
			{
				filtered = filtered.Where(f => f.FundName.Contains(jq.sSearch)
					|| f.AgencyGroupName.Contains(jq.sSearch)
					|| f.Name.Contains(jq.sSearch)
					|| f.CurrencyId.Equals(jq.sSearch));
			}

			var ordered = filtered.OrderByField(jq.sSortCol_0, jq.bSortDir_0);

			return new CC.Web.Models.jQueryDataTableResult<object>
			{
				aaData = ordered.Skip(jq.iDisplayStart).Take(jq.iDisplayLength).ToList(),
				iTotalDisplayRecords = filtered.Count(),
				iTotalRecords = q.Count(),
				sEcho = jq.sEcho
			};

		}

		public IQueryable<AppImportRowModel> PreviewData(ccEntities db)
		{
			var q = from i in db.AppsImports
					join f in db.Funds on i.FundId equals f.Id into fg
					from f in fg.DefaultIfEmpty()
					join ag in db.AgencyGroups on i.AgencyGroupId equals ag.Id into agg
					from ag in agg.DefaultIfEmpty()
					join cur in db.Currencies on i.CurrencyId equals cur.Id into curg
					from cur in curg.DefaultIfEmpty()
					where i.Id == Id
					select new AppImportRowModel
					{
						FundName = f.Name,
						AgencyGroupName = ag.Name,
						Name = i.Name,
						AgencyContribution = i.AgencyContribution,
						CcGrant = i.CcGrant,
						RequiredMatch = i.RequiredMatch,
						MaxAdminAmount = i.MaxAdminAmount,
						MaxNonHcAmount = i.MaxNonHcAmount,
						HistoricalExpenditureAmount = i.HistoricalExpenditureAmount,
						AvgReimbursementCost = i.AvgReimbursementCost,
						Year = System.Data.Objects.SqlClient.SqlFunctions.DatePart("year", i.StartDate),
						StartDate = i.StartDate,
						EndDate = i.EndDate,
						CurrencyId = i.CurrencyId,
						USDRate = i.USDRate,
						ILSRate = i.ILSRate,
						EURRate = i.EURRate,
						EndOfYearValidationOnly = i.EndOfYearValidationOnly,
						InterlineTransfer = i.InterlineTransfer,
						Errors =
						
						(
							(
								db.AppsImports
									.Where(a => a.Id == Id && a.RowId!=i.RowId)
									.Any(a => a.Name == i.Name)
							) ? "Duplicate Name." : string.Empty
						) +
						(
							(f == null) ? "Fund is required. " : string.Empty
						) +
						(
							(ag == null) ? "Ser is required. " : string.Empty
						) +
						(
							(cur == null) ? "Currency is required. " : string.Empty
						) +
						(
							(i.Name == null)? "Name is Required. ":string.Empty
						)+
						(
							(i.AgencyContribution == null)? "Agency Contribution is Required. ":string.Empty
						)+
						(
							(i.CcGrant == null)? "CcGrant is Required. ":string.Empty
						)+
						(
							(i.RequiredMatch == null)? "Required Match is Required. ":string.Empty
						)+
						(
							(i.StartDate == null)? "Calendaric Year is Required. ":string.Empty
						)+
						(
							i.MaxAdminAmount <= 0? "Total Admin allowed amount must be empty or greater than zero": string.Empty
						) +
						(
							i.MaxNonHcAmount <= 0 ? "Total None homecare allowed amount must be empty or greater than zero" : string.Empty
						) +
						(
							i.HistoricalExpenditureAmount > i.CcGrant? "Historical Expenditure Amount must be less than the CC Grant.":string.Empty
						)+
						(
							!(
								(i.MaxNonHcAmount == null && i.MaxAdminAmount == null)
								|| (i.MaxAdminAmount!=null && i.MaxNonHcAmount != null)
							)
							? "Total Admin allowed amount and Total None homecare allowed amount both empty or both non empty." : string.Empty
						)
						,


					};
			return q;
		}

		public class CsvRow
		{

		}

		internal void Import(Guid id)
		{
			throw new NotImplementedException();
		}
	}



	public class AppImportRowModel : AppsImport
	{
		public string FundName { get; set; }

		public string AgencyGroupName { get; set; }

		public string Errors { get; set; }
		
		public class CsvMap : CsvHelper.Configuration.CsvClassMap<AppsImport>
		{
			public CsvMap()
			{
				Map(f => f.FundId).Name("Fund");
				Map(f => f.AgencyGroupId).Name("Ser");
				Map(f => f.CurrencyId).Name("Currency");
				Map(f => f.Name).Name("Name");
				Map(f => f.AgencyContribution).Name("Agency Contribution");
				Map(f => f.CcGrant).Name("CcGrant");
				Map(f => f.RequiredMatch).Name("Required Match");
				Map(f => f.Year).Name("Calendaric Year");
				Map(f => f.USDRate).Name("USD Rate");
				Map(f => f.ILSRate).Name("ILS Rate");
				Map(f => f.EURRate).Name("EUR Rate");
				Map(f => f.EndOfYearValidationOnly).Name("Only EOY validation");
				Map(f => f.InterlineTransfer).Name("Interline Transfer");
				Map(f => f.MaxAdminAmount).Name("Total Admin allowed");
				Map(f => f.MaxNonHcAmount).Name("Total all NONE Homecare services amount allowed");
				Map(f => f.HistoricalExpenditureAmount).Name("Historical Expenditure Amount");
				Map(f => f.AvgReimbursementCost).Name("Average Reimbursement Cost");
			}
		}
	}
}