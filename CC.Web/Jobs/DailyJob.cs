using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CC.Data.Repositories;

namespace CC.Web.Jobs
{
    public class AutomaticNotificationsJob : LoggingJob
    {
		public AutomaticNotificationsJob()
		{
			this.log = log4net.LogManager.GetLogger(typeof(AutomaticNotificationsJob));
		}
		protected override void ExecuteInternal(IJobExecutionContext contex)
        {
            CC.Data.Maintanance.NeedToApply();
            CC.Data.Maintanance.NoContact();
            CC.Data.Maintanance.RejectedClients();
        }
    }
}