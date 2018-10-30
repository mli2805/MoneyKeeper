using System;
using System.Collections.ObjectModel;
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

        public ShellViewModel(ILifetimeScope globalScope, IWindowManager windowManager,
              MainMenuViewModel mainMenuViewModel, AccountTreeViewModel accountTreeViewModel,
            KeeperDb keeperDb)
        {
            _globalScope = globalScope;
            _windowManager = windowManager;

            MainMenuViewModel = mainMenuViewModel;
            AccountTreeViewModel = accountTreeViewModel;
            AccountTreeViewModel.Status = "Status set from ShellViewModel";
           // AccountTreeViewModel.Accounts = AccountsOldTxt.LoadFromOldTxt();
            AccountTreeViewModel.Accounts = Accounts2018Txt.LoadFromTxt();

            _keeperDb = keeperDb;
        }
        protected async override void OnViewLoaded(object view)
        {
            DisplayName = "Keeper 2018";

            _keeperDb = await DbSerializer.Deserialize();
            if (_keeperDb == null)
            {
                _keeperDb = new KeeperDb();
                _keeperDb.Accounts = Accounts2018Txt.LoadFromTxt();
                var rates = await RatesSerializer.DeserializeRates() ?? await NbRbRatesOldTxt.LoadFromOldTxtAsync();
                 
                _keeperDb.OfficialRates = new ObservableCollection<OfficialRates>();
                foreach (var rate in rates)
                {
                    _keeperDb.OfficialRates.Add(rate);
                }
            }
            var vm = _globalScope.Resolve<OfficialRatesViewModel>();
        }

        public async override void CanClose(Action<bool> callback)
        {
            //Accounts2018Txt.SaveInTxt(AccountTreeViewModel.Accounts);
            await DbSerializer.Serialize(_keeperDb);
            base.CanClose(callback);
        }
    }
}