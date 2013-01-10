using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.Utils
{
  class Rate
  {
    [Import]
    public static KeeperDb Db { get { return IoC.Get<KeeperDb>(); } }

    public static double GetRate(CurrencyCodes currency, DateTime day)
    {
      var rate = (from currencyRate in Db.CurrencyRates.Local
                  where currencyRate.BankDay == day && currencyRate.Currency == currency
                  select currencyRate).FirstOrDefault();
      return rate != null ? rate.Rate : 0.0;
    }
  }
}
