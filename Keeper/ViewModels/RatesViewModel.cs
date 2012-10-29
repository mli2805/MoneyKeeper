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
    public static List<CurrencyCodes> CurrencyList { get; private set; }

    static AllCurrencyCodes()
    {
      CurrencyList = Enum.GetValues(typeof(CurrencyCodes)).OfType<CurrencyCodes>().ToList();
    }
  }

  public class RatesViewModel : Screen
  {
    public KeeperDb Db { get { return IoC.Get<KeeperDb>(); } }

    public ObservableCollection<CurrencyRate> Rows { get; set; }

    public RatesViewModel()
    {
      Db.CurrencyRates.Load();
      Rows = Db.CurrencyRates.Local;
    }

  }
}
