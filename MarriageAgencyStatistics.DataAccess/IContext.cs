using System;
using System.Linq;
using MarriageAgencyStatistics.Core.DataProviders;

namespace MarriageAgencyStatistics.DataAccess
{
    public interface IContext : IDisposable
    {
        IQueryable<User> UsersSet { get; }
        IQueryable<SelectedUser> SelectedUsersSet { get; set; }
        IQueryable<UserOnline> UsersOnlineSet { get; set; }
        IQueryable<UserEmails> UsersEmailsSet { get; set; }
        IQueryable<UserBonuses> UserBonusesSet { get; set; }
        IQueryable<UserChat> UserChatsSet { get; set; }
    }
}