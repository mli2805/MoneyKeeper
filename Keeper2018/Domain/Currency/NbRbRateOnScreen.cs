using System;
using System.Globalization;
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
        public double BasketDelta { get; set; }
        private double _procBasket, _procBasketBreak, _procBasketAnnual;

        public string UsdStr { get; set; }
        public Brush UsdBrush { get; set; }
        public string EuroStr { get; set; }
        public string RurStr { get; set; }

        public string BasketStr { get; set; }
        public Brush BasketBrush { get; set; }

        public string BasketAfterBreakStr { get; set; }
        public Brush BasketBreakBrush { get; set; }
        public string BasketAnnualStr { get; set; }
        public Brush BasketAnnualBrush { get; set; }

        public string UsdAnnualStr { get; set; }
        public Brush UsdAnnualBrush { get; set; }

        public NbRbRateOnScreen(NbRbRate record, NbRbRateOnScreen previous, NbRbRateOnScreen annual)
        {
            Date = record.Date;
            TodayValues = record.Values;
            YesterdayValues = previous?.TodayValues;

            UsdStr = TodayValues.Usd.Value.ToString("#,#.####", new CultureInfo("ru-RU"));
            EuroStr = TodayValues.Euro.Value.ToString("#,#.####", new CultureInfo("ru-RU"));
            RurStr = TodayValues.Rur.Value.ToString("#,#.####", new CultureInfo("ru-RU"));
            UsdBrush = YesterdayValues == null || YesterdayValues.Usd.Value.Equals(TodayValues.Usd.Value)
                ? Brushes.Black 
                : YesterdayValues.Usd.Value > TodayValues.Usd.Value 
                    ? Brushes.LimeGreen : Brushes.Red;

            _basket = BelBaskets.Calculate(record);

            _yesterdayBasket = previous == null 
                ? 0 
                : previous.Date.Year == 1999 && record.Date.Year == 2000 
                    ? previous._basket / 1000
                    : previous._basket;

            SetBasketStr();
            SetBasketBreakStr(previous);
            SetUsdAnnualStr(annual);
            SetBasketAnnualStr(annual);
        }

        private void SetBasketStr()
        {
            BasketDelta = _basket - _yesterdayBasket;
            _procBasket = _yesterdayBasket.Equals(0.0) ? 0 : BasketDelta / _yesterdayBasket * 100;

            BasketStr = Date < new DateTime(1999, 01, 11) // Euro appeared
                ? ""
                : $"{_basket.ToString("#,0.####", new CultureInfo("ru-RU"))} ({_procBasket:+0.##;-0.##;0}%)";

            BasketBrush = BasketDelta.Equals(0.0)
                ? Brushes.Gray
                : BasketDelta < 0
                    ? Brushes.LimeGreen : Brushes.Red;
        }

        private void SetBasketBreakStr(NbRbRateOnScreen previous)
        {
            if (previous == null) return;
            if (BasketDelta * previous.BasketDelta >= 0) // no break
            {
                _procBasketBreak = previous._procBasketBreak + _procBasket;
                BasketAfterBreakStr = $"{_procBasketBreak:+0.##;-0.##;0}%";
                BasketBreakBrush = _procBasketBreak < 0 ? Brushes.LimeGreen : Brushes.Red;
            }
            else
            {
                _procBasketBreak = _procBasket;
                BasketAfterBreakStr = "";
            }
        }

        private void SetUsdAnnualStr(NbRbRateOnScreen annual)
        {
            if (annual == null) return;

            var lastYear = Date.Year == 2000 
                ? annual.TodayValues.Usd.Value / 1000 
                : Date > new DateTime(2016, 06, 30) && Date < new DateTime(2017, 1, 1) 
                    ? annual.TodayValues.Usd.Value / 10000 
                    : annual.TodayValues.Usd.Value;

            var delta = TodayValues.Usd.Value - lastYear;
            var proc = delta / lastYear * 100;
            UsdAnnualStr = $"{delta.ToString("#,0.####", new CultureInfo("ru-RU"))} ({proc:+0.##;-0.##;0}%)";
            UsdAnnualBrush = proc < 0 ? Brushes.LimeGreen : Brushes.Red;
        }  
        
        private void SetBasketAnnualStr(NbRbRateOnScreen annual)
        {
            if (annual == null || annual._basket.Equals(0)) return;

            var lastYear = Date.Year == 2000 
                ? annual._basket / 1000 
                : Date > new DateTime(2016, 06, 30) && Date < new DateTime(2017, 1, 1) 
                    ? annual._basket / 10000 
                    : annual._basket;

            var delta = _basket - lastYear;
            _procBasketAnnual = delta / lastYear * 100;
            BasketAnnualStr = $"{_procBasketAnnual:+0.##;-0.##;0}%";
            BasketAnnualBrush = _procBasketAnnual < 0 ? Brushes.LimeGreen : Brushes.Red;
        }
    }
}