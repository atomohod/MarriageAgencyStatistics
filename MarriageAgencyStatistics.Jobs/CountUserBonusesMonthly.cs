using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Threading.Tasks;
using MarriageAgencyStatistics.Common;
using MarriageAgencyStatistics.Core.DataProviders;
using MarriageAgencyStatistics.Core.Services;
using MarriageAgencyStatistics.DataAccess;
using MarriageAgencyStatistics.DataAccess.EF;

namespace MarriageAgencyStatistics.Jobs
{
    public class CountUserBonusesMonthly : UserBasedMonthlyJob<BrideForeverDataContext>
    {
        private readonly BrideForeverService _brideForeverService;

        public CountUserBonusesMonthly(BrideForeverService brideForeverService, IDataContextProvider<BrideForeverDataContext> contextProvider)
            : base(brideForeverService, contextProvider)
        {
            _brideForeverService = brideForeverService;
        }

        protected override string OperationName => "Count user bonuses monthly";

        protected override async Task ApplyUserUpdates(BrideForeverDataContext context, User user, DateTime currentDay)
        {
            var bonuses = await _brideForeverService.GetUserBonuses(new[] { user }, currentDay);

            var existingRecord = await context.UserBonuses.FirstOrDefaultAsync(b =>
                b.User.ID == user.ID && b.Date == currentDay);

            context.UserBonuses.AddOrUpdate(u => u.Id, new UserBonuses
            {
                Id = existingRecord?.Id ?? Guid.NewGuid(),
                User = user,
                Date = currentDay,
                Bonuses = bonuses.ToBytes()
            });
        }
    }
}