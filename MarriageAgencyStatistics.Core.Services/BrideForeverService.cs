using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarriageAgencyStatistics.Common;
using MarriageAgencyStatistics.Core.DataProviders;
using MarriageAgencyStatistics.DataAccess.EF;

namespace MarriageAgencyStatistics.Core.Services
{
    //TODO remove reference to EF and use IEnumerable as sources
    public class BrideForeverService
    {
        private readonly BrideForeverDataProvider _dataProvider;
        private readonly BrideForeverDataContext _dataContext;

        public BrideForeverService(BrideForeverDataProvider dataProvider, BrideForeverDataContext dataContext)
        {
            _dataProvider = dataProvider;
            _dataContext = dataContext;
        }

        public async Task<IEnumerable<User>> GetUsers()
        {
            return await _dataContext.Users.ToListAsync();
        }

        public async Task<IEnumerable<SentEmailStatistics>> GetCountOfSentEmails(DateTime from, DateTime to)
        {
            var users = await GetUsers();

            var result = new List<SentEmailStatistics>();

            foreach (var user in users)
            {
                var emails = _dataProvider.GetSentEmailsData(user, from, to);
                result.Add(new SentEmailStatistics
                {
                    User = user,
                    SentEmails = (await emails).Count()
                });
            }

            return result;
        }

        public async Task<IEnumerable<OnlineStatistics>> GetOnlineStatistic(DateTime day)
        {
            var unixDayStart = ((DateTimeOffset) day.ToStartOfTheDay()).ToUnixTimeSeconds();
            var unixDayEnd = ((DateTimeOffset)day.ToEndOfTheDay()).ToUnixTimeSeconds();

            var usersOnline = 
                await _dataContext
                .UsersOnline
                .Where(s => s.Online >= unixDayStart && s.Online <= unixDayEnd)
                .GroupBy(online => online.User)
                .ToListAsync();

            var result = new List<OnlineStatistics>();

            foreach (var gr in usersOnline)
            {
                result.Add(new OnlineStatistics
                {
                    User = gr.Key,
                    Online = (double)gr.Count(online => online.IsOnline) / (double)gr.Count()
                });
            }

            return result;
        }
    }
}
