using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using A = DocumentFormat.OpenXml.Drawing;
using Ap = DocumentFormat.OpenXml.ExtendedProperties;
using Vt = DocumentFormat.OpenXml.VariantTypes;

namespace GeneratedCode
{

	public class MostlyGeneratedClass
	{
		// Creates a SpreadsheetDocument
		public void CreatePackage<T>(string filePath, IEnumerable<T> data)
		{
			using (SpreadsheetDocument package = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook))
			{
				CreateParts<T>(package, data);
			}
		}

		// Adds child parts and generates content of the specified part
		private void CreateParts<T>(SpreadsheetDocument document, IEnumerable<T> data)
		{
			WorkbookPart workbookPart1 = document.AddWorkbookPart();
			GenerateWorkbookPart1Content(workbookPart1);

			WorksheetPart worksheetPart1 = workbookPart1.AddNewPart<WorksheetPart>("rId1");
			GenerateWorksheetPart1Content<T>(worksheetPart1, data);
		}

		// Generates content of workbookPart1. 
		private void GenerateWorkbookPart1Content(WorkbookPart workbookPart1)
		{
			Workbook workbook1 = new Workbook();
			workbook1.AddNamespaceDeclaration("r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");

			Sheets sheets1 = new Sheets();
			Sheet sheet1 = new Sheet() { Name = "Sheet1", SheetId = (UInt32Value)1U, Id = "rId1" };
			sheets1.Append(sheet1);

			workbook1.Append(sheets1);
			workbookPart1.Workbook = workbook1;
		}

		// Generates content of worksheetPart1. 
		private void GenerateWorksheetPart1Content<T>(WorksheetPart worksheetPart1, IEnumerable<T> data)
		{
			Worksheet worksheet1 = new Worksheet();
			SheetData sheetData1 = new SheetData();


			var props = (typeof(T)).GetProperties().Where(f =>

				!f.IsDefined(typeof(ScaffoldColumnAttribute), true) || ((ScaffoldColumnAttribute)f.GetCustomAttributes(typeof(ScaffoldColumnAttribute), true).First()).Scaffold == true
			).OrderBy(f=>{
				if (f.IsDefined(typeof(DisplayAttribute), true))
				{
					return ((DisplayAttribute)f.GetCustomAttributes(typeof(DisplayAttribute), true).First()).GetOrder();
				}
				else
				{
					return default(int);
				}
			});
			DocumentFormat.OpenXml.Spreadsheet.Row headerRow = new DocumentFormat.OpenXml.Spreadsheet.Row();
			UInt32Value rowIndex = 1;
			headerRow.RowIndex = rowIndex++;
			List<String> columns = new List<string>();
			foreach (var prop in props)
			{
				var headerName = prop.Name;
				if (prop.IsDefined(typeof(DisplayNameAttribute), true))
				{
					headerName = ((DisplayNameAttribute)prop.GetCustomAttributes(typeof(DisplayNameAttribute), true).First()).DisplayName;
				}
				if (prop.IsDefined(typeof(DisplayAttribute), true))
				{
					var da = ((DisplayAttribute)prop.GetCustomAttributes(typeof(DisplayAttribute), true).First());
					headerName = da.GetShortName()?? da.GetName()?? prop.Name;
				}

				columns.Add(prop.Name);
				DocumentFormat.OpenXml.Spreadsheet.Cell cell = headerRow.Create();
				cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;

				cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(headerName);
				headerRow.AppendChild(cell);
			}
			sheetData1.AppendChild(headerRow);

			
			foreach (var item in data)
			{
				int cellIndex = 0;
				var row = sheetData1.Create();
				foreach (var prop in props)
				{
					var val = prop.GetValue(item, null);
					string str = null;
					if (val == null)
					{
						str = null;
					}
					else if ((prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(Nullable<DateTime>)) && prop.IsDefined(typeof(DisplayFormatAttribute),true))
					{

						var df = ((DisplayFormatAttribute)prop.GetCustomAttributes(typeof(DisplayFormatAttribute),true).First());

						if (df != null)
						{
							var date = (val as DateTime?).Value;

							str = date.ToString(df.DataFormatString);
						}
					}
					else
					{
						str = val.ToString();
					}

					var cell = new Cell()
					{
						CellReference = XlsxExtensions.Column(cellIndex++) + rowIndex.ToString()
					};

					if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(Nullable<int>)
						||prop.PropertyType == typeof(decimal) || prop.PropertyType == typeof(Nullable<decimal>))
					{
						cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.Number;
					}
					else
					{
						cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
					}
					
					

					cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(str);
					row.AppendChild(cell);

				}
				sheetData1.AppendChild(row);
				rowIndex++;
			}



			worksheet1.Append(sheetData1);
			worksheetPart1.Worksheet = worksheet1;
		}


	}
	internal static class XlsxExtensions
	{
		public static Cell Create(this Row row)
		{
			return new Cell()
			{
				CellReference = Column(row.Descendants<Cell>().Count()) + row.RowIndex.ToString()
			};
		}
		public static Row Create(this SheetData sh)
		{
			return new Row() { };
		}

		private static char[] baseChars = Enumerable.Range(65, 26).Select(f => (char)f).ToArray();


		public static string Column(int column)
		{



			var a = column % 26;
			string result = "" + (char)(65 + a);
			column = (column - a) / 26;

			while (column > 0)
			{
				a = (column % 26);
				result = (char)(65 + a - 1) + result;
				column = (column - a) / 26;
			}

			return result;
		}
	}



}
