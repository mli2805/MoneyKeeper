using System;
using System.Windows;
using Caliburn.Micro;
using Keeper2018.PayCards;

namespace Keeper2018
{
    public class MainMenuViewModel : PropertyChangedBase
    {
        private readonly IWindowManager _windowManager;
        private readonly KeeperDataModel _keeperDataModel;
        private readonly DbSaver _dbSaver;
        private readonly ShellPartsBinder _shellPartsBinder;
        private readonly CurrencyRatesViewModel _currencyRatesViewModel;
        private readonly MonthAnalysisViewModel _monthAnalysisViewModel;
        private readonly TransactionsViewModel _transactionsViewModel;
        private readonly BankOffersViewModel _bankOffersViewModel;
        private readonly ArticlesAssociationsViewModel _articlesAssociationsViewModel;
        private readonly SettingsViewModel _settingsViewModel;
        private readonly CarsViewModel _carsViewModel;
        private readonly ExpenseByCategoriesViewModel _expenseByCategoriesViewModel;
        private readonly DepoCurrResultViewModel _depoCurrResultViewModel;
        private readonly GskViewModel _gskViewModel;
        private readonly OpenDepositsViewModel _openDepositsViewModel;
        private readonly PayCardsViewModel _payCardsViewModel;
        private readonly SalaryViewModel _salaryViewModel;

        public MainMenuViewModel(IWindowManager windowManager, KeeperDataModel keeperDataModel,
            DbSaver dbSaver, ShellPartsBinder shellPartsBinder,
            TransactionsViewModel transactionsViewModel, CurrencyRatesViewModel currencyRatesViewModel,
            MonthAnalysisViewModel monthAnalysisViewModel, BankOffersViewModel bankOffersViewModel,
            ArticlesAssociationsViewModel articlesAssociationsViewModel, SettingsViewModel settingsViewModel,
            CarsViewModel carsViewModel, ExpenseByCategoriesViewModel expenseByCategoriesViewModel,
            DepoCurrResultViewModel depoCurrResultViewModel, GskViewModel gskViewModel,
            OpenDepositsViewModel openDepositsViewModel,
            PayCardsViewModel payCardsViewModel, SalaryViewModel salaryViewModel)
        {
            _windowManager = windowManager;
            _keeperDataModel = keeperDataModel;
            _dbSaver = dbSaver;
            _shellPartsBinder = shellPartsBinder;
            _currencyRatesViewModel = currencyRatesViewModel;
            _monthAnalysisViewModel = monthAnalysisViewModel;
            _transactionsViewModel = transactionsViewModel;
            _bankOffersViewModel = bankOffersViewModel;
            _articlesAssociationsViewModel = articlesAssociationsViewModel;
            _settingsViewModel = settingsViewModel;
            _carsViewModel = carsViewModel;
            _expenseByCategoriesViewModel = expenseByCategoriesViewModel;
            _depoCurrResultViewModel = depoCurrResultViewModel;
            _gskViewModel = gskViewModel;
            _openDepositsViewModel = openDepositsViewModel;
            _payCardsViewModel = payCardsViewModel;
            _salaryViewModel = salaryViewModel;
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
                    ShowOfficialRatesForm();
                    break;
                case MainMenuAction.ShowMonthAnalysisForm:
                    ShowMonthAnalysisForm();
                    break;
                case MainMenuAction.ShowDepositOffersForm:
                    ShowDepositOffersForm();
                    break;
                case MainMenuAction.ShowTagAssociationsForm:
                    ShowTagAssociationsForm();
                    break;
                case MainMenuAction.Save:
                    Save();
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
                if (_transactionsViewModel.IsFirstLaunch)
                    _transactionsViewModel.Initialize();
                else
                    _transactionsViewModel.ReInitializeOnlyAccountLists();

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
                Save();
            }
        }

        public void ShowOfficialRatesForm()
        {
            _windowManager.ShowDialog(_currencyRatesViewModel);
            _shellPartsBinder.JustToForceBalanceRecalculation = DateTime.Now;
            Save();
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
        public void ShowTagAssociationsForm()
        {
            _articlesAssociationsViewModel.Init();
            _windowManager.ShowDialog(_articlesAssociationsViewModel);
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
            _windowManager.ShowDialog(_openDepositsViewModel);
        }

        public void ShowPayCardsForm()
        {
            _payCardsViewModel.Initialize();
            _windowManager.ShowDialog(_payCardsViewModel);
        }

        public async void Save()
        {
            await _dbSaver.Save();
        }

        public void ShowSettingsForm()
        {
            _windowManager.ShowDialog(_settingsViewModel);
        }
    }
}
