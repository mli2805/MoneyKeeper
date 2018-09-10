using System;
using System.Windows.Media;

namespace Keeper2018
{
    public class NbRbRateOnScreen
    {
        public DateTime Date { get; set; }
        public MainCurrenciesRates TodayValues { get; set; }
        private MainCurrenciesRates YesterdayValues { get; set; }

        private double Basket { get; set; }
        private double YesterdayBasket { get; set; }
        public string BasketStr { get; set; }
        public Brush BasketBrush => Basket > YesterdayBasket ? Brushes.Red : Brushes.ForestGreen;

        public NbRbRateOnScreen(NbRbRate record, NbRbRate previous)
        {
            Date = record.Date;
            TodayValues = record.Values;
            YesterdayValues = previous?.Values;

            Basket = BelBaskets.Calculate(TodayValues);
            YesterdayBasket = previous != null ? BelBaskets.Calculate(YesterdayValues) : 0;

            BasketStr = GetBasketStr();
        }

        private string GetBasketStr()
        {
            if (Date < new DateTime(2016, 7, 1)) return "";

            var delta = YesterdayBasket - Basket;
            var proc = delta / Basket * 100;

            return $"{Basket:0.0000} ({proc:+0.##;-0.##;0}%)";
        }
    }
}