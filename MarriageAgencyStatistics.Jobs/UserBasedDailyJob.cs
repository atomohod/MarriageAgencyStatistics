using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using MarriageAgencyStatistics.Common;
using MarriageAgencyStatistics.Core.DataProviders;
using MarriageAgencyStatistics.Core.Services;
using MarriageAgencyStatistics.DataAccess;
using MarriageAgencyStatistics.DataAccess.EF;

namespace MarriageAgencyStatistics.Jobs
{
    public abstract class UserBasedDailyJob<T> : NoConcurrencyNoRetryJob where T : DbContext, IContext, new()
    {
        private readonly BrideForeverService _brideForeverService;
        private readonly IDataContextProvider<T> _contextProvider;

        protected UserBasedDailyJob(BrideForeverService brideForeverService, IDataContextProvider<T> contextProvider)
        {
            _brideForeverService = brideForeverService;
            _contextProvider = contextProvider;
        }

        protected override async Task ExecuteAsync()
        {
            var users = await _brideForeverService.GetUsers(UserMode.Active, UserMode.Silent);

            var yesterday = (DateTime.UtcNow - TimeSpan.FromDays(1)).ToStartOfTheDay();

            var enumeratedUsers = users as User[] ?? users.ToArray();

            using (var context = _contextProvider.Create())
            {
                foreach (var user in enumeratedUsers)
                {
                    var contextUser = context.Set<User>().First(u => u.ID == user.ID);
                    await ApplyUserUpdatesAsync(context, contextUser, yesterday);
                }

                await context.SaveChangesAsync();
            }
        }

        protected abstract Task ApplyUserUpdatesAsync(T context, User user, DateTime yesterday);
    }
}