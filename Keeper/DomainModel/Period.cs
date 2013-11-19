using System;
using System.Collections;
using Keeper.DomainModel;

namespace Keeper.Utils
{
  enum ShiftDay
  {
    BeforeThisDay,
    AsIs,
    AfterThisDay
  }

  public class Period
  {
    private readonly DateTime _start;
    private readonly DateTime _finish;

    public Period()
    {
      _start = new DateTime(0);
      _finish = DateTime.Today;
    }

    public Period(DateTime start, DateTime finish)
    {
        _start = start;
        _finish = finish;
    }

    #region Override == , != , Equals and GetHashCode

    public static bool operator ==(Period a, Period b)
    {
      // If both are null, or both are same instance, return true.
      if (ReferenceEquals(a, b)) return true;
      // If one is null, but not both, return false.
      if (((object)a == null) || ((object)b == null)) return false;
      return (a.GetStart() == b.GetStart() && a.GetFinish() == b.GetFinish());
    }

    public static bool operator !=(Period a, Period b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      if (object.ReferenceEquals(this, obj)) return true;
      var other = obj as Period;
      if (other == null) return false;
      return (this.GetStart() == other.GetStart());
    }

    public override int GetHashCode()
    {
      return GetStart().GetHashCode();
    }

    #endregion

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
