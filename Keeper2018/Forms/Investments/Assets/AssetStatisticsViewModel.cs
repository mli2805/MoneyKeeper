using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;

namespace Keeper2018
{
    public class AssetStatisticsViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;
        private InvestmentAssetModel _investmentAssetModel;
        public StatisticsLinesViewModel StatisticsLinesViewModel { get; set; } = new StatisticsLinesViewModel();

        public AssetStatisticsViewModel(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Статистика по активу";
        }

        public void Initialize(InvestmentAssetModel assetModel)
        {
            _investmentAssetModel = assetModel;

            StatisticsLinesViewModel.Rows = new List<TrustStatisticsLine>();
            decimal balanceBefore = 0;

            foreach (var tr in _dataModel.InvestTranModels.Where(t => t.Asset.Id == _investmentAssetModel.Id))
            {
                var line = tr.Create(balanceBefore, false);
                StatisticsLinesViewModel.Rows.Add(line);
                balanceBefore = line.BalanceAfter;
            }
        }
    }
}
