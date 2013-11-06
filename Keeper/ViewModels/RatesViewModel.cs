using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.ViewModels
{
  public class RatesViewModel : Screen
  {
    public KeeperDb Db { get { return IoC.Get<KeeperDb>(); } }


    /*****************************************************************************
     * метод GetDefaultView получает ObservableCollection и делает из нее CollectionViewSource
     * 
     * Если в памяти уже есть такой CollectionViewSource, то новый не создается , а возвращается 
     * указатель на существующий, таким образом, можно сколько угодно раз вызывать GetDefaultView
     * получать один и тот же CollectionViewSource и производить над ним действия: присваивать фильтры
     * сортировки  и т.п.
     * 
     * В xaml когда подвязываем DataGrid к нашей ObservableCollection неявно вызывается тот же
     * GetDefaultView и значит DataGrid показывает тот же CollectionViewSource, для которого 
     * мы устанавливаем фильтры , сортировки и прочее.
     * 
     * У CollectionViewSource свойство Filter получает делегата
     * в данном случае это должен быть метод получающий object и возвращающий bool - 
     * надо показывать этот объект или нет.
     * Реализуем такой метод и подвязываем его к свойству Filter нашего CollectionViewSource
     * 
     * Сколько бы разных реализаций метода сортировки не подвязывалось к одному CollectionViewSource
     * в различных частях программы - действовать будет последний
    */


    private CurrencyRatesFilter _selectedFilter;
    private string _expanderHeader;
    private bool _isInInputMode;
    private double _lastByrRate;
    private DateTime _newDate;
    private double _lastEurRate;

    public CurrencyRatesFilter SelectedFilter
    {
      get { return _selectedFilter; }
      set
      {
        _selectedFilter = value;
        var view = CollectionViewSource.GetDefaultView(Rows);
        view.Refresh();
      }
    }

    //*****************************************************************************

    public ObservableCollection<CurrencyRate> Rows { get; set; }

    public DateTime NewDate
    {
      get { return _newDate; }
      set
      {
        if (value.Equals(_newDate)) return;
        _newDate = value;
        NotifyOfPropertyChange(() => NewDate);
      }
    }

    public double LastByrRate 
    {
      get { return _lastByrRate; }
      set
      {
        if (value.Equals(_lastByrRate)) return;
        _lastByrRate = value;
        NotifyOfPropertyChange(() => LastByrRate);
      }
    }

    public double LastEurRate
    {
      get { return _lastEurRate; }
      set
      {
        if (value.Equals(_lastEurRate)) return;
        _lastEurRate = value;
        NotifyOfPropertyChange(() => LastEurRate);
      }
    }

    public bool IsInInputMode
    {
      get { return _isInInputMode; }
      set
      {
        if (value.Equals(_isInInputMode)) return;
        _isInInputMode = value;
        ExpanderHeader = _isInInputMode ? "" : "Удобный способ ввода курсов валют";
        if (_isInInputMode) FillInFields();
        NotifyOfPropertyChange(() => IsInInputMode);
      }
    }

    public void FillInFields()
    {
      var lastCurrencyRate = (from cr in Rows
                              where cr.Currency == CurrencyCodes.BYR
                              orderby cr.BankDay
                              select cr).Last();
      NewDate = lastCurrencyRate.BankDay.Date.AddDays(1);
      LastByrRate = lastCurrencyRate.Rate;

      lastCurrencyRate = (from cr in Rows
                          where cr.Currency == CurrencyCodes.EUR
                          orderby cr.BankDay
                          select cr).Last();
      if (NewDate <= lastCurrencyRate.BankDay.Date) NewDate = lastCurrencyRate.BankDay.Date.AddDays(1); 
      LastEurRate = Math.Round(1/lastCurrencyRate.Rate,3);
    }

    public string ExpanderHeader
    {
      get { return _expanderHeader; }
      set
      {
        if (value == _expanderHeader) return;
        _expanderHeader = value;
        NotifyOfPropertyChange(() => ExpanderHeader);
      }
    }

    public RatesViewModel()
    {
      Rows = Db.CurrencyRates;
      SelectedFilter = CurrencyRatesFilterListForCombo.FilterList.First(f => !f.IsOn);

      var view = CollectionViewSource.GetDefaultView(Rows);

      view.Filter += OnFilter;
      view.SortDescriptions.Add(new SortDescription("BankDay", ListSortDirection.Ascending));

      IsInInputMode = false;
    }

    protected override void OnViewLoaded(object view)
    {
      DisplayName = "Курсы валют";
    }

    private bool OnFilter(object o)
    {
      var currencyRate = (CurrencyRate)o;
      if (SelectedFilter.IsOn == false) return true;
      return currencyRate.Currency == SelectedFilter.Currency;
    }

    public void SaveNewRates()
    {
      var newByrCurrencyRate = new CurrencyRate {BankDay = NewDate, Currency = CurrencyCodes.BYR, Rate = LastByrRate};
      Rows.Add(newByrCurrencyRate);

      var newEurCurrencyRate = new CurrencyRate
                                 {BankDay = NewDate, Currency = CurrencyCodes.EUR, Rate = Math.Round(1/LastEurRate, 4)};
      Rows.Add(newEurCurrencyRate);

      IsInInputMode = false;
    }

    public void CancelNewRates()
    {
      IsInInputMode = false;
    }

    public void CloseView()
    {
      TryClose();
    }
  }
}
