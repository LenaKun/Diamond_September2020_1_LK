using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CC.Web.Jobs
{
	internal class TempApprovedJob: ResearchInProgressJob
	{
		public TempApprovedJob()
		{
			this.log = log4net.LogManager.GetLogger(typeof(TempApprovedJob));
		}
		protected override DateTime RULE_START_DATE
		{
			get
			{
				
				return new DateTime(2014, 10, 4);
			}
		}
		protected override CC.Data.ApprovalStatusEnum RULE_APPROVAL_STATUS
		{
			get
			{
				return CC.Data.ApprovalStatusEnum.ApprovedTemp;
			}
		}
	}
}