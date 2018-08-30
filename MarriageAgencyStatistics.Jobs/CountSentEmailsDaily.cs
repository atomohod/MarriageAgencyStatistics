using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Threading.Tasks;
using MarriageAgencyStatistics.Common;
using MarriageAgencyStatistics.Core.DataProviders;
using MarriageAgencyStatistics.Core.Services;
using MarriageAgencyStatistics.DataAccess.EF;

namespace MarriageAgencyStatistics.Jobs
{
    public class CountSentEmailsDaily : UserBasedDailyJob
    {
        private readonly BrideForeverService _brideForeverService;
        private readonly BrideForeverDataContext _context;

        public CountSentEmailsDaily(BrideForeverService brideForeverService, BrideForeverDataContext context) 
            : base(brideForeverService, context)
        {
            _brideForeverService = brideForeverService;
            _context = context;
        }

        protected override async Task ApplyUserUpdatesAsync(User user, DateTime yesterday)
        {
            var emails =
                await _brideForeverService.GetCountOfSentEmails(new[] { user }, yesterday, yesterday);

            var existingRecord = await _context.UsersEmails.FirstOrDefaultAsync(item => item.User.ID == user.ID && item.Date == yesterday);

            _context.UsersEmails.AddOrUpdate(u => u.Id, new UserEmails
            {
                Id = existingRecord?.Id ?? Guid.NewGuid(),
                User = user,
                Date = yesterday,
                Emails = emails.ToBytes()
            });
        }
    }
}