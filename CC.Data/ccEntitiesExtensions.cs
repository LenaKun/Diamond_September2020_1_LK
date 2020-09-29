using System;
using System.Collections.Generic;
using System.Collections;
using System.Data.Objects.SqlClient;
using System.Linq;
using System.Text;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
namespace CC.Data
{
	public static class ccEntitiesExtensions
	{
		/// <summary>
		/// Returns client amount reports that are contained in main report with specific Id or in any other main report which was submitted by the agency
		/// </summary>
		/// <param name="ClientAmountReports"></param>
		/// <param name="mainReportId"></param>
		/// <returns></returns>

		#region extensions

		#endregion
		[EdmFunction("Edm", "AddDays")]
		public static DateTime? AddDays(DateTime? dateValue, int? addValue)
		{
			return dateValue == null || addValue == null ? (DateTime?)null : dateValue.Value.AddDays(addValue.Value);
		}
		[EdmFunction("Edm", "AddMonths")]
		public static DateTime? AddMonths(DateTime? dateValue, int? addValue)
		{
			return dateValue == null || addValue == null ? (DateTime?)null : dateValue.Value.AddMonths(addValue.Value);
		}
		[EdmFunction("ccModel.Store", "tz_check")]
		public static bool? tz_check(string input)
		{
			throw new NotImplementedException();
		}
		[EdmFunction("ccModel.Store", "HcCap")]
		public static decimal? HcCap(int clientId, DateTime? checkPeriodStart, DateTime? checkPeriodEnd)
		{
			using (var db = new ccEntities())
			{
				var queryText = "SELECT ccModel.Store.HcCap(@clientId, @checkPeriodStart, @checkPeriodEnd) FROM {1}";
				var result = db.CreateQuery<decimal?>(queryText
					,new ObjectParameter("clientId", clientId)
					,new ObjectParameter("checkPeriodStart", checkPeriodStart)
					,new ObjectParameter("checkPeriodEnd", checkPeriodEnd));
				return result.First();
			}
		}
		[EdmFunction("ccModel.Store", "HcCapWithoutCarry")]
		public static decimal? HcCapWithoutCarry(int clientId, DateTime? checkPeriodStart, DateTime? checkPeriodEnd)
		{
			using (var db = new ccEntities())
			{
				var queryText = "SELECT ccModel.Store.HcCapWithoutCarry(@clientId, @checkPeriodStart, @checkPeriodEnd) FROM {1}";
				var result = db.CreateQuery<decimal?>(queryText
					, new ObjectParameter("clientId", clientId)
					, new ObjectParameter("checkPeriodStart", checkPeriodStart)
					, new ObjectParameter("checkPeriodEnd", checkPeriodEnd));
				return result.First();
			}
		}
		[EdmFunction("ccModel.Store", "HcCapLeaveDate")]
		public static decimal? HcCapLeaveDate(int clientId, DateTime? checkPeriodStart, DateTime? checkPeriodEnd)
		{
			using (var db = new ccEntities())
			{
				var queryText = "SELECT ccModel.Store.HcCapLeaveDate(@clientId, @checkPeriodStart, @checkPeriodEnd) FROM {1}";
				var result = db.CreateQuery<decimal?>(queryText
					, new ObjectParameter("clientId", clientId)
					, new ObjectParameter("checkPeriodStart", checkPeriodStart)
					, new ObjectParameter("checkPeriodEnd", checkPeriodEnd));
				return result.First();
			}
		}
		[EdmFunction("ccModel.Store", "GovHcCapLeaveDate")]
		public static decimal? GovHcCapLeaveDate(int clientId, DateTime? checkPeriodStart, DateTime? checkPeriodEnd)
		{
			using (var db = new ccEntities())
			{
				var queryText = "SELECT ccModel.Store.GovHcCapLeaveDate(@clientId, @checkPeriodStart, @checkPeriodEnd) FROM {1}";
				var result = db.CreateQuery<decimal?>(queryText
					, new ObjectParameter("clientId", clientId)
					, new ObjectParameter("checkPeriodStart", checkPeriodStart)
					, new ObjectParameter("checkPeriodEnd", checkPeriodEnd));
				return result.First();
			}
		}
		[EdmFunction("ccModel.Store", "GetWeekNumber")]
		public static int GetWeekNumber(DateTime ReportDate, int WeekStartDay)
		{
			using (var db = new ccEntities())
			{
				var queryText = "SELECT ccModel.Store.GetWeekNumber(@ReportDate, @WeekStartDay) FROM {1}";
				var result = db.CreateQuery<int>(queryText
					, new ObjectParameter("ReportDate", ReportDate)
					, new ObjectParameter("WeekStartDay", WeekStartDay));
				return result.First();
			}
		}
		/// <summary>
		/// returns last submitted hc report date end (+1month)
		/// No Eligibility periods or functionality scores are allowed to be changed prior this date.
		/// </summary>
		/// <param name="clientId"></param>
		/// <returns></returns>
		public static DateTime? LastSubmittedHcRepDate(int clientId)
		{
			using (var db = new ccEntities())
			{
				var result = (from cr in db.ClientReports
							  where cr.ClientId == clientId
							  join sr in db.SubReports on cr.SubReportId equals sr.Id
							  join mr in db.MainReports.Where(MainReport.Submitted) on sr.MainReportId equals mr.Id
							  where sr.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.Homecare ||
									sr.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.HomecareWeekly
							  select mr).OrderByDescending(f => f.Start);
				if(result.Any())
				{
					return result.FirstOrDefault().End;
				}
				return null;
			}
		}

		[EdmFunction("ccModel.Store", "CfsLevelForGivenDate")]
		public static int CfsLevelForGivenDate(int clientId, DateTime? givenDate)
		{
			using (var db = new ccEntities())
			{
				var queryText = "SELECT ccModel.Store.CfsLevelForGivenDate(@clientId, @givenDate) FROM {1}";
                var result = db.CreateQuery<int>(queryText
                	, new ObjectParameter("clientId", clientId)
                	, new ObjectParameter("givenDate", givenDate));
                
                return result.First();
               
			}
		}

		/// <summary>
		/// Returns the first date of an approval status Approved or Research in progress with proof
		/// No Eligibility periods are allowed to be changed prior this date.
		/// </summary>
		/// <param name="clientId"></param>
		/// <returns></returns>
		public static DateTime? ClientFirstApprovedDate(int clientId)
		{
			using (var db = new ccEntities())
			{
				var result = (from h in db.Histories
							 where h.ReferenceId == clientId && h.TableName == "Clients" && h.FieldName == "ApprovalStatusId"
							 where h.NewValue == "2" || h.NewValue == "2048"
							 select h).OrderBy(f => f.UpdateDate);
				if (result.Any()) return result.FirstOrDefault().UpdateDate;
				return null;
			}
		}
	}
}
