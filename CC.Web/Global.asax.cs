using System;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.DynamicData;
using System.Web.Mvc;
using System.Web.Routing;
using log4net;
using CC.Web.Security;
using System.Configuration;

namespace CC.Web
{
	// Note: For instructions on enabling IIS6 or IIS7 classic mode, 
	// visit http://go.microsoft.com/?LinkId=9394801

	public class MvcApplication : System.Web.HttpApplication
	{
		private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


		protected void Application_Start(object sender, EventArgs e)
		{
			log4net.Config.XmlConfigurator.Configure();

			_log.Debug("Application Started");

			AreaRegistration.RegisterAllAreas();

			RegisterGlobalFilters(GlobalFilters.Filters);
			RegisterRoutes(RouteTable.Routes);
			
			ModelBinders.Binders.Add(typeof(int[]), new CC.Web.Helpers.StringSplitModelBinder());

			DataAnnotationsModelValidatorProvider.RegisterAdapter(
				typeof(System.ComponentModel.DataAnnotations.DateFormatAttribute),
				typeof(System.Web.Mvc.RegularExpressionAttributeAdapter));
			DataAnnotationsModelValidatorProvider.RegisterAdapter(
				typeof(System.ComponentModel.DataAnnotations.ClientOnlyRegexAttribute),
				typeof(System.Web.Mvc.RegularExpressionAttributeAdapter));

#if !DEBUG
			try
			{
				Jobs.Schedule.StartSchedule();
			}
			catch (Exception ex)
			{
				_log.Fatal("Failed to start Quartz.net scheduler", ex);
			}
#endif

		}

		#region Mvc registrations

		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
		}

		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

			routes.MapRoute(
				"Default", // Route name
				"{controller}/{action}/{id}", // URL with parameters
				new { controller = "Default", action = "Index", id = UrlParameter.Optional } // Parameter defaults
				,new[]{"CC.Web.Controllers"}
			);

		}
		#endregion



		protected void Application_AuthenticateRequest()
		{
			if (Request.IsAuthenticated)
			{
				//get the username which we previously set in
				//forms authentication ticket in our login1_authenticate event
				string loggedUser = HttpContext.Current.User.Identity.Name;

				//build a custom identity and custom principal object based on this username
				var principal = new CcPrincipal(HttpContext.Current.User);

				HttpContext.Current.Items[System.CCDecimals.DecimalDigitsDisplayItemName] = principal.CcUser.DecimalDisplayDigits;

				//set the principal to the current context
				HttpContext.Current.User = principal;
			}
		}

		protected void Application_BeginRequest()
		{
			CultureInfo currentCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
			currentCulture.DateTimeFormat.ShortDatePattern = "dd MMM yyyy";
			currentCulture.DateTimeFormat.LongTimePattern = "HH:mm:ss";
			currentCulture.NumberFormat.NumberGroupSeparator = ",";
			Thread.CurrentThread.CurrentCulture = currentCulture;
		}

		protected void Application_Error(object sender, EventArgs e)
		{



			Exception ex = HttpContext.Current.Server.GetLastError();

			var errorDetails = CC.Extensions.Errors.CreateMessage(HttpContext.Current);

			_log.Fatal(errorDetails, ex);

		}

        protected void Application_End()
        {
            Jobs.Schedule.Stop();
        }

	}


}