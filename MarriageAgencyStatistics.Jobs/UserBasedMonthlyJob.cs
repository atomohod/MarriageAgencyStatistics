using System;
using System.Linq;
using System.Threading.Tasks;
using MarriageAgencyStatistics.Common;
using MarriageAgencyStatistics.Core.DataProviders;
using MarriageAgencyStatistics.Core.Services;
using MarriageAgencyStatistics.DataAccess.EF;

namespace MarriageAgencyStatistics.Jobs
{
    public abstract class UserBasedMonthlyJob : NoConcurrencyNoRetryJob
    {
        private readonly BrideForeverService _brideForeverService;
        private readonly BrideForeverDataContext _context;

        protected UserBasedMonthlyJob(BrideForeverService brideForeverService, BrideForeverDataContext context)
        {
            _brideForeverService = brideForeverService;
            _context = context;
        }

        protected override async Task ExecuteAsync()
        {
            var users = await _brideForeverService.GetUsers();

            var fromDay = DateTime.UtcNow.GetFirstDayOfTheMonth();
            var toDay = DateTime.UtcNow.ToStartOfTheDay();

            var enumeratedUsers = users as User[] ?? users.ToArray();

            do
            {
                foreach (var user in enumeratedUsers)
                {
                    var contextUser = _context.Users.First(u => u.ID == user.ID);

                    await ApplyUserUpdates(contextUser, fromDay);
                }

                fromDay = fromDay + TimeSpan.FromDays(1);

                await _context.SaveChangesAsync();
            } while (fromDay < toDay);
        }

        protected  abstract Task ApplyUserUpdates(User user, DateTime currentDay);
    }
}