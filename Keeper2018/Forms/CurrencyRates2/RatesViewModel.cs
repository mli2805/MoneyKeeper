using Caliburn.Micro;

namespace Keeper2018
{
    public class RatesViewModel : Screen
    {
        public ExchangeRatesViewModel ExchangeRatesViewModel { get; }
        public GoldRatesViewModel GoldRatesViewModel { get; }


        public RatesViewModel(ExchangeRatesViewModel exchangeRatesViewModel, GoldRatesViewModel goldRatesViewModel)
        {
            ExchangeRatesViewModel = exchangeRatesViewModel;
            GoldRatesViewModel = goldRatesViewModel;
        }

        public void Initialize()
        {
            ExchangeRatesViewModel.Initialize();
            GoldRatesViewModel.Initialize();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Курсы валют";
        }

        public void Close()
        {
            TryClose();
        }
    }
}
