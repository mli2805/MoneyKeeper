using System;

namespace Keeper.Utils.Common
{
  class DayProcessor
  {
    private readonly DateTime _day;

    public DayProcessor(DateTime day)
    {
      _day = day;
    }

    public DateTime BeforeThisDay()
    {
      var yy = _day.Year;
      var mm = _day.Month;
      var dd = _day.Day;
      return new DateTime(yy, mm, dd, 0, 0, 0, 0);
    }

    public DateTime BeforeThisMonth()
    {
      var yy = _day.Year;
      var mm = _day.Month;
      var dd = _day.Day;
      return new DateTime(yy, mm, 1, 0, 0, 0, 0);
    }

    public DateTime AfterThisDay()
    {
      var yy = _day.Year;
      var mm = _day.Month;
      var dd = _day.Day;
      return new DateTime(yy, mm, dd, 23, 59, 59, 999);
    }
  }
}
