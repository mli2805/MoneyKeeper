using Caliburn.Micro;

namespace Keeper2018
{
    public class InputMyUsdViewModel : Screen
    {
        private readonly KeeperDb _db;
        public CurrencyRatesModel CurrentLine { get; set; }

        public double MyUsdRate { get; set; }
        public double EurUsd { get; set; }
        public double RubUsd { get; set; }

        public InputMyUsdViewModel(KeeperDb db)
        {
            _db = db;
        }

        public void Initialize(CurrencyRatesModel currentLine, CurrencyRates previousDate)
        {
            CurrentLine = currentLine;

            MyUsdRate = CurrentLine.TodayRates.MyUsdRate.Value > 0
                ? CurrentLine.TodayRates.MyUsdRate.Value
                : previousDate.MyUsdRate.Value;
            EurUsd = CurrentLine.TodayRates.MyEurUsdRate.Value > 0
                ? CurrentLine.TodayRates.MyEurUsdRate.Value
                : previousDate.MyEurUsdRate.Value;
            RubUsd = CurrentLine.TodayRates.CbrRate.Usd.Value > 0
                ? CurrentLine.TodayRates.CbrRate.Usd.Value
                : previousDate.MyUsdRate.Value;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Ввод курсов";
        }

        public void Save()
        {
            var rateLine = _db.Bin.Rates[CurrentLine.Date];
            rateLine.MyUsdRate.Value = MyUsdRate;
            rateLine.MyEurUsdRate.Value = EurUsd;
            rateLine.CbrRate.Usd.Value = RubUsd;

            CurrentLine.Input(MyUsdRate, EurUsd, RubUsd);
            TryClose();
        }

        public void Cancel()
        {
            TryClose();
        }
    }
}
