using System;
using System.Collections.Generic;

namespace MarriageAgencyStatistics.Core.DataProviders
{
    public class UserCommunications
    {
        public UserCommunications(User user, IEnumerable<SentEmailData> sentEmails)
        {
            User = user;
            SentEmails = sentEmails;
        }

        public User User { get; }
        public IEnumerable<SentEmailData> SentEmails { get; }
    }

    public class SentEmailData
    {
        public static SentEmailData Read(DateTime whenWasSent)
        {
            return new SentEmailData
            {
                WasSent = whenWasSent,
                IsRead = true
            };
        }

        public static SentEmailData NotRead(DateTime whenWasSent)
        {
            return new SentEmailData
            {
                WasSent = whenWasSent,
                IsRead = false
            };
        }

        private SentEmailData()
        {
        }

        public DateTime WasSent { get; private set; }
        public bool IsRead { get; private set; }
    }
}