using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using MarriageAgencyStatistics.Common;
using MarriageAgencyStatistics.Core.DataProviders;
using MarriageAgencyStatistics.Core.Services;
using MarriageAgencyStatistics.DataAccess.EF;

namespace MarriageAgencyStatistics.Scheduler.Web.Jobs
{
    public class CountUserBonusesDaily : UserBasedDailyJob
    {
        private readonly BrideForeverService _brideForeverService;
        private readonly BrideForeverDataContext _context;

        public CountUserBonusesDaily(BrideForeverService brideForeverService, BrideForeverDataContext context) 
            : base(brideForeverService, context)
        {
            _brideForeverService = brideForeverService;
            _context = context;
        }
        
        protected override async Task ApplyUserUpdates(User user, DateTime today)
        {
            var bonuses =
                await _brideForeverService.GetUserBonuses(new[] { user }, today);

            var existingRecord = await _context.UserBonuses.FirstOrDefaultAsync(userBonuses =>
                userBonuses.User.ID == user.ID && userBonuses.Date == today);

            _context.UserBonuses.AddOrUpdate(new UserBonuses
            {
                Id = existingRecord?.Id ?? Guid.NewGuid(),
                User = user,
                Date = today,
                Bonuses = bonuses.ToBytes()
            });
        }
    }
}