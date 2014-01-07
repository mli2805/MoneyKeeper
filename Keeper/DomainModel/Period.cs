using System;
using System.Collections;

namespace Keeper.DomainModel
{
	public class Period
  {
	  public Period()
    {
      Start = new DateTime(0);
      Finish = DateTime.Today;
    }

    public Period(DateTime start, DateTime finish)
    {
        Start = start;
        Finish = finish;
    }

    #region Override == , != , Equals and GetHashCode

    public static bool operator ==(Period a, Period b)
    {
      // If both are null, or both are same instance, return true.
      if (ReferenceEquals(a, b)) return true;
      // If one is null, but not both, return false.
      if (((object)a == null) || ((object)b == null)) return false;
      return (a.Start == b.Start && a.Finish == b.Finish);
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
      return (this.Start == other.Start);
    }

    public override int GetHashCode()
    {
      return Start.GetHashCode();
    }

    #endregion

    public bool IsDateTimeIn(DateTime checkDate)
    {
      return checkDate >= Start && checkDate <= Finish;
    }

    public bool IsDateIn(DateTime checkDate)
    {
      return checkDate.Date >= Start.Date && checkDate.Date <= Finish.Date;
    }

	  public DateTime Start { get; private set; }
	  public DateTime Finish { get; private set; }

	  public IEnumerator GetEnumerator()
    {
      for (var dt = Start; dt <= Finish; dt = dt.AddDays(1))
      {
        yield return dt;
      }
    }
  
  }
}
