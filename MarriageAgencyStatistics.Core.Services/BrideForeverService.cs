using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Remoting.Messaging;
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

        public async Task<IEnumerable<User>> GetSelectedUsers()
        {
            return await _dataContext.SelectedUsers.Select(user => user.User).ToListAsync();
        }

        public async Task<IEnumerable<User>> GetUsers()
        {
            return await _dataContext.Users.ToListAsync();
        }

        public async Task<IEnumerable<User>> GetUsers(string[] names)
        {
            return await _dataContext.Users.Where(user => names.Contains(user.Name)).ToListAsync();
        }

        public async Task<IEnumerable<Bonus>> GetUserBonuses(User[] users, DateTime date)
        {
            List<Bonus> bonuses = new List<Bonus>();

            foreach (var user in users)
            {
                bonuses.Add(await _dataProvider.GetUserBonus(user, date));
            }

            return bonuses;

        }

        public async Task<IEnumerable<Bonus>> GetUserBonusesHistory(User[] users, DateTime date)
        {
            List<Bonus> result = new List<Bonus>();

            foreach (var user in users)
            {
                var bonuses = (await _dataContext
                        .UserBonuses
                        .Where(e => e.User.ID == user.ID && e.Date == date)
                        .ToListAsync())
                    .SelectMany(e => e.Bonuses.ToObject<List<Bonus>>());

                result.AddRange(bonuses);
            }

            return result;
        }

        public async Task<IEnumerable<SentEmailStatistics>> GetCountOfSentEmailsHistory(User[] users, DateTime from, DateTime to)
        {
            var result = new List<SentEmailStatistics>();

            foreach (var user in users)
            {
                var emails = (await _dataContext
                    .UsersEmails
                    .Where(e => e.User.ID == user.ID && e.Date >= from && e.Date <= to)
                    .ToListAsync())
                    .SelectMany(e => e.Emails.ToObject<List<SentEmailStatistics>>());

                result.Add(new SentEmailStatistics
                {
                    User = user,
                    SentEmails = emails.Sum(statistics => statistics.SentEmails)
                });
            }

            return result;
        }

        public async Task<object> GetChatStatistics(DateTime from, DateTime to)
        {
            var result = await _dataProvider.GetChats(from, to, 3);

            var stats = result
                .GroupBy(item => item.Sender)
                .Select(g =>
                new
                {
                    Name = g.Key,
                    Messages = g.GroupBy(i => i.Message)
                    .Select(m => new
                    {
                        Message = m.Key,
                        Count = m.Count()
                    })
                });

            return null;
        }

        public async Task<IEnumerable<SentEmailStatistics>> GetCountOfSentEmails(User[] users, DateTime from, DateTime to)
        {
            var result = new List<SentEmailStatistics>();

            foreach (var user in users)
            {
                var emails = _dataProvider.GetSentEmailsData(user, from, to);

                result.Add(new SentEmailStatistics
                {
                    User = user,
                    SentEmails = (await emails).SentEmails.Count()
                });
            }

            return result;
        }

        public async Task<IEnumerable<OnlineStatistics>> GetOnlineStatistic(User[] user, DateTime day)
        {
            var unixDayStart = ((DateTimeOffset)day.ToStartOfTheDay()).ToUnixTimeSeconds();
            var unixDayEnd = ((DateTimeOffset)day.ToEndOfTheDay()).ToUnixTimeSeconds();

            var userIds = user.Select(u => u.ID);

            var usersOnline =
                await _dataContext
                .UsersOnline
                .Where(s => s.Online >= unixDayStart && s.Online <= unixDayEnd && userIds.Contains(s.User.ID))
                .GroupBy(online => online.User)
                .ToListAsync();

            var result = new List<OnlineStatistics>();

            foreach (var userGroup in usersOnline)
            {
                var ordered = userGroup.OrderBy(online => online.Online).ToList();
                long totalSeconds = 0;
                long prevTimestamp = ordered.First().Online;

                foreach (var userOnline in ordered)
                {
                    if (userOnline.IsOnline)
                    {
                        totalSeconds += userOnline.Online - prevTimestamp;
                    }
                    prevTimestamp = userOnline.Online;
                }

                result.Add(new OnlineStatistics
                {
                    User = userGroup.Key,
                    PercentageOnline = (double)userGroup.Count(online => online.IsOnline) / (double)userGroup.Count(),
                    TotalMinutesOnline = (int)totalSeconds / 60
                });
            }

            return result;
        }

        public async Task SetSelectedUsers(string[] userNames)
        {
            var users = _dataContext.SelectedUsers.ToList();
            _dataContext.SelectedUsers.RemoveRange(users);

            _dataContext.SelectedUsers.AddRange(
                _dataContext.Users.Where(user => userNames.Contains(user.Name))
                    .ToList()
                    .Select(user =>
                        new SelectedUser
                        {
                            //TODO create this in DAL
                            Id = Guid.NewGuid(),
                            User = user
                        }));

            await _dataContext.SaveChangesAsync();
        }
    }
}
