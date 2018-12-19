using Caliburn.Micro;

namespace Keeper2018
{
    public class MainMenuViewModel : PropertyChangedBase
    {
        private readonly IWindowManager _windowManager;
        private readonly KeeperDb _keeperDb;
        private readonly OfficialRatesViewModel _officialRatesViewModel;
        private readonly MonthAnalysisViewModel _monthAnalysisViewModel;
        private readonly TransactionsViewModel _transactionsViewModel;
        private readonly BankOffersViewModel _bankOffersViewModel;
        private readonly ArticlesAssociationsViewModel _articlesAssociationsViewModel;
        private readonly SettingsViewModel _settingsViewModel;

        public MainMenuViewModel(IWindowManager windowManager, KeeperDb keeperDb,
            TransactionsViewModel transactionsViewModel, OfficialRatesViewModel officialRatesViewModel,
            MonthAnalysisViewModel monthAnalysisViewModel, BankOffersViewModel bankOffersViewModel, 
            ArticlesAssociationsViewModel articlesAssociationsViewModel, SettingsViewModel settingsViewModel)
        {
            _windowManager = windowManager;
            _keeperDb = keeperDb;
            _officialRatesViewModel = officialRatesViewModel;
            _monthAnalysisViewModel = monthAnalysisViewModel;
            _transactionsViewModel = transactionsViewModel;
            _bankOffersViewModel = bankOffersViewModel;
            _articlesAssociationsViewModel = articlesAssociationsViewModel;
            _settingsViewModel = settingsViewModel;
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
            }
        }

        public void ShowTransactionsForm()
        {
            _transactionsViewModel.Init();
            _windowManager.ShowDialog(_transactionsViewModel);
        }

        public void ShowOfficialRatesForm()
        {
            _windowManager.ShowDialog(_officialRatesViewModel);
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
        public async void Save()
        {
            _keeperDb.FlattenAccountTree();
            await DbSerializer.Serialize(_keeperDb.Bin);
        }
        public void ShowSettingsForm()
        {
            _windowManager.ShowDialog(_settingsViewModel);
        }
    }
}
