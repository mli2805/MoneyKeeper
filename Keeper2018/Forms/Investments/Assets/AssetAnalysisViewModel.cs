using System;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class AssetAnalysisViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;
        private TrustAssetModel _asset;

        private Period _activePeriod;
        private AssetOnPeriodReportModel _reportModel;

        public AssetOnPeriodReportModel ReportModel
        {
            get => _reportModel;
            set
            {
                if (Equals(value, _reportModel)) return;
                _reportModel = value;
                NotifyOfPropertyChange();
            }
        }

        public AssetAnalysisViewModel(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        public void Initialize(TrustAssetModel asset)
        {
            _asset = asset;
            _activePeriod = DateTime.Today.GetFullMonthForDate();
            ReportModel = _dataModel.Analyze(_asset, _activePeriod).CreateReport();
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _asset.Title + "  " + _activePeriod.ToStringD();
        }


        public void ShowPreviousMonth()
        {
            _activePeriod = _activePeriod.StartDate.AddMonths(-1).GetFullMonthForDate();
            ReportModel = _dataModel.Analyze(_asset, _activePeriod).CreateReport();
        }

        public void ShowNextMonth()
        {
            _activePeriod = _activePeriod.StartDate.AddMonths(1).GetFullMonthForDate();
            ReportModel = _dataModel.Analyze(_asset, _activePeriod).CreateReport();
        }

        public void ShowPreviousQuarter()
        {
            ReportModel = _dataModel.Analyze(_asset, _activePeriod).CreateReport();
        }

        public void ShowNextQuarter()
        {
            ReportModel = _dataModel.Analyze(_asset, _activePeriod).CreateReport();
        }
        public void ShowNextYear()
        {
            ReportModel = _dataModel.Analyze(_asset, _activePeriod).CreateReport();
        }

        public void ShowPreviousYear()
        {
            ReportModel = _dataModel.Analyze(_asset, _activePeriod).CreateReport();
        }

    }
}
