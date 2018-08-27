using Hangfire;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(MarriageAgencyStatistics.WindowsService.Startup))]
namespace MarriageAgencyStatistics.WindowsService
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseHangfireDashboard();
        }
    }
}