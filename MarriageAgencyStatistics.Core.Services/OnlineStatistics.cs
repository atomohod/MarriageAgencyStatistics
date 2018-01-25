using MarriageAgencyStatistics.Core.DataProviders;

namespace MarriageAgencyStatistics.Core.Services
{
    //TODO move out here
    public class OnlineStatistics
    {
        public User User { get; set; }
        public double PercentageOnline { get; set; }
        public int TotalMinutesOnline { get; set; }
    }
}