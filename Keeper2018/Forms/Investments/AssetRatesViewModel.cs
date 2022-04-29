using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class AssetRatesViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;

        public ObservableCollection<AssetRate> Rates { get; set; }
        public AssetRate SelectedRate { get; set; }
        public List<InvestmentAsset> Assets { get; set; }

        public DateTime SelectedDate { get; set; } = DateTime.Today;

        public AssetRatesViewModel(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        public void Initialize()
        {
            Assets = _dataModel.InvestmentAssets;
            Rates = new ObservableCollection<AssetRate>(_dataModel.AssetRates);
            SelectedRate = Rates.Last();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Стоимость активов";
        }

        public void DeleteSelected()
        {
            if (SelectedRate != null)
                Rates.Remove(SelectedRate);
        }

        public void AddForDate()
        {
            var lastId = Rates.Max(r => r.Id);
            foreach (var asset in Assets.Where(a=>a.Ticker != "CASH"))
            {
                var prev = Rates.FirstOrDefault(r => r.TickerId == asset.Id);
                Rates.Add(new AssetRate()
                {
                    Id = ++lastId, 
                    TickerId = asset.Id, 
                    Currency = prev?.Currency ?? CurrencyCode.RUB,
                    Unit = prev?.Unit ?? 1,
                    Value = 0,
                    Date = SelectedDate,
                });
            }
        }

        public override void CanClose(Action<bool> callback)
        {
            _dataModel.AssetRates = Rates.ToList();
            base.CanClose(callback);
        }
    }
}
