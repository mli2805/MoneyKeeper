using System;
using System.Collections.Generic;

namespace Keeper.DomainModel
{
    class CurrencyRatesAsDictionary
    {
        public Dictionary<CurrencyCodes, Dictionary<DateTime, double>> Rates { get; set; }

        public CurrencyRatesAsDictionary(IEnumerable<CurrencyRate> collection)
        {
            Rates = new Dictionary<CurrencyCodes, Dictionary<DateTime, double>>
            {
                {CurrencyCodes.BYR, new Dictionary<DateTime, double>()},
                {CurrencyCodes.EUR, new Dictionary<DateTime, double>()},
                {CurrencyCodes.RUB, new Dictionary<DateTime, double>()}
            };
            foreach (var currencyRate in collection)
            {
                Rates[currencyRate.Currency].Add(currencyRate.BankDay.Date, currencyRate.Rate);
            }
        }

        /// <summary>
        /// returns 0 if there's no rate for specified date
        /// </summary>
        /// <param name="currency"></param>
        /// <param name="amount"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public decimal GetUsdEquivalent(CurrencyCodes currency, decimal amount, DateTime date)
        {
            if (currency == CurrencyCodes.USD) return amount;

            double rate;
            return Rates[currency].TryGetValue(date, out rate) ? amount/(decimal)rate : 0;
        }

    }
}
