using Autofac;
using MarriageAgencyStatistics.Core.Clients;
using MarriageAgencyStatistics.Core.DataProviders;
using MarriageAgencyStatistics.Core.Services;
using MarriageAgencyStatistics.DataAccess;
using MarriageAgencyStatistics.DataAccess.EF;

namespace MarriageAgencyStatistics.Bootstrapper
{
    public class MarriageAgencyStaticticsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register(c => new BrideForeverClient("viktorya.tory1", "QZW17111992QZW"))
                .AsSelf()
                .InstancePerDependency();

            builder
                .Register(c => new BrideForeverDataProvider(c.Resolve<BrideForeverClient>()))
                .AsSelf()
                .InstancePerDependency();

            builder
                .RegisterType<BrideForeverDataContextProvider>()
                .As<IDataContextProvider<BrideForeverDataContext>>()
                .InstancePerDependency();

            builder
                .RegisterType<BrideForeverService>()
                .AsSelf()
                .InstancePerDependency();
        }
    }
}