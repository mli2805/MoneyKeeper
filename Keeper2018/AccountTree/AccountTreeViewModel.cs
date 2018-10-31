using System.Collections.ObjectModel;
using Caliburn.Micro;

namespace Keeper2018
{
    public class AccountTreeViewModel : PropertyChangedBase
    {
        public IWindowManager WindowManager { get; }
        public AskDragAccountActionViewModel AskDragAccountActionViewModel { get; }

        public ObservableCollection<Account> Accounts { get; set; }

        public AccountTreeViewModel(KeeperDb keeperDb, IWindowManager windowManager,
            AskDragAccountActionViewModel askDragAccountActionViewModel)
        {
            WindowManager = windowManager;
            AskDragAccountActionViewModel = askDragAccountActionViewModel;

            Accounts = keeperDb.Accounts;
        }

    }
}
