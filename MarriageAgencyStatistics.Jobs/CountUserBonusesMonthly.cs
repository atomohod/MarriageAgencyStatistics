using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Threading.Tasks;
using MarriageAgencyStatistics.Common;
using MarriageAgencyStatistics.Core.DataProviders;
using MarriageAgencyStatistics.Core.Services;
using MarriageAgencyStatistics.DataAccess.EF;

namespace MarriageAgencyStatistics.Jobs
{
    public class CountUserBonusesMonthly : UserBasedMonthlyJob
    {
        private readonly BrideForeverService _brideForeverService;
        private readonly BrideForeverDataContext _context;

        public CountUserBonusesMonthly(BrideForeverService brideForeverService, BrideForeverDataContext context)
            : base(brideForeverService, context)
        {
            _brideForeverService = brideForeverService;
            _context = context;
        }

        protected override async Task ApplyUserUpdates(User user, DateTime currentDay)
        {
            var bonuses = await _brideForeverService.GetUserBonuses(new[] { user }, currentDay);

            var existingRecord = await _context.UserBonuses.FirstOrDefaultAsync(b =>
                b.User.ID == user.ID && b.Date == currentDay);

            _context.UserBonuses.AddOrUpdate(u => u.Id, new UserBonuses
            {
                Id = existingRecord?.Id ?? Guid.NewGuid(),
                User = user,
                Date = currentDay,
                Bonuses = bonuses.ToBytes()
            });
        }
    }
}