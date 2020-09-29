using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CC.Data;
using Quartz;

namespace CC.Web.Jobs
{
	internal class ResearchInProgressJob : LoggingJob
	{
		/// <summary>
		/// "THIS ONLY APPLIES IF THE MODIFICATION HISTORY DATE FOR THE STATUS TO CHANGE TO NEED TO APPLY IS Starting at OCT 7-2014 
		/// and onwards this should be a hard codeded date 
		/// (for clients who have never applied to CC, 90 days after they have been entered into Diamond)
		/// </summary>
		//20275
		private int KEREN_SER_ID = (int)System.Web.Configuration.WebConfigurationManager.AppSettings["IsraelKerenSerNumber"].Parse<int>();
		protected virtual DateTime RULE_START_DATE { get { return new DateTime(2014, 10, 07); } }
		protected virtual ApprovalStatusEnum RULE_APPROVAL_STATUS { get { return ApprovalStatusEnum.ResearchInProgress; } }
		public ResearchInProgressJob()
		{
			this.log = log4net.LogManager.GetLogger(typeof(ResearchInProgressJob));
		}

			/// <summary>
			/// for clients who have Research in Progress approval status, 90 days after they have been entered into Diamond, they no longer can be provided service, but can be reported on for the first 90 days
			/// But, for the Keren only, the clients have to apply before receiving services or being reported on, they do not get any 90 day grace period.
			/// A new automated daily process will be run to check for any clients with """"Research in Progress"""" that this status was modified before 90 days or more and will be updating leave date for today, 
			/// and leave reason ""No Client Follow Up {90 days}"" and will also check mark true a new field Administrative leave.
			/// If any values exists in the deceased date/ reason, the system automated task will not overwrite those.
			/// Once this check box is turned on, the agency is not allowed to change leave reason/ leave date/ leave remarks in any case (acts like deceased date is entered).
			/// If the client deceased during those 90 days, then the system will still allow receiving services, according to the deceased 90 days policy (implemented earlier)
			/// </summary>
			/// <param name="context"></param>
		protected override void ExecuteInternal(IJobExecutionContext contex)
		{

			using (var db = new ccEntities())
			{
				var approvalStatus = RULE_APPROVAL_STATUS;
				var approvalStatusIdstr = ((int)RULE_APPROVAL_STATUS).ToString();
				var t1 = DateTime.Now.Date.AddDays(-90);
				var leaveDate = DateTime.Now.Date;
				var now = DateTime.Now;
				var ruleStartDate = RULE_START_DATE;
				var sysAdminId = db.Users.Where(f => f.UserName == "sysadmin").Select(f => f.Id).FirstOrDefault();
				var a = from c in db.Clients
						let h = (
							from h in db.Histories
							where h.ReferenceId == c.Id
							where h.TableName == "Clients"
							where h.FieldName == "ApprovalStatusId"
							orderby h.UpdateDate descending
							select h
						).FirstOrDefault()
						where h.NewValue == approvalStatusIdstr
						where h.UpdateDate >= ruleStartDate
						where h.UpdateDate < t1
						where c.Agency.GroupId != KEREN_SER_ID
						where c.ApprovalStatusId == (int)approvalStatus
						where c.LeaveDate == null || !c.AdministrativeLeave
						where c.AutoLeaveOverride == null || c.AutoLeaveOverride < leaveDate
						select c;
				foreach (var client in a.ToList())
				{
					log.InfoFormat("Updating leave details for client id {0}", client.Id);
					if (client.LeaveDate == null)
					{
						client.LeaveDate = leaveDate;
						client.LeaveReasonId = 11; /*No Contact (90 days)*/
					}
					client.AdministrativeLeave = true;
					//updatedat/by for history records
					client.UpdatedById = sysAdminId;
					client.UpdatedAt = now;
					try
					{
						db.SaveChanges();
					}
					catch (Exception ex)
					{
						log.Error(ex.Message, ex);
					}
				}
			}
		}
	}
}
