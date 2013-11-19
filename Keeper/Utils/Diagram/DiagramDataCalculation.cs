﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Keeper.DomainModel;

namespace Keeper.Utils.Diagram
{
  class DiagramDataCalculation
  {
    private readonly KeeperDb _db;
    private readonly RateExtractor _rateExtractor;

		[ImportingConstructor]
    public DiagramDataCalculation(KeeperDb db)
		{
			_db = db;
			_rateExtractor = new RateExtractor(db);
		}

    #region Core calculations

    public decimal ConvertAllCurrenciesToUsd(Dictionary<CurrencyCodes, decimal> amounts, DateTime date)
    {
      decimal inUsd = 0;
      foreach (var amount in amounts)
      {
        inUsd += _rateExtractor.GetUsdEquivalent(amount.Value, amount.Key, date);
      }
      return inUsd;
    }

    public Dictionary<DateTime, Dictionary<CurrencyCodes, decimal>>
      AccountBalancesForPeriodInCurrencies(Account balancedAccount, Period period)
    {
      var result = new Dictionary<DateTime, Dictionary<CurrencyCodes, decimal>>();
      var balanceInCurrencies = new Dictionary<CurrencyCodes, decimal>();
      var currentDate = period.GetStart();

      foreach (var transaction in _db.Transactions)
      {
        if (currentDate != transaction.Timestamp.Date)
        {
          result.Add(currentDate, new Dictionary<CurrencyCodes, decimal>(balanceInCurrencies));
          currentDate = currentDate.AddDays(1);
          while (currentDate < transaction.Timestamp.Date)
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

    // получение остатка по счету за каждую дату периода [,когда были операции]
    // реализовано не через функцию получения остатка на дату, вызванную для дат периода
    // а за один проход по БД с получением остатков накопительным итогом, т.к. гораздо быстрее
    public Dictionary<DateTime, decimal> AccountBalancesForPeriodInUsdThirdWay(Account balancedAccount, Period period, Every frequency)
    {
      var result = new Dictionary<DateTime, decimal>();
      var balanceInCurrencies = new Dictionary<CurrencyCodes, decimal>();
      var currentDate = period.GetStart();

      foreach (var transaction in _db.Transactions)
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

    public Dictionary<DateTime, decimal> KategoriesTrafficForPeriodInUsd(Account kategory, Period period, Every frequency)
    {
      var result = new Dictionary<DateTime, decimal>();
      decimal movement = 0;
      var currentDate = period.GetStart();

      foreach (var transaction in _db.Transactions)
      {
        if (transaction.Timestamp.Date < period.GetStart()) continue;
        if (transaction.Timestamp.Date > period.GetFinish()) break;

        if (transaction.Timestamp.Date != currentDate)
        {
          if (!FunctionsWithEvery.IsTheSamePeriod(transaction.Timestamp.Date, currentDate, frequency))
          {
            // последняя подходящая операция в периоде может быть когда угодно, а записываем последним числом периода (чтобы диаграмма красивей была)
            result.Add(FunctionsWithEvery.GetLastDayOfTheSamePeriod(currentDate.Date, frequency), movement);
            movement = 0;
          }
          currentDate = transaction.Timestamp.Date;
        }

        if (transaction.Article == null || !transaction.Article.IsTheSameOrDescendantOf(kategory)) continue;

        if (transaction.Debet.IsTheSameOrDescendantOf("Мои"))
          movement -= _rateExtractor.GetUsdEquivalent(transaction.Amount, transaction.Currency, transaction.Timestamp);
        else
          movement += _rateExtractor.GetUsdEquivalent(transaction.Amount, transaction.Currency, transaction.Timestamp);
      }
      result.Add(FunctionsWithEvery.GetLastDayOfTheSamePeriod(currentDate.Date, frequency), movement);
      return result;
    }

    #endregion

    // на сколько изменился остаток по счету за месяц (разница остатка после Nного и N-1го месяцев)
    // годится именно для счетов, (не для категорий, на которых нет остатка, есть движение)
    public Dictionary<DateTime, decimal> MonthlyResults(string accountName)
    {
      var result = new Dictionary<DateTime, decimal>();

      var accountForAnalisys = (from account in _db.AccountsPlaneList where account.Name == accountName select account).FirstOrDefault();
      var balances = AccountBalancesForPeriodInUsdThirdWay(accountForAnalisys, 
                                                           new Period(new DateTime(2001, 12, 31), DateTime.Today, true), 
                                                           Every.Month).OrderBy(pair => pair.Key).ToList();

      for (var i = 1; i < balances.Count; i++)
      {
        result.Add(balances[i].Key, balances[i].Value - balances[i - 1].Value);
      }
      return result;
    }

    // какие обороты были по счету за месяц
    // применяется для счетов - категорий

    public Dictionary<DateTime, decimal> MonthlyTraffic(string accountName)
    {
      var kategory = (from account in _db.AccountsPlaneList where account.Name == accountName select account).FirstOrDefault();

      return KategoriesTrafficForPeriodInUsd(kategory, new Period(new DateTime(2002, 1, 1), DateTime.Today, true), Every.Month);
    }

    private static int MonthCountFromStart(DateTime date)
    {
      return (date.Year - 2002) * 12 + date.Month;
    }

    public void AverageMonthlyResults(Dictionary<DateTime, decimal> monthlyResults)
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
        averageFromStartDictionary.Add(monthSaldo.Key, Math.Round(averageFromStart / MonthCountFromStart(monthSaldo.Key)));

        if (monthSaldo.Key.Month == 1) averageFromJanuary = 0;
        averageFromJanuary += monthSaldo.Value;
        averageFromJanuaryDictionary.Add(monthSaldo.Key, Math.Round(averageFromJanuary / monthSaldo.Key.Month));

        if (last12Months.Count < 12) last12Months.Add(monthSaldo.Key, monthSaldo.Value);
        else
        {
          var minDate = last12Months.Min(pair => pair.Key);
          last12Months.Remove(minDate);
          last12Months.Add(monthSaldo.Key, monthSaldo.Value);
          decimal averageForLast12Months = last12Months.Sum(pair => pair.Value) / 12;
          averageForLast12MonthsDictionary.Add(monthSaldo.Key, averageForLast12Months);
        }
      }
    }

  }
}
