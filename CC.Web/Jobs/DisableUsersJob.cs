using Quartz;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Data.Entity;
using System.Web;

namespace CC.Web.Jobs
{
    public class DisableUsersJob : LoggingJob
    {
        private static readonly int UserDisabledInDays = int.Parse(ConfigurationManager.AppSettings["UserDisabledInDays"]);

		public DisableUsersJob()
		{
			this.log = log4net.LogManager.GetLogger(typeof(DisableUsersJob));
		}
		protected override void ExecuteInternal(IJobExecutionContext contex)
        {
            using(var db = new CC.Data.ccEntities())
            {
                var users = db.MembershipUsers.Include("User").ToList();
                foreach (var user in users)
                {
                    if ((DateTime.Now - (user.LastLoginDate ?? DateTime.Now)).TotalDays > UserDisabledInDays)
                    {
                        var u = db.Users.SingleOrDefault(f => f.Id == user.User.Id);
                        u.Disabled = true;
                        db.SaveChanges();
                    }
                }
            }
        }
    }
}