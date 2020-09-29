using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace System.Data.SqlClient
{
	public static class ConnectionStringHelper
	{
		public static string GetProviderConnectionString()
		{
			var entityConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ccEntities"].ConnectionString;
			var builder = new System.Data.EntityClient.EntityConnectionStringBuilder(entityConnectionString);
			string providerConnectionString = builder.ProviderConnectionString;
			return providerConnectionString;
		}
	}
}