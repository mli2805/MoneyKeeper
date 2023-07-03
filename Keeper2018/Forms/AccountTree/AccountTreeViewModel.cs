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
        private readonly OneCardViewModel _oneCardViewModel;
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
            OneAccountViewModel oneAccountViewModel, OneDepositViewModel oneDepositViewModel, OneCardViewModel oneCardViewModel,
            ExpensesOnAccountViewModel expensesOnAccountViewModel, DepositInterestViewModel depositInterestViewModel,
            DepositReportViewModel depositReportViewModel, BalanceVerificationViewModel balanceVerificationViewModel)
        {
            _oneAccountViewModel = oneAccountViewModel;
            _oneDepositViewModel = oneDepositViewModel;
            _oneCardViewModel = oneCardViewModel;
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

        public void AddAccount()
        {
            var accountItemModel = new AccountItemModel(KeeperDataModel.AcMoDict.Keys.Max() + 1,
                    "", ShellPartsBinder.SelectedAccountItemModel);
            _oneAccountViewModel.Initialize(accountItemModel, true);
            WindowManager.ShowDialog(_oneAccountViewModel);
            if (!_oneAccountViewModel.IsSavePressed) return;

            ShellPartsBinder.SelectedAccountItemModel.Children.Add(accountItemModel);
            KeeperDataModel.AcMoDict.Add(accountItemModel.Id, accountItemModel);
        }

        public void AddCard()
        {

        }

        public void AddDeposit()
        {
            var accountModel = new AccountItemModel(KeeperDataModel.AcMoDict.Keys.Max() + 1,
                "", ShellPartsBinder.SelectedAccountItemModel)
            {
                Deposit = new Deposit()
            };
            _oneDepositViewModel.Initialize(accountModel, true);
            WindowManager.ShowDialog(_oneDepositViewModel);
            if (!_oneDepositViewModel.IsSavePressed) return;

            accountModel.Deposit.Id = KeeperDataModel.AcMoDict.Values
                .Where(a => a.IsDeposit)
                .Max(d => d.Id) + 1;
            //if (accountModel.PayCard != null)
            //    accountModel.PayCard.DepositId = accountModel.Deposit.Id;
            ShellPartsBinder.SelectedAccountItemModel.Children.Add(accountModel);
            KeeperDataModel.AcMoDict.Add(accountModel.Id, accountModel);
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
                _oneCardViewModel.Initialize(accountItemModel, false);
                WindowManager.ShowDialog(_oneCardViewModel);
            }
            else if (accountItemModel.IsDeposit)
            {
                _oneDepositViewModel.Initialize(accountItemModel, false);
                WindowManager.ShowDialog(_oneDepositViewModel);
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

        public void ConvertToDeposit()
        {
            if (ShellPartsBinder.SelectedAccountItemModel.IsDeposit) return;
            ShellPartsBinder.SelectedAccountItemModel.Deposit =
                new Deposit
                {
                    MyAccountId = ShellPartsBinder.SelectedAccountItemModel.Id,

                };
            _oneDepositViewModel.Initialize(ShellPartsBinder.SelectedAccountItemModel, true);
            WindowManager.ShowDialog(_oneDepositViewModel);
        }
    }
}
