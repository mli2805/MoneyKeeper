using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keeper.DomainModel
{
  class Saldo
  {
    public Decimal BeginBalance { get; set; }
    public Decimal Incomes { get; set; }
    public Decimal Expense { get; set; }
    public Decimal ExchangeDifference { get { return EndBalance - BeginBalance - Incomes - Expense; } }
    public Decimal EndBalance { get; set; }
    public Decimal SaldoIncomesExpense { get { return Incomes + Expense; } }
    public Decimal Result { get { return EndBalance - BeginBalance; } }
  }
}
