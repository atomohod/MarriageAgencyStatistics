using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Threading.Tasks;
using MarriageAgencyStatistics.Core.DataProviders;
using MarriageAgencyStatistics.Core.Services;
using MarriageAgencyStatistics.DataAccess;
using MarriageAgencyStatistics.DataAccess.EF;

namespace MarriageAgencyStatistics.Jobs
{
    public class CountChatsStatisticsDaily : UserBasedDailyJob<BrideForeverDataContext>
    {
        private readonly BrideForeverService _brideForeverService;

        public CountChatsStatisticsDaily(BrideForeverService brideForeverService, IDataContextProvider<BrideForeverDataContext> contextProvider) 
            : base(brideForeverService, contextProvider)
        {
            _brideForeverService = brideForeverService;
        }

        protected override async Task ApplyUserUpdatesAsync(BrideForeverDataContext context, User user, DateTime yesterday)
        {
            var statistic = await _brideForeverService.GetChatStatistics(yesterday, yesterday, user);

            var existingRecord = await context.UserChats.FirstOrDefaultAsync(item => item.User.ID == user.ID && item.Date == yesterday);

            context.UserChats.AddOrUpdate(u => u.Id, new UserChat
            {
                User = user,
                ChatInvatationsCount = statistic.ChatInvatationsCount,
                Date = yesterday,
                Id = existingRecord?.Id ?? Guid.NewGuid()
            });
        }
    }
}