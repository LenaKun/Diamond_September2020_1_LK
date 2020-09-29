// <copyright file="ExcelResult.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>The Export to Excel controller results class.</summary>
namespace p4b.Web.Export
{
	using System;
	using System.IO;
	using System.Linq;
	using System.Collections.Generic;
	using System.Web;
	using System.Web.Mvc;


	/// <summary>
	/// Excel result class
	/// </summary>
	public class ExcelResult<T> : ActionResult where T : class
	{
		/// <summary>
		/// File Name. 
		/// </summary>
		private string excelFileName;

		/// <summary>
		/// Sheet Name. 
		/// </summary>
		private string excelWorkSheetName;

		/// <summary>
		/// Excel Row data.
		/// </summary>
		private IEnumerable<T> rowData;

		/// <summary>
		/// Excel Header Data.
		/// </summary>
		private string[] headerData = null;

		/// <summary>
		/// Row Data Keys.
		/// </summary>
		private string[] rowPointers = null;

		/// <summary>
		/// Action Result: Excel result constructor for returning values from rows. 
		/// </summary>
		/// <param name="fileName">Excel file name.</param>
		/// <param name="workSheetName">Excel worksheet name: default: sheet1.</param>
		/// <param name="rows">Excel row values.</param>
		public ExcelResult(string fileName, string workSheetName, IEnumerable<T> rows)
			: this(fileName, workSheetName, rows, null, null)
		{
		}

		/// <summary>
		/// Action Result: Excel result constructor for returning values from rows and headers. 
		/// </summary>
		/// <param name="fileName">Excel file name.</param>
		/// <param name="workSheetName">Excel worksheet name: default: sheet1.</param>
		/// <param name="rows">Excel row values.</param>
		/// <param name="headers">Excel header values.</param>
		public ExcelResult(string fileName, string workSheetName, IEnumerable<T> rows, string[] headers)
			: this(fileName, workSheetName, rows, headers, null)
		{
		}

		/// <summary>
		/// Action Result: Excel result constructor for returning values from rows and headers and row keys. 
		/// </summary>
		/// <param name="fileName">Excel file name.</param>
		/// <param name="workSheetName">Excel worksheet name: default: sheet1.</param>
		/// <param name="rows">Excel row values.</param>
		/// <param name="headers">Excel header values.</param>
		/// <param name="rowKeys">Key values for the rows collection.</param>
		public ExcelResult(string fileName, string workSheetName, IEnumerable<T> rows, string[] headers, string[] rowKeys)
		{
			this.rowData = rows;
			this.excelFileName = fileName;
			this.excelWorkSheetName = workSheetName;
			this.headerData = headers;
			this.rowPointers = rowKeys;
		}
		public ExcelResult(string fileName, IEnumerable<T> rows)
		{
			this.rowData = rows;
			this.excelWorkSheetName = fileName;
			this.excelFileName = fileName;
		}

		/// <summary>
		///  Gets a value for file name.
		/// </summary>
		public string ExcelFileName
		{
			get { return this.excelFileName; }
		}

		/// <summary>
		///  Gets a value for file name.
		/// </summary>
		public string ExcelWorkSheetName
		{
			get { return this.excelWorkSheetName; }
		}

		/// <summary>
		/// Gets a value for rows.
		/// </summary>
		public IEnumerable<T> ExcelRowData
		{
			get { return this.rowData; }
		}

		/// <summary>
		/// Execute the Excel Result. 
		/// </summary>
		/// <param name="context">Controller context.</param>
		public override void ExecuteResult(ControllerContext context)
		{

			string fn = System.IO.Path.GetTempFileName();

			OpenXmlPowerTools.SpreadsheetWriter.Worksheet workSheet = null;


			if (typeof(T) == typeof(OpenXmlPowerTools.SpreadsheetWriter.Row))
			{
				workSheet = new OpenXmlPowerTools.SpreadsheetWriter.Worksheet
				{
					Rows = this.rowData.Cast<OpenXmlPowerTools.SpreadsheetWriter.Row>()
				};
			}
			else
			{
				workSheet = OpenXmlPowerTools.SpreadsheetWriter.SpreadsheetWriter.ToWorkSheet(this.rowData, true);

			}

			var writer = new Class1();
			writer.fileName = fn;
			writer.Write(workSheet);
			try
			{
				WriteFile(fn, this.excelFileName);
			}
			finally
			{
				System.IO.File.Delete(fn);
			}
		}

		/// <summary>
		/// Writes the memory stream to the browser.
		/// </summary>
		/// <param name="memoryStream">Memory stream.</param>
		/// <param name="excelFileName">Excel file name.</param>
		private static void WriteStream(MemoryStream memoryStream, string excelFileName)
		{
			HttpContext context = HttpContext.Current;
			if (context != null && !context.Response.IsClientConnected)
			{
				return;
			}
			context.Response.Clear();
			context.Response.AddHeader("content-disposition", String.Format("attachment;filename={0}.xlsx", excelFileName));
			context.Response.AddHeader("Content-Type", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
			memoryStream.WriteTo(context.Response.OutputStream);

			memoryStream.Close();
			context.Response.End();
		}

		private static void WriteFile(string file, string excelFileName)
		{
			HttpContext context = HttpContext.Current;
			if (context != null && !context.Response.IsClientConnected)
			{
				return;
			}
			context.Response.Clear();
			context.Response.AddHeader("content-disposition", String.Format("attachment;filename={0}.xlsx", excelFileName));

			context.Response.AddHeader("Content-Type", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

			context.Response.WriteFile(file);

			context.Response.End();
		}
	}
}