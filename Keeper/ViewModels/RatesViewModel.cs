using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.ViewModels
{
  public static class CurrencyCodesForFilter
  {
    public static List<CurrencyCodes> CurrencyList { get; private set; }

    static CurrencyCodesForFilter()
    {
      CurrencyList = Enum.GetValues(typeof(CurrencyCodes)).OfType<CurrencyCodes>().ToList();
      CurrencyList.Remove(CurrencyCodes.USD);
    }
  }

  public static class AllCurrencyCodes
  {
    public static List<CurrencyCodes> CurrencyList { get; private set; }

    static AllCurrencyCodes()
    {
      CurrencyList = Enum.GetValues(typeof(CurrencyCodes)).OfType<CurrencyCodes>().ToList();
    }
  }

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


    private bool _isFilterChecked;
    public bool IsFilterChecked
    {
      get { return _isFilterChecked; }
      set
      {
        _isFilterChecked = value;
        var view = CollectionViewSource.GetDefaultView(Rows);
        view.Refresh();
      }
    }

    private CurrencyCodes _selectedCurrencyFilter;
    public CurrencyCodes SelectedCurrencyFilter 
    {
      get { return _selectedCurrencyFilter; }
      set
      {
        _selectedCurrencyFilter = value;
        var view = CollectionViewSource.GetDefaultView(Rows);
        view.Refresh();
      }
    }

    //*****************************************************************************

    public ObservableCollection<CurrencyRate> Rows { get; set; }

    public RatesViewModel()
    {
      Db.CurrencyRates.Load();
      Rows = Db.CurrencyRates.Local;

      IsFilterChecked = false;
      SelectedCurrencyFilter = CurrencyCodes.BYR;

      var view = CollectionViewSource.GetDefaultView(Rows);

      view.Filter +=OnFilter;
    }

    private bool OnFilter(object o)
    {
      var currencyRate = (CurrencyRate) o;
      if (IsFilterChecked == false) return true;
      return currencyRate.Currency == SelectedCurrencyFilter;
    }
  }
}
