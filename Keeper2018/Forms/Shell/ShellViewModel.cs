using System;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;

namespace Keeper2018
{
    public class ShellViewModel : Screen, IShell
    {
        public MainMenuViewModel MainMenuViewModel { get; set; }
        public AccountTreeViewModel AccountTreeViewModel { get; }
        public BalanceOrTrafficViewModel BalanceOrTrafficViewModel { get; }
        public TwoSelectorsViewModel TwoSelectorsViewModel { get; }

        private readonly KeeperDb _keeperDb;
        private readonly DbLoader _dbLoader;
        private readonly DbSaver _dbSaver;
        private readonly CurrencyRatesViewModel _currencyRatesViewModel;
        public ShellPartsBinder ShellPartsBinder { get; }
        private bool _dbLoaded;

        public ShellViewModel(KeeperDb keeperDb, ShellPartsBinder shellPartsBinder, 
            DbLoader dbLoader, DbSaver dbSaver, CurrencyRatesViewModel currencyRatesViewModel,
            MainMenuViewModel mainMenuViewModel, AccountTreeViewModel accountTreeViewModel, 
            BalanceOrTrafficViewModel balanceOrTrafficViewModel, TwoSelectorsViewModel twoSelectorsViewModel)
        {
            MainMenuViewModel = mainMenuViewModel;
            AccountTreeViewModel = accountTreeViewModel;
            BalanceOrTrafficViewModel = balanceOrTrafficViewModel;
            TwoSelectorsViewModel = twoSelectorsViewModel;

            _keeperDb = keeperDb;
            _dbLoader = dbLoader;
            _dbSaver = dbSaver;
            _currencyRatesViewModel = currencyRatesViewModel;
            ShellPartsBinder = shellPartsBinder;
        }

        protected override async void OnViewLoaded(object view)
        {
            DisplayName = "Keeper 2018";
            _dbLoaded = await _dbLoader.Load();
            if (!_dbLoaded)
            {
                TryClose();
                return;
            }

            ExpandBinToDb(_keeperDb);
            var account = _keeperDb.AccountsTree.First(r => r.Name == "Мои");
            account.IsSelected = true;
            ShellPartsBinder.SelectedAccountModel = account; 
        }

        private void ExpandBinToDb(KeeperDb keeperDb)
        {
            _currencyRatesViewModel.Initialize();
            keeperDb.FillInAccountTree(); // must be first

            keeperDb.TagAssociationModels = new ObservableCollection<TagAssociationModel>
                (keeperDb.Bin.TagAssociations.Select(a => a.Map(keeperDb.AcMoDict)));
        }
      
        public override async void CanClose(Action<bool> callback)
        {
            if (_dbLoaded)
            {
                if (!await _dbSaver.Save()) return;
            }

            base.CanClose(callback);
        }
    }
}