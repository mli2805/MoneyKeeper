using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;

using Caliburn.Micro;

using Keeper.DomainModel;
using Keeper.ViewModels;

namespace Keeper.Utils
{
  class BalanceTrio
  {
    public Account MyAccount;
    public decimal Amount;
    public CurrencyCodes Currency;

    public new string ToString()
    {
      return String.Format("{0}  {1:#,0} {2}", MyAccount.Name, Amount, Currency.ToString().ToLower());
    }
  }

	[Export]
	public class BalancesForTransactionsCalculator
  {
    private readonly KeeperDb _db;

    [ImportingConstructor]
    public BalancesForTransactionsCalculator(KeeperDb db)
    {
      _db = db;
    }

    public List<string> CalculateDayResults(DateTime dt)
    {
      var dayResults = new List<string> { String.Format("                              {0:dd MMMM yyyy}", dt.Date) };

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

      if (incomes.Any()) dayResults.Add("  Доходы");
      foreach (var balanceTrio in incomes)
      {
        dayResults.Add(balanceTrio.ToString());
      }

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

      if (dayResults.Count > 0) dayResults.Add("");
      if (expense.Any()) dayResults.Add("  Расходы");
      foreach (var balanceTrio in expense)
      {
        dayResults.Add(balanceTrio.ToString());
      }

      return dayResults;
    }

    public string EndDayBalances(DateTime dt)
    {
	    var balanceCalculator = IoC.Get<BalanceCalculator>();
      var period = new Period(new DateTime(0), new DayProcessor(dt).AfterThisDay());
      var result = String.Format(" На конец {0:dd MMMM yyyy} :   ", dt.Date);

      var depo = (from a in _db.AccountsPlaneList
                  where a.Name == "Депозиты"
                  select a).First();
      var calculatedAccounts = new List<Account>(UsefulLists.MyAccountsForShopping);
      calculatedAccounts.Add(depo);
      foreach (var account in calculatedAccounts)
      {
        var pairs = balanceCalculator.AccountBalancePairs(account, period).ToList();
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
