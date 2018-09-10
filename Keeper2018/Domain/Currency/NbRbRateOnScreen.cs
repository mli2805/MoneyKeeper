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
        private double _delta;

        public string BasketStr { get; set; }
        public Brush BasketBrush { get; set; }

        private double _deltaAfterBreak;
        public string BasketChangeAfterBreak { get; set; }
        public Brush BasketBreakBrush { get; set; }

        public NbRbRateOnScreen(NbRbRate record, NbRbRateOnScreen previous)
        {
            Date = record.Date;
            TodayValues = record.Values;
            YesterdayValues = previous?.TodayValues;

            _basket = BelBaskets.Calculate(TodayValues);
            _yesterdayBasket = previous != null ? BelBaskets.Calculate(YesterdayValues) : 0;

            if (Date < new DateTime(2016, 7, 1)) return;

            SetBasketStr(previous);
            SetBasketBreakStr(previous);
        }

        private void SetBasketStr(NbRbRateOnScreen previous)
        {
            _delta = _basket - _yesterdayBasket;
            var proc = _delta / _yesterdayBasket * 100;

            BasketStr = $"{_basket:0.0000} ({proc:+0.##;-0.##;0}%)";

            if (_delta.Equals(0.0))
                BasketBrush = previous.BasketBrush;
            else
            {
                BasketBrush = _delta > 0 ? Brushes.LimeGreen : Brushes.Red;
            }
        }

        private void SetBasketBreakStr(NbRbRateOnScreen previous)
        {
            if (_delta * previous._deltaAfterBreak >= 0) // no break
            {
                _deltaAfterBreak = _deltaAfterBreak + _delta;
                BasketChangeAfterBreak = $"{_deltaAfterBreak:+0.##;-0.##;0}%";
                BasketBreakBrush = _delta > 0 ? Brushes.LimeGreen : Brushes.Red;
            }
            else
            {
                _deltaAfterBreak = _delta;
                BasketChangeAfterBreak = "";
            }
        }
    }
}