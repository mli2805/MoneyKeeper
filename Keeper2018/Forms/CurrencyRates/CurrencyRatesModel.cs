using System;
using System.Globalization;
using System.Windows.Media;
using Caliburn.Micro;
using KeeperDomain;
using KeeperDomain.Basket;

namespace Keeper2018
{
    public class CurrencyRatesModel : PropertyChangedBase
    {
        public int Id;
        public DateTime Date { get; set; }
        public CurrencyRates TodayRates { get; set; }
        private NbRbRates YesterdayNbRbRates { get; set; }

        public readonly double Basket;
        private readonly double _yesterdayBasket;
        public double BasketDelta { get; set; }
        public double ProcBasketDelta;
        private double _procBasketBreak, _procBasketAnnual;

        public string UsdStr { get; set; }

        private string _myUsdStr;
        public string MyUsdStr
        {
            get => _myUsdStr;
            set
            {
                if (value == _myUsdStr) return;
                _myUsdStr = value;
                NotifyOfPropertyChange();
            }
        }

        public Brush UsdBrush { get; set; }
        public string EuroBynStr { get; set; }

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

        public string RurStr { get; set; }

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

        public string BasketStr { get; set; }

        public Brush BasketBrush { get; set; }

        public string BasketAfterBreakStr { get; set; }
        public Brush BasketBreakBrush { get; set; }
        public string BasketAnnualStr { get; set; }
        public Brush BasketAnnualBrush { get; set; }

        public string UsdAnnualStr { get; set; }
        public Brush UsdAnnualBrush { get; set; }

        private string Template => Date >= new DateTime(2016, 7, 1) ? "#,#.0000" : "#,#.####";

        public CurrencyRatesModel(CurrencyRates record, CurrencyRatesModel previous, CurrencyRatesModel annual)
        {
            Id = record.Id;
            Date = record.Date;
            TodayRates = record;
            YesterdayNbRbRates = previous?.TodayRates.NbRates;

            UsdStr = TodayRates.NbRates.Usd.Value.ToString(Template, new CultureInfo("ru-RU"));
            MyUsdStr = TodayRates.MyUsdRate.Value.Equals(0) ? "" : TodayRates.MyUsdRate.Value.ToString(Template, new CultureInfo("ru-RU"));
            EuroBynStr = TodayRates.NbRates.Euro.Value.ToString(Template, new CultureInfo("ru-RU"));
            EuroUsdStr = TodayRates.NbRates.EuroUsdCross.ToString("0.###", new CultureInfo("ru-RU"));
            RurStr = TodayRates.NbRates.Rur.Value.ToString(Template, new CultureInfo("ru-RU"));

            UsdBrush = YesterdayNbRbRates == null || YesterdayNbRbRates.Usd.Value.Equals(TodayRates.NbRates.Usd.Value)
                ? Brushes.Black
                : YesterdayNbRbRates.Usd.Value > TodayRates.NbRates.Usd.Value
                    ? Brushes.LimeGreen : Brushes.Red;

            Basket = BelBaskets.Calculate(record);

            _yesterdayBasket = previous == null
                ? 0
                : previous.Date.Year == 1999 && record.Date.Year == 2000
                    ? previous.Basket / 1000
                    : previous.Basket;

            SetBasketStr();
            SetBasketBreakStr(previous);
            SetUsdAnnualStr(annual);
            SetBasketAnnualStr(annual);
            RurUsdStr = TodayRates.CbrRate.Usd.Value.Equals(0) ? "" : TodayRates.CbrRate.Usd.Value.ToString("#,#.##", new CultureInfo("ru-RU"));
        }

        public void Input(double myusd)
        {
            MyUsdStr = myusd.ToString(Template, new CultureInfo("ru-RU"));
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

        private void SetBasketBreakStr(CurrencyRatesModel previous)
        {
            if (previous == null) return;
            if (BasketDelta * previous.BasketDelta >= 0) // no break
            {
                _procBasketBreak = previous._procBasketBreak + ProcBasketDelta;
                BasketAfterBreakStr = $"{_procBasketBreak:+0.##;-0.##;0}%";
                BasketBreakBrush = _procBasketBreak < 0 ? Brushes.LimeGreen : Brushes.Red;
            }
            else
            {
                _procBasketBreak = ProcBasketDelta;
                BasketAfterBreakStr = "";
            }
        }

        private void SetUsdAnnualStr(CurrencyRatesModel annual)
        {
            if (annual == null) return;

            var lastYear = Date.Year == 2000
                ? annual.TodayRates.NbRates.Usd.Value / 1000
                : Date > new DateTime(2016, 06, 30) && Date < new DateTime(2017, 1, 1)
                    ? annual.TodayRates.NbRates.Usd.Value / 10000
                    : annual.TodayRates.NbRates.Usd.Value;

            var delta = TodayRates.NbRates.Usd.Value - lastYear;
            var proc = delta / lastYear * 100;
            UsdAnnualStr = $"{delta.ToString("#,0.####", new CultureInfo("ru-RU"))} ({proc:+0.##;-0.##;0}%)";
            UsdAnnualBrush = proc < 0 ? Brushes.LimeGreen : Brushes.Red;
        }

        private void SetBasketAnnualStr(CurrencyRatesModel annual)
        {
            if (annual == null || annual.Basket.Equals(0)) return;

            var lastYear = Date.Year == 2000
                ? annual.Basket / 1000
                : Date > new DateTime(2016, 06, 30) && Date < new DateTime(2017, 1, 1)
                    ? annual.Basket / 10000
                    : annual.Basket;

            var delta = Basket - lastYear;
            _procBasketAnnual = delta / lastYear * 100;
            BasketAnnualStr = $"{_procBasketAnnual:+0.##;-0.##;0}%";
            BasketAnnualBrush = _procBasketAnnual < 0 ? Brushes.LimeGreen : Brushes.Red;
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
        private string _euroUsdStr;
        private string _rurUsdStr;

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