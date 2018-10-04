using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarriageAgencyStatistics.Common;
using MarriageAgencyStatistics.Core.Clients;
using MarriageAgencyStatistics.Core.DataProviders;
using MarriageAgencyStatistics.Core.Services;
using MarriageAgencyStatistics.DataAccess.EF;
using MarriageAgencyStatistics.Formatters;
using MarriageAgencyStatistics.Jobs;

namespace MarriageAgencyStatistics.ConsoleUI
{
    class Program
    {
        static void Main()
        {
            var brideForeverDataProvider = new BrideForeverDataProvider(new BrideForeverClient("viktorya.tory1", "QZW17111992QZW"));
            var service = new BrideForeverService(brideForeverDataProvider, new BrideForeverDataContextProvider());

            var job = new CountUserBonusesMonthly(service, new BrideForeverDataContextProvider());
            job.ExecuteAsync(new DateTime(2018, 9, 17), new DateTime(2018, 9, 30)).Wait();

            //var users = service.GetUsers().Result;
            //var user = users.FirstOrDefault(u => u.ID == "42676");
            ////var chats = service.GetChatStatistics(new DateTime(2018, 7, 2), new DateTime(2018, 7, 2), krivko).Result;
            //var chats = service.GetCountOfSentEmails(new []{user},new DateTime(2018, 9, 2), new DateTime(2018, 9, 2)).Result;
        }
    }
}
