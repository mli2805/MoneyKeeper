using System.Collections.Generic;
using System.Linq;
using KeeperDomain;

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

        public Balance(Balance source)
        {
            Currencies = new Dictionary<CurrencyCode, decimal>();
            foreach (var pair in source.Currencies)
            {
                Currencies.Add(pair.Key, pair.Value);
            }
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

        public void SubBalance(Balance balance)
        {
            foreach (var currency in balance.Currencies) { Sub(currency.Key, currency.Value); }
        }

        public override string ToString()
        {
            if (Currencies.All(c => c.Value == 0)) return "0";
            if (Currencies.Count(c => c.Value != 0) == 1)
            {
                var currency = Currencies.First(c => c.Value != 0).Key;
                var value = Currencies.First(c => c.Value != 0).Value;
                var amountStr = currency == CurrencyCode.BYR ? value.ToString("0,0") : value.ToString("0.00");
                return $"{amountStr} {currency.ToString().ToLower()}";
            }
            return "more than 1 currency";
        }

    }
}