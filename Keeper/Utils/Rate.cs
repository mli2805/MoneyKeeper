using System;
using System.Linq;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.Utils
{
  class Rate
  {
    public static KeeperTxtDb Db { get { return IoC.Get<KeeperTxtDb>(); } }

    public static double GetRate(CurrencyCodes currency, DateTime day)
    {
      var rate = (from currencyRate in Db.CurrencyRates
                  where currencyRate.BankDay.Date == day.Date && currencyRate.Currency == currency
                  select currencyRate).FirstOrDefault();
      return rate != null ? rate.Rate : 0.0;
    }

    public static string GetUsdEquivalent(decimal amount, CurrencyCodes currency, DateTime timestamp)
    {
      decimal temp;
      return GetUsdEquivalent(amount, currency, timestamp, out temp);
    }

    public static string GetUsdEquivalent(decimal amount, CurrencyCodes currency, DateTime timestamp, out decimal amountInUsd)
    {
      amountInUsd = 0;
      var rate = GetRate(currency, timestamp);
      if (rate.Equals(0.0)) return "не задан курс " + currency + " на эту дату";

      amountInUsd = amount/(decimal) rate;
      var res = amountInUsd.ToString("F2") + "$ по курсу " + rate;
      if (currency == CurrencyCodes.EUR)
        res = amountInUsd.ToString("F2") + "$ по курсу " + (1 / rate).ToString("F3");
      return res;
    }

  }
}
