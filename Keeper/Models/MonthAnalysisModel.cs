using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;

using Keeper.DomainModel;
using Keeper.Utils.Accounts;
using Keeper.Utils.Balances;
using Keeper.Utils.Rates;

namespace Keeper.Utils.CommonKeeper
{
	[Export]
  class MonthAnalysisModel
  {
    public Saldo Result { get; set; }

    private readonly KeeperDb _db;
    private readonly RateExtractor _rateExtractor;
		readonly ICurrencyConverter _currencyConverter;
		private readonly BalanceCalculator _balanceCalculator;
	  private readonly AccountTreeStraightener _accountTreeStraightener;

	  [ImportingConstructor]
    public MonthAnalysisModel(KeeperDb db, BalanceCalculator balanceCalculator, AccountTreeStraightener accountTreeStraightener, RateExtractor rateExtractor, ICurrencyConverter currencyConverter)
    {
	    _db = db;
	    _balanceCalculator = balanceCalculator;
      _accountTreeStraightener = accountTreeStraightener;
      _rateExtractor = rateExtractor;
	    _currencyConverter = currencyConverter;

	    Result = new Saldo();
    }

    private IEnumerable<Transaction> GetMonthTransactionsForAnalysis(DateTime someDate, IEnumerable<Transaction> transactions)
    {
      return (from transaction in transactions
              where (transaction.Operation == OperationType.Доход ||
                 transaction.Operation == OperationType.Расход) &&
                transaction.Timestamp.Month == someDate.Month &&
                 transaction.Timestamp.Year == someDate.Year
              select transaction);
    }

    private ExtendedBalance InitializeWithBalanceBeforeDate(DateTime startDay, string accountName)
    {
      var extendedBalance = new ExtendedBalance();
      var account = _accountTreeStraightener.Seek(accountName, _db.Accounts);
      extendedBalance.InCurrencies = _balanceCalculator.AccountBalancePairsBeforeDay(account, startDay).ToList();
      extendedBalance.TotalInUsd = _currencyConverter.BalancePairsToUsd(extendedBalance.InCurrencies, startDay.AddDays(-1));
      return extendedBalance;
    }

    private ExtendedBalanceForAnalysis InitializeWithBalanceBeforeDate(DateTime startDay)
    {
      return new ExtendedBalanceForAnalysis
                                         {
                                           Common = InitializeWithBalanceBeforeDate(startDay, "Мои"),
                                           OnHands = InitializeWithBalanceBeforeDate(startDay, "На руках"),
                                           OnDeposits = InitializeWithBalanceBeforeDate(startDay, "Депозиты")
                                         };
    }

	  public Saldo AnalizeMonth(DateTime initialDay)
	  {
      Result.StartDate = initialDay.AddDays(-initialDay.Day + 1);
	    Result.BeginBalance = InitializeWithBalanceBeforeDate(Result.StartDate);
      Result.BeginByrRate = (decimal)_rateExtractor.GetRateThisDayOrBefore(CurrencyCodes.BYR, Result.StartDate.AddDays(-1));

	    var transactions = GetMonthTransactionsForAnalysis(Result.StartDate, _db.Transactions);
        
      foreach (var transaction in transactions)
      {
        var amountInUsd = _rateExtractor.GetUsdEquivalent(transaction.Amount, transaction.Currency, transaction.Timestamp);
        if (transaction.Operation == OperationType.Доход)
          Result.Incomes += amountInUsd;
        else
        {
          Result.Expense += amountInUsd;
          if (amountInUsd >= 50) Result.LargeExpense += amountInUsd;
        }
      }

	    Result.EndBalance = InitializeWithBalanceBeforeDate(Result.StartDate.AddMonths(1));

      if (!transactions.Any())
      {
        Result.LastDayWithTransactionsInMonth = Result.StartDate;
        Result.EndByrRate = Result.BeginByrRate;
      }
      else
      {
        var lastTransaction = transactions.Last();
        Result.LastDayWithTransactionsInMonth = lastTransaction.Timestamp.Date;
        Result.EndByrRate = (decimal)_rateExtractor.GetRate(CurrencyCodes.BYR, lastTransaction.Timestamp);
      }

      return Result;
    }

  }
}
