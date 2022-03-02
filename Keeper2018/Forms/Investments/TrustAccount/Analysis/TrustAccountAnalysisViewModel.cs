using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class TrustAccountAnalysisViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;

        public List<InvestmentAssetEvaluation> Rows { get; set; } = new List<InvestmentAssetEvaluation>();
        public TrustAccountAnalysisViewModel(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        public void Initialize(TrustAccount trustAccount)
        {
            var bal = _dataModel.GetTrustAccountBalance(trustAccount, DateTime.Today);
            foreach (var asset in bal.Assets)
            {
                asset.CurrentPrice = asset.Quantity * _dataModel.AssetRates.Last(r => r.TickerId == asset.InvestmentAssetId).Value;
            }

            Rows.Clear();
            Rows.AddRange(bal.Assets);
        }
    }
}
