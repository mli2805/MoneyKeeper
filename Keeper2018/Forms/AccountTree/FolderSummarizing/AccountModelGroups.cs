using System;
using System.Collections.Generic;
using System.Linq;
using KeeperDomain;

namespace Keeper2018
{
    public class AccountGroup
    {
        public string Title;
        public List<AccountModel> Accounts = new List<AccountModel>();
        public BalanceWithDetails BalanceWithDetails;
        public decimal Procent;

        public AccountGroup(string title) { Title = title; }
    }

    public class AccountGroups
    {
        public List<AccountGroup> Groups;
        public decimal TotalInUsd;

        public AccountGroups(List<AccountGroup> groups) { Groups = groups; }

        public void Evaluate(KeeperDataModel dataModel)
        {
            foreach (var accountGroup in Groups)
                accountGroup.BalanceWithDetails = EvaluateOneGroupBalance(accountGroup.Accounts, dataModel);

            TotalInUsd = Groups.Sum(g => g.BalanceWithDetails.TotalInUsd);
            foreach (var accountGroup in Groups)
                accountGroup.Procent = accountGroup.BalanceWithDetails.TotalInUsd * 100 / TotalInUsd;
        }

        private BalanceWithDetails EvaluateOneGroupBalance(List<AccountModel> group, KeeperDataModel dataModel)
        {
            var groupBalance = new Balance();
            foreach (var accountModel in group)
            {
                var calc = new TrafficOfAccountCalculator(dataModel, accountModel,
                        new Period(new DateTime(2001, 12, 31), DateTime.Today.AddDays(1)));
                var bal = calc.EvaluateBalance();
                groupBalance.AddBalance(bal);
            }

            return groupBalance.EvaluateDetails(dataModel, DateTime.Today.AddDays(1));
        }

        public IEnumerable<string> ToStrings()
        {
            foreach (var accountGroup in Groups)
            {
                yield return $"{accountGroup.Title}  {accountGroup.BalanceWithDetails.TotalInUsd:0,0.00} usd  {accountGroup.Procent:0.00}% ";
            }

            yield return $"Итого  {TotalInUsd:0.00} usd";
        }

        public List<string> ToStringList()
        {
            var result = new List<string>();
            foreach (var accountGroup in Groups)
            {
                result.Add($"{accountGroup.Title}  {accountGroup.Procent:0.00}% ");
                result.AddRange(accountGroup.BalanceWithDetails.ToStrings());
                result.Add("");
            }
            result.Add($"Итого  {TotalInUsd:0,0.00} usd");
            return result;
        }
    }

 
}