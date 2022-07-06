using Caliburn.Micro;

namespace Keeper2018
{
    public class RatesViewModel : Screen
    {
        public ExchangeRatesViewModel ExchangeRatesViewModel { get; }


        public RatesViewModel(ExchangeRatesViewModel exchangeRatesViewModel)
        {
            ExchangeRatesViewModel = exchangeRatesViewModel;
        }

        public void Initialize()
        {
            ExchangeRatesViewModel.Initialize();
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
