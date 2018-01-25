namespace MarriageAgencyStatistics.DataTransferModels
{
    public class UserOnlineStatisticsModel
    {
        public UserModel User { get; set; }
        public double Online { get; set; }
        public int TotalMinutesOnline { get; set; }
    }
}