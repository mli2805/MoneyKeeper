using System.Collections.Generic;
using System.Linq;

namespace Keeper2018
{
    public class BalanceWithTurnoverOfBranch
    {
        public readonly Dictionary<AccountModel, BalanceWithTurnover> ChildAccounts = new Dictionary<AccountModel, BalanceWithTurnover>();

        public void Add(AccountModel accountModel, CurrencyCode currency, decimal amount)
        {
            if (!ChildAccounts.ContainsKey(accountModel))
                ChildAccounts.Add(accountModel, new BalanceWithTurnover());
            ChildAccounts[accountModel].Add(currency, amount);
        }

        public void Sub(AccountModel accountModel, CurrencyCode currency, decimal amount)
        {
            if (!ChildAccounts.ContainsKey(accountModel))
                ChildAccounts.Add(accountModel, new BalanceWithTurnover());
            ChildAccounts[accountModel].Sub(currency, amount);
        }

        public List<string> Report(BalanceOrTraffic mode)
        {
            var sum = Summarize();
            var result = new List<string>(sum.Report(mode));

            foreach (var child in ChildAccounts)
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

        public BalanceWithTurnover Summarize()
        {
            var sum = new BalanceWithTurnover();
            foreach (var pair in ChildAccounts)
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
            foreach (var pair in ChildAccounts)
            {
                balance.AddBalance(pair.Value.Balance());
            }
            return balance;
        }
    }
}