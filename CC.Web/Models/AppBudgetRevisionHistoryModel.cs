using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CC.Data;

namespace CC.Web.Models
{
	public class AppBudgetRevisionHistoryModel
	{
		public AppBudgetRevisionHistoryModel()
		{
			this.Filter = new FilterModel();
		}
		public int Id { get; set; }
		public AppBudget AppBudget { get; set; }
		public IEnumerable<Entry> Rows{get;set;}
		public FilterModel Filter { get; set; }

		public class FilterModel
		{
			public int? AgencyId { get; set; }
			public DateTime? RevisionDate { get; set; }
			public int? ServiceTypeId { get; set; }
			public int? ServiceId { get; set; }
		}
		public class Entry : AppBudgetServiceDetailsModel
		{
			public DateTime Date { get; set; }

			public int AgencyId { get; set; }

			public int ServiceId { get; set; }

			public int ServiceTypeId { get; set; }
		}
	}
}