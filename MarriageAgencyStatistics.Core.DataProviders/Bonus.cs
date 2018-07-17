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

    [Serializable]
    public class ChatItem
    {
        public string Id { get; set; }
        public string SentDate { get; set; }
        public string Sender { get; set; }
        public string Reciever { get; set; }
        public string Message { get; set; }
    }
}