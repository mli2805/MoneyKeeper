using System.Threading.Tasks;
using Caliburn.Micro;

namespace Keeper2018
{
    public class RatesViewModel : Screen
    {
        public OfficialRatesViewModel OfficialRatesViewModel { get; }
        public ExchangeRatesViewModel ExchangeRatesViewModel { get; }
        public GoldRatesViewModel GoldRatesViewModel { get; }


        public RatesViewModel(OfficialRatesViewModel officialRatesViewModel,
            ExchangeRatesViewModel exchangeRatesViewModel, GoldRatesViewModel goldRatesViewModel)
        {
            OfficialRatesViewModel = officialRatesViewModel;
            ExchangeRatesViewModel = exchangeRatesViewModel;
            GoldRatesViewModel = goldRatesViewModel;
        }

        public async Task Initialize()
        {
            await OfficialRatesViewModel.Initialize();
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
