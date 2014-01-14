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
  public class AccountForestViewModel : Screen
  {
    [Import]
    public IWindowManager WindowManager { get; set; }

    private KeeperDb _db;
    private readonly AccountTreesGardener _accountTreesGardener;
    private readonly IDbFromTxtLoader _dbFromTxtLoader;
    private readonly IDbToTxtSaver _dbToTxtSaver;

    private readonly List<Screen> _launchedForms = new List<Screen>();

    private Account _selectedAccountInControl;
    public Account SelectedAccountInControl
    {
      get { return _selectedAccountInControl; }
      set
      {
        _selectedAccountInControl = value;
        IsDeposit = value != null && value.IsDescendantOf("Депозиты") && value.Children.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
      }
    }

    #region Fields for binding
    public AccountForest MyForest { get; set; }

    private int _openedAccountPage;
    public int OpenedAccountPage
    {
      get { return _openedAccountPage; }
      set
      {
        _openedAccountPage = value;
        SelectedAccountInControl = MyForest.FindSelectedOrAssignFirstAccountOnPage(_openedAccountPage);
        SelectedAccountInControl.IsSelected = true;
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

    [ImportingConstructor]
    public AccountForestViewModel(KeeperDb db, AccountTreesGardener accountTreesGardener,
                                 IDbFromTxtLoader dbFromTxtLoader, IDbToTxtSaver dbToTxtSaver)
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
      if (MyForest == null)  MyForest = new AccountForest();
      MyForest.MineAccountsRoot = new ObservableCollection<Account>(from account in _db.Accounts
                                                                    where account.Name == "Мои"
                                                                    select account);
      MyForest.ExternalAccountsRoot = new ObservableCollection<Account>(from account in _db.Accounts
                                                                        where account.Name == "Внешние" || account.Name == "Для ввода стартовых остатков"
                                                                        select account);
      MyForest.IncomesRoot = new ObservableCollection<Account>(from account in _db.Accounts
                                                               where account.Name == "Все доходы"
                                                               select account);
      MyForest.ExpensesRoot = new ObservableCollection<Account>(from account in _db.Accounts
                                                                where account.Name == "Все расходы"
                                                                select account);

      NotifyOfPropertyChange(() => MyForest.MineAccountsRoot);
      NotifyOfPropertyChange(() => MyForest.ExternalAccountsRoot);
      NotifyOfPropertyChange(() => MyForest.IncomesRoot);
      NotifyOfPropertyChange(() => MyForest.ExpensesRoot);
    }

    #region // методы реализации контекстного меню на дереве счетов

    public void RemoveSelectedAccount()
    {
      _accountTreesGardener.RemoveAccount(SelectedAccountInControl);
    }

    public void AddSelectedAccount()
    {
      var newSelectedAccount = _accountTreesGardener.AddAccount(SelectedAccountInControl);
      if (SelectedAccountInControl.Name == "Депозиты") ReorderDepositAccounts();
      SelectedAccountInControl = newSelectedAccount;
      NotifyOfPropertyChange(() => MyForest.MineAccountsRoot);
    }

    private void ReorderDepositAccounts()
    {
      _dbToTxtSaver.SaveDbInTxt();
      var result = _dbFromTxtLoader.LoadDbFromTxt(Settings.Default.TemporaryTxtDbPath);
      if (result.Code != 0) MessageBox.Show(result.Explanation);
      else
      {
        _db = result.Db;
        InitVariablesToShowAccounts();
      }
    }

    public void ChangeSelectedAccount()
    {
      _accountTreesGardener.ChangeAccount(SelectedAccountInControl);
    }

    public void ShowDeposit()
    {
      if (!SelectedAccountInControl.IsDescendantOf("Депозиты") || SelectedAccountInControl.Children.Count != 0) return;

      foreach (var launchedForm in _launchedForms)
      {
        if (launchedForm is DepositViewModel && launchedForm.IsActive
          && ((DepositViewModel)launchedForm).Deposit.Account == SelectedAccountInControl) launchedForm.TryClose();
      }

      var depositForm = IoC.Get<DepositViewModel>();
      depositForm.SetAccount(SelectedAccountInControl);
      _launchedForms.Add(depositForm);
      depositForm.Renewed += DepositViewModelRenewed; // ?
      WindowManager.ShowWindow(depositForm);
    }

    void DepositViewModelRenewed(object sender, Account newAccount)
    {
      SelectedAccountInControl.IsSelected = false;
      SelectedAccountInControl = newAccount;
    }

    #endregion
  }
}
