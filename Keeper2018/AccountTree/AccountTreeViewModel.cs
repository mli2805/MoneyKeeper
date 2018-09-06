using System.Collections.ObjectModel;
using Caliburn.Micro;

namespace Keeper2018
{
    public class AccountTreeViewModel : PropertyChangedBase
    {
        public IWindowManager WindowManager { get; }
        public AskDragAccountActionViewModel AskDragAccountActionViewModel { get; }
        public ObservableCollection<Account> Accounts { get; set; }

        public string Status { get; set; } = "Under construction";

        public AccountTreeViewModel(IWindowManager windowManager, AskDragAccountActionViewModel askDragAccountActionViewModel)
        {
            WindowManager = windowManager;
            AskDragAccountActionViewModel = askDragAccountActionViewModel;
        }
    }
}
