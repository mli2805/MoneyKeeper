using System;
using Autofac;
using Caliburn.Micro;

namespace Keeper2018
{
    public class ShellViewModel : Screen, IShell
    {
        private ILifetimeScope _globalScope;
        private IWindowManager _windowManager;
        public MainMenuViewModel MainMenuViewModel { get; set; }
        public AccountTreeViewModel AccountTreeViewModel { get; set; }

        public ShellViewModel(ILifetimeScope globalScope, IWindowManager windowManager,
              MainMenuViewModel mainMenuViewModel, AccountTreeViewModel accountTreeViewModel)
        {
            _globalScope = globalScope;
            _windowManager = windowManager;

            MainMenuViewModel = mainMenuViewModel;
            AccountTreeViewModel = accountTreeViewModel;
            AccountTreeViewModel.Status = "Status set from ShellViewModel";
           // AccountTreeViewModel.Accounts = AccountsOldTxt.LoadFromOldTxt();
            AccountTreeViewModel.Accounts = Accounts2018Txt.LoadFromTxt();
        }
        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Keeper 2018";

            var vm = _globalScope.Resolve<OfficialRatesViewModel>();
        }

        public override void CanClose(Action<bool> callback)
        {
            Accounts2018Txt.SaveInTxt(AccountTreeViewModel.Accounts);
            base.CanClose(callback);
        }
    }
}