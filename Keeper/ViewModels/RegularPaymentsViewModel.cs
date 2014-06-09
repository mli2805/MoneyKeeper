﻿using System;
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

    private RegularPayments _payments;

    public RegularPayments Payments
    {
      get { return _payments; }
      set
      {
        if (Equals(value, _payments)) return;
        _payments = value;
        NotifyOfPropertyChange(() => Payments);
      }
    }

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
