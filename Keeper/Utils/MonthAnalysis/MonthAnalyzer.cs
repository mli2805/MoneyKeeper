﻿using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Keeper.DomainModel;
using Keeper.Utils.Accounts;
using Keeper.Utils.Balances;
using Keeper.Utils.Rates;

namespace Keeper.Utils.MonthAnalysis
{
  [Export]
  class MonthAnalyzer
  {
    public Saldo Result { get; set; }

    private readonly KeeperDb _db;
    private readonly RateExtractor _rateExtractor;
    private readonly MonthForecaster _monthForecaster;
    private readonly BalanceForMonthAnalysisCalculator _balanceCalculator;
    private readonly AccountTreeStraightener _accountTreeStraightener;

    [ImportingConstructor]
    public MonthAnalyzer(KeeperDb db, BalanceForMonthAnalysisCalculator balanceCalculator, AccountTreeStraightener accountTreeStraightener,
      RateExtractor rateExtractor, MonthForecaster monthForecaster)
    {
      _db = db;
      _balanceCalculator = balanceCalculator;
      _accountTreeStraightener = accountTreeStraightener;
      _rateExtractor = rateExtractor;
      _monthForecaster = monthForecaster;

      Result = new Saldo();
    }

    private IEnumerable<Transaction> GetMonthTransactionsForAnalysis(OperationType operationType,
                                          DateTime someDate, IEnumerable<Transaction> transactions)
    {
      return (from transaction in transactions
              where transaction.Operation == operationType &&
              transaction.Timestamp.Month == someDate.Month && transaction.Timestamp.Year == someDate.Year
              select transaction);
    }

    private IEnumerable<Transaction> GetMonthTransactionsForAnalysis(
                                          DateTime someDate, IEnumerable<Transaction> transactions)
    {
      return (from transaction in transactions
              where transaction.Timestamp.Month == someDate.Month && transaction.Timestamp.Year == someDate.Year
              select transaction);
    }

    private void RegisterIncome(Transaction transaction)
    {
      var amountInUsd = _rateExtractor.GetUsdEquivalent(transaction.Amount, transaction.Currency, transaction.Timestamp);
      if (transaction.Credit.Deposit != null)
      {
        Result.Incomes.OnDeposits.Transactions.Add(transaction);
        Result.Incomes.OnDeposits.TotalInUsd += amountInUsd;
      }
      else
      {
        Result.Incomes.OnHands.Transactions.Add(transaction);
        Result.Incomes.OnHands.TotalInUsd += amountInUsd;
      }
    }

    private void RegisterExpense(IEnumerable<Transaction> expenseTransactions)
    {
      var expenseTransactionsInUsd = ConvertTransactionsQuery(expenseTransactions);
      GroupExpenseByCategories(expenseTransactionsInUsd);
      Result.Expense.TotalInUsd = expenseTransactionsInUsd.Sum(t => t.AmountInUsd);

      var largeTransactions = expenseTransactionsInUsd.Where(transaction => transaction.AmountInUsd > 50);
      foreach (var transaction in largeTransactions)
      {
        Result.Expense.LargeTransactions.Add(transaction);
      }
      Result.Expense.TotalForLargeInUsd = largeTransactions.Sum(t => t.AmountInUsd);
    }

    private void GroupExpenseByCategories(IEnumerable<ConvertedTransaction> expenseTransactionsInUsd)
    {
      var expenseRoot = _accountTreeStraightener.Seek("Все расходы", _db.Accounts);
      foreach (var expense in expenseRoot.Children)
      {
        var amountInUsd = (from e in expenseTransactionsInUsd
                           where e.Article.Is(expense.Name)
                           select e).Sum(a => a.AmountInUsd);

        if (amountInUsd != 0)
          Result.Expense.Categories.Add(new BalanceTrio { Amount = amountInUsd, Currency = CurrencyCodes.USD, MyAccount = expense });
      }
    }

    /// <summary>
    /// работает гораздо быстрее чем ConvertTransactions
    /// но если нет курса за день операции, не может взять предшествующий курс
    /// </summary>
    /// <param name="expenseTransactions"></param>
    /// <returns></returns>
    private IEnumerable<ConvertedTransaction> ConvertTransactionsQuery(IEnumerable<Transaction> expenseTransactions)
    {
      return from t in expenseTransactions
             join r in _db.CurrencyRates
               on new { t.Timestamp.Date, t.Currency } equals new { r.BankDay.Date, r.Currency } into g
             from rate in g.DefaultIfEmpty()
             select new ConvertedTransaction
                      {
                        Timestamp = t.Timestamp,
                        Amount = t.Amount,
                        Currency = t.Currency,
                        Article = t.Article,
                        AmountInUsd = rate != null ? t.Amount / (decimal)rate.Rate : t.Amount,
                        Comment = t.Comment
                      };
    }

    /// <summary>
    /// работает гораздо медленнее чем через запрос
    /// при переходе между месяцами заметна пауза
    /// </summary>
    /// <param name="expenseTransactions"></param>
    /// <returns></returns>
    private IEnumerable<ConvertedTransaction> ConvertTransactions(IEnumerable<Transaction> expenseTransactions)
    {
      foreach (var t in expenseTransactions)
      {
        yield return new ConvertedTransaction
        {
          Timestamp = t.Timestamp,
          Amount = t.Amount,
          Currency = t.Currency,
          Article = t.Article,
          AmountInUsd = t.Currency != CurrencyCodes.USD ?
            t.Amount / (decimal)_rateExtractor.GetRateThisDayOrBefore(t.Currency, t.Timestamp)
            : t.Amount,
          Comment = t.Comment
        };
      }
    }

    private void RegisterDepositsTraffic(List<Transaction> allMonthTransactions)
    {
      Result.TransferFromDeposit = allMonthTransactions.
        Where(t => t.Debet.IsDeposit() && !t.Credit.IsDeposit()).
        Sum(t => _rateExtractor.GetUsdEquivalent(t.Amount, t.Currency, t.Timestamp));

      Result.TransferToDeposit = allMonthTransactions.
        Where(t => !t.Debet.IsDeposit() && t.Credit.IsDeposit()).
        Sum(t => _rateExtractor.GetUsdEquivalent(t.Amount, t.Currency, t.Timestamp)) - Result.Incomes.OnDeposits.TotalInUsd;
    }

    private List<CurrencyRate> InitializeRates(DateTime date)
    {
      var result = new List<CurrencyRate>();
      var currencyList = Enum.GetValues(typeof(CurrencyCodes)).OfType<CurrencyCodes>();
      foreach (CurrencyCodes currencyCode in currencyList)
      {
        if (currencyCode != CurrencyCodes.USD) result.Add(_rateExtractor.FindRateForDateOrBefore(currencyCode, date));
      }
      return result;
    }

    public Saldo AnalizeMonth(DateTime initialDay)
    {
      Result = new Saldo();
      Result.StartDate = initialDay.AddDays(-initialDay.Day + 1);
      Result.BeginBalance = _balanceCalculator.GetExtendedBalanceBeforeDate(Result.StartDate);
      Result.BeginRates = InitializeRates(Result.StartDate);

      var incomeTransactions = GetMonthTransactionsForAnalysis(OperationType.Доход, Result.StartDate, _db.Transactions);
      foreach (var transaction in incomeTransactions) RegisterIncome(transaction);

      RegisterExpense(GetMonthTransactionsForAnalysis(OperationType.Расход, Result.StartDate, _db.Transactions));
//      RegisterDepositsTraffic(GetMonthTransactionsForAnalysis(OperationType.Перенос, Result.StartDate, _db.Transactions).ToList());
      RegisterDepositsTraffic(GetMonthTransactionsForAnalysis(Result.StartDate, _db.Transactions).ToList());

      Result.EndBalance = _balanceCalculator.GetExtendedBalanceBeforeDate(Result.StartDate.AddMonths(1));

      Result.EndRates = InitializeRates(Result.StartDate.AddMonths(1));

      _monthForecaster.CollectEstimates(Result);

      return Result;
    }

  }
}
