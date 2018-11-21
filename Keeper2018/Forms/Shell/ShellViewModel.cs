using System;
using System.Linq;
using Caliburn.Micro;

namespace Keeper2018
{
    public enum BalanceOrTraffic
    {
        Balance,
        Traffic,
    }

    public class Period
    {
        public DateTime StartDate { get; set; } = new DateTime(2002,1,1);
        public DateTime FinishMoment { get; set; }

        public bool IsIn(DateTime timestamp)
        {
            return timestamp > StartDate && timestamp < FinishMoment;
        }
    }

    public class ShellPartsBinder : PropertyChangedBase
    {
        private AccountModel _selectedAccountModel;

        public AccountModel SelectedAccountModel
        {
            get => _selectedAccountModel;
            set
            {
                if (Equals(value, _selectedAccountModel)) return;
                _selectedAccountModel = value;
                NotifyOfPropertyChange();
            }
        }

        private BalanceOrTraffic _balanceOrTraffic;
        public BalanceOrTraffic BalanceOrTraffic    
        {
            get => _balanceOrTraffic;
            set
            {
                if (value == _balanceOrTraffic) return;
                _balanceOrTraffic = value;
                NotifyOfPropertyChange();
            }
        }

        public Period SelectedPeriod { get; set; } = new Period(){FinishMoment = DateTime.Today.Date.AddDays(1).AddSeconds(-1)};
    }

    public class ShellViewModel : Screen, IShell
    {
        public MainMenuViewModel MainMenuViewModel { get; set; }
        public AccountTreeViewModel AccountTreeViewModel { get;  }
        public BalanceOrTrafficViewModel BalanceOrTrafficViewModel { get; }
        public DateOrPeriodViewModel DateOrPeriodViewModel { get; }

        private readonly KeeperDb _keeperDb;
        private readonly ShellPartsBinder _shellPartsBinder;
        private readonly IWindowManager _windowManager;
        private readonly DbLoadingViewModel _dbLoadingViewModel;
        private readonly OfficialRatesViewModel _officialRatesViewModel;
        private bool _dbLoaded;

        public ShellViewModel(IWindowManager windowManager, KeeperDb keeperDb, DbLoadingViewModel dbLoadingViewModel,
            MainMenuViewModel mainMenuViewModel, ShellPartsBinder shellPartsBinder,
            AccountTreeViewModel accountTreeViewModel, BalanceOrTrafficViewModel balanceOrTrafficViewModel, 
            DateOrPeriodViewModel dateOrPeriodViewModel,
            OfficialRatesViewModel officialRatesViewModel)
        {
            _windowManager = windowManager;
            _dbLoadingViewModel = dbLoadingViewModel;
            _officialRatesViewModel = officialRatesViewModel;

            MainMenuViewModel = mainMenuViewModel;
            AccountTreeViewModel = accountTreeViewModel;
            BalanceOrTrafficViewModel = balanceOrTrafficViewModel;
            DateOrPeriodViewModel = dateOrPeriodViewModel;

            _keeperDb = keeperDb;
            _shellPartsBinder = shellPartsBinder;
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
            DbLoader.ExpandBinToDb(_keeperDb);
            _shellPartsBinder.SelectedAccountModel = _keeperDb.AccountsTree.First(r => r.Name == "Мои");
            _officialRatesViewModel.Initialize();
        }

        public override async void CanClose(Action<bool> callback)
        {
            if (_dbLoaded)
            {
                _keeperDb.DbToBin();
                await DbSerializer.Serialize(_keeperDb.Bin);
            }
            base.CanClose(callback);
        }
    }
}