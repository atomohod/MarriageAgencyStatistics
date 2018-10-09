using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using MarriageAgencyStatistics.Common;
using MarriageAgencyStatistics.Core.DataProviders;
using MarriageAgencyStatistics.Core.Services;
using MarriageAgencyStatistics.DataTransferModels;

namespace MarriageAgencyStatistics.WebAPI.Controllers.BrideForever
{
    [Route("api/brideforever")]
    public class BrideForeverController : ApiController
    {
        private readonly BrideForeverService _brideForeverService;

        public BrideForeverController(BrideForeverService brideForeverService)
        {
            _brideForeverService = brideForeverService;
        }

        [HttpGet]
        [Route("statistic")]
        public async Task<IEnumerable<UserOnlineStatisticsModel>> GetUserOnlineStatistics(DateTime date, [FromUri] string[] userNames)
        {
            var users = await GetUsefulUsers(userNames);

            var statistics = await _brideForeverService.GetOnlineStatistic(users.ToArray(), date);

            return statistics.Select(onlineStatistics => new UserOnlineStatisticsModel
            {
                User = new UserModel
                {
                    Title = onlineStatistics.User.Name
                },

                Online = onlineStatistics.PercentageOnline,
                TotalMinutesOnline = onlineStatistics.TotalMinutesOnline
            });
        }

        [HttpGet]
        [Route("sentemails")]
        public async Task<IEnumerable<UserSentEmailsStatisticsModel>> GetUserSentEmailsCountToday([FromUri] string[] userNames)
        {
            var users = await GetUsefulUsers(userNames);

            var statistics = await _brideForeverService.GetCountOfSentEmails(users.ToArray(), DateTime.UtcNow.ToStartOfTheDay(), DateTime.UtcNow);

            return statistics.Select(emailStatistics => new UserSentEmailsStatisticsModel
            {
                User = new UserModel
                {
                    Title = emailStatistics.User.Name
                },

                EmailsCount = emailStatistics.SentEmails
            });
        }

        [HttpGet]
        [Route("sentemailshistory")]
        public async Task<IEnumerable<UserSentEmailsStatisticsModel>> GetUserSentEmailsCount(DateTime dateFrom,
            DateTime dateTo, [FromUri] string[] userNames)
        {
            var users = await GetUsefulUsers(userNames);

            var statistics =  await _brideForeverService.GetCountOfSentEmailsHistory(users.ToArray(), dateFrom, dateTo);

            return statistics.Select(emailStatistics => new UserSentEmailsStatisticsModel
            {
                User = new UserModel
                {
                    Title = emailStatistics.User.Name
                },

                EmailsCount = emailStatistics.SentEmails
            });
        }
        
        [HttpGet]
        [Route("bonus")]
        public async Task<IEnumerable<UserBonusModel>> GetUserBonusToday(DateTime date, [FromUri] string[] userNames)
        {
            var users = await GetUsefulUsers(userNames);

            var userBonuses = await _brideForeverService.GetUserBonuses(users.ToArray(), date);

            //TODO user automapper
            return userBonuses.Select(bonus => new UserBonusModel
            {
                User = new UserModel
                {
                    Title = bonus.User.Name
                },

                Bonus = new BonusModel
                {
                    Daily = bonus.Today,
                    Monthly = bonus.LastMonth
                }
            });
        }


        [HttpGet]
        [Route("chathistory")]
        public async Task<IEnumerable<UserChatStatisticsModel>> GetUserChatStatistics(DateTime date, [FromUri] string[] userNames)
        {
            var users = await GetUsefulUsers(userNames);

            var chatStatistics = await _brideForeverService.GetChatStatisticsHistory(users.ToArray(), date, date);

            //TODO user automapper
            return chatStatistics.Where(statistic => users.Select(user => user.Name).Contains(statistic.User.Name)).Select(chatStatistic => new UserChatStatisticsModel
            {
                User = new UserModel
                {
                    Title = chatStatistic.User.Name
                },

                ChatStatistics = new ChatStatisticsModel
                {
                    CountSentInvatations = chatStatistic.ChatInvatationsCount
                }
            });
        }

        [HttpGet]
        [Route("bonushistory")]
        public async Task<IEnumerable<UserBonusModel>> GetUserBonus(DateTime date, [FromUri] string[] userNames)
        {
            var users = await GetUsefulUsers(userNames);

            var userBonuses = await _brideForeverService.GetUserBonusesHistory(users.ToArray(), date);

            //TODO user automapper
            return userBonuses.Select(bonus => new UserBonusModel
            {
                User = new UserModel
                {
                    Title = bonus.User.Name
                },

                Bonus = new BonusModel
                {
                    Daily = bonus.Today,
                    Monthly = bonus.LastMonth
                }
            });
        }

        [HttpGet]
        [Route("users")]
        public async Task<IEnumerable<UserModel>> GetUsers()
        {
            var users = await _brideForeverService.GetUsers(UserMode.Active, UserMode.Silent);

            var result = new List<UserModel>();

            foreach (var user in users)
            {
                //TODO user automapper
                result.Add(new UserModel
                {
                    Title = user.Name
                });
            }

            return result;
        }

        [HttpGet]
        [Route("selectedusers")]
        public async Task<IEnumerable<UserModel>> GetSelectedUsers()
        {
            var users = await _brideForeverService.GetSelectedUsers();

            var result = new List<UserModel>();

            foreach (var user in users)
            {
                //TODO user automapper
                result.Add(new UserModel
                {
                    Title = user.Name
                });
            }

            return result;
        }

        [HttpPost]
        [Route("selectedusers")]
        public async Task<IHttpActionResult> SetSelectedUsers([FromBody] string[] userNames)
        {
            await _brideForeverService.SetSelectedUsers(userNames);
            return Ok();
        }

        private async Task<IEnumerable<User>> GetUsefulUsers(string[] userNames)
        {
            return await _brideForeverService.GetUsers(userNames, UserMode.Active,  UserMode.Silent);
        }
    }
}
