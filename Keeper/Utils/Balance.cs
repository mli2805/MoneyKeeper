using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.Utils
{
  class Balance
  {
    [Import]
    public static KeeperDb Db { get { return IoC.Get<KeeperDb>(); } }

    class BalancePair
    {
      public CurrencyCodes? Currency;
      public decimal Amount;

      public string NullableToString()
      {
        if (Amount == 0) return null;
        return String.Format("{0:#,#} {1}", Amount, Currency);
      }
    }

    private static List<string> AccountBalance(Account balancedAccount)
    {
      var tempBalance = (from t in Db.Transactions.Local
                         where t.Credit.IsTheSameOrDescendantOf(balancedAccount.Name) || t.Debet.IsTheSameOrDescendantOf(balancedAccount.Name)
                         group t by t.Currency into g
                         select new BalancePair()
                                  {
                                    Currency = g.Key,
                                    Amount = g.Sum(a => a.Amount*a.SignForAmount(balancedAccount))
                                  }).
                    Concat
                        (from t in Db.Transactions.Local
                         where t.Amount2 != 0 && (t.Credit.IsTheSameOrDescendantOf(balancedAccount.Name) || t.Debet.IsTheSameOrDescendantOf(balancedAccount.Name))
                         group t by t.Currency2 into g
                         select new BalancePair()
                                  {
                                    Currency = g.Key,
                                    Amount = g.Sum(a => a.Amount2*a.SignForAmount(balancedAccount)*-1)
                                  });


      var balanceByCurrency = from b in tempBalance
                              group b by b.Currency into g
                              select new BalancePair()
                                       {
                                         Currency = g.Key,
                                         Amount = g.Sum(a => a.Amount)
                                       }.NullableToString();

      var balance = new List<string>();
      foreach (var item in balanceByCurrency)
        if (item != null) balance.Add(item);

      return balance;
    }

    private static List<string> ArticleBalance(Account balancedAccount)
    {
      var balanceByCurrency = from t in Db.Transactions.Local
                              where t.Article != null && t.Article.IsTheSameOrDescendantOf(balancedAccount.Name)
                              group t by t.Currency into g
                              select new BalancePair()
                              {
                                Currency = g.Key,
                                Amount = g.Sum(a => a.Amount)
                              }.NullableToString();

      var balance = new List<string>();
      foreach (var item in balanceByCurrency)
        if (item != null) balance.Add(item);

      return balance;
    }


    public static void CountBalances(Account selectedAccount, ObservableCollection<string> balanceList)
    {
      balanceList.Clear();
      List<string> b;
      bool kind = selectedAccount.IsTheSameOrDescendantOf("Все доходы") || selectedAccount.IsTheSameOrDescendantOf("Все расходы");

      b = kind ? ArticleBalance(selectedAccount) : AccountBalance(selectedAccount);
      foreach (var st in b)
        balanceList.Add(st);

      foreach (var child in selectedAccount.Children)
      {
        b = kind ? ArticleBalance(child) : AccountBalance(child);
        if (b.Count > 0) balanceList.Add("         " + child.Name);
        foreach (var st in b)
          balanceList.Add("    " + st);
      }
    }

  }
}
