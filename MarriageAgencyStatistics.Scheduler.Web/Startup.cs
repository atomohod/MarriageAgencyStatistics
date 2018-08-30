using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using MarriageAgencyStatistics.Core.Clients;
using MarriageAgencyStatistics.Core.DataProviders;
using MarriageAgencyStatistics.Core.Services;
using MarriageAgencyStatistics.DataAccess.EF;
using MarriageAgencyStatistics.Jobs;
using MarriageAgencyStatistics.Scheduler.Web;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Owin;
using Swashbuckle.Application;
using GlobalConfiguration = Hangfire.GlobalConfiguration;

[assembly: OwinStartup(typeof(Startup))]

namespace MarriageAgencyStatistics.Scheduler.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
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

            //TODO add authorization
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                AuthorizationFilters = Enumerable.Empty<IAuthorizationFilter>()
            });
            app.UseHangfireServer();
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