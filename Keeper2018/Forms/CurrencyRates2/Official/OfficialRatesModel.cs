using System;
using System.Globalization;
using System.Windows.Media;
using Caliburn.Micro;
using KeeperDomain;
using KeeperDomain.Basket;

namespace Keeper2018
{
    public class StringWithBrush
    {
        public string Str { get; set; }
        public Brush Brush { get; set; }

        public StringWithBrush(string str, Brush brush)
        {
            Str = str;
            Brush = brush;
        }
    }
    public class OfficialRatesModel : PropertyChangedBase
    {
        public int Id;
        public DateTime Date { get; set; }
        public OfficialRates TodayRates { get; set; }
        private NbRbRates YesterdayNbRbRates { get; set; }

        public readonly double Basket;
        private readonly double _yesterdayBasket;
        public double BasketDelta { get; set; }
        public double ProcBasketDelta;


        public string UsdStr { get; set; }
        public Brush UsdBrush { get; set; }

        public StringWithBrush UsdToEndOfYear { get; set; }
        public StringWithBrush UsdY2Y { get; set; }

        public string EuroBynStr { get; set; }

        private string _euroUsdStr;
        public string EuroUsdStr
        {
            get => _euroUsdStr;
            set
            {
                if (value == _euroUsdStr) return;
                _euroUsdStr = value;
                NotifyOfPropertyChange();
            }
        }

        public string RurUnitStr { get; set; }
        public string RurStr { get; set; }

        private string _rurUsdStr;
        public string RurUsdStr
        {
            get => _rurUsdStr;
            set
            {
                if (value == _rurUsdStr) return;
                _rurUsdStr = value;
                NotifyOfPropertyChange();
            }
        }

        public string CnyUnitStr { get; set; }
        public string CnyStr { get; set; }

        public string BasketNumberStr { get; set; }
        public string BasketStr { get; set; }
        public Brush BasketBrush { get; set; }

        // private double _procBasketBreak;
        // public string BasketAfterBreakStr { get; set; }
        // public Brush BasketBreakBrush { get; set; }

        public StringWithBrush BasketToEndOfYear { get; set; } // To last day of previous year
        public StringWithBrush BasketY2Y { get; set; } // To this day year ago


        private string Template => Date >= new DateTime(2016, 7, 1) ? "#,#.0000" : "#,#.####";

        public OfficialRatesModel(OfficialRates record,
            OfficialRatesModel previous, OfficialRatesModel endOfLastYear, OfficialRatesModel yearAgo)
        {
            Id = record.Id;
            Date = record.Date;
            TodayRates = record;
            YesterdayNbRbRates = previous?.TodayRates.NbRates;

            UsdStr = TodayRates.NbRates.Usd.Value.ToString(Template, new CultureInfo("ru-RU"));
            EuroBynStr = TodayRates.NbRates.Euro.Value.ToString(Template, new CultureInfo("ru-RU"));
            EuroUsdStr = TodayRates.NbRates.EuroUsdCross.ToString("0.####", new CultureInfo("ru-RU"));
            RurStr = TodayRates.NbRates.Rur.Value.ToString(Template, new CultureInfo("ru-RU"));
            RurUnitStr = RurStr == "" ? "" : TodayRates.NbRates.Rur.Unit.ToString();
            CnyStr = TodayRates.NbRates.Cny.Value.ToString(Template, new CultureInfo("ru-RU"));
            CnyUnitStr = CnyStr == "" ? "" : TodayRates.NbRates.Cny.Unit.ToString();

            UsdBrush = YesterdayNbRbRates == null || YesterdayNbRbRates.Usd.Value.Equals(TodayRates.NbRates.Usd.Value)
                ? Brushes.Black
                : YesterdayNbRbRates.Usd.Value > TodayRates.NbRates.Usd.Value
                    ? Brushes.LimeGreen : Brushes.Red;

            Basket = BelBaskets.CalculateByLastRules(record, out int basketRulesNumber);
            BasketNumberStr = basketRulesNumber.ToString();

            _yesterdayBasket = previous == null
                ? 0
                : previous.Date.Year == 1999 && record.Date.Year == 2000
                    ? previous.Basket / 1000
                    : previous.Basket;

            SetBasketStr();
            // SetBasketBreakStr(previous);

            if (endOfLastYear != null)
            {
                UsdToEndOfYear = SetUsdChanges(endOfLastYear.TodayRates.NbRates.Usd.Value);
                BasketToEndOfYear = SetBasketChanges(endOfLastYear.Basket);
            }

            if (yearAgo != null)
            {
                UsdY2Y = SetUsdChanges(yearAgo.TodayRates.NbRates.Usd.Value);
                BasketY2Y = SetBasketChanges(yearAgo.Basket);
            }

            RurUsdStr = TodayRates.CbrRate.Usd.Value.Equals(0)
                ? "" : TodayRates.CbrRate.Usd.Value.ToString("#,#.##", new CultureInfo("ru-RU"));
        }

        private void SetBasketStr()
        {
            BasketDelta = Basket - _yesterdayBasket;
            ProcBasketDelta = _yesterdayBasket.Equals(0.0) ? 0 : BasketDelta / _yesterdayBasket * 100;

            BasketStr = Date < new DateTime(1999, 01, 11) // Euro appeared
                ? ""
                : $"{Basket.ToString("#,0.####", new CultureInfo("ru-RU"))} ({ProcBasketDelta:+0.##;-0.##;0}%)";

            BasketBrush = BasketDelta.Equals(0.0)
                ? Brushes.Gray
                : BasketDelta < 0
                    ? Brushes.LimeGreen : Brushes.Red;
        }

        // private void SetBasketBreakStr(OfficialRatesModel previous)
        // {
        //     if (previous == null) return;
        //     if (BasketDelta * previous.BasketDelta >= 0) // no break
        //     {
        //         _procBasketBreak = previous._procBasketBreak + ProcBasketDelta;
        //         BasketAfterBreakStr = $"{_procBasketBreak:+0.##;-0.##;0}%";
        //         BasketBreakBrush = _procBasketBreak < 0 ? Brushes.LimeGreen : Brushes.Red;
        //     }
        //     else
        //     {
        //         _procBasketBreak = ProcBasketDelta;
        //         BasketAfterBreakStr = "";
        //     }
        // }

        private StringWithBrush SetUsdChanges(double usdRateToCompare)
        {
            var denominatedRate = Date.Year == 2000
                ? usdRateToCompare / 1000
                : Date > new DateTime(2016, 06, 30) && Date < new DateTime(2017, 1, 1)
                    ? usdRateToCompare / 10000
                    : usdRateToCompare;

            var delta = TodayRates.NbRates.Usd.Value - denominatedRate;
            var proc = delta / denominatedRate * 100;
            return new StringWithBrush(
                $"{delta.ToString("#,0.####", new CultureInfo("ru-RU"))} ({proc:+0.##;-0.##;0}%)",
                    proc < 0 ? Brushes.LimeGreen : Brushes.Red);
        }

        private StringWithBrush SetBasketChanges(double basketToCompare)
        {
            if (basketToCompare.Equals(0)) return new StringWithBrush(string.Empty, Brushes.Transparent);

            var denominatedBasket = Date.Year == 2000
                ? basketToCompare / 1000
                : Date > new DateTime(2016, 06, 30) && Date < new DateTime(2017, 1, 1)
                    ? basketToCompare / 10000
                    : basketToCompare;

            var delta = Basket - denominatedBasket;
            var percent = delta / denominatedBasket * 100;
            return new StringWithBrush(
                $"{percent:+0.##;-0.##;0}%",
                percent < 0 ? Brushes.LimeGreen : percent.Equals(0) ? Brushes.Gray : Brushes.Red);
        }

        public string BasketStrWithoutProc
        {
            get
            {
                var indexOf = BasketStr.IndexOf(' ');
                return BasketStr.Substring(0, indexOf);
            }
        }

        #region ' _isSelected '
        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value.Equals(_isSelected)) return;
                _isSelected = value;
                NotifyOfPropertyChange(() => IsSelected);
            }
        }
        #endregion
    }
}