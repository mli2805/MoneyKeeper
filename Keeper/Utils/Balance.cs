using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.ViewModels;

namespace Keeper.Utils
{
  public class Balance
  {
    public static KeeperTxtDb Db { get { return IoC.Get<KeeperTxtDb>(); } }

    public class BalancePair : IComparable
    {
      public CurrencyCodes Currency { get; set; }
      public decimal Amount { get; set; }

      public new string ToString()
      {
        return String.Format("{0:#,0} {1}", Amount, Currency.ToString().ToLower());
      }

      public int CompareTo(object obj)
      {
        return Currency.CompareTo(((BalancePair) obj).Currency);
      }
    }

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

    /// <summary>
    /// вызов с параметром 2 февраля 2013 - вернет остаток по счету на утро 2 февраля 2013 
    /// </summary>
    /// <param name="balancedAccount">счет, по которому будет вычислен остаток</param>
    /// <param name="dateTime">день, до которого остаток</param>
    /// <returns></returns>
    public static IEnumerable<BalancePair> AccountBalancePairsBeforeDay(Account balancedAccount, DateTime dateTime)
    {
                                                      // выделение даты без времени и минус минута
      var period = new Period(new DateTime(0), dateTime.Date.AddMinutes(-1));
      return AccountBalancePairs(balancedAccount, period);
    }

    /// <summary>
    /// вызов с параметром 2 февраля 2013 - вернет остаток по счету после 2 февраля 2013 
    /// </summary>
    /// <param name="balancedAccount">счет, по которому будет вычислен остаток</param>
    /// <param name="dateTime">день, после которого остаток</param>
    /// <returns></returns>
    public static IEnumerable<BalancePair> AccountBalancePairsAfterDay(Account balancedAccount, DateTime dateTime)

    {                                                    // выделение даты без времени плюс день и минус минута
      var period = new Period(new DateTime(0), dateTime.Date.AddDays(1).AddMinutes(-1));
      return AccountBalancePairs(balancedAccount, period);
    }

    /// <summary>
    /// переводит остатки во всех валютах по balancedAccount после dateTime в доллары
    /// </summary>
    /// <param name="balancedAccount">счет, по которому будет вычислен остаток</param>
    /// <param name="dateTime">день, после которого остаток</param>
    /// <returns></returns>
    public static decimal AccountBalanceAfterDayInUsd(Account balancedAccount, DateTime dateTime)
    {
      var inCurrencies = AccountBalancePairsAfterDay(balancedAccount, dateTime);
      decimal result = 0;
      foreach (var balancePair in inCurrencies)
      {
        if (balancePair.Currency == CurrencyCodes.USD) result += balancePair.Amount;
        else
          result += balancePair.Amount / (decimal)Rate.GetRateThisDayOrBefore(balancePair.Currency, dateTime);
      }
      return Math.Round(result*100)/100;
    }

    public static IEnumerable<BalancePair> AccountBalancePairs(Account balancedAccount, Period period)
    {
      var trs = (from t in Db.Transactions
                 where t.Credit.IsTheSameOrDescendantOf(balancedAccount.Name)
                       || t.Debet.IsTheSameOrDescendantOf(balancedAccount.Name)
                 select t).ToList();

      if (trs.Count > 0)
      {
        
      }

      var tempBalance =
        (from t in Db.Transactions
         where period.IsDateTimeIn(t.Timestamp) &&
               (t.Credit.IsTheSameOrDescendantOf(balancedAccount.Name) && !t.Debet.IsTheSameOrDescendantOf(balancedAccount.Name) ||
               (t.Debet.IsTheSameOrDescendantOf(balancedAccount.Name) && !t.Credit.IsTheSameOrDescendantOf(balancedAccount.Name)))
         group t by t.Currency into g
         select new BalancePair
                    {
                      Currency = g.Key,
                      Amount = g.Sum(a => a.Amount * a.SignForAmount(balancedAccount))
                    }).
        Concat
        (from t in Db.Transactions
         // учесть вторую сторону обмена - приход денег в другой валюте
         where t.Amount2 != 0 && period.IsDateTimeIn(t.Timestamp) &&
               (t.Credit.IsTheSameOrDescendantOf(balancedAccount.Name) ||
                                           t.Debet.IsTheSameOrDescendantOf(balancedAccount.Name))
         group t by t.Currency2 into g
         select new BalancePair
                    {
                      Currency = (CurrencyCodes)g.Key,
                      Amount = g.Sum(a => a.Amount2 * a.SignForAmount(balancedAccount) * -1)
                    });

      return from b in tempBalance
             group b by b.Currency into g
             select new BalancePair
                             {
                               Currency = g.Key,
                               Amount = g.Sum(a => a.Amount)
                             };
    }

    private static IEnumerable<BalancePair> ArticleBalancePairs(Account balancedAccount, Period period)
    {
      return from t in Db.Transactions
             where t.Article != null && t.Article.IsTheSameOrDescendantOf(balancedAccount.Name) && period.IsDateTimeIn(t.Timestamp)
             group t by t.Currency into g
             select new BalancePair
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

    public static List<string> CalculateDayResults(DateTime dt)
    {
      var dayResults = new List<string> { String.Format("                              {0:dd MMMM yyyy}", dt.Date) };

      var incomes = from t in Db.Transactions
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

      var expense = from t in Db.Transactions
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

    public static string EndDayBalances(DateTime dt)
    {
      var period = new Period(new DateTime(0), dt.Date.AddDays(1).AddMinutes(-1));
      var result = String.Format(" На конец {0:dd MMMM yyyy} :   ", dt.Date);

      var depo = (from a in Db.AccountsPlaneList
                  where a.Name == "Депозиты"
                  select a).First();
      var calculatedAccounts = new List<Account>(UsefulLists.MyAccountsForShopping);
      calculatedAccounts.Add(depo);
      foreach (var account in calculatedAccounts)
      {
        var pairs = AccountBalancePairs(account, period).ToList();
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
