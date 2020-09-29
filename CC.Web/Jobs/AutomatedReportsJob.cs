using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CC.Web.Jobs
{
	public class AutomatedReportsJob : LoggingJob
	{
		public AutomatedReportsJob()
		{
			this.log = log4net.LogManager.GetLogger(typeof(AutomatedReportsJob));
		}
		protected override void ExecuteInternal(IJobExecutionContext contex)
		{
			using (var db = new CC.Data.ccEntities())
			{
				CC.Data.User user = db.Users.SingleOrDefault(f => f.UserName == HttpContext.Current.User.Identity.Name);
				CC.Web.Helpers.AutomatedReportsHelper.AutoEmailAllReports(CC.Data.Services.PermissionsFactory.GetPermissionsFor(user));
			}
		}
	}
}