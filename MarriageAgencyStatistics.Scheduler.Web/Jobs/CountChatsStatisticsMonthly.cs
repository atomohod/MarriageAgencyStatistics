using System;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using MarriageAgencyStatistics.Common;
using MarriageAgencyStatistics.Core.DataProviders;
using MarriageAgencyStatistics.Core.Services;
using MarriageAgencyStatistics.DataAccess.EF;

namespace MarriageAgencyStatistics.Scheduler.Web.Jobs
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

            _context.UserChats.AddOrUpdate(new UserChat
            {
                User = user,
                ChatInvatationsCount = statistic.ChatInvatationsCount,
                Date = currentDay,
                Id = Guid.NewGuid()
            });
        }
    }
}