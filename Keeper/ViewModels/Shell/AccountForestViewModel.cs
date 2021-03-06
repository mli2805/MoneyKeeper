﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Linq;
using Caliburn.Micro;
using Keeper.DomainModel.DbTypes;
using Keeper.DomainModel.Extentions;
using Keeper.Models.Shell;
using Keeper.Utils.AccountEditing;
using Keeper.Utils.DepositProcessing;
using Keeper.ViewModels.Deposits;

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
        
        private readonly DepositCalculationAggregator _depositCalculationAggregator;

        private readonly List<Screen> _launchedForms = new List<Screen>();

        public AccountForestModel MyForestModel { get; set; }

        [ImportingConstructor]
        public AccountForestViewModel(ShellModel shellModel, KeeperDb db, AccountTreesGardener accountTreesGardener,
          AccountLowLevelOperator accountLowLevelOperator, DepositCalculationAggregator depositCalculationAggregator)
        {
            MyForestModel = shellModel.MyForestModel;
            _db = db;
            _accountTreesGardener = accountTreesGardener;
            _accountLowLevelOperator = accountLowLevelOperator;
            
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
            _accountLowLevelOperator.SortDepositAccounts(_db.SeekAccount("Депозиты"));
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
                _accountLowLevelOperator.SortDepositAccounts(_db.SeekAccount("Депозиты"));
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
