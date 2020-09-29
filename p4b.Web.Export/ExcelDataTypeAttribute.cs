using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace p4b.Web.Export
{

	public class ExcelDataTypeAttribute : Attribute
	{
		public OpenXmlPowerTools.SpreadsheetWriter.CellDataType DataType { get; private set; }
		public ExcelDataTypeAttribute(OpenXmlPowerTools.SpreadsheetWriter.CellDataType type)
		{
			this.DataType = type;
		}
	}
}
