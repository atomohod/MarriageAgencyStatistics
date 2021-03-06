﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Autofac;
using Autofac.Integration.WebApi;
using MarriageAgencyStatistics.Core.Clients;
using MarriageAgencyStatistics.Core.DataProviders;
using MarriageAgencyStatistics.Core.Services;
using MarriageAgencyStatistics.DataAccess;
using MarriageAgencyStatistics.DataAccess.EF;
using MarriageAgencyStatistics.WebAPI;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Owin;
using Swashbuckle.Application;

[assembly: OwinStartup(typeof(Startup))]

namespace MarriageAgencyStatistics.WebAPI
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var builder = new ContainerBuilder();
            var config = new HttpConfiguration();

            RegisterServices(builder);
           
            var container = builder.Build();

            InitializeConfiguration(config, container);
            RegisterOwinPipeline(app, container, config);

            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/octet-stream"));
        }

        //TODO: delete if no errors
        private static void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            builder
                .Register(context => new BrideForeverClient("viktorya.tory1", "QZW17111992QZW"))
                .AsSelf()
                .InstancePerRequest();

            builder
                .Register(context => new BrideForeverDataProvider(context.Resolve<BrideForeverClient>()))
                .AsSelf()
                .InstancePerRequest();

            builder
                .RegisterType<BrideForeverDataContextProvider>()
                .As<IDataContextProvider<BrideForeverDataContext>>()
                .InstancePerRequest();

            builder.RegisterType<BrideForeverService>()
                .AsSelf()
                .InstancePerRequest();
        }

        private static void InitializeConfiguration(HttpConfiguration config, IContainer container)
        {
            WebApiConfig.Register(config);
            config
                .EnableSwagger(c =>
                {
                    c.SingleApiVersion("v1", "Marriage Statistics Agency API");
                })
                .EnableSwaggerUi();

            //config.Filters.Add(new AuthorizeAttribute());
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
        }

        private static void RegisterOwinPipeline(IAppBuilder app, ILifetimeScope container, HttpConfiguration config)
        {
            app.UseAutofacMiddleware(container);
            app.UseCors(CorsOptions.AllowAll);

            app.UseAutofacWebApi(config);
            app.UseWebApi(config);
        }
    }
}