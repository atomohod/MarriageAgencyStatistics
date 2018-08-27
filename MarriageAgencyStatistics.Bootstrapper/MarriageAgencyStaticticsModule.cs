using Autofac;
using MarriageAgencyStatistics.Core.Clients;
using MarriageAgencyStatistics.Core.DataProviders;
using MarriageAgencyStatistics.Core.Services;
using MarriageAgencyStatistics.DataAccess.EF;

namespace MarriageAgencyStatistics.Bootstrapper
{
    public class MarriageAgencyStaticticsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register(context => new BrideForeverClient("viktorya.tory1", "QZW17111992QZW"))
                .AsSelf()
                .InstancePerDependency();

            builder
                .Register(context => new BrideForeverDataProvider(context.Resolve<BrideForeverClient>()))
                .AsSelf()
                .InstancePerDependency();

            builder
                .Register(context => new BrideForeverDataContext())
                .AsSelf()
                .InstancePerDependency();

            builder
                .RegisterType<BrideForeverService>()
                .AsSelf()
                .InstancePerDependency();
        }
    }
}