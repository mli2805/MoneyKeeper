using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.Utils.Accounts;

namespace Keeper.ViewModels
{
  [Export]
  public class OpenOrEditDepositViewModel : Screen
  {
    public Deposit DepositInWork { get; set; }
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
      _windowTitle = windowTitle;
      DepositInWork = deposit;

      if (windowTitle == "Добавить")
      {
        DepositInWork.Bank = BankAccounts.First();
        DepositInWork.StartDate = DateTime.Today;
        DepositInWork.FinishDate = DateTime.Today.AddMonths(1);
        DepositInWork.Currency = CurrencyCodes.BYR;
        DepositInWork.DepositRate = 6;
      }

    }

    protected override void OnViewLoaded(object view)
    {
      DisplayName = _windowTitle;
    }

    public void SaveDeposit()
    {
      TryClose(true);
    }

    public void CompileAccountName()
    {
      DepositInWork.ParentAccount.Name = string.Format("{0} {1} {2:d//MM/yyyy} - {3:d/MM/yyyy} {4:0.#}%",
         DepositInWork.Bank.Name, DepositInWork.Title, DepositInWork.StartDate, DepositInWork.FinishDate, DepositInWork.DepositRate);
    }

  }
}
