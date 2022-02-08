using System;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class StockTickersViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;

        public ObservableCollection<StockTiсker> Tickers { get; set; }
        public StockTiсker SelectedTicker { get; set; }

        public StockTickersViewModel(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        public void Initialize()
        {
            Tickers = new ObservableCollection<StockTiсker>();
            foreach (var stockTicker in _dataModel.StockTickers.OrderBy(l=>l.Id))
            {
                Tickers.Add(stockTicker);
            }
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Инвестиционные инструменты";
        }

       
        public void DeleteSelected()
        {
            if (SelectedTicker != null)
                Tickers.Remove(SelectedTicker);
        }

        public override void CanClose(Action<bool> callback)
        {
            _dataModel.StockTickers = Tickers.ToList();
            base.CanClose(callback);
        }
    }
}
