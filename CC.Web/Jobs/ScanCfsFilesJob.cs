using CC.Web.Controllers;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CC.Web.Jobs
{
    public class ScanCfsFilesJob : LoggingJob
    {
		public ScanCfsFilesJob()
		{
			this.log = log4net.LogManager.GetLogger(typeof(ScanCfsFilesJob));
		}
		protected override void ExecuteInternal(IJobExecutionContext contex)
		{
            ClientsController cl = new ClientsController();
            cl.ScanAndImportCfsFiles();
        }
    }
}