using System;

namespace Keeper.Utils
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

    public DateTime AfterThisDay()
    {
      var yy = _day.Year;
      var mm = _day.Month;
      var dd = _day.Day;
      return new DateTime(yy, mm, dd, 0, 0, 0, 0).AddDays(1);
    }
  }
}
