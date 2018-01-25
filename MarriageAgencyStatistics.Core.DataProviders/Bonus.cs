namespace MarriageAgencyStatistics.Core.DataProviders
{
    public class Bonus
    {
        public User User { get; set; }
        public decimal Today { get; set; }
        public decimal LastMonth { get; set; }
    }
}