using System;
using System.Collections.Generic;
using Keeper.Utils.Balances;

namespace Keeper.DomainModel
{
  class ExtendedBalance
  {
    public decimal TotalInUsd { get; set; }
    public List<MoneyPair> InCurrencies { get; set; }
  }

  class ExtendedBalanceForAnalysis
  {
    public ExtendedBalance Common { get; set; }
    public ExtendedBalance OnHands { get; set; }
    public ExtendedBalance OnDeposits { get; set; }

    public ExtendedBalanceForAnalysis()
    {
      Common = new ExtendedBalance();
      OnHands = new ExtendedBalance();
      OnDeposits = new ExtendedBalance();
    }
  }

  class Saldo
  {
    public DateTime StartDate { get; set; }
    public ExtendedBalanceForAnalysis BeginBalance { get; set; }
    public decimal BeginByrRate { get; set; }
    public decimal Incomes { get; set; }
    public decimal Expense { get; set; }
    public decimal LargeExpense { get; set; }
    public decimal ExchangeDifference { get { return EndBalance.Common.TotalInUsd - BeginBalance.Common.TotalInUsd - Incomes + Expense; } }
    public ExtendedBalanceForAnalysis EndBalance { get; set; }
    public decimal EndByrRate { get; set; }
    public DateTime LastDayWithTransactionsInMonth { get; set; }
    public decimal SaldoIncomesExpense { get { return Incomes - Expense; } }
    public decimal Result { get { return EndBalance.Common.TotalInUsd - BeginBalance.Common.TotalInUsd; } }

    public decimal ForecastIncomes { get; set; }
    public decimal ForecastExpense { get; set; }
    public decimal ForecastFinResult { get { return ForecastIncomes - ForecastExpense; } }
    public decimal ForecastEndBalance { get { return BeginBalance.Common.TotalInUsd + ForecastFinResult; } }

    public Saldo()
    {
      BeginBalance = new ExtendedBalanceForAnalysis();
      EndBalance = new ExtendedBalanceForAnalysis();
    }
  }
}
