using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CC.Data;
using System.Globalization;
using CC.Web.Helpers;

namespace CC.Web.Models.Import.ClientReports
{
	class ImportHelperBase<TEntity, TPreview>
		where TEntity : CC.Data.CriBase, new()
		where TPreview : class,ICriPreview
	{
		public ImportHelperBase(CC.Data.Services.IPermissionsBase permissions)
		{
			this.permissions = permissions;
		}

		protected log4net.ILog _log = log4net.LogManager.GetLogger(typeof(ImportHelperBase<TEntity, TPreview>));

		protected Func<ccEntities> getDbContext = () => new ccEntities();
		protected IEnumerable<Client> GetClients(ccEntities db) { return db.Clients.Where(this.permissions.ClientsFilter); }
		protected IEnumerable<SubReport> GetSubReports(ccEntities db) { return db.SubReports.Where(this.permissions.SubReportsFilter); }

		protected List<jQueryDataTableColumn> columns;

		public IEnumerable<jQueryDataTableColumn> PreviewDataColumns()
		{
			return columns;
		}

		public IEnumerable<object> Errors(Guid importId)
		{
			yield break;
		}

		protected CC.Data.Services.IPermissionsBase permissions;


		public bool IsValid(Guid importId)
		{
			using (var _db = getDbContext())
			{
				var source = PreviewQuery(importId, _db);
				var rowsWithErrors = source.Where(f => f.Errors != null && f.Errors != "");
				return !rowsWithErrors.Any();
			}
		}

		protected virtual void SetMapping(CsvHelper.Configuration.CsvConfiguration conf)
		{

		}

		public IEnumerable<string> CsvColumnNames()
		{
			var conf = new CsvHelper.Configuration.CsvConfiguration();
			this.SetMapping(conf);
			return conf.Properties.Select(f => f.NameValue);
		}

		public void Upload(string filename, Guid importId, int subReportId)
		{
			_log.Debug("Entered" + System.Reflection.MethodBase.GetCurrentMethod().Name);
			var t = DateTime.Now;

			var csvConf = new CsvHelper.Configuration.CsvConfiguration()
			{
				IsStrictMode = false,
				IsCaseSensitive = false,
				HasHeaderRecord = true,
				SkipEmptyRecords = true,
			};

			this.SetMapping(csvConf);

			var textReader = new System.IO.StreamReader(filename);


			DateTime t2 = DateTime.Now;

			using (var csvReader = new CsvHelper.CsvReader(textReader, csvConf))
			{

				foreach (var chunk in this.TryConvert(csvReader))
				{

					using (var db = getDbContext())
					{
						var objectSet = db.CriBases.OfType<TEntity>();

						var details = (from sr in db.SubReports
									   where sr.Id == subReportId
									   select new
									   {
										   mrStart = sr.MainReport.Start,
										   mrEnd = sr.MainReport.End,
										   reportingMethodId = sr.AppBudgetService.Service.ReportingMethodId,
										   agencyId = sr.AppBudgetService.AgencyId,
										   appId = sr.AppBudgetService.AppBudget.AppId,
										   agencyGroupId = sr.AppBudgetService.Agency.GroupId
									   }).SingleOrDefault();
												
						DayOfWeek selectedDOW = details.mrStart.DayOfWeek;
						int? selectedDowDb = GlobalHelper.GetWeekStartDay(details.agencyGroupId, details.appId);
						if (selectedDowDb.HasValue)
						{
							selectedDOW = (DayOfWeek)selectedDowDb.Value;
						}

						foreach (var record in chunk)
						{
							record.ImportId = importId;
							record.SubReportId = subReportId;
							var type = record.GetType();
							if(details.reportingMethodId == (int)Service.ReportingMethods.HomecareWeekly && type.Name == "CriHc")
							{
								var reportDate = (record as CC.Data.CriHc).Date;
								if(reportDate.HasValue && reportDate.Value >= details.mrStart && reportDate < details.mrEnd)
								{
									reportDate = reportDate.Value.WeekStart(selectedDOW);
									if (reportDate < details.mrStart)
									{
										reportDate = details.mrStart;
									}
									(record as CC.Data.CriHc).Date = reportDate;
								}
							}
							db.CriBases.AddObject(record);
						}
						_log.Debug(System.Reflection.MethodBase.GetCurrentMethod().Name + " chunk read in " + (DateTime.Now - t2));
						t2 = DateTime.Now;

						db.SaveChanges();

						if(details.reportingMethodId == (int)Service.ReportingMethods.HomecareWeekly)
						{
							var result = db.UpdateImportClientReportWeekly(importId);
						}

						_log.Debug(System.Reflection.MethodBase.GetCurrentMethod().Name + " chunk written in " + (DateTime.Now - t2));
						t2 = DateTime.Now;

					}
					System.Threading.Thread.Sleep(100);
				}
			}

			System.IO.File.Delete(filename);

			_log.Debug("exiting " + System.Reflection.MethodBase.GetCurrentMethod().Name + " in " + (DateTime.Now - t));
		}
		private IEnumerable<IEnumerable<TEntity>> TryConvert(CsvHelper.ICsvReader reader)
		{
			TEntity record = null;
			var chunkSize = 1000;
			int rowIndex = 2;
			var chunk = new List<TEntity>(chunkSize);
			while (reader.Read())
			{
				try
				{
					record = reader.GetRecord<TEntity>();
				}
				catch (CsvHelper.CsvReaderException ex)
				{
					record = new TEntity
					{
						Errors = "Could not read value \"" + ex.FieldValue + "\" of column \"" + ex.FieldName + "\". "
					};
				}

				record.RowIndex = rowIndex++;

				chunk.Add(record);
				if (rowIndex % chunkSize == 2)
				{
					yield return chunk;
					chunk.Clear();
				}
			}
			if (chunk.Any())
			{
				yield return chunk;
			}

		}

		public virtual void Import(Guid importId)
		{
			throw new NotImplementedException();
		}

		public jQueryDataTableResult<object> GetPreviewData(Guid id, jQueryDataTableParamModel jq)
		{
			var result = new jQueryDataTableResult<object>();

			using (var _db = getDbContext())
			{
				var source = PreviewQuery(id, _db);
				var filtered = source;
				foreach (var s in (jq.sSearch ?? "").Split(new char[] { ' ' }).Where(f => !string.IsNullOrEmpty(f)))
				{
					filtered = filtered.Where(this.GetSearchFilter(s));
				}

				var sorted = filtered.OrderByField(this.columns.ToArray()[jq.iSortCol_0].sName, jq.sSortDir_0 == "asc");


				var aaData = sorted.Skip(jq.iDisplayStart).Take(jq.iDisplayLength)
					.ToList().Select(f => this.ToArray(f));

				result.iTotalDisplayRecords = filtered.Count();
				result.iTotalRecords = source.Count();
				result.aaData = aaData;
				return result;
			}
		}

		protected virtual IQueryable<TPreview> PreviewQuery(Guid id, ccEntities _db)
		{
			throw new NotImplementedException();
		}

		protected virtual System.Linq.Expressions.Expression<Func<TPreview, bool>> GetSearchFilter(string s)
		{
			throw new NotImplementedException();
		}

		protected object[] ToArray(object obj)
		{
			var props = typeof(TPreview).GetProperties();
			var q = from item in this.columns
					join prop in props on item.sName equals prop.Name
					select prop.PropertyType == typeof(List<string>) ?
						string.Join(",", (prop.GetValue(obj, null) as List<string>).Where(f => !string.IsNullOrEmpty(f))) :
						prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(Nullable<DateTime>) ?
						ToString(prop.GetValue(obj, null) as Nullable<DateTime>, item.format) :
						prop.GetValue(obj, null);
			return q.ToArray();
		}

		private string ToString(DateTime? val, string format)
		{
			if (val == null) return null;
			else if (string.IsNullOrEmpty(format)) { return val.Value.ToString(); }
			else { return val.Value.ToString(format); }
		}

	}

	interface ICriPreview
	{
		string Errors { get; set; }
	}
}
