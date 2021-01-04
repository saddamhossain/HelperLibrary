using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelperLibrary
{
    public static class DateTimeHelper
    {
        public static DateTime FirstDayOfMonth(this DateTime date)
        {
            return new DateTime(date.AddMonths(-1).Year, date.AddMonths(-1).Month, 1);
        }

        public static DateTime LastDayOfMonth(this DateTime date)
        {
            return date.FirstDayOfMonth().AddMonths(1).AddDays(-1);
        }

        public static int DaysInMonth(this DateTime date)
        {
            return DateTime.DaysInMonth(date.Year, date.Month);
        }

        public static DateTime ToDateTime(this object obj)
        {
            try
            {
                return Convert.ToDateTime(obj);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        public static DateTime ToDateTime(this string text, string format)
        {
            try
            {
                return DateTime.ParseExact(text, format, CultureInfo.InvariantCulture, DateTimeStyles.None);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        public static int MonthDifference(this DateTime now, DateTime date)
        {
            return ((now.Month - date.Month) + (12 * (now.Year - date.Year)));
        }

        public static int DayDifference(this DateTime now, DateTime date)
        {
            return (int)(now.Date - date.Date).TotalDays;
        }

        public static int YearDifference(this DateTime startDate, DateTime endDate)
        {
            int age = (endDate.Year - startDate.Year);
            if (startDate > endDate.AddYears(-age))
                age--;

            return age;
        }

        public static bool IsDayEquals(this DateTime now, DateTime date)
        {
            return (now.Date == date.Date);
        }

        public static bool IsMonthEquals(this DateTime now, DateTime date)
        {
            return (now.Year == date.Year && now.Month == date.Month);
        }

        public static bool IsYearEquals(this DateTime now, DateTime date)
        {
            return (now.Year == date.Year);
        }

        public static string ToRelativeFormat(this DateTime source)
        {
            string result = string.Empty;

            var ts = new TimeSpan(DateTime.Now.Ticks - source.Ticks);
            double delta = ts.TotalSeconds;

            if (delta > 0)
            {
                if (delta < 60) // 60 (seconds)
                {
                    result = ts.Seconds == 1 ? "bir saniye önce" : ts.Seconds + " saniye önce";
                }
                else if (delta < 120) //2 (minutes) * 60 (seconds)
                {
                    result = "1 dakika önce";
                }
                else if (delta < 2700) // 45 (minutes) * 60 (seconds)
                {
                    result = ts.Minutes + " dakika önce";
                }
                else if (delta < 5400) // 90 (minutes) * 60 (seconds)
                {
                    result = "bir saat önce";
                }
                else if (delta < 86400) // 24 (hours) * 60 (minutes) * 60 (seconds)
                {
                    int hours = ts.Hours;
                    if (hours == 1)
                        hours = 2;
                    result = hours + " saat önce";
                }
                else if (delta < 172800) // 48 (hours) * 60 (minutes) * 60 (seconds)
                {
                    result = "dün";
                }
                else if (delta < 2592000) // 30 (days) * 24 (hours) * 60 (minutes) * 60 (seconds)
                {
                    result = ts.Days + " gün önce";
                }
                else if (delta < 31104000) // 12 (months) * 30 (days) * 24 (hours) * 60 (minutes) * 60 (seconds)
                {
                    int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                    result = months <= 1 ? "bir ay önce" : months + " ay önce";
                }
                else
                {
                    int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
                    if (years <= 1)
                        result = "bir yıl önce";
                    else
                        result = source.ToString();

                    //result = years <= 1 ? "bir yıl önce" : years + " yıl önce";
                }
            }
            return result;
        }

        public static ReadOnlyCollection<TimeZoneInfo> GetSystemTimeZones()
        {
            return TimeZoneInfo.GetSystemTimeZones();
        }

        public static DateTime ConvertToUtcTime(DateTime dt)
        {
            return ConvertToUtcTime(dt, dt.Kind);
        }

        public static DateTime ConvertToUtcTime(DateTime dt, DateTimeKind sourceDateTimeKind)
        {
            dt = DateTime.SpecifyKind(dt, sourceDateTimeKind);
            return TimeZoneInfo.ConvertTimeToUtc(dt);
        }

        public static DateTime ConvertToUtcTime(DateTime dt, TimeZoneInfo sourceTimeZone)
        {
            if (sourceTimeZone.IsInvalidTime(dt))
            {
                //could not convert
                return dt;
            }

            return TimeZoneInfo.ConvertTimeToUtc(dt, sourceTimeZone);
        }
    }
}
