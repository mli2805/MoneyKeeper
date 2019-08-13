using System;
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