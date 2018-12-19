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
        private int _visibility = 4;

        public PlotModel LongTermModel { get; set; }
        public LineSeries UsdNbSeries { get; set; }
        public LineSeries UsdMySeries { get; set; }
        public LineSeries RubBelSeries { get; set; }
        public LineSeries RubUsdSeries { get; set; }

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
            LongTermModel.Series.Add(RubBelSeries);
            LongTermModel.Series.Add(RubUsdSeries);

            LongTermModel.Axes.Add(new DateTimeAxis()
            {
                Position = AxisPosition.Bottom,
                IntervalLength = 75,
                MinorIntervalType = DateTimeIntervalType.Days,
                IntervalType = DateTimeIntervalType.Days,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dash,
            });
            LongTermModel.Axes.Add(new LinearAxis() { Position = AxisPosition.Left, MajorGridlineStyle = LineStyle.DashDashDotDot });
        }

        private void PrepareSeries()
        {
            var dt20160701 = new DateTime(2016, 07, 01); // 10000000;
            var dt20000101 = new DateTime(2000, 01, 01); // 1000;

            UsdNbSeries = new LineSeries() { Title = "USD (по НБРБ)", Color = OxyColors.Green, IsVisible = false };
            UsdMySeries = new LineSeries() { Title = "USD (мой)", Color = OxyColors.LimeGreen, IsVisible = false };
            RubBelSeries = new LineSeries() { Title = "Rub / Byn", Color = OxyColors.Orange, IsVisible = true };
            RubUsdSeries = new LineSeries() { Title = "Rub / Usd", Color = OxyColors.Red, IsVisible = false };

            foreach (var currencyRatesModel in _rates)
            {
                var usdValue = currencyRatesModel.TodayRates.NbRates.Usd.Value;
                var usdValue2 = currencyRatesModel.TodayRates.MyUsdRate.Value;
                var rubBel = currencyRatesModel.TodayRates.NbRates.Rur.Value;
                var rubUsd = currencyRatesModel.TodayRates.CbrRate.Usd.Value;
                if (currencyRatesModel.Date >= dt20160701)
                {
                    usdValue = usdValue * 10000000;
                    usdValue2 = usdValue2 * 10000000;
                    rubBel = rubBel * 10000000;
                }
                else if (currencyRatesModel.Date >= dt20000101)
                {
                    usdValue = usdValue * 1000;
                    usdValue2 = usdValue2 * 1000;
                    rubBel = rubBel * 1000;
                }

                var day = DateTimeAxis.ToDouble(currencyRatesModel.Date);
                UsdNbSeries.Points.Add(new DataPoint(day, usdValue));
                UsdMySeries.Points.Add(new DataPoint(day, usdValue2));
                RubBelSeries.Points.Add(new DataPoint(day, rubBel));
                RubUsdSeries.Points.Add(new DataPoint(day, rubUsd));
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
                    _visibility = 2;
                    UsdNbSeries.IsVisible = false;
                    UsdMySeries.IsVisible = true;

                    RubBelSeries.IsVisible = false;
                    RubUsdSeries.IsVisible = false;
                    break;
                case 2:
                    _visibility = 3;
                    UsdNbSeries.IsVisible = true;
                    UsdMySeries.IsVisible = true;

                    RubBelSeries.IsVisible = false;
                    RubUsdSeries.IsVisible = false;
                    break;
                case 3:
                    _visibility = 4;
                    UsdNbSeries.IsVisible = false;
                    UsdMySeries.IsVisible = false;

                    RubBelSeries.IsVisible = true;
                    RubUsdSeries.IsVisible = false;
                    break;
                case 4:
                    _visibility = 5;
                    UsdNbSeries.IsVisible = false;
                    UsdMySeries.IsVisible = false;

                    RubBelSeries.IsVisible = false;
                    RubUsdSeries.IsVisible = true;
                    break;
                case 5:
                    _visibility = 6;
                    UsdNbSeries.IsVisible = false;
                    UsdMySeries.IsVisible = false;

                    RubBelSeries.IsVisible = true;
                    RubUsdSeries.IsVisible = true;
                    break;
                case 6:
                    _visibility = 1;
                    UsdNbSeries.IsVisible = true;
                    UsdMySeries.IsVisible = false;

                    RubBelSeries.IsVisible = false;
                    RubUsdSeries.IsVisible = false;
                    break;
            }
            LongTermModel.InvalidatePlot(true);
        }
    }
}
