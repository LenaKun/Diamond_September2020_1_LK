using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CC.Web.Helpers;
using CC.Data;


namespace CC.Web.Areas.Admin.Models
{
	public class MasterIdsImportModel
	{
		public static Import Upload(HttpPostedFileWrapper file, int userId)
		{
			var newImport = NewMethod(userId);

			var csvConf = new CsvHelper.Configuration.CsvConfiguration
			{
				SkipEmptyRecords = true,
				IsCaseSensitive = false,
				IsStrictMode = true
			};
			csvConf.ClassMapping<MasterIdImportCsvMap>();
			var streamReader = new System.IO.StreamReader(file.InputStream);
			using (var csvReader = new CsvHelper.CsvReader(streamReader, csvConf))
			{
				var data = csvReader.GetRecords<ImportClient>().Select((record, i) =>
						   new ImportClient
						   {
							   RowIndex = i,
							   ImportId = newImport.Id,

							   ClientId = record.ClientId,
							   MasterId = record.MasterId,
							   
							   UpdatedAt = newImport.StartedAt,
							   UpdatedById = newImport.UserId,
						   });


				string connectionString = System.Data.SqlClient.ConnectionStringHelper.GetProviderConnectionString();
				var sqlBulkCopy = new System.Data.SqlClient.SqlBulkCopy(connectionString);
				sqlBulkCopy.DestinationTableName = "ImportClients";
				sqlBulkCopy.ColumnMappings.Add("ImportId", "ImportId");
				sqlBulkCopy.ColumnMappings.Add("RowIndex", "RowIndex");
				sqlBulkCopy.ColumnMappings.Add("ClientId", "ClientId");
				sqlBulkCopy.ColumnMappings.Add("MasterId", "MasterId");
				sqlBulkCopy.ColumnMappings.Add("UpdatedAt", "UpdatedAt");
				sqlBulkCopy.ColumnMappings.Add("UpdatedById", "UpdatedById");

				var dataReader = new IEnumerableDataReader<ImportClient>(data);

				sqlBulkCopy.WriteToServer(dataReader);

				sqlBulkCopy.Close();
			}

			return newImport;
		}
		private static Import NewMethod(int userId)
		{
			Guid id = Guid.NewGuid();
			using (var db = new ccEntities())
			{
				var import = new Import()
				{
					Id = id,
					StartedAt = DateTime.Now,
					UserId = userId
				};

				db.Imports.AddObject(import);
				db.SaveChanges();
				return import;
			}
		}
	}

	public class MasterIdImportCsvMap : CsvHelper.Configuration.CsvClassMap<ImportClient>
	{
		public MasterIdImportCsvMap()
		{
			Map(f => f.ClientId).Name("CC_ID", "ID", "CCID", "CC ID", "CLIENT_ID", "CLIENTID", "CLIENT ID");
			Map(f => f.MasterId).Name("MASTER_ID", "MASTERID", "MASTER ID");
		}
	}
}