using System;

namespace Keeper.DomainModel
{
  public class RegularPayment
  {
    public String Comment;
    public Decimal Amount;
    public CurrencyCodes Currency;
    public bool ShouldBeReminded;
    public int DayOfMonth;
  }
}
