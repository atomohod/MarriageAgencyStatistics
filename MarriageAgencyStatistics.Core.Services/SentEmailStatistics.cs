using System;
using MarriageAgencyStatistics.Core.DataProviders;

namespace MarriageAgencyStatistics.Core.Services
{
    [Serializable]
    public class SentEmailStatistics
    {
        public User User { get; set; }
        public int SentEmails { get; set; }
    }
}