using System.Collections.ObjectModel;
using Caliburn.Micro;

namespace Keeper2018
{
    public class AccountTreeViewModel : PropertyChangedBase
    {
        public ObservableCollection<Account> MineAccounts { get; set; }
      

        public string Status { get; set; } = "Under construction";

    }
}
