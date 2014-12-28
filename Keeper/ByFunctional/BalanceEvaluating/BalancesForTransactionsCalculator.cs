using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Caliburn.Micro;
using Keeper.ByFunctional.AccountEditing;
using Keeper.DomainModel;
using Keeper.Utils.Common;

namespace Keeper.ByFunctional.BalanceEvaluating
{
	[Export]
	public class BalancesForTransactionsCalculator
  {
    private readonly KeeperDb _db;
	  private readonly AccountTreeStraightener _accountTreeStraightener;

	  [ImportingConstructor]
    public BalancesForTransactionsCalculator(KeeperDb db, AccountTreeStraightener accountTreeStraightener)
    {
      _db = db;
      _accountTreeStraightener = accountTreeStraightener;
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

	  public string EndDayBalances(DateTime dt)
    {
	    var balanceCalculator = IoC.Get<AccountBalanceCalculator>();
      var period = new Period(new DateTime(0), dt.GetEndOfDate());
      var result = String.Format(" На конец {0:dd MMMM yyyy} :   ", dt.Date);

      var depo = (from a in new AccountTreeStraightener().Flatten(_db.Accounts)
                  where a.Name == "Депозиты"
                  select a).First();

      var calculatedAccounts = new List<Account>((_accountTreeStraightener.Flatten(_db.Accounts).Where(account => account.Is("Мои") &&
        account.Children.Count == 0 && !account.Is("Депозиты"))));
      calculatedAccounts.Add(depo);
      foreach (var account in calculatedAccounts)
      {
        var pairs = balanceCalculator.GetAccountBalancePairsWithTimeChecking(account, period).ToList();
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
