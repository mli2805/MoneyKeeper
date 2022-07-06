using System.Collections.ObjectModel;
using Caliburn.Micro;
using KeeperDomain.Exchange;

namespace Keeper2018
{
    public class ExchangeRatesViewModel : PropertyChangedBase
    {
        private readonly KeeperDataModel _keeperDataModel;
        public ObservableCollection<ExchangeRates> Rows { get; set; }

        public ExchangeRatesViewModel(KeeperDataModel keeperDataModel)
        {
            _keeperDataModel = keeperDataModel;
        }

        public void Initialize()
        {
            Rows = new ObservableCollection<ExchangeRates>(_keeperDataModel.ExchangeRates.Values);
        }

        public async void Update()
        {
            // var newRates = await ExchangeRatesFetcher.Get(5);

            var bnb = ExchangeRatesSelector.GetAllBnb();
            var newRates = ExchangeRatesSelector.SelectMiddayRates(bnb);
            

            foreach (var newRate in newRates)
            {
                Rows.Add(newRate);
                _keeperDataModel.ExchangeRates.Add(newRate.Date, newRate);
            }
        }
    }
}
