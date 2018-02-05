using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Parser.Html;
using MarriageAgencyStatistics.Common;
using RestSharp;

namespace MarriageAgencyStatistics.Core.Clients
{
    public class BrideForeverClient : Client
    {
        private readonly string _username;
        private readonly string _password;

        public BrideForeverClient(string username, string password)
            : base("https://bride-forever.com/agency/")
        {
            _username = username;
            _password = password;
        }

        public override async Task<bool> Reconnect()
        {
            var payload = await GetAuthPayload();

            if (string.IsNullOrEmpty(payload.Item1) || string.IsNullOrEmpty(payload.Item2))
                return false;

            await SetCookies(payload);

            var request = new RestRequest("login", Method.POST);

            var loginBody = new
            {
                login = _username,
                password = _password,
                submit = "Login"
            };

            request.AddParameter("application/x-www-form-urlencoded", loginBody.ToQueryString(), ParameterType.RequestBody);

            _client.Execute(request);

            return true;
        }

        private Task SetCookies((string, string) payload)
        {
            return Task.Run(() =>
            {
                _client.CookieContainer = new CookieContainer();
                _client.CookieContainer.Add(new Cookie("PHPSESSID", payload.Item1, "/", "bride-forever.com"));
                _client.CookieContainer.Add(new Cookie("__cfduid", payload.Item2, "/", "bride-forever.com"));
            });
        }

        private async Task<(string, string)> GetAuthPayload()
        {
            var loginRequest = new RestRequest("login", Method.GET);
            IRestResponse loginResponse = await _client.ExecuteTaskAsync(loginRequest);

            var phpsessid = loginResponse.Cookies.FirstOrDefault(cookie => cookie.Name == "PHPSESSID")?.Value;
            var __cfduid = loginResponse.Cookies.FirstOrDefault(cookie => cookie.Name == "__cfduid")?.Value;

            return (phpsessid, __cfduid);
        }

        protected override bool IsReloginRequired(IRestResponse response)
        {
            var cookies = _client.CookieContainer?.GetCookieHeader(new Uri("https://bride-forever.com"));
            if (cookies == null || !cookies.Contains("PHPSESSID") || !cookies.Contains("__cfduid"))
                return true;

            var parser = new HtmlParser();
            var document = parser.Parse(response.Content);

            if (document.Title == "Login" || !response.ResponseUri.ToString().Contains("agency"))
                return true;

            return false;
        }
    }
}
