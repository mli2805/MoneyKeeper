using System.Collections.Generic;

namespace Keeper2018
{
    public class TrafficOfAccount
    {
        public readonly Dictionary<CurrencyCode, TrafficPair> Traffic = new Dictionary<CurrencyCode, TrafficPair>();
        public void Add(CurrencyCode currency, decimal amount)
        {
            if (Traffic.ContainsKey(currency)) Traffic[currency].Plus = Traffic[currency].Plus + amount;
            else Traffic.Add(currency, new TrafficPair() { Plus = amount });
        }

        public void Sub(CurrencyCode currency, decimal amount)
        {
            if (Traffic.ContainsKey(currency)) Traffic[currency].Minus = Traffic[currency].Minus + amount;
            else Traffic.Add(currency, new TrafficPair() { Minus = amount });
        }

        public IEnumerable<string> Report(BalanceOrTraffic mode)
        {
            foreach (var pair in Traffic)
            {
                var total = Traffic[pair.Key].Plus - Traffic[pair.Key].Minus;
                if (total == 0 && mode == BalanceOrTraffic.Balance) continue;

                if (mode == BalanceOrTraffic.Balance)
                    yield return $"{ pair.Key}: {total:#,0.##}";
                else
                {
                    var plusMinus = $"{Traffic[pair.Key].Plus:#,0.##} - {Traffic[pair.Key].Minus:#,0.##}";
                    yield return $"{ pair.Key}: {plusMinus} = {total:#,0.##}";
                }
            }
        }

        public BalanceOfAccount Balance()
        {
            var balance = new BalanceOfAccount();
            foreach (var pair in Traffic)
            {
                if (pair.Value.Plus != pair.Value.Minus)
                    balance.Add(pair.Key, pair.Value.Plus - pair.Value.Minus);
            }
            return balance;
        }
    }
}