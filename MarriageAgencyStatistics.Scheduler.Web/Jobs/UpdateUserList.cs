﻿using System.Data.Entity.Migrations;
using System.Threading.Tasks;
using MarriageAgencyStatistics.Core.DataProviders;
using MarriageAgencyStatistics.Core.Services;
using MarriageAgencyStatistics.DataAccess.EF;

namespace MarriageAgencyStatistics.Scheduler.Web.Jobs
{
    public class UpdateUserList : NoConcurrencyNoRetryJob
    {
        private readonly BrideForeverService _brideForeverService;
        private readonly BrideForeverDataContext _context;

        public UpdateUserList(BrideForeverService brideForeverService, BrideForeverDataContext context)
        {
            _brideForeverService = brideForeverService;
            _context = context;
        }

        protected override async Task ExecuteAsync()
        {
            var users = await _brideForeverService.GetUsers();

            foreach (var user in users)
            {
                _context.Users.AddOrUpdate(new User
                {
                    ID = user.ID,
                    Name = $"{user.Name}"
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}