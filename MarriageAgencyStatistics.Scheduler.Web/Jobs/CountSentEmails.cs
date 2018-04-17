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

            var countDay = DateTime.UtcNow.ToStartOfTheDay();

            foreach (var user in users)
            {
                var emails =
                    await _brideForeverService.GetCountOfSentEmails(new[] { user }, countDay, countDay);

                var existingRecord = await _context.UsersEmails.FirstOrDefaultAsync(userEmails =>
                    userEmails.User.ID == user.ID && userEmails.Date == countDay);
                
                    _context.UsersEmails.AddOrUpdate(new UserEmails
                    {
                        Id = existingRecord?.Id ?? Guid.NewGuid(),
                        User = user,
                        Date = countDay,
                        Emails = emails.ToBytes()
                    });
            }

            await _context.SaveChangesAsync();
        }
    }
}