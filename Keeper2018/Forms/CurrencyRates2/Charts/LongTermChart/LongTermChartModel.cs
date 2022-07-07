﻿using System;
using System.Collections.Generic;
using KeeperDomain;
using KeeperDomain.Basket;
using KeeperDomain.Exchange;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Keeper2018
{
    public class LongTermChartModel
    {
        private DateTime dt20160701 = new DateTime(2016, 07, 01); // 10000000;
        private DateTime dt20000101 = new DateTime(2000, 01, 01); // 1000;
        private DateTime dt19990111 = new DateTime(1999, 01, 11); // EURO
        private DateTime dt19980101 = new DateTime(1998, 01, 01); // 1000 RUB

        public PlotModel LongTermModel { get; set; }
        public LineSeries UsdNbSeries { get; set; }

        private bool _isUsdNb;
        public bool IsUsdNb
        {
            get => _isUsdNb;
            set
            {
                _isUsdNb = value;
                UsdNbSeries.IsVisible = value;
                LongTermModel.InvalidatePlot(true);
            }
        }

        public LineSeries UsdMySeries { get; set; }

        private bool _isUsdMy = true;
        public bool IsUsdMy
        {
            get => _isUsdMy;
            set
            {
                _isUsdMy = value;
                UsdMySeries.IsVisible = value;
                LongTermModel.InvalidatePlot(true);
            }
        }

        public LineSeries RubNbSeries { get; set; }

        private bool _isRubNb;
        public bool IsRubNb
        {
            get => _isRubNb;
            set
            {
                _isRubNb = value;
                RubNbSeries.IsVisible = value;
                LongTermModel.InvalidatePlot(true);
            }
        }

        public LineSeries RubUsdSeries { get; set; }

        private bool _isRubUsd;
        public bool IsRubUsd
        {
            get => _isRubUsd;
            set
            {
                _isRubUsd = value;
                RubUsdSeries.IsVisible = value;
                LongTermModel.InvalidatePlot(true);
            }
        }

        public LineSeries EurNbSeries { get; set; }

        private bool _isEurNb;
        public bool IsEurNb
        {
            get => _isEurNb;
            set
            {
                _isEurNb = value;
                EurNbSeries.IsVisible = value;
                LongTermModel.InvalidatePlot(true);
            }
        }

        public LineSeries EurUsdSeries { get; set; }

        private bool _isEurUsd;

        public bool IsEurUsd
        {
            get => _isEurUsd;
            set
            {
                _isEurUsd = value;
                EurUsdSeries.IsVisible = value;
                LongTermModel.InvalidatePlot(true);
            }
        }

        public LineSeries BasketSeries { get; set; }

        private bool _isBasket;
        public bool IsBasket
        {
            get => _isBasket;
            set
            {
                _isBasket = value;
                BasketSeries.IsVisible = value;
                LongTermModel.InvalidatePlot(true);
            }
        }

        public void Build(List<CurrencyRatesModel> rates, KeeperDataModel keeperDataModel)
        {
            PrepareSeries(rates, keeperDataModel);
            LongTermModel = new PlotModel() { LegendPosition = LegendPosition.LeftTop };
            LongTermModel.Series.Add(UsdNbSeries);
            LongTermModel.Series.Add(UsdMySeries);
            LongTermModel.Series.Add(RubNbSeries);
            LongTermModel.Series.Add(RubUsdSeries);
            LongTermModel.Series.Add(EurNbSeries);
            LongTermModel.Series.Add(EurUsdSeries);
            LongTermModel.Series.Add(BasketSeries);

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

        private void PrepareSeries(List<CurrencyRatesModel> rates, KeeperDataModel keeperDataModel)
        {
            UsdNbSeries = new LineSeries() { Title = "USD (по НБ РБ)", Color = OxyColors.Green, IsVisible = false };
            UsdMySeries = new LineSeries() { Title = "USD (мой)", Color = OxyColors.LimeGreen, IsVisible = true };
            RubNbSeries = new LineSeries() { Title = "Rub / Byn", Color = OxyColors.Orange, IsVisible = false };
            RubUsdSeries = new LineSeries() { Title = "Rub / Usd", Color = OxyColors.Red, IsVisible = false };
            EurNbSeries = new LineSeries() { Title = "EUR (по НБ РБ)", Color = OxyColors.Blue, IsVisible = false };
            EurUsdSeries = new LineSeries() { Title = "Eur / Usd", Color = OxyColors.DeepSkyBlue, IsVisible = false };
            BasketSeries = new LineSeries() { Title = "Basket", Color = OxyColors.Black, IsVisible = false };

            foreach (var currencyRatesModel in rates)
            {
                var exchangeRates = keeperDataModel.ExchangeRates.ContainsKey(currencyRatesModel.Date) 
                    ? keeperDataModel.ExchangeRates[currencyRatesModel.Date]
                    : null;
                var normalizedRates = GetNormalizedRates(currencyRatesModel.TodayRates, exchangeRates);

                var day = DateTimeAxis.ToDouble(currencyRatesModel.Date);
                UsdNbSeries.Points.Add(new DataPoint(day, normalizedRates.UsdNb));
                if (!normalizedRates.UsdMy.Equals(0))
                    UsdMySeries.Points.Add(new DataPoint(day, normalizedRates.UsdMy));
                RubNbSeries.Points.Add(new DataPoint(day, normalizedRates.RubNb));
                RubUsdSeries.Points.Add(new DataPoint(day, normalizedRates.RubUsd));
                if (currencyRatesModel.Date >= dt19990111)
                {
                    EurNbSeries.Points.Add(new DataPoint(day, normalizedRates.EurNb));
                    EurUsdSeries.Points.Add(new DataPoint(day, normalizedRates.EurUsd));
                    BasketSeries.Points.Add(new DataPoint(day, normalizedRates.Basket));
                }
            }
        }

        private NormalizedRates GetNormalizedRates(CurrencyRates currencyRates, ExchangeRates exchangeRates)
        {
            var result = new NormalizedRates();
            if (currencyRates.Date >= dt20160701)
            {
                result.UsdNb = currencyRates.NbRates.Usd.Value * 10000000;
                result.EurNb = currencyRates.NbRates.Euro.Value * 10000000;
                result.RubNb = currencyRates.NbRates.Rur.Value / currencyRates.NbRates.Rur.Unit * 10000000;
                result.UsdMy = exchangeRates != null ? exchangeRates.BynToUsd * 10000000 : 0;
                result.RubUsd = currencyRates.CbrRate.Usd.Value * 1000;
                result.Basket = BelBaskets.Calculate(currencyRates);
            }
            else if (currencyRates.Date >= dt20000101)
            {
                result.UsdNb = currencyRates.NbRates.Usd.Value * 1000;
                result.EurNb = currencyRates.NbRates.Euro.Value * 1000;
                result.RubNb = currencyRates.NbRates.Rur.Value * 1000;
                result.UsdMy = exchangeRates != null ? exchangeRates.BynToUsd * 1000 : 0;
                result.RubUsd = currencyRates.CbrRate.Usd.Value * 1000;
                result.Basket = BelBaskets.Calculate(currencyRates) / 10000;
            }
            else if (currencyRates.Date >= dt19990111) // euro and basket start
            {
                result.UsdNb = currencyRates.NbRates.Usd.Value;
                result.EurNb = currencyRates.NbRates.Euro.Value;
                result.RubNb = currencyRates.NbRates.Rur.Value;
                result.UsdMy = exchangeRates?.BynToUsd ?? 0;
                result.RubUsd = currencyRates.CbrRate.Usd.Value * 1000;
                result.Basket = BelBaskets.Calculate(currencyRates) / 10000000;
            }
            else if (currencyRates.Date >= dt19980101) // russian denomination
            {
                result.UsdNb = currencyRates.NbRates.Usd.Value;
                result.RubNb = currencyRates.NbRates.Rur.Value;
                result.UsdMy = exchangeRates?.BynToUsd ?? 0;
                result.RubUsd = currencyRates.CbrRate.Usd.Value * 1000;
            }
            else
            {
                result.UsdNb = currencyRates.NbRates.Usd.Value;
                result.RubNb = currencyRates.NbRates.Rur.Value;
                result.UsdMy = exchangeRates?.BynToUsd ?? 0;
                result.RubUsd = currencyRates.CbrRate.Usd.Value;
            }
            return result;
        }
    }
}