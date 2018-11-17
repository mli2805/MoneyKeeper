using System;
using Caliburn.Micro;

namespace Keeper2018
{
    public class ShellViewModel : Screen, IShell
    {
        public MainMenuViewModel MainMenuViewModel { get; set; }
        public AccountTreeViewModel AccountTreeViewModel { get; set; }

        private readonly KeeperDb _keeperDb;
        private readonly IWindowManager _windowManager;
        private readonly DbLoadingViewModel _dbLoadingViewModel;
        private readonly OfficialRatesViewModel _officialRatesViewModel;
        private bool _dbLoaded;

        public ShellViewModel(IWindowManager windowManager, KeeperDb keeperDb,
            DbLoadingViewModel dbLoadingViewModel, MainMenuViewModel mainMenuViewModel,
            AccountTreeViewModel accountTreeViewModel, OfficialRatesViewModel officialRatesViewModel)
        {
            _windowManager = windowManager;
            _dbLoadingViewModel = dbLoadingViewModel;
            _officialRatesViewModel = officialRatesViewModel;

            MainMenuViewModel = mainMenuViewModel;
            AccountTreeViewModel = accountTreeViewModel;

            _keeperDb = keeperDb;
        }

        protected override async void OnViewLoaded(object view)
        {
            DisplayName = "Keeper 2018";
            _keeperDb.Bin = await DbSerializer.Deserialize();
            if (_keeperDb.Bin == null)
            {
                _windowManager.ShowDialog(_dbLoadingViewModel);
                if (!_dbLoadingViewModel.DbLoaded)
                    TryClose();
            }
            _dbLoaded = true;
            await DbLoader.ExpandBinToDb(_keeperDb);

            _officialRatesViewModel.Initialize();
        }

        public override async void CanClose(Action<bool> callback)
        {
            if (_dbLoaded)
            {
                _keeperDb.Flatten();
                await DbSerializer.Serialize(_keeperDb.Bin);
            }
            base.CanClose(callback);
        }
    }
}