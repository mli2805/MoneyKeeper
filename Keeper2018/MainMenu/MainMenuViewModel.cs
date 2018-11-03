using Caliburn.Micro;

namespace Keeper2018
{
    public class MainMenuViewModel : PropertyChangedBase
    {
        private readonly IWindowManager _windowManager;
        private readonly OfficialRatesViewModel _officialRatesViewModel;
        private readonly TransactionsViewModel _transactionsViewModel;

        public MainMenuViewModel(IWindowManager windowManager,
            OfficialRatesViewModel officialRatesViewModel, TransactionsViewModel transactionsViewModel)
        {
            _windowManager = windowManager;
            _officialRatesViewModel = officialRatesViewModel;
            _transactionsViewModel = transactionsViewModel;
        }

        public void ActionMethod(MainMenuAction action)
        {
            if (action == MainMenuAction.ShowOfficialRatesForm)
                ShowOfficialRatesForm();
            if (action == MainMenuAction.ShowTransactionsForm)
                ShowTransactionsForm();
        }

        public void ShowOfficialRatesForm()
        {
            _windowManager.ShowDialog(_officialRatesViewModel);
        }
        public void ShowTransactionsForm()
        {
            _windowManager.ShowDialog(_transactionsViewModel);
        }
    }
}
