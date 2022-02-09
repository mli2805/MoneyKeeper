using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class InvestmentAssetsViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;

        public ObservableCollection<InvestmentAsset> Tickers { get; set; }
        public InvestmentAsset SelectedTicker { get; set; }

        public List<AssetType> SecuritiesTypes { get; set; }

        public InvestmentAssetsViewModel(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
            SecuritiesTypes = Enum.GetValues(typeof(AssetType)).OfType<AssetType>().ToList();
        }

        public void Initialize()
        {
            Tickers = new ObservableCollection<InvestmentAsset>();
            foreach (var stockTicker in _dataModel.TrustTickers.OrderBy(l=>l.Id))
            {
                Tickers.Add(stockTicker);
            }
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Инвестиционные активы";
        }

       
        public void DeleteSelected()
        {
            if (SelectedTicker != null)
                Tickers.Remove(SelectedTicker);
        }

        public override void CanClose(Action<bool> callback)
        {
            _dataModel.TrustTickers = Tickers.ToList();
            base.CanClose(callback);
        }
    }
}
