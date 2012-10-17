using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keeper.DomainModel
{
  public class CurrencyRate
  {
//    Все курсы валют хранятся относительно USD (дата - валюта - курс к доллару)
    public int Id { get; set; }
    public DateTime BankDay { get; set; }
    public CurrencyCodes Currency { get; set; }
    public Decimal Rate { get; set; }

    public CurrencyRate()
    {
      BankDay = DateTime.Today;
      Currency = CurrencyCodes.BYR;
      Rate = 0;
    }

    public string ToDump()
    {
      return BankDay + " , " + Currency + " , " + Rate;
    }
  }
}
