using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keeper.DomainModel
{
    class CurrencyRatesAsDictionary
    {
        public Dictionary<CurrencyCodes, Dictionary<DateTime, double>> Rates { get; set; }

        public CurrencyRatesAsDictionary(IEnumerable<CurrencyRate> collection)
        {
            Rates = new Dictionary<CurrencyCodes, Dictionary<DateTime, double>>
            {
                {CurrencyCodes.USD, new Dictionary<DateTime, double>()},
                {CurrencyCodes.EUR, new Dictionary<DateTime, double>()},
                {CurrencyCodes.RUB, new Dictionary<DateTime, double>()}
            };
            foreach (var currencyRate in collection)
            {
                Rates[currencyRate.Currency].Add(currencyRate.BankDay, currencyRate.Rate);
            }
        }

        public double GetRateThisDayOrBefore(CurrencyCodes currency, DateTime day)
        {
            var rate = (from r in Rates[currency]
                        where r.Key <= day.Date 
                        select r.Value).LastOrDefault();
            return rate;
        }

    }
}
