using Caliburn.Micro;
using KeeperDomain;

namespace Keeper2018
{
    public class InputMyUsdViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;
        public CurrencyRatesModel CurrentLine { get; set; }

        public double MyUsdRate { get; set; }

        public InputMyUsdViewModel(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        public void Initialize(CurrencyRatesModel currentLine, CurrencyRates previousDate)
        {
            CurrentLine = currentLine;

            MyUsdRate = CurrentLine.TodayRates.MyUsdRate.Value > 0
                ? CurrentLine.TodayRates.MyUsdRate.Value
                : previousDate.MyUsdRate.Value;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Ввод курсов";
        }

        public void Save()
        {
            var rateLine = _dataModel.Rates[CurrentLine.Date];
            rateLine.MyUsdRate.Value = MyUsdRate;

            CurrentLine.Input(MyUsdRate);
            TryClose();
        }

        public void Cancel()
        {
            TryClose();
        }
    }
}
