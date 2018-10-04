using System;
using System.Data.Entity;
using System.Linq;
using MarriageAgencyStatistics.Core.DataProviders;

namespace MarriageAgencyStatistics.DataAccess.EF
{
    public class BrideForeverDataContextProvider : IDataContextProvider<BrideForeverDataContext>
    {
        public BrideForeverDataContext Create()
        {
            return new BrideForeverDataContext();
        }
    }

    public class BrideForeverDataContext : DbContext, IContext
    {
        public BrideForeverDataContext()
            : base("brideforever")
        {
        }

        public BrideForeverDataContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        public DbSet<SelectedUser> SelectedUsers { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserOnline> UsersOnline { get; set; }
        public DbSet<UserEmails> UsersEmails { get; set; }
        public DbSet<UserBonuses> UserBonuses { get; set; }
        public DbSet<UserChat> UserChats { get; set; }

        public IQueryable<User> UsersSet { get; set; }
        public IQueryable<SelectedUser> SelectedUsersSet { get; set; }
        public IQueryable<UserOnline> UsersOnlineSet { get; set; }
        public IQueryable<UserEmails> UsersEmailsSet { get; set; }
        public IQueryable<UserBonuses> UserBonusesSet { get; set; }
        public IQueryable<UserChat> UserChatsSet { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<User>()
                .HasKey(user => user.ID);

            modelBuilder
                .Entity<UserOnline>()
                .HasKey(online => online.Id);

            modelBuilder
                .Entity<UserEmails>()
                .HasKey(emails => emails.Id);

            modelBuilder
                .Entity<UserBonuses>()
                .HasKey(bonuses => bonuses.Id);

            modelBuilder
                .Entity<UserChat>()
                .HasKey(chat => chat.Id);

            modelBuilder
                .Entity<SelectedUser>()
                .HasKey(user => user.Id);
        }
    }
}
