using System.Collections.Generic;

namespace Keeper2018
{
    public class Balance
    {
        public readonly Dictionary<CurrencyCode, decimal> Currencies;

        public Balance() { Currencies = new Dictionary<CurrencyCode, decimal>(); }

        public Balance(CurrencyCode currency, decimal amount)
        {
            Currencies = new Dictionary<CurrencyCode, decimal>() { { currency, amount } };
        }
        public void Add(CurrencyCode currency, decimal amount)
        {
            if (Currencies.ContainsKey(currency)) Currencies[currency] = Currencies[currency] + amount; else Currencies.Add(currency, amount);
        }

        public void Sub(CurrencyCode currency, decimal amount)
        {
            if (Currencies.ContainsKey(currency)) Currencies[currency] = Currencies[currency] - amount; else Currencies.Add(currency, -amount);
        }

        public void AddBalance(Balance balance)
        {
            foreach (var currency in balance.Currencies) { Add(currency.Key, currency.Value); }
        }

    }
}