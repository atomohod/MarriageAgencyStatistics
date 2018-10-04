using System;
using System.Linq;
using System.Threading.Tasks;
using MarriageAgencyStatistics.Core.DataProviders;
using MarriageAgencyStatistics.DataAccess;
using MarriageAgencyStatistics.DataAccess.EF;

namespace MarriageAgencyStatistics.Jobs
{
    public class TrackOnlineUsers : NoConcurrencyNoRetryJob
    {
        private readonly BrideForeverDataProvider _brideForeverDataProvider;
        private readonly IDataContextProvider<BrideForeverDataContext> _contextProvider;

        public TrackOnlineUsers(BrideForeverDataProvider brideForeverDataProvider, IDataContextProvider<BrideForeverDataContext> contextProvider)
        {
            _brideForeverDataProvider = brideForeverDataProvider;
            _contextProvider = contextProvider;
        }

        protected override async Task ExecuteAsync()
        {
            var idsOnline = await _brideForeverDataProvider.GetUserIdsOnline();

            using (var context = _contextProvider.Create())
            {
                var users = context.Users.ToList();
                var usersOnline = idsOnline as string[] ?? idsOnline.ToArray();

                foreach (var user in users)
                {
                    context.UsersOnline.Add(new UserOnline
                    {
                        User = user,
                        Id = Guid.NewGuid(),
                        IsOnline = usersOnline.Any(s => s == user.ID),
                        Online = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                    });
                }

                await context.SaveChangesAsync();
            }
        }
    }
}