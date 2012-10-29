using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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
    public Double Rate { get; set; }

    public CurrencyRate()
    {
      BankDay = DateTime.Today;
      Currency = CurrencyCodes.BYR;
      Rate = 0;
    }

    public string ToDump()
    {
      return BankDay + " , " + Currency + " , " + Math.Round(Rate,4);
    }


    #region Поля для показа на форме "Курсы валют"

    [NotMapped]
    public double RateOnScreen
    {
      get
      {
        if (Currency == CurrencyCodes.EUR) return Math.Round(1 / Rate, 3);
         else return Math.Round(Rate,0);
      }
      set { if (Currency == CurrencyCodes.EUR) Rate = 1 / value; else Rate = value; }
    }

    private CurrencyCodes _currencyLeft;
    [NotMapped]
    public CurrencyCodes CurrencyLeft
    {
      get
      {
        if (Currency == CurrencyCodes.EUR) return CurrencyCodes.USD;
        else
          return Currency;
      }
      set
      {
        if (value == CurrencyCodes.USD)
        {
          if (_currencyRight != CurrencyCodes.EUR)
          {
            _currencyRight = _currencyLeft;
            _currencyLeft = value;
          }
        }
        else
        {
          Currency = value;
          _currencyLeft = value;
          _currencyRight = CurrencyCodes.USD;
        }
      }
    }

    private CurrencyCodes _currencyRight;
    [NotMapped]
    public CurrencyCodes CurrencyRight
    {
      get
      {
        if (Currency == CurrencyCodes.EUR) return CurrencyCodes.EUR;
        else return CurrencyCodes.USD;
      }
      set
      {
        if (value == CurrencyCodes.USD)
        {
          if (_currencyRight == CurrencyCodes.EUR)
          {
            _currencyLeft = CurrencyCodes.EUR;
            _currencyRight = CurrencyCodes.USD;
          }
        }
        else
        {
          _currencyLeft = CurrencyCodes.USD;
          _currencyRight = value;
          Currency = value;
        }

      }
    }

    [NotMapped]
    public string ForOne { get { return "за 1"; } }
    #endregion
  }
}
