using System.Data.Entity.Migrations;
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
            var users = await _brideForeverDataProvider.GetUsers();

            using (var context = _contextProvider.Create())
            {
                foreach (var user in users)
                {
                    context.Users.AddOrUpdate(new User
                    {
                        ID = user.ID,
                        Name = $"{user.Name}"
                    });
                }

                await context.SaveChangesAsync();
            }
        }
    }
}