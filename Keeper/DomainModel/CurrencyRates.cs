using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keeper.DomainModel
{
  class CurrencyRates
  {
//    Все курсы валют хранятся относительно USD (дата - валюта - курс к доллару)
    public DateTime BankDay { get; set; }
    public CurrencyCodes Currency { get; set; }
    public Decimal Rate { get; set; }
  }
}
