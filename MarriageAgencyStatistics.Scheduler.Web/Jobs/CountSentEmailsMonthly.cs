using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using MarriageAgencyStatistics.Common;
using MarriageAgencyStatistics.Core.DataProviders;
using MarriageAgencyStatistics.Core.Services;
using MarriageAgencyStatistics.DataAccess.EF;

namespace MarriageAgencyStatistics.Scheduler.Web.Jobs
{
    public class CountSentEmailsMonthly : NoConcurrencyNoRetryJob
    {
        private readonly BrideForeverService _brideForeverService;
        private readonly BrideForeverDataContext _context;

        public CountSentEmailsMonthly(BrideForeverService brideForeverService, BrideForeverDataContext context)
        {
            _brideForeverService = brideForeverService;
            _context = context;
        }

        protected override async Task ExecuteAsync()
        {
            var users = await _brideForeverService.GetUsers();

            var fromDay = DateTime.UtcNow.GetFirstDayOfTheMonth();
            var toDay = DateTime.UtcNow.ToStartOfTheDay();

            var enumeratedUsers = users as User[] ?? users.ToArray();

            do
            {
                foreach (var user in enumeratedUsers)
                {
                    var contextUser = _context.Users.First(u => u.ID == user.ID);
                    var emails =
                        await _brideForeverService.GetCountOfSentEmails(new[] { user }, fromDay, fromDay);

                    var existingRecord = await _context.UsersEmails.FirstOrDefaultAsync(userEmails =>
                        userEmails.User.ID == user.ID && userEmails.Date == fromDay);

                    _context.UsersEmails.AddOrUpdate(new UserEmails
                    {
                        Id = existingRecord?.Id ?? Guid.NewGuid(),
                        User = contextUser,
                        Date = fromDay,
                        Emails = emails.ToBytes()
                    });
                }

                fromDay = fromDay + TimeSpan.FromDays(1);

                await _context.SaveChangesAsync();
            } while (fromDay < toDay);
        }
    }
}