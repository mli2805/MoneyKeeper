using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.Utils
{
  class Balance
  {
    public static KeeperDb Db { get { return IoC.Get<KeeperDb>(); } }

    class BalancePair
    {
      public CurrencyCodes? Currency;
      public decimal Amount;
    }

    private static IEnumerable<BalancePair> AccountBalancePairs(Account balancedAccount, Period period)
    {
      var tempBalance = 
        (from t in Db.Transactions.Local
         where period.IsDateTimeIn(t.Timestamp) && 
               (t.Credit.IsTheSameOrDescendantOf(balancedAccount.Name) && !t.Debet.IsTheSameOrDescendantOf(balancedAccount.Name) ||
               (t.Debet.IsTheSameOrDescendantOf(balancedAccount.Name) && !t.Credit.IsTheSameOrDescendantOf(balancedAccount.Name)))
         group t by t.Currency into g
         select new BalancePair()
                    {
                      Currency = g.Key,
                      Amount = g.Sum(a => a.Amount * a.SignForAmount(balancedAccount))
                    }).
        Concat
        (from t in Db.Transactions.Local
         // учесть вторую сторону обмена - приход денег в другой валюте
         where t.Amount2 != 0 && period.IsDateTimeIn(t.Timestamp) && 
               (t.Credit.IsTheSameOrDescendantOf(balancedAccount.Name) ||
                                           t.Debet.IsTheSameOrDescendantOf(balancedAccount.Name))
         group t by t.Currency2 into g
         select new BalancePair()
                    {
                      Currency = g.Key,
                      Amount = g.Sum(a => a.Amount2 * a.SignForAmount(balancedAccount) * -1)
                    });

      return from b in tempBalance
             group b by b.Currency into g
             select new BalancePair()
                             {
                               Currency = g.Key,
                               Amount = g.Sum(a => a.Amount)
                             };
    }

    private static IEnumerable<BalancePair> ArticleBalancePairs(Account balancedAccount, Period period)
    {
      return from t in Db.Transactions.Local
             where t.Article != null && t.Article.IsTheSameOrDescendantOf(balancedAccount.Name) && period.IsDateTimeIn(t.Timestamp)
             group t by t.Currency into g
             select new BalancePair()
                        {
                          Currency = g.Key,
                          Amount = g.Sum(a => a.Amount)
                        };
    }

    private static List<string> OneBalance(Account balancedAccount, Period period)
    {
      var balance = new List<string>();
      if (balancedAccount == null) return balance;

      bool kind = balancedAccount.IsTheSameOrDescendantOf("Все доходы") || balancedAccount.IsTheSameOrDescendantOf("Все расходы");
      var balancePairs = kind ? ArticleBalancePairs(balancedAccount, period) : AccountBalancePairs(balancedAccount, period);

      foreach (var item in balancePairs)
        if (item.Amount != 0) balance.Add(String.Format("{0:#,#} {1}", item.Amount, item.Currency));
      return balance;
    }

    /// <summary>
    /// Функция нужна только заполнения для 2-й рамки на ShellView
    /// Расчитываются остатки по счету и его потомкам 1-го поколения
    /// </summary>
    public static void CountBalances(Account selectedAccount, Period period, ObservableCollection<string> balanceList)
    {
      balanceList.Clear();
      if (selectedAccount == null) return;

      var b = OneBalance(selectedAccount, period);
      foreach (var st in b)
        balanceList.Add(st);

      foreach (var child in selectedAccount.Children)
      {
        b = OneBalance(child, period);
        if (b.Count > 0) balanceList.Add("         " + child.Name);
        foreach (var st in b)
          balanceList.Add("    " + st);
      }
    }


    // Хреново!!! - запрашивает баланс по всем валютам, и возращает по одной переданной в качестве параметра
    public static decimal GetBalanceInCurrency(Account account, Period period, CurrencyCodes currency)
    {
      if (account == null) return 0;
      var balances = AccountBalancePairs(account, period);
      foreach (var balancePair in balances)
      {
        if (balancePair.Currency == currency) return balancePair.Amount;
      }
      return 0;
    }

  }
}
