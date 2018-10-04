using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MarriageAgencyStatistics.Common
{
    public static class Extensions
    {
        public static byte[] ToBytes<T>(this T obj)
        {
            if (obj == null)
                return null;

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, obj);
                return memoryStream.ToArray();
            }
        }

        public static T ToObject<T>(this byte[] param)
        {
            using (MemoryStream ms = new MemoryStream(param))
            {
                IFormatter br = new BinaryFormatter();
                return (T)br.Deserialize(ms);
            }
        }

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

        public static IEnumerable<DateTime> RangeTo(this DateTime fromDate, DateTime toDate)
        {
            return Enumerable.Range(0, toDate.Subtract(fromDate).Days + 1)
                .Select(d => fromDate.AddDays(d));
        }
    }
}
