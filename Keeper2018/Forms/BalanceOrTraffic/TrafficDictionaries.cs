using System.Collections.Generic;

namespace Keeper2018
{
    public class TrafficDictionaries
    {
        private readonly Dictionary<AccountModel, TrafficLines> _traffics = new Dictionary<AccountModel, TrafficLines>();

        public void Add(AccountModel accountModel, CurrencyCode currency, decimal amount)
        {
            if (!_traffics.ContainsKey(accountModel))
                _traffics.Add(accountModel, new TrafficLines());
            _traffics[accountModel].Add(currency, amount);
        }

        public void Sub(AccountModel accountModel, CurrencyCode currency, decimal amount)
        {
            if (!_traffics.ContainsKey(accountModel))
                _traffics.Add(accountModel, new TrafficLines());
            _traffics[accountModel].Sub(currency, amount);
        }

        public IEnumerable<string> Report()
        {
            var total = Summarize();
            foreach (var str in total.Report())
            {
                yield return str;
            }

            foreach (var child in _traffics)
            {
                yield return $"    {child.Key.Name}";
                foreach (var str in child.Value.Report())
                {
                    yield return str;
                }
            }
        }

        private TrafficLines Summarize()
        {
            var total = new TrafficLines();
            foreach (var pair in _traffics)
            {
                foreach (var trafficLine in pair.Value.Traffic)
                {
                    total.Add(trafficLine.Key, trafficLine.Value.Plus);
                    total.Sub(trafficLine.Key, trafficLine.Value.Minus);
                }
            }
            return total;
        }
    }
}