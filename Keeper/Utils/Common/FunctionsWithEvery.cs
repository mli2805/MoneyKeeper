using System;

namespace Keeper.Utils.Common
{
	internal class FunctionsWithEvery
  {
    public static bool IsLastDayOf(DateTime date, Every period)
    {
      if (period == Every.Day) return true;
      if (period == Every.Week && date.DayOfWeek == DayOfWeek.Sunday) return true;
      if (period == Every.Month && date.Month != date.AddDays(1).Month) return true;
      if (period == Every.Quarter && date.Month != date.AddDays(1).Month && date.Month%3 == 0) return true;
      if (period == Every.Year && date.Day == 31 && date.Month == 12) return true;
      return false;
    }

    public static DateTime GetLastDayOfTheSamePeriod(DateTime date, Every period)
    {
      if (period == Every.Day) return date;
      if (period == Every.Week)
      {
        while (date.DayOfWeek != DayOfWeek.Sunday) date = date.AddDays(1);
        return date;
      }
      if (period == Every.Month)
      {
        date = date.AddMonths(1);
        return new DateTime(date.Year, date.Month, 1).AddDays(-1);
      }
      if (period == Every.Quarter)
      {
        date = date.AddMonths(3 - (date.Month - 1) % 3);
        return new DateTime(date.Year, date.Month, 1).AddDays(-1);
      }
      if (period == Every.Year) return new DateTime(date.Year +1, 12, 31);
      throw new Exception("Такого периода не бывает!");
    }

    public static int WeekNumber(DateTime date)
    {
      int weekNumber = date.DayOfYear/7;
      if ((int) date.DayOfWeek < date.DayOfYear%7) weekNumber++;
      return weekNumber;
    }

    public static int QuarterNumber(DateTime date)
    {
      return (date.Month - 1)/3 + 1;
    }

    public static bool IsTheSamePeriod(DateTime date1, DateTime date2, Every period)
    {
      if (period == Every.Day) return date1.Date == date2.Date;
      if (period == Every.Week) return WeekNumber(date1) == WeekNumber(date2);
      if (period == Every.Month) return date1.Year == date2.Year && date1.Month == date2.Month;
      if (period == Every.Quarter) return date1.Year == date2.Year && QuarterNumber(date1) == QuarterNumber(date2);
      if (period == Every.Year) return date1.Year == date2.Year;
      
      throw new Exception("Такого периода не бывает!");
    }
  }
}