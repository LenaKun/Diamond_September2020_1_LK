using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CC.Data;

namespace CC.Web.Areas.Admin.Controllers
{
	[CcAuthorize(CC.Data.FixedRoles.Admin)]
	public class ResourcesController : AdminControllerBase
	{
		public ViewResult Index()
		{
			return View();
		}
		public JsonResult IndexData(CC.Web.Models.jQueryDataTableParamModel p)
		{
			var q = db.Resources.AsQueryable();
			var filtered = q;
			if (!string.IsNullOrEmpty(p.sSearch))
			{
				filtered = filtered.Where(f => f.Culture.Contains(p.sSearch)
					|| f.Key.Contains(p.sSearch)
					|| f.Value.Contains(p.sSearch));
			}
			var ordered = filtered.OrderByField(p.sSortCol_0, p.bSortDir_0);
			if (!string.IsNullOrEmpty(p.sSortCol_1))
			{
				ordered = ordered.ThenByField(p.sSortCol_1, p.bSortDir_1);
			}

			var result = new CC.Web.Models.jQueryDataTableResult<Resource>
			{
				aaData = ordered.Skip(p.iDisplayStart).Take(p.iDisplayLength).ToList(),
				iTotalDisplayRecords = filtered.Count(),
				iTotalRecords = q.Count(),
				sEcho = p.sEcho
			};
			return this.MyJsonResult(result);
		}

		[HttpPost]
		public ActionResult SaveResource(string culture, string key, string value)
		{
			var resourceProvider = new CC.Data.Resources.DbResourceProvider();

			if (string.IsNullOrWhiteSpace(value))
			{
				Response.StatusCode = 400;
				return this.MyJsonResult("Value is empty");


			}
			resourceProvider.WriteResource(key, culture, value.Trim());
			return MyJsonResult(null);
		}
		public ActionResult Export()
		{
			var q = db.Resources;
			return this.Csv(q);
		}

		[HttpPost]
		public ActionResult Import(HttpPostedFileBase file)
		{
			var cnnstr = System.Data.SqlClient.ConnectionStringHelper.GetProviderConnectionString();
			var textReader = new System.IO.StreamReader(file.InputStream);
			using (var csvreqder = new CsvHelper.CsvReader(textReader))
			{
				csvreqder.Configuration.SkipEmptyRecords = true;
				var sqlcnn = (System.Data.SqlClient.SqlConnection)((System.Data.EntityClient.EntityConnection)db.Connection).StoreConnection;
				sqlcnn.Open();
				ExecuteSqlCommandText(sqlcnn, "create table #resources ([Culture] varchar(5), [Key] varchar(100), [Value] nvarchar(4000));");
				using (var bcp = new System.Data.SqlClient.SqlBulkCopy(sqlcnn))
				{
					bcp.DestinationTableName = "tempdb..#resources";
					bcp.ColumnMappings.Add("Culture", "Culture");
					bcp.ColumnMappings.Add("Key", "Key");
					bcp.ColumnMappings.Add("Value", "Value");
					var records = csvreqder.GetRecords<Resource>();
					var irdf = CC.Web.Helpers.idrf.GetReader(records);
					bcp.WriteToServer(irdf);
				}
				var cmd = "merge dbo.resources as t using (select distinct * from #resources) as s on t.[Culture] = s.[Culture] and t.[Key] = s.[Key]";
				cmd += "when not matched by target then insert ([Culture], [Key], [Value]) values (lower(ltrim(rtrim(s.[Culture]))), ltrim(rtrim(s.[Key])), ltrim(rtrim(s.[Value])))";
				cmd += "when matched and t.[Value] <> s.[Value] then update set [Value] = s.[Value];";
				ExecuteSqlCommandText(sqlcnn, cmd);
				sqlcnn.Close();
				Resources.Resource.InvalidateCache();
			}
			return RedirectToAction("Index");
		}

		private void ExecuteSqlCommandText(System.Data.SqlClient.SqlConnection sqlcnn, string commandText)
		{

			using (var cmd = new System.Data.SqlClient.SqlCommand(commandText, sqlcnn))
			{
				cmd.ExecuteNonQuery();
			}
		}

	}
}