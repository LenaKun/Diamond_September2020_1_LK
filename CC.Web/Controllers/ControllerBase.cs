using System;
using System.Collections.Generic;

using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Principal;
using CC.Data;
using CC.Data.Repositories;
using CC.Web.Controllers.Attributes;
using CC.Web.Attributes;

namespace CC.Web.Controllers
{	
	[PasswordExpirationCheckAttribute()]
	[CC.Web.Attributes.LogActionFilter()]
	[NoCache]
	[CC.Web.Attributes.LogError()]
	[HandleApiError(System.Net.HttpStatusCode.BadRequest, ExceptionType = typeof(InvalidPasswordException), View = "InvalidPassword")]
	public class PrivateCcControllerBase : CcControllerBase
	{


	}

	public class CcControllerBase : Controller
	{
		private readonly log4net.ILog _logger = log4net.LogManager.GetLogger(typeof(ClientsController));
		protected ccEntities db = new ccEntities();

		public CcRepository _repo = null;
		public CcRepository Repo
		{
			get
			{
				if (_repo == null) _repo = new CcRepository(this.CcUser);
				return _repo;
			}
		}

		public IPrincipal principal { get; set; }

		private User _ccUser;

		public User CcUser
		{
			get
			{
				if (_ccUser == null)
				{

					if (User != null && User.Identity != null && User.Identity.IsAuthenticated)
					{

						using (var context = new ccEntities())
						{
							_ccUser = context.Users
								.Include(f => f.Agency.AgencyGroup)
								.Include(f => f.AgencyGroup)
								.SingleOrDefault(f => f.MembershipUser.LoweredUserName == User.Identity.Name.ToLower());
							if (_ccUser == null)
							{
								System.Web.Security.FormsAuthentication.SignOut();
								System.Web.Security.FormsAuthentication.RedirectToLoginPage();
								Response.End();
							}

						}
					}
				}
				return _ccUser;
			}
			set { _ccUser = value; }
		}

		private CC.Data.Services.IPermissionsBase _permissions;

		public CC.Data.Services.IPermissionsBase Permissions
		{
			get
			{
				if (_permissions == null)
					_permissions = CC.Data.Services.PermissionsFactory.GetPermissionsFor(this.CcUser);
				return _permissions;
			}
			set { _permissions = value; }
		}

		public CcControllerBase(User user)
		{
			Repo.CurrentUser = user;

			this.CcUser = user;

			System.Security.Principal.GenericIdentity identity = new GenericIdentity(this.CcUser.UserName);
		}
		public CcControllerBase()
			: base()
		{
			
		}

		public static User GetCurrentUser(IPrincipal principal, IRepository<User> usersRepository)
		{
			if (System.Web.HttpContext.Current != null)
			{
				if (principal.Identity.IsAuthenticated)
				{
					string userName = principal.Identity.Name.ToLower();

					var user = usersRepository.Select.Single(f => f.MembershipUser.LoweredUserName == userName);

					return user;
				}
			}

			return null;
		}
		protected override void OnAuthorization(AuthorizationContext filterContext)
		{
			base.OnAuthorization(filterContext);
			if (Request.IsAuthenticated)
			{
				//get the username which we previously set in
				//forms authentication ticket in our login1_authenticate event
				string loggedUser = User.Identity.Name;

				//build a custom identity and custom principal object based on this username
				var principal = new CC.Web.Security.CcPrincipal(User);

				//set the principal to the current context

			}
		}

		public string RenderRazorViewToString(string viewName, object model)
		{
			ViewData.Model = model;
			using (var sw = new System.IO.StringWriter())
			{
				var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
				var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
				viewResult.View.Render(viewContext, sw);
				viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
				return sw.GetStringBuilder().ToString();
			}
		}

		protected JsonResult MyJsonResult(object data, JsonRequestBehavior jsonRequestBehavior)
		{
			return new jsdtr()
			{
				Data = data,
				JsonRequestBehavior = jsonRequestBehavior

			};
		}
		protected JsonResult MyJsonResult(object data)
		{

			return MyJsonResult(data, JsonRequestBehavior.AllowGet);
		}
		protected JsonResult MyJsonResult(object data, int statusCode)
		{
			Response.StatusCode = statusCode;
			Response.TrySkipIisCustomErrors = true;
			return MyJsonResult(data, JsonRequestBehavior.AllowGet);
		}
		protected string GetDeviceId()
		{
			return this.HttpContext.Request.Headers["x-cc-deviceid"];
		}
		protected override void Dispose(bool disposing)
		{
			if (db != null)
			{
				db.Dispose();
				db = null;
			}

			base.Dispose(disposing);
		}

	}



	public class NoCacheAttribute : ActionFilterAttribute
	{
		public override void OnResultExecuting(ResultExecutingContext filterContext)
		{
			filterContext.HttpContext.Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
			base.OnResultExecuting(filterContext);
		}
	}


	class jsdtr : JsonResult
	{
		public override void ExecuteResult(ControllerContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException("context");
			}
			if (JsonRequestBehavior == JsonRequestBehavior.DenyGet &&
				String.Equals(context.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidOperationException("Get is not allowed");
			}

			HttpResponseBase response = context.HttpContext.Response;

			if (!String.IsNullOrEmpty(ContentType))
			{
				response.ContentType = ContentType;
			}
			else
			{
				response.ContentType = "application/json";
			}
			if (ContentEncoding != null)
			{
				response.ContentEncoding = ContentEncoding;
			}
			if (Data != null)
			{
				var str = Newtonsoft.Json.JsonConvert.SerializeObject(Data, new Newtonsoft.Json.Converters.IsoDateTimeConverter());

				response.Output.Write(str);
			}
		}
	}

}