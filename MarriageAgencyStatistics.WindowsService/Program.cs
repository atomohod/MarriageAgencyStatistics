using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Hangfire;
using Hangfire.SqlServer;
using MarriageAgencyStatistics.Bootstrapper;
using MarriageAgencyStatistics.Scheduler.Web.Jobs;
using Microsoft.Owin;
using Microsoft.Owin.Hosting;

namespace MarriageAgencyStatistics.WindowsService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            var builder = new ContainerBuilder();

            var bootstrapper = new Bootstrapper.Bootstrapper();
            bootstrapper.Load(builder);
            
            GlobalConfiguration.Configuration.UseSqlServerStorage("auxiliaryDb", new SqlServerStorageOptions
                {
                    //Our jobs are compile-time known, so the interval can be this long
                    QueuePollInterval = TimeSpan.FromMinutes(10),
                    JobExpirationCheckInterval = TimeSpan.FromHours(3)
                })
                .UseAutofacActivator(builder.Build());

            AddJobs();

            StartOptions options = new StartOptions();
            options.Urls.Add("http://localhost:9095");
            options.Urls.Add("http://127.0.0.1:9095");
            options.Urls.Add($"http://{Environment.MachineName}:9095");

            WebApp.Start<Startup>(options);

            var servicesToRun = new ServiceBase[]
            {
                new MarriageAgencyStatistics()
            };

            ServiceBase.Run(servicesToRun);
        }

        private static void AddJobs()
        {
            RecurringJob.AddOrUpdate<TrackOnlineUsers>("Track Users Online", j => j.ExecuteJobAsync(), Cron.MinuteInterval(10));
            RecurringJob.AddOrUpdate<UpdateUserList>("Update User List", j => j.ExecuteJobAsync(), Cron.Daily);
            RecurringJob.AddOrUpdate<CountSentEmailsDaily>("Count Emails Daily", j => j.ExecuteJobAsync(), Cron.Daily);
            RecurringJob.AddOrUpdate<CountSentEmailsMonthly>("Count Emails Monthly", j => j.ExecuteJobAsync(), Cron.Monthly);
            RecurringJob.AddOrUpdate<CountUserBonusesDaily>("Count Bonuses Daily", j => j.ExecuteJobAsync(), Cron.Daily);
            RecurringJob.AddOrUpdate<CountUserBonusesMonthly>("Count Bonuses Monthly", j => j.ExecuteJobAsync(), Cron.Monthly);
            RecurringJob.AddOrUpdate<CountChatsStatisticsMonthly>("Count Chats Monthly", j => j.ExecuteJobAsync(), Cron.Monthly);
            RecurringJob.AddOrUpdate<CountChatsStatisticsDaily>("Count Chats Daily", j => j.ExecuteJobAsync(), Cron.Daily);

            RecurringJob.RemoveIfExists("Count Emails Monthly");
            RecurringJob.RemoveIfExists("Count Chats Monthly");
        }
    }
}
