using System.Data.Entity;
using MarriageAgencyStatistics.Core.DataProviders;

namespace MarriageAgencyStatistics.DataAccess.EF
{
    public class BrideForeverDataContext : DbContext
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
                .Entity<SelectedUser>()
                .HasKey(user => user.Id);
        }
    }
}
