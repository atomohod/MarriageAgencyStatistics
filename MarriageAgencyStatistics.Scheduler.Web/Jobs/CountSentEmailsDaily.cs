﻿using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using MarriageAgencyStatistics.Common;
using MarriageAgencyStatistics.Core.DataProviders;
using MarriageAgencyStatistics.Core.Services;
using MarriageAgencyStatistics.DataAccess.EF;
using Newtonsoft.Json;

namespace MarriageAgencyStatistics.Scheduler.Web.Jobs
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

        protected override async Task ApplyUserUpdates(User user, DateTime today)
        {
            var emails =
                await _brideForeverService.GetCountOfSentEmails(new[] { user }, today, today);

            var existingRecord = await _context.UsersEmails.FirstOrDefaultAsync(userEmails =>
                userEmails.User.ID == user.ID && userEmails.Date == today);

            _context.UsersEmails.AddOrUpdate(new UserEmails
            {
                Id = existingRecord?.Id ?? Guid.NewGuid(),
                User = user,
                Date = today,
                Emails = emails.ToBytes()
            });
        }
    }
}