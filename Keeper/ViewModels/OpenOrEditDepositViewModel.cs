using System;
using System.Collections.Generic;
using System.Composition;
using System.Globalization;
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
    private readonly IWindowManager _windowManager;
    private readonly AccountTreeStraightener _accountTreeStraightener;

    public string Junction
    {
      get { return DepositInWork.ParentAccount.Name; }
      set
      {
        if (value == DepositInWork.ParentAccount.Name) return;
        DepositInWork.ParentAccount.Name = value;
        NotifyOfPropertyChange(() => Junction);
      }
    }

    #region Списки для комбобоксов
    private List<CurrencyCodes> _currencyList;
    private List<Account> _bankAccounts;
    private List<Account> _myAccounts;

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

    public List<Account> MyAccounts
    {
      get { return _myAccounts; }
      set
      {
        if (Equals(value, _myAccounts)) return;
        _myAccounts = value;
        NotifyOfPropertyChange(() => MyAccounts);
      }
    }

    private void InitializeListsForCombobox()
    {
      CurrencyList = Enum.GetValues(typeof(CurrencyCodes)).OfType<CurrencyCodes>().ToList();
      BankAccounts =_accountTreeStraightener.Flatten(_db.Accounts).Where(a => a.Is("Банки") && a.Children.Count == 0).ToList();
      MyAccounts = _accountTreeStraightener.Flatten(_db.Accounts).Where(a => a.Is("Мои") && a.Children.Count != 0).ToList();
    }

    #endregion

    [ImportingConstructor]
    public OpenOrEditDepositViewModel(KeeperDb db, IWindowManager windowManager, AccountTreeStraightener accountTreeStraightener)
    {
      _db = db;
      _windowManager = windowManager;
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

    public void MoveToClosed()
    {
      
    }

    public void CompileAccountName()
    {
      Junction = string.Format("{0} {1} {2} - {3} {4:0.#}%",
         DepositInWork.Bank.Name, DepositInWork.Title, 
         DepositInWork.StartDate.ToString("d/MM/yyyy",CultureInfo.InvariantCulture),
         DepositInWork.FinishDate.ToString("d/MM/yyyy", CultureInfo.InvariantCulture), 
         DepositInWork.DepositRate);
    }

    public void FillDepositRatesTable()
    {
      var depositRatesViewModel = IoC.Get<DepositRatesViewModel>();
      depositRatesViewModel.Initialize(DepositInWork);
      _windowManager.ShowDialog(depositRatesViewModel);
    }
  }
}
