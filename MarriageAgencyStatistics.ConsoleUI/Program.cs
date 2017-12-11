using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using RestSharp;

namespace MarriageAgencyStatistics.ConsoleUI
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new ConnectClient(@"https://bride-forever.com/agency/login", "viktorya.tory1", "QZW17111992QZW");
            var t = client.MakeGetRequest<object>("https://bride-forever.com/agency/login/");
        }
    }

    public sealed class ConnectClient
    {
        //it is not good to keep sensetive information as a field, we can change it later
        private readonly string _authUrl;
        private readonly string _username;
        private readonly string _password;

        public ConnectClient(string authUrl, string username, string password)
        {
            _authUrl = authUrl;
            _username = username;
            _password = password;
        }

        public async Task<T> MakeGetRequest<T>(string url)
        {
            var connection = await ConnectAsync();

            var result = connection.Execute(new RestRequest(Method.GET)).Content;

            return JsonConvert.DeserializeObject<T>(result);
        }

        private Task<RestClient> ConnectAsync()
        {
            var client = new RestClient("https://bride-forever.com/agency/")
            {
                Proxy = new WebProxy("127.0.0.1", 8888)
            };

            var loginRequest = new RestRequest("login", Method.GET);
            IRestResponse loginResponse = client.Execute(loginRequest);

            var phpsessid = loginResponse.Cookies.First(cookie => cookie.Name == "PHPSESSID").Value;
            var __cfduid = loginResponse.Cookies.First(cookie => cookie.Name == "__cfduid").Value;

            client.CookieContainer = new CookieContainer();
            client.CookieContainer.Add(new Cookie("PHPSESSID", phpsessid, "/", "bride-forever.com"));
            client.CookieContainer.Add(new Cookie("__cfduid", __cfduid, "/", "bride-forever.com"));

            var request = new RestRequest("login", Method.POST);

            var obj = new { login = _username, password = _password, submit = "Login" };

            request.AddParameter("application/x-www-form-urlencoded", obj.GetQueryString(), ParameterType.RequestBody);

            IRestResponse response = client.Execute(request);

            return Task.FromResult(client);
        }

        private async Task GetListOfUsers()
        {

        }
    }

    public static class ExtensionMethods
    {
        public static string GetQueryString(this object obj)
        {
            var properties = from p in obj.GetType().GetProperties()
                             where p.GetValue(obj, null) != null
                             select p.Name + "=" + HttpUtility.UrlEncode(p.GetValue(obj, null).ToString());

            return String.Join("&", properties.ToArray());
        }
    }
}