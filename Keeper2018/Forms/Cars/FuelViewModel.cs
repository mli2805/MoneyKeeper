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
        public List<FuellingVm> Rows { get; set; }
        public string Total => $"Итого {Rows.Sum(f => f.Volume)} литров";

        public PlotModel ChartModel { get; set; }

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
            DisplayName = "Автомобильное топливо;  T - Toggle mode";
        }

        public void Initialize()
        {
            Rows = _dataModel.FuellingVms;
            InitializeChartModel();
        }

        private void InitializeChartModel()
        {
            ChartModel = new PlotModel();

            ChartModel.Axes.Add(new DateTimeAxis()
            {
                Position = AxisPosition.Bottom,
                IntervalLength = 75,
                MinorIntervalType = DateTimeIntervalType.Days,
                IntervalType = DateTimeIntervalType.Days,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dash,
            });
            ChartModel.Axes.Add(new LinearAxis()
            {
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Automatic,
                MinorGridlineStyle = LineStyle.Automatic,
            });

            var dieselSeries = new LineSeries()
            { Title = "Дт Евро5", Color = OxyColors.BlueViolet };
            foreach (var fuelling in Rows.Where(f => f.FuelType == FuelType.ДтЕвро5))
            {
                dieselSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(fuelling.Timestamp.Date), (double)fuelling.OneLitreInUsd));
            }
            ChartModel.Series.Add(dieselSeries);

            var arkticSeries = new ScatterSeries() {Title = "Дт Арктика", MarkerType  = MarkerType.Circle, MarkerFill = OxyColors.Red };
            foreach (var fuelling in Rows.Where(f => f.FuelType == FuelType.ДтЕвро5Арктика))
            {
                arkticSeries.Points.Add(new ScatterPoint(DateTimeAxis.ToDouble(fuelling.Timestamp.Date), (double)fuelling.OneLitreInUsd));
            }
            ChartModel.Series.Add(arkticSeries);
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

    }
}
