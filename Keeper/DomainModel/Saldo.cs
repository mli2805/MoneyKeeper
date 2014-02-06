using System;
using System.Collections.Generic;
using Keeper.Utils.Balances;

namespace Keeper.DomainModel
{
  public class ExtendedBalance
  {
    public List<MoneyPair> InCurrencies { get; set; }
    public decimal TotalInUsd { get; set; }
  }

  public class ExtendedBalanceForAnalysis
  {
    public ExtendedBalance Common { get; set; }
    public ExtendedBalance OnHands { get; set; }
    public ExtendedBalance OnDeposits { get; set; }
  }

  public class ExtendedTraffic
  {
    public List<Transaction> Transactions { get; set; }
    public decimal TotalInUsd { get; set; }

    public ExtendedTraffic()
    {
      Transactions = new List<Transaction>();
    }
  }

  public class ExtendedIncomes
  {
    public ExtendedTraffic OnDeposits { get; set; }
    public ExtendedTraffic OnHands { get; set; }
    public decimal TotalInUsd { get { return OnDeposits.TotalInUsd + OnHands.TotalInUsd; } }

    public ExtendedIncomes()
    {
      OnHands = new ExtendedTraffic();
      OnDeposits = new ExtendedTraffic(); 
    }
  }

  public class ConvertedTransaction
  {
    public DateTime Timestamp { get; set; }
    public decimal Amount { get; set; }
    public CurrencyCodes Currency { get; set; }
    public decimal AmountInUsd { get; set; }
    public Account Article { get; set; }
    public string Comment { get; set; }
 }

  public class ExtendedTrafficWithCategories
  {
    public List<ConvertedTransaction> LargeTransactions { get; set; }
    public List<BalanceTrio> Categories { get; set; }
    public decimal TotalForLargeInUsd { get; set; }
    public decimal TotalInUsd { get; set; }

    public ExtendedTrafficWithCategories()
    {
      LargeTransactions = new List<ConvertedTransaction>();
      Categories = new List<BalanceTrio>();
    }
  }

  public class EstimatedMoney
  {
    public decimal Amount { get; set; }
    public CurrencyCodes Currency { get; set; }
    public string ArticleName { get; set; }
  }

  public class EstimatedIncomes
  {
    public List<EstimatedMoney> Incomes { get; set; }
    public decimal EstimatedIncomesSum { get; set; }
    public decimal TotalInUsd { get; set; }

    public EstimatedIncomes()
    {
      Incomes = new List<EstimatedMoney>();
    }
  }

  public class Saldo
  {
    public DateTime StartDate { get; set; }
    public ExtendedBalanceForAnalysis BeginBalance { get; set; }
    public List<CurrencyRate> BeginRates { get; set; }
    public ExtendedIncomes Incomes { get; set; }
    public ExtendedTrafficWithCategories Expense { get; set; }

    public decimal ExchangeDifference { get { return EndBalance.Common.TotalInUsd - BeginBalance.Common.TotalInUsd - Incomes.TotalInUsd + Expense.TotalInUsd; } }
    public decimal ExchangeDepositDifference { get
      { return EndBalance.OnDeposits.TotalInUsd - BeginBalance.OnDeposits.TotalInUsd - Incomes.OnDeposits.TotalInUsd - TransferToDeposit + TransferFromDeposit; } }

    public ExtendedBalanceForAnalysis EndBalance { get; set; }
    public List<CurrencyRate> EndRates { get; set; }

    public decimal TransferToDeposit { get; set; }
    public decimal TransferFromDeposit { get; set; }

    public EstimatedIncomes ForecastIncomes { get; set; }
    public decimal ForecastExpense { get; set; }
    public decimal ForecastFinResult { get { return ForecastIncomes.TotalInUsd - ForecastExpense; } }
    public decimal ForecastEndBalance { get { return BeginBalance.Common.TotalInUsd + ForecastFinResult; } }

    public Saldo()
    {
      Incomes = new ExtendedIncomes();
      Expense = new ExtendedTrafficWithCategories();
    }
  }
 
}
