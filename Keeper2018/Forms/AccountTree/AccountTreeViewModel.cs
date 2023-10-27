using System.Linq;
using Caliburn.Micro;
using Keeper2018.ExpensesOnAccount;
using KeeperDomain;

namespace Keeper2018
{
    public class AccountTreeViewModel : PropertyChangedBase
    {
        private readonly OneAccountViewModel _oneAccountViewModel;
        private readonly OneBankAccountViewModel _oneBankAccountViewModel;
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
            OneAccountViewModel oneAccountViewModel, OneBankAccountViewModel oneBankAccountViewModel,
            ExpensesOnAccountViewModel expensesOnAccountViewModel, DepositInterestViewModel depositInterestViewModel,
            DepositReportViewModel depositReportViewModel, BalanceVerificationViewModel balanceVerificationViewModel)
        {
            _oneAccountViewModel = oneAccountViewModel;
            _oneBankAccountViewModel = oneBankAccountViewModel;
            _expensesOnAccountViewModel = expensesOnAccountViewModel;
            _depositInterestViewModel = depositInterestViewModel;
            _depositReportViewModel = depositReportViewModel;
            _balanceVerificationViewModel = balanceVerificationViewModel;
            WindowManager = windowManager;
            ShellPartsBinder = shellPartsBinder;
            AskDragAccountActionViewModel = askDragAccountActionViewModel;

            KeeperDataModel = keeperDataModel;
        }

        public void AddFolder()
        {
            var accountItemModel = new AccountItemModel(KeeperDataModel.AcMoDict.Keys.Max() + 1,
                "", ShellPartsBinder.SelectedAccountItemModel);
            accountItemModel.IsFolder = true;
            var oneFolderVm = new OneFolderViewModel();
            oneFolderVm.Initialize(accountItemModel, true);
            WindowManager.ShowDialog(oneFolderVm);
            if (!oneFolderVm.IsSavePressed) return;

            ShellPartsBinder.SelectedAccountItemModel.Children.Add(accountItemModel);
            KeeperDataModel.AcMoDict.Add(accountItemModel.Id, accountItemModel);
        }

        public void AddAccount(int param)
        {
            var accountItemModel = new AccountItemModel(KeeperDataModel.AcMoDict.Keys.Max() + 1,
                    "", ShellPartsBinder.SelectedAccountItemModel);

            // if ((accountItemModel.Parent?.Parent?.Id ?? 0) == 161)
            if (param == 1) // контрагент или тэг
            {
                _oneAccountViewModel.Initialize(accountItemModel, true);
                WindowManager.ShowDialog(_oneAccountViewModel);
                if (!_oneAccountViewModel.IsSavePressed) return;
            }
            else // 2 - счет в банке; 3 - депозит; 4 - карточка 
            {
                accountItemModel.BankAccount = new BankAccountModel { Id = accountItemModel.Id, };
                if (param == 3)
                    accountItemModel.BankAccount.Deposit = new Deposit() { Id = accountItemModel.Id };
                if (param == 4)
                    accountItemModel.BankAccount.PayCard = new PayCard() { Id = accountItemModel.Id };

                _oneBankAccountViewModel.Initialize(accountItemModel, true, param == 4);
                WindowManager.ShowDialog(_oneBankAccountViewModel);
                if (!_oneBankAccountViewModel.IsSavePressed) return;
            }

            ShellPartsBinder.SelectedAccountItemModel.Children.Add(accountItemModel);
            KeeperDataModel.AcMoDict.Add(accountItemModel.Id, accountItemModel);
        }

        public void ChangeAccount()
        {
            var accountItemModel = ShellPartsBinder.SelectedAccountItemModel;
            if (accountItemModel.IsFolder)
            {
                var vm = new OneFolderViewModel();
                vm.Initialize(accountItemModel, false);
                WindowManager.ShowDialog(vm);
            }
            else if (accountItemModel.IsCard)
            {
                _oneBankAccountViewModel.Initialize(accountItemModel, false, true);
                WindowManager.ShowDialog(_oneBankAccountViewModel);
            }
            else if (accountItemModel.IsDeposit)
            {
                _oneBankAccountViewModel.Initialize(accountItemModel, false, false);
                WindowManager.ShowDialog(_oneBankAccountViewModel);
            }
            else if (accountItemModel.IsBankAccount)
            {
                _oneBankAccountViewModel.Initialize(accountItemModel, false, false);
                WindowManager.ShowDialog(_oneBankAccountViewModel);
            }
            else
            {
                _oneAccountViewModel.Initialize(accountItemModel, false);
                WindowManager.ShowDialog(_oneAccountViewModel);
            }
        }
        public void RemoveSelectedAccount()
        {
            KeeperDataModel.RemoveSelectedAccount();
        }


        public void EnrollDepositInterest()
        {
            if (_depositInterestViewModel.Initialize(ShellPartsBinder.SelectedAccountItemModel))
                WindowManager.ShowDialog(_depositInterestViewModel);
        }

        public void ShowDepositReport()
        {
            _depositReportViewModel.Initialize(ShellPartsBinder.SelectedAccountItemModel);
            WindowManager.ShowDialog(_depositReportViewModel);
        }

        public void ShowVerificationForm()
        {
            _balanceVerificationViewModel.Initialize(ShellPartsBinder.SelectedAccountItemModel);
            WindowManager.ShowDialog(_balanceVerificationViewModel);
        }

        public void ShowFolderSummaryForm()
        {
            var folderSummaryViewModel = new FolderSummaryViewModel(KeeperDataModel);
            folderSummaryViewModel.Initialize(ShellPartsBinder.SelectedAccountItemModel);
            WindowManager.ShowWindow(folderSummaryViewModel);
        }

        public void ShowPaymentWaysForm()
        {
            var paymentWaysViewModel = new PaymentWaysViewModel(KeeperDataModel);
            paymentWaysViewModel.Initialize(ShellPartsBinder.SelectedAccountItemModel);
            WindowManager.ShowWindow(paymentWaysViewModel);
        }

        public void ShowExpensesOnAccount()
        {
            _expensesOnAccountViewModel.Initialize(ShellPartsBinder.SelectedAccountItemModel, ShellPartsBinder.SelectedPeriod);
            WindowManager.ShowDialog(_expensesOnAccountViewModel);
        }
    }
}
