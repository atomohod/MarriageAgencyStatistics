using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using MarriageAgencyStatistics.Core.DataProviders;
using MarriageAgencyStatistics.DataAccess.EF;

namespace MarriageAgencyStatistics.Scheduler.Web.Jobs
{
    public class TrackOnlineUsers : NoConcurrencyNoRetryJob
    {
        private readonly BrideForeverDataProvider _brideForeverDataProvider;
        private readonly BrideForeverDataContext _context;

        public TrackOnlineUsers(BrideForeverDataProvider brideForeverDataProvider, BrideForeverDataContext context)
        {
            _brideForeverDataProvider = brideForeverDataProvider;
            _context = context;
        }

        protected override async Task ExecuteAsync()
        {
            var idsOnline = await _brideForeverDataProvider.GetUserIdsOnline();
            var users = await _context.Users.ToListAsync();
            var usersOnline = idsOnline as string[] ?? idsOnline.ToArray();

            foreach (var user in users)
            {
                _context.UsersOnline.Add(new UserOnline
                {
                    User = user,
                    Id = Guid.NewGuid(),
                    IsOnline = usersOnline.Any(s => s == user.ID),
                    Online = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}