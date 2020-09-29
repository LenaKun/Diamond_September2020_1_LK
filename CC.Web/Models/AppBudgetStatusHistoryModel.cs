using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CC.Data;

namespace CC.Web.Models
{
	public class AppBudgetStatusHistoryModel
	{
		public AppBudget AppBudget { get; set; }
		public IEnumerable<AppBudgetStatusHistoryEntry> StatusHistory { get; set; }
		public class AppBudgetStatusHistoryEntry
		{
			public int OldStatusId { get; set; }
			public AppBudgetApprovalStatuses OldStatus { get { return (AppBudgetApprovalStatuses)this.OldStatusId; } }
			public int NewStatusId { get; set; }
			public AppBudgetApprovalStatuses NewStatus { get { return (AppBudgetApprovalStatuses)this.NewStatusId; } }
			public string UserName { get; set; }
			public DateTime Date { get; set; }
		}
	}
}