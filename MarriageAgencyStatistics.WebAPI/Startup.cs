using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarriageAgencyStatistics.Core;
using MarriageAgencyStatistics.Core.Clients.BrideForever;
using MarriageAgencyStatistics.Core.DataProviders.BrideForever;
using MarriageAgencyStatistics.DataAccess;
using MarriageAgencyStatistics.WebAPI.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MarriageAgencyStatistics.WebAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddDbContext<BrideForeverDataContext>(options =>
                options.UseSqlServer(Configuration["ConnectionStrings:DefaultConnection"]));

            services.AddSingleton<Client>(provider => new BrideForeverClient(Configuration["BrideForever:Login"], Configuration["BrideForever:Password"]));
            services.AddSingleton<BrideForeverDataProvider>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
