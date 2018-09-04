using System.Collections.Generic;
using Caliburn.Micro;

namespace Keeper2018
{
    public class Account
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public Account Parent { get; set; }
        public List<Account> Children { get; set; }
        public int DepositCode { get; set; } = -1;
    }


    public class ShellViewModel : Screen, IShell
    {
        public AccountTreeViewModel AccountTreeViewModel { get; set; } = new AccountTreeViewModel();
        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Keeper 2018";
        }

      
    }
}