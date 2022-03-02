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
        }
    }
}
