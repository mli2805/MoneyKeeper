using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using Caliburn.Micro;
using Keeper.ByFunctional.EditingAccounts;
using Keeper.DomainModel;
using Keeper.Models.Shell;
using Keeper.Utils.Deposits;

namespace Keeper.ViewModels.Shell
{
    [Export]
    public class AccountForestViewModel : Screen
    {
        [Import]
        public IWindowManager WindowManager { get; set; }

        private readonly KeeperDb _db;
        private readonly AccountTreesGardener _accountTreesGardener;
        private readonly AccountLowLevelOperator _accountLowLevelOperator;
        private readonly AccountTreeStraightener _accountTreeStraightener;
        private readonly DepositCalculationAggregator _depositCalculationAggregator;

        private readonly List<Screen> _launchedForms = new List<Screen>();

        public AccountForestModel MyForestModel { get; set; }

        [ImportingConstructor]
        public AccountForestViewModel(ShellModel shellModel, KeeperDb db, AccountTreesGardener accountTreesGardener,
          AccountLowLevelOperator accountLowLevelOperator, AccountTreeStraightener accountTreeStraightener, DepositCalculationAggregator depositCalculationAggregator)
        {
            MyForestModel = shellModel.MyForestModel;
            _db = db;
            _accountTreesGardener = accountTreesGardener;
            _accountLowLevelOperator = accountLowLevelOperator;
            _accountTreeStraightener = accountTreeStraightener;
            _depositCalculationAggregator = depositCalculationAggregator;

            InitVariablesToShowAccounts();
        }

        protected override void OnViewLoaded(object view)
        {
            MyForestModel.OpenedAccountPage = 0;
        }

        public void InitVariablesToShowAccounts()
        {
            MyForestModel.MineAccountsRoot = new ObservableCollection<Account>
                (from account in _db.Accounts where account.Name == "Мои" select account);
            MyForestModel.ExternalAccountsRoot = new ObservableCollection<Account>
                (from account in _db.Accounts where account.Name == "Внешние" || account.Name == "Для ввода стартовых остатков" select account);
            MyForestModel.IncomesRoot = new ObservableCollection<Account>
                (from account in _db.Accounts where account.Name == "Все доходы" select account);
            MyForestModel.ExpensesRoot = new ObservableCollection<Account>
              (from account in _db.Accounts where account.Name == "Все расходы" select account);

            NotifyOfPropertyChange(() => MyForestModel.MineAccountsRoot);
            NotifyOfPropertyChange(() => MyForestModel.ExternalAccountsRoot);
            NotifyOfPropertyChange(() => MyForestModel.IncomesRoot);
            NotifyOfPropertyChange(() => MyForestModel.ExpensesRoot);
        }

        public void AccountDebugInfoIntoConsole()
        {
            Console.WriteLine("Account name: {0} ;  id = {1} ", MyForestModel.SelectedAccount.Name, MyForestModel.SelectedAccount.Id);
            Console.WriteLine("Parent name: {0}", MyForestModel.SelectedAccount.Parent.Name);

            if (MyForestModel.SelectedAccount.Children.Count > 0)
            {
                Console.WriteLine("Children names:");

                foreach (var child in MyForestModel.SelectedAccount.Children)
                {
                    Console.WriteLine("      " + child.Name);

                }
            }
        }

        #region // методы реализации контекстного меню на дереве счетов

        public void RemoveSelectedAccount()
        {
            _accountTreesGardener.RemoveAccount(MyForestModel.SelectedAccount);
        }

        public void AddSelectedAccount()
        {
            var newSelectedAccount = _accountTreesGardener.AddAccount(MyForestModel.SelectedAccount);
            if (newSelectedAccount == null) return;

            MyForestModel.SelectedAccount.IsSelected = false;
            MyForestModel.SelectedAccount = newSelectedAccount;
            MyForestModel.SelectedAccount.IsSelected = true;
            NotifyOfPropertyChange(() => MyForestModel.MineAccountsRoot);
        }

        public void AddSelectedDeposit()
        {
            var newSelectedAccount = _accountTreesGardener.AddDeposit(MyForestModel.SelectedAccount);
            if (newSelectedAccount == null) return;
            //      _accountLowLevelOperator.SortDepositAccounts(MyForestModel.SelectedAccount);
            _accountLowLevelOperator.SortDepositAccounts(_accountTreeStraightener.Seek("Депозиты", _db.Accounts));
            MyForestModel.SelectedAccount.IsSelected = false;
            MyForestModel.SelectedAccount = newSelectedAccount;
            MyForestModel.SelectedAccount.IsSelected = true;
            NotifyOfPropertyChange(() => MyForestModel.MineAccountsRoot);
        }

        public void ChangeSelectedAccount()
        {
            if (MyForestModel.SelectedAccount.Deposit != null)
            {
                _accountTreesGardener.ChangeDeposit(MyForestModel.SelectedAccount);
                var selection = MyForestModel.SelectedAccount;
                //        _accountLowLevelOperator.SortDepositAccounts(MyForestModel.SelectedAccount.Parent);
                _accountLowLevelOperator.SortDepositAccounts(_accountTreeStraightener.Seek("Депозиты", _db.Accounts));
                MyForestModel.SelectedAccount = selection;
            }
            else
                _accountTreesGardener.ChangeAccount(MyForestModel.SelectedAccount);
        }

        public void ShowDepositReport()
        {
            if (!MyForestModel.SelectedAccount.IsDeposit() || MyForestModel.SelectedAccount.Children.Count != 0) return;

            foreach (var launchedForm in _launchedForms)
            {
                if (launchedForm is DepositViewModel && launchedForm.IsActive
                  && ((DepositViewModel)launchedForm).Deposit.ParentAccount == MyForestModel.SelectedAccount) launchedForm.TryClose();
            }

            var depositForm = IoC.Get<DepositViewModel>();
            _depositCalculationAggregator.FillinFieldsForOneDepositReport(MyForestModel.SelectedAccount.Deposit);
            depositForm.SetAccount(MyForestModel.SelectedAccount);
            _launchedForms.Add(depositForm);
            depositForm.RenewPressed += DepositViewModelRenewed; // подписываемся на переофрмление депозита, если оно произойдет надо сменить селекшен
            WindowManager.ShowWindow(depositForm);
        }

        void DepositViewModelRenewed(object sender, RenewPressedEventArgs e)
        {
            MyForestModel.SelectedAccount.IsSelected = false;
            MyForestModel.SelectedAccount = e.NewAccount;
        }

        #endregion
    }
}
