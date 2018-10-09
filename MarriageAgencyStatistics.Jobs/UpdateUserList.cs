using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using MarriageAgencyStatistics.Core.DataProviders;
using MarriageAgencyStatistics.DataAccess;
using MarriageAgencyStatistics.DataAccess.EF;

namespace MarriageAgencyStatistics.Jobs
{
    public class UpdateUserList : NoConcurrencyNoRetryJob
    {
        private readonly BrideForeverDataProvider _brideForeverDataProvider;
        private readonly IDataContextProvider<BrideForeverDataContext> _contextProvider;

        public UpdateUserList(BrideForeverDataProvider brideForeverDataProvider, IDataContextProvider<BrideForeverDataContext> contextProvider)
        {
            _brideForeverDataProvider = brideForeverDataProvider;
            _contextProvider = contextProvider;
        }

        protected override async Task ExecuteAsync()
        {
            using (var context = _contextProvider.Create())
            {
                await UpdateActiveUsers(context);
                await UpdateInActiveUsers(context);
                await UpdateHiddenUsers(context);

                await context.SaveChangesAsync();
            }
        }

        private async Task UpdateActiveUsers(BrideForeverDataContext context)
        {
            var users = await _brideForeverDataProvider.GetActiveUsers();
            
            foreach (var user in users)
            {
                context.Users.AddOrUpdate(new User
                {
                    ID = user.ID,
                    UserMode = UserMode.Active,
                    Name = $"{user.Name}"
                });
            }
        }

        private async Task UpdateInActiveUsers(BrideForeverDataContext context)
        {
            var users = await _brideForeverDataProvider.GetInactiveUsers();

            foreach (var user in users)
            {
                context.Users.AddOrUpdate(new User
                {
                    ID = user.ID,
                    UserMode = UserMode.Inactive,
                    Name = $"{user.Name}"
                });
            }
        }

        private async Task UpdateHiddenUsers(BrideForeverDataContext context)
        {
            var users = await _brideForeverDataProvider.GetSilentUsers();

            foreach (var user in users)
            {
                context.Users.AddOrUpdate(new User
                {
                    ID = user.ID,
                    UserMode = UserMode.Silent,
                    Name = $"{user.Name}"
                });
            }
        }
    }
}