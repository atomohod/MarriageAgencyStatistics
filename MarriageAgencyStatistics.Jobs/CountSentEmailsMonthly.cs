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
    public class CountSentEmailsMonthly : UserBasedMonthlyJob<BrideForeverDataContext>
    {
        private readonly BrideForeverService _brideForeverService;

        public CountSentEmailsMonthly(BrideForeverService brideForeverService, IDataContextProvider<BrideForeverDataContext> contextProvider) 
            : base(brideForeverService, contextProvider)
        {
            _brideForeverService = brideForeverService;
        }

        protected override string OperationName => "Count sent emails monthly";

        protected override async Task ApplyUserUpdates(BrideForeverDataContext context, User user, DateTime currentDay)
        {
            var emails =
                await _brideForeverService.GetCountOfSentEmails(new[] { user }, currentDay, currentDay);

            var existingRecord = await context.UsersEmails.FirstOrDefaultAsync(userEmails =>
                userEmails.User.ID == user.ID && userEmails.Date == currentDay);

            context.UsersEmails.AddOrUpdate(u => u.Id, new UserEmails
            {
                Id = existingRecord?.Id ?? Guid.NewGuid(),
                User = user,
                Date = currentDay,
                Emails = emails.ToBytes()
            });
        }
    }
}