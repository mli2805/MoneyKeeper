using Caliburn.Micro;

namespace Keeper2018
{
    public class AccountTreeViewModel : PropertyChangedBase
    {
        private readonly OneAccountViewModel _oneAccountViewModel;
        private readonly OneDepositViewModel _oneDepositViewModel;
        private AccountModel _selectedAccount;
        public IWindowManager WindowManager { get; }
        public AskDragAccountActionViewModel AskDragAccountActionViewModel { get; }

        public KeeperDb KeeperDb { get; set; }

        public AccountModel SelectedAccount
        {
            get => _selectedAccount;
            set
            {
                if (Equals(value, _selectedAccount)) return;
                _selectedAccount = value;
                NotifyOfPropertyChange();
            }
        }

        public AccountTreeViewModel(KeeperDb keeperDb, IWindowManager windowManager,
            AskDragAccountActionViewModel askDragAccountActionViewModel, 
            OneAccountViewModel oneAccountViewModel, OneDepositViewModel oneDepositViewModel)
        {
            _oneAccountViewModel = oneAccountViewModel;
            _oneDepositViewModel = oneDepositViewModel;
            WindowManager = windowManager;
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
            if (SelectedAccount.IsDeposit)
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

        }

    }
}
