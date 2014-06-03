using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.ViewModels
{
  [Export]
  class RegularPaymentsViewModel : Screen
  {
    public RegularPaymentsViewModel()
    {
      CurrencyList = Enum.GetValues(typeof(CurrencyCodes)).OfType<CurrencyCodes>().ToList();
    }


    public static List<CurrencyCodes> CurrencyList { get; private set; }
    
    public ObservableCollection<RegularPayment> Income { get; set; }
    public ObservableCollection<RegularPayment> Expenses { get; set; }

    protected override void OnViewLoaded(object view)
    {
      DisplayName = "Регулярные платежи";

      Income = ReadRegularIncome();
      Expenses = ReadRegularExpenses();
    }

    private ObservableCollection<RegularPayment> ReadRegularIncome()
    {
      return new ObservableCollection<RegularPayment>();
    }

    private ObservableCollection<RegularPayment> ReadRegularExpenses()
    {
      return new ObservableCollection<RegularPayment>();
    }

    private void SaveRegularIncome()
    {
      
    }

    private void SaveRegularExpenses()
    {

    }
  }
}
