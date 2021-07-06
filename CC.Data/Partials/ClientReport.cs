using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace CC.Data
{
	public partial class ClientReport : IValidatableObject, IClientReport
	{

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			var sender = validationContext.ObjectInstance;





			if (this.ClientId == default(int) && this.Client == null)
			{
				yield return new ValidationResult("Client not found");
				yield break;
			}

            int reportingMethodId = 0;
            //check uniqueness of the record
            using (var db = new ccEntities())
            {
                var service = (db.SubReports.Where(f => f.Id == this.SubReportId).Select(f => new { reportingMethodId = f.AppBudgetService.Service.ReportingMethodId })).SingleOrDefault();
                if (service != null)
                {
					reportingMethodId = service.reportingMethodId;
                }

                var duplicates = db.ClientReports
                    .Where(f => f.ClientId == this.ClientId && f.SubReportId == this.SubReportId && f.Rate == this.Rate)	//natural key
                    .Where(f => this.Id == default(int) || f.Id != this.Id);	//surrogate key if this is not a new record (id=0)
                if (duplicates.Any())
                {
                    yield return new ValidationResult("Can't insert duplicate record");
                    yield break;
                }
            }




			if (this.Client != null)
			{
				if (this.SubReport != null)
				{
					if (this.SubReport.AppBudgetService != null)
					{
						//user belongs to the agency group
						if (this.Client.AgencyId != SubReport.AppBudgetService.AgencyId)
						{
							yield return new ValidationResult(string.Format("The client (CC ID: {0}) does not belong to the subreport's agency (Org ID: {1}.",
								this.ClientId, this.SubReport.AppBudgetService.AgencyId));
						}
					}



				}
			}

			if (sender is ClientReport)
			{
				if (this.Client != null)
				{
					if (this.Client.LeaveDate.HasValue)
					{
						var leaveDate = this.Client.LeaveDate.Value;
						DateTime? reportStartdate = null;
						DateTime? reportEndDate = null;
						if (this.SubReport != null && this.SubReport.MainReport != null)
						{
							reportStartdate = this.SubReport.MainReport.Start;
							reportEndDate = this.SubReport.MainReport.End;
						}
						if (this.ClientAmountReports != null && this.ClientAmountReports.Any() && (reportingMethodId == (int)Service.ReportingMethods.Homecare || reportingMethodId == (int)Service.ReportingMethods.HomecareWeekly))
						{
							reportStartdate = this.ClientAmountReports.Max(f => f.ReportDate);
						}

						if (this.Client.DeceasedDate.HasValue && reportingMethodId == (int)Service.ReportingMethods.Homecare)
						{
                            leaveDate = leaveDate.AddMonths(1);
                            var daysInMonth = DateTime.DaysInMonth(leaveDate.Year, leaveDate.Month);
                            leaveDate = leaveDate.AddDays(daysInMonth - leaveDate.Day);
						}

						if (reportStartdate.HasValue)
						{
							if (leaveDate < reportStartdate && reportingMethodId != (int)Service.ReportingMethods.HomecareWeekly)
							{
								yield return new ValidationResult(string.Format("Report Date {0}, CCID {2} is invalid. It must be less than {1}.", reportStartdate.Value.ToShortDateString(), leaveDate.ToShortDateString(), this.Client.Id));
							}
							else if (this.Client.DeceasedDate.HasValue && this.Client.DeceasedDate < reportStartdate && string.IsNullOrWhiteSpace(this.Remarks))
							{
								yield return new ValidationResult("Please specify Unique Circumstances for report");
							}
						}
					}

				




				}

			//home care only   
				if (this.Client!=null && this.SubReport!=null)
					if (this.Client.ApprovalStatus.Id == (int)CC.Data.ApprovalStatusEnum.ApprovedHomecareOnly &&
						this.SubReport.AppBudgetService.Service.ServiceType.Id != (int)Service.ServiceTypes.Homecare)
					{

						DateTime modifDate = (DateTime) Client.ApprovalStatusUpdated;
						if (modifDate >= SubReport.MainReport.Start && modifDate <= SubReport.MainReport.End)
						{
							if (string.IsNullOrWhiteSpace(this.Remarks))
							{
								yield return new ValidationResult("Client Approval Status is Approved -Homecare only during this reporting period. Please enter unique circumastances");
							}
						}
						else if (modifDate < SubReport.MainReport.Start)
						{
							if (string.IsNullOrWhiteSpace(this.Remarks))
							{
								yield return new ValidationResult("Client Approval Status is Approved -Homecare only");
							}
						}

					}
						

				}



				//run validation on children if the validation started in clientreport.Validate()
				foreach (var ar in this.ClientAmountReports)
				{
					foreach (var vr in ar.Validate(validationContext))
					{
						yield return vr;
					}
				}

				if (this.SubReport != null)
				{

					if (this.SubReport.AppBudgetService.Service.EnforceTypeConstraints)
					{
						switch (this.SubReport.AppBudgetService.Service.ReportingMethodEnum)
						{
							case Service.ReportingMethods.Homecare:
							case Service.ReportingMethods.HomecareWeekly:
								break;
							case Service.ReportingMethods.Emergency:
								foreach (var vr in this.ValidateEmergencyReport(validationContext)) { yield return vr; }
								break;
						}
					}

                //validation for HAS2Date for 168 hours
             //   if (this.Client.HAS2Date.HasValue)
              //  {
              //      var HAS2Date = this.Client.HAS2Date.Value;
              //  }


                }


		}

		private IEnumerable<ValidationResult> ValidateEmergencyReport(ValidationContext context)
		{

			if (this.Amount == null)
			{
				yield return new ValidationResult("Amount is a required field", new[] { "Amount" });
			}

			using (var db = new ccEntities())
			{


			}
		}
		public void ApplyValues(ClientReport cr, int? monthsToAdd)
		{
			//primitives go here
			this.Amount = cr.Amount;
			this.Rate = cr.Rate;

			foreach (var ar in cr.ClientAmountReports)
			{
				var newDate = ar.ReportDate.AddMonths(monthsToAdd ?? 0);
				var own = this.ClientAmountReports.SingleOrDefault(f => f.ReportDate == newDate);
				if (own == null)
				{
					own = new ClientAmountReport();
				}
				own.Quantity = ar.Quantity;
				own.ReportDate = newDate;
				this.ClientAmountReports.Add(own);
			}
		}
	}

	public interface IClientReport
	{
		int ClientId { get; set; }
		int SubReportId { get; set; }
	}
	public interface IClientAmountReport : IClientReport
	{
		decimal Amount { get; set; }
	}

}
