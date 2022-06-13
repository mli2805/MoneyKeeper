using System;
using System.Linq;
using Caliburn.Micro;

namespace Keeper2018
{
    public class InvestmentAnalysisViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;

        public InvestmentAnalysisViewModel(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        public void Initialize()
        {
            var period = new Period(new DateTime(2022, 5, 1),
                new DateTime(2022, 6, 1).AddMilliseconds(-1));
            foreach (var asset in _dataModel.InvestmentAssets.Skip(1))
            {
                 _dataModel.Analyze(asset, period);
            }


        }
    }
}
