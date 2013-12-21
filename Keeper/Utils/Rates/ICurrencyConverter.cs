using System;
using System.Collections.Generic;

namespace Keeper.Utils
{
  public interface ICurrencyConverter {
    decimal BalancePairsToUsd(IEnumerable<MoneyPair> inCurrencies, DateTime dateTime);
  }
}