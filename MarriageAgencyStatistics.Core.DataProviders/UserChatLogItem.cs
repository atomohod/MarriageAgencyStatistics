using System;
using System.Runtime.Serialization;

namespace MarriageAgencyStatistics.Core.DataProviders
{
    public class UserChatLogItem
    {
        public User User { get; set; }
        public DateTime SentOn { get; set; }
        public string Name { get; set; }

        [IgnoreDataMember]
        public bool SentByUser => Name.ToLowerInvariant().Contains(User.FirstName.ToLowerInvariant());
    }
}