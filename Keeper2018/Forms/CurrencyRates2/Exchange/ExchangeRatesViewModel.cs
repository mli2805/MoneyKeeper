using System;
using System.Collections.ObjectModel;
using System.Linq;
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
            var last = Rows.Last();
            _keeperDataModel.ExchangeRates.Remove(last.Date);
            Rows.Remove(last);

            var days = (DateTime.Now - Rows.Last().Date).Days;
            if (days == 0) return;

            var newRates = await ExchangeRatesFetcher.Get("Alfa", days);
            var middayRates =
                ExchangeRatesFetcher.SelectMiddayRates(newRates.OrderBy(l => l.Date).ToList(), Rows.Last().Date.AddDays(1));

            var lastId = Rows.Last().Id;

            foreach (var newRate in middayRates)
            {
                if (!_keeperDataModel.ExchangeRates.ContainsKey(newRate.Date))
                {
                    newRate.Id = ++lastId;
                    Rows.Add(newRate);
                    _keeperDataModel.ExchangeRates.Add(newRate.Date, newRate);
                }
            }
        }

    }
}
