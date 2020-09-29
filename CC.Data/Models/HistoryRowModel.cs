using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CC.Data.Models
{
	public class HistoryRowModel
	{
		public DateTime UpdateDate { get; set; }

		public string UpdatedBy { get; set; }

		public string OldValue { get; set; }

		public string NewValue { get; set; }

		public string FieldName { get; set; }
	}
}