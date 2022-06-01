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

        public ObservableCollection<InvestmentAsset> Assets { get; set; }
        public InvestmentAsset SelectedAsset { get; set; }

        public List<AssetType> AssetTypes { get; set; }

        public InvestmentAssetsViewModel(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
            AssetTypes = Enum.GetValues(typeof(AssetType)).OfType<AssetType>().ToList();
        }

        public void Initialize()
        {
            Assets = new ObservableCollection<InvestmentAsset>(_dataModel.InvestmentAssets.OrderBy(l => l.Id));
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Инвестиционные активы";
        }

       
        public void DeleteSelected()
        {
            if (SelectedAsset != null)
                Assets.Remove(SelectedAsset);
        }

        public override void CanClose(Action<bool> callback)
        {
            _dataModel.InvestmentAssets = Assets.ToList();
            base.CanClose(callback);
        }
    }
}
