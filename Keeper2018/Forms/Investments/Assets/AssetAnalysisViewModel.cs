using System;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class AssetAnalysisViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;
        private InvestmentAsset _asset;

        public InvestmentAssetOnPeriod Model { get; set; }

        public AssetAnalysisViewModel(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        public void Initialize(InvestmentAsset asset)
        {
            _asset = asset;
            var period = new DateTime(2022, 5, 1).GetFullMonthForDate();
            Model = _dataModel.Analyze(asset, period);
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _asset.Title;
        }
    }
}
