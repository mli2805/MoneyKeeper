using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;

using Keeper.DomainModel;

namespace Keeper.Utils
{
[Export]
  public class BalancesForShellCalculator
  {
    private readonly BalanceCalculator _balanceCalculator;
    private readonly RateExtractor _rateExtractor;

    [ImportingConstructor]
    public BalancesForShellCalculator(KeeperDb db, RateExtractor rateExtractor, BalanceCalculator balanceCalculator)
    {
	    _rateExtractor = rateExtractor;
	    _balanceCalculator = balanceCalculator;
    }

	  private List<string> OneBalance(Account balancedAccount, Period period, out decimal totalInUsd)
    {
      var balance = new List<string>();
      totalInUsd = 0;
      if (balancedAccount == null) return balance;

      var kind = balancedAccount.IsTheSameOrDescendantOf("Все доходы") || balancedAccount.IsTheSameOrDescendantOf("Все расходы");
      var balancePairs = kind ? 
         _balanceCalculator.ArticleBalancePairs(balancedAccount, period) :
         _balanceCalculator.AccountBalancePairs(balancedAccount, period);

      foreach (var item in balancePairs)
      {
        if (item.Amount != 0) balance.Add(String.Format("{0:#,#} {1}", item.Amount, item.Currency));
        totalInUsd += _rateExtractor.GetUsdEquivalent(item.Amount, item.Currency, period.Finish);
      }

      return balance;
    }

    public decimal CountBalances(Account selectedAccount, Period period, ObservableCollection<string> balanceList)
    {
      balanceList.Clear();
      if (selectedAccount == null) return 0;

      decimal inUsd;
      var b = OneBalance(selectedAccount, period, out inUsd);
      foreach (var st in b)
        balanceList.Add(st);

      foreach (var child in selectedAccount.Children)
      {
        decimal temp;
        b = OneBalance(child, period, out temp);
        if (b.Count > 0) balanceList.Add("         " + child.Name);
        foreach (var st in b)
          balanceList.Add("    " + st);
      }
      return inUsd;
    }
  }
}
