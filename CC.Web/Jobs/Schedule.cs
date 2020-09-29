using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using CC.Web.Areas.Admin.Models;
using CC.Web.Helpers;

namespace CC.Web.Jobs
{
    public static class Schedule
    {
        // construct item scheduler factory
        private static ISchedulerFactory schedFact = null;

        // get item scheduler
        private static IScheduler sched = null;

        public static void Start()
        {
            sched.Start();
        }

        public static void Stop()
        {
            if (sched != null)
                sched.Shutdown(true);
        }

        public static IJobDetail GetJobDetailByJobKey(JobKey jobKey)
        {
            if(sched == null)
            {
                StartSchedule();
            }
            return sched.GetJobDetail(jobKey);
        }

        public static void DeleteJob(JobKey jobKey)
        {
            sched.DeleteJob(jobKey);
        }

        public static void StartSchedule()
        {
            NameValueCollection properties = new NameValueCollection();
            properties["quartz.scheduler.instanceName"] = "RemoteServer";

            ////// set thread pool info
            properties["quartz.threadPool.type"] = "Quartz.Simpl.SimpleThreadPool, Quartz";
            properties["quartz.threadPool.threadCount"] = "5";
            properties["quartz.threadPool.threadPriority"] = "Normal";

            properties["quartz.jobStore.type"] = "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz";
            properties["quartz.jobStore.useProperties"] = "true";
            properties["quartz.jobStore.dataSource"] = "default";
            properties["quartz.jobStore.tablePrefix"] = "QRTZ_";
            properties["quartz.jobStore.lockHandler.type"] = "Quartz.Impl.AdoJobStore.UpdateLockRowSemaphore, Quartz";

            properties["quartz.dataSource.default.connectionString"] = System.Data.SqlClient.ConnectionStringHelper.GetProviderConnectionString();
            properties["quartz.dataSource.default.provider"] = "SqlServer-20";


            // construct item scheduler factory
            schedFact = new StdSchedulerFactory(properties);

            // get item scheduler
            sched = schedFact.GetScheduler();


            RegisterNotifications();
            RegisterApprovedTempJob();
            RegisterScanCfsFiles();
            RegisterDisableUsers();
            RegisterHcMaximumAllowedCheck();
			RegisterCfsDailyDigestJob();
			RegisterExportCfsClientRecordsJob();
			sched.Start();
        }



        private static void RegisterStatusChangeNotificationsJob()
        {

            var jobKey = new JobKey("StatusChangeNotificationsJob");
            var job = sched.GetJobDetail(jobKey);

            if (job == null)
            {
                // define the job and tie it to our HelloJob class
                job = JobBuilder.Create<StatusChangeNotificationsJob>()
                    .WithIdentity(jobKey)
                    .Build();


                var triggerKey = new TriggerKey("StatusChangeNotificationsJob");

                // Trigger the job to run now, and daily at midnight
                ITrigger trigger = TriggerBuilder.Create()
                  .WithIdentity(triggerKey)
                  .StartNow()
                  .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(22, 0).WithMisfireHandlingInstructionFireAndProceed())
                  .Build();

                sched.ScheduleJob(job, trigger);

            }
        }

        private static void RegisterNotifications()
        {
            var jobKey = new JobKey("AutomaticNotificationsJob");
            var job = sched.GetJobDetail(jobKey);

            if (job == null)
            {
                // define the job and tie it to our HelloJob class
                job = JobBuilder.Create<AutomaticNotificationsJob>()
                    .WithIdentity(jobKey)
                    .Build();


                var triggerKey = new TriggerKey("AutomaticNotificationsJob");

                // Trigger the job to run now, and daily at midnight
                ITrigger trigger = TriggerBuilder.Create()
                  .WithIdentity(triggerKey)
                  .StartNow()
                  .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(0, 0).WithMisfireHandlingInstructionFireAndProceed())
                  .Build();

                sched.ScheduleJob(job, trigger);
            }

        }

        private static void RegisterApprovedTempJob()
        {
            var jobKey = new JobKey("ApprovedTempJob");
            var job = sched.GetJobDetail(jobKey);

            if (job == null)
            {
                // define the job and tie it to our HelloJob class
                job = JobBuilder.Create<TempApprovedJob>()
                    .WithIdentity(jobKey)
                    .Build();


                var triggerKey = new TriggerKey("ApprovedTempJob");

                // Trigger the job to run now, and daily at midnight
                ITrigger trigger = TriggerBuilder.Create()
                  .WithIdentity(triggerKey)
                  .StartNow()
                  .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(0, 0))
                  .Build();

                sched.ScheduleJob(job, trigger);
            }

        }

        private static void RegisterScanCfsFiles()
        {

            var jobKey = new JobKey("ScanCfsFiles");
            var job = sched.GetJobDetail(jobKey);

            if (job == null)
            {
                // define the job and tie it to our HelloJob class
                job = JobBuilder.Create<ScanCfsFilesJob>()
                    .WithIdentity(jobKey)
                    .Build();


                var triggerKey = new TriggerKey("ScanCfsFiles");

                // Trigger the job to run now, and daily at midnight
                ITrigger trigger = TriggerBuilder.Create()
                  .WithIdentity(triggerKey)
                  .StartNow()
                  .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(10, 0).WithMisfireHandlingInstructionFireAndProceed())
                  .Build();

                sched.ScheduleJob(job, trigger);

            }
        }

        private static void RegisterDisableUsers()
        {

            var jobKey = new JobKey("DisableUsers");
            var job = sched.GetJobDetail(jobKey);

            if (job == null)
            {
                // define the job and tie it to our HelloJob class
                job = JobBuilder.Create<DisableUsersJob>()
                    .WithIdentity(jobKey)
                    .Build();


                var triggerKey = new TriggerKey("DisableUsers");

                // Trigger the job to run now, and daily at midnight
                ITrigger trigger = TriggerBuilder.Create()
                  .WithIdentity(triggerKey)
                  .StartNow()
                  .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(0, 0).WithMisfireHandlingInstructionFireAndProceed())
                  .Build();

                sched.ScheduleJob(job, trigger);

            }
        }

        private static void RegisterAutomatedReports()
        {
            var jobKey = new JobKey("AutomatedReportsJob");
            var job = sched.GetJobDetail(jobKey);

            if (job == null)
            {
                // define the job and tie it to our HelloJob class
                job = JobBuilder.Create<AutomaticNotificationsJob>()
                    .WithIdentity(jobKey)
                    .Build();


                var triggerKey = new TriggerKey("AutomatedReportsJob");

                // Trigger the job to run now, and daily at midnight
                ITrigger trigger = TriggerBuilder.Create()
                  .WithIdentity(triggerKey)
                  .StartNow()
                  .WithSchedule(CronScheduleBuilder.MonthlyOnDayAndHourAndMinute(15, 0, 0).WithMisfireHandlingInstructionFireAndProceed())
                  .Build();

                sched.ScheduleJob(job, trigger);
            }

        }

        private static void RegisterHcMaximumAllowedCheck()
        {
            var jobKey = new JobKey("HcMaximumAllowedCheckJob");
            var job = sched.GetJobDetail(jobKey);

            if (job == null)
            {
                // define the job and tie it to our HelloJob class
                job = JobBuilder.Create<HcMaximumAllowedCheckJob>()
                    .WithIdentity(jobKey)
                    .Build();


                var triggerKey = new TriggerKey("HcMaximumAllowedCheckJob");

                // Trigger the job to run now, and daily at midnight
                ITrigger trigger = TriggerBuilder.Create()
                  .WithIdentity(triggerKey)
                  .StartNow()
                  .WithSchedule(CronScheduleBuilder.WeeklyOnDayAndHourAndMinute(DayOfWeek.Monday, 8, 0).WithMisfireHandlingInstructionFireAndProceed())
                  .Build();

                sched.ScheduleJob(job, trigger);
            }
        }

		public static void RegisterCfsDailyDigestJob()
		{

			var jobKey = new JobKey("CfsDailyDigestJob");
			var job = sched.GetJobDetail(jobKey);

			if (job == null)
			{
				// define the job and tie it to our HelloJob class
				job = JobBuilder.Create<CfsDailyDigestJob>()
					.WithIdentity(jobKey)
					.Build();


				var triggerKey = new TriggerKey("CfsDailyDigestJob");

                var hour = GlobalDbSettings.GetString(GlobalDbSettings.GlobalStringNames.CfsDailyDigestFireHour);
                int h = 0; // as default set it to 12AM
                if (!string.IsNullOrEmpty(hour) && !int.TryParse(hour, out h))
                {
                    h = 0;
                }

                // Trigger the job to run now, and daily at midnight
                ITrigger trigger = TriggerBuilder.Create()
				  .WithIdentity(triggerKey)
				  .StartNow()
				  .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(h, 0).WithMisfireHandlingInstructionFireAndProceed())
				  .Build();

				sched.ScheduleJob(job, trigger);

			}
		}

		private static void RegisterExportCfsClientRecordsJob()
		{

			var jobKey = new JobKey("ExportCfsClientRecordsJob");
			var job = sched.GetJobDetail(jobKey);

			if (job == null)
			{
				// define the job and tie it to our HelloJob class
				job = JobBuilder.Create<ExportCfsClientRecordsJob>()
					.WithIdentity(jobKey)
					.Build();


				var triggerKey = new TriggerKey("ExportCfsClientRecordsJob");

				// Trigger the job to run now, and daily at midnight
				ITrigger trigger = TriggerBuilder.Create()
				  .WithIdentity(triggerKey)
				  .StartNow()
				  .WithSchedule(CronScheduleBuilder.MonthlyOnDayAndHourAndMinute(1, 0, 0).WithMisfireHandlingInstructionFireAndProceed())
				  .Build();

				sched.ScheduleJob(job, trigger);

			}
		}
    }
}