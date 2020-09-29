using CC.Web.Controllers.Attributes;
using CC.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CC.Web.Helpers;

namespace CC.Web.Controllers
{
	[PasswordExpirationCheckAttribute()]
	public class LandingPageController : PrivateCcControllerBase
    {
        public ActionResult Index()
        {
			var model = new LandingPageModel();
			model.TopMessageContent = GlobalDbSettings.GetString(GlobalDbSettings.GlobalStringNames.LandingPageMessage);
			model.FilesList = db.Files.Where(f => f.IsLandingPage).OrderBy(f => f.Order).ThenBy(f => f.Description).ToList();
            return View(model);
        }

		[HttpGet]
		public FileResult DownloadFile(Guid id)
		{
			return GetFileById(id);
		}

		public FileResult GetFileById(Guid id)
		{
			var file = db.Files.SingleOrDefault(f => f.Id == id);
			if (file != null)
			{
				var path = FilesHelper.fileAbsolutePath(id, FilesHelper.LandingPagePath, Server);
				if (System.IO.File.Exists(path))
				{
					return this.File(path, "application/octet-stream", file.Description + file.FileEnding);
				}
			}

			throw new HttpException(404, "File not found");
		}
    }
}
