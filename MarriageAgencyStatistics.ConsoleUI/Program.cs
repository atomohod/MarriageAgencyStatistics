using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarriageAgencyStatistics.Common;
using MarriageAgencyStatistics.Core.Clients;
using MarriageAgencyStatistics.Core.DataProviders;
using MarriageAgencyStatistics.Formatters;

namespace MarriageAgencyStatistics.ConsoleUI
{
    class Program
    {
        static void Main()
        {
            var brideForeverDataProvider = new BrideForeverDataProvider(new BrideForeverClient("viktorya.tory1", "QZW17111992QZW"));

            var users = brideForeverDataProvider.GetUsers().Result;

            //var emails = brideForeverDataProvider.GetSentEmailsData(users.First(user => user.ID == "115182"), new DateTime(2018, 4, 1), new DateTime(2018, 4, 1)).Result;
            var chats = brideForeverDataProvider.GetChats(new DateTime(2018, 7, 15), new DateTime(2018, 7, 15)).Result;
        }
    }
}
