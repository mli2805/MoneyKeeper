using System;
using System.Collections;
using System.Collections.Generic;

namespace Keeper.Utils
{
  public class Period
  {
    private readonly DateTime _start;
    private readonly DateTime _finish;

    public Period(DateTime start, DateTime finish)
    {
      _start = start;
      _finish = finish;
    }

    public bool IsDateTimeIn(DateTime checkDate)
    {
      return checkDate >= _start && checkDate <= _finish;
    }

    public DateTime GetStart()
    {
      return _start;
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
