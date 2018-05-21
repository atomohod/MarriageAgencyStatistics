namespace MarriageAgencyStatistics.Applications.Models
{
    public class UserOnlineStatistics
    {
        public User User { get; set; }
        public double Online { get; set; }
        public int TotalMinutesOnline { get; set; }
    }
}