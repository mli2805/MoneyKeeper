﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

        public override void CanClose(Action<bool> callback)
        {
            _dataModel.AssetRates = Rates.ToList();
            base.CanClose(callback);
        }
    }
}