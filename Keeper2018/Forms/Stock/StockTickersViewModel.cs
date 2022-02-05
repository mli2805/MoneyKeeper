using System.Collections.Generic;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class StockTickersViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;

        public List<StockTiсker> Tickers { get; set; }

        public StockTickersViewModel(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        public void Initialize()
        {
            Tickers = _dataModel.StockTickers;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Инвестиционные инструменты";
        }


    }
}
