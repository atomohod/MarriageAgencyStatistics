using System;
using MarriageAgencyStatistics.Domain.BrideForever;

namespace MarriageAgencyStatistics.DataAccess
{
    //TODO this should be stored in AzureTable
    //TODO create model for data access
    public class UserOnline
    {
        public Guid Id { get; set; }
        public User User { get; set; }
        public DateTime Online { get; set; }
    }
}