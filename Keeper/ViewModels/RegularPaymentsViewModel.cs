using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Caliburn.Micro;
using Keeper.ByFunctional.AccountEditing;
using Keeper.DomainModel;

namespace Keeper.ViewModels
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
    private readonly AccountTreeStraightener _accountTreeStraightener;

    [ImportingConstructor]
    public RegularPaymentsViewModel(RegularPaymentsProvider provider, KeeperDb db, AccountTreeStraightener accountTreeStraightener)
    {
      _provider = provider;
      _db = db;
      _accountTreeStraightener = accountTreeStraightener;
      CurrencyList = Enum.GetValues(typeof(CurrencyCodes)).OfType<CurrencyCodes>().ToList();
      ArticleList = new List<string>();
      var accounts = _accountTreeStraightener.Flatten(_db.Accounts).Where(a => a.Is("Все доходы") || a.Is("Все расходы")).ToList();
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
