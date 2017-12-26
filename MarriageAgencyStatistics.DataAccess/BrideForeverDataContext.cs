using MarriageAgencyStatistics.Domain.BrideForever;
using Microsoft.EntityFrameworkCore;

namespace MarriageAgencyStatistics.DataAccess
{
    public class BrideForeverDataContext : DbContext
    {
        public BrideForeverDataContext(DbContextOptions options) 
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserOnline> UsersOnline { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<User>()
                .HasKey(user => user.ID);

            modelBuilder
                .Entity<UserOnline>()
                .HasKey(online => online.Id);
        }
    }
}
