using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CsvHelper;
using CC.Data;
using System.Data.SqlClient;
using CC.Web.Helpers;
using System.ComponentModel.DataAnnotations;
namespace CC.Web.Models
{
	public class GovHcHoursUploadModel
	{
		public Guid id = Guid.NewGuid();

		public System.Web.HttpPostedFileBase file { get; set; }

		public virtual IEnumerable<string> GetCsvColumnNames()
		{
			return CsvHelper.CsvHelperExtenstions.ColumnHeaderNames<CsvMap>();
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
				var records = csvReader.GetRecords<GovHcHour>();
				var data = records.Select((f, i) => new ImportGovHcHour
				{
					RowIndex = i + 2,
					ImportId = this.id,
					ClientId = f.ClientId,
					StartDate = f.StartDate,
					Value = f.Value
				});

				string connectionString = ConnectionStringHelper.GetProviderConnectionString();


				using (var sqlBulkCopy = new System.Data.SqlClient.SqlBulkCopy(connectionString))
				{
					sqlBulkCopy.DestinationTableName = "dbo.ImportGovHcHours";
					sqlBulkCopy.ColumnMappings.Add("ImportId", "ImportId");
					sqlBulkCopy.ColumnMappings.Add("RowIndex", "RowIndex");
					sqlBulkCopy.ColumnMappings.Add("ClientId", "ClientId");
					sqlBulkCopy.ColumnMappings.Add("StartDate", "StartDate");
					sqlBulkCopy.ColumnMappings.Add("Value", "Value");
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



		private class CsvMap : CsvHelper.Configuration.CsvClassMap<GovHcHour>
		{
			public CsvMap()
			{
				Map(f => f.ClientId).Name(CC.Data.Client.ccidColumnName);
				Map(f => f.StartDate).Name("Start Date").TypeConverter<InvariantDateTypeConverter>();
				Map(f => f.Value).Name("Value");
			}
		}
	}

	public class GovHcHoursImportPreviewModel : System.ComponentModel.DataAnnotations.IValidatableObject
	{
		public Guid Id { get; set; }
		public CC.Data.Services.IPermissionsBase Permissions { get; set; }
		public ccEntities db { get; set; }




		public IEnumerable<System.ComponentModel.DataAnnotations.ValidationResult> Validate(System.ComponentModel.DataAnnotations.ValidationContext validationContext)
		{
			if (db != null && Permissions != null)
			{

				var q = GovHcHoursImportPreviewDataModel.PreviewQuery(db, Permissions, Id);
				if (q.Count(f => f.RowErrors.Any == true) > 0)
				{
					yield return new ValidationResult("One or more rows are invalid.");
				}
			}
		}

	}

	public class GovHcHoursImportPreviewDataModel : jQueryDataTableParamModel
	{
		public Guid Id { get; set; }
		public CC.Data.Services.IPermissionsBase Permissions { get; set; }
		public static IQueryable<DataRow> PreviewQuery(ccEntities db, CC.Data.Services.IPermissionsBase Permissions, Guid Id)
		{

			var maxDate = DateTime.Now.Date;
			var source =
				from item in
					(
					from n in db.ImportGovHcHours
					where n.ImportId == Id
					join c in db.Clients.Where(Permissions.ClientsFilter) on n.ClientId equals c.Id into cg
					join d in db.ImportGovHcHours.Where(f => f.ImportId == Id) on new { ClientId = n.ClientId, StartDate = n.StartDate } equals
					   new { ClientId = (int?)d.ClientId, StartDate = (DateTime?)d.StartDate } into dg
					from c in cg.DefaultIfEmpty()
					join e in db.GovHcHours on
					   new { ClientId = n.ClientId, StartDate = n.StartDate } equals
					   new { ClientId = (int?)e.ClientId, StartDate = (DateTime?)e.StartDate } into eg
					from e in eg.DefaultIfEmpty()
					let lastReport = (from cr in db.ClientReports
									  where cr.ClientId == c.Id
									  join sr in db.SubReports on cr.SubReportId equals sr.Id
									  join mr in db.MainReports.Where(MainReport.Submitted) on sr.MainReportId equals mr.Id
									  where sr.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.Homecare ||
											sr.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.HomecareWeekly
									  select mr).OrderByDescending(f => f.Start).FirstOrDefault()
					let max = lastReport != null ? (DateTime?)lastReport.End : (DateTime?)null
					select new
					{
						RowIndex = n.RowIndex,
						ClientId = n.ClientId,
						FirstName = c.FirstName,
						LastName = c.LastName,
						StartDate = n.StartDate,
						Value = n.Value,
						ExistingValue = e.Value,
						RowErrors = new
						{
							ClientNotFound = c.Id == null,
							NegativeValue = n.Value < 0,
							FutureStartDate = n.StartDate > maxDate,
							InvalidStartDate = n.StartDate < c.JoinDate || n.StartDate > c.LeaveDate,
							DuplicatesRowIndexes = dg.Where(f => f.RowIndex != n.RowIndex).Select(f => f.RowIndex),
							StartDateBeforeReportDate = Permissions.User.RoleId != (int)FixedRoles.Admin && (n.StartDate < max)
						}
					})
				select new DataRow
				{
					RowIndex = item.RowIndex,
					ClientId = item.ClientId,
					FirstName = item.FirstName,
					LastName = item.LastName,
					StartDate = item.StartDate,
					Value = item.Value,
					ExistingValue = item.Value,
					RowErrors = new DataRow.Errors
					{
						ClientNotFound = item.RowErrors.ClientNotFound,
						NegativeValue = item.RowErrors.NegativeValue,
						FutureStartDate = item.RowErrors.FutureStartDate,
						InvalidStartDate = item.RowErrors.InvalidStartDate,
						DuplicatesRowIndexes = item.RowErrors.DuplicatesRowIndexes,
						Any = item.RowErrors.ClientNotFound
							|| item.RowErrors.NegativeValue
							|| item.RowErrors.FutureStartDate
							|| item.RowErrors.InvalidStartDate
							|| item.RowErrors.DuplicatesRowIndexes.Any(),
						StartDateBeforeReportEnd = item.RowErrors.StartDateBeforeReportDate
					}
				};
			return source;
		}
		public object GetData()
		{
			using (var db = new ccEntities())
			{
				var source = PreviewQuery(db, Permissions, Id);

				var filtered = source;
				IOrderedQueryable<DataRow> ordered = null;
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
		public class DataRow
		{
			public class Errors
			{
				public bool? ClientNotFound { get; set; }

				public bool? NegativeValue { get; set; }

				public bool? FutureStartDate { get; set; }

				public bool? InvalidStartDate { get; set; }

				public IEnumerable<int> DuplicatesRowIndexes { get; set; }

				public bool? Any { get; set; }

				public bool? StartDateBeforeReportEnd { get; set; }
			}

			public int? ClientId { get; set; }

			public string FirstName { get; set; }

			public string LastName { get; set; }

			public DateTime? StartDate { get; set; }

			public decimal Value { get; set; }

			public decimal ExistingValue { get; set; }

			public Errors RowErrors { get; set; }

			public int RowIndex { get; set; }
		}
	}

}
