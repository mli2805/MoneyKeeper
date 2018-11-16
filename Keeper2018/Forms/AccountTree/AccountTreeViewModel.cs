using Caliburn.Micro;

namespace Keeper2018
{
    public class AccountTreeViewModel : PropertyChangedBase
    {
        public IWindowManager WindowManager { get; }
        public AskDragAccountActionViewModel AskDragAccountActionViewModel { get; }

        public KeeperDb KeeperDb { get; set; }

        public bool IsSelectedAccountFolder => KeeperDb.GetSelectedAccountModel().IsFolder;
        public bool IsSelectedAccountDeposit => KeeperDb.GetSelectedAccountModel().IsDeposit;

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
