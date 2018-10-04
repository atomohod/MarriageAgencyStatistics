using System;
using MarriageAgencyStatistics.Core.DataProviders;

namespace MarriageAgencyStatistics.DataAccess
{
    public class UserBonuses
    {
        public Guid Id { get; set; }
        public User User { get; set; }
        public DateTime Date { get; set; }
        public byte[] Bonuses { get; set; }
    }
}