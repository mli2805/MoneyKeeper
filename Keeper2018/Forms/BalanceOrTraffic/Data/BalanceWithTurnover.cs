using System;
using System.Collections.Generic;

namespace Keeper2018
{
    public class BalanceWithTurnover
    {
        public readonly Dictionary<CurrencyCode, TrafficPair> Currencies = new Dictionary<CurrencyCode, TrafficPair>();
        public void Add(CurrencyCode currency, decimal amount)
        {
            if (Currencies.ContainsKey(currency)) Currencies[currency].Plus = Currencies[currency].Plus + amount;
            else Currencies.Add(currency, new TrafficPair() { Plus = amount });
        }

        public void Sub(CurrencyCode currency, decimal amount)
        {
            if (Currencies.ContainsKey(currency)) Currencies[currency].Minus = Currencies[currency].Minus + amount;
            else Currencies.Add(currency, new TrafficPair() { Minus = amount });
        }

        public IEnumerable<KeyValuePair<DateTime, string>> Report(BalanceOrTraffic mode)
        {
            foreach (var pair in Currencies)
            {
                var total = Currencies[pair.Key].Plus - Currencies[pair.Key].Minus;
                if (total == 0 && mode == BalanceOrTraffic.Balance) continue;

                if (mode == BalanceOrTraffic.Balance)
                    yield return new KeyValuePair<DateTime, string>(new DateTime(), $"{ pair.Key}: {total:#,0.##}");
                else
                {
                    var plusMinus = $"{Currencies[pair.Key].Plus:#,0.##} - {Currencies[pair.Key].Minus:#,0.##}";
                    yield return new KeyValuePair<DateTime, string>(new DateTime(), $"{ pair.Key}: {plusMinus} = {total:#,0.##}");
                }
            }
        }

        public Balance Balance()
        {
            var balance = new Balance();
            foreach (var pair in Currencies)
            {
                if (pair.Value.Plus != pair.Value.Minus)
                    balance.Add(pair.Key, pair.Value.Plus - pair.Value.Minus);
            }
            return balance;
        }
    }
}