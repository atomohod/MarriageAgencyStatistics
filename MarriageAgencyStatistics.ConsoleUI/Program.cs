using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            var online = brideForeverDataProvider.GetUserIdsOnline().Result;

            var users = brideForeverDataProvider.GetUsers().Result;

            List<(User, Bonus)> userBonuses = new List<(User, Bonus)>();

            foreach (var user in users)
            {
                userBonuses.Add((user, brideForeverDataProvider.GetUserBonus(user).Result));
            }

            var excel = new BrideForeverExcel();

            excel.UpdateUserBonuses(userBonuses);
        }
    }
}
