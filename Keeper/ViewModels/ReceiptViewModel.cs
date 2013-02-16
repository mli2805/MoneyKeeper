using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Keeper.DomainModel;

namespace Keeper.ViewModels
{
  class ReceiptViewModel
  {
    public decimal TotalAmount { get; set; }
    public CurrencyCodes Currency { get; set; }

    public List<Tuple<decimal, Account, string>> Expense { get; set; }

    public decimal Amount { get; set; }
    public Account Article { get; set; }
    public string Comment { get; set; }

    public bool Result { get; set; }


  }
}
