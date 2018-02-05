using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MarriageAgencyStatistics.Common
{
    public static class Extensions
    {
        public static string ToQueryString(this object obj)
        {
            var properties = from p in obj.GetType().GetProperties()
                             where p.GetValue(obj, null) != null
                             select p.Name + "=" + HttpUtility.UrlEncode(p.GetValue(obj, null).ToString());

            return String.Join("&", properties.ToArray());
        }

        public static string RemoveLast(this string s)
        {
            return string.IsNullOrEmpty(s) ? s : s.Remove(s.Length - 1, 1);
        }

        public static DateTime GetFirstDayOfTheMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        }

        public static DateTime GetLastDayOfTheMonth(this DateTime date)
        {
            var firstDayOfMonth = date.GetFirstDayOfTheMonth();
            return firstDayOfMonth.AddMonths(1).AddDays(-1);
        }

        public static DateTime ToStartOfTheDay(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
        }

        public static DateTime ToEndOfTheDay(this DateTime date)
        {
            var dayStart = date.ToStartOfTheDay();
            return dayStart.AddDays(1);
        }
    }
}
