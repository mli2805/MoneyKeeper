using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using KeeperDomain;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using DateTimeAxis = OxyPlot.Axes.DateTimeAxis;
using LinearAxis = OxyPlot.Axes.LinearAxis;

namespace Keeper2018
{
    public class FuelViewModel : Screen
    {
        private readonly KeeperDataModel _dataModel;
        public List<FuellingModel> Rows { get; set; }
        public string Total => $"Итого {Rows.Sum(f => f.Volume)} литров";

        public PlotModel FuelPricePlotModel { get; set; }

        private Visibility _tableVisibility = Visibility.Collapsed;
        public Visibility TableVisibility
        {
            get => _tableVisibility;
            set
            {
                if (value == _tableVisibility) return;
                _tableVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        private Visibility _chartVisibility = Visibility.Visible;
        public Visibility ChartVisibility
        {
            get => _chartVisibility;
            set
            {
                if (value == _chartVisibility) return;
                _chartVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        public FuelViewModel(KeeperDataModel dataModel)
        {
            _dataModel = dataModel;
        }

        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Автомобильное топливо;  T - Toggle mode;   C - Toggle currency";
        }

        public void Initialize()
        {
            Rows = _dataModel.FuellingVms.OrderByDescending(l=>l.Timestamp).ToList();
            InitializeChartModel();
        }

        private LinearAxis _linearAxisRb;
        private LinearAxis _linearAxisUsd;
        private LineSeries _dieselSeries;
        private LineSeries _dieselSeriesUsd;
        private ScatterSeries _arcticSeries;
        private void InitializeChartModel()
        {
            FuelPricePlotModel = new PlotModel();

            FuelPricePlotModel.Axes.Add(new DateTimeAxis()
            {
                Position = AxisPosition.Bottom,
                IntervalLength = 75,
                MinorIntervalType = DateTimeIntervalType.Days,
                IntervalType = DateTimeIntervalType.Days,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dash,
            });
            _linearAxisRb = new LinearAxis
            {
                Key = "RB",
                Position = AxisPosition.Right,
                TitleColor = OxyColors.BlueViolet,
                TextColor = OxyColors.BlueViolet,
                AxislineColor = OxyColors.BlueViolet,
                MajorGridlineStyle = LineStyle.Automatic,
                MajorGridlineColor = OxyColors.BlueViolet,
                // MajorStep = 0.3,
                IsAxisVisible = false,
            };
            FuelPricePlotModel.Axes.Add(_linearAxisRb);
            _linearAxisUsd = new LinearAxis()
            {
                Key = "USD",
                Position = AxisPosition.Left,
                TitleColor = OxyColors.Green,
                TextColor = OxyColors.Green,
                AxislineColor = OxyColors.Green,
                MajorGridlineStyle = LineStyle.Automatic,
                MajorGridlineColor = OxyColors.Green,
                // MajorStep = 0.18,
            };
            FuelPricePlotModel.Axes.Add(_linearAxisUsd);

            _dieselSeries = new LineSeries { Title = "Дт Евро5", Color = OxyColors.BlueViolet, IsVisible = false };
            _dieselSeriesUsd = new LineSeries { Title = "Дт Евро5 (usd)", Color = OxyColors.Green };
            _dieselSeries.YAxisKey = "RB";
            _dieselSeriesUsd.YAxisKey = "USD";

            foreach (var fuelling in Rows.Where(f => f.FuelType == FuelType.ДтЕвро5))
            {
                _dieselSeries.Points.Add(
                    new DataPoint(
                        DateTimeAxis.ToDouble(fuelling.Timestamp.Date),
                        fuelling.Timestamp.Date < new DateTime(2016, 7, 1) 
                            ? (double)fuelling.OneLitrePrice / 10000 
                            : (double)fuelling.OneLitrePrice));
                _dieselSeriesUsd.Points.Add(new DataPoint(DateTimeAxis.ToDouble(fuelling.Timestamp.Date), (double)fuelling.OneLitreInUsd));
            }

            FuelPricePlotModel.Series.Add(_dieselSeries);
            FuelPricePlotModel.Series.Add(_dieselSeriesUsd);
          

            _arcticSeries = new ScatterSeries() {Title = "Дт Арктика (usd)", MarkerType  = MarkerType.Circle, MarkerFill = OxyColors.Red };
            _arcticSeries.YAxisKey = "USD";
            foreach (var fuelling in Rows.Where(f => f.FuelType == FuelType.ДтЕвро5Арктика))
            {
                _arcticSeries.Points.Add(new ScatterPoint(DateTimeAxis.ToDouble(fuelling.Timestamp.Date), (double)fuelling.OneLitreInUsd));
            }
            FuelPricePlotModel.Series.Add(_arcticSeries);
        }
    
        public void ToggleMode()
        {
            if (_tableVisibility == Visibility.Visible)
            {
                TableVisibility = Visibility.Collapsed;
                ChartVisibility = Visibility.Visible;
            }
            else
            {
                TableVisibility = Visibility.Visible;
                ChartVisibility = Visibility.Collapsed;
            }
        }

        private int _currencyInt;
        public void ToggleCurrency()
        {
            switch (_currencyInt)
            {
                case 0: 
                    _currencyInt = 1;
                    _linearAxisRb.IsAxisVisible = true;
                    _linearAxisUsd.IsAxisVisible = false;
                    _dieselSeries.IsVisible = true;
                    _dieselSeriesUsd.IsVisible = false;
                    _arcticSeries.IsVisible = false;
                    break;
                 case 1: 
                    _currencyInt = 2;
                    _linearAxisRb.IsAxisVisible = true;
                    _linearAxisUsd.IsAxisVisible = true;
                    _dieselSeries.IsVisible = true;
                    _dieselSeriesUsd.IsVisible = true;
                    _arcticSeries.IsVisible = true;
                    break;
                 case 2: 
                    _currencyInt = 0;
                    _linearAxisRb.IsAxisVisible = false;
                    _linearAxisUsd.IsAxisVisible = true;
                    _dieselSeries.IsVisible = false;
                    _dieselSeriesUsd.IsVisible = true;
                    _arcticSeries.IsVisible = true;
                    break;
            }
            FuelPricePlotModel.InvalidatePlot(true);

        }
    }
}
