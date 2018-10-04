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
    public class CountChatsStatisticsMonthly : UserBasedMonthlyJob<BrideForeverDataContext>
    {
        private readonly BrideForeverService _brideForeverService;

        public CountChatsStatisticsMonthly(BrideForeverService brideForeverService, IDataContextProvider<BrideForeverDataContext> contextProvider)
            : base(brideForeverService, contextProvider)
        {
            _brideForeverService = brideForeverService;
        }

        protected override string OperationName => "Count chat statistics monthly";

        protected override async Task ApplyUserUpdates(BrideForeverDataContext context, User user, DateTime currentDay)
        {
            var statistic = await _brideForeverService.GetChatStatistics(currentDay, currentDay, user);

            var existingRecord = await context.UserChats.FirstOrDefaultAsync(item => item.User.ID == user.ID && item.Date == currentDay);

            context.UserChats.AddOrUpdate(u => u.Id, new UserChat
            {
                User = user,
                ChatInvatationsCount = statistic.ChatInvatationsCount,
                Date = currentDay,
                Id = existingRecord?.Id ?? Guid.NewGuid()
            });
        }
    }
}