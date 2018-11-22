using System.Collections.Generic;

namespace Keeper2018
{
    public class TrafficLines
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

        public IEnumerable<string> Report()
        {
            foreach (var pair in Traffic)
            {
                yield return $"{ pair.Key}: {Traffic[pair.Key].Plus:#,0.##} - {Traffic[pair.Key].Minus:#,0.##} = {(Traffic[pair.Key].Plus - Traffic[pair.Key].Minus):#,0.##}";
            }
        }
    }
}