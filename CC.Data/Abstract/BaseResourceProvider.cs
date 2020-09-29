using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Resources.Entities;

namespace Resources.Abstract
{
    public abstract class BaseResourceProvider: IResourceProvider
    {
        // Cache list of resources
        private static Dictionary<string, ResourceEntry> resources = null;
        private static object lockResources = new object();

        public BaseResourceProvider() {
            Cache = true; // By default, enable caching for performance
        }

        protected bool Cache { get; set; }

		protected string CachedResourceKey(string name, string culture)
		{
			return string.Format("{0}.{1}", culture, name);
		}
        /// <summary>
        /// Returns a single resource for a specific culture
        /// </summary>
        /// <param name="name">Resorce name (ie key)</param>
        /// <param name="culture">Culture code</param>
        /// <returns>Resource</returns>
        public object GetResource(string name, string culture)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Resource name cannot be null or empty.");

            if (string.IsNullOrWhiteSpace(culture))
                throw new ArgumentException("Culture name cannot be null or empty.");

            // normalize
            culture = culture.ToLowerInvariant();

            if (Cache && resources == null) {
                // Fetch all resources
                
                lock (lockResources) {

                    if (resources == null) {
                        resources = ReadResources().ToDictionary(r => CachedResourceKey(r.Name, r.Culture));
                    }
                }
            }

            if (Cache) {
				try
				{
					return resources[CachedResourceKey(name, culture)].Value;
				}
				catch (KeyNotFoundException ex)
				{
					var msg=string.Format("Resource {0} for culture {1} was not found", name, culture);
					throw new Exception(msg, ex);
				}
            }

            return ReadResource(name, culture).Value;

        }

		public void InvalidateCache()
		{
			lock (lockResources)
			{
				resources.Clear();
			}
		}

        /// <summary>
        /// Returns all resources for all cultures. (Needed for caching)
        /// </summary>
        /// <returns>A list of resources</returns>
        protected abstract IList<ResourceEntry> ReadResources();


        /// <summary>
        /// Returns a single resource for a specific culture
        /// </summary>
        /// <param name="name">Resorce name (ie key)</param>
        /// <param name="culture">Culture code</param>
        /// <returns>Resource</returns>
        protected abstract ResourceEntry ReadResource(string name, string culture);

		/// <summary>
		/// Updates a value of a resource
		/// </summary>
		/// <param name="name">Resource Key</param>
		/// <param name="culture">Resource Culture</param>
		/// <param name="value">Value</param>
		public abstract void WriteResource(string name, string culture, string value);

		protected void UpdateCachedResource(string name, string culture, string value)
		{

			if (Cache && resources != null)
			{
				var dictKey = CachedResourceKey(name, culture);
				if (resources.ContainsKey(dictKey))
				{
					resources[dictKey].Value = value;
				}
			}
		}

    }
}
