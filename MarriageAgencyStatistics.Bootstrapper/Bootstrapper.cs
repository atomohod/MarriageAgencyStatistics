using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Hangfire;
using MarriageAgencyStatistics.Scheduler.Web.Jobs;

namespace MarriageAgencyStatistics.Bootstrapper
{
    public class Bootstrapper
    {
        public void Load(ContainerBuilder builder)
        {
            builder.RegisterModule<MarriageAgencyStaticticsModule>();
            RegisterJobs(builder);
        }

        private static void RegisterJobs(ContainerBuilder builder)
        {
            builder
                .RegisterType<TrackOnlineUsers>()
                .AsSelf()
                .InstancePerDependency();

            builder
                .RegisterType<UpdateUserList>()
                .AsSelf()
                .InstancePerDependency();

            builder
                .RegisterType<CountSentEmailsDaily>()
                .AsSelf()
                .InstancePerDependency();

            builder
                .RegisterType<CountSentEmailsMonthly>()
                .AsSelf()
                .InstancePerDependency();

            builder
                .RegisterType<CountUserBonusesDaily>()
                .AsSelf()
                .InstancePerDependency();

            builder
                .RegisterType<CountUserBonusesMonthly>()
                .AsSelf()
                .InstancePerDependency();

            builder
                .RegisterType<CountChatsStatisticsMonthly>()
                .AsSelf()
                .InstancePerDependency();

            builder
                .RegisterType<CountChatsStatisticsDaily>()
                .AsSelf()
                .InstancePerDependency();
        }
    }


}
