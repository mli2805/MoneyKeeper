using System.Diagnostics;
using System.Linq;
using Caliburn.Micro;

namespace Keeper2018
{
    public class AccountTreeViewModel : PropertyChangedBase
    {
        private readonly PdfProvider _pdfProvider;
        private readonly OneAccountViewModel _oneAccountViewModel;
        private readonly OneDepositViewModel _oneDepositViewModel;
        private readonly DepositReportViewModel _depositReportViewModel;
        private readonly BalanceVerificationViewModel _balanceVerificationViewModel;
        public IWindowManager WindowManager { get; }
        public ShellPartsBinder ShellPartsBinder { get; }
        public AskDragAccountActionViewModel AskDragAccountActionViewModel { get; }

        public KeeperDb KeeperDb { get; set; }

        public AccountTreeViewModel(KeeperDb keeperDb, IWindowManager windowManager, ShellPartsBinder shellPartsBinder,
            PdfProvider pdfProvider, AskDragAccountActionViewModel askDragAccountActionViewModel,
            OneAccountViewModel oneAccountViewModel, OneDepositViewModel oneDepositViewModel,
            DepositReportViewModel depositReportViewModel, BalanceVerificationViewModel balanceVerificationViewModel)
        {
            _pdfProvider = pdfProvider;
            _oneAccountViewModel = oneAccountViewModel;
            _oneDepositViewModel = oneDepositViewModel;
            _depositReportViewModel = depositReportViewModel;
            _balanceVerificationViewModel = balanceVerificationViewModel;
            WindowManager = windowManager;
            ShellPartsBinder = shellPartsBinder;
            AskDragAccountActionViewModel = askDragAccountActionViewModel;

            KeeperDb = keeperDb;
        }

        public void AddAccount()
        {
            var accountModel = new AccountModel("")
            {
                Id = KeeperDb.Bin.AccountPlaneList.Max(a => a.Id) + 1,
                Owner = ShellPartsBinder.SelectedAccountModel
            };
            _oneAccountViewModel.Initialize(accountModel, true);
            WindowManager.ShowDialog(_oneAccountViewModel);
            if (!_oneAccountViewModel.IsSavePressed) return;

            ShellPartsBinder.SelectedAccountModel.Items.Add(accountModel);
            KeeperDb.Bin.AccountPlaneList.Add(accountModel.Map());
        }

        public void AddAccountDeposit()
        {
            var accountModel = new AccountModel("");
            accountModel.Deposit = new Deposit();
            accountModel.Owner = ShellPartsBinder.SelectedAccountModel;

            _oneDepositViewModel.InitializeForm(accountModel, true);
            WindowManager.ShowDialog(_oneDepositViewModel);
            if (!_oneDepositViewModel.IsSavePressed) return;

            ShellPartsBinder.SelectedAccountModel.Items.Add(accountModel);
            KeeperDb.Bin.AccountPlaneList.Add(accountModel.Map());
        }

        public void ChangeAccount()
        {
            if (ShellPartsBinder.SelectedAccountModel.IsDeposit)
            {
                var accountModel = ShellPartsBinder.SelectedAccountModel;
                _oneDepositViewModel.InitializeForm(accountModel, false);
                WindowManager.ShowDialog(_oneDepositViewModel);

                if (_oneDepositViewModel.IsSavePressed) { }
            }
            else
            {
                var accountModel = ShellPartsBinder.SelectedAccountModel;
                _oneAccountViewModel.Initialize(accountModel, false);
                WindowManager.ShowDialog(_oneAccountViewModel);

                if (_oneAccountViewModel.IsSavePressed)
                    KeeperDb.FlattenAccountTree();
            }
        }
        public void RemoveSelectedAccount()
        {
            KeeperDb.RemoveSelectedAccount();
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

        public void ShowTagInDetails()
        {
            var document = _pdfProvider.Create(ShellPartsBinder.SelectedAccountModel);
            string filename = $@"c:\temp\{ShellPartsBinder.SelectedAccountModel.Name}.pdf";
            document.Save(filename);
            Process.Start(filename);
        }
    }
}
