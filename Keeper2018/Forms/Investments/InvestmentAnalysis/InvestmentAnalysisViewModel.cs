using System;
using Caliburn.Micro;

namespace Keeper2018
{
    public class InvestmentAnalysisViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;
        private readonly InvestmentAnalysis _investmentAnalysis;

        public InvestmentAnalysisViewModel(KeeperDataModel dataModel, InvestmentAnalysis investmentAnalysis)
        {
            _dataModel = dataModel;
            _investmentAnalysis = investmentAnalysis;
        }

        public void Initialize()
        {
            var period = new Period(new DateTime(2022, 5, 1),
                new DateTime(2022, 6, 1).AddMilliseconds(-1));
            foreach (var asset in _dataModel.InvestmentAssets)
            {
                 _investmentAnalysis.Analyze(asset, period);
            }


        }
    }
}
