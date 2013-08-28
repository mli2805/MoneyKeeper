using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.ViewModels;

namespace Keeper.Utils
{
  public class Balance
  {
    public static KeeperDb Db { get { return IoC.Get<KeeperDb>(); } }

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
      var result = BalancePairsToUsd(inCurrencies, dateTime);
      return Math.Round(result*100)/100;
    }

    public static decimal BalancePairsToUsd(IEnumerable<BalancePair> inCurrencies, DateTime dateTime)
    {
      decimal result = 0;
      foreach (var balancePair in inCurrencies)
      {
        if (balancePair.Currency == CurrencyCodes.USD) result += balancePair.Amount;
        else
          result += balancePair.Amount / (decimal)Rate.GetRateThisDayOrBefore(balancePair.Currency, dateTime);
      }
      return result;
    }

    /// <summary>
    /// остатки за каждый день периода, 
    /// даже если в какой-то день не было движения по счету
    /// </summary>
    /// <param name="balancedAccount"></param>
    /// <param name="period"></param>
    /// <returns></returns>
    public static Dictionary<DateTime,decimal> AccountBalancesForPeriodInUsd(Account balancedAccount, Period period)
    {
      var result = new Dictionary<DateTime, decimal>();

      decimal balance = 0;
      foreach (DateTime day in period)
      {
        // получаем обороты по счету за 1 день
        var oneDayInCurrencies = AccountBalancePairs(balancedAccount,
                                                     new Period(day.Date, day.Date.AddDays(1).AddSeconds(-1)));

        // и нарастающим итогом сохраняем в массив
        var oneDayResult = BalancePairsToUsd(oneDayInCurrencies, day);
        if (oneDayResult == 0) continue;
        balance += oneDayResult;
        result.Add(day, Math.Round(balance*100)/100);
      }

      return result;
    }


    public static decimal ConvertAllCurrenciesToUsd(Dictionary<CurrencyCodes, decimal> balances, DateTime date)
    {
      decimal inUsd = 0;
      foreach (var balance in balances)
      {
        inUsd += Rate.GetUsdEquivalent(balance.Value, balance.Key, date);
      }
      return inUsd;
    }

    public static Dictionary<DateTime, Dictionary<CurrencyCodes, decimal>> 
                          AccountBalancesForPeriodInCurrencies(Account balancedAccount, Period period)
    {
      var result = new Dictionary<DateTime, Dictionary<CurrencyCodes, decimal>>();
      var balanceInCurrencies = new Dictionary<CurrencyCodes, decimal>();
      var currentDate = period.GetStart();

      foreach (var transaction in Db.Transactions)
      {
        if (currentDate != transaction.Timestamp.Date)
        {
          result.Add(currentDate, new Dictionary<CurrencyCodes, decimal>(balanceInCurrencies));
          currentDate = currentDate.AddDays(1);
          while (currentDate != transaction.Timestamp.Date)
          {
            //  result.Add(currentDate, balance); добавлять если не изменился остаток
            currentDate = currentDate.AddDays(1);
          }
        }

        if (transaction.Debet.IsTheSameOrDescendantOf(balancedAccount))
        {
          if (!balanceInCurrencies.ContainsKey(transaction.Currency))
            balanceInCurrencies.Add(transaction.Currency, -transaction.Amount);
          else balanceInCurrencies[transaction.Currency] -= transaction.Amount;
          if (transaction.Amount2 != 0)
          {
            if (!balanceInCurrencies.ContainsKey((CurrencyCodes) transaction.Currency2))
              balanceInCurrencies.Add((CurrencyCodes) transaction.Currency2, transaction.Amount2);
            else balanceInCurrencies[(CurrencyCodes) transaction.Currency2] += transaction.Amount2;
          }
        }

        if (transaction.Credit.IsTheSameOrDescendantOf(balancedAccount))
        {
          if (!balanceInCurrencies.ContainsKey(transaction.Currency)) balanceInCurrencies.Add(transaction.Currency, transaction.Amount);
          else balanceInCurrencies[transaction.Currency] += transaction.Amount;
          if (transaction.Amount2 != 0)
          {
            if (!balanceInCurrencies.ContainsKey((CurrencyCodes)transaction.Currency2))
              balanceInCurrencies.Add((CurrencyCodes)transaction.Currency2, -transaction.Amount2);
            else balanceInCurrencies[(CurrencyCodes)transaction.Currency2] -= transaction.Amount2;
          }
        }

      }
      return result;
    }

    public static Dictionary<DateTime, decimal> AccountBalancesForPeriodInUsdThirdWay(Account balancedAccount, Period period)
    {
      var result = new Dictionary<DateTime, decimal>();
      var balanceInCurrencies = new Dictionary<CurrencyCodes, decimal>();
      var currentDate = period.GetStart();

      foreach (var transaction in Db.Transactions)
      {
        if (currentDate != transaction.Timestamp.Date)
        {
          result.Add(currentDate, ConvertAllCurrenciesToUsd(balanceInCurrencies,currentDate));
          currentDate = currentDate.AddDays(1);
          while (currentDate != transaction.Timestamp.Date)
          {
            //            result.Add(currentDate, balance); добавлять если не изменился остаток
            currentDate = currentDate.AddDays(1);
          }
        }

        if (transaction.Debet.IsTheSameOrDescendantOf(balancedAccount))
        {
          if (!balanceInCurrencies.ContainsKey(transaction.Currency)) balanceInCurrencies.Add(transaction.Currency, -transaction.Amount);
          else balanceInCurrencies[transaction.Currency] -= transaction.Amount;
          if (transaction.Amount2 != 0)
          {
            if (!balanceInCurrencies.ContainsKey((CurrencyCodes)transaction.Currency2))
              balanceInCurrencies.Add((CurrencyCodes)transaction.Currency2, transaction.Amount2);
            else balanceInCurrencies[(CurrencyCodes) transaction.Currency2] += transaction.Amount2;
          }
        }

        if (transaction.Credit.IsTheSameOrDescendantOf(balancedAccount))
        {
          if (!balanceInCurrencies.ContainsKey(transaction.Currency)) balanceInCurrencies.Add(transaction.Currency, transaction.Amount);
          else balanceInCurrencies[transaction.Currency] += transaction.Amount;
          if (transaction.Amount2 != 0)
          {
            if (!balanceInCurrencies.ContainsKey((CurrencyCodes)transaction.Currency2))
              balanceInCurrencies.Add((CurrencyCodes)transaction.Currency2, -transaction.Amount2);
            else balanceInCurrencies[(CurrencyCodes)transaction.Currency2] -= transaction.Amount2;
          }
        }
      }
      result.Add(currentDate, ConvertAllCurrenciesToUsd(balanceInCurrencies, currentDate));
      return result;
    }

    /// <summary>
    /// Second way to build daily balances
    /// 
    /// This way doesn't consider excange rate differences!!!
    /// </summary>
    /// <param name="balancedAccount"></param>
    /// <param name="period"></param>
    /// <returns></returns>
    public static Dictionary<DateTime,decimal> AccountBalancesForPeriodInUsdSecondWay(Account balancedAccount, Period period)
    {
      var result = new Dictionary<DateTime, decimal>();

      var currentDate = period.GetStart();
      decimal balance = 0;
      foreach (var transaction in Db.Transactions)
      {
        if (currentDate != transaction.Timestamp.Date)
        {
          result.Add(currentDate,balance);
          currentDate = currentDate.AddDays(1);
          while (currentDate != transaction.Timestamp.Date)
          {
//            result.Add(currentDate, balance); добавлять если не изменился остаток
            currentDate = currentDate.AddDays(1);
          }
        }

        if (transaction.Debet.IsTheSameOrDescendantOf(balancedAccount))
        {
          balance -= Rate.GetUsdEquivalent(transaction.Amount, transaction.Currency, transaction.Timestamp);
          if (transaction.Amount2 != 0)
            balance += Rate.GetUsdEquivalent(transaction.Amount2, (CurrencyCodes)transaction.Currency2, transaction.Timestamp);
        }

        if (transaction.Credit.IsTheSameOrDescendantOf(balancedAccount))
        {
          balance += Rate.GetUsdEquivalent(transaction.Amount, transaction.Currency, transaction.Timestamp);
          if (transaction.Amount2 != 0)
            balance -= Rate.GetUsdEquivalent(transaction.Amount2, (CurrencyCodes)transaction.Currency2, transaction.Timestamp);
        }

      }
      result.Add(currentDate, balance);
      return result;
    }


    /// <summary>
    /// First way to build daily balances
    /// 
    /// This way doesn't consider excange rate differences!!!
    /// </summary>
    /// <param name="balancedAccount"></param>
    /// <param name="period"></param>
    /// <returns></returns>
    public static IEnumerable<BalancePair> AccountBalancePairs(Account balancedAccount, Period period)
    {
      var tempBalance =
        (from t in Db.Transactions
         where period.IsDateTimeIn(t.Timestamp) &&
            (t.Credit.IsTheSameOrDescendantOf(balancedAccount) && !t.Debet.IsTheSameOrDescendantOf(balancedAccount) ||
             (t.Debet.IsTheSameOrDescendantOf(balancedAccount) && !t.Credit.IsTheSameOrDescendantOf(balancedAccount)))
         group t by t.Currency into g
         select new BalancePair
                    {
                      Currency = g.Key,
                      Amount = g.Sum(a => a.Amount * a.SignForAmount(balancedAccount))
                    }).
        Concat // учесть вторую сторону обмена - приход денег в другой валюте
        (from t in Db.Transactions
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

    private static List<string> OneBalance(Account balancedAccount, Period period, out decimal totalInUsd)
    {
      var balance = new List<string>();
      totalInUsd = 0;
      if (balancedAccount == null) return balance;

      bool kind = balancedAccount.IsTheSameOrDescendantOf("Все доходы") || balancedAccount.IsTheSameOrDescendantOf("Все расходы");
      var balancePairs = kind ? ArticleBalancePairs(balancedAccount, period) : AccountBalancePairs(balancedAccount, period);

      foreach (var item in balancePairs)
      {
        if (item.Amount != 0) balance.Add(String.Format("{0:#,#} {1}", item.Amount, item.Currency));
        totalInUsd += Rate.GetUsdEquivalent(item.Amount, item.Currency, period.GetFinish());
      }

      return balance;
    }

    /// <summary>
    /// Функция нужна только заполнения для 2-й рамки на ShellView
    /// Расчитываются остатки по счету и его потомкам 1-го поколения
    /// </summary>
    public static decimal CountBalances(Account selectedAccount, Period period, ObservableCollection<string> balanceList)
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
