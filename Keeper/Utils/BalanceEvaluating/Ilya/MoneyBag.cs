using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Keeper.DomainModel.Enumes;

namespace Keeper.Utils.BalanceEvaluating.Ilya
{
  public sealed class MoneyBag : ReadOnlyCollection<Money>
  {
    public MoneyBag(IEnumerable<Money> money)
      : base(money.ToArray())
    {
    }
    public MoneyBag(params Money[] money)
      : base(money)
    {
    }

    public decimal this[CurrencyCodes index]
    {
      get
      {
        var m = this.FirstOrDefault(c => c.Currency == index);
        return m == null ? 0 : m.Amount;
      }
    }

    public bool IsEmpty { get { return Count == 0; } }

      public bool IsZero() // записи есть, но все с нулем
      {
          return Items.All(item => item.Amount == 0);
      }

      public static MoneyBag operator -(MoneyBag x)
    {
      return new MoneyBag(x.Select(m => new Money(m.Currency, -m.Amount)));
    }
    public static MoneyBag operator -(MoneyBag x, MoneyBag y)
    {
      return x + (-y);
    }
    public static MoneyBag operator +(MoneyBag x, MoneyBag y)
    {
      return y == null ?
                x :
                new MoneyBag(x
                            .Concat(y)
                            .GroupBy(m => m.Currency)
                            .Select(Sum)
                            .Where(m => m.Amount != 0));
    }
    static Money Sum(IEnumerable<Money> source)
    {
      var list = source.ToList();
      if (list.Count == 0) throw new Exception("Empty sequence");
      if (list.Count == 1) return list[0];
      if (list.Select(i => i.Currency).Distinct().Count() != 1)
        throw new Exception("Cannot sum different currencies");
      return new Money(list[0].Currency, list.Sum(i => i.Amount));
    }

    public static MoneyBag operator +(MoneyBag x, Money y)
    {
      return x + new MoneyBag(y);
    }

      public static MoneyBag operator -(MoneyBag x, Money y)
      {
          return x - new MoneyBag(y);
      }

    public override string ToString()
    {
      return string.Join(", ", this);
    }
  }
}