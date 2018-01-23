﻿using System;
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
        private readonly BrideForeverDataProvider _dataProvider;
        private readonly BrideForeverService _brideForeverService;

        public BrideForeverController(BrideForeverDataProvider dataProvider, BrideForeverService brideForeverService)
        {
            _dataProvider = dataProvider;
            _brideForeverService = brideForeverService;
        }

        [HttpGet]
        [Route("statistic")]
        public async Task<IEnumerable<UserOnlineStatisticsModel>> GetOnlineStatistics(DateTime date)
        {
            var statistics = await _brideForeverService.GetOnlineStatistic(date);

            return statistics.Select(onlineStatistics => new UserOnlineStatisticsModel
            {
                User = new UserModel
                {
                    Title = onlineStatistics.User.Name
                },

                Online = onlineStatistics.Online
            });
        }

        [HttpGet]
        [Route("sentemails")]
        public async Task<IEnumerable<UserSentEmailsStatisticsModel>> GetSentEmailsCount(DateTime dateFrom, DateTime dateTo)
        {
            var statistics = await _brideForeverService.GetCountOfSentEmails(dateFrom, dateTo);

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
            var users = await _dataProvider.GetUsers();

            var result = new List<UserBonusModel>();

            foreach (var user in users)
            {
                var userBonus = await _dataProvider.GetUserBonus(user, date);

                //TODO user automapper
                result.Add(new UserBonusModel
                {
                    User = new UserModel
                    {
                        Title = user.Name
                    },

                    Bonus = new BonusModel
                    {
                        Daily = userBonus.Today,
                        Monthly = userBonus.LastMonth
                    }
                });
            }

            return result;
        }

        [HttpGet]
        [Route("users")]
        public async Task<IEnumerable<UserModel>> GetUsers()
        {
            var users = await _dataProvider.GetUsers();

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
    }
}
