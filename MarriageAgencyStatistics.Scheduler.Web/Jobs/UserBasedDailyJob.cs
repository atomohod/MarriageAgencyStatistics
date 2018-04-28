using System;
using System.Linq;
using System.Threading.Tasks;
using MarriageAgencyStatistics.Common;
using MarriageAgencyStatistics.Core.DataProviders;
using MarriageAgencyStatistics.Core.Services;
using MarriageAgencyStatistics.DataAccess.EF;

namespace MarriageAgencyStatistics.Scheduler.Web.Jobs
{
    public abstract class UserBasedDailyJob : NoConcurrencyNoRetryJob
    {
        private readonly BrideForeverService _brideForeverService;
        private readonly BrideForeverDataContext _context;

        protected UserBasedDailyJob(BrideForeverService brideForeverService, BrideForeverDataContext context)
        {
            _brideForeverService = brideForeverService;
            _context = context;
        }

        protected override async Task ExecuteAsync()
        {
            var users = await _brideForeverService.GetUsers();

            var today = (DateTime.UtcNow - TimeSpan.FromDays(1)).ToStartOfTheDay();

            var enumeratedUsers = users as User[] ?? users.ToArray();

            foreach (var user in enumeratedUsers)
            {
                var contextUser = _context.Users.First(u => u.ID == user.ID);
                await ApplyUserUpdates(contextUser, today);
            }

            await _context.SaveChangesAsync();
        }

        protected abstract Task ApplyUserUpdates(User user, DateTime today);
    }
}