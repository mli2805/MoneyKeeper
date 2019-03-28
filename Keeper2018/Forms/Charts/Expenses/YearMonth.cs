using System;

namespace Keeper2018
{
    public class YearMonth : IComparable<YearMonth>
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public YearMonth(int year, int month)
        {
            Year = year;
            Month = month;
        }

        public YearMonth(DateTime date)
        {
            Year = date.Year;
            Month = date.Month;
        }

        public YearMonth AddMonth(int mm)
        {
            int newMonth = Month + mm;
            int newYear = Year;
            if (newMonth > 12)
            {
                newYear = newYear + newMonth / 12;
                newMonth = newMonth % 12;
                if (newMonth == 0)
                {
                    newYear--;
                    newMonth = 12;
                }
            }
            else if (newMonth < 1)
            {
                newYear = newYear - newMonth/12 - 1;
                newMonth = newMonth + (newMonth/12 + 1)*12;
            }
            return new YearMonth(newYear, newMonth);
        }

        public YearMonth AddYear(int yy)
        {
            return new YearMonth(Year + yy, Month);
        }

        public DateTime LastDay()
        {
            var temp = AddMonth(1);
            return new DateTime(temp.Year, temp.Month, 1).AddDays(-1);
        }

        public static YearMonth operator -(YearMonth instance1, YearMonth instance2)
        {
            return instance2.Month >= instance1.Month ? 
                new YearMonth(instance1.Year - instance2.Year - 1, instance1.Month + 12 - instance2.Month) :
                new YearMonth(instance1.Year - instance2.Year, instance1.Month - instance2.Month) ;
        }

        public static bool operator <(YearMonth instance1, YearMonth instance2)
        {
            return instance1.Year < instance2.Year || (instance1.Year == instance2.Year && instance1.Month < instance2.Month);
        }
        public static bool operator >(YearMonth instance1, YearMonth instance2)
        {
            return instance1.Year > instance2.Year || (instance1.Year == instance2.Year && instance1.Month > instance2.Month);
        }

        /// <summary>
        /// если finish == start возвращает 1 месяц
        /// </summary>
        /// <param name="finish"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public static int PeriodInMonths(YearMonth finish, YearMonth start)
        {
            var sub = finish - start;
            return sub.Year*12 + sub.Month + 1;
        }
        public int CompareTo(YearMonth other)
        {
            return (Year*12 + Month).CompareTo(other.Year*12 + other.Month);
        }


        public override string ToString()
        {
            var date = new DateTime(Year, Month, 1);
            return date.ToString("MMMM yyyy");
        }

        public static string IntervalToString(Tuple<YearMonth, YearMonth> tuple)
        {
            return $"{tuple.Item1} - {tuple.Item2}";
        }

        public bool InInterval(Tuple<YearMonth, YearMonth> interval) { return !(this < interval.Item1 || this > interval.Item2);}
    }
}