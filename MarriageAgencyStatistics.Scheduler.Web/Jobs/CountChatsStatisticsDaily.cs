using System;
using System.Threading.Tasks;
using MarriageAgencyStatistics.Common;
using MarriageAgencyStatistics.Core.Services;
using MarriageAgencyStatistics.DataAccess.EF;

namespace MarriageAgencyStatistics.Scheduler.Web.Jobs
{
    public class CountChatsStatisticsDaily : NoConcurrencyNoRetryJob
    {
        private readonly BrideForeverService _brideForeverService;
        private readonly BrideForeverDataContext _context;

        public CountChatsStatisticsDaily(BrideForeverService brideForeverService, BrideForeverDataContext context)
        {
            _brideForeverService = brideForeverService;
            _context = context;
        }

        protected async override Task ExecuteAsync()
        {
            var today = (DateTime.UtcNow - TimeSpan.FromDays(1)).ToStartOfTheDay();

            //var chats = _brideForeverService
        }
    }
}