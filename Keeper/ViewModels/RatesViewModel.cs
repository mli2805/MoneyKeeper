using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.ViewModels
{
  public static class AllCurrencyCodes
  {
    public static string ForOne = " за 1 ";
    public static List<CurrencyCodes> CurrencyList { get; private set; }

    static AllCurrencyCodes()
    {
      CurrencyList = Enum.GetValues(typeof(CurrencyCodes)).OfType<CurrencyCodes>().ToList();
      //      CurrencyList.Remove(CurrencyCodes.USD);
    }
  }

  public class RateForView
  {
    public DateTime BankDay { get; set; }
    public CurrencyCodes CurrencyLeft { get; set; }
    public Decimal Rate { get; set; }
    public Decimal RateOnScreen { get; set; }
    public CurrencyCodes CurrencyRight { get; set; }
    public string ForOne { get; set; }

    public RateForView()
    {
      BankDay = DateTime.Now;
      CurrencyLeft = CurrencyCodes.USD;
      CurrencyRight = CurrencyCodes.USD;
      Rate = 1;
      RateOnScreen = 1;
      ForOne = AllCurrencyCodes.ForOne;
    }
  }

  public class RatesViewModel : Screen
  {
    public KeeperDb Db { get { return IoC.Get<KeeperDb>(); } }

    public ObservableCollection<CurrencyRate> Rows { get; set; }

    public List<RateForView> Rates { get; set; }

    public RatesViewModel()
    {
      Db.CurrencyRates.Load();
      Rows = Db.CurrencyRates.Local;

      BuildRates();
    }


    public override void CanClose(Action<bool> callback)
    {
      BuildRowsFromRates();
      callback(true);
    }

    // TODO прибиндиться не к Rows, а к Rates и по окончании работы формы Rates запихивать в базу

    #region // преобразование из БД к экранному виду и обратно

    private void BuildRates()
    {
      if (Rates == null) Rates = new List<RateForView>();
      else Rates.Clear();

      foreach (var row in Rows)
      {
        var rate = new RateForView();
        rate.BankDay = row.BankDay;
        rate.Rate = row.Rate;
        if (row.Rate > 1)
        {
          rate.CurrencyLeft = row.Currency;
          rate.RateOnScreen = row.Rate;
          rate.CurrencyRight = CurrencyCodes.USD;
        }
        else
        {
          rate.CurrencyLeft = CurrencyCodes.USD;
          rate.RateOnScreen = row.Rate != 0 ? 1 / row.Rate : 0;
          rate.CurrencyRight = row.Currency;
        }
        Rates.Add(rate);
      }
    }

    private void BuildRowsFromRates()
    {
      Rows.Clear();
      foreach (var rate in Rates)
      {
        var row = new CurrencyRate();
        row.BankDay = rate.BankDay;
        if (rate.CurrencyRight == CurrencyCodes.USD)
        {
          row.Rate = rate.RateOnScreen;
          row.Currency = rate.CurrencyLeft;
        }
        else
        {
          const decimal delta = 0.001M;
          if ( Math.Abs(rate.Rate - 1/rate.RateOnScreen) > delta) row.Rate = 1/rate.RateOnScreen;
          else
          {
            row.Rate = rate.Rate;
          }
          row.Currency = rate.CurrencyRight;
          
        }

        Rows.Add(row);
      }
    }

    #endregion

  }
}
