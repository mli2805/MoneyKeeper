using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class TickerRatesViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;

        public ObservableCollection<TickerRate> Rates { get; set; }
        public TickerRate SelectedRate { get; set; }
        public List<TrustTiсker> Tickers { get; set; }

        public TickerRatesViewModel(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        public void Initialize()
        {
            Tickers = _dataModel.TrustTickers;
            Rates = new ObservableCollection<TickerRate>();
            foreach (var rate in _dataModel.TickerRates)
            {
                Rates.Add(rate);
            }
            SelectedRate = Rates.Last();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Курсы акций";
        }

        public void DeleteSelected()
        {
            if (SelectedRate != null)
                Rates.Remove(SelectedRate);
        }

        public override void CanClose(Action<bool> callback)
        {
            _dataModel.TickerRates = Rates.ToList();
            base.CanClose(callback);
        }
    }
}
