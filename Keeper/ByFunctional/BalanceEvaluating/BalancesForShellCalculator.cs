using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using Keeper.DomainModel;
using Keeper.Utils.Rates;

namespace Keeper.ByFunctional.BalanceEvaluating
{
  [Export]
  public class BalancesForShellCalculator
  {
    private readonly AccountBalanceCalculator _accountBalanceCalculator;
    private readonly ArticleBalanceCalculator _articleBalanceCalculator;
    private readonly RateExtractor _rateExtractor;

    [ImportingConstructor]
    public BalancesForShellCalculator(RateExtractor rateExtractor, AccountBalanceCalculator accountBalanceCalculator, ArticleBalanceCalculator articleBalanceCalculator)
    {
	    _rateExtractor = rateExtractor;
	    _accountBalanceCalculator = accountBalanceCalculator;
      _articleBalanceCalculator = articleBalanceCalculator;
    }

	  private List<string> OneBalance(Account balancedAccount, Period period, out decimal totalInUsd)
    {
      var balance = new List<string>();
      totalInUsd = 0;
      if (balancedAccount == null) return balance;

      var balancePairs = _accountBalanceCalculator.GetAccountBalancePairsFromMidnightToMidnight(balancedAccount, period);

      foreach (var item in balancePairs)
      {
        if (item.Amount != 0) balance.Add(String.Format("{0:#,#} {1}", item.Amount, item.Currency));
        totalInUsd += _rateExtractor.GetUsdEquivalent(item.Amount, item.Currency, period.Finish);
      }

      return balance;
    }

    private decimal FillListWithBalance(Account selectedAccount, Period period, ObservableCollection<string> balanceList)
    {
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

    private decimal FillListWithTraffic(Account selectedAccount, Period period, ObservableCollection<string> trafficList)
    {
      trafficList.Clear();
      var firstTransactions = new List<string>();
      var b = _articleBalanceCalculator.GetArticleBalanceInUsdPlusFromMidnightToMidnight(selectedAccount, period, firstTransactions);
      trafficList.Add(b == 0
                        ? "В данном периоде \nдвижение по выбранному счету не найдено"
                        : string.Format("{0}   {1:#,0} usd", selectedAccount.Name, b));

      foreach (var child in selectedAccount.Children)
      {
          decimal c = _articleBalanceCalculator.GetArticleBalanceInUsdPlusFromMidnightToMidnight(child, period, firstTransactions);
        if (c != 0) trafficList.Add(string.Format("   {0}   {1:#,0} usd", child.Name, c));
      }

      if (selectedAccount.Children.Count == 0)
        foreach (var transaction in firstTransactions)
          trafficList.Add(transaction);

      return b;
    }

    public decimal FillListForShellView(Account selectedAccount, Period period, ObservableCollection<string> balanceList)
    {
      balanceList.Clear();
      if (selectedAccount == null) return 0;
      if (selectedAccount.Is("Все доходы") || selectedAccount.Is("Все расходы"))
        return FillListWithTraffic(selectedAccount, period, balanceList);
      else return FillListWithBalance(selectedAccount, period, balanceList);

    }


  }
}
