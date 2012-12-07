using System.Collections.ObjectModel;
using System.Data.Entity;
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

    public RatesViewModel()
    {
      Db.CurrencyRates.Load();
      Rows = Db.CurrencyRates.Local;
      SelectedFilter = AllCurrencyRatesFilters.FilterList.First(f => !f.IsOn);

      var view = CollectionViewSource.GetDefaultView(Rows);

      view.Filter +=OnFilter;
    }

    protected override void OnViewLoaded(object view)
    {
      DisplayName = "Курсы валют";
    }

    private bool OnFilter(object o)
    {
      var currencyRate = (CurrencyRate) o;
      if (SelectedFilter.IsOn == false) return true;
      return currencyRate.Currency == SelectedFilter.Currency;
    }
  }
}
