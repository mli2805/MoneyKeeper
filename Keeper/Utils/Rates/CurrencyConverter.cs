using System;
using System.Collections.Generic;
using System.Composition;
using Keeper.DomainModel;
using Keeper.Utils.Balances;

namespace Keeper.Utils.Rates
{
  [Export(typeof(ICurrencyConverter))]
  public class CurrencyConverter : ICurrencyConverter
  {
    private readonly IRateExtractor _rateExtractor;

    [ImportingConstructor]
    public CurrencyConverter(IRateExtractor rateExtractor)
    {
      _rateExtractor = rateExtractor;
    }

    public decimal BalancePairsToUsd(IEnumerable<MoneyPair> inCurrencies, DateTime dateTime)
    {
      decimal result = 0;
      foreach (var balancePair in inCurrencies)
      {
        if (balancePair.Currency == CurrencyCodes.USD)
          result += balancePair.Amount;
        else
          result += balancePair.Amount / (decimal)_rateExtractor.GetRateThisDayOrBefore(balancePair.Currency, dateTime);
      }
      return result;
    }

		
  }
}