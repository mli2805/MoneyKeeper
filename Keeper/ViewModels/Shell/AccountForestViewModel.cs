using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.Models;
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


    #region Fields for binding
    public AccountForestModel MyForestModel { get; set; }

    #endregion

    [ImportingConstructor]
    public AccountForestViewModel(ShellModel shellModel, KeeperDb db, AccountTreesGardener accountTreesGardener,
                                 IDbFromTxtLoader dbFromTxtLoader, IDbToTxtSaver dbToTxtSaver)
    {
      MyForestModel = shellModel.MyForestModel;
      _db = db;
      _accountTreesGardener = accountTreesGardener;
      _dbFromTxtLoader = dbFromTxtLoader;
      _dbToTxtSaver = dbToTxtSaver;

      InitVariablesToShowAccounts();
    }

    protected override void OnViewLoaded(object view)
    {
      MyForestModel.OpenedAccountPage = 0;
    }

    public void InitVariablesToShowAccounts()
    {
      MyForestModel.MineAccountsRoot = new ObservableCollection<Account>(from account in _db.Accounts
                                                                    where account.Name == "Мои"
                                                                    select account);
      MyForestModel.ExternalAccountsRoot = new ObservableCollection<Account>(from account in _db.Accounts
                                                                        where account.Name == "Внешние" || account.Name == "Для ввода стартовых остатков"
                                                                        select account);
      MyForestModel.IncomesRoot = new ObservableCollection<Account>(from account in _db.Accounts
                                                               where account.Name == "Все доходы"
                                                               select account);
      MyForestModel.ExpensesRoot = new ObservableCollection<Account>(from account in _db.Accounts
                                                                where account.Name == "Все расходы"
                                                                select account);

      NotifyOfPropertyChange(() => MyForestModel.MineAccountsRoot);
      NotifyOfPropertyChange(() => MyForestModel.ExternalAccountsRoot);
      NotifyOfPropertyChange(() => MyForestModel.IncomesRoot);
      NotifyOfPropertyChange(() => MyForestModel.ExpensesRoot);
    }

    #region // методы реализации контекстного меню на дереве счетов

    public void RemoveSelectedAccount()
    {
      _accountTreesGardener.RemoveAccount(MyForestModel.SelectedAccount);
    }

    public void AddSelectedAccount()
    {
      var newSelectedAccount = _accountTreesGardener.AddAccount(MyForestModel.SelectedAccount);
      if (MyForestModel.SelectedAccount.Name == "Депозиты") ReorderDepositAccounts();
      MyForestModel.SelectedAccount = newSelectedAccount;
      NotifyOfPropertyChange(() => MyForestModel.MineAccountsRoot);
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
      _accountTreesGardener.ChangeAccount(MyForestModel.SelectedAccount);
    }

    public void ShowDeposit()
    {
      if (!MyForestModel.SelectedAccount.IsDescendantOf("Депозиты") || MyForestModel.SelectedAccount.Children.Count != 0) return;

      foreach (var launchedForm in _launchedForms)
      {
        if (launchedForm is DepositViewModel && launchedForm.IsActive
          && ((DepositViewModel)launchedForm).Deposit.Account == MyForestModel.SelectedAccount) launchedForm.TryClose();
      }

      var depositForm = IoC.Get<DepositViewModel>();
      depositForm.SetAccount(MyForestModel.SelectedAccount);
      _launchedForms.Add(depositForm);
      depositForm.Renewed += DepositViewModelRenewed; // ?
      WindowManager.ShowWindow(depositForm);
    }

    void DepositViewModelRenewed(object sender, Account newAccount)
    {
      MyForestModel.SelectedAccount.IsSelected = false;
      MyForestModel.SelectedAccount = newAccount;
    }

    #endregion
  }
}
