using System;
using System.Collections.Generic;
using Keeper.Utils.Balances;

namespace Keeper.DomainModel
{
  class ExtendedBalance
  {
    public List<MoneyPair> InCurrencies { get; set; }
    public decimal TotalInUsd { get; set; }
  }

  class ExtendedBalanceForAnalysis
  {
    public ExtendedBalance Common { get; set; }
    public ExtendedBalance OnHands { get; set; }
    public ExtendedBalance OnDeposits { get; set; }
  }

  class ExtendedTraffic
  {
    public List<Transaction> Transactions { get; set; }
    public decimal TotalInUsd { get; set; }

    public ExtendedTraffic()
    {
      Transactions = new List<Transaction>();
    }
  }

  class ExtendedIncomes
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

  class ExtendedTrafficWithCategories
  {
    public List<Transaction> LargeTransactions { get; set; }
    public List<BalanceTrio> Categories { get; set; }
    public decimal TotalForLargeInUsd { get; set; }
    public decimal TotalInUsd { get; set; }

    public ExtendedTrafficWithCategories()
    {
      LargeTransactions = new List<Transaction>();
      Categories = new List<BalanceTrio>();
    }
  }

  class Saldo
  {
    public DateTime StartDate { get; set; }
    public ExtendedBalanceForAnalysis BeginBalance { get; set; }
    public List<CurrencyRate> BeginRates { get; set; }
    public ExtendedIncomes Incomes { get; set; }
    public ExtendedTrafficWithCategories Expense { get; set; }
    public decimal ExchangeDifference { get { return EndBalance.Common.TotalInUsd - BeginBalance.Common.TotalInUsd - Incomes.TotalInUsd + Expense.TotalInUsd; } }
    public ExtendedBalanceForAnalysis EndBalance { get; set; }
    public List<CurrencyRate> EndRates { get; set; }
    public decimal SaldoIncomesExpense { get { return Incomes.TotalInUsd - Expense.TotalInUsd; } }
    public decimal Result { get { return EndBalance.Common.TotalInUsd - BeginBalance.Common.TotalInUsd; } }

    public decimal ForecastIncomes { get; set; }
    public decimal ForecastExpense { get; set; }
    public decimal ForecastFinResult { get { return ForecastIncomes - ForecastExpense; } }
    public decimal ForecastEndBalance { get { return BeginBalance.Common.TotalInUsd + ForecastFinResult; } }

    public Saldo()
    {
      Incomes = new ExtendedIncomes();
      Expense = new ExtendedTrafficWithCategories();
    }
  }
 
}
