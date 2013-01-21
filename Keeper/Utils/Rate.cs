using System;
using System.ComponentModel.Composition;
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
      var rate = (from currencyRate in Db.CurrencyRates.Local
                  where currencyRate.BankDay.Date == day.Date && currencyRate.Currency == currency
                  select currencyRate).FirstOrDefault();
      return rate != null ? rate.Rate : 0.0;
    }
  }
}
