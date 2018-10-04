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
    public class CountUserBonusesDaily : UserBasedDailyJob<BrideForeverDataContext>
    {
        private readonly BrideForeverService _brideForeverService;

        public CountUserBonusesDaily(BrideForeverService brideForeverService, IDataContextProvider<BrideForeverDataContext> contextProvider) 
            : base(brideForeverService, contextProvider)
        {
            _brideForeverService = brideForeverService;
        }
        
        protected override async Task ApplyUserUpdatesAsync(BrideForeverDataContext context, User user, DateTime yesterday)
        {
            var bonuses =
                await _brideForeverService.GetUserBonuses(new[] { user }, yesterday);

            var existingRecord = await context.UserBonuses.FirstOrDefaultAsync(userBonuses =>
                userBonuses.User.ID == user.ID && userBonuses.Date == yesterday);

            context.UserBonuses.AddOrUpdate(u => u.Id, new UserBonuses
            {
                Id = existingRecord?.Id ?? Guid.NewGuid(),
                User = user,
                Date = yesterday,
                Bonuses = bonuses.ToBytes()
            });
        }
    }
}