using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CC.Data
{
	public partial class EmergencyReport : IValidatableObject
	{

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			//check report date value
			//it should be included in mainreport's period in case exists for all report types
			if (this.SubReport != null)
			{

				var mr = this.SubReport.MainReport;
				if (mr != null)
				{
					if (this.ReportDate >= mr.End || this.ReportDate < mr.Start)
					{
						var msg = string.Format("Report date ({2}) must be included in Financial Report's period (Start: {0}, End: {1})",
							this.SubReport.MainReport.Start.ToMonthString(),
							this.SubReport.MainReport.End.ToMonthString(),
							this.ReportDate.ToDateString());
						yield return new ValidationResult(msg, new[] { "ReportDate" });
					}

				}
			}

			using (var db = new ccEntities())
			{
				var q = (from c in db.Clients
						 where c.Id == this.ClientId
						 select new
						 {
							 JoinDate = c.JoinDate,
							 LeaveDate = c.LeaveDate,
							 DeceasedDate = c.DeceasedDate,
                             AustrianEligible = c.AustrianEligible,
							 RomanianEligible=c.RomanianEligible,
							 ApprovalStatusId=c.ApprovalStatusId,
							 ApprovalStatusUpdated=c.ApprovalStatusUpdated

						 }).SingleOrDefault();
				if (q == null)
				{
					//client does not exist
					//the date can not be verified
				}
				else
				{

				   if (this.SubReport!=null)
				   {

					   var app_budget_service = db.AppBudgetServices.Where(f => f.Id == this.SubReport.AppBudgetServiceId).SingleOrDefault();
					   if (app_budget_service!=null)
					   {
						   var service=db.Services.Where(s=>s.Id==app_budget_service.ServiceId).SingleOrDefault();
					        if (service!=null)
							{

								if (q.ApprovalStatusId == (int)CC.Data.ApprovalStatusEnum.ApprovedHomecareOnly &&
									service.TypeId != (int)Service.ServiceTypes.Homecare)
								{
									//Home Care only

									if (q.ApprovalStatusUpdated >= SubReport.MainReport.Start && q.ApprovalStatusUpdated <= SubReport.MainReport.End)
									{
										if (string.IsNullOrWhiteSpace(this.UniqueCircumstances))
										{
											yield return new ValidationResult("Client Approval Status is Approved -Homecare only during this reporting period. Please enter unique circumastances");
										}
									}
									else if (q.ApprovalStatusUpdated <= SubReport.MainReport.Start)
									{
										yield return new ValidationResult("Client Approval Status is Approved -Homecare only");
									}
								}
					          }
						
				           }
				      }

					var leaveDateEx = q.LeaveDate;
					if (leaveDateEx.HasValue) { leaveDateEx = q.LeaveDate.Value.AddDays(q.DeceasedDate.HasValue ? SubReport.DeceasedDaysOverhead : 0); }
					if (this.ReportDate < q.JoinDate)
					{
						yield return new ValidationResult("Report date must be greater than client join date.");
					}
					if (this.ReportDate > q.LeaveDate && !q.AustrianEligible && !q.RomanianEligible)
					{
						if (q.DeceasedDate.HasValue)
						{
							if (this.ReportDate > q.DeceasedDate.Value.AddDays(SubReport.DeceasedDaysOverhead))
							{
								yield return new ValidationResult("Report date can not be greater than client deceased date.");
							}
							else if (string.IsNullOrEmpty(this.Remarks))
							{
								yield return new ValidationResult("Please specify Purpose of Grant");
							}
						}
						else
						{
							yield return new ValidationResult("Report date can not be greater than client Leave date.");
						}
					}
                    else if (this.ReportDate > q.LeaveDate && q.AustrianEligible && !q.RomanianEligible && string.IsNullOrEmpty(this.UniqueCircumstances))
                    {
                        yield return new ValidationResult("The unique circumstances must be filled.");
                    }
				}
			}
		}

	}


}
