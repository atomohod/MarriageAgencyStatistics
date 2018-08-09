﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using MarriageAgencyStatistics.Common;
using MarriageAgencyStatistics.Core.DataProviders;
using MarriageAgencyStatistics.DataAccess.EF;
using Newtonsoft.Json;

namespace MarriageAgencyStatistics.Core.Services
{
    public class BrideForeverBaseDataAnalyzer
    {
        protected readonly BrideForeverDataProvider DataProvider;
        protected IEnumerable<User> Users { get; private set; }

        public BrideForeverBaseDataAnalyzer(BrideForeverDataProvider dataProvider)
        {
            DataProvider = dataProvider;
        }

        public async Task LoadUsers()
        {
            Users = await DataProvider.GetUsers();
        }
    }

    public sealed class BrideForeverCountChatStatistics : BrideForeverBaseDataAnalyzer
    {
        private readonly DateTime _from;
        private readonly DateTime _to;
        private IEnumerable<ChatItem> _chats;

        public BrideForeverCountChatStatistics(BrideForeverDataProvider dataProvider, DateTime from, DateTime to)
            : base(dataProvider)
        {
            _from = from;
            _to = to;
        }

        public async Task Load()
        {
            _chats = await DataProvider.GetChats(_from, _to);
        }

        public async Task<IEnumerable<UserChatStatistic>> GetPhrasesStatistic()
        {
            var chatStats = _chats
                .GroupBy(item => item.Sender)
                .Where(g => Users.Any(user => user.Name == g.Key))
                .Select(g =>
                new
                {
                    User = Users.First(user => user.Name == g.Key),
                    Items = g.ToList(),
                    Count = g.Count()
                });

            var stats = new List<UserChatStatistic>();

            foreach (var stat in chatStats)
            {
                var chatLogs = await DataProvider.GetChatLogMessages(stat.User, _from, _to);

                var userChatStatistics = new UserChatStatistic
                {
                    User = stat.User
                };

                await ApplyCountChatInvatations(userChatStatistics, stat.Count);
                await ApplyPhrasesStataistics(userChatStatistics, stat.Items);

                stats.Add(userChatStatistics);
            }

            return stats;
        }

        private Task ApplyPhrasesStataistics(UserChatStatistic userChatStatistics, IEnumerable<ChatItem> items)
        {
            var data = items
                .Select(item =>
                new
                {
                    Phrase = item,
                    Splitted = item.Message.Split()
                })
                .ToList();



            for (int i = 0; i < data.Count; i++)
            {
                for (int j = 0; j < data.Count; j++)
                {
                    if (i == j)
                        continue;
                    
                    //define arrays that will be compared
                    var x = data[i].Splitted;
                    var y = data[j].Splitted;

                    for (int k = 0; k < x.Length; k++)
                    {
                        var up = k > 0 ? (x[k] == y[k - 1] ? 1 : 0) : 0;
                        var down = k < y.Length - 1 ? (x[k] == y[k + 1] ? 1 : 0) : 0;
                        var middle = k < y.Length ? (x[k] == y[k] ? 1 : 0) : 0;
                    }

                    
                }
            }

            return null;

        }

        private async Task ApplyCountChatInvatations(UserChatStatistic result, int totalCount)
        {
            var chatLogs = await DataProvider.GetChatLogMessages(result.User, _from, _to);
            result.ChatInvatationsCount = totalCount - (chatLogs?.Count(item => item.SentByUser) ?? 0);
        }
    }

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

        public async Task<IEnumerable<UserChatStatistic>> GetChatStatisticsHistory(User[] users, DateTime from, DateTime to)
        {
            var result = new List<UserChatStatistic>();

            foreach (var user in users)
            {
                var chat = await _dataContext
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

        public async Task<IEnumerable<UserChatStatistic>> GetChatStatistics(DateTime from, DateTime to)
        {
            var users = await _dataProvider.GetUsers();

            //List<ChatItem> chats;

            //using (StreamReader file = File.OpenText(@"e:\chats.json"))
            //{
            //    JsonSerializer serializer = new JsonSerializer();
            //    chats = (List<ChatItem>)serializer.Deserialize(file, typeof(List<ChatItem>));
            //}

            var chats = await _dataProvider.GetChats(from, to);


            //using (StreamWriter file = File.CreateText($"e:\\chats.json"))
            //{
            //    JsonSerializer serializer = new JsonSerializer();
            //    serializer.Serialize(file, chats.ToList());
            //}

            var chatStats = chats
                .GroupBy(item => item.Sender)
                .Where(g => users.Any(user => user.Name == g.Key))
                .Select(g =>
                new
                {
                    User = users.First(user => user.Name == g.Key),
                    Count = g.Count()
                });

            var stats = new List<UserChatStatistic>();

            foreach (var stat in chatStats)
            {
                var chatLogs = await _dataProvider.GetChatLogMessages(stat.User, from, to);
                stats.Add(new UserChatStatistic
                {
                    User = stat.User,
                    ChatInvatationsCount = stat.Count - (chatLogs?.Count(item => item.SentByUser) ?? 0)
                });
            }

            return stats;
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
