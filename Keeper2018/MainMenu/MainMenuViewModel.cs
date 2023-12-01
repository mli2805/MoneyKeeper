using System;
using System.Windows;
using Caliburn.Micro;

namespace Keeper2018
{
    public class MainMenuViewModel : PropertyChangedBase
    {
        private readonly IWindowManager _windowManager;
        private readonly KeeperDataModel _keeperDataModel;
        private readonly DbSaver _dbSaver;
        private readonly ShellPartsBinder _shellPartsBinder;
        private readonly RatesViewModel _ratesViewModel;
        private readonly MonthAnalysisViewModel _monthAnalysisViewModel;
        private readonly TransactionsViewModel _transactionsViewModel;
        private readonly BankOffersViewModel _bankOffersViewModel;
        private readonly SettingsViewModel _settingsViewModel;
        private readonly MemosViewModel _memosViewModel;
        private readonly CarsViewModel _carsViewModel;
        private readonly ExpenseByCategoriesViewModel _expenseByCategoriesViewModel;
        private readonly DepoCurrResultViewModel _depoCurrResultViewModel;
        private readonly GskViewModel _gskViewModel;
        private readonly OpenDepositsViewModel _openDepositsViewModel;
        private readonly CardsAndAccountsViewModel _cardsAndAccountsViewModel;
        private readonly SalaryViewModel _salaryViewModel;
        private readonly InvestmentAssetsViewModel _investmentAssetsViewModel;
        private readonly AssetRatesViewModel _assetRatesViewModel;
        private readonly TrustAccountsViewModel _trustAccountsViewModel;
        private readonly InvestmentTransactionsViewModel _investmentTransactionsViewModel;
        private readonly InvestmentAnalysisViewModel _investmentAnalysisViewModel;
        private readonly ButtonCollectionBuilderViewModel _buttonCollectionBuilderViewModel;

        #region Reminder icon
        private Visibility _reminderIconVisibility = Visibility.Collapsed;
        public Visibility ReminderIconVisibility
        {
            get => _reminderIconVisibility;
            set
            {
                if (value == _reminderIconVisibility) return;
                _reminderIconVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        private Visibility _reminderWaitIconVisibility = Visibility.Visible;
        public Visibility ReminderWaitIconVisibility
        {
            get => _reminderWaitIconVisibility;
            set
            {
                if (value == _reminderWaitIconVisibility) return;
                _reminderWaitIconVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        private string _bellPath;
        public string BellPath
        {
            get => _bellPath;
            set
            {
                if (value == _bellPath) return;
                _bellPath = value;
                NotifyOfPropertyChange();
            }
        }

        public void SetBellPath()
        {
            var hasAlarm = _keeperDataModel.HasAlarm();
            BellPath = hasAlarm ? "../../Resources/mainmenu/yellow-bell.png" : "../../Resources/mainmenu/white-bell.png";
            ReminderWaitIconVisibility = Visibility.Collapsed;
            ReminderIconVisibility = Visibility.Visible;
        }
        #endregion

        #region Exchange icon
        private Visibility _exchangeWaitIconVisibility = Visibility.Visible;
        public Visibility ExchangeWaitIconVisibility    
        {
            get => _exchangeWaitIconVisibility;
            set
            {
                if (value == _exchangeWaitIconVisibility) return;
                _exchangeWaitIconVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        private Visibility _exchangeIconVisibility = Visibility.Collapsed;
       
        public Visibility ExchangeIconVisibility
        {
            get => _exchangeIconVisibility;
            set
            {
                if (value == _exchangeIconVisibility) return;
                _exchangeIconVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        public void SetExchangeIcon()
        {
            ExchangeWaitIconVisibility = Visibility.Collapsed;
            ExchangeIconVisibility = Visibility.Visible;
        }
        #endregion

        public MainMenuViewModel(IWindowManager windowManager, KeeperDataModel keeperDataModel,
            DbSaver dbSaver, ShellPartsBinder shellPartsBinder,
            TransactionsViewModel transactionsViewModel, RatesViewModel ratesViewModel,
            MonthAnalysisViewModel monthAnalysisViewModel, BankOffersViewModel bankOffersViewModel,
             SettingsViewModel settingsViewModel, MemosViewModel memosViewModel,
            CarsViewModel carsViewModel, ExpenseByCategoriesViewModel expenseByCategoriesViewModel,
            DepoCurrResultViewModel depoCurrResultViewModel, GskViewModel gskViewModel,
            OpenDepositsViewModel openDepositsViewModel,
            CardsAndAccountsViewModel cardsAndAccountsViewModel, SalaryViewModel salaryViewModel,
            InvestmentAssetsViewModel investmentAssetsViewModel, AssetRatesViewModel assetRatesViewModel,
            TrustAccountsViewModel trustAccountsViewModel, InvestmentTransactionsViewModel investmentTransactionsViewModel,
            InvestmentAnalysisViewModel investmentAnalysisViewModel, ButtonCollectionBuilderViewModel buttonCollectionBuilderViewModel)
        {
            _windowManager = windowManager;
            _keeperDataModel = keeperDataModel;
            _dbSaver = dbSaver;
            _shellPartsBinder = shellPartsBinder;
            _ratesViewModel = ratesViewModel;
            _monthAnalysisViewModel = monthAnalysisViewModel;
            _transactionsViewModel = transactionsViewModel;
            _bankOffersViewModel = bankOffersViewModel;
            _settingsViewModel = settingsViewModel;
            _memosViewModel = memosViewModel;
            _carsViewModel = carsViewModel;
            _expenseByCategoriesViewModel = expenseByCategoriesViewModel;
            _depoCurrResultViewModel = depoCurrResultViewModel;
            _gskViewModel = gskViewModel;
            _openDepositsViewModel = openDepositsViewModel;
            _cardsAndAccountsViewModel = cardsAndAccountsViewModel;
            _salaryViewModel = salaryViewModel;
            _investmentAssetsViewModel = investmentAssetsViewModel;
            _assetRatesViewModel = assetRatesViewModel;
            _trustAccountsViewModel = trustAccountsViewModel;
            _investmentTransactionsViewModel = investmentTransactionsViewModel;
            _investmentAnalysisViewModel = investmentAnalysisViewModel;
            _buttonCollectionBuilderViewModel = buttonCollectionBuilderViewModel;
        }

        // for short-cuts
        public void ActionMethod(MainMenuAction action)
        {
            switch (action)
            {
                case MainMenuAction.ShowTransactionsForm:
                    ShowTransactionsForm();
                    break;
                case MainMenuAction.ShowOfficialRatesForm:
                    ShowRatesForm();
                    break;
                case MainMenuAction.ShowMonthAnalysisForm:
                    ShowMonthAnalysisForm();
                    break;
                case MainMenuAction.ShowDepositOffersForm:
                    ShowDepositOffersForm();
                    break;
                case MainMenuAction.Save:
                    SaveAllDb();
                    break;
                case MainMenuAction.ShowSettingsForm:
                    ShowSettingsForm();
                    break;
                case MainMenuAction.Experiment:
                    break;
            }
        }

        public void ShowTransactionsForm()
        {
            try
            {
                _transactionsViewModel.Initialize();
                _windowManager.ShowDialog(_transactionsViewModel);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return;
            }

            if (_transactionsViewModel.Model.IsCollectionChanged)
            {
                _shellPartsBinder.JustToForceBalanceRecalculation = DateTime.Now;
                SaveAllDb();
            }
        }

        public void ShowRatesForm()
        {
            // initialized on application start (ShellViewModel.OnViewLoaded);
            _windowManager.ShowDialog(_ratesViewModel);
            _shellPartsBinder.JustToForceBalanceRecalculation = DateTime.Now;
            SaveAllDb();
        }

        public void ShowMonthAnalysisForm()
        {
            _monthAnalysisViewModel.Initialize();
            _windowManager.ShowDialog(_monthAnalysisViewModel);
        }

        public void ShowDepositOffersForm()
        {
            _bankOffersViewModel.Initialize();
            _windowManager.ShowDialog(_bankOffersViewModel);
        }

        public void ShowBalancesAndSaldosChart()
        {
            var vm = new BalancesAndSaldosViewModel();
            vm.Initialize(_keeperDataModel);
            _windowManager.ShowWindow(vm);
        }

        public void ShowExpensesChart()
        {
            _expenseByCategoriesViewModel.Initialize();
            _windowManager.ShowDialog(_expenseByCategoriesViewModel);
        }

        public void ShowDepoPlusCurreniesChart()
        {
            _windowManager.ShowDialog(_depoCurrResultViewModel);
        }

        public void ShowSalaryForm()
        {
            _salaryViewModel.Initialize();
            _windowManager.ShowDialog(_salaryViewModel);
        }

        public void ShowGskForm()
        {
            _gskViewModel.Initialize();
            _windowManager.ShowDialog(_gskViewModel);
        }

        public void ShowCarForm()
        {
            _carsViewModel.Initialize();
            _windowManager.ShowDialog(_carsViewModel);
        }

        public void ShowDepositsForm()
        {
            _openDepositsViewModel.Initialize();
            _windowManager.ShowWindow(_openDepositsViewModel);
        }

        public void ShowPayCardsForm()
        {
            _cardsAndAccountsViewModel.Initialize();
            _windowManager.ShowDialog(_cardsAndAccountsViewModel);
        }

        #region Investments
        public void ShowTrustAccountsForm()
        {
            _trustAccountsViewModel.Initialize();
            _windowManager.ShowWindow(_trustAccountsViewModel);
        }

        public void ShowInvestmentAssetsForm()
        {
            _investmentAssetsViewModel.Initialize();
            _windowManager.ShowWindow(_investmentAssetsViewModel);
        }

        public void ShowAssetRatesForm()
        {
            _assetRatesViewModel.Initialize();
            _windowManager.ShowWindow(_assetRatesViewModel);
        }

        public void ShowInvestmentTransactionsForm()
        {
            _investmentTransactionsViewModel.Initialize();
            _windowManager.ShowWindow(_investmentTransactionsViewModel);
            _shellPartsBinder.JustToForceBalanceRecalculation = DateTime.Now;
            SaveAllDb();
        }

        public void ShowInvestmentAnalysisForm()
        {
            _investmentAnalysisViewModel.Initialize();
            _windowManager.ShowWindow(_investmentAnalysisViewModel);
        }
        #endregion


        public async void SaveAllDb()
        {
            await _dbSaver.Save();
        }

        public void ShowRemindersForm()
        {
            _memosViewModel.Initialize();
            _windowManager.ShowDialog(_memosViewModel);
        }

        public void ShowSettingsForm()
        {
            _settingsViewModel.Initialize();
            _windowManager.ShowDialog(_settingsViewModel);
        }

        public void ShowButtonCollectionBuilder()
        {
            _buttonCollectionBuilderViewModel.Initialize();
            _windowManager.ShowDialog(_buttonCollectionBuilderViewModel);
        }
    }
}
