using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using log4net;
using log4net.Config;

namespace Scheduler
{
	public partial class Service1 : ServiceBase
	{
		public Service1()
		{
			InitializeComponent();
		}

		IScheduler scheduler;

		protected override void OnStart(string[] args)
		{
			var f = new Quartz.Impl.StdSchedulerFactory();
			scheduler = f.GetScheduler();
			var job = JobBuilder.Create<jj>().Build();
			var trigger = TriggerBuilder.Create().WithIdentity("daily")
				.ForJob(job)
				.WithSimpleSchedule(a=>a.WithIntervalInSeconds(10))
				//.WithDailyTimeIntervalSchedule(a=>{ a.StartingDailyAt(new TimeOfDay(0, 0)).WithIntervalInMinutes(1).WithRepeatCount(5); })
				.Build();
			scheduler.ScheduleJob(trigger);
		}

		protected override void OnStop()
		{
		}
	}

	public class jj : IJob
	{
		private readonly ILog log = LogManager.GetLogger(typeof(jj));

		public void Execute(IJobExecutionContext context)
		{
			log.Info("job start");	
		}
	}
}
