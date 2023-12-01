using System;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace Keeper2018
{
    public class ShellViewModel : Screen, IShell
    {
        public MainMenuViewModel MainMenuViewModel { get; set; }
        public AccountTreeViewModel AccountTreeViewModel { get; }
        public BalanceOrTrafficViewModel BalanceOrTrafficViewModel { get; }
        public TwoSelectorsViewModel TwoSelectorsViewModel { get; }

        private readonly KeeperDataModel _keeperDataModel;
        private readonly DbLoader _dbLoader;
        private readonly DbSaver _dbSaver;
        private readonly RatesViewModel _ratesViewModel;
        public ShellPartsBinder ShellPartsBinder { get; }
        private bool _dbLoaded;

        public ShellViewModel(KeeperDataModel keeperDataModel, ShellPartsBinder shellPartsBinder,
            DbLoader dbLoader, DbSaver dbSaver, RatesViewModel ratesViewModel,
            MainMenuViewModel mainMenuViewModel, AccountTreeViewModel accountTreeViewModel,
            BalanceOrTrafficViewModel balanceOrTrafficViewModel, TwoSelectorsViewModel twoSelectorsViewModel)
        {
            MainMenuViewModel = mainMenuViewModel;
            AccountTreeViewModel = accountTreeViewModel;
            BalanceOrTrafficViewModel = balanceOrTrafficViewModel;
            TwoSelectorsViewModel = twoSelectorsViewModel;

            _keeperDataModel = keeperDataModel;
            _dbLoader = dbLoader;
            _dbSaver = dbSaver;
            _ratesViewModel = ratesViewModel;
            ShellPartsBinder = shellPartsBinder;
        }

        protected override async void OnViewLoaded(object view)
        {
            DisplayName = "Keeper 2018";
            _dbLoaded = await _dbLoader.LoadAndExpand();
            if (!_dbLoaded)
            {
                TryClose();
                return;
            }

            await Task.Factory.StartNew(RatesInitializationLongOperation);
            await Task.Factory.StartNew(MemosInitializationLongOperation);

            var account = _keeperDataModel.AccountsTree.First(r => r.Name == "Мои");
            account.IsSelected = true;
            ShellPartsBinder.SelectedAccountItemModel = account;
        }

        private async Task RatesInitializationLongOperation()
        {
            await _ratesViewModel.Initialize();
            MainMenuViewModel.SetExchangeIcon();
        }

        private async Task MemosInitializationLongOperation()
        {
            await _keeperDataModel.RememberAll();
            MainMenuViewModel.SetBellPath();
        }

        public override async void CanClose(Action<bool> callback)
        {
            if (_dbLoaded)
            {
                if (!await _dbSaver.SaveWithUi()) return;
            }

            base.CanClose(callback);
        }
    }
}