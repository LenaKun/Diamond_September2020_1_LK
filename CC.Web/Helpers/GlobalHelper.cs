using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web;

namespace CC.Web.Helpers
{
	public static class GlobalHelper
	{
		public static int? GetWeekStartDay(int AgengyGroupId, int AppId)
		{
			using(var db = new CC.Data.ccEntities())
			{
				if(db.AgencyApps.Any(f => f.Agency.GroupId == AgengyGroupId && f.AppId == AppId))
				{
					return db.AgencyApps.FirstOrDefault(f => f.Agency.GroupId == AgengyGroupId && f.AppId == AppId).WeekStartDay;
				}
				return null;
			}
		}
		public static int GetWeekStartDay(CC.Data.MainReport mainReport, CC.Data.ccEntities db)
		{
			var agencyApp = (from sr in db.SubReports
							 where sr.MainReportId == mainReport.Id
							 select new
							 {
								 AgencyId = sr.AppBudgetService.AgencyId,
								 AppId = sr.AppBudgetService.AppBudget.AppId,
								 AgencyGroupId = sr.AppBudgetService.Agency.GroupId
							 }).FirstOrDefault();

			var startingWeek = mainReport.Start;
			DayOfWeek selectedDOW = startingWeek.DayOfWeek;
			int? selectedDowDb = GlobalHelper.GetWeekStartDay(agencyApp.AgencyGroupId, agencyApp.AppId);
			return selectedDowDb ?? (int)selectedDOW;
		}
		/// <summary>
		/// Retrieves last successful end time of the rep.FinSumJob stored procedure execution
		/// </summary>
		/// <returns></returns>
		public static DateTime? GetFinSumJobDate()
		{
			var message = "FinSumJob";
			using (var db = new CC.Data.ccEntities())
			{
				return db.Globals.Where(f => f.Message == message)
					.OrderByDescending(f => f.Date)
					.Select(f=>f.Date)
					.FirstOrDefault();
			}
		}
	}
}