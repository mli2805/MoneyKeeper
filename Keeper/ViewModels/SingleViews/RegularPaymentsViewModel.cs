using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Caliburn.Micro;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.Extentions;
using Keeper.DomainModel.WorkTypes;
using Keeper.Utils.AccountEditing;
using Keeper.Utils.DbInputOutput;

namespace Keeper.ViewModels.SingleViews
{
  [Export]
  class RegularPaymentsViewModel : Screen
  {
    public static List<CurrencyCodes> CurrencyList { get; private set; }
    public static List<string> ArticleList { get; private set; }

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
    private readonly KeeperDb _db;
    

    [ImportingConstructor]
    public RegularPaymentsViewModel(RegularPaymentsProvider provider, KeeperDb db)
    {
      _provider = provider;
      _db = db;
      
      CurrencyList = Enum.GetValues(typeof(CurrencyCodes)).OfType<CurrencyCodes>().ToList();
      ArticleList = new List<string>();
      var accounts = _db.FlattenAccounts().Where(a => a.Is("Все доходы") || a.Is("Все расходы")).ToList();
      foreach (var account in accounts)
      {
      ArticleList.Add(account.Name);
      }
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
