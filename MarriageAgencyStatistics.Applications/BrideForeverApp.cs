using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarriageAgencyStatistics.Applications.Models;
using Newtonsoft.Json;
using RestSharp;

namespace MarriageAgencyStatistics.Applications
{
    public class BrideForeverApp
    {
        private readonly RestClient _client;

        public BrideForeverApp(RestClient client)
        {
            _client = client;
        }

        public async Task<List<UserBonus>> GetBonuses(DateTime choosenDate, params string[] userNames)
        {
            return await _client.GetTaskAsync<List<UserBonus>>(new RestRequest($"bonushistory?date={choosenDate.Month}%2F{choosenDate.Day}%2F{choosenDate.Year}{GetSelectedUsersString(userNames)}"));
        }

        public async Task<List<UserOnlineStatistics>> GetStatistics(DateTime choosenDate, params string[] userNames)
        {
            return await _client.GetTaskAsync<List<UserOnlineStatistics>>(new RestRequest($"statistic?date={choosenDate.Month}%2F{choosenDate.Day}%2F{choosenDate.Year}{GetSelectedUsersString(userNames)}"));
        }

        public async Task<List<UserSentEmailsStatistics>> GetSentEmails(DateTime choosenDate, params string[] userNames)
        {
            return await _client.GetTaskAsync<List<UserSentEmailsStatistics>>(new RestRequest($"sentemailshistory?dateFrom={choosenDate.Month}%2F{choosenDate.Day}%2F{choosenDate.Year}&dateTo={choosenDate.Month}%2F{choosenDate.Day}%2F{choosenDate.Year}{GetSelectedUsersString(userNames)}"));
        }

        public async Task<List<User>> GetUsers()
        {
            return await _client.GetTaskAsync<List<User>>(new RestRequest($"users"));
        }

        public async Task<List<User>> GetSelectedUsers()
        {
            return await _client.GetTaskAsync<List<User>>(new RestRequest($"selectedusers"));
        }

        public void SaveSelectedUsers(params string[] userNames)
        {
            var restRequest = new RestRequest($"selectedusers", Method.POST);
            restRequest.AddHeader("content-type", "application/json");
            restRequest.AddParameter("application/json", JsonConvert.SerializeObject(userNames), ParameterType.RequestBody);

            _client.Post(restRequest);
        }

        private static string GetSelectedUsersString(params string[] userNames)
        {
            return userNames
                .Select(item => $"&userNames={item}")
                .Aggregate((a, b) => $"{a}{b}");
        }
    }
}
