using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Caliburn.Micro;
using Keeper.DomainModel;

namespace Keeper.ViewModels
{
  [Export]
  class RegularPaymentsViewModel : Screen
  {
    public static List<CurrencyCodes> CurrencyList { get; private set; }

    public RegularPayments Payments { get; set; }
    private readonly RegularPaymentsProvider _provider;

    [ImportingConstructor]
    public RegularPaymentsViewModel(RegularPaymentsProvider provider)
    {
      _provider = provider;
      CurrencyList = Enum.GetValues(typeof(CurrencyCodes)).OfType<CurrencyCodes>().ToList();
    }

    protected override void OnViewLoaded(object view)
    {
      DisplayName = "Регулярные платежи";

      Payments = (RegularPayments)_provider.RegularPayments.Clone();
      Payments.Income.Add(new RegularPayment(){Amount = 1000, Comment = "1", Currency = CurrencyCodes.USD, DayOfMonth = 3});
    }

    public void SavePayments()
    {
      _provider.RegularPayments = Payments;
      _provider.Write();

      TryClose();
    }

    public void CancelChanges()
    {
      TryClose();
    }

  }
}
