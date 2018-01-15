using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Hangfire;
using MarriageAgencyStatistics.Core.DataProviders;
using MarriageAgencyStatistics.DataAccess.EF;

namespace MarriageAgencyStatistics.Scheduler.Web.Jobs
{
    public abstract class NoConcurrencyNoRetryJob
    {
        [DisableConcurrentExecution(timeoutInSeconds: 1000)]
        [AutomaticRetry(Attempts = 0)]
        public Task ExecuteJobAsync()
        {
            return ExecuteAsync();
        }

        protected abstract Task ExecuteAsync();
    }

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

            //bad - SOLID!!!
            var newUserIds = idsOnline.Where(id => !_context.Users.Select(user => user.ID).Contains(id));

            foreach (var newUserId in newUserIds)
            {
                _context.Users.Add(new User
                {
                    ID = newUserId,
                    Name = "NewUser"
                });
            }

            var users = await _context.Users.ToListAsync();

            foreach (var id in idsOnline)
            {
                _context.UsersOnline.Add(new UserOnline
                {
                    User = users.Find(user => user.ID == id),
                    Id = Guid.NewGuid(),
                    Online = DateTime.UtcNow
                });
            }
        }
    }
}