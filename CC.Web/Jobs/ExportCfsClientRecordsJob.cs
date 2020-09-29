using CC.Web.Controllers;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CC.Web.Jobs
{
	public class ExportCfsClientRecordsJob : LoggingJob
	{
		public ExportCfsClientRecordsJob()
		{
			this.log = log4net.LogManager.GetLogger(typeof(ExportCfsClientRecordsJob));
		}
		protected override void ExecuteInternal(IJobExecutionContext contex)
		{
			try
			{
				ClientsController cl = new ClientsController();
				cl.ExportCfsClientRecords(true);
			}
			catch(Exception ex)
			{
				log.Error(ex);
			}
		}
	}
}