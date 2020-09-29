using System;
using System.Collections.Generic;
using System.Data.Services;
using System.Data.Services.Common;
using System.Linq;
using System.Linq.Expressions;
using System.ServiceModel.Web;
using System.Data.Entity;
using CC.Data;
using System.Web;

namespace CC.Web.Data
{
	[System.ServiceModel.ServiceBehavior(IncludeExceptionDetailInFaults = true)]
	public class DataService : DataService<CC.Data.ccEntities>
	{

		public CC.Data.Services.IPermissionsBase permissions;
		public DataService()
		{

		}

		// This method is called only once to initialize service-wide policies.
		public static void InitializeService(DataServiceConfiguration config)
		{

			config.UseVerboseErrors = true;
			// Examples:
			config.SetEntitySetAccessRule("Clients", EntitySetRights.AllRead);
			config.SetEntitySetAccessRule("Agencies", EntitySetRights.AllRead);
			config.SetEntitySetAccessRule("AgencyGroups", EntitySetRights.AllRead);
			config.SetEntitySetAccessRule("Apps", EntitySetRights.AllRead);
			config.SetEntitySetAccessRule("Funds", EntitySetRights.AllRead);
			config.SetEntitySetAccessRule("MasterFunds", EntitySetRights.AllRead);
			config.SetEntitySetAccessRule("States", EntitySetRights.AllRead);
			config.SetEntitySetAccessRule("Countries", EntitySetRights.AllRead);
			config.SetEntitySetAccessRule("Regions", EntitySetRights.AllRead);
			config.SetEntitySetAccessRule("Services", EntitySetRights.AllRead);
			config.SetEntitySetAccessRule("ServiceTypes", EntitySetRights.AllRead);
			config.SetEntitySetAccessRule("EmergencyReportTypes", EntitySetRights.AllRead);
			config.SetEntitySetAccessRule("Currencies", EntitySetRights.AllRead);
			config.SetEntitySetAccessRule("BirthCountries", EntitySetRights.AllRead);
			config.SetEntitySetAccessRule("CfsEndDateReasons", EntitySetRights.AllRead);
			config.SetEntitySetAccessRule("AgencyOverRideReasons", EntitySetRights.AllRead);
			config.DataServiceBehavior.MaxProtocolVersion = DataServiceProtocolVersion.V2;
		}

		protected override CC.Data.ccEntities CreateDataSource()
		{
			var cource = new CC.Data.ccEntities();
			cource.ContextOptions.ProxyCreationEnabled = false;
			return cource;
		}


		protected override void OnStartProcessingRequest(ProcessRequestArgs args)
		{
			string username = HttpContext.Current.User.Identity.Name;
			permissions = CC.Data.Services.PermissionsFactory.GetPermissionsFor(username);

			base.OnStartProcessingRequest(args);
		}

		protected override void HandleException(HandleExceptionArgs args)
		{
			base.HandleException(args);
		}

		[QueryInterceptor("Clients")]
		public Expression<Func<Client, bool>> OnQueryClients()
		{
			return permissions.ClientsFilter;
		}
		[QueryInterceptor("Agencies")]
		public Expression<Func<Agency, bool>> OnQueryAgencies()
		{
			return permissions.AgencyFilter;
		}
		[QueryInterceptor("AgencyGroups")]
		public Expression<Func<AgencyGroup, bool>> OnQueryAgencyGroups()
		{
			return permissions.AgencyGroupsFilter;
		}
		[QueryInterceptor("Apps")]
		public Expression<Func<App, bool>> OnQueryApps()
		{
			return permissions.AppsFilter;
		}
		[QueryInterceptor("Funds")]
		public Expression<Func<Fund, bool>> OnQueryFunds()
		{
			return permissions.FundsFilter;
		}
        [QueryInterceptor("MasterFunds")]
        public Expression<Func<MasterFund, bool>> OnQueryMasterFunds()
        {
            return permissions.MasterFundsFilter;
        }
        [QueryInterceptor("Regions")]
        public Expression<Func<Region, bool>> OnQueryRegions()
        {
            return permissions.RegionsFilter;
        }

		[QueryInterceptor("Currencies")]
		public Expression<Func<Currency, bool>> OnQueryCurrencies()
		{
			return c => CC.Data.Currency.ConvertableCurrencies.Contains(c.Id);
		}

		[QueryInterceptor("CfsEndDateReasons")]
		public Expression<Func<CfsEndDateReason, bool>> OnQueryCfsEndDateReasons()
		{
			return c => true;
		}
	}
}
