using System;
using OpenXmlPowerTools.SpreadsheetWriter;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication3
{
	class Program
	{
		static void Main(string[] args)
		{
			var t = new p4b.Web.Export.Class1();
			t.fileName = @"c:\temp\Book1_out.xlsx";
			t.Write(new OpenXmlPowerTools.SpreadsheetWriter.Worksheet
			{
				Rows = new List<OpenXmlPowerTools.SpreadsheetWriter.Row>
				{
					new OpenXmlPowerTools.SpreadsheetWriter.Row{
						Cells = new List<Cell>{
							new Cell{
								CellDataType=OpenXmlPowerTools.SpreadsheetWriter.CellDataType.Date,
								Value=DateTime.Now

							},
							new Cell{
								CellDataType=OpenXmlPowerTools.SpreadsheetWriter.CellDataType.String,
								Value="a"+DateTime.Now.Millisecond
							},
							new Cell{
								CellDataType=OpenXmlPowerTools.SpreadsheetWriter.CellDataType.Number,
								Value=DateTime.Now.Second/37
							},
							new Cell{
								CellDataType=OpenXmlPowerTools.SpreadsheetWriter.CellDataType.Boolean,
								Value=true
							},
						}
					}
				}
			});
			System.Diagnostics.Process.Start(t.fileName);
		}
	}
}
