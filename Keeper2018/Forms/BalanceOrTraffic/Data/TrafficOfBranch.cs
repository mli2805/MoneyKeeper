using System.Collections.Generic;
using System.Linq;

namespace Keeper2018
{
    public class TrafficOfBranch
    {
        private readonly Dictionary<AccountModel, TrafficOfAccount> _traffics = new Dictionary<AccountModel, TrafficOfAccount>();

        public void Add(AccountModel accountModel, CurrencyCode currency, decimal amount)
        {
            if (!_traffics.ContainsKey(accountModel))
                _traffics.Add(accountModel, new TrafficOfAccount());
            _traffics[accountModel].Add(currency, amount);
        }

        public void Sub(AccountModel accountModel, CurrencyCode currency, decimal amount)
        {
            if (!_traffics.ContainsKey(accountModel))
                _traffics.Add(accountModel, new TrafficOfAccount());
            _traffics[accountModel].Sub(currency, amount);
        }

        public List<string> Report(BalanceOrTraffic mode)
        {
            var sum = Summarize();
            var result = new List<string>(sum.Report(mode));

            foreach (var child in _traffics)
            {
                var subReport = child.Value.Report(mode).Select(x=>"   " + x).ToList();
                if (subReport.Any())
                {
                    result.Add($"     {child.Key.Name}");
                    result.AddRange(subReport);
                }
            }

            return result;
        }

        private TrafficOfAccount Summarize()
        {
            var sum = new TrafficOfAccount();
            foreach (var pair in _traffics)
            {
                foreach (var trafficLine in pair.Value.Traffic)
                {
                    sum.Add(trafficLine.Key, trafficLine.Value.Plus);
                    sum.Sub(trafficLine.Key, trafficLine.Value.Minus);
                }
            }
            return sum;
        }

        public BalanceOfAccount Balance()
        {
            var balance = new BalanceOfAccount();
            foreach (var pair in _traffics)
            {
                balance.AddBalance(pair.Value.Balance());
            }
            return balance;
        }
    }
}