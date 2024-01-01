using System;

namespace KeeperDomain
{
    public static class DateTimeExtensions
    {
        public static DateTime GetStartOfDate(this DateTime day)
        {
            var yy = day.Year;
            var mm = day.Month;
            var dd = day.Day;
            return new DateTime(yy, mm, dd, 0, 0, 0, 0);
        }

        public static DateTime GetStartOfMonth(this DateTime day)
        {
            var yy = day.Year;
            var mm = day.Month;
            return new DateTime(yy, mm, 1, 0, 0, 0, 0);
        }

        public static DateTime GetStartOfYear(this DateTime day)
        {
            var yy = day.Year;
            return new DateTime(yy, 1, 1, 0, 0, 0, 0);
        }

        public static DateTime GetEndOfDate(this DateTime day)
        {
            var yy = day.Year;
            var mm = day.Month;
            var dd = day.Day;
            return new DateTime(yy, mm, dd, 23, 59, 59, 999);
        }

        public static DateTime GetEndOfMonth(this DateTime day)
        {
            if (day == DateTime.MaxValue) return day;

            var yy = day.Year;
            var mm = day.Month;
            return new DateTime(yy, mm, 1, 0, 0, 0, 0).AddMonths(1).AddSeconds(-1);
        }

        public static DateTime GetEndOfYear(this DateTime day)
        {
            if (day == DateTime.MaxValue) return day;

            var yy = day.Year + 1;
            return new DateTime(yy, 1, 1, 0, 0, 0, 0).AddSeconds(-1);
        }

        public static int GetDaysInMonth(this DateTime day)
        {
            return new DateTime(day.Year, day.Month, 1, 0, 0, 0, 0).AddMonths(1).AddSeconds(-1).Day;
        }

        public static Period GetFullMonthForDate(this DateTime day)
        {
            return new Period(GetStartOfMonth(day), GetStartOfMonth(day).AddMonths(1).AddSeconds(-1));
        }

        public static Period GetPassedPartOfMonthWithFullThisDate(this DateTime day)
        {
            return new Period(GetStartOfMonth(day), DateTime.Today.GetEndOfDate());
        }

        public static bool IsMonthTheSame(this DateTime day, DateTime otherDay)
        {
            return (day.Year == otherDay.Year && day.Month == otherDay.Month);
        }

        public static int MonthsBetween(this DateTime lastDateTime, DateTime firstDateTime)
        {
            return lastDateTime.Year * 12 + lastDateTime.Month - (firstDateTime.Year * 12 + firstDateTime.Month) + 1;
        }
    }

}
