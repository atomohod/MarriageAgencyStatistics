using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Threading.Tasks;
using MarriageAgencyStatistics.Common;
using MarriageAgencyStatistics.Core.DataProviders;
using MarriageAgencyStatistics.Core.Services;
using MarriageAgencyStatistics.DataAccess.EF;

namespace MarriageAgencyStatistics.Scheduler.Web.Jobs
{
    public class CountSentEmailsMonthly : UserBasedMonthlyJob
    {
        private readonly BrideForeverService _brideForeverService;
        private readonly BrideForeverDataContext _context;

        public CountSentEmailsMonthly(BrideForeverService brideForeverService, BrideForeverDataContext context) 
            : base(brideForeverService, context)
        {
            _brideForeverService = brideForeverService;
            _context = context;
        }
        
        protected override async Task ApplyUserUpdates(User user, DateTime currentDay)
        {
            var emails =
                await _brideForeverService.GetCountOfSentEmails(new[] { user }, currentDay, currentDay);

            var existingRecord = await _context.UsersEmails.FirstOrDefaultAsync(userEmails =>
                userEmails.User.ID == user.ID && userEmails.Date == currentDay);

            _context.UsersEmails.AddOrUpdate(new UserEmails
            {
                Id = existingRecord?.Id ?? Guid.NewGuid(),
                User = user,
                Date = currentDay,
                Emails = emails.ToBytes()
            });
        }
    }
}