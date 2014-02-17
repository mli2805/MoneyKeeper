using System;
using System.Collections.Generic;
using Keeper.Utils.Balances;

namespace Keeper.Utils.Rates
{
  public interface ICurrencyConverter {
    decimal BalancePairsToUsd(IEnumerable<MoneyPair> inCurrencies, DateTime dateTime);
  }
}