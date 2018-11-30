using System.Collections.Generic;
using System.Linq;

namespace Keeper2018
{
    public class BalanceWithTurnoverOfBranch
    {
        private readonly Dictionary<AccountModel, BalanceWithTurnover> _childAccounts = new Dictionary<AccountModel, BalanceWithTurnover>();

        public void Add(AccountModel accountModel, CurrencyCode currency, decimal amount)
        {
            if (!_childAccounts.ContainsKey(accountModel))
                _childAccounts.Add(accountModel, new BalanceWithTurnover());
            _childAccounts[accountModel].Add(currency, amount);
        }

        public void Sub(AccountModel accountModel, CurrencyCode currency, decimal amount)
        {
            if (!_childAccounts.ContainsKey(accountModel))
                _childAccounts.Add(accountModel, new BalanceWithTurnover());
            _childAccounts[accountModel].Sub(currency, amount);
        }

        public List<string> Report(BalanceOrTraffic mode)
        {
            var sum = Summarize();
            var result = new List<string>(sum.Report(mode));

            foreach (var child in _childAccounts)
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

        private BalanceWithTurnover Summarize()
        {
            var sum = new BalanceWithTurnover();
            foreach (var pair in _childAccounts)
            {
                foreach (var trafficLine in pair.Value.Currencies)
                {
                    sum.Add(trafficLine.Key, trafficLine.Value.Plus);
                    sum.Sub(trafficLine.Key, trafficLine.Value.Minus);
                }
            }
            return sum;
        }

        public Balance Balance()
        {
            var balance = new Balance();
            foreach (var pair in _childAccounts)
            {
                balance.AddBalance(pair.Value.Balance());
            }
            return balance;
        }
    }
}