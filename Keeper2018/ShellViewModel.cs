using System.Collections.ObjectModel;
using Caliburn.Micro;

namespace Keeper2018
{
    public class ShellViewModel : Screen, IShell
    {
        public AccountTreeViewModel AccountTreeViewModel { get; set; } = new AccountTreeViewModel();

        public ShellViewModel()
        {
            AccountTreeViewModel.Status = "Status set from ShellViewModel";
            AccountTreeViewModel.MineAccounts = new ObservableCollection<Account>()
            {
                AccountFactory.CreateExample()
            };
        }
        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Keeper 2018";
        }

      
    }
}