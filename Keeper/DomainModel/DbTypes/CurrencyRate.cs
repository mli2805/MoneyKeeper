﻿using System;
using System.Globalization;
using Caliburn.Micro;
using Keeper.DomainModel.Enumes;

namespace Keeper.DomainModel.DbTypes
{
  [Serializable] // для binary formatter
  public class CurrencyRate : PropertyChangedBase
  {
    //    Все курсы валют хранятся относительно USD (дата - валюта - курс к доллару)
    public DateTime BankDay { get; set; }
    public CurrencyCodes Currency { get; set; }
    public Double Rate { get; set; }

    #region Поля для показа на форме "Курсы валют"
  
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

    #endregion

  }



  /*
   * для XML сериализатора нужно чтобы класс был Public и был конструктор без параметров
   * 
   * чтобы поле было атрибутом , а не элементом - [XmlAttribute]
   * 
   * public поля для показа на форме не надо хранить - [XmlIgnore]
   * public без set - не сериализует, public с private set - runtime error при сериализации
   */
}
