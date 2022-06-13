using System;
using Caliburn.Micro;

namespace Keeper2018
{
    public class AssetAnalysisViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;
        private InvestmentAssetModel _asset;

        public AssetOnPeriodData AssetOnPeriodData { get; set; }
        public AssetOnPeriodReportModel ReportModel { get; set; }

        public AssetAnalysisViewModel(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        public void Initialize(InvestmentAssetModel asset)
        {
            _asset = asset;
            var period = new DateTime(2022, 5, 1).GetFullMonthForDate();
            AssetOnPeriodData = _dataModel.Analyze(asset, period);
            ReportModel = AssetOnPeriodData.CreateReport();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _asset.Title + "  " + AssetOnPeriodData.Period.ToStringD();
        }
    }
}
