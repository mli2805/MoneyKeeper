using System;
using System.Collections.Generic;
using Caliburn.Micro;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Keeper2018
{
    public class LongTermChartViewModel : Screen
    {
        private string _caption;
        private List<CurrencyRatesModel> _rates;
        private int _visibility = 3;

        public PlotModel LongTermModel { get; set; }
        public LineSeries UsdNbSeries { get; set; }
        public LineSeries UsdMySeries { get; set; }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = _caption;
        }

        public void Initalize(string caption, List<CurrencyRatesModel> rates)
        {
            _caption = caption;
            _rates = rates;

            PrepareSeries();
            LongTermModel = new PlotModel() { LegendPosition = LegendPosition.LeftTop };
            LongTermModel.Series.Add(UsdNbSeries);
            LongTermModel.Series.Add(UsdMySeries);

            LongTermModel.Axes.Add(new DateTimeAxis() { Position = AxisPosition.Bottom, MajorGridlineStyle = LineStyle.Solid, });
            LongTermModel.Axes.Add(new LinearAxis() { Position = AxisPosition.Left, MajorGridlineStyle = LineStyle.DashDashDotDot });
        }

        private void PrepareSeries()
        {
            var dt20160701 = new DateTime(2016, 07, 01); // 10000000;
            var dt20000101 = new DateTime(2000, 01, 01); // 1000;

            UsdNbSeries = new LineSeries() { Title = "USD (по НБРБ)", Color = OxyColors.Green};
            UsdMySeries = new LineSeries() { Title = "USD (мой)", Color = OxyColors.LimeGreen};
            foreach (var currencyRatesModel in _rates)
            {
                var usdValue = currencyRatesModel.TodayRates.NbRates.Usd.Value;
                var usdValue2 = currencyRatesModel.TodayRates.MyUsdRate.Value;
                if (currencyRatesModel.Date >= dt20160701)
                {
                    usdValue = usdValue * 10000000;
                    usdValue2 = usdValue2 * 10000000;
                }
                else if (currencyRatesModel.Date >= dt20000101)
                {
                    usdValue = usdValue * 1000;
                    usdValue2 = usdValue2 * 1000;
                }
                UsdNbSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(currencyRatesModel.Date), usdValue));
                UsdMySeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(currencyRatesModel.Date), usdValue2));
            }
        }

        public void LogarithmChart()
        {
            var currentIsLogarithmic = LongTermModel.Axes[1].GetType() == typeof(LogarithmicAxis);
            if (currentIsLogarithmic)
                LongTermModel.Axes[1] = new LinearAxis { Position = AxisPosition.Left };
            else
                LongTermModel.Axes[1] = new LogarithmicAxis { Position = AxisPosition.Left };
            LongTermModel.InvalidatePlot(true);
        }

        public void ToggleVisibility()
        {
            switch (_visibility)
            {
                case 1:
                    UsdMySeries.IsVisible = true;
                    UsdNbSeries.IsVisible = false;
                    _visibility = 2;
                    break;
                case 2:
                    UsdNbSeries.IsVisible = true;
                    _visibility = 3;
                    break;
                case 3:
                    UsdMySeries.IsVisible = false;
                    _visibility = 1;
                    break;
            }
            LongTermModel.InvalidatePlot(true);
        }
    }
}
