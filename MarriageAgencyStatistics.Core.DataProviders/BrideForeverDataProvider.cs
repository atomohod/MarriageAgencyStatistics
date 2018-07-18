using System;
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

        //https://bride-forever.com/en/agency/users/
        public async Task<IEnumerable<User>> GetUsers()
        {
            int page = 1;
            List<User> users = new List<User>();
            bool noNewUsers = false;

            do
            {
                var userList = await _client.GetAsync("https://bride-forever.com/en/agency/users/index/page/" + page, async content =>
                {
                    var contentBox = await GetContentAsync(content);

                    var names = contentBox.ChildNodes
                        .Select(node => node as IHtmlDivElement)
                        .Where(element => element?.InnerHtml != null &&
                                          element.InnerHtml.Contains(
                                              "/en/agency/profile/index/userId/"))
                        .Select(element => element.TextContent)
                        .ToList();

                    List<User> items = new List<User>();

                    foreach (var name in names)
                    {
                        var data = name.Split(new[] { "ID:" }, StringSplitOptions.None);

                        if (data.Length != 2)
                            continue;

                        items.Add(new User
                        {
                            Name = data[0].Trim(),
                            ID = data[1].Trim()
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
        public async Task<UserCommunications> GetSentEmailsData(User user, DateTime from, DateTime to)
        {
            int page = 1;
            bool stop = false;
            from = from.ToStartOfTheDay();
            to = to.ToEndOfTheDay();

            DateTime lastTimeEmailWasSent = to;
            List<SentEmailData> result = new List<SentEmailData>();

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
                    result.AddRange(sentEmailDatas);

                page++;

            } while (lastTimeEmailWasSent >= from && page < 500 && !stop);

            return new UserCommunications(user, result);
        }

        public async Task<object> Get(User user, DateTime date)
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
                        .SingleOrDefault()?.Rows;
                });

            return null;
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

        //https://bride-forever.com/en/agency/statistic/chat/minDate/2018-07-15/maxDate/2018-07-15/page/2
        public async Task<IEnumerable<ChatItem>> GetChats(DateTime @from, DateTime to, int? maxPages = -1)
        {
            int page = 1;
            bool stop = false;
            var fromString = from.ToString("yyyy-MM-dd");
            var toString = to.ToString("yyyy-MM-dd"); ;

            var chats = new List<ChatItem>();

            var maxPage = maxPages ?? await _client.GetAsync($"https://bride-forever.com/en/agency/statistic/chat/minDate/{fromString}/maxDate/{toString}/",
                async content =>
                {
                    var contentBox = await GetContentAsync(content);

                    var result = contentBox
                    .ChildNodes.Last(node => node is IHtmlDivElement)
                    .ChildNodes.First(node => node is IHtmlUnorderedListElement)
                    .ChildNodes.OfType<IHtmlListItemElement>()
                    .Select(item => item.Text().Trim())
                    .Last(item => int.TryParse(item, out int _));

                    return int.Parse(result);
                });

            do
            {
                var result = await _client.GetAsync(
                    $"https://bride-forever.com/en/agency/statistic/chat/minDate/{fromString}/maxDate/{toString}/page/{page}",
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

            } while (page < maxPage && !stop);

            return chats;
        }
    }
}
