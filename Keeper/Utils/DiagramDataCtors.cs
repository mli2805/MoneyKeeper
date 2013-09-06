using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.Utils
{
  public enum Every
  {
    Day,
    Week,
    Month,
    Quarter,
    Year
  }

  class FunctionsWithEvery
  {
    public static bool IsLastDayOf(DateTime date, Every period)
    {
      if (period == Every.Day) return true;
      if (period == Every.Week && date.DayOfWeek == DayOfWeek.Sunday) return true;
      if (period == Every.Month && date.Month != date.AddDays(1).Month) return true;
      if (period == Every.Quarter && date.Month != date.AddDays(1).Month && date.Month % 3 == 0) return true;
      if (period == Every.Year && date.Day == 31 && date.Month == 12) return true;
      return false;
    }
  }

  class DiagramDataCtors
  {
    public static KeeperDb Db { get { return IoC.Get<KeeperDb>(); } }

    #region для диаграмм ежедневные остатки и ежедневное распределение по валютам депозитов
    // получение остатка по счету за каждую дату периода [,когда были операции]
    // реализовано не через функцию получения остатка на дату, вызванную для дат периода
    // а за один проход по БД с получением остатков накопительным итогом, т.к. гораздо быстрее

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

      foreach (var transaction in Balance.Db.Transactions)
      {
        if (currentDate != transaction.Timestamp.Date)
        {
          result.Add(currentDate, new Dictionary<CurrencyCodes, decimal>(balanceInCurrencies));
          currentDate = currentDate.AddDays(1);
          while (currentDate != transaction.Timestamp.Date)
          {
            //  result.Add(currentDate, balance); // раскомментарить, если даты когда не было изменений тоже должны попадать набор
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
            if (!balanceInCurrencies.ContainsKey((CurrencyCodes)transaction.Currency2))
              balanceInCurrencies.Add((CurrencyCodes)transaction.Currency2, transaction.Amount2);
            else balanceInCurrencies[(CurrencyCodes)transaction.Currency2] += transaction.Amount2;
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

    public static Dictionary<DateTime, decimal> AccountBalancesForPeriodInUsdThirdWay(Account balancedAccount, Period period, Every frequency)
    {
      var result = new Dictionary<DateTime, decimal>();
      var balanceInCurrencies = new Dictionary<CurrencyCodes, decimal>();
      var currentDate = period.GetStart();

      foreach (var transaction in Balance.Db.Transactions)
      {
        if (currentDate != transaction.Timestamp.Date)
        {
          if (FunctionsWithEvery.IsLastDayOf(currentDate, frequency)) result.Add(currentDate, ConvertAllCurrenciesToUsd(balanceInCurrencies, currentDate));
          currentDate = currentDate.AddDays(1);
          while (currentDate != transaction.Timestamp.Date)
          {
            // закомментарить часть условия frequency != Every.Day, если надо ежедневно и даты когда не было изменений тоже должны попадать набор
            if (frequency != Every.Day && FunctionsWithEvery.IsLastDayOf(currentDate, frequency)) result.Add(currentDate, ConvertAllCurrenciesToUsd(balanceInCurrencies, currentDate));
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
            else balanceInCurrencies[(CurrencyCodes)transaction.Currency2] += transaction.Amount2;
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

    #endregion 

    #region для диаграммы ЕЖЕМЕСЯЧНОЕ САЛЬДО
    // медленно, возможно придется считать строго ежемесячные результаты в одном цикле, не отвлекаясь на остальные поля Saldo 
    public static Dictionary<DateTime, decimal> MonthlyResultsOld()
    {
      var result = new Dictionary<DateTime, decimal>();
      for (var date = new DateTime(2002, 1, 1); date <= DateTime.Today; date = date.AddMonths(1))
      {
        var saldo = MonthAnalisysCtor.AnalizeMonth(date);
        result.Add(saldo.LastDayWithTransactionsInMonth,saldo.Result);
      }
      return result;
    }

    public static Dictionary<DateTime, decimal> MonthlyResults()
    {
      var result = new Dictionary<DateTime, decimal>();

      var allMyMoney = (from account in Db.Accounts where account.Name == "Мои" select account).FirstOrDefault();
      var balances = DiagramDataCtors.AccountBalancesForPeriodInUsdThirdWay(allMyMoney, new Period(new DateTime(2001, 12, 31), DateTime.Today), Every.Month).OrderBy(pair => pair.Key).ToList();

      for (var i = 1; i < balances.Count; i++)
      {
        result.Add(balances[i].Key, balances[i].Value - balances[i-1].Value);
      }
      return result;
    }


    private static int MonthsFromStart(DateTime date)
    {
      return (date.Year - 2002)*12 + date.Month;
    }

    public static void AverageMonthlyResults(Dictionary<DateTime, decimal> monthlyResults)
    {
      var averageFromStartDictionary = new Dictionary<DateTime, decimal>();
      var averageFromJanuaryDictionary = new Dictionary<DateTime, decimal>();
      var averageForLast12MonthsDictionary = new Dictionary<DateTime, decimal>();

      decimal averageFromStart = 0;
      decimal averageFromJanuary = 0;
      var last12Months = new SortedDictionary<DateTime, decimal>();
      foreach (var monthSaldo in monthlyResults.OrderBy(pair => pair.Key))
      {
        averageFromStart += monthSaldo.Value;
        averageFromStartDictionary.Add(monthSaldo.Key, Math.Round(averageFromStart / MonthsFromStart(monthSaldo.Key)));

        if (monthSaldo.Key.Month == 1) averageFromJanuary = 0;
        averageFromJanuary += monthSaldo.Value;
        averageFromJanuaryDictionary.Add(monthSaldo.Key, Math.Round(averageFromJanuary / monthSaldo.Key.Month));

        if (last12Months.Count < 12) last12Months.Add(monthSaldo.Key, monthSaldo.Value);
        else
        {
          var minDate = last12Months.Min(pair => pair.Key);
          last12Months.Remove(minDate);
          last12Months.Add(monthSaldo.Key, monthSaldo.Value);
          decimal averageForLast12Months = last12Months.Sum(pair => pair.Value)/12;
          averageForLast12MonthsDictionary.Add(monthSaldo.Key, averageForLast12Months);
        }
      }

    }

    #endregion

  }
}
