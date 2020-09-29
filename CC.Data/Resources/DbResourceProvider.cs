using System;
using Resources.Abstract;
using Resources.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace CC.Data.Resources
{
	public class DbResourceProvider : BaseResourceProvider
	{
		public DbResourceProvider()
			: base()
		{

		}
		private string GetConnectionString()
		{
			return System.Data.SqlClient.ConnectionStringHelper.GetProviderConnectionString();
		}
		protected override IList<ResourceEntry> ReadResources()
		{
			using (var db = new ccEntities())
			{
				var resources = db.Resources.Select(item => new ResourceEntry
					{
						Name = item.Key,
						Value = item.Value,
						Culture = item.Culture
					}).ToList();
				foreach (var item in resources)
				{
					item.Culture = item.Culture.ToLowerInvariant();
				}
				return resources;
			}
		}
		protected override ResourceEntry ReadResource(string name, string culture)
		{
			using (var db = new ccEntities())
			{
				var resources = db.Resources
					.Where(f => f.Culture == culture && f.Key == name)
					.Select(item => new ResourceEntry
					{
						Name = item.Key,
						Value = item.Value,
						Culture = item.Culture
					}).FirstOrDefault();
				if (resources == null)
				{
					throw new Exception(string.Format("Resource {0} for culture {1} was not found", name, culture));
				}
				else
				{
					return resources;
				}
			}
		}

		public override void WriteResource(string name, string culture, string value)
		{
			using (var db = new ccEntities())
			{
				var resources = db.Resources
					.Where(f => f.Culture == culture && f.Key == name)
					.FirstOrDefault();
				if (resources == null)
				{
					throw new Exception(string.Format("Resource {0} for culture {1} was not found", name, culture));
				}
				else
				{
					resources.Value = value;
					db.SaveChanges();
					base.UpdateCachedResource(name, culture, value);
				}
			}
		}
	}
}
