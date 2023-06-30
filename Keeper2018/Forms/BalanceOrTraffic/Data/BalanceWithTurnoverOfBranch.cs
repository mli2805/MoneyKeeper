using System;
using System.Collections.Generic;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public class BalanceWithTurnoverOfBranch
    {
        public readonly Dictionary<AccountItemModel, BalanceWithTurnover> ChildAccounts = new Dictionary<AccountItemModel, BalanceWithTurnover>();

        public void Add(AccountItemModel accountItemModel, CurrencyCode currency, decimal amount)
        {
            if (!ChildAccounts.ContainsKey(accountItemModel))
                ChildAccounts.Add(accountItemModel, new BalanceWithTurnover());
            ChildAccounts[accountItemModel].Add(currency, amount);
        }

        public void Sub(AccountItemModel accountItemModel, CurrencyCode currency, decimal amount)
        {
            if (!ChildAccounts.ContainsKey(accountItemModel))
                ChildAccounts.Add(accountItemModel, new BalanceWithTurnover());
            ChildAccounts[accountItemModel].Sub(currency, amount);
        }

        public IEnumerable<KeyValuePair<DateTime, string>> Report(BalanceOrTraffic mode)
        {
            var sum = Summarize();
            foreach (var pair in sum.Report(mode))
            {
                yield return pair;
            }

            foreach (var child in ChildAccounts)
            {
                var subReport = child.Value.Report(mode).ToList();
                if (subReport.Any())
                {
                    yield return new KeyValuePair<DateTime, string>(new DateTime(), $"     {child.Key.Name}");
                    foreach (var pair in subReport)
                    {
                        yield return pair;
                    }
                }
            }
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