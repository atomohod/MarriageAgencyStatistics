using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarriageAgencyStatistics.Common
{
    public static class Extensions
    {
        public static DateTime GetFirstDayOfTheMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

        public static DateTime GetLastDayOfTheMonth(this DateTime date)
        {
            var firstDayOfMonth = date.GetFirstDayOfTheMonth();
            return firstDayOfMonth.AddMonths(1).AddDays(-1);
        }

        public static DateTime ToStartOfTheDay(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day);
        }

        public static DateTime ToEndOfTheDay(this DateTime date)
        {
            var dayStart = date.ToStartOfTheDay();
            return dayStart.AddDays(1);
        }
    }
}
