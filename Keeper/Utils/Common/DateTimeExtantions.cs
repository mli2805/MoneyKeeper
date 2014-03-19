using System;
using Keeper.DomainModel;

namespace Keeper.Utils.Common
{
  public static class DateTimeExtantions
  {
    public static DateTime GetStartOfDate(this DateTime day)
    {
      var yy = day.Year;
      var mm = day.Month;
      var dd = day.Day;
      return new DateTime(yy, mm, dd, 0, 0, 0, 0);
    }

    public static DateTime GetStartOfMonthForDate(this DateTime day)
    {
      var yy = day.Year;
      var mm = day.Month;
      return new DateTime(yy, mm, 1, 0, 0, 0, 0);
    }

    public static DateTime GetEndOfDate(this DateTime day)
    {
      var yy = day.Year;
      var mm = day.Month;
      var dd = day.Day;
      return new DateTime(yy, mm, dd, 23, 59, 59, 999);
    }

    public static DateTime GetEndOfMonthForDate(this DateTime day)
    {
      var yy = day.Year;
      var mm = day.Month;
      return new DateTime(yy, mm, 1, 0, 0, 0, 0).AddMonths(1).AddSeconds(-1);
    }

    public static Period GetFullMonthForDate(this DateTime day)
    {
      return new Period(GetStartOfMonthForDate(day), GetStartOfMonthForDate(day).AddMonths(1).AddSeconds(-1));
    }

    public static Period GetPassedPartOfMonthWithFullThisDate(this DateTime day)
    {
      return new Period(GetStartOfMonthForDate(day), DateTime.Today.GetEndOfDate());
    }

  }

}
