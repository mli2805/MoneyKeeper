using System;
using System.Collections;

namespace Keeper.Utils
{
  public class Period
  {
    private readonly DateTime _start;
    private readonly DateTime _finish;

    private DateTime GetDayBegin(DateTime day)
    {
      var yy = day.Year;
      var mm = day.Month;
      var dd = day.Day;
      return new DateTime(yy, mm, dd, 0, 0, 0, 0);
    }

    public Period()
    {
      _start = new DateTime(0);
      _finish = DateTime.Today;
    }

    public Period(DateTime start, DateTime finish, bool extendToMidnight)
    {
      if (extendToMidnight)
      {
        _start = GetDayBegin(start);
        _finish = GetDayBegin(finish.AddDays(1)).AddMilliseconds(-1);
      }
      else
      {
        _start = start;
        _finish = finish;
      }
    }

    public bool IsDateTimeIn(DateTime checkDate)
    {
      return checkDate >= _start && checkDate <= _finish;
    }

    public DateTime GetStart()
    {
      return _start;
    }

    public DateTime GetFinish()
    {
      return _finish;
    }

    public IEnumerator GetEnumerator()
    {
      for (var dt = _start; dt <= _finish; dt = dt.AddDays(1))
      {
        yield return dt;
      }
    }
  
  }
}
