using System;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using MarriageAgencyStatistics.Common;
using MarriageAgencyStatistics.Core.Services;
using MarriageAgencyStatistics.DataAccess.EF;

namespace MarriageAgencyStatistics.Scheduler.Web.Jobs
{
    public class CountChatsStatisticsDaily : NoConcurrencyNoRetryJob
    {
        private readonly BrideForeverService _brideForeverService;
        private readonly BrideForeverDataContext _context;

        public CountChatsStatisticsDaily(BrideForeverService brideForeverService, BrideForeverDataContext context)
        {
            _brideForeverService = brideForeverService;
            _context = context;
        }

        protected override async Task ExecuteAsync()
        {
            var today = (DateTime.UtcNow - TimeSpan.FromDays(1)).ToStartOfTheDay();

            var statistics = await _brideForeverService.GetChatStatistics(today, today);

            foreach (var statistic in statistics)
            {
                _context.UserChats.AddOrUpdate(new UserChat
                {
                    User = _context.Users.First( user => user.Name == statistic.User.Name),
                    ChatInvatationsCount = statistic.ChatInvatationsCount,
                    Date = today,
                    Id = Guid.NewGuid()
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}