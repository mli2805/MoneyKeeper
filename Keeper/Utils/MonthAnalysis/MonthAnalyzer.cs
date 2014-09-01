﻿using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Keeper.DomainModel;
using Keeper.Utils.Accounts;
using Keeper.Utils.Balances;
using Keeper.Utils.CommonKeeper;
using Keeper.Utils.DbInputOutput.TxtTasks;
using Keeper.Utils.Rates;

namespace Keeper.Utils.MonthAnalysis
{
  [Export]
  class MonthAnalyzer
  {
    public Saldo Result { get; set; }

    private readonly KeeperDb _db;
    private readonly RateExtractor _rateExtractor;
    private readonly TransactionSetConvertor _transactionSetConvertor;
    private readonly MonthForecaster _monthForecaster;
    private readonly BalanceForMonthAnalysisCalculator _balanceCalculator;
    private readonly AccountTreeStraightener _accountTreeStraightener;

    [ImportingConstructor]
    public MonthAnalyzer(KeeperDb db, BalanceForMonthAnalysisCalculator balanceCalculator, AccountTreeStraightener accountTreeStraightener,
      RateExtractor rateExtractor, TransactionSetConvertor transactionSetConvertor, MonthForecaster monthForecaster)
    {
      _db = db;
      _balanceCalculator = balanceCalculator;
      _accountTreeStraightener = accountTreeStraightener;
      _rateExtractor = rateExtractor;
      _transactionSetConvertor = transactionSetConvertor;
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
      var expenseTransactionsInUsd = _transactionSetConvertor.ConvertTransactionsQuery(expenseTransactions);
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

 
    private void RegisterDepositsTraffic(List<Transaction> allMonthTransactions)
    {
      Result.TransferFromDeposit = allMonthTransactions.
        Where(t => t.Debet.IsDeposit() && !t.Credit.IsDeposit() && t.Operation != OperationType.Обмен).
        Sum(t => _rateExtractor.GetUsdEquivalent(t.Amount, t.Currency, t.Timestamp));

      Result.TransferToDeposit = allMonthTransactions.
        Where(t => !t.Debet.IsDeposit() && t.Credit.IsDeposit() && t.Operation != OperationType.Обмен).
        Sum(t => _rateExtractor.GetUsdEquivalent(t.Amount, t.Currency, t.Timestamp)) - Result.Incomes.OnDeposits.TotalInUsd;
    }

    private void GetDepositsTraffic(List<Transaction> allMonthTransactions)
    {
      var minus = allMonthTransactions.
        Where(t => t.Debet.IsDeposit() && !t.Credit.IsDeposit() && t.Operation != OperationType.Обмен).ToList();

      foreach (var transaction in minus)
      {
        var dd = new DbClassesInstanceDumper();
        Console.WriteLine(dd.Dump(transaction));
      }  

      var plus = allMonthTransactions.
        Where(t => !t.Debet.IsDeposit() && t.Credit.IsDeposit() && t.Operation != OperationType.Обмен).ToList();
     
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

      GetDepositsTraffic(GetMonthTransactionsForAnalysis(Result.StartDate, _db.Transactions).ToList());
      RegisterDepositsTraffic(GetMonthTransactionsForAnalysis(Result.StartDate, _db.Transactions).ToList());

      Result.EndBalance = _balanceCalculator.GetExtendedBalanceBeforeDate(Result.StartDate.AddMonths(1));

      Result.EndRates = InitializeRates(Result.StartDate.AddMonths(1));

      _monthForecaster.CollectEstimates(Result);

      return Result;
    }

  }
}
