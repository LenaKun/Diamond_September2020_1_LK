using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Globalization;


namespace CC.Data
{
	public partial class ClientAmountReport : IValidatableObject
	{
		public decimal QuantityChange { get; set; }
		//public enum Types
		//{
		//	HomeCareWeekDay = 0,
		//	HomeCareHoliday = 1
		//}

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			//the entrance of the validation chain
			var origin = validationContext.ObjectInstance as ClientAmountReport;

			var instance = this;

			//quantity is less then max home care allowed hours

			var start = this.ReportDate;
			if (start == default(DateTime))
			{
				yield return new ValidationResult("date not set");
			}

			using (var db = new ccEntities())
			{
				var reportingMethodId = db.SubReports.Where(f => f.Id == instance.ClientReport.SubReportId).Select(f => f.AppBudgetService.Service.ReportingMethodId).SingleOrDefault();
				if (reportingMethodId != (int)Service.ReportingMethods.HomecareWeekly && start.Day != 1)
				{
					yield return new ValidationResult("report date is invalid - must be the first day of the month");
				}
			}
			if (this.Quantity < 0)
			{
				var msg = string.Format("Quantity must be greater or equal to zero. (reported {0}, on {1})", this.Quantity.Format(), this.ReportDate.ToMonthString());
				yield return new ValidationResult(msg, new[] { "Quantity" });
			}

			if (instance != null)
			{
				if (instance.ClientReport != null)
				{
					if (instance.ClientReport.SubReport != null)
					{
						if (instance.ClientReport.SubReport.MainReport != null)
						{
							var repStart = instance.ReportDate;
							var repEnd = instance.ReportDate.AddMonths(1);
							var mrs = instance.ClientReport.SubReport.MainReport.Start;
							var mre = instance.ClientReport.SubReport.MainReport.End;

							if (repStart < mrs || repStart > mre.AddDays(-1))
							{
								var msg = string.Format("The report date {0}) is invalid because it is outside of the main report period ({1},{2}).", repStart.ToMonthString(), mrs.ToMonthString(), mre.ToMonthString());
								yield return new ValidationResult(msg);
							}
                            var RepWeek = start.AddDays(5);

                            if (instance.ClientReport.Client.HAS2Date > start.AddDays(5) && instance.Quantity > 56)
                            {
                                
                                var CCID = instance.ClientReport.ClientId;

                                var startingWeek = mrs;
                                DayOfWeek selectedDOW = startingWeek.DayOfWeek;

                                DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
                                dfi.FirstDayOfWeek = selectedDOW;
                                Calendar cal = dfi.Calendar;
                                // int weeksCount = cal.GetWeekOfYear(mre.AddDays(-1), dfi.CalendarWeekRule, dfi.FirstDayOfWeek) - cal.GetWeekOfYear(startingWeek, dfi.CalendarWeekRule, dfi.FirstDayOfWeek) + 1;
                                int selectedWeek = cal.GetWeekOfYear(start.AddDays(5), dfi.CalendarWeekRule, dfi.FirstDayOfWeek);

                                var msg = "CCID:" + CCID + "," + " " + "Reporting week: W" + selectedWeek + "," + " " + "Amount Reported:" + instance.Quantity.Format() + " " + "hours" + " " + "(Weekly cap: 56.00 hours)"; 
                               yield return new ValidationResult(msg);
                             }
                        }
					}

				}
			}

			yield break;
		}

	}
}
