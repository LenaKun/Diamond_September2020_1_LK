using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quartz;

namespace CC.Web.Jobs
{
	public abstract class LoggingJob:Quartz.IJob
	{
		protected log4net.ILog log;
		
		protected abstract void ExecuteInternal(IJobExecutionContext contex);
		public void Execute(Quartz.IJobExecutionContext context)
		{
			log.Info("Execute started");
			try
			{
				this.ExecuteInternal(context);
				log.Info("Execute ended");
			}
			catch (Exception ex)
			{
				log.Fatal("Execute failed", ex);
			}
		}
	}
}