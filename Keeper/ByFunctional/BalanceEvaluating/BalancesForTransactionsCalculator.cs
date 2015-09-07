using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Caliburn.Micro;
using Keeper.ByFunctional.AccountEditing;
using Keeper.ByFunctional.BalanceEvaluating.Ilya;
using Keeper.DomainModel;
using Keeper.Utils.Common;

namespace Keeper.ByFunctional.BalanceEvaluating
{
    [Export]
    public class BalancesForTransactionsCalculator
    {
        private readonly KeeperDb _db;
        private readonly AccountTreeStraightener _accountTreeStraightener;
        private readonly AccountBalanceCalculator _accountBalanceCalculator;

        [ImportingConstructor]
        public BalancesForTransactionsCalculator(KeeperDb db, AccountTreeStraightener accountTreeStraightener, AccountBalanceCalculator accountBalanceCalculator)
        {
            _db = db;
            _accountTreeStraightener = accountTreeStraightener;
            _accountBalanceCalculator = accountBalanceCalculator;
        }

        public List<string> CalculateDayResults(DateTime dt)
        {
            var dayResults = new List<string> { String.Format("                              {0:dd MMMM yyyy}", dt.Date) };

            var incomes = GetMyDayIncomes(dt);
            if (incomes.Any())
            {
                dayResults.Add("  Доходы");
                dayResults.AddRange(incomes.Select(element => element.ToString()));
                dayResults.Add("");
            }

            var expense = GetMyDayExpense(dt);
            if (expense.Any())
            {
                dayResults.Add("  Расходы");
                dayResults.AddRange(expense.Select(element => element.ToString()));
            }

            return dayResults;
        }

        private IEnumerable<BalanceTrio> GetMyDayExpense(DateTime dt)
        {
            var expense = from t in _db.Transactions
                          where t.Operation == OperationType.Расход && t.Timestamp.Date == dt.Date
                          group t by new
                                       {
                                           t.Debet,
                                           t.Currency
                                       }
                              into g
                              select new BalanceTrio
                                       {
                                           MyAccount = g.Key.Debet,
                                           Currency = g.Key.Currency,
                                           Amount = g.Sum(a => a.Amount)
                                       };
            return expense;
        }

        private IEnumerable<BalanceTrio> GetMyDayIncomes(DateTime dt)
        {
            var incomes = from t in _db.Transactions
                          where t.Operation == OperationType.Доход && t.Timestamp.Date == dt.Date
                          group t by new
                                       {
                                           t.Credit,
                                           t.Currency
                                       }
                              into g
                              select new BalanceTrio
                                       {
                                           MyAccount = g.Key.Credit,
                                           Currency = g.Key.Currency,
                                           Amount = g.Sum(a => a.Amount)
                                       };
            return incomes;
        }


        private IEnumerable<Account> PrepareAccountList()
        {
            var allMyAccounts = new List<Account>((_accountTreeStraightener.Flatten(_db.Accounts).Where(account => account.Is("Мои") &&
              account.Children.Count == 0 && !account.Is("Депозиты"))));
            var depoRoot = (from a in new AccountTreeStraightener().Flatten(_db.Accounts)
                            where a.Name == "Депозиты"
                            select a).First();
            allMyAccounts.Add(depoRoot);
            return allMyAccounts;
        }

        private IEnumerable<Account> OmitNotUsedAccounts(IEnumerable<Account> list)
        {
            return (from account in list let tr = _db.Transactions.LastOrDefault(t => t.EitherDebitOrCreditIs(account)) 
                                   where tr != null && (DateTime.Now - tr.Timestamp).TotalDays < 40 select account).ToList();
        }
        public string EndDayBalances(DateTime dt)
        {
            var period = new Period(new DateTime(0), dt.GetEndOfDate());
            var result = String.Format(" На конец {0:d MMMM yyyy} :   ", dt.Date);

            var calculatedAccounts = OmitNotUsedAccounts(PrepareAccountList());
            foreach (var account in calculatedAccounts)
            {
                var pairs = _accountBalanceCalculator.GetAccountBalancePairsWithTimeChecking(account, period).ToList();
                if (account.Name == "Депозиты") result += "\n";
                foreach (var balancePair in pairs.ToArray())
                    if (balancePair.Amount == 0) pairs.Remove(balancePair);
                if (pairs.Any())
                    result = result + String.Format("   {0}  {1}", account.Name, pairs[0].ToString());
                if (pairs.Count() > 1)
                    for (var i = 1; i < pairs.Count(); i++)
                        result = result + String.Format(" + {0}", pairs[i].ToString());
            }

            return result;
        }
    }
}
