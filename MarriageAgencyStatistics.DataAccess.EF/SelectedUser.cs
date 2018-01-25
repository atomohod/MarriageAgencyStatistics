using System;
using MarriageAgencyStatistics.Core.DataProviders;

namespace MarriageAgencyStatistics.DataAccess.EF
{
    public class SelectedUser
    {
        public Guid Id { get; set; }
        public User User { get; set; }
    }
}