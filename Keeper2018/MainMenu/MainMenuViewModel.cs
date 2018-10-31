using Caliburn.Micro;

namespace Keeper2018
{
    public class MainMenuViewModel : PropertyChangedBase
    {
        private readonly IWindowManager _windowManager;
        private readonly OfficialRatesViewModel _officialRatesViewModel;

        public MainMenuViewModel(IWindowManager windowManager,
            OfficialRatesViewModel officialRatesViewModel)
        {
            _windowManager = windowManager;
            _officialRatesViewModel = officialRatesViewModel;
        }

        public void ShowOfficialRates()
        {
            _windowManager.ShowDialog(_officialRatesViewModel);
        }
    }
}
