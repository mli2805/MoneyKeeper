using System;
using System.Globalization;
using Keeper.DomainModel;

namespace Keeper.Models
{
  class CurrencyRateModel
  {	  
    #region Поля для показа на форме "Курсы валют"

    public CurrencyRate MyCurrencyRate { get; set; }

    public string RateOnScreen
    {
      get
      {
        if (MyCurrencyRate.Currency == CurrencyCodes.EUR) return Math.Round(1 / MyCurrencyRate.Rate, 3).ToString(CultureInfo.CurrentCulture);
        if (MyCurrencyRate.Currency == CurrencyCodes.BYR) return Math.Round(MyCurrencyRate.Rate, 0).ToString(CultureInfo.CurrentCulture);
        return MyCurrencyRate.Rate.ToString(CultureInfo.CurrentCulture);
      }
      set
      {
        double rate;
        if (!Double.TryParse(value, out rate)) return;
        MyCurrencyRate.Rate = MyCurrencyRate.Currency == CurrencyCodes.EUR ? 1/rate : rate;
      }
    }

    private CurrencyCodes _currencyLeft;
    public CurrencyCodes CurrencyLeft
    {
      get
      {
        return MyCurrencyRate.Currency == CurrencyCodes.EUR ? CurrencyCodes.USD : MyCurrencyRate.Currency;
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
          MyCurrencyRate.Currency = value;
          _currencyLeft = value;
          _currencyRight = CurrencyCodes.USD;
        }
      }
    }

    private CurrencyCodes _currencyRight;
    public CurrencyCodes CurrencyRight
    {
      get
      {
        return MyCurrencyRate.Currency == CurrencyCodes.EUR ? CurrencyCodes.EUR : CurrencyCodes.USD;
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
          MyCurrencyRate.Currency = value;
        }

      }
    }

//    public string ForOne { get { return "за 1"; } }
    #endregion

  }
}
