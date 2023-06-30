using System;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using Keeper2018.ExpensesOnAccount;
using KeeperDomain;

namespace Keeper2018
{
    public class AccountTreeViewModel : PropertyChangedBase
    {
        private readonly OneAccountViewModel _oneAccountViewModel;
        private readonly OneDepositViewModel _oneDepositViewModel;
        private readonly ExpensesOnAccountViewModel _expensesOnAccountViewModel;
        private readonly DepositInterestViewModel _depositInterestViewModel;
        private readonly DepositReportViewModel _depositReportViewModel;
        private readonly BalanceVerificationViewModel _balanceVerificationViewModel;
        public IWindowManager WindowManager { get; }
        public ShellPartsBinder ShellPartsBinder { get; }
        public AskDragAccountActionViewModel AskDragAccountActionViewModel { get; }

        public KeeperDataModel KeeperDataModel { get; set; }

      
        public AccountTreeViewModel(KeeperDataModel keeperDataModel, IWindowManager windowManager, ShellPartsBinder shellPartsBinder,
            AskDragAccountActionViewModel askDragAccountActionViewModel,
            OneAccountViewModel oneAccountViewModel, OneDepositViewModel oneDepositViewModel,
            ExpensesOnAccountViewModel expensesOnAccountViewModel, DepositInterestViewModel depositInterestViewModel,
            DepositReportViewModel depositReportViewModel, BalanceVerificationViewModel balanceVerificationViewModel)
        {
            _oneAccountViewModel = oneAccountViewModel;
            _oneDepositViewModel = oneDepositViewModel;
            _expensesOnAccountViewModel = expensesOnAccountViewModel;
            _depositInterestViewModel = depositInterestViewModel;
            _depositReportViewModel = depositReportViewModel;
            _balanceVerificationViewModel = balanceVerificationViewModel;
            WindowManager = windowManager;
            ShellPartsBinder = shellPartsBinder;
            AskDragAccountActionViewModel = askDragAccountActionViewModel;

            KeeperDataModel = keeperDataModel;
        }

        public void AddAccount()
        {
            if (KeeperDataModel.AccountUsedInTransaction(ShellPartsBinder.SelectedAccountModel.Id))
            {
                WindowManager.ShowDialog(new MyMessageBoxViewModel(MessageType.Error,
                    "Этот счет используется в проводках!"));
                return;
            }
            var accountModel = new AccountModel("")
            {
                Id = KeeperDataModel.AcMoDict.Keys.Max() + 1,
                Owner = ShellPartsBinder.SelectedAccountModel
            };
            _oneAccountViewModel.Initialize(accountModel, true);
            WindowManager.ShowDialog(_oneAccountViewModel);
            if (!_oneAccountViewModel.IsSavePressed) return;

            ShellPartsBinder.SelectedAccountModel.Items.Add(accountModel);
            KeeperDataModel.AcMoDict.Add(accountModel.Id, accountModel);
        }

        public void AddAccountDeposit()
        {
            if (KeeperDataModel.AccountUsedInTransaction(ShellPartsBinder.SelectedAccountModel.Id))
            {
                WindowManager.ShowDialog(new MyMessageBoxViewModel(MessageType.Error,
                    "Этот счет используется в проводках!"));
                return;
            }
            var accountModel = new AccountModel("")
            {
                Id = KeeperDataModel.AcMoDict.Keys.Max() + 1,
                Owner = ShellPartsBinder.SelectedAccountModel,
                Deposit = new Deposit()
            };
            _oneDepositViewModel.InitializeForm(accountModel, true);
            WindowManager.ShowDialog(_oneDepositViewModel);
            if (!_oneDepositViewModel.IsSavePressed) return;

            accountModel.Deposit.Id = KeeperDataModel.AcMoDict.Values
                .Where(a => a.IsDeposit)
                .Max(d => d.Id) + 1;
            if (accountModel.Deposit.Card != null)
                accountModel.Deposit.Card.DepositId = accountModel.Deposit.Id;
            ShellPartsBinder.SelectedAccountModel.Items.Add(accountModel);
            KeeperDataModel.AcMoDict.Add(accountModel.Id, accountModel);
        }

        public void ChangeAccount()
        {
            if (ShellPartsBinder.SelectedAccountModel.IsDeposit)
            {
                var accountModel = ShellPartsBinder.SelectedAccountModel;
                _oneDepositViewModel.InitializeForm(accountModel, false);
                WindowManager.ShowDialog(_oneDepositViewModel);
            }
            else
            {
                var accountModel = ShellPartsBinder.SelectedAccountModel;
                _oneAccountViewModel.Initialize(accountModel, false);
                WindowManager.ShowDialog(_oneAccountViewModel);
            }
        }
        public void RemoveSelectedAccount()
        {
            KeeperDataModel.RemoveSelectedAccount();
        }


        public void EnrollDepositInterest()
        {
            if (_depositInterestViewModel.Initialize(ShellPartsBinder.SelectedAccountModel))
                WindowManager.ShowDialog(_depositInterestViewModel);
        }

        public void ShowDepositReport()
        {
            _depositReportViewModel.Initialize(ShellPartsBinder.SelectedAccountModel);
            WindowManager.ShowDialog(_depositReportViewModel);
        }

        public void ShowVerificationForm()
        {
            _balanceVerificationViewModel.Initialize(ShellPartsBinder.SelectedAccountModel);
            WindowManager.ShowDialog(_balanceVerificationViewModel);
        }

        public void ShowFolderSummaryForm()
        {
            var folderSummaryViewModel = new FolderSummaryViewModel(KeeperDataModel);
            folderSummaryViewModel.Initialize(ShellPartsBinder.SelectedAccountModel);
            WindowManager.ShowWindow(folderSummaryViewModel);
        }

        public void ShowPaymentWaysForm()
        {
            var paymentWaysViewModel = new PaymentWaysViewModel(KeeperDataModel);
            paymentWaysViewModel.Initialize(ShellPartsBinder.SelectedAccountModel);
            WindowManager.ShowWindow(paymentWaysViewModel);
        }

        public void ShowExpensesOnAccount()
        {
            _expensesOnAccountViewModel.Initialize(ShellPartsBinder.SelectedAccountModel, ShellPartsBinder.SelectedPeriod);
            WindowManager.ShowDialog(_expensesOnAccountViewModel);
        }

        public void ConvertToDeposit()
        {
            if (ShellPartsBinder.SelectedAccountModel.IsDeposit) return;
            ShellPartsBinder.SelectedAccountModel.Deposit =
                new Deposit
                {
                    MyAccountId = ShellPartsBinder.SelectedAccountModel.Id,

                };
            _oneDepositViewModel.InitializeForm(ShellPartsBinder.SelectedAccountModel, true);
            WindowManager.ShowDialog(_oneDepositViewModel);
        }
    }
}
