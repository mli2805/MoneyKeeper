using System;

namespace Keeper.DomainModel
{
  class Saldo
  {
    public decimal BeginBalance { get; set; }
    public decimal BeginByrRate { get; set; }
    public decimal Incomes { get; set; }
    public decimal Expense { get; set; }
    public decimal ExchangeDifference { get { return EndBalance - BeginBalance - Incomes - Expense; } }
    public decimal EndBalance { get; set; }
    public decimal LastByrRate { get; set; }
    public DateTime LastDayWithTransactionsInMonth { get; set; }
    public decimal SaldoIncomesExpense { get { return Incomes + Expense; } }
    public decimal Result { get { return EndBalance - BeginBalance; } }
  }
}
