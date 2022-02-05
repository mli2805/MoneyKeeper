using System.Collections.Generic;
using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class TickerRatesViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;

        public List<TickerRate> Rates { get; set; }

        public TickerRatesViewModel(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        public void Initialize()
        {
            Rates = _dataModel.TickerRates;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Курсы акций";
        }
    }
}
