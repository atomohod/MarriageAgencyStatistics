using System;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using MarriageAgencyStatistics.Core.DataProviders;
using MarriageAgencyStatistics.Core.Services;
using MarriageAgencyStatistics.DataAccess.EF;

namespace MarriageAgencyStatistics.Scheduler.Web.Jobs
{
    public class CountSentEmails : NoConcurrencyNoRetryJob
    {
        private readonly BrideForeverService _brideForeverService;
        private readonly BrideForeverDataContext _context;

        public CountSentEmails(BrideForeverService brideForeverService, BrideForeverDataContext context)
        {
            _brideForeverService = brideForeverService;
            _context = context;
        }

        protected override async Task ExecuteAsync()
        {
            var users = await _brideForeverService.GetUsers();
            foreach (var user in users)
            {
                var emails = await _brideForeverService.GetCountOfSentEmails(new[] { user }, DateTime.UtcNow, DateTime.UtcNow);
            }

            _context.UsersEmails.AddOrUpdate(new UserEmails
            {

            });

            await _context.SaveChangesAsync();
        }
    }
}