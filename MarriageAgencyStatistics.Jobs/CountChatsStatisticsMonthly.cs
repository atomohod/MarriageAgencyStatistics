using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Threading.Tasks;
using MarriageAgencyStatistics.Core.DataProviders;
using MarriageAgencyStatistics.Core.Services;
using MarriageAgencyStatistics.DataAccess.EF;

namespace MarriageAgencyStatistics.Jobs
{
    public class CountChatsStatisticsMonthly : UserBasedMonthlyJob
    {
        private readonly BrideForeverService _brideForeverService;
        private readonly BrideForeverDataContext _context;

        public CountChatsStatisticsMonthly(BrideForeverService brideForeverService, BrideForeverDataContext context)
            : base(brideForeverService, context)
        {
            _brideForeverService = brideForeverService;
            _context = context;
        }

        protected override async Task ApplyUserUpdates(User user, DateTime currentDay)
        {
            var statistic = await _brideForeverService.GetChatStatistics(currentDay, currentDay, user);

            var existingRecord = await _context.UserChats.FirstOrDefaultAsync(item => item.User.ID == user.ID && item.Date == currentDay);

            _context.UserChats.Add(new UserChat
            {
                User = user,
                ChatInvatationsCount = statistic.ChatInvatationsCount,
                Date = currentDay,
                Id = existingRecord?.Id ?? Guid.NewGuid()
            });
        }
    }
}