using Caliburn.Micro;

namespace Keeper2018
{
    public class AccountTreeViewModel : PropertyChangedBase
    {
        private readonly OneAccountViewModel _oneAccountViewModel;
        private readonly OneDepositViewModel _oneDepositViewModel;
        private readonly DepositReportViewModel _depositReportViewModel;
        private readonly DepositReportFactory _depositReportFactory;
        public IWindowManager WindowManager { get; }
        public ShellPartsBinder ShellPartsBinder { get; }
        public AskDragAccountActionViewModel AskDragAccountActionViewModel { get; }

        public KeeperDb KeeperDb { get; set; }

        public AccountTreeViewModel(KeeperDb keeperDb, IWindowManager windowManager, ShellPartsBinder shellPartsBinder,
            AskDragAccountActionViewModel askDragAccountActionViewModel, 
            OneAccountViewModel oneAccountViewModel, OneDepositViewModel oneDepositViewModel, 
            DepositReportViewModel depositReportViewModel, DepositReportFactory depositReportFactory)
        {
            _oneAccountViewModel = oneAccountViewModel;
            _oneDepositViewModel = oneDepositViewModel;
            _depositReportViewModel = depositReportViewModel;
            _depositReportFactory = depositReportFactory;
            WindowManager = windowManager;
            ShellPartsBinder = shellPartsBinder;
            AskDragAccountActionViewModel = askDragAccountActionViewModel;

            KeeperDb = keeperDb;
        }

        public void AddAccount()
        {
            WindowManager.ShowDialog(_oneAccountViewModel);
        }

        public void AddAccountDeposit()
        {
            WindowManager.ShowDialog(_oneDepositViewModel);
        }

        public void ChangeAccount()
        {
            if (ShellPartsBinder.SelectedAccountModel.IsDeposit)
                WindowManager.ShowDialog(_oneDepositViewModel);
            else
                WindowManager.ShowDialog(_oneAccountViewModel);
        }
        public void RemoveSelectedAccount()
        {
            KeeperDb.RemoveSelectedAccount();
        }

        public void ShowDepositReport()
        {
            _depositReportViewModel.Model = _depositReportFactory.Create(ShellPartsBinder.SelectedAccountModel);
            WindowManager.ShowDialog(_depositReportViewModel);
        }

    }
}
