using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarriageAgencyStatistics.Common;
using MarriageAgencyStatistics.Core.Clients;
using MarriageAgencyStatistics.Core.DataProviders;
using MarriageAgencyStatistics.Core.Services;
using MarriageAgencyStatistics.Formatters;

namespace MarriageAgencyStatistics.ConsoleUI
{
    class Program
    {
        static void Main()
        {
            var brideForeverDataProvider = new BrideForeverDataProvider(new BrideForeverClient("viktorya.tory1", "QZW17111992QZW"));
            var service = new BrideForeverService(brideForeverDataProvider, null);

            var users = brideForeverDataProvider.GetUsers().Result;
            var krivko = users.FirstOrDefault(user => user.LastName == "Krivko");
            //var chats = service.GetChatStatistics(new DateTime(2018, 7, 2), new DateTime(2018, 7, 2), krivko).Result;
            var chats = service.GetChatStatistics(new DateTime(2018, 8, 11), new DateTime(2018, 8, 12), krivko).Result;
        }
    }
}
