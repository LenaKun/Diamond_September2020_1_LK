using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CC.Data;
using System.IO;

namespace CC.Web.Helpers
{
	public static class FilesHelper
	{
		public const string LandingPagePath = "~/App_Data/LandingPageFiles";
		public const string DefaultPath = "~/App_Data/UploadedFiles";

		public static bool SaveFile(HttpPostedFileBase file, Guid id, string description, float order, bool isLandingPage, HttpServerUtilityBase Server, ref List<string> Errors)
		{
			bool result = false;
			string fileName = file.FileName;
			string path = fileAbsolutePath(id, isLandingPage ? LandingPagePath : DefaultPath, Server);
			try
			{
				file.SaveAs(path);
				result = true;
			}
			catch (System.IO.DirectoryNotFoundException)
			{
				var directory = System.IO.Path.GetDirectoryName(path);
				System.IO.Directory.CreateDirectory(directory);
				try
				{
					file.SaveAs(path);
					result = true;
				}
				catch(Exception exi)
				{
					var msg = exi.Message;
					if(exi.InnerException != null)
					{
						var inner = exi.InnerException;
						while(inner.InnerException != null)
						{
							inner = inner.InnerException;
						}
						msg = inner.Message;
					}
					Errors.Add(msg);
				}
			}
			catch(Exception ex)
			{
				var msg = ex.Message;
				if (ex.InnerException != null)
				{
					var inner = ex.InnerException;
					while (inner.InnerException != null)
					{
						inner = inner.InnerException;
					}
					msg = inner.Message;
				}
				Errors.Add(msg);
			}
			if (result)
			{
				using (var db = new ccEntities())
				{
					var newfile = new CC.Data.File
					{
						Id = id,
						Description = description,
						Order = order,
						UploadDate = DateTime.Now,
						IsLandingPage = isLandingPage,
						FileEnding = fileName.Substring(fileName.LastIndexOf('.'), fileName.Length - fileName.LastIndexOf('.'))
					};
					db.Files.AddObject(newfile);
					try
					{
						db.SaveChanges();
					}
					catch(Exception ex)
					{
						result = false;
						var msg = ex.Message;
						if (ex.InnerException != null)
						{
							var inner = ex.InnerException;
							while (inner.InnerException != null)
							{
								inner = inner.InnerException;
							}
							msg = inner.Message;
						}
						Errors.Add(msg);
						DeleteFile(id, isLandingPage, Server, ref Errors);
					}
				}
			}
			return result;
		}

		public static void DeleteFile(Guid id, bool isLandingPage, HttpServerUtilityBase Server, ref List<string> Errors)
		{
			string path = Server.MapPath(isLandingPage ? LandingPagePath : DefaultPath);
			using (var db = new ccEntities())
			{
				var file = db.Files.SingleOrDefault(f => f.Id == id);
				try
				{
					var di = new DirectoryInfo(path);
					var name = id.ToString();
					var delfile = di.GetFiles().SingleOrDefault(f => f.Name == name);
					if (delfile != null)
					{
						delfile.Delete();
					}
					if (file != null)
					{
						db.Files.DeleteObject(file);
						db.SaveChanges();
					}
				}
				catch (Exception ex)
				{
					var msg = ex.Message;
					if (ex.InnerException != null)
					{
						var inner = ex.InnerException;
						while (inner.InnerException != null)
						{
							inner = inner.InnerException;
						}
						msg = inner.Message;
					}
					Errors.Add(msg);
				}
			}
		}

		public static string fileAbsolutePath(Guid id, string FilesDirectory, HttpServerUtilityBase Server)
		{
			var p1 = VirtualPathUtility.AppendTrailingSlash(FilesDirectory);
			var p2 = VirtualPathUtility.Combine(p1, id.ToString());
			var p3 = Server.MapPath(p2);
			return p3;
		}
	}
}