using System.Collections.Generic;

namespace MarriageAgencyStatistics.Core.DataProviders
{
    public class UserChatStatistic
    {
        public User User { get; set; }
        public int ChatInvatationsCount { get; set; }

        public IDictionary<string, (int, double)> PhrasesStatistics { get; set; }
    }
}