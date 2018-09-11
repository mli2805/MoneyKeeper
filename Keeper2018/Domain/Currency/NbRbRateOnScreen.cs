using System;
using System.Windows.Media;

namespace Keeper2018
{
    public class NbRbRateOnScreen
    {
        public DateTime Date { get; set; }
        public MainCurrenciesRates TodayValues { get; set; }
        private MainCurrenciesRates YesterdayValues { get; set; }

        private readonly double _basket;
        private readonly double _yesterdayBasket;
        public double Delta { get; set; }
        private double _proc, _procFromBreak, _procAnnual;

        public string BasketStr { get; set; }
        public Brush BasketBrush { get; set; }

        public string BasketChangeAfterBreak { get; set; }
        public Brush BasketBreakBrush { get; set; }
        public string BasketAnnual { get; set; }
        public Brush BasketAnnualBrush { get; set; }

        public NbRbRateOnScreen(NbRbRate record, NbRbRateOnScreen previous, NbRbRateOnScreen annual)
        {
            Date = record.Date;
            TodayValues = record.Values;
            YesterdayValues = previous?.TodayValues;

            _basket = BelBaskets.Calculate(TodayValues);
            _yesterdayBasket = previous != null ? BelBaskets.Calculate(YesterdayValues) : 0;

            if (Date < new DateTime(2016, 7, 1)) return;

            SetBasketStr(previous);
            SetBasketBreakStr(previous);
            SetBasketAnnualStr(annual);
        }

        private void SetBasketStr(NbRbRateOnScreen previous)
        {
            Delta = _basket - _yesterdayBasket;
            _proc = _yesterdayBasket.Equals(0.0) ? 0 : Delta / _yesterdayBasket * 100;

            BasketStr = $"{_basket:0.0000} ({_proc:+0.##;-0.##;0}%)";

            BasketBrush = Delta.Equals(0.0)
                ? Brushes.Gray
                : Delta < 0
                    ? Brushes.LimeGreen : Brushes.Red;
        }

        private void SetBasketBreakStr(NbRbRateOnScreen previous)
        {
            if (previous == null) return;
            if (Delta * previous.Delta >= 0) // no break
            {
                _procFromBreak = previous._procFromBreak + _proc;
                BasketChangeAfterBreak = $"{_procFromBreak:+0.##;-0.##;0}%";
                BasketBreakBrush = _procFromBreak < 0 ? Brushes.LimeGreen : Brushes.Red;
            }
            else
            {
                _procFromBreak = _proc;
                BasketChangeAfterBreak = "";
            }
        }

        private void SetBasketAnnualStr(NbRbRateOnScreen annual)
        {
            if (annual == null) return;

            var delta = _basket - annual._basket;
            _procAnnual = delta / annual._basket * 100;
            BasketAnnual = $"{_procAnnual:+0.##;-0.##;0}%";
            BasketAnnualBrush = _procAnnual < 0 ? Brushes.LimeGreen : Brushes.Red;
        }
    }
}