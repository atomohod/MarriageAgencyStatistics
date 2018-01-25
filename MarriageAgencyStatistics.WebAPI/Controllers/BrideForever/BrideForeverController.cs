using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
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
        public async Task<IEnumerable<UserOnlineStatisticsModel>> GetOnlineStatistics(DateTime date)
        {
            var users = await _brideForeverService.GetSelectedUsers();

            var statistics = await _brideForeverService.GetOnlineStatistic(users.ToArray(), date);

            return statistics.Select(onlineStatistics => new UserOnlineStatisticsModel
            {
                User = new UserModel
                {
                    Title = onlineStatistics.User.Name
                },

                Online = onlineStatistics.PercentageOnline
            });
        }

        [HttpGet]
        [Route("sentemails")]
        public async Task<IEnumerable<UserSentEmailsStatisticsModel>> GetSentEmailsCount(DateTime dateFrom,
            DateTime dateTo)
        {
            var users = await _brideForeverService.GetSelectedUsers();

            var statistics = await _brideForeverService.GetCountOfSentEmails(users.ToArray(), dateFrom, dateTo);

            return statistics.Select(emailStatistics => new UserSentEmailsStatisticsModel
            {
                User = new UserModel
                {
                    Title = emailStatistics.User.Name
                },

                EmailsCount = emailStatistics.SentEmails
            });
        }

        // GET api/values
        [HttpGet]
        [Route("bonus")]
        public async Task<IEnumerable<UserBonusModel>> GetUserBonus(DateTime date)
        {
            var users = await _brideForeverService.GetSelectedUsers();


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
        [Route("users")]
        public async Task<IEnumerable<UserModel>> GetUsers()
        {
            var users = await _brideForeverService.GetAllUsers();

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
        public async Task<IHttpActionResult> SetSelectedUsers(string[] ids)
        {
            await _brideForeverService.SetSelectedUsers(ids);
            return Ok();
        }
    }
}
