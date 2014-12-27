using System;
using System.Collections.Generic;
using System.Linq;

namespace Keeper.ByFunctional.BalanceEvaluating.Ilya
{
  public static class MoneyExtensions
  {
    public static MoneyBag Sum<T>(this IEnumerable<T> source, Func<T, Money> selector)
    {
      return new MoneyBag(source.Select(selector)
                            .GroupBy(m => m.Currency)
                            .Select(mm => new Money(mm.Key, mm.Sum(m => m.Amount)))
                            .Where(m => m.Amount != 0));
    }
		
    public static MoneyBag Sum<T>(this IEnumerable<T> source, Func<T, MoneyBag> selector)
    {
//			return new MoneyBag(source.Select(selector)
//				.SelectMany(m => m)
//			    .GroupBy(m => m.Currency)
//			    .Select(mm => new Money(mm.Key, mm.Sum(m => m.Amount)))
//			    .Where(m => m.Amount != 0));

      return source.Select(selector).Aggregate(new MoneyBag(), (a, b) => a + b);
    }
  }
}