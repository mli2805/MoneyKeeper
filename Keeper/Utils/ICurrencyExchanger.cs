using System;
using System.Collections.Generic;

namespace Keeper.Utils
{
  public interface ICurrencyExchanger {
    decimal BalancePairsToUsd(IEnumerable<MoneyPair> inCurrencies, DateTime dateTime);
  }
}