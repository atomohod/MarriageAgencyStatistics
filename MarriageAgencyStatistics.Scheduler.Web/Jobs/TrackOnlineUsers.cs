﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using MarriageAgencyStatistics.Core.DataProviders;
using MarriageAgencyStatistics.DataAccess.EF;

namespace MarriageAgencyStatistics.Scheduler.Web.Jobs
{
    public class TrackOnlineUsers : NoConcurrencyNoRetryJob
    {
        private readonly BrideForeverDataProvider _brideForeverDataProvider;
        private readonly BrideForeverDataContext _context;

        public TrackOnlineUsers(BrideForeverDataProvider brideForeverDataProvider, BrideForeverDataContext context)
        {
            _brideForeverDataProvider = brideForeverDataProvider;
            _context = context;
        }

        protected override async Task ExecuteAsync()
        {
            var idsOnline = await _brideForeverDataProvider.GetUserIdsOnline();
            var users = await _context.Users.ToListAsync();

            foreach (var id in idsOnline)
            {
                _context.UsersOnline.Add(new UserOnline
                {
                    User = users.Find(user => user.ID == id),
                    Id = Guid.NewGuid(),
                    Online = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}