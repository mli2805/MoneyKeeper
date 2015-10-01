using System;

namespace Keeper.Utils.Common
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

        public YearMonth AddMonth(int mm)
        {
            int newMonth = Month + mm;
            int newYear = Year;
            if (newMonth > 12) newYear = newYear + newMonth/12;
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

        public static YearMonth operator -(YearMonth instance1, YearMonth instance2)
        {
            return instance2.Month >= instance1.Month ? 
                new YearMonth(instance1.Year - instance2.Year - 1, instance1.Month + 12 - instance2.Month) :
                new YearMonth(instance1.Year - instance2.Year, instance1.Month - instance2.Month) ;
        }


        public int CompareTo(YearMonth other)
        {
            return (Year*12 + Month).CompareTo(other.Year*12 + other.Month);
        }
    }
}
