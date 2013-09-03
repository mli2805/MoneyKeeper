using System;
using System.Linq;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.Utils
{
  class Rate
  {
    public static KeeperDb Db { get { return IoC.Get<KeeperDb>(); } }

    public static double GetRate(CurrencyCodes currency, DateTime day)
    {
      var rate = (from currencyRate in Db.CurrencyRates
                  where currencyRate.BankDay.Date == day.Date && currencyRate.Currency == currency
                  select currencyRate).FirstOrDefault();
      return rate != null ? rate.Rate : 0.0;
    }

    public static double GetLastRate(CurrencyCodes currency)
    {
      var rate = (from currencyRate in Db.CurrencyRates
                  where currencyRate.Currency == currency
                  orderby currencyRate.BankDay
                  select currencyRate).LastOrDefault();
      return rate != null ? rate.Rate : 0.0;
    }

    public static double GetRateThisDayOrBefore(CurrencyCodes currency, DateTime day)
    {
      var rate = (from currencyRate in Db.CurrencyRates
                  where currencyRate.BankDay.Date <= day.Date && currencyRate.Currency == currency
                  select currencyRate).LastOrDefault();
      return rate != null ? rate.Rate : 0.0;
    }

    public static decimal GetUsdEquivalent(decimal amount, CurrencyCodes currency, DateTime timestamp)
    {
      if (currency == CurrencyCodes.USD) return amount;

      var rate = GetRateThisDayOrBefore(currency, timestamp);
      if (rate.Equals(0.0)) return -1;

      return amount / (decimal)rate;
    }

    public static string GetUsdEquivalentString(decimal amount, CurrencyCodes currency, DateTime timestamp)
    {
      decimal temp;
      return GetUsdEquivalentString(amount, currency, timestamp, out temp);
    }

    public static string GetUsdEquivalentString(decimal amount, CurrencyCodes currency, DateTime timestamp, out decimal amountInUsd)
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
