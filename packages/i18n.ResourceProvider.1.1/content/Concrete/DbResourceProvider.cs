using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Resources.Abstract;
using Resources.Entities;

namespace Resources.Concrete
{
    public class DbResourceProvider : BaseResourceProvider
    {
        // Database connection string        
        private static string connectionString = null;

        public DbResourceProvider(){

            connectionString = ConfigurationManager.ConnectionStrings["MvcInternationalization"].ConnectionString;
        }

        public DbResourceProvider(string connection)
        {
            connectionString = connection;
        }

        protected override IList<ResourceEntry> ReadResources()
        {
            var resources = new List<ResourceEntry>();

            const string sql = "select Culture, Name, Value from dbo.Resources;";

            using (var con = new SqlConnection(connectionString)) {
                var cmd = new SqlCommand(sql, con);

                con.Open();

                using (var reader = cmd.ExecuteReader()) {
                    while (reader.Read()) {
                        resources.Add(new ResourceEntry { 
                            Name = reader["Name"].ToString(),
                            Value = reader["Value"].ToString(),
                            Culture = reader["Culture"].ToString()
                        });
                    }

                    if (!reader.HasRows) throw new Exception("No resources were found");
                }
            }

            return resources;
            
        }

        protected override ResourceEntry ReadResource(string name, string culture)
        {
            ResourceEntry resource = null;

            const string sql = "select Culture, Name, Value from dbo.Resources where culture = @culture and name = @name;";

            using (var con = new SqlConnection(connectionString)) {
                var cmd = new SqlCommand(sql, con);

                cmd.Parameters.AddWithValue("@culture", culture);
                cmd.Parameters.AddWithValue("@name", name);

                con.Open();

                using (var reader = cmd.ExecuteReader()) {
                    if (reader.Read()) {
                        resource = new ResourceEntry {
                            Name = reader["Name"].ToString(),
                            Value = reader["Value"].ToString(),
                            Culture = reader["Culture"].ToString()
                        };
                    }

                    if (!reader.HasRows) throw new Exception(string.Format("Resource {0} for culture {1} was not found", name, culture));
                }
            }

            return resource;            
           
        }

       

       
    }
}
