using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.Utils.Accounts;

namespace Keeper.ViewModels
{
  [Export]
  public class OpenOrEditDepositViewModel : Screen
  {
    private Deposit Deposit { get; set; }
    private string _windowTitle;
    private readonly KeeperDb _db;
    private readonly AccountTreeStraightener _accountTreeStraightener;

    #region Списки для комбобоксов
    private List<CurrencyCodes> _currencyList;
    private List<Account> _bankAccounts;
    
    public List<CurrencyCodes> CurrencyList
    {
      get { return _currencyList; }
      private set
      {
        if (Equals(value, _currencyList)) return;
        _currencyList = value;
        NotifyOfPropertyChange(() => CurrencyList);
      }
    }

    public List<Account> BankAccounts
    {
      get { return _bankAccounts; }
      set
      {
        if (Equals(value, _bankAccounts)) return;
        _bankAccounts = value;
        NotifyOfPropertyChange(() => BankAccounts);
      }
    }

    private void InitializeListsForCombobox()
    {
      CurrencyList = Enum.GetValues(typeof(CurrencyCodes)).OfType<CurrencyCodes>().ToList();
      BankAccounts =_accountTreeStraightener.Flatten(_db.Accounts).Where(a => a.Is("Банки") && a.Children.Count == 0).ToList();
    }

    #endregion

    [ImportingConstructor]
    public OpenOrEditDepositViewModel(KeeperDb db, AccountTreeStraightener accountTreeStraightener)
    {
      _db = db;
      _accountTreeStraightener = accountTreeStraightener;
      InitializeListsForCombobox();
    }

    public void InitializeForm(Deposit deposit, string windowTitle)
    {
      Deposit = deposit;
      _windowTitle = windowTitle;
    }

    protected override void OnViewLoaded(object view)
    {
      DisplayName = _windowTitle;
    }


  }
}
