using System;
using Caliburn.Micro;

namespace Keeper2018
{
    public class ShellViewModel : Screen, IShell
    {
        public MainMenuViewModel MainMenuViewModel { get; set; }
        public AccountTreeViewModel AccountTreeViewModel { get; set; }

        private readonly KeeperDb _keeperDb;

        public ShellViewModel(KeeperDb keeperDb,
              MainMenuViewModel mainMenuViewModel, AccountTreeViewModel accountTreeViewModel)
        {
            MainMenuViewModel = mainMenuViewModel;
            AccountTreeViewModel = accountTreeViewModel;

            _keeperDb = keeperDb;
        }
        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Keeper 2018";
        }

        public override async void CanClose(Action<bool> callback)
        {
            _keeperDb.Flatten();
            await DbSerializer.Serialize(_keeperDb.Bin);
            base.CanClose(callback);
        }
    }
}