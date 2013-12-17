using System;
using System.Collections.Generic;
using Keeper.Utils;

namespace Keeper.DomainModel
{
  class Saldo
  {
    public DateTime StartDate { get; set; }
    public List<MoneyPair> BeginBalanceInCurrencies { get; set; }
    public decimal BeginBalance { get; set; }
    public decimal BeginByrRate { get; set; }
    public decimal Incomes { get; set; }
    public decimal Expense { get; set; }
    public decimal LargeExpense { get; set; }
    public decimal ExchangeDifference { get { return EndBalance - BeginBalance - Incomes + Expense; } }
    public List<MoneyPair> EndBalanceInCurrencies { get; set; }
    public decimal EndBalance { get; set; }
    public decimal EndByrRate { get; set; }
    public DateTime LastDayWithTransactionsInMonth { get; set; }
    public decimal SaldoIncomesExpense { get { return Incomes - Expense; } }
    public decimal Result { get { return EndBalance - BeginBalance; } }

    public decimal ForecastIncomes { get; set; }
    public decimal ForecastExpense { get; set; }
    public decimal ForecastFinResult { get { return ForecastIncomes - ForecastExpense; } }
    public decimal ForecastEndBalance { get { return BeginBalance + ForecastFinResult; } }
  }
}
