using CC.Data;
using CC.Web.Helpers;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace CC.Web.Models
{
	public class UnmetNeedsOtherUploadModel
	{
		public Guid id = Guid.NewGuid();

		public System.Web.HttpPostedFileBase file { get; set; }

		public bool NewRows { get; set; }

		public virtual IEnumerable<string> GetCsvColumnNames()
		{
			return CsvHelper.CsvHelperExtenstions.ColumnHeaderNames<CsvMap>();
		}

		public virtual Dictionary<int, string> GetAllowedServiceTypes()
		{
			using(var db = new ccEntities())
			{
				return db.ServiceTypes.Where(f => !f.DoNotReportInUnmetNeedsOther).Select(f => new { Id = f.ServiceTypeImportId ?? f.Id, Name = f.Name }).ToDictionary(f => f.Id, f => f.Name);
			}
		}

		public void Import()
		{
			var csvConf = new CsvHelper.Configuration.CsvConfiguration()
			{
				IsStrictMode = true,
				IsCaseSensitive = false,
				SkipEmptyRecords = true,
			};

			csvConf.ClassMapping<CsvMap>();

			using (var csvReader = new CsvReader(new System.IO.StreamReader(file.InputStream), csvConf))
			{
				var records = csvReader.GetRecords<UnmetNeedsOther>();
				var data = records.Select((f, i) => new ImportUnmetNeedsOther
				{
					RowIndex = i + 2,
					ImportId = this.id,
					ClientId = f.ClientId,
					ServiceTypeImportId = f.ServiceTypeId,
					Amount = f.Amount
				});

				string connectionString = ConnectionStringHelper.GetProviderConnectionString();


				using (var sqlBulkCopy = new System.Data.SqlClient.SqlBulkCopy(connectionString))
				{
					sqlBulkCopy.DestinationTableName = "dbo.ImportUnmetNeedsOther";
					sqlBulkCopy.ColumnMappings.Add("ImportId", "ImportId");
					sqlBulkCopy.ColumnMappings.Add("RowIndex", "RowIndex");
					sqlBulkCopy.ColumnMappings.Add("ClientId", "ClientId");
					sqlBulkCopy.ColumnMappings.Add("ServiceTypeImportId", "ServiceTypeImportId");
					sqlBulkCopy.ColumnMappings.Add("Amount", "Amount");
					try
					{
						sqlBulkCopy.WriteToServer(idrf.GetReader(data));
					}
					catch
					{
						throw;
					}

				}

			}
		}



		private class CsvMap : CsvHelper.Configuration.CsvClassMap<UnmetNeedsOther>
		{
			public CsvMap()
			{
				Map(f => f.ClientId).Name(CC.Data.Client.ccidColumnName);
				Map(f => f.ServiceTypeId).Name("ServiceTypeImportId");
				Map(f => f.Amount).Name("Amount");
			}
		}
	}

	public class UnmetNeedsOtherImportPreviewModel : System.ComponentModel.DataAnnotations.IValidatableObject
	{
		public Guid Id { get; set; }
		public CC.Data.Services.IPermissionsBase Permissions { get; set; }
		public ccEntities db { get; set; }

		public bool NewRows { get; set; }


		public IEnumerable<System.ComponentModel.DataAnnotations.ValidationResult> Validate(System.ComponentModel.DataAnnotations.ValidationContext validationContext)
		{
			if (db != null && Permissions != null)
			{

				var q = UnmetNeedsOtherImportPreviewDataModel.PreviewQuery(db, Permissions, Id, this.NewRows);
				if (q.Count(f => f.RowErrors.Any == true) > 0)
				{
					yield return new ValidationResult("One or more rows are invalid.");
				}
			}
		}

	}

	public class UnmetNeedsOtherImportPreviewDataModel : jQueryDataTableParamModel
	{
		public Guid Id { get; set; }
		public CC.Data.Services.IPermissionsBase Permissions { get; set; }
		public static IQueryable<UnmetDataRow> PreviewQuery(ccEntities db, CC.Data.Services.IPermissionsBase Permissions, Guid Id, bool newRows)
		{

			var maxDate = DateTime.Now.Date;
			var source =
				from item in
					(
					from n in db.ImportUnmetNeedsOthers
					where n.ImportId == Id
					join c in db.Clients.Where(Permissions.ClientsFilter) on n.ClientId equals c.Id into cg
					join d in db.ImportUnmetNeedsOthers.Where(f => f.ImportId == Id) on new { ClientId = n.ClientId, ServiceTypeImportId = n.ServiceTypeImportId } equals
					   new { ClientId = d.ClientId, ServiceTypeImportId = d.ServiceTypeImportId } into dg
					join de in db.UnmetNeedsOthers on new { ClientId = n.ClientId, ServiceTypeImportId = n.ServiceTypeImportId } equals
					   new { ClientId = de.ClientId, ServiceTypeImportId = de.ServiceTypeId } into deg
					from c in cg.DefaultIfEmpty()
					select new
					{
						RowIndex = n.RowIndex,
						ClientId = n.ClientId,
						FirstName = c.FirstName,
						LastName = c.LastName,
						ServiceTypeId = n.ServiceTypeImportId,
						Amount = n.Amount,
						RowErrors = new
						{
							ClientNotFound = c.Id == null,
							InvalidAmount = n.Amount < 0,
							DuplicatesRowIndexes = dg.Where(f => f.RowIndex != n.RowIndex).Select(f => f.RowIndex),
							DuplicateEntries = deg.Any() && newRows,
							InvalidServiceType = db.ServiceTypes.Any(f => f.Id == n.ServiceTypeImportId && f.DoNotReportInUnmetNeedsOther)
						}
					})
				select new UnmetDataRow
				{
					RowIndex = item.RowIndex,
					ClientId = item.ClientId,
					FirstName = item.FirstName,
					LastName = item.LastName,
					ServiceTypeImportId = item.ServiceTypeId,
					Amount = item.Amount,
					RowErrors = new UnmetDataRow.Errors
					{
						ClientNotFound = item.RowErrors.ClientNotFound,
						DuplicatesRowIndexes = item.RowErrors.DuplicatesRowIndexes,
						DuplicateEntries = item.RowErrors.DuplicateEntries,
						InvalidServiceType = item.RowErrors.InvalidServiceType,
						Any = item.RowErrors.ClientNotFound
							|| item.RowErrors.InvalidAmount
							|| item.RowErrors.DuplicatesRowIndexes.Any()
							|| item.RowErrors.DuplicateEntries
							|| item.RowErrors.InvalidServiceType
					}
				};
			return source;
		}
		public object GetData(bool newRows)
		{
			using (var db = new ccEntities())
			{
				var source = PreviewQuery(db, Permissions, Id, newRows);

				var filtered = source;
				IOrderedQueryable<UnmetDataRow> ordered = null;
				if (this.sSortCol_0 == "RowErrors")
				{
					ordered = source.OrderBy(f => f.RowErrors.Any, this.sSortDir_0 == "asc");
				}
				else { ordered = source.OrderByField(this.sSortCol_0, this.sSortDir_0 == "asc"); }

				return new jQueryDataTableResult
				{
					aaData = ordered.Skip(this.iDisplayStart).Take(this.iDisplayLength).ToList(),
					iTotalDisplayRecords = filtered.Count(),
					iTotalRecords = source.Count(),
					sEcho = sEcho
				};
			}

		}
		public class UnmetDataRow
		{
			public class Errors
			{
				public bool? ClientNotFound { get; set; }

				public bool? InvalidAmount { get; set; }
				public bool? InvalidServiceType { get; set; }

				public IEnumerable<int> DuplicatesRowIndexes { get; set; }

				public bool? DuplicateEntries { get; set; }

				public bool? Any { get; set; }
			}

			public int? ClientId { get; set; }

			public string FirstName { get; set; }

			public string LastName { get; set; }

			public int ServiceTypeImportId { get; set; }

			public decimal Amount { get; set; }

			public Errors RowErrors { get; set; }

			public int RowIndex { get; set; }
		}
	}
}