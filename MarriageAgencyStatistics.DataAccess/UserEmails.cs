using System;
using MarriageAgencyStatistics.Core.DataProviders;

namespace MarriageAgencyStatistics.DataAccess
{
    public class UserEmails
    {
        public Guid Id { get; set; }
        public User User { get; set; }
        public DateTime Date { get; set; }
        public byte[] Emails { get; set; }
    }
}