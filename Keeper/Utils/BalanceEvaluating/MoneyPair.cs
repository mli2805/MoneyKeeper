using System;
using Keeper.DomainModel.Enumes;

namespace Keeper.Utils.BalanceEvaluating
{
  public class MoneyPair : IComparable
  {
    public CurrencyCodes Currency { get; set; }
    public decimal Amount { get; set; }

    public new string ToString()
    {
      return String.Format("{0:#,0} {1}", Amount, Currency.ToString().ToLower());
    }

    public int CompareTo(object obj)
    {
      return Currency.CompareTo(((MoneyPair)obj).Currency);
    }
  }
}