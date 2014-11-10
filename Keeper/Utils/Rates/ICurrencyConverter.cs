using System;
using System.Collections.Generic;
using Keeper.ByFunctional.EvaluatingBalances;

namespace Keeper.Utils.Rates
{
  public interface ICurrencyConverter {
    decimal BalancePairsToUsd(IEnumerable<MoneyPair> inCurrencies, DateTime dateTime);
  }
}