using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.Properties;
using Keeper.Utils.Accounts;
using Keeper.Utils.DbInputOutput.TxtTasks;

namespace Keeper.ViewModels.Shell
{
  [Export]
  class AccountTreesViewModel : Screen
  {
    [Import]
    public IWindowManager WindowManager { get; set; }

    private KeeperDb _db;
    private readonly AccountTreesGardener _accountTreesGardener;
    private readonly DbFromTxtLoader _dbFromTxtLoader;
    private readonly DbToTxtSaver _dbToTxtSaver;

    private readonly List<Screen> _launchedForms = new List<Screen>();


    #region Field for binding
    public ObservableCollection<Account> MineAccountsRoot { get; private set; }
    public ObservableCollection<Account> ExternalAccountsRoot { get; private set; }
    public ObservableCollection<Account> IncomesRoot { get; private set; }
    public ObservableCollection<Account> ExpensesRoot { get; private set; }

    private Account _selectedAccount;
    public Account SelectedAccount
    {
      get { return _selectedAccount; }
      set
      {
        _selectedAccount = value;
        IsDeposit = value != null && value.IsDescendantOf("Депозиты") && value.Children.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
      }
    }

    private int _openedAccountPage;
    public int OpenedAccountPage
    {
      get { return _openedAccountPage; }
      set
      {
        _openedAccountPage = value;
        var a = FindSelectedOrAssignFirstAccountOnPage(_openedAccountPage);
        SelectedAccount = a;
      }
    }

    private Visibility _isDeposit;
    public Visibility IsDeposit
    {
      get { return _isDeposit; }
      set
      {
        if (value.Equals(_isDeposit)) return;
        _isDeposit = value;
        NotifyOfPropertyChange(() => IsDeposit);
      }
    }
    #endregion

    #region functions for AccountTree only
    private Account GetSelectedInBranch(Account account)
    {
      if (account.IsSelected) return account;
      foreach (var child in account.Children)
      {
        var result = GetSelectedInBranch(child);
        if (result != null) return result;
      }
      return null;
    }

    private Account GetSelectedInCollection(IEnumerable<Account> roots)
    {
      foreach (var branch in roots)
      {
        var result = GetSelectedInBranch(branch);
        if (result != null) return result;
      }
      return null;
    }

    private Account FindSelectedOrAssignFirstAccountOnPage(int pageNumber)
    {
      ObservableCollection<Account> collection;
      switch (pageNumber)
      {
        case 0:
          collection = MineAccountsRoot; break;
        case 1:
          collection = ExternalAccountsRoot; break;
        case 2:
          collection = IncomesRoot; break;
        case 3:
          collection = ExpensesRoot; break;
        default:
          collection = MineAccountsRoot; break;
      }

      var result = GetSelectedInCollection(collection);

      if (result == null && collection.Count != 0)
      {
        result = (from account in collection
                  select account).First();
        result.IsSelected = true;
      }
      return result;
    }
    #endregion

    [ImportingConstructor]
    public AccountTreesViewModel(KeeperDb db, AccountTreesGardener accountTreesGardener, 
                                 DbFromTxtLoader dbFromTxtLoader, DbToTxtSaver dbToTxtSaver)
    {
      _db = db;
      _accountTreesGardener = accountTreesGardener;
      _dbFromTxtLoader = dbFromTxtLoader;
      _dbToTxtSaver = dbToTxtSaver;

      InitVariablesToShowAccounts();
    }

    protected override void OnViewLoaded(object view)
    {
      OpenedAccountPage = 0;
    }

    public void InitVariablesToShowAccounts()
    {
      MineAccountsRoot = new ObservableCollection<Account>(from account in _db.Accounts
                                                           where account.Name == "Мои"
                                                           select account);
      ExternalAccountsRoot = new ObservableCollection<Account>(from account in _db.Accounts
                                                               where account.Name == "Внешние" || account.Name == "Для ввода стартовых остатков"
                                                               select account);
      IncomesRoot = new ObservableCollection<Account>(from account in _db.Accounts
                                                      where account.Name == "Все доходы"
                                                      select account);
      ExpensesRoot = new ObservableCollection<Account>(from account in _db.Accounts
                                                       where account.Name == "Все расходы"
                                                       select account);

      NotifyOfPropertyChange(() => MineAccountsRoot);
      NotifyOfPropertyChange(() => ExternalAccountsRoot);
      NotifyOfPropertyChange(() => IncomesRoot);
      NotifyOfPropertyChange(() => ExpensesRoot);
    }



    #region // методы реализации контекстного меню на дереве счетов

    public void RemoveSelectedAccount()
    {
      _accountTreesGardener.RemoveAccount(SelectedAccount);
    }

    public void AddSelectedAccount()
    {
      var newSelectedAccount = _accountTreesGardener.AddAccount(SelectedAccount);
      if (SelectedAccount.Name == "Депозиты") ReorderDepositAccounts();
      SelectedAccount = newSelectedAccount;
      NotifyOfPropertyChange(() => MineAccountsRoot);


    }

    private void ReorderDepositAccounts()
    {
      _dbToTxtSaver.SaveDbInTxt();
      var result = _dbFromTxtLoader.LoadDbFromTxt(Settings.Default.TemporaryTxtDbPath);
      if (result.Code != 0) MessageBox.Show(result.Explanation);
      else
      {
        _db = result.Db;
        //        InitVariablesToShowAccounts();
      }
    }

    public void ChangeSelectedAccount()
    {
      _accountTreesGardener.ChangeAccount(SelectedAccount);
    }

    public void ShowDeposit()
    {
      if (!SelectedAccount.IsDescendantOf("Депозиты") || SelectedAccount.Children.Count != 0) return;

      foreach (var launchedForm in _launchedForms)
      {
        if (launchedForm is DepositViewModel && launchedForm.IsActive
          && ((DepositViewModel)launchedForm).Deposit.Account == SelectedAccount) launchedForm.TryClose();
      }

      var depositForm = IoC.Get<DepositViewModel>();
      depositForm.SetAccount(SelectedAccount);
      _launchedForms.Add(depositForm);
      depositForm.Renewed += DepositViewModelRenewed; // ?
      WindowManager.ShowWindow(depositForm);
    }

    void DepositViewModelRenewed(object sender, Account newAccount)
    {
      SelectedAccount.IsSelected = false;
      SelectedAccount = newAccount;
    }


    #endregion


  }
}
