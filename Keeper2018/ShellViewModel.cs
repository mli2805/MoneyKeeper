using System;
using Caliburn.Micro;

namespace Keeper2018
{
    public class ShellViewModel : Screen, IShell
    {
        public AccountTreeViewModel AccountTreeViewModel { get; set; }

        public ShellViewModel(AccountTreeViewModel accountTreeViewModel)
        {
            AccountTreeViewModel = accountTreeViewModel;
            AccountTreeViewModel.Status = "Status set from ShellViewModel";
           // AccountTreeViewModel.Accounts = AccountsOldTxt.LoadFromOldTxt();
            AccountTreeViewModel.Accounts = Accounts2018Txt.LoadFromTxt();
        }
        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Keeper 2018";
        }

        public override void CanClose(Action<bool> callback)
        {
            Accounts2018Txt.SaveInTxt(AccountTreeViewModel.Accounts);
            base.CanClose(callback);
        }
    }
}