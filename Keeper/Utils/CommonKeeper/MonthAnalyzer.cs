﻿using System;
using System.Composition;
using System.Linq;

using Keeper.DomainModel;
using Keeper.Utils.Balances;
using Keeper.Utils.Rates;

namespace Keeper.Utils.CommonKeeper
{
	[Export]
  class MonthAnalyzer
  {
    private readonly KeeperDb _db;
    private readonly RateExtractor _rateExtractor;
		readonly ICurrencyConverter _currencyConverter;
		private readonly BalanceCalculator _balanceCalculator;

    [ImportingConstructor]
    public MonthAnalyzer(KeeperDb db, BalanceCalculator balanceCalculator, RateExtractor rateExtractor, ICurrencyConverter currencyConverter)
    {
	    _db = db;
	    _balanceCalculator = balanceCalculator;
	    _rateExtractor = rateExtractor;
	    _currencyConverter = currencyConverter;
    }

	  public Saldo AnalizeMonth(DateTime initialDay)
    {
      var myAccountsRoot = (from account in _db.Accounts
                            where account.Name == "Мои"
                            select account).FirstOrDefault();
      var result = new Saldo();

      result.StartDate = initialDay.AddDays(-initialDay.Day + 1);
      result.BeginBalanceInCurrencies = _balanceCalculator.AccountBalancePairsBeforeDay(myAccountsRoot, result.StartDate).ToList();
      result.BeginBalance = _currencyConverter.BalancePairsToUsd(result.BeginBalanceInCurrencies, result.StartDate.AddDays(-1));
      result.BeginByrRate = (decimal)_rateExtractor.GetRateThisDayOrBefore(CurrencyCodes.BYR, result.StartDate.AddDays(-1));

      var transactions = (from transaction in _db.Transactions
                          where (transaction.Operation == OperationType.Доход ||
                             transaction.Operation == OperationType.Расход) &&
                            transaction.Timestamp.Month == result.StartDate.Month &&
                             transaction.Timestamp.Year == result.StartDate.Year
                          select transaction);
      foreach (var transaction in transactions)
      {
        var amountInUsd = _rateExtractor.GetUsdEquivalent(transaction.Amount, transaction.Currency, transaction.Timestamp);
        if (transaction.Operation == OperationType.Доход)
          result.Incomes += amountInUsd;
        else
        {
          result.Expense += amountInUsd;
          if (amountInUsd >= 50) result.LargeExpense += amountInUsd;
        }
      }

      result.EndBalanceInCurrencies =
        _balanceCalculator.AccountBalancePairsBeforeDay(myAccountsRoot, result.StartDate.AddMonths(1)).ToList();
      result.EndBalance = _currencyConverter.BalancePairsToUsd(result.EndBalanceInCurrencies,
                                                    result.StartDate.AddMonths(1).AddDays(-1));
      if (!transactions.Any())
      {
        result.LastDayWithTransactionsInMonth = result.StartDate;
        result.EndByrRate = result.BeginByrRate;
      }
      else
      {
        var lastTransaction = transactions.Last();
        result.LastDayWithTransactionsInMonth = lastTransaction.Timestamp.Date;
        result.EndByrRate = (decimal)_rateExtractor.GetRate(CurrencyCodes.BYR, lastTransaction.Timestamp);
      }

      return result;
    }

  }
}