using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using MarriageAgencyStatistics.Core.Clients.BrideForever;
using MarriageAgencyStatistics.Core.DataProviders.BrideForever;
using MarriageAgencyStatistics.Domain.BrideForever;
using MarriageAgencyStatistics.Parser.Core;
using Newtonsoft.Json;
using RestSharp;

namespace MarriageAgencyStatistics.ConsoleUI
{
    class Program
    {
        static void Main()
        {
            var brideForeverDataProvider = new BrideForeverDataProvider(new BrideForeverClient("viktorya.tory1", "QZW17111992QZW"));

            var online = brideForeverDataProvider.GetUserIdsOnline().Result;
            
            var users = brideForeverDataProvider.GetUsers().Result;
           
            List<(User,Bonus)> userBonuses = new List<(User, Bonus)>();

            foreach (var user in users)
            {
                userBonuses.Add((user, brideForeverDataProvider.GetUserBonus(user).Result));
            }
            
            var excel = new BrideForeverExcel();

            excel.UpdateUserBonuses(userBonuses);
        }
    }
}