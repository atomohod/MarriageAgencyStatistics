using System;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MarriageAgencyStatistics.Common;
using MarriageAgencyStatistics.Core.DataProviders;
using MarriageAgencyStatistics.Core.Services;
using MarriageAgencyStatistics.DataAccess;
using MarriageAgencyStatistics.DataAccess.EF;

namespace MarriageAgencyStatistics.Jobs
{
    public abstract class UserBasedMonthlyJob<T> : NoConcurrencyNoRetryJob where T : DbContext, IContext, new()
    {
        private readonly BrideForeverService _brideForeverService;
        private readonly IDataContextProvider<T> _contextProvider;

        protected UserBasedMonthlyJob(BrideForeverService brideForeverService, IDataContextProvider<T> contextProvider)
        {
            _brideForeverService = brideForeverService;
            _contextProvider = contextProvider;
        }

        public async Task ExecuteAsync(DateTime from, DateTime to)
        {
            var users = await _brideForeverService.GetUsers(UserMode.Active, UserMode.Silent);
            var enumeratedUsers = users as User[] ?? users.ToArray();

            var dates = from.RangeTo(to).ToList();
            
            Task.WaitAll(dates.Select(async date =>
            {
                using (var context = _contextProvider.Create())
                {
                    foreach (var user in enumeratedUsers)
                    {
                        var contextUser = context.Set<User>().First(u => u.ID == user.ID);

                        Stopwatch stopwatch = Stopwatch.StartNew();

                        await ApplyUserUpdates(context, contextUser, date);

                        stopwatch.Stop();

                        Console.WriteLine($"Operation at {date.ToShortDateString()} for user {user.Name} completed in {stopwatch.Elapsed.Seconds} seconds.");
                        
                        await context.SaveChangesAsync();
                    }
                }
            }).ToArray());
        }

        protected override async Task ExecuteAsync()
        {
            var now = DateTime.UtcNow;

            await ExecuteAsync(now.GetFirstDayOfTheMonth(), now.ToStartOfTheDay());
        }

        protected abstract string OperationName { get; }

        protected abstract Task ApplyUserUpdates(T context, User user, DateTime currentDay);
    }
}