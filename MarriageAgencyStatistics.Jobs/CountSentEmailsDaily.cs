using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Threading.Tasks;
using MarriageAgencyStatistics.Common;
using MarriageAgencyStatistics.Core.DataProviders;
using MarriageAgencyStatistics.Core.Services;
using MarriageAgencyStatistics.DataAccess;
using MarriageAgencyStatistics.DataAccess.EF;

namespace MarriageAgencyStatistics.Jobs
{
    public class CountSentEmailsDaily : UserBasedDailyJob<BrideForeverDataContext>
    {
        private readonly BrideForeverService _brideForeverService;

        public CountSentEmailsDaily(BrideForeverService brideForeverService, IDataContextProvider<BrideForeverDataContext> contextProvider) 
            : base(brideForeverService, contextProvider)
        {
            _brideForeverService = brideForeverService;
        }

        protected override async Task ApplyUserUpdatesAsync(BrideForeverDataContext context, User user, DateTime yesterday)
        {
            var emails =
                await _brideForeverService.GetCountOfSentEmails(new[] { user }, yesterday, yesterday);

            var existingRecord = await context.UsersEmails.FirstOrDefaultAsync(item => item.User.ID == user.ID && item.Date == yesterday);

            context.UsersEmails.AddOrUpdate(u => u.Id, new UserEmails
            {
                Id = existingRecord?.Id ?? Guid.NewGuid(),
                User = user,
                Date = yesterday,
                Emails = emails.ToBytes()
            });
        }
    }
}