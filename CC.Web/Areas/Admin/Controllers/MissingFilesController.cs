using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CC.Web.Areas.Admin.Controllers
{
	[CcAuthorize(CC.Data.FixedRoles.Admin)]
	public class MissingFilesController : AdminControllerBase
    {
        //
        // GET: /Admin/MissingFiles/

        public ActionResult Index()
        {
            return View();
        }

		public JsonResult IndexData(CC.Web.Models.jQueryDataTableParamModel p)
		{
			var poFiles = (from m in db.MainReports
						   where m.ProgramOverviewFileName != null
						   select new MissingFilesRow
						   {
							   MainReportId = m.Id,
							   FileName = m.ProgramOverviewFileName,
							   FileType = "Program Overview"
						   });
			var mhsaFiles = (from m in db.MainReports
							 where m.MhsaFileName != null
							 select new MissingFilesRow
							 {
								 MainReportId = m.Id,
								 FileName = m.MhsaFileName,
								 FileType = "Mhsa"
							 });
			var dbq = poFiles.Union(mhsaFiles);
			var q = (from item in dbq.ToList()
					where !IsFileExists(item.MainReportId, item.FileType)
					select new MissingFilesRow
					{
						MainReportId = item.MainReportId,
						FileName = item.FileName,
						FileType = item.FileType
					}).AsQueryable();
			var filtered = q;
			if (!string.IsNullOrEmpty(p.sSearch))
			{
				filtered = filtered.Where(f => f.FileName.Contains(p.sSearch)
					|| f.FileType.Contains(p.sSearch));
			}
			var ordered = filtered.OrderByField(p.sSortCol_0, p.bSortDir_0);
			if (!string.IsNullOrEmpty(p.sSortCol_1))
			{
				ordered = ordered.ThenByField(p.sSortCol_1, p.bSortDir_1);
			}

			var result = new CC.Web.Models.jQueryDataTableResult<MissingFilesRow>
			{
				aaData = ordered.Skip(p.iDisplayStart).Take(p.iDisplayLength).ToList(),
				iTotalDisplayRecords = filtered.Count(),
				iTotalRecords = q.Count(),
				sEcho = p.sEcho
			};
			return this.MyJsonResult(result);
		}

		public bool IsFileExists(int mrId, string type)
		{
			bool result = false;
			try
			{
				if (type.Contains("Program Overview"))
				{
					GetFileByPath(programOverviewFileAbsolutePath(mrId), false, mrId);
					result = true;
				}
				else if (type.Contains("Mhsa"))
				{
					GetFileByPath(mhsaFileAbsolutePath(mrId), true, mrId);
					result = true;
				}
			}
			catch(Exception ex)
			{

			}
			return result;
		}

		private const string ProgramOverviewFilesDirectory = "~/App_Data/ProgramOverview";
		private const string MhsaFilesDirectory = "~/App_Data/Mhsa";
		private string programOverviewFileAbsolutePath(int id)
		{
			return fileAbsolutePath(id, ProgramOverviewFilesDirectory);
		}
		private string mhsaFileAbsolutePath(int id)
		{
			return fileAbsolutePath(id, MhsaFilesDirectory);
		}
		private string fileAbsolutePath(int id, string FilesDirectory)
		{
			var p1 = VirtualPathUtility.AppendTrailingSlash(FilesDirectory);
			var p2 = VirtualPathUtility.Combine(p1, id.ToString());
			var p3 = Server.MapPath(p2);
			return p3;
		}

		public FileResult GetFileByPath(string path, bool mhsa, int id)
		{
			var mainReport = db.MainReports.SingleOrDefault(f => f.Id == id);
			if (mainReport != null)
			{
				if (System.IO.File.Exists(path))
				{
					return this.File(path, "application/octet-stream", mhsa ? mainReport.MhsaFileName : mainReport.ProgramOverviewFileName);
				}
			}

			throw new HttpException(404, "File not found");
		}

		public class MissingFilesRow
		{
			public int MainReportId { get; set; }
			public string FileName { get; set; }
			public string FileType { get; set; }
		}
    }
}
