using System;
using System.Windows;
using Caliburn.Micro;
using Keeper2018.PayCards;

namespace Keeper2018
{
    public class MainMenuViewModel : PropertyChangedBase
    {
        private readonly IWindowManager _windowManager;
        private readonly KeeperDb _keeperDb;
        private readonly ShellPartsBinder _shellPartsBinder;
        private readonly CurrencyRatesViewModel _currencyRatesViewModel;
        private readonly MonthAnalysisViewModel _monthAnalysisViewModel;
        private readonly TransactionsViewModel _transactionsViewModel;
        private readonly BankOffersViewModel _bankOffersViewModel;
        private readonly ArticlesAssociationsViewModel _articlesAssociationsViewModel;
        private readonly SettingsViewModel _settingsViewModel;
        private readonly CarsViewModel _carsViewModel;
        private readonly ExpenseByCategoriesViewModel _expenseByCategoriesViewModel;
        private readonly PayCardsViewModel _payCardsViewModel;

        public MainMenuViewModel(IWindowManager windowManager, KeeperDb keeperDb, ShellPartsBinder shellPartsBinder,
            TransactionsViewModel transactionsViewModel, CurrencyRatesViewModel currencyRatesViewModel,
            MonthAnalysisViewModel monthAnalysisViewModel, BankOffersViewModel bankOffersViewModel,
            ArticlesAssociationsViewModel articlesAssociationsViewModel, SettingsViewModel settingsViewModel,
            CarsViewModel carsViewModel, ExpenseByCategoriesViewModel expenseByCategoriesViewModel,
            PayCardsViewModel payCardsViewModel)
        {
            _windowManager = windowManager;
            _keeperDb = keeperDb;
            _shellPartsBinder = shellPartsBinder;
            _currencyRatesViewModel = currencyRatesViewModel;
            _monthAnalysisViewModel = monthAnalysisViewModel;
            _transactionsViewModel = transactionsViewModel;
            _bankOffersViewModel = bankOffersViewModel;
            _articlesAssociationsViewModel = articlesAssociationsViewModel;
            _settingsViewModel = settingsViewModel;
            _carsViewModel = carsViewModel;
            _expenseByCategoriesViewModel = expenseByCategoriesViewModel;
            _payCardsViewModel = payCardsViewModel;
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
            if (_transactionsViewModel.IsFirstLaunch)
                _transactionsViewModel.Initialize();
            else
                _transactionsViewModel.ReInitializeOnlyAccountLists();
            _windowManager.ShowDialog(_transactionsViewModel);

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
            vm.Initialize(_keeperDb);
            _windowManager.ShowWindow(vm);
        }

        public void ShowExpensesChart()
        {
            _expenseByCategoriesViewModel.Initialize();
            _windowManager.ShowDialog(_expenseByCategoriesViewModel);
        }

        public void ShowCarForm()
        {
            _carsViewModel.Initialize();
            _windowManager.ShowDialog(_carsViewModel);
        }

        public void ShowPayCardsForm()
        {
            _payCardsViewModel.Initialize();
            _windowManager.ShowDialog(_payCardsViewModel);
        }


        public async void Save()
        {
            try
            {
                if (_shellPartsBinder.IsBusy) return;
                _shellPartsBinder.IsBusy = true;
                _shellPartsBinder.FooterVisibility = Visibility.Visible;
                _keeperDb.FlattenAccountTree();
                await DbSerializer.Serialize(_keeperDb.Bin);

                if (await _keeperDb.SaveAllToNewTxtAsync())
                    if (await DbTxtSaver.ZipTxtDbAsync())
                        DbTxtSaver.DeleteTxtFiles();

                _transactionsViewModel.Model.IsCollectionChanged = false;
                _shellPartsBinder.FooterVisibility = Visibility.Collapsed;
                _shellPartsBinder.IsBusy = false;
            }
            catch (Exception e)
            {
                var vm = new MyMessageBoxViewModel(MessageType.Error, $"Exception during db saving: {e.Message}");
                _windowManager.ShowDialog(vm);
            }
        }

        public void ShowSettingsForm()
        {
            _windowManager.ShowDialog(_settingsViewModel);
        }
    }
}
