using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Extensions;
using AngleSharp.Parser.Html;
using MarriageAgencyStatistics.Common;

namespace MarriageAgencyStatistics.Core.DataProviders
{
    public class BrideForeverDataProvider
    {
        private readonly Client _client;

        public BrideForeverDataProvider(Client client)
        {
            _client = client;
        }

        private IHtmlDocument Parse(string content)
        {
            var parser = new HtmlParser();
            return parser.Parse(content);
        }

        //https://bride-forever.com/en/agency/users/
        public async Task<IEnumerable<User>> GetUsers()
        {
            int page = 1;
            List<User> users = new List<User>();
            bool noNewUsers = false;

            do
            {
                var userList = await _client.Get("https://bride-forever.com/en/agency/users/index/page/" + page, c =>
                {
                    var doc = Parse(c);

                    var contentBox = doc.GetElementsByClassName("contentbox").First();
                    var names = contentBox.ChildNodes
                        .Select(node => node as IHtmlDivElement)
                        .Where(element => element?.PreviousElementSibling?.InnerHtml != null &&
                                          element.PreviousElementSibling.InnerHtml.Contains(
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
        public async Task<IEnumerable<SentEmailData>> GetSentEmailsData(User user, DateTime from, DateTime to)
        {
            int page = 1;
            bool stop = false;
            from = from.ToStartOfTheDay();
            to = to.ToEndOfTheDay();

            DateTime lastTimeEmailWasSent = to;
            List<SentEmailData> result = new List<SentEmailData>();

            do
            {
                var sentEmailDatas = await _client.Get($"https://bride-forever.com/en/agency/mail/read-sent/userId/{user.ID}/page/{page}", c =>
                {
                    var doc = Parse(c);

                    var contentBox = doc.GetElementsByClassName("contentbox").First();
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

                        items.Add(new SentEmailData
                        {
                            WasSent = emailWasSentAt,
                            IsRead = isRead
                        });
                    }

                    return items;
                });

                if (sentEmailDatas != null && sentEmailDatas.Any())
                    result.AddRange(sentEmailDatas);

                page++;

            } while (lastTimeEmailWasSent >= from && page < 100 && !stop);

            return result;
        }

        //https://bride-forever.com/en/agency/statistic/bonuses/
        public async Task<Bonus> GetUserBonus(User user, DateTime date)
        {
            var dailyBonus = await _client.Post("https://bride-forever.com/en/agency/statistic/bonuses/",
                new
                {
                    female = user.ID,
                    periodStart = date.Date.ToString(@"yyyy-MM-dd"),
                    periodEnd = date.Date.ToString(@"yyyy-MM-dd"),
                    sum = 1
                },
                content =>
                {
                    var doc = Parse(content);
                    var contentBox = doc.GetElementsByClassName("contentbox").First();

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

            var monthlyBonus = await _client.Post("https://bride-forever.com/en/agency/statistic/bonuses/",
                new
                {
                    female = user.ID,
                    periodStart =
                    new DateTime(date.Date.Year, date.Month, 1).ToString(@"yyyy-MM-dd"),
                    periodEnd = date.Date.ToString(@"yyyy-MM-dd"),
                    sum = 1
                },
                content =>
                {
                    var doc = Parse(content);
                    var contentBox = doc.GetElementsByClassName("contentbox").First();

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
                Today = dailyBonus == null ? 0m : decimal.Parse(dailyBonus.RemoveLast()),
                LastMonth = monthlyBonus == null ? 0m : decimal.Parse(monthlyBonus.RemoveLast())
            };
        }

        //https://bride-forever.com/en/agency/statistic/online/
        public async Task<IEnumerable<string>> GetUserIdsOnline()
        {
            var ids = await _client.Get("https://bride-forever.com/en/agency/statistic/online/", content =>
            {
                var doc = Parse(content);
                var contentBox = doc.GetElementsByClassName("contentbox").First();

                return contentBox.ChildNodes.Any() ? contentBox
                            .ChildNodes.First(node => node is IHtmlTableElement)
                            .ChildNodes.Last(node => node is IHtmlTableSectionElement)
                            .ChildNodes.OfType<IHtmlTableRowElement>()
                            .Select(element => element.Cells[0]?.Text()?.Trim())
                            .ToList() : null;
            });

            return ids;
        }
    }
}
