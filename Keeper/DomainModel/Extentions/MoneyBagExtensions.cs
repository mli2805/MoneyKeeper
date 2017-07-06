using System;
using System.Collections.Generic;
using System.Linq;
using Keeper.DomainModel.WorkTypes;

namespace Keeper.DomainModel.Extentions
{
  public static class MoneyBagExtensions
  {
    public static MoneyBag Sum<T>(this IEnumerable<T> source, Func<T, MoneyBag> selector)
    {
      return source.Select(selector).Aggregate(new MoneyBag(), (a, b) => a + b);
    }
  }
}