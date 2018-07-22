using System;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using MarriageAgencyStatistics.Common;
using MarriageAgencyStatistics.Core.Services;
using MarriageAgencyStatistics.DataAccess.EF;

namespace MarriageAgencyStatistics.Scheduler.Web.Jobs
{
    public class CountChatsStatisticsMonthly : NoConcurrencyNoRetryJob
    {
        private readonly BrideForeverService _brideForeverService;
        private readonly BrideForeverDataContext _context;

        public CountChatsStatisticsMonthly(BrideForeverService brideForeverService, BrideForeverDataContext context)
            
        {
            _brideForeverService = brideForeverService;
            _context = context;
        }

        protected override async Task ExecuteAsync()
        {
            var fromDay = DateTime.UtcNow.GetFirstDayOfTheMonth();
            var toDay = DateTime.UtcNow.ToStartOfTheDay();

            do
            {
                var statistics = await _brideForeverService.GetChatStatistics(fromDay, toDay);

                foreach (var statistic in statistics)
                {
                    _context.UserChats.AddOrUpdate(new UserChat
                    {
                        User = _context.Users.First(user => user.Name == statistic.User.Name),
                        ChatInvatationsCount = statistic.ChatInvatationsCount,
                        Date = fromDay,
                        Id = Guid.NewGuid()
                    });
                }

                fromDay = fromDay + TimeSpan.FromDays(1);
                await _context.SaveChangesAsync();

            } while (fromDay < toDay);
        }
    }
}