using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using Hangfire;
using Hangfire.SqlServer;
using MarriageAgencyStatistics.Core.Clients;
using MarriageAgencyStatistics.Core.DataProviders;
using MarriageAgencyStatistics.DataAccess.EF;
using MarriageAgencyStatistics.Scheduler.Web;
using MarriageAgencyStatistics.Scheduler.Web.Jobs;
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
            //var config = new HttpConfiguration();

            RegisterServices(builder);
            RegisterJobs(builder);

            GlobalConfiguration.Configuration.UseSqlServerStorage("auxiliaryDb", new SqlServerStorageOptions
                {
                    //Our jobs are compile-time known, so the interval can be this long
                    QueuePollInterval = TimeSpan.FromMinutes(10),
                    JobExpirationCheckInterval = TimeSpan.FromHours(3)
                })
                .UseAutofacActivator(builder.Build());

            AddJobs();

            //InitializeConfiguration(config, container);
            //RegisterOwinPipeline(app, container, config);

            app.UseHangfireDashboard("/hangfire");
            app.UseHangfireServer();
        }

        private static void RegisterServices(ContainerBuilder builder)
        {
            builder
                .Register(context => new BrideForeverClient("viktorya.tory1", "QZW17111992QZW"))
                .AsSelf()
                .SingleInstance();

            builder
                .Register(context => new BrideForeverDataProvider(context.Resolve<BrideForeverClient>()))
                .AsSelf()
                .SingleInstance();
            
            builder
                .Register(context => new BrideForeverDataContext())
                .AsSelf()
                .SingleInstance();
        }

        private static void AddJobs()
        {
            RecurringJob.AddOrUpdate<TrackOnlineUsers>("Track Users Online", j => j.ExecuteJobAsync(), Cron.MinuteInterval(10));
        }

        private static void RegisterJobs(ContainerBuilder builder)
        {
            builder
                .RegisterType<TrackOnlineUsers>()
                .AsSelf()
                .InstancePerDependency();
        }

        //private static void InitializeConfiguration(HttpConfiguration config, IContainer container)
            //{
            //    WebApiConfig.Register(config);
            //    config
            //        .EnableSwagger(c =>
            //        {
            //            c.SingleApiVersion("v1", "Marriage Statistics Agency Schedule");
            //        })
            //        .EnableSwaggerUi();

            //    //config.Filters.Add(new AuthorizeAttribute());
            //    config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
            //}

            //private static void RegisterOwinPipeline(IAppBuilder app, ILifetimeScope container, HttpConfiguration config)
            //{
            //    app.UseAutofacMiddleware(container);
            //    app.UseCors(CorsOptions.AllowAll);

            //    app.UseAutofacWebApi(config);
            //    app.UseWebApi(config);
            //}
        }
}