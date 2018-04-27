using System;

namespace MarriageAgencyStatistics.Core.DataProviders
{
    [Serializable]
    public class Bonus
    {
        public User User { get; set; }
        public decimal Today { get; set; }
        public decimal LastMonth { get; set; }
    }
}