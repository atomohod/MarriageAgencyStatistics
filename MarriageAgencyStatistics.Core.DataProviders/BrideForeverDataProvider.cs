﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Extensions;
using MarriageAgencyStatistics.Common;

namespace MarriageAgencyStatistics.Core.DataProviders
{
    public class BrideForeverDataProvider : BaseDataProvider
    {
        private readonly Client _client;

        public BrideForeverDataProvider(Client client)
        {
            _client = client;
        }

        //https://bride-forever.com/en/agency/users/index/viewType/list/page/
        public async Task<IEnumerable<User>> GetActiveUsers()
        {
            return await GetUsers("https://bride-forever.com/en/agency/users/index/viewType/list/page/");
        }

        //https://bride-forever.com/en/agency/users/index/viewType/list/page/
        public async Task<IEnumerable<User>> GetSilentUsers()
        {
            return await GetUsers("https://bride-forever.com/en/agency/users/hidden/page/");
        }

        //https://bride-forever.com/en/agency/users/index/viewType/list/page/
        public async Task<IEnumerable<User>> GetInactiveUsers()
        {
            return await GetUsers("https://bride-forever.com/en/agency/users/inactive/page/");
        }

        private async Task<IEnumerable<User>> GetUsers(string url)
        {
            int page = 1;
            List<User> users = new List<User>();
            bool noNewUsers = false;

            do
            {
                var userList = await _client.GetAsync(url + page, async content =>
                {
                    var contentBox = await GetContentAsync(content);

                    var names = contentBox.ChildNodes.OfType<IHtmlTableElement>().FirstOrDefault()?.Rows
                        .Where(element => element?.InnerHtml != null &&
                                          element.InnerHtml.Contains(
                                              "/en/agency/profile/index/userId/"))
                        .Select(element => new { Text = element.TextContent, Href = element })
                        .ToList();

                    List<User> items = new List<User>();

                    foreach (var name in names)
                    {
                        var userData = name.Text.Split('\t');
                        var meaningfulData = userData
                            .Select(s => s.Trim())
                            .Where(s => !string.IsNullOrEmpty(s) && s != "to" && s != "\n")
                            .ToList();

                        var id = name.Href.QuerySelectorAll("*").OfType<IHtmlAnchorElement>().First().Href.Split('/').Last(s => int.TryParse(s, out int _));
                        
                        items.Add(new User
                        {
                            Name = $"{meaningfulData[0].Trim()} {meaningfulData[1].Trim()}",
                            ID = id
                        });
                    }

                    return items;
                });

                noNewUsers = userList.All(user => users.Any(u => u.ID == user.ID));

                if (!noNewUsers)
                    users.AddRange(userList);

                page++;

            } while (!noNewUsers || page > 10);

            return users;
        }

        //https://bride-forever.com/en/agency/mail/read-sent/userId/{userId}/page/{pageId}
        public async Task<UserCommunications> GetSentEmailsData(User user, DateTime from, DateTime to, int? maxPages = null)
        {
            int page = 1;
            bool stop = false;
            from = from.ToStartOfTheDay();
            to = to.ToEndOfTheDay();

            DateTime lastTimeEmailWasSent = to;
            List<SentEmailData> emails = new List<SentEmailData>();

            int? maxPage = maxPages ?? await _client.GetAsync(
                               $"https://bride-forever.com/en/agency/mail/read-sent/userId/{user.ID}",
                               async content =>
                               {
                                   var contentBox = await GetContentAsync(content);

                                   return contentBox
                                       .ChildNodes.LastOrDefault(node => node is IHtmlUnorderedListElement)?
                                       .ChildNodes.OfType<IHtmlListItemElement>()
                                       .Select(item =>
                                       {
                                           int.TryParse(item.Text().Trim(), out int parsed);
                                           return parsed;
                                       }).Max();
                               });

            if (maxPage > 500) maxPage = 500;

            do
            {
                var sentEmailDatas = await _client.GetAsync($"https://bride-forever.com/en/agency/mail/read-sent/userId/{user.ID}/page/{page}", async content =>
                {
                    var contentBox = await GetContentAsync(content);

                    var headings = contentBox.ChildNodes
                        .OfType<IHtmlHeadingElement>()
                        .ToList();

                    List<SentEmailData> items = new List<SentEmailData>();

                    foreach (var headingElement in headings)
                    {
                        var textContent = headingElement.TextContent;

                        var splittedText = textContent.Split('\t');
                        var meaningfulData = splittedText
                            .Select(s => s.Trim())
                            .Where(s => !string.IsNullOrEmpty(s) && s != "to" && s != "\n")
                            .ToList();

                        var emailWasSentAt = DateTime.Parse(meaningfulData[0]);

                        if (emailWasSentAt < from)
                        {
                            stop = true;
                            return items;
                        }

                        if (emailWasSentAt > to)
                            continue;

                        lastTimeEmailWasSent = emailWasSentAt;
                        var isRead = meaningfulData[1].ToLower() == "read";

                        items.Add(isRead ? SentEmailData.Read(emailWasSentAt) : SentEmailData.NotRead(emailWasSentAt));
                    }

                    return items;
                });

                if (sentEmailDatas != null && sentEmailDatas.Any())
                    emails.AddRange(sentEmailDatas);

                page++;

            } while (lastTimeEmailWasSent >= from && page <= maxPage && !stop);

            return new UserCommunications(user, emails);
        }

        public async Task<IEnumerable<UserChatLogItem>> GetChatLogMessages(User user, DateTime from, DateTime to)
        {
            var chatLogsUrls = await _client.PostAsync("https://bride-forever.com/en/agency/statistic/bonuses/",
                new
                {
                    female = user.ID,
                    trans_type = 6,
                    periodStart = from.Date.ToString(@"yyyy-MM-dd"),
                    periodEnd = to.Date.ToString(@"yyyy-MM-dd")
                },
                async content =>
                {
                    var contentBox = await GetContentAsync(content);

                    // Show statistic per selected period 
                    var result = contentBox.ChildNodes
                        .OfType<IHtmlTableElement>().FirstOrDefault()?
                        .Rows.Select(row => row.Cells).Skip(1)
                        .SelectMany(elements => elements)
                        .Where(element => element.Text().Contains("View Chat Log"))
                        .SelectMany(element => element.ChildNodes)
                        .OfType<IHtmlAnchorElement>()
                        .Select(element => element.PathName)
                        .ToList();

                    return result;
                });

            if (chatLogsUrls == null)
                return null;

            List<UserChatLogItem> logs = new List<UserChatLogItem>();

            foreach (var chatLogsUrl in chatLogsUrls)
            {
                var userChatLogItems = await GetChatLogMessages(from, to, user, chatLogsUrl);
                logs.AddRange(userChatLogItems);
            }

            return logs;
        }

        private async Task<IEnumerable<UserChatLogItem>> GetChatLogMessages(DateTime from, DateTime to, User user, string url)
        {
            from = from.ToStartOfTheDay();
            to = to.ToEndOfTheDay();

            var result = await _client.GetAsync($"https://bride-forever.com{url}",
                async content =>
                {
                    var contentBox = await GetContentAsync(content);

                    var headings = contentBox
                        .ChildNodes
                        .OfType<IHtmlHeadingElement>()
                        .Select(node =>
                        {
                            var text = node.Text();

                            var splitted = text.Split();

                            return new UserChatLogItem
                            {
                                User = user,
                                Name = splitted[15].Replace(":", ""),
                                SentOn = DateTime.Parse($"{splitted[4]} {splitted[5]}")
                            };
                        })
                        .ToList();

                    var filtered = headings
                        .Where(item => item.SentOn >= from && item.SentOn <= to)
                        .ToList();

                    return filtered;
                });

            return result;
        }

        //https://bride-forever.com/en/agency/statistic/bonuses/
        public async Task<Bonus> GetUserBonus(User user, DateTime date)
        {
            var dailyBonus = await _client.PostAsync("https://bride-forever.com/en/agency/statistic/bonuses/",
                new
                {
                    female = user.ID,
                    periodStart = date.Date.ToString(@"yyyy-MM-dd"),
                    periodEnd = date.Date.ToString(@"yyyy-MM-dd"),
                    sum = 1
                },
                async content =>
                {
                    var contentBox = await GetContentAsync(content);

                    // Show statistic per selected period 
                    return contentBox.ChildNodes
                        .OfType<IComment>()
                        .SingleOrDefault(comment => comment.Text() == " Show statistic per selected period ")?
                        .NextElementSibling.ChildNodes
                        .OfType<IHtmlTableElement>()
                        .SingleOrDefault()?.Rows
                        .SingleOrDefault(element => element.TextContent.Contains("Total amount:"))?
                        .TextContent.Split(new[] { "Total amount:" }, StringSplitOptions.None)[1].Trim();
                });

            var monthlyBonus = await _client.PostAsync("https://bride-forever.com/en/agency/statistic/bonuses/",
                new
                {
                    female = user.ID,
                    periodStart =
                        new DateTime(date.Date.Year, date.Month, 1).ToString(@"yyyy-MM-dd"),
                    periodEnd = date.Date.ToString(@"yyyy-MM-dd"),
                    sum = 1
                },
                async content =>
                {
                    var contentBox = await GetContentAsync(content);

                    // Show statistic per selected period 
                    return contentBox.ChildNodes
                        .OfType<IComment>()
                        .SingleOrDefault(comment => comment.Text() == " Show statistic per selected period ")?
                        .NextElementSibling.ChildNodes
                        .OfType<IHtmlTableElement>()
                        .SingleOrDefault()?.Rows
                        .SingleOrDefault(element => element.TextContent.Contains("Total amount:"))?
                        .TextContent.Split(new[] { "Total amount:" }, StringSplitOptions.None)[1].Trim();
                });

            return new Bonus
            {
                User = user,
                Today = dailyBonus == null ? 0m : decimal.Parse(dailyBonus.RemoveLast()),
                LastMonth = monthlyBonus == null ? 0m : decimal.Parse(monthlyBonus.RemoveLast())
            };
        }

        //https://bride-forever.com/en/agency/statistic/online/
        public async Task<IEnumerable<string>> GetUserIdsOnline()
        {
            var ids = await _client.GetAsync("https://bride-forever.com/en/agency/statistic/online/", async content =>
            {
                var contentBox = await GetContentAsync(content);

                if (contentBox?.ChildNodes == null || !contentBox.ChildNodes.Any())
                    return null;

                return contentBox
                            .ChildNodes.First(node => node is IHtmlTableElement)
                            .ChildNodes.Last(node => node is IHtmlTableSectionElement)
                            .ChildNodes.OfType<IHtmlTableRowElement>()
                            .Select(element => element.Cells[0]?.Text()?.Trim())
                            .ToList();
            });

            return ids;
        }

        //https://bride-forever.com/en/agency/statistic/chat/minDate/{fromString}/maxDate/{toString}/female_id/{user.ID}/filter_type/inv/filter_text//page/{page}
        public async Task<IEnumerable<ChatItem>> GetChats(DateTime @from, DateTime to, User user, int? maxPages = null)
        {
            int page = 1;
            var fromString = from.ToString("yyyy-MM-dd");
            var toString = to.ToString("yyyy-MM-dd");

            var chats = new List<ChatItem>();

            var isUserValid = await _client.GetAsync($"https://bride-forever.com/en/agency/statistic/chat/minDate/{fromString}/maxDate/{toString}/female_id/{user.ID}/filter_type/inv/filter_text/",
                async content =>
                {
                    var contentBox = await GetContentAsync(content); //0 1 10

                    var users = contentBox.ChildNodes.FirstOrDefault(e => e is IHtmlFormElement)?
                        .ChildNodes.FirstOrDefault(e => e is IHtmlParagraphElement)?
                        .ChildNodes.FirstOrDefault(e => e is IHtmlSelectElement)?
                        .ChildNodes.OfType<IHtmlOptionElement>()
                        .Select(element => element.Text)
                        .ToList();

                    return users.Any(s => s.ToLower().Contains(user.ID.ToLower()));

                });

            if (!isUserValid)
                return chats;

            int? maxPage = maxPages ?? await _client.GetAsync($"https://bride-forever.com/en/agency/statistic/chat/minDate/{fromString}/maxDate/{toString}/female_id/{user.ID}/filter_type/inv/filter_text/",
                async content =>
                {
                    var contentBox = await GetContentAsync(content);

                    var result = contentBox
                    .ChildNodes.LastOrDefault(node => node is IHtmlDivElement)?
                    .ChildNodes.FirstOrDefault(node => node is IHtmlUnorderedListElement)?
                    .ChildNodes.OfType<IHtmlListItemElement>()
                    .Select(item => item.Text().Trim())
                    .LastOrDefault(item => int.TryParse(item, out int _));

                    return string.IsNullOrEmpty(result) ? null : (int?)int.Parse(result);
                });

            if (maxPage == null)
                return chats;

            do
            {
                var result = await _client.GetAsync(
                    $"https://bride-forever.com/en/agency/statistic/chat/minDate/{fromString}/maxDate/{toString}/female_id/{user.ID}/filter_type/inv/filter_text//page{page}",
                    async content =>
                    {
                        var contentBox = await GetContentAsync(content);

                        var items = contentBox
                                .ChildNodes.First(node => node is IHtmlTableElement)
                                .ChildNodes.Last(node => node is IHtmlTableSectionElement)
                                .ChildNodes.OfType<IHtmlTableRowElement>()
                                .Select(element => element.Cells)
                                .ToList();

                        var chatItems = items.Select(item =>
                        new ChatItem
                        {
                            Id = item[0].Text().Trim(),
                            SentDate = item[1].Text().Trim(),
                            Sender = item[2].Text().Trim(),
                            Reciever = item[3].Text().Trim(),
                            Message = item[4].Text().Trim()
                        });

                        page++;

                        return chatItems;
                    });

                chats.AddRange(result);

            } while (page < maxPage);

            return chats;
        }
    }
}
