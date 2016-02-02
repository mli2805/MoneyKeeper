using System;
using System.Collections.Generic;
using System.Linq;

namespace Keeper.DomainModel
{
    class CurrencyRatesAsDictionary
    {
        public Dictionary<CurrencyCodes, Dictionary<DateTime, double>> Rates { get; set; }

        public CurrencyRatesAsDictionary(List<CurrencyRate> collection)
        {
            Rates = new Dictionary<CurrencyCodes, Dictionary<DateTime, double>>
            {
                {CurrencyCodes.BYR, ConvertOneCurrency(collection, CurrencyCodes.BYR)},
                {CurrencyCodes.EUR, ConvertOneCurrency(collection, CurrencyCodes.EUR)},
                {CurrencyCodes.RUB, ConvertOneCurrency(collection, CurrencyCodes.RUB)}
            };
        }

        private Dictionary<DateTime, double> ConvertOneCurrency(IEnumerable<CurrencyRate> collection, CurrencyCodes currency)
        {
            var dictionary = new Dictionary<DateTime, double>();
            var previousDate = new DateTime(2001, 12, 31);
            var previousRate = 0.0;
            foreach (var currentRate in collection.Where(t => t.Currency == currency).OrderBy(t => t.BankDay))
            {
                while (previousDate.Date < currentRate.BankDay.Date)
                {
                    dictionary.Add(previousDate.Date, previousRate);
                    previousDate = previousDate.AddDays(1);
                }
                dictionary.Add(currentRate.BankDay.Date, currentRate.Rate);
                previousDate = currentRate.BankDay.AddDays(1);
                previousRate = currentRate.Rate;
            }
            return dictionary;
        }
    }
}
