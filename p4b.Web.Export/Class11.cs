using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using OpenXmlPowerTools.SpreadsheetWriter;
using OpenXmlPowerTools;
using System.Xml;

namespace p4b.Web.Export
{
	public class Class1
	{
		public string fileName;


		string template = "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";
		string ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
		string rns = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
		//string mcns = "http://schemas.openxmlformats.org/markup-compatibility/2006";
		//string x14ns = "http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac";


		int rowCount = 0;
		int totalStringsCount = 0;
		List<string> strings = new List<string>();



		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// template has to:
		/// Be a Excel spreadsheet (xlsx)
		/// Have at least one sheet
		/// Have style=1 for date
		/// Have style=2 for decimal
		/// </remarks>
		/// <param name="data"></param>
		public void Write(Worksheet data)
		{
			if (System.IO.File.Exists(fileName))
				System.IO.File.Delete(fileName);


			//System.IO.File.WriteAllBytes(fileName, System.IO.File.ReadAllBytes(@"c:\temp\book3.xlsx"));// Convert.FromBase64String(template));
			System.IO.File.WriteAllBytes(fileName, Convert.FromBase64String(template));

			using (var sDoc = SpreadsheetDocument.Open(fileName, true))
			{
				var wb = sDoc.WorkbookPart;
				var ws = wb.WorksheetParts.FirstOrDefault();
				var ss = wb.SharedStringTablePart;



				WriteSpreadsheetData(data, ws);

				WriteSharedStrings(ss);

			}

		}

		/// <summary>
		/// Dumps collected shared strings into package
		/// </summary>
		/// <param name="ss"></param>
		private void WriteSharedStrings(SharedStringTablePart ss)
		{
			using (var wss = System.Xml.XmlWriter.Create(ss.GetStream(System.IO.FileMode.Create, System.IO.FileAccess.Write)))
			{
				wss.WriteStartDocument();


				wss.WriteStartElement("sst", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");
				//add unique and count later
				wss.WriteAttributeString("uniqueCount", strings.Count.ToString());
				wss.WriteAttributeString("count", totalStringsCount.ToString());
				foreach (var s in strings)
				{
					wss.WriteStartElement("si");
					wss.WriteStartElement("t");
					wss.WriteValue(s);
					wss.WriteEndElement();
					wss.WriteEndElement();
				}

				wss.WriteEndElement();
				wss.WriteEndDocument();

				//<?xml version="1.0" encoding="UTF-8" standalone="true"?>
				//-<sst uniqueCount="4" count="5" xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
				//-<si>
				//<t>date</t>
				//</si>
				//-<si>
				//<t>bool</t>
				//</si>
				//-<si>
				//<t>string</t>
				//</si>
				//-<si>
				//<t>num</t>
				//</si>
				//</sst>
			}
		}

		/// <summary>
		/// Writes data into Sheet1
		/// </summary>
		/// <param name="data"></param>
		/// <param name="ws"></param>
		private void WriteSpreadsheetData(Worksheet data, WorksheetPart ws)
		{
			using (var wss = System.Xml.XmlWriter.Create(ws.GetStream(System.IO.FileMode.Create, System.IO.FileAccess.Write)))
			{

				InitSpreadSheet(wss);
				wss.WriteStartElement("sheetData");

				if (data.ColumnHeadings != null)
				{
					rowCount++;
					WriteRow(wss, data.ColumnHeadings);
				}
				foreach (var row in data.Rows)
				{
					rowCount++;

					WriteRow(wss, row.Cells);

				}

				wss.WriteEndElement();

				wss.WriteStartElement("pageMargins");
				wss.WriteAttributeString("footer", "0.3");
				wss.WriteAttributeString("header", "0.3");
				wss.WriteAttributeString("bottom", "0.75");
				wss.WriteAttributeString("top", "0.75");
				wss.WriteAttributeString("right", "0.7");
				wss.WriteAttributeString("left", "0.7");
				wss.WriteEndElement();


				wss.WriteEndElement();


				wss.WriteEndDocument();
			}


		}

		private void WriteRow(XmlWriter wss, IEnumerable<Cell> row)
		{
			wss.WriteStartElement("row");
			wss.WriteStartAttribute("r");
			wss.WriteValue(rowCount);
			wss.WriteEndAttribute();
			//wss.WriteAttributeString("dyDescent", x14ns, "0.25");

			wss.WriteStartAttribute("spans");
			wss.WriteValue("1:");
			wss.WriteValue(row.Count());
			wss.WriteEndAttribute();

			int cellCount = 0;
			foreach (var cell in row)
			{

				WriteCell(wss, cellCount, cell);

				cellCount++;
			}

			wss.WriteEndElement();
		}

		/// <summary>
		/// Writes cell element
		/// </summary>
		/// <param name="wss"></param>
		/// <param name="cellCount"></param>
		/// <param name="cell"></param>
		private void WriteCell(XmlWriter wss, int cellCount, Cell cell)
		{
			if (cell.Value != null)
			{

				wss.WriteStartElement("c");
				wss.WriteStartAttribute("r");
				wss.WriteValue(SpreadsheetMLUtil.IntToColumnId(cellCount));
				wss.WriteValue(rowCount);
				wss.WriteEndAttribute();
				switch (cell.CellDataType)
				{
					case CellDataType.Boolean:
						wss.WriteAttributeString("t", "b");
						break;
					case CellDataType.Date:
						wss.WriteAttributeString("s", "1");
						break;
					case CellDataType.Number:
						if (cell.Value is decimal)
						{
							wss.WriteAttributeString("s", "2");
						}
						break;
					case CellDataType.String:
						wss.WriteAttributeString("t", "s");
						break;
					case CellDataType.Pct:
						wss.WriteAttributeString("s", "3");
						break;
					default:
						break;
				}


				wss.WriteStartElement("v");
				switch (cell.CellDataType)
				{
					case CellDataType.Boolean:
						wss.WriteValue((bool)cell.Value == true ? 1 : 0);
						break;
					case CellDataType.Date:
						wss.WriteValue(((DateTime)cell.Value).ToOADate());
						break;
					case CellDataType.Pct:
					case CellDataType.Number:
						if (cell.Value is decimal)
						{
							
							var sv = Math.Round(((decimal)cell.Value), 16).ToString();
							if (sv.Contains('.')) 	sv=sv.TrimEnd('0'); //remove zero-s after point
							wss.WriteString(sv);
						}
						else
						{
							wss.WriteValue(cell.Value);
						}
						break;
					case CellDataType.String:
						wss.WriteValue(SharedStringIndex((string)cell.Value));
						break;
					default:
						break;
				}
				wss.WriteEndElement();//close value
				wss.WriteEndElement();
			}
		}

		/// <summary>
		/// Returns an index of shared string.
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		private int SharedStringIndex(string s)
		{
			
			totalStringsCount++;
			var index = strings.BinarySearch(s);
			if (index < 0)
			{
				strings.Add(s);
				return strings.Count - 1;
			}
			else
			{
				return index;
			}


		}

		/// <summary>
		/// writes xml declarations
		/// </summary>
		/// <param name="writer"></param>
		private void InitSpreadSheet(XmlWriter writer)
		{
			writer.WriteStartDocument();

			writer.WriteStartElement("worksheet", ns);
			writer.WriteAttributeString("xmlns", "r", null, rns);
			//writer.WriteAttributeString("xmlns", "mc", null, mcns);
			//writer.WriteAttributeString("mc", "Ignorable", mcns, "x14ac");
			//writer.WriteAttributeString("xmlns", "x14ac", null, x14ns);

			//writer.WriteStartElement("dimensions");
			//writer.WriteAttributeString("ref","A1:D1");
			//writer.WriteEndElement();

			writer.WriteStartElement("sheetViews");
			{
				writer.WriteStartElement("sheetView");
				writer.WriteAttributeString("tabSelected", "1");
				writer.WriteAttributeString("workbookViewId", "0");
				//{
				//	writer.WriteStartElement("selection");
				//	writer.WriteAttributeString("sqref", "A1");
				//	writer.WriteAttributeString("activeCell", "A1");
				//	writer.WriteEndElement();
				//}
				writer.WriteEndElement();
			}
			writer.WriteEndElement();

			writer.WriteStartElement("sheetFormatPr");
			writer.WriteAttributeString("defaultRowHeight", "17");
			//writer.WriteAttributeString("dyDescent", x14ns, "0.25");
			writer.WriteEndElement();






		}


	}
}
