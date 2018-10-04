using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using MarriageAgencyStatistics.Common;
using MarriageAgencyStatistics.Core.DataProviders;
using MarriageAgencyStatistics.DataAccess;
using MarriageAgencyStatistics.DataAccess.EF;
using Newtonsoft.Json;

namespace MarriageAgencyStatistics.Core.Services
{
    //TODO remove reference to EF and use IEnumerable as sources
    public class BrideForeverService
    {
        private readonly BrideForeverDataProvider _dataProvider;
        private readonly IDataContextProvider<BrideForeverDataContext> _dataContextProvider;

        public BrideForeverService(BrideForeverDataProvider dataProvider, IDataContextProvider<BrideForeverDataContext> dataContextProviderProvider)
        {
            _dataProvider = dataProvider;
            _dataContextProvider = dataContextProviderProvider;
        }

        public async Task<IEnumerable<User>> GetSelectedUsers()
        {
            using (var context = _dataContextProvider.Create())
            {
                return await context.SelectedUsers.Select(user => user.User).ToListAsync();
            }
        }

        public async Task<IEnumerable<User>> GetUsers()
        {
            using (var context = _dataContextProvider.Create())
            {
                return await context.Users.ToListAsync();
            }
        }

        public async Task<IEnumerable<User>> GetUsers(string[] names)
        {
            using (var context = _dataContextProvider.Create())
            {
                return await context.Users.Where(user => names.Contains(user.Name)).ToListAsync();
            }
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

            using (var context = _dataContextProvider.Create())
            {

                foreach (var user in users)
                {
                    var bonuses = (await context
                            .UserBonuses
                            .Where(e => e.User.ID == user.ID && e.Date == date)
                            .ToListAsync())
                        .SelectMany(e => e.Bonuses.ToObject<List<Bonus>>());

                    result.AddRange(bonuses);
                }

                return result;
            }
        }

        public async Task<IEnumerable<SentEmailStatistics>> GetCountOfSentEmailsHistory(User[] users, DateTime from, DateTime to)
        {
            var result = new List<SentEmailStatistics>();

            using (var context = _dataContextProvider.Create())
            {

                foreach (var user in users)
                {
                    var emails = (await context
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
        }

        public async Task<IEnumerable<UserChatStatistic>> GetChatStatisticsHistory(User[] users, DateTime from, DateTime to)
        {
            var result = new List<UserChatStatistic>();

            using (var context = _dataContextProvider.Create())
            {
                foreach (var user in users)
                {
                    var chat = await context
                        .UserChats
                        .FirstOrDefaultAsync(e => e.User.ID == user.ID && e.Date >= from && e.Date <= to);

                    if (chat != null)
                        result.Add(new UserChatStatistic
                        {
                            User = user,
                            ChatInvatationsCount = chat.ChatInvatationsCount
                        });
                }

                return result;
            }
        }

        public async Task<UserChatStatistic> GetChatStatistics(DateTime from, DateTime to, User user)
        {
            //List<ChatItem> chats;
            //using (StreamReader file = File.OpenText(@"e:\chats.json"))
            //{
            //    JsonSerializer serializer = new JsonSerializer();
            //    chats = (List<ChatItem>)serializer.Deserialize(file, typeof(List<ChatItem>));
            //}

            var chats = await _dataProvider.GetChats(from, to, user);

            //using (StreamWriter file = File.CreateText($"e:\\chats.json"))
            //{
            //    JsonSerializer serializer = new JsonSerializer();
            //    serializer.Serialize(file, chats.ToList());
            //}

            return new UserChatStatistic
            {
                User = user,
                ChatInvatationsCount = chats.Count()
            };
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

            using (var context = _dataContextProvider.Create())
            {
                var usersOnline =
                    await context
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
                        PercentageOnline =
                            (double)userGroup.Count(online => online.IsOnline) / (double)userGroup.Count(),
                        TotalMinutesOnline = (int)totalSeconds / 60
                    });
                }
                
                return result;
            }
        }

        public async Task SetSelectedUsers(string[] userNames)
        {
            using (var context = _dataContextProvider.Create())
            {
                var users = context.SelectedUsers.ToList();
                context.SelectedUsers.RemoveRange(users);

                context.SelectedUsers.AddRange(
                    context.Users.Where(user => userNames.Contains(user.Name))
                        .ToList()
                        .Select(user =>
                            new SelectedUser
                            {
                                //TODO create this in DAL
                                Id = Guid.NewGuid(),
                                User = user
                            }));

                await context.SaveChangesAsync();
            }
        }
    }
}
