using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class TrustTickersViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;

        public ObservableCollection<TrustTiсker> Tickers { get; set; }
        public TrustTiсker SelectedTicker { get; set; }

        public List<SecuritiesType> SecuritiesTypes { get; set; }

        public TrustTickersViewModel(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
            SecuritiesTypes = Enum.GetValues(typeof(SecuritiesType)).OfType<SecuritiesType>().ToList();
        }

        public void Initialize()
        {
            Tickers = new ObservableCollection<TrustTiсker>();
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
