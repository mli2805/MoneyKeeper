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
              where transaction.Timestamp.Month == someDate.Month && transaction.Timestamp.Year == someDate.Year
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

    private void RegisterTransaction(Transaction transaction)
    {
      if (transaction.Operation == OperationType.Доход) RegisterIncome(transaction);
      if (transaction.Operation == OperationType.Расход) RegisterExpense(transaction);
      if (transaction.Operation == OperationType.Перенос) RegisterTransfer(transaction);
    }

    private void RegisterIncome(Transaction transaction)
    {
      var amountInUsd = _rateExtractor.GetUsdEquivalent(transaction.Amount, transaction.Currency, transaction.Timestamp);
      if (transaction.Credit.Is("Депозиты"))
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

    private void RegisterExpense(Transaction transaction)
    {
      var amountInUsd = _rateExtractor.GetUsdEquivalent(transaction.Amount, transaction.Currency, transaction.Timestamp);
      Result.Expense.TotalInUsd += amountInUsd;
      if (amountInUsd >= 50)
      {
        Result.Expense.LargeTransactions.Add(transaction);
        Result.Expense.TotalForLargeInUsd += amountInUsd;
      }
     
    }
    private void RegisterTransfer(Transaction transaction){}

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
	    Result.BeginBalance = InitializeWithBalanceBeforeDate(Result.StartDate);
      Result.BeginRates = InitializeRates(Result.StartDate);

	    var transactions = GetMonthTransactionsForAnalysis(Result.StartDate, _db.Transactions);

      foreach (var transaction in transactions)
      { 
        RegisterTransaction(transaction);
      }

	    Result.EndBalance = InitializeWithBalanceBeforeDate(Result.StartDate.AddMonths(1));

      Result.EndRates = InitializeRates(Result.StartDate.AddMonths(1));

      return Result;
    }

  }
}
