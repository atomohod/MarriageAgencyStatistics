using System;
using MarriageAgencyStatistics.Core.DataProviders;

namespace MarriageAgencyStatistics.DataAccess.EF
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