using Caliburn.Micro;

namespace Keeper2018
{
    public class AccountTreeViewModel : PropertyChangedBase
    {
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
            AskDragAccountActionViewModel askDragAccountActionViewModel)
        {
            WindowManager = windowManager;
            AskDragAccountActionViewModel = askDragAccountActionViewModel;

            KeeperDb = keeperDb;
        }

        public void AddAccount()
        {

        }

        public void AddAccountDeposit()
        {

        }

        public void ChangeAccount()
        {

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
