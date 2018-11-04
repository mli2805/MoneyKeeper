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

        private KeeperDb _keeperDb;

        public ShellViewModel(ILifetimeScope globalScope, IWindowManager windowManager, KeeperDb keeperDb,
              MainMenuViewModel mainMenuViewModel, AccountTreeViewModel accountTreeViewModel)
        {
            _globalScope = globalScope;
            _windowManager = windowManager;

            MainMenuViewModel = mainMenuViewModel;
            AccountTreeViewModel = accountTreeViewModel;

            _keeperDb = keeperDb;
        }
        protected async override void OnViewLoaded(object view)
        {
            DisplayName = "Keeper 2018";
        }

        public async override void CanClose(Action<bool> callback)
        {
            AccountTreeViewModel.RefreshPlaneList();
            await DbSerializer.Serialize(_keeperDb);
            base.CanClose(callback);
        }
    }
}