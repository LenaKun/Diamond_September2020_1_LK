using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using CsvHelper;
using CsvHelper.Configuration;
using System.IO;
using System.Reflection;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace p4b.Web.Export
{
	public class CsvResult<T> : ActionResult where T : class
	{
		#region constructors

		public CsvResult()
			: base()
		{
			this.fileName = "output.csv";
		}

		public CsvResult(IEnumerable<T> records)
			: this()
		{
			this.records = records;
		}

		public CsvResult(IEnumerable<T> records, IEnumerable<PropertyInfo> properties)
			: this(records)
		{
			this.properties = properties;
		}

		#endregion

		#region props

		public string fileName { get; set; }
		public IEnumerable<T> records { get; set; }
		public IEnumerable<PropertyInfo> properties { get; set; }

		#endregion

		public override void ExecuteResult(ControllerContext context)
		{
			var response = context.HttpContext.Response;

			response.Clear();

			var tempfile = System.IO.Path.GetTempFileName();
			using (var tempfileStreamWriter = new StreamWriter(tempfile, false, System.Text.Encoding.UTF8))
			{

				WriteRecords(tempfileStreamWriter);
			}
			WriteHeaders(response, tempfile);
			response.WriteFile(tempfile);
		}

		#region private members

		private void WriteHeaders(HttpResponseBase response, string tempFile)
		{

			//response.AddHeader("content-disposition", String.Format("attachment;filename={0}.csv", this.fileName));
            response.AddHeader("content-disposition", String.Format("attachment;filename={0}", this.fileName));
			response.AddHeader("Content-Type", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
			response.AddHeader("Content-Length", new FileInfo(tempFile).Length.ToString());
		}

		private void WriteRecords(System.IO.TextWriter textWriter)
		{
			var csvConfiguration = new CsvConfiguration()
			{
				Quote = '\"',
				QuoteAllFields = false 
			};

			var writer = new CsvWriter(textWriter, csvConfiguration);
			{

				if (csvConfiguration.Properties.Count == 0)
				{
					if (this.properties == null)
					{
						csvConfiguration.AttributeMapping<T>();
					}
					else
					{

						csvConfiguration.AttributeMapping<T>();

						var q = from p in csvConfiguration.Properties
								where !this.properties.Any(pi=>pi.Name == p.PropertyValue.Name)
								select p;
						foreach (var item in q.ToList())
						{
							csvConfiguration.Properties.Remove(item);
						}
						
					}
				}

				foreach (var p in csvConfiguration.Properties)
				{
					writer.WriteField(p.NameValue);
				}

				writer.NextRecord();
				foreach (var record in records)
				{
					foreach (var p in csvConfiguration.Properties)
					{
						var value = p.PropertyValue.GetValue(record, null);
						if (value == null)
						{
							writer.WriteField("NULL", false);
						}
						else if (value is DateTime)
						{
							writer.WriteField(((DateTime)value).ToString(p.FormatValue));
						}
						else if (value is int)
						{
							writer.WriteField(value.ToString());
						}
						else if (value is bool)
						{
							writer.WriteField(((bool)value == true ? 1 : 0).ToString());
						}
						else
						{
							writer.WriteField(value, p.TypeConverterValue);
						}
					}
					writer.NextRecord();
				}

			}
		}

		#endregion
	}
}
