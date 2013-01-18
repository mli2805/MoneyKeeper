using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using Caliburn.Micro;

namespace Keeper.DomainModel
{
  public class CurrencyRate : PropertyChangedBase
  {
    //    Все курсы валют хранятся относительно USD (дата - валюта - курс к доллару)
    public int Id { get; set; }
    public DateTime BankDay { get; set; }
    public CurrencyCodes Currency { get; set; }
    public Double Rate { get; set; }

    public string ToDump()
    {
      return BankDay + " ; " + Currency + " ; " + Math.Round(Rate, 4);
    }


    #region Поля для показа на форме "Курсы валют"

    [NotMapped]
    public string RateOnScreen
    {
      get
      {
        if (Currency == CurrencyCodes.EUR) return Math.Round(1 / Rate, 3).ToString(CultureInfo.CurrentCulture);
        if (Currency == CurrencyCodes.BYR) return Math.Round(Rate, 0).ToString(CultureInfo.CurrentCulture);
        return Rate.ToString(CultureInfo.CurrentCulture);
      }
      set
      {
        double rate;
        if (!Double.TryParse(value, out rate)) return;
        Rate = Currency == CurrencyCodes.EUR ? 1/rate : rate;
      }
    }

    private CurrencyCodes _currencyLeft;
    [NotMapped]
    public CurrencyCodes CurrencyLeft
    {
      get
      {
        return Currency == CurrencyCodes.EUR ? CurrencyCodes.USD : Currency;
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
        return Currency == CurrencyCodes.EUR ? CurrencyCodes.EUR : CurrencyCodes.USD;
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
