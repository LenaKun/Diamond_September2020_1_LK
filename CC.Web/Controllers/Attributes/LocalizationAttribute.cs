using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using System.Web.Mvc;

namespace CC.Web.Attributes
{
	public abstract class LocalizationAttributeBase : System.Web.Mvc.ActionFilterAttribute
	{
		public static void SetCulture(string cultureName)
		{
			if (!string.IsNullOrEmpty(cultureName))
			{
				var culture = System.Globalization.CultureInfo.GetCultureInfo(cultureName);
				if (culture != null)
				{
					Thread.CurrentThread.CurrentUICulture = culture;
				}
			}
		}
		protected abstract string GetCultureName(ActionExecutingContext filterContext);
		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			var cultureName = GetCultureName(filterContext);
			SetCulture(cultureName);
			base.OnActionExecuting(filterContext);
		}

	}
	public class LocalizationAttribute : LocalizationAttributeBase
	{
		protected override string GetCultureName(ActionExecutingContext filterContext)
		{
			var username = filterContext.HttpContext.User.Identity.Name;
			using (var db = new CC.Data.ccEntities())
			{
				var q = (from u in db.Users
						 where u.UserName == username
						 select new
						 {
							 u.RoleId,
							 AgencyCulture = u.Agency.AgencyGroup.Culture ??
							  u.Agency.AgencyGroup.Country.Culture,
							 SerCulture = u.AgencyGroup.Culture ??
							 u.AgencyGroup.Country.Culture
						 }).FirstOrDefault();
				if (q != null)
				{
					switch ((CC.Data.FixedRoles)q.RoleId)
					{
						case CC.Data.FixedRoles.AgencyUser:
						case CC.Data.FixedRoles.DafEvaluator:
						case CC.Data.FixedRoles.DafReviewer:
						case CC.Data.FixedRoles.AgencyUserAndReviewer:
							return q.AgencyCulture;
						case CC.Data.FixedRoles.Ser:
						case CC.Data.FixedRoles.SerAndReviewer:
							return q.SerCulture;
					}
				}
				return null;
			}
		}
	}

	public class LocalizationByClientIdAttribute : LocalizationAttributeBase
	{
		protected string _parameterName;
		public LocalizationByClientIdAttribute(string parameterName)
			: base()
		{
			_parameterName = parameterName ?? "id";
		}
		protected override string GetCultureName(System.Web.Mvc.ActionExecutingContext filterContext)
		{
			var dd = filterContext.ActionParameters[_parameterName];
			int id;
			if (int.TryParse(dd.ToString(), out id))
			{
				using (var db = new CC.Data.ccEntities())
				{
					var cultureName = db.Clients.Where(f => f.Id == id).Select(f => f.Agency.AgencyGroup.Culture ?? f.Agency.AgencyGroup.Country.Culture).FirstOrDefault();
					return cultureName;
				}
			}
			else
			{
				return null;
			}
		}
	}
	public class LocalizationByDafIdAttribute : LocalizationByClientIdAttribute
	{
		public LocalizationByDafIdAttribute(string parameterName) : base(parameterName) {

		}
		protected override string GetCultureName(System.Web.Mvc.ActionExecutingContext filterContext)
		{
			var dd = filterContext.ActionParameters[_parameterName];
			int id;
			if (int.TryParse(dd.ToString(), out id))
			{
				using (var db = new CC.Data.ccEntities())
				{
					var cultureName = db.Dafs.Where(f => f.Id == id).Select(f => f.Client.Agency.AgencyGroup.Culture ?? f.Client.Agency.AgencyGroup.Country.Culture).FirstOrDefault();
					return cultureName;
				}
			}
			else
			{
				return null;
			}
		}
	}
}