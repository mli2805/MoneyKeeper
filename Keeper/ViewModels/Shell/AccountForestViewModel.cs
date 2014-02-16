using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Keeper.ByFunctional.EditingAccounts;
using Keeper.DomainModel;
using Keeper.Models.Shell;
using Keeper.Properties;
using Keeper.Utils;
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
    private readonly AccountOperations _accountOperations;
    private readonly IDbFromTxtLoader _dbFromTxtLoader;
    private readonly IDbToTxtSaver _dbToTxtSaver;
    private readonly DepositParser _depositParser;

    private readonly List<Screen> _launchedForms = new List<Screen>();

    public AccountForestModel MyForestModel { get; set; }

    [ImportingConstructor]
    public AccountForestViewModel(ShellModel shellModel, KeeperDb db, AccountTreesGardener accountTreesGardener,
      AccountOperations accountOperations,
                                 IDbFromTxtLoader dbFromTxtLoader, IDbToTxtSaver dbToTxtSaver, DepositParser depositParser)
    {
      MyForestModel = shellModel.MyForestModel;
      _db = db;
      _accountTreesGardener = accountTreesGardener;
      _accountOperations = accountOperations;
      _dbFromTxtLoader = dbFromTxtLoader;
      _dbToTxtSaver = dbToTxtSaver;
      _depositParser = depositParser;

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
      var newSelectedAccount = MyForestModel.SelectedAccount.Name == "Депозиты" ? 
        _accountTreesGardener.AddDeposit(MyForestModel.SelectedAccount) :
        _accountTreesGardener.AddAccount(MyForestModel.SelectedAccount) ;
      if (newSelectedAccount == null) return;
      if (MyForestModel.SelectedAccount.Name == "Депозиты")
        _accountOperations.SortDepositAccounts(MyForestModel.SelectedAccount);
      MyForestModel.SelectedAccount.IsSelected = false;
      MyForestModel.SelectedAccount = newSelectedAccount;
      MyForestModel.SelectedAccount.IsSelected = true;
      NotifyOfPropertyChange(() => MyForestModel.MineAccountsRoot);
    }

    public void ChangeSelectedAccount()
    {
      if (MyForestModel.SelectedAccount.Is("Депозиты"))
        _accountTreesGardener.ChangeDeposit(MyForestModel.SelectedAccount);
      else
        _accountTreesGardener.ChangeAccount(MyForestModel.SelectedAccount);
    }

    public void ShowDeposit()
    {
      if (!MyForestModel.SelectedAccount.Is("Депозиты") || MyForestModel.SelectedAccount.Children.Count != 0) return;

      foreach (var launchedForm in _launchedForms)
      {
        if (launchedForm is DepositViewModel && launchedForm.IsActive
          && ((DepositViewModel)launchedForm).Deposit.ParentAccount == MyForestModel.SelectedAccount) launchedForm.TryClose();
      }

      var depositForm = IoC.Get<DepositViewModel>();
      depositForm.SetAccount(_depositParser.Analyze(MyForestModel.SelectedAccount));
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
