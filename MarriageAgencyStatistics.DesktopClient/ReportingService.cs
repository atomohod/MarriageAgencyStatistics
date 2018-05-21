using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarriageAgencyStatistics.Applications;
using MarriageAgencyStatistics.Applications.Models;
using MarriageAgencyStatistics.Common;

namespace MarriageAgencyStatistics.DesktopClient
{
    public class BrideForeverReportingService
    {
        private readonly BrideForeverApp _app;

        public BrideForeverReportingService(BrideForeverApp app)
        {
            _app = app;
        }

        public void GetMonthlyReport()
        {
            DateTime today = DateTime.UtcNow;
            DateTime firstDayOfTheMonth = today.GetFirstDayOfTheMonth();

            var selectedUsers = _app
                .GetSelectedUsers()
                .Result
                .Select(user => user.Title)
                .ToArray();

            var current = firstDayOfTheMonth;

            List<(DateTime, List<UserSentEmailsStatistics>, List<UserOnlineStatistics>)> result = new List<(DateTime, List<UserSentEmailsStatistics>, List<UserOnlineStatistics>)>();

            do
            {
                var emails = _app.GetSentEmails(current, selectedUsers).Result;
                var statistics = _app.GetStatistics(current, selectedUsers).Result;

                result.Add((current, emails, statistics));

                current = current + TimeSpan.FromDays(1);

            } while (current <= today);

        }
    }
}
