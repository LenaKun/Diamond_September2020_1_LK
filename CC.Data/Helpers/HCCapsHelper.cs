using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Objects.SqlClient;
using System.Text;

namespace CC.Data.Helpers
{
	public class HCCapsHelper
	{
		public static IQueryable<WeeklyQuantity> GetWeeklyQuantities(IQueryable<SubReport> subreports, int weekStart)
		{
			var q1 = from sr in subreports
					 from cr in sr.ClientReports
					 from ar in cr.ClientAmountReports
					 group ar by new { cr.ClientId, ar.ReportDate } into g
					 select new
					 {
						 g.Key.ClientId,
						 g.Key.ReportDate,
						 Quantity = g.Sum(f => f.Quantity),
					 }
					  into ar
					 let wd = SqlFunctions.DatePart("weekday", ar.ReportDate)
					 let ws = weekStart
					 let weekStartDate = wd < ws ?
							SqlFunctions.DateAdd("day", ws - wd - 7, ar.ReportDate) :
							SqlFunctions.DateAdd("day", ws - wd, ar.ReportDate)
					 let weekEndDate = SqlFunctions.DateAdd("week", 1, weekStartDate)
					 select new WeeklyQuantity
					 {
						 ClientId = ar.ClientId,
						 ReportDate  = ar.ReportDate,
						 WeekStart = weekStartDate,
						 WeekEnd = weekEndDate,
						 Quantity = ar.Quantity,
					 };
			return q1;
		}

	}
	public class WeeklyQuantity
	{
		public int ClientId { get; internal set; }
		public DateTime ReportDate { get; internal set; }
		public DateTime? WeekStart { get; internal set; }
		public DateTime? WeekEnd { get; internal set; }
		public decimal Quantity { get; internal set; }
	}
}
