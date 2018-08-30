using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Threading.Tasks;
using MarriageAgencyStatistics.Core.DataProviders;
using MarriageAgencyStatistics.Core.Services;
using MarriageAgencyStatistics.DataAccess.EF;

namespace MarriageAgencyStatistics.Jobs
{
    public class CountChatsStatisticsDaily : UserBasedDailyJob
    {
        private readonly BrideForeverService _brideForeverService;
        private readonly BrideForeverDataContext _context;

        public CountChatsStatisticsDaily(BrideForeverService brideForeverService, BrideForeverDataContext context) : base(brideForeverService, context)
        {
            _brideForeverService = brideForeverService;
            _context = context;
        }

        protected override async Task ApplyUserUpdatesAsync(User user, DateTime yesterday)
        {
            var statistic = await _brideForeverService.GetChatStatistics(yesterday, yesterday, user);

            var existingRecord = await _context.UserChats.FirstOrDefaultAsync(item => item.User.ID == user.ID && item.Date == yesterday);

            _context.UserChats.Add(new UserChat
            {
                User = user,
                ChatInvatationsCount = statistic.ChatInvatationsCount,
                Date = yesterday,
                Id = existingRecord?.Id ?? Guid.NewGuid()
            });
        }
    }
}